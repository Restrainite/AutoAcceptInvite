using System;
using System.Threading;
using System.Threading.Tasks;
using Elements.Core;
using FrooxEngine;
using HarmonyLib;
using ResoniteModLoader;
using SkyFrost.Base;
using MessageType = SkyFrost.Base.MessageType;

namespace AutoAcceptInvite.Patches;

[HarmonyPatch]
internal static class MessageHandler
{
    private static long _lastInvite = DateTime.UnixEpoch.ToBinary();
    
    [HarmonyPatch(typeof(EngineSkyFrostInterface), "OnLogin")]
    [HarmonyPrefix]
    private static void OnLogin(EngineSkyFrostInterface __instance)
    {
        __instance.Messages.OnMessageReceived += MessagesOnOnMessageReceived;
    }

    private static void MessagesOnOnMessageReceived(Message? message)
    {
        if (message == null || message.IsRead || message.IsSelfMessage || !message.IsValid || message.IsSent) return;
        if (Engine.Current.Cloud.Status.OnlineStatus < AutoAcceptInviteMod.Configuration.MinimumOnlineStatusLevel()) return;

        switch (message.MessageType)
        {
            case MessageType.SessionInvite when
                AutoAcceptInviteMod.Configuration.IsAutoAcceptInvitesEnabled():
                var sessionInfo = message.ExtractContent<SessionInfo>();
                HandleInvite(message, sessionInfo);
                break;
            case MessageType.InviteRequest when
                AutoAcceptInviteMod.Configuration.IsAutoAcceptInviteRequestsEnabled():
                var inviteRequest = message.ExtractContent<InviteRequest>();
                HandleInviteRequest(message, inviteRequest);
                break;
        }
    }
    
    private static void HandleInvite(Message message, SessionInfo sessionInfo)
    {
        ResoniteMod.Msg("Got invite from " + message.SenderId);
        if (!AutoAcceptInviteMod.Configuration.IsUserIdEnabled(message.SenderId)) return;
        var updatedSessionInfo = Engine.Current.Cloud.Sessions.TryGetInfo(sessionInfo.SessionId);
        if (updatedSessionInfo?.IsCompatible() != true) return;
        if (updatedSessionInfo.HasEnded) return;
        if (CheckInviteInterval()) return;
        
        ResoniteMod.Msg("Accepting invite from " + message.SenderId);
        Userspace.OpenWorld(new WorldStartSettings()
        {
            URIs = sessionInfo.GetSessionURLs(),
            HostUserId = sessionInfo.HostUserId,
            GetExisting = true,
            FetchedWorldName = (LocaleString) sessionInfo.Name
        });
        MarkMessageAsRead(message);
    }

    private static bool CheckInviteInterval()
    {
        var lastInvite = DateTime.FromBinary(_lastInvite);
        var now = DateTime.UtcNow;
        var intervalEnd = lastInvite.AddSeconds(AutoAcceptInviteMod.Configuration.MinIntervalInSeconds());
        if (intervalEnd > now)
        {
            ResoniteMod.Msg("Too soon to auto accept invite, waiting at least " + intervalEnd.Subtract(now).TotalSeconds + " seconds.");
            return true;
        }
        Interlocked.Exchange(ref _lastInvite, DateTime.UtcNow.ToBinary());
        return false;
    }

    private static void HandleInviteRequest(Message message, InviteRequest inviteRequest)
    {
        ResoniteMod.Msg("Got invite request from " + message.SenderId);
        if (!AutoAcceptInviteMod.Configuration.IsUserIdEnabled(message.SenderId)) return;
        if (inviteRequest.IsGranted) return;
        var focusedWorld = Engine.Current.WorldManager.FocusedWorld;
        if (inviteRequest.RequestingFromUserId == Engine.Current.Cloud.CurrentUserID)
        {
            var canInviteDirectly =
                focusedWorld.IsAuthority || focusedWorld.AccessLevel >= SessionAccessLevel.ContactsPlus;
            if (canInviteDirectly)
            {
                if (CheckInviteInterval()) return;
                SendDirectInvite(message, inviteRequest);
            }
            else
            {
                var userId = focusedWorld.HostUser.UserID;
                if (string.IsNullOrEmpty(userId))
                {
                    return;
                }
                if (AutoAcceptInviteMod.Configuration.IsAllowForwardingToInstanceOwnerEnabled() &&
                    Engine.Current.Cloud.Contacts.IsContact(userId, true))
                {
                    if (CheckInviteInterval()) return;
                    ForwardInviteRequestToHost(message, inviteRequest);
                }
            }
        }
        else if (AutoAcceptInviteMod.Configuration.IsAutoAcceptForwardedInviteRequestsEnabled())
        {
            if (CheckInviteInterval()) return;
            GrantInvite(message, inviteRequest);
        }
    }

    private static void SendDirectInvite(Message message, InviteRequest inviteRequest)
    {
        ResoniteMod.Msg("Sending direct invite to " + inviteRequest.UserIdToInvite);
        var contactId = inviteRequest.UserIdToInvite;
        var targetWorld = Engine.Current.WorldManager.FocusedWorld;
        if (targetWorld.IsAuthority && targetWorld.IsUserAllowed(contactId))
        {
            return;
        }
        var messages = Engine.Current.Cloud.Messages.GetUserMessages(contactId);
        targetWorld.Coroutines.StartTask((Func<Task>) (async () =>
        {
            var inviteMessage = await messages.CreateInviteMessage(targetWorld);
            await messages.SendMessage(inviteMessage);
        }));
        MarkMessageAsRead(message);
    }
    
    private static void ForwardInviteRequestToHost(Message message, InviteRequest inviteRequest)
    {
        var focusedWorld = Engine.Current.WorldManager.FocusedWorld;
        var hostId = focusedWorld.HostUser.UserID;
        var sessionId = focusedWorld.SessionId;
        var sessionName = focusedWorld.Name;
        ResoniteMod.Msg("Forwarding invite request to host " + hostId);
        focusedWorld.Coroutines.StartTask((Func<Task>) (async () =>
        {
            await Engine.Current.Cloud.InviteRequests.ForwardInviteRequest(inviteRequest, sessionId, sessionName, hostId);
        }));
        MarkMessageAsRead(message);
    }
    
    private static void GrantInvite(Message message, InviteRequest inviteRequest)
    {
        ResoniteMod.Msg("Granting invite request from " + message.SenderId + " to " + inviteRequest.UserIdToInvite);
        var world = Engine.Current.WorldManager.GetWorld(w => w.IsAuthority && w.SessionId == inviteRequest.ForSessionId);
        Task.Run((Func<Task>) (async () =>
        {
            if (world != null)
                await Engine.Current.Cloud.InviteRequests.SendInvite(inviteRequest, world);
            else
                await Engine.Current.Cloud.InviteRequests.SendResponse(inviteRequest, InviteRequestResponse.SendInvite, message.SenderId);
        }));
        MarkMessageAsRead(message);
    }

    private static void MarkMessageAsRead(Message message)
    {
        if (message.IsSent || message.IsRead) return;
        var sendReadNotification = Engine.Current.Cloud.Status.OnlineStatus != OnlineStatus.Invisible &&
                                   !Engine.Current.Cloud.DoNotSendReadStatus;
        Task.Run((Func<Task>)(async () =>
        {
            await Engine.Current.Cloud.HubClient.MarkMessagesRead(new MarkReadBatch()
            {
                SenderId = sendReadNotification ? message.SenderId : null,
                Ids = [message.Id],
                ReadTime = DateTime.UtcNow
            }).ConfigureAwait(false);
        }));
    }

    [HarmonyPatch(typeof(EngineSkyFrostInterface), "OnLogout")]
    [HarmonyPrefix]
    private static void OnLogout(EngineSkyFrostInterface __instance)
    {
        __instance.Messages.OnMessageReceived -= MessagesOnOnMessageReceived;
    }
}