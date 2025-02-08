namespace affinity_daemon;

[Flags]
public enum PerformanceGroup
{
    HighPerformanceCore = 0,
    PerformanceCore = 1,
    EfficiencyCore = 2,
    UltraLowPowerCore = 3,
    UnknownCore = 4,
}
