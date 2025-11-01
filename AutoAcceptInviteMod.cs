namespace AutoAcceptInvite;

using System;
using HarmonyLib;
using ResoniteModLoader;

public class AutoAcceptInviteMod : ResoniteMod
{
    public const string LogReportUrl =
        "Please report this to AutoAcceptInvite (https://github.com/Restrainite/AutoAcceptInvite/issues):";

    internal static readonly Configuration Configuration = new();

    public override string Name => "AutoAcceptInvite";
    public override string Author => "SnepDrone";

    public static Version AssemblyVersion => typeof(AutoAcceptInviteMod).Assembly.GetName().Version ?? new Version(0, 0, 0);
    public override string Version => $"{AssemblyVersion.Major}.{AssemblyVersion.Minor}.{AssemblyVersion.Build}";

    public override string Link => "https://github.com/Restrainite/AutoAcceptInvite/";

    public override void DefineConfiguration(ModConfigurationDefinitionBuilder builder)
    {
        Configuration.DefineConfiguration(builder);
    }

    /*
     * There are more graceful ways to handle incompatible configs, but this is the simplest.
     * Default is ERROR (prevents saving), CLOBBER overwrites the config file.
     */
    public override IncompatibleConfigurationHandlingOption HandleIncompatibleConfigurationVersions(
        Version serializedVersion, Version definedVersion)
    {
        return IncompatibleConfigurationHandlingOption.CLOBBER;
    }

    public override void OnEngineInit()
    {
        Configuration.Init(GetConfiguration());

        PatchResonite();

        InitializePatches();
    }

    private static void PatchResonite()
    {
        var harmony = new Harmony("Restrainite.AutoAcceptInvite");

        AccessTools.GetTypesFromAssembly(typeof(AutoAcceptInviteMod).Assembly)
            .Do<Type>(type =>
            {
                try
                {
                    harmony.CreateClassProcessor(type).Patch();
                }
                catch (Exception ex)
                {
                    Error($"{LogReportUrl} Failed to patch {type.FullName}: {ex}");
                }
            });
    }

    private static void InitializePatches()
    {
        
    }
}