# Rayman 2 Randomizer — Beta 0.1.5 (source for Nexus Mods review)

Source-code review package for the public release currently submitted on Nexus Mods.

**Mod page:** https://www.nexusmods.com/mods/1?game_id=10631&published=1

This package contains the original C++ source for the x86 game mod (`src/dllmain.cpp`),
the C# WinForms installer/launcher (`src/Rayman2Randomizer_Launcher.cs`),
a representative INI file, build instructions, and security/release verification notes.

## What the release does

- Launches Rayman 2 using the Twofold mod loader with a mod DLL in the game's `Mods/` folder.
- Provides configurable randomization of stats, speeds, music, sound effects, textures, and texts.
- Item randomization is optional and experimental; enemy randomization is disabled.
- Creates backups of the mod DLL and INI and has a `Restore Original` action.
- Can offer to download Twofold **only after explicit user confirmation**, from the official GitHub project.

## Important limits of the provided source

The C++ file was compiled as part of the author's existing Visual Studio C++ game-mod project.
That project contains additional build scaffolding, a precompiled header and external headers and import libraries
(see `BUILD_INSTRUCTIONS.md`). Those project-specific files were not provided in this review archive;
therefore a standalone, independently confirmed DLL rebuild from only this ZIP is **not** guaranteed.
The separate WinForms launcher has its exact C# source and an included rebuild script.
The published executable and embedded DLL SHA-256 fingerprints are documented in `SHA256SUMS.txt`.

The mod does not bundle proprietary Rayman 2 game files or a copy of Twofold.
It is an unofficial fan-made mod and is not endorsed by Ubisoft or the third-party loader developers.
AI assistance (OpenAI ChatGPT) was used in programming, troubleshooting, and release preparation.

For any questions about code provenance, build setup, network access, or the original Visual Studio project,
the author is willing to provide further details to Nexus Mods staff.
