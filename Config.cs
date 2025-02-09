using System.Collections;
using System.Text.Json.Serialization;

namespace affinity_daemon;

public class Config
{
    [JsonIgnore]
    public bool Dirty = false;
    public Dictionary<PerformanceGroup, HashSet<AppCfg>> AppsByGroup { get; set; } = []; 
}

public class AppCfg
{
    public string Name { get; set; } = "";
    public bool IsForegroundApp { get; set; }
    [JsonIgnore]
    public PerformanceGroup? ForceGroup { get; set; } = PerformanceGroup.UnknownCore;
}
