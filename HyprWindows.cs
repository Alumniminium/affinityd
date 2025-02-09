using System.Text.Json;
using System.Text.Json.Serialization;

namespace affinity_daemon;

public static class HyprWindows
{
    public static Dictionary<int, AppCfg> Apps { get; private set; } = new();

    public static void GetState()
    {
        try
        {
            var hypr = Kernel.RunCommand("hyprctl", "activewindow -j");

            var app = JsonSerializer.Deserialize<Root>(hypr)!;
            var (group, appCfg) = Kernel.GetApp(app.Class);
            if (appCfg == null)
                return;

            if (appCfg.IsForegroundApp)
                return;

            Kernel.Config.AppsByGroup[group].Remove(appCfg);
            appCfg.IsForegroundApp = true;
            Kernel.Config.AppsByGroup[group].Add(appCfg);
            Kernel.Config.Dirty = true;
        }
        catch (Exception e)
        {
            Console.WriteLine($"Kernel: " + e.Message);
        }
    }
    public record Root(
        [property: JsonPropertyName("address")] string Address,
        [property: JsonPropertyName("mapped")] bool Mapped,
        [property: JsonPropertyName("hidden")] bool Hidden,
        [property: JsonPropertyName("at")] IReadOnlyList<int> At,
        [property: JsonPropertyName("size")] IReadOnlyList<int> Size,
        [property: JsonPropertyName("workspace")] Workspace Workspace,
        [property: JsonPropertyName("floating")] bool Floating,
        [property: JsonPropertyName("pseudo")] bool Pseudo,
        [property: JsonPropertyName("monitor")] int Monitor,
        [property: JsonPropertyName("class")] string Class,
        [property: JsonPropertyName("title")] string Title,
        [property: JsonPropertyName("initialClass")] string InitialClass,
        [property: JsonPropertyName("initialTitle")] string InitialTitle,
        [property: JsonPropertyName("pid")] int Pid,
        [property: JsonPropertyName("xwayland")] bool Xwayland,
        [property: JsonPropertyName("pinned")] bool Pinned,
        [property: JsonPropertyName("fullscreen")] int Fullscreen,
        [property: JsonPropertyName("fullscreenClient")] int FullscreenClient,
        [property: JsonPropertyName("grouped")] IReadOnlyList<object> Grouped,
        [property: JsonPropertyName("tags")] IReadOnlyList<object> Tags,
        [property: JsonPropertyName("swallowing")] string Swallowing,
        [property: JsonPropertyName("focusHistoryID")] int FocusHistoryID,
        [property: JsonPropertyName("inhibitingIdle")] bool InhibitingIdle
    );

    public record Workspace(
        [property: JsonPropertyName("id")] int Id,
        [property: JsonPropertyName("name")] string Name
    );
}