using System.Reflection;
using ResoniteModLoader;

namespace AutoAcceptInvite;

internal static class RestrainiteIntegration
{
    private static readonly Lazy<Assembly?> Restrainite = new(FindRestrainite);
    private static readonly Lazy<Func<bool>> PreventSwitchingWorld = new(() => FindRestriction("PreventSwitchingWorld"));
    private static readonly Lazy<Func<bool>> PreventInviteContact = new(() => FindRestriction("PreventInviteContact"));

    internal static bool IsSwitchingWorldPrevented => IsRestricted(PreventSwitchingWorld);

    internal static bool IsInvitingContactsPrevented => IsRestricted(PreventInviteContact);

    private static bool IsRestricted(Lazy<Func<bool>> restriction)
    {
        if (!AutoAcceptInviteMod.Configuration.IsRespectRestrainiteEnabled()) return false;
        try
        {
            return restriction.Value();
        }
        catch (Exception ex)
        {
            ResoniteMod.Warn($"Failed to check the Restrainite restriction: {ex}");
            return true;
        }
    }

    private static Assembly? FindRestrainite()
    {
        return AppDomain.CurrentDomain.GetAssemblies()
            .FirstOrDefault(assembly => "Restrainite".Equals(assembly.GetName().Name, StringComparison.Ordinal));
    }

    private static Func<bool> FindRestriction(string name)
    {
        if (Restrainite.Value == null) return () => false;

        var restriction = Restrainite.Value.GetType("Restrainite.Restrictions")
            ?.GetField(name, BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic)
            ?.GetValue(null);
        var getter = restriction?.GetType().GetProperty("IsRestricted")?.GetMethod;
        if (getter != null) return getter.CreateDelegate<Func<bool>>(restriction);

        ResoniteMod.Warn($"The installed version of Restrainite does not support {name}. " +
                         "Auto-accepting is limited while Respect Restrainite is enabled.");
        return () => true;
    }
}
