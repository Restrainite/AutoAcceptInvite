using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using FrooxEngine;
using FrooxEngine.UIX;
using HarmonyLib;
using ResoniteModLoader;

namespace AutoAcceptInvite.Patches;


[HarmonyPatch]
internal static class ContactsDialogPatcher
{
    private delegate void InjectionDelegate(ContactsDialog contactsDialog, UIBuilder actionsUi);
    
    private static readonly FieldInfo? ActionsUiField = AccessTools.Field(typeof(ContactsDialog), "actionsUi");
    private static readonly MethodInfo? UpdateBanButtonsMethod = AccessTools.Method(typeof(ContactsDialog), "UpdateBanButtons");

    [HarmonyTranspiler]
    [HarmonyPatch(typeof(ContactsDialog), "UpdateSelectedContactUI")]
    private static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
    {
        var initialized = false;
        if (ActionsUiField == null || UpdateBanButtonsMethod == null)
        {
            ResoniteMod.Error(AutoAcceptInviteMod.LogReportUrl + " - ContactsDialogPatcher: ActionsUiField or UpdateBanButtonsMethod is null");
        }
        else
        {
            initialized = true;
        }
        foreach (var code in instructions)
        {
            yield return code;

            if (!initialized || !code.Calls(UpdateBanButtonsMethod)) continue;
            foreach (var injectedCode in CodeInjection(CreateButton))
            {
                yield return injectedCode;
            }
        }
    }

    private static IEnumerable<CodeInstruction> CodeInjection(InjectionDelegate method)
    {
        // Load contactsDialog (this)
        yield return new CodeInstruction(OpCodes.Ldarg_0);
        // Duplicate it for the field access
        yield return new CodeInstruction(OpCodes.Dup);  
        // Load actionsUi field from contactsDialog
        yield return new CodeInstruction(OpCodes.Ldfld, ActionsUiField);
        // Call method (now stack has: contactsDialog, actionsUi)
        yield return new CodeInstruction(OpCodes.Call, method.Method);
    }
    
    private static void CreateButton(ContactsDialog contactsDialog, UIBuilder actionsUi)
    {
        var contactUserId = contactsDialog.SelectedContact?.ContactUserId;
        if (contactUserId == null) return;

        var active = AutoAcceptInviteMod.Configuration.IsUserIdEnabled(contactUserId);
        var button = actionsUi.Button("Auto-Accept " + (active ? "Enabled" : "Disabled"));
        button.SetColors(active
            ? RadiantUI_Constants.TAB_ACTIVE_BACKGROUND_COLOR
            : RadiantUI_Constants.TAB_INACTIVE_BACKGROUND_COLOR);
        button.LocalReleased += (_, _) => ButtonOnLocalReleased(button, contactUserId);
    }

    private static void ButtonOnLocalReleased(Button button, string contactUserId)
    {
        if (AutoAcceptInviteMod.Configuration.IsUserIdEnabled(contactUserId))
        {
            AutoAcceptInviteMod.Configuration.DisableUserId(contactUserId);
            button.SetColors(RadiantUI_Constants.TAB_INACTIVE_BACKGROUND_COLOR);
            button.LabelText = "Auto-Accept Disabled";
        }
        else
        {
            AutoAcceptInviteMod.Configuration.EnableUserId(contactUserId);
            button.SetColors(RadiantUI_Constants.TAB_ACTIVE_BACKGROUND_COLOR);
            button.LabelText = "Auto-Accept Enabled";
        }
    }
}