---
title: Installation & Setup
---

# Installation & Setup
**FlaxSave** is designed to be Plug&Play. Once the plugin is installed into your project, you only need to link a settings asset to begin using the save system.

## Installation


### Requirements

- **FlaxEngine** `v. 1.11` or above
- **Git** Installed and configured, for the [The Easy Way](#the-easy-way) method

---

### Plugin Installation

#### The Easy Way
- In the Flax Editor, go to `Tools > Plugins > Clone Project`
- Paste this repo link `https://github.com/Myterian/FlaxSave.git` into the `Git Path`
- Click `Clone`
- Restart the Editor
- Done


#### Manual Installation
- Close the Editor
- Clone the [FlaxSave repo](https://github.com/Myterian/FlaxSave/) into `<your-game-project-folder>\Plugins\FlaxSave\`
- Add a reference to FlaxEvent to your game, by modifying the `<your-game>.flaxproj` file
```
...
"References": [
    {
        "Name": "$(EnginePath)/Flax.flaxproj"
    },
    {
        "Name": "$(ProjectPath)/Plugins/FlaxSave/FlaxSave.flaxproj"
    }
]
...
```
- Restart the Editor
- Done

---

## Setup
After successfull installation, you can tell the engine which configuration to use. Without this step, FlaxSave will use its **Default Settings**. The default settings are great for prototyping, but may not satify your project needs during production.

### 1. Create the Settings Asset
In your **Content Window**, right-click and select `New > FlaxSave > Save Settings`.

---

### 2. Register with GameSettings
![Register the SaveSettings asset in the global GameSettings](../settings_place.jpg)

To activate your configuration, the save settings asset must be registered in the global `GameSettings`.

1. Open your projects `GameSettings`
2. Navigate to `Other Settings > Custom Settings`
3. Create a new entry with the save settings asset

!!! tip "Project Versioning"
    Open your save settings asset and set your initial `Savegame Version`. This ensure that even your first playtests savegames are correctly versioned.

---

### 3. Alternative Settings
Once linked, the `SaveManager` will automatically detect these settings on startup. For rapid testing, you can create more save settings assets with unique configurations, because only the save settings asset linked in `GameSettings` will affect the system.