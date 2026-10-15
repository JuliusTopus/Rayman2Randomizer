# Rebuilding Rayman 2 Randomizer Beta 0.1.5 (Windows)

## Requirements

- Windows and Visual Studio with MSVC C++ x86 build tools, Windows SDK, and the author's
  existing working `Rayman2ObjectTypeInjector` C++ DLL project (`.slnx` or `.vcxproj`).
- The project's own `pch.h`, `ACP_Ray2.h`, `Ray2x/ACP_Ray2x.h`, and the linked import libraries
  `ACP_Ray2.lib` / `ACP_Ray2x.lib` and any additional normal project dependencies.
  These external dependencies are **not included in this source archive**.
- The Microsoft .NET Framework 4.x C# compiler (`csc.exe`) to build the WinForms launcher.

Relevant external upstream projects:
- ACP_Ray2 source: https://github.com/raytools/ACP_Ray2
- Twofold source/release: https://github.com/spitfirex86/Twofold

## A. Build the x86 native mod DLL

1. In Windows, back up the original `dllmain.cpp` in the existing working Visual Studio project.
2. Replace that file with this package's **exact** `src/dllmain.cpp`.
3. Open the existing solution / C++ project in Visual Studio.
4. Build the DLL project in **Release | Win32 (x86)**. If the solution's `.slnx` reports
   MSB4126 for `Release|Win32`, choose/configure the Win32 project platform or build the
   `.vcxproj` directly. Do **not** switch the DLL to x64.
5. Locate the newly produced Win32 DLL. Rename/copy this **compiled DLL output** to
   `Rayman2Randomizer.dll` next to `src/Rayman2Randomizer_Launcher.cs` or use `BUILD_LAUNCHER_WINDOWS.bat`.

**Note:** Because the Visual Studio project and external dependency files are not in this archive,
these steps document the compilation method originally used, rather than claiming a fully self-contained build.
Rebuilds may have different binary SHA-256 hashes due to toolchain metadata or build configuration.

## B. Build the C# installer/launcher EXE

1. With the Release/Win32 DLL available, place a copy named `Rayman2Randomizer.dll` in this package root.
2. Double-click `BUILD_LAUNCHER_WINDOWS.bat`. It compiles the launcher from
   `src/Rayman2Randomizer_Launcher.cs` using the .NET Framework C# compiler.
3. The resulting `build/Rayman2Randomizer_Beta0.1.5_Public_Rebuilt.exe` has the DLL
   embedded as a managed resource named `R2R_EMBEDDED_MOD_DLL`.
4. Run it only on a backup copy of the game to verify INSTALL & PLAY / RESTORE ORIGINAL.

The published release file was built and tested on the author's Windows PC; this package's
build script was prepared for third-party reviewers and has not been run on Windows by this assistant.

## C. Verify the submitted release

Python 3 (optional):

    python VERIFY_RELEASE.py "Rayman2Randomizer_Beta0.1.5_Public_Release.zip"

The script prints SHA-256 values, validates PE header architecture, and verifies that the
mod DLL is embedded exactly in the published executable. It does not constitute a virus scan
or a guarantee of safety. See `SHA256SUMS.txt` for the hashes of the original submitted release.
