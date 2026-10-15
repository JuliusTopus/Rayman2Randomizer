# Security review / executable behavior

This is a Windows game mod loader/installer, with an x86 DLL embedded inside a managed
.NET Framework WinForms executable. The DLL is written to:

    <Rayman 2 game directory>/Mods/Rayman2Randomizer.dll

The configuration is written to:

    <Rayman 2 game directory>/Rayman2Randomizer.ini

The launcher creates `R2R-Originals` backups where applicable, and the `Restore Original`
action restores DLL/INI backups. It starts `Ray2x.exe` to launch Rayman 2 with Twofold.
The user must already possess Rayman 2; no game executable or game assets are distributed.

## Internet use

The reviewed launcher source has a single optional download workflow for **Twofold**:

    GitHub API: https://api.github.com/repos/spitfirex86/Twofold/releases/latest
    GitHub release base: https://github.com/spitfirex86/Twofold/releases

This download is **not automatic on startup**: it is offered through a Yes/No confirmation
if the game folder does not contain `Ray2x.exe`. The code restricts download links to HTTPS
and the expected GitHub release path, checks file size and ZIP entry names/paths,
and refuses to overwrite existing files during the Twofold download.

The launcher has no telemetry or account-login functionality visible in the supplied C# source.
This note is a functional description, **not an independent security certification**;
Nexus Mods should conduct its usual review.

## Why automated scanning may need manual review

- The download includes a Windows `.exe` that embeds an x86 DLL as a resource.
- The native DLL interacts with a running game engine, as expected of this type of mod.
- The installer can (with user approval) fetch a separate third-party mod loader from GitHub.

None of the above proves the program safe or malicious on its own.
The published ZIP contains the EXE and four plain text files, with **no nested archives**.
Please review the code and binary hashes independently.
