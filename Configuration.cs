using System.Collections.Generic;
using System.Linq;
using ResoniteModLoader;
using SkyFrost.Base;

namespace AutoAcceptInvite;

internal class Configuration
{
    private readonly ModConfigurationKey<bool> _autoAcceptInvites = new (
        "Auto-Accept Invites", "Automatically accept invites to other worlds.",() => true);

    private readonly ModConfigurationKey<bool> _autoAcceptInviteRequests = new(
        "Auto-Accept Invite Requests", "Automatically accept invite requests to your current instance.", () => true);
    
    private readonly ModConfigurationKey<bool> _allowForwardingToInstanceOwner = new(
        "Allow Forwarding to Instance Owner", "Allow forwarding invites to the instance owner.", () => true);

    private readonly ModConfigurationKey<bool> _autoAcceptForwardedInviteRequests = new(
        "Auto-Accept Forwarded Invite Requests", "Automatically accept forwarded invite requests.", () => true);
    
    private readonly ModConfigurationKey<OnlineStatus> _minimumOnlineStatusLevel = new(
        "Minimum Online Status Level", "Your minimum online status level required to auto-accept invites.", () => OnlineStatus.Online);
    
    private readonly ModConfigurationKey<int> _minIntervalInSeconds = new(
        "Minimum Interval (Seconds)", "The minimum interval in seconds between auto-accepted invites. Other invites are ignored.", () => 60);

    private readonly ModConfigurationKey<List<string>> _userIds = new(
        "User IDs", "The user IDs to enable auto-accepting invites for.", () => [], true
    );

    private readonly HashSet<string> _userIdSet = [];
    
    private ModConfiguration? _configuration;

    public void DefineConfiguration(ModConfigurationDefinitionBuilder builder)
    {
        ResoniteMod.Msg("Defining configuration...");
        builder.Version(AutoAcceptInviteMod.AssemblyVersion);
        builder.Key(_autoAcceptInvites);
        builder.Key(_autoAcceptInviteRequests);
        builder.Key(_allowForwardingToInstanceOwner);
        builder.Key(_autoAcceptForwardedInviteRequests);
        builder.Key(_minimumOnlineStatusLevel);
        builder.Key(_minIntervalInSeconds);
        builder.Key(_userIds);
    }
    
    public void Init(ModConfiguration? configuration)
    {
        _configuration = configuration;
        _userIdSet.Clear();
        _userIdSet.UnionWith(_configuration?.GetValue(_userIds) ?? []);
        _configuration?.Save(true);
    }
    
    public bool IsAutoAcceptInvitesEnabled() => _configuration?.GetValue(_autoAcceptInvites) ?? true;
    public bool IsAutoAcceptInviteRequestsEnabled() => _configuration?.GetValue(_autoAcceptInviteRequests) ?? true;
    public bool IsAllowForwardingToInstanceOwnerEnabled() => _configuration?.GetValue(_allowForwardingToInstanceOwner) ?? true;
    
    public bool IsAutoAcceptForwardedInviteRequestsEnabled() => _configuration?.GetValue(_autoAcceptForwardedInviteRequests) ?? true;
    
    public OnlineStatus MinimumOnlineStatusLevel() => _configuration?.GetValue(_minimumOnlineStatusLevel) ?? OnlineStatus.Online;
    
    public int MinIntervalInSeconds() => _configuration?.GetValue(_minIntervalInSeconds) ?? 60;
    
    public bool IsUserIdEnabled(string userId) => _userIdSet.Contains(userId);
    
    public void EnableUserId(string userId)
    {
        if (IsUserIdEnabled(userId)) return;
        _userIdSet.Add(userId);
        _configuration?.Set(_userIds, _userIdSet.ToList());
        _configuration?.Save();
    }
    
    public void DisableUserId(string userId)
    {
        if (!IsUserIdEnabled(userId)) return;
        _userIdSet.Remove(userId);
        _configuration?.Set(_userIds, _userIdSet.ToList());
        _configuration?.Save();
    }
}