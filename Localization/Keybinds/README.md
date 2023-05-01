# Localization Note

If a keybind is generated via tool registration (See `Register()` function in `Tool.cs`), its translation entry in languages other than English **should not** be changed. It will be automatically assigned to the tool's localization entry.

Modders must redirect localized entries for auto-generated keybinds to the corresponding tool.

Only keybinds registered in `ExtraKeybindSystem.cs` should be translated.