# Welcome to the FlaxSave API

Welcome to the **FlaxSave** C# API. This section of the documentation provides the technical specification for all user-facing classes and methods within **FlaxSave**.
</p> If you're looking for a step-by-step guide on setting up the plugin, check out the [Manual](../Manual/index.md)

## Namespaces
The API is organized by namespaces and you'll find that the primary namespace `FlaxSave` contains the core logic and managers.

|Class|Description|
|---|---|
|SaveManager|The main entry point. Handles save and load request, threading and task queue.|
|SaveMeta|Data model for save file information (names, dates, custom metadata)|
|FlaxSaveSettings|The engine integrated configuration asset for paths, versioning and auto-save settings.|
|ISavableAsset|Base interface for serializable assets, like engine settings and global configuration.|