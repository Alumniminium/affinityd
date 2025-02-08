using System.Text.Json;

namespace affinity_daemon;
public static class CPU
    {
        public static readonly Dictionary<int, Core> Cores = [];
        public static readonly Dictionary<PerformanceGroup, List<Core>> CoresByGroup = new()
        {
            { PerformanceGroup.HighPerformanceCore, [] },
            { PerformanceGroup.PerformanceCore, [] },
            { PerformanceGroup.EfficiencyCore, [] },
            { PerformanceGroup.UltraLowPowerCore, [] },
            { PerformanceGroup.UnknownCore, [] },
        };

        public class Core
        {
            public int LogicalCore { get; set; }
            public int PhysicalCore { get; set; }
            public PerformanceGroup CoreClassification { get; set; } = PerformanceGroup.UnknownCore;
            public int MaxMhz { get; set; }
            public bool IsSMT { get; set; }
            public override string ToString() => $"{CoreClassification}: {LogicalCore}(P:{PhysicalCore}{(IsSMT ? "[SMT])" : ")")}";
        }

        public static void Initialize()
        {
            Console.WriteLine($"Reading cpu data... (lscpu -e -J)");
            var result = Kernel.RunCommand("lscpu", "-e -J");
            var cpuData = JsonSerializer.Deserialize<CpuData>(result)!;
            Console.WriteLine($"CPU data read: {cpuData.cpus.Count} cores");

            cpuData.cpus.OrderByDescending(x => x.maxmhz).ToList().ForEach(cpu => AddCore(new CPU.Core
            {
                LogicalCore = cpu.cpu,
                PhysicalCore = cpu.core,
                MaxMhz = (int)cpu.maxmhz,
            }));

            var freqGroups = Cores.Values.GroupBy(c => c.MaxMhz).OrderByDescending(x => x.Key).ToList();
            foreach (var core in Cores)
            {
                Console.WriteLine($"Frequency group: {core.Value.CoreClassification}: {freqGroups.FindIndex(g => g.Key == core.Value.MaxMhz)}");
                core.Value.CoreClassification = (PerformanceGroup)freqGroups.FindIndex(g => g.Key == core.Value.MaxMhz);
                CoresByGroup[core.Value.CoreClassification].Add(core.Value);
            }

            PrintCores();
        }


        public static void AddCore(Core core)
        {
            foreach (var c in Cores.Values)
                if (c.PhysicalCore == core.PhysicalCore)
                    core.IsSMT = true;

            Cores.Add(core.LogicalCore, core);
        }

        public static void PrintCores()
        {
            Console.WriteLine("Class \t\t\t MHz \t Logical ID \t Physical ID \t Real Core");
            foreach (var core in Cores.Values.OrderByDescending(c => c.MaxMhz))
                Console.WriteLine($"{core.CoreClassification}: \t {core.MaxMhz} \t {core.LogicalCore} \t\t {core.PhysicalCore}\t\t {(core.IsSMT ? "no" : "yes")}");
        }
    }