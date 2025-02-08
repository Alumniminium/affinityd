using System.Diagnostics;
using System.Text.Json;

namespace affinity_daemon;

public static class Kernel
{
    public const string ConfigFile = "/etc/affinityd.json";
    public static Config Config { get; private set; } = new();
    public static readonly List<string> BlacklistedApps =
    [
        "kworker", "ksoftirqd", "rcu", "migration", "cpuhp",
        "pool_workqueue_release", "writeback", "rcub", "idle_inject",
        "kthreadd", "irq", "card0-crtc", "f2fs", "kswapd0", "khugepaged",
        "khungtaskd", "krfcommd", "ksmd", "kdevtmpfs", "kcompactd", "kauditd",
        "gvfs", "dbus", "at-spi", "watchdogd", "(sd-pam)", "(udev-worker)"
    ];

    public static void Initialize()
    {
        ReadConfig();
        CPU.Initialize();
    }

    public static void ReadConfig()
    {
        if (!File.Exists(ConfigFile))
            UpdateConfig();

        Config = JsonSerializer.Deserialize<Config>(File.ReadAllText(ConfigFile))!;
    }

    public static void SetAffinities()
    {
        var processes = Process.GetProcesses();

        foreach (var app in Config.Apps)
        {
            var process = processes.FirstOrDefault(p => p.ProcessName.Split(' ').First() == app.Name);
            if (process == null)
                continue;

            nint affinityMask = 0;

            if (app.OnlyRealCores)
            {
                affinityMask = 0;
                foreach (var core in CPU.Cores.Values)
                    if (core.IsSMT)
                        affinityMask |= 1 << core.LogicalCore;
            }

            if (app.ForceGroup != null)
            {
                affinityMask = 0;
                foreach (var core in CPU.CoresByGroup[app.ForceGroup.Value])
                    if (app.OnlyRealCores && core.IsSMT)
                        continue;
                    else
                        affinityMask |= 1 << core.LogicalCore;
            }

            if (app.DisableGroup != null)
            {
                foreach (var core in CPU.CoresByGroup[app.DisableGroup.Value])
                    affinityMask &= ~(1 << core.LogicalCore);
            }

            if (app.SpecificCores.Count > 0)
            {
                affinityMask = 0;
                foreach (var core in app.SpecificCores)
                    affinityMask |= 1 << core;
            }
            try
            {
                if (process.ProcessorAffinity != affinityMask)
                {
                    process.ProcessorAffinity = affinityMask;
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.WriteLine($"\nSet affinity for {process.ProcessName[..Math.Min(32, process.ProcessName.Length)]} (PID: {process.Id}) to {affinityMask}");
                }
            }
            catch (Exception e)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"\nError setting affinity for {process.ProcessName[..Math.Min(32, process.ProcessName.Length)]} (PID: {process.Id}): {e.Message}");
            }
        }

        UpdateConfig(processes);
    }

    public static void UpdateConfig(Process[]? processes = null)
    {
        processes ??= Process.GetProcesses();
        var filteredProcesses = new HashSet<string>();

        foreach (var process in processes)
        {
            var valid = true;
            var name = process.ProcessName.Split(' ').First();

            foreach (var black in BlacklistedApps)
                if (name.Contains(black))
                    valid = false;

            if (valid)
                filteredProcesses.Add(name);
        }

        var addedAny = false;
        foreach (var process in filteredProcesses)
        {
            var app = Config.Apps.FirstOrDefault(a => a.Name == process);
            if (app != null)
                continue;

            addedAny = true;
            Console.WriteLine($"Adding {process} to config");
            Config.Apps.Add(new AppCfg { Name = process, ForceGroup = PerformanceGroup.UltraLowPowerCore, DisableGroup = PerformanceGroup.UnknownCore, SpecificCores = [], OnlyRealCores = false });
        }

        if (addedAny || Config.Dirty)
        {
            Config.Dirty = false;
            Config.Apps = [.. Config.Apps.OrderBy(a => a.Name).OrderByDescending(x=> x.FoundInForegroundTimes).ToList()];
            Console.WriteLine($"Writing config file... ({ConfigFile})");
            File.WriteAllText(ConfigFile, JsonSerializer.Serialize(Config, new JsonSerializerOptions { WriteIndented = true }));
        }
    }

    public static string RunCommand(string cmd, string args)
    {
        var processStartInfo = new ProcessStartInfo
        {
            FileName = cmd,
            Arguments = args,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true
        };

        var process = Process.Start(processStartInfo);

        if (process is null)
            return "";

        string output = process.StandardOutput.ReadToEnd();
        string error = process.StandardError.ReadToEnd();
        process.WaitForExit();

        return string.IsNullOrEmpty(error) ? output : error;
    }
}
