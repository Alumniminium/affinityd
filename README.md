# affinityd

## Description

`affinityd` is a daemon that manages the affinity of processes on a Hyprland system.

## Usage

use `exec-once` to run `affinityd` on Hyprland startup.

```lua
exec-once = sudo /path/to/affinityd
```

## Configuration

Configuration is done via `/etc/affinityd.json`.

The following configuration options are available:

### `affinityd.json`

```json
{
  "Apps": [
    {
      "Name": "code",
      "SpecificCores": [],
      "OnlyRealCores": false,
      "IsForegroundApp": true,
      "FoundInForegroundTimes": 71,
      "DisableGroup": 3,
      "ForceGroup": 0
    },
    {
      "Name": "chromium",
      "SpecificCores": [],
      "OnlyRealCores": false,
      "IsForegroundApp": true,
      "FoundInForegroundTimes": 28,
      "DisableGroup": 4,
      "ForceGroup": 0
    },
    {
      "Name": "discord",
      "SpecificCores": [],
      "OnlyRealCores": false,
      "IsForegroundApp": true,
      "FoundInForegroundTimes": 4,
      "DisableGroup": 0,
      "ForceGroup": 3
    }
}
```

- `Apps`: An array of application objects.
- `Name`: The name of the application.
- `SpecificCores`: An array of specific cores to use for the application.
  - If empty, all available cores will be used.
- `OnlyRealCores`: If true, only real cores will be used for the application.
  - If false, all available cores will be used.
- `IsForegroundApp`: If true, the application was found in the foreground.
- `FoundInForegroundTimes`: The number of times the application has been found in the foreground.
- `DisableGroup`: The group to disable the application in.
  - If 0, the application will not run on high performance cores.
  - If 1, the application will not run on performance cores.
  - If 2, the application will not run on efficiency cores.
  - If 3, the application will not run on ultra low power cores.
- `ForceGroup`: The group to force the application to run in.
  - If 0, the application will run on high performance cores.
  - If 1, the application will run on performance cores.
  - If 2, the application will run on efficiency cores.
  - If 3, the application will run on ultra low power cores.

## License

This project is licensed under the **Giant Penis License** - see the [LICENSE](LICENSE) file for details. This