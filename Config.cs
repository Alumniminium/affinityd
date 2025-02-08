using System.Text.Json.Serialization;

namespace affinity_daemon;

public class Config
{
    [JsonIgnore]
    public bool Dirty = false;
    public HashSet<AppCfg> Apps { get; set; } = [];
}

public class AppCfg
{
    public string Name { get; set; } = "";
    public List<int> SpecificCores { get; set; } = [];
    public bool OnlyRealCores { get; set; } = false;
    public bool IsForegroundApp { get; set; }
    public int FoundInForegroundTimes { get; set; }
    public PerformanceGroup? DisableGroup { get; set; } = PerformanceGroup.UnknownCore;
    public PerformanceGroup? ForceGroup { get; set; } = PerformanceGroup.UnknownCore;
}
