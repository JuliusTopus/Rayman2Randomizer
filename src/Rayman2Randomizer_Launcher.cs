// Rayman 2 Randomizer - Beta 0.1.5 Release Candidate
// Single-file WinForms installer/launcher. Build embeds the developer-compiled x86 mod DLL
// as a managed resource. No external Python/zip extraction is needed by end users.
// Copyright (c) 2026 Rayman 2 Randomizer contributors. Source included for review.
using System;
using System.IO;
using System.Text;
using System.Drawing;
using System.Windows.Forms;
using System.Diagnostics;
using System.Net;
using System.Text.RegularExpressions;
using System.IO.Compression;
using System.Reflection;
using System.Security.Cryptography;
using System.Collections.Generic;
using Microsoft.Win32;

internal static class Program
{
    [STAThread]
    static void Main()
    {
        Application.EnableVisualStyles();
        Application.SetCompatibleTextRenderingDefault(false);
        Application.Run(new BetaLauncher());
    }
}

internal sealed class BetaLauncher : Form
{
    const string ModuleFile = "Rayman2Randomizer.dll";
    const string IniFile = "Rayman2Randomizer.ini";
    const string ResourceName = "R2R_EMBEDDED_MOD_DLL";
    const string InstallMarker = "r2r-beta-015-install.txt";
    const string TwofoldReleases = "https://github.com/spitfirex86/Twofold/releases";
    const string TwofoldApi = "https://api.github.com/repos/spitfirex86/Twofold/releases/latest";
    const string BackupFolder = "R2R-Originals";

    TextBox folderBox, seedBox;
    CheckBox stat, move, music, sounds, textures, texts, items;
    Label status;
    Button installButton;

    static readonly Color Bg = Color.FromArgb(20, 25, 37);
    static readonly Color Card = Color.FromArgb(30, 38, 54);
    static readonly Color Fore = Color.FromArgb(233, 241, 250);
    static readonly Color Muted = Color.FromArgb(159, 175, 196);
    static readonly Color Accent = Color.FromArgb(67, 158, 249);

    public BetaLauncher()
    {
        Text = "Rayman 2 Randomizer  |  Beta 0.1.5";
        ClientSize = new Size(730, 666);
        MinimumSize = new Size(730, 690);
        StartPosition = FormStartPosition.CenterScreen;
        FormBorderStyle = FormBorderStyle.Sizable;
        Font = new Font("Segoe UI", 10f);
        BackColor = Bg;
        ForeColor = Fore;
        AutoScroll = true;

        Label heading = MakeLabel("RAYMAN 2 RANDOMIZER", 23, 14, 22, 675, 40, true);
        heading.ForeColor = Color.White;
        Controls.Add(heading);
        Controls.Add(MakeLabel("BETA 0.1.5  |  INSTALL, CONFIGURE && PLAY", 10, 16, 62, 680, 24, false));

        Controls.Add(MakeLabel("GAME FOLDER", 10, 18, 106, 650, 24, true));
        folderBox = new TextBox { Left = 18, Top = 133, Width = 584, Height = 31, Font = new Font("Segoe UI", 10), BackColor = Card, ForeColor = Fore, BorderStyle = BorderStyle.FixedSingle };
        Controls.Add(folderBox);
        Button browse = MakeButton("Browse", 608, 131, 102, 34);
        browse.Click += delegate { BrowseGame(); };
        Controls.Add(browse);
        Controls.Add(MakeLabel("Select Rayman2.exe. If Twofold is missing, the official installer will be offered.", 9, 18, 174, 680, 29, false));

        Panel opts = new Panel { Left = 18, Top = 209, Width = 692, Height = 272, BackColor = Card, Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right };
        Controls.Add(opts);
        Label optTitle = MakeLabel("RANDOMIZER OPTIONS", 11, 14, 12, 600, 24, true);
        opts.Controls.Add(optTitle);
        stat = MakeCheck(opts, "Stats (Health)", 18, 52, true);
        move = MakeCheck(opts, "Movement Speeds", 18, 91, false);
        music = MakeCheck(opts, "Music", 18, 130, true);
        sounds = MakeCheck(opts, "Sound Effects", 18, 169, false);
        textures = MakeCheck(opts, "Textures", 360, 52, false);
        texts = MakeCheck(opts, "Texts", 360, 91, false);
        items = MakeCheck(opts, "Items (EXPERIMENTAL)", 360, 130, false);
        items.ForeColor = Color.FromArgb(255, 197, 98);
        CheckBox enemies = MakeCheck(opts, "Enemy Randomizer (COMING SOON)", 360, 169, false);
        enemies.Enabled = false;
        enemies.ForeColor = Muted;
        opts.Controls.Add(MakeLabel("Items are experimental (softlocks possible). Enemy mode is coming soon.", 9, 18, 217, 652, 38, false));
        stat.CheckedChanged += delegate { move.Enabled = stat.Checked; if (!stat.Checked) move.Checked = false; };

        Controls.Add(MakeLabel("SEED", 10, 18, 502, 100, 26, true));
        seedBox = new TextBox { Left = 85, Top = 502, Width = 340, Height = 31, BackColor = Card, ForeColor = Fore, Text = NewSeed().ToString(), BorderStyle = BorderStyle.FixedSingle };
        Controls.Add(seedBox);
        Button reroll = MakeButton("New Seed", 438, 501, 153, 34);
        reroll.Click += delegate { seedBox.Text = NewSeed().ToString(); };
        Controls.Add(reroll);
        Controls.Add(MakeLabel("The same seed and settings produce repeatable results where supported.", 9, 18, 541, 690, 26, false));

        installButton = MakeButton("INSTALL && PLAY", 18, 578, 292, 48);
        installButton.BackColor = Accent;
        installButton.ForeColor = Color.FromArgb(10, 22, 39);
        installButton.Font = new Font("Segoe UI Semibold", 11, FontStyle.Bold);
        installButton.Click += delegate { InstallAndPlay(); };
        Controls.Add(installButton);
        Button restore = MakeButton("RESTORE ORIGINAL", 325, 578, 240, 48);
        restore.Click += delegate { Restore(); };
        Controls.Add(restore);
        Button logs = MakeButton("Logs", 577, 578, 133, 48);
        logs.Click += delegate { OpenLogs(); };
        Controls.Add(logs);
        status = MakeLabel("Public beta: back up saves and report any bugs.", 9, 18, 632, 688, 28, false);
        Controls.Add(status);

        folderBox.Text = FindGame();
        if (folderBox.Text.Length != 0) LoadExistingSettings();
    }

    static Label MakeLabel(string t, float size, int x, int y, int w, int h, bool strong)
    {
        return new Label { Text = t, Left = x, Top = y, Width = w, Height = h, Font = new Font("Segoe UI", size, strong ? FontStyle.Bold : FontStyle.Regular), ForeColor = strong ? Fore : Muted, BackColor = Color.Transparent, AutoEllipsis = false };
    }

    static Button MakeButton(string t, int x, int y, int w, int h)
    {
        return new Button { Text = t, Left = x, Top = y, Width = w, Height = h, FlatStyle = FlatStyle.Flat, BackColor = Card, ForeColor = Fore, Cursor = Cursors.Hand, UseVisualStyleBackColor = false };
    }

    static CheckBox MakeCheck(Control parent, string text, int x, int y, bool initial)
    {
        CheckBox c = new CheckBox { Text = text, Left = x, Top = y, Width = 324, Height = 33, Checked = initial, BackColor = Card, ForeColor = Fore, AutoSize = false };
        parent.Controls.Add(c);
        return c;
    }

    static int NewSeed()
    {
        byte[] b = new byte[4];
        using (RandomNumberGenerator g = RandomNumberGenerator.Create()) g.GetBytes(b);
        int n = BitConverter.ToInt32(b, 0) & 0x7fffffff;
        return n == 0 ? 1 : n;
    }

    static bool ValidGame(string dir)
    {
        return !String.IsNullOrEmpty(dir) && Directory.Exists(dir) && File.Exists(Path.Combine(dir, "Rayman2.exe"));
    }

    static string FindGame()
    {
        List<string> bases = new List<string>();
        string current = Environment.CurrentDirectory;
        bases.Add(current);
        bases.Add(AppDomain.CurrentDomain.BaseDirectory);
        try
        {
            object p = Registry.GetValue(@"HKEY_CURRENT_USER\Software\Valve\Steam", "SteamPath", null);
            if (p != null) bases.Add(p.ToString().Replace('/', '\\'));
        }
        catch { }
        string p86 = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86);
        string p64 = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles);
        bases.Add(p86); bases.Add(p64);
        foreach (string b in bases)
        {
            if (String.IsNullOrEmpty(b)) continue;
            string[] candidates = { b, Path.Combine(b, "Rayman 2"), Path.Combine(b, "Rayman 2 - The Great Escape"), Path.Combine(b, "Rayman 2 The Great Escape"), Path.Combine(b, @"steamapps\common\Rayman 2"), Path.Combine(b, @"steamapps\common\Rayman 2 The Great Escape"), Path.Combine(b, @"Ubisoft\Rayman2") };
            foreach (string c in candidates) if (ValidGame(c)) return c;
        }
        return "";
    }

    void BrowseGame()
    {
        using (FolderBrowserDialog d = new FolderBrowserDialog())
        {
            d.Description = "Select the folder containing Rayman2.exe";
            if (Directory.Exists(folderBox.Text)) d.SelectedPath = folderBox.Text;
            if (d.ShowDialog(this) == DialogResult.OK) { folderBox.Text = d.SelectedPath; LoadExistingSettings(); }
        }
    }

    static string ReadIniKey(string file, string name)
    {
        if (!File.Exists(file)) return null;
        bool inSection = false;
        foreach (string raw in File.ReadAllLines(file))
        {
            string s = raw.Trim();
            if (s.StartsWith("[")) { inSection = s.Equals("[Randomizer]", StringComparison.OrdinalIgnoreCase); continue; }
            if (!inSection || s.StartsWith(";") || s.StartsWith("#")) continue;
            int k = s.IndexOf('=');
            if (k >= 0 && s.Substring(0,k).Trim().Equals(name,StringComparison.OrdinalIgnoreCase)) return s.Substring(k+1).Trim();
        }
        return null;
    }

    static void SetCheck(CheckBox control, string file, string key)
    {
        string v = ReadIniKey(file, key);
        if (v != null) control.Checked = v == "1";
    }

    void LoadExistingSettings()
    {
        try
        {
            string file = Path.Combine(folderBox.Text, IniFile);
            SetCheck(stat, file, "Stats"); SetCheck(move, file, "RandomMovementSpeeds");
            SetCheck(music, file, "Music"); SetCheck(sounds, file, "Sounds");
            SetCheck(textures, file, "Textures"); SetCheck(texts, file, "Texts");
            SetCheck(items, file, "Items");
            string seed = ReadIniKey(file, "Seed");
            if (seed != null) { int n; if (Int32.TryParse(seed,out n) && n>0) seedBox.Text = seed; }
        }
        catch { }
    }

    string MakeIni(int seed)
    {
        StringBuilder s = new StringBuilder();
        s.AppendLine("[Randomizer]");
        s.AppendLine("; Rayman 2 Randomizer Beta 0.1.5");
        s.AppendLine("; Experimental Item Randomizer enabled only when specifically selected.");
        s.AppendLine("Enabled=1");
        s.AppendLine("Seed=" + seed);
        s.AppendLine("Stats=" + (stat.Checked ? 1 : 0));
        s.AppendLine("RandomMovementSpeeds=" + (stat.Checked && move.Checked ? 1 : 0));
        s.AppendLine("Music=" + (music.Checked ? 1 : 0));
        s.AppendLine("MusicAllAPM=1");
        s.AppendLine("Sounds=" + (sounds.Checked ? 1 : 0));
        s.AppendLine("Textures=" + (textures.Checked ? 1 : 0));
        s.AppendLine("Texts=" + (texts.Checked ? 1 : 0));
        s.AppendLine("Items=" + (items.Checked ? 1 : 0));
        s.AppendLine("Enemies=0");
        s.AppendLine("EnemiesMenuTest=0");
        s.AppendLine("EnemiesWorldRemixTest=0");
        s.AppendLine("TestingPowers=0");
        return s.ToString();
    }

    static byte[] EmbeddedDll()
    {
        using (Stream s = Assembly.GetExecutingAssembly().GetManifestResourceStream(ResourceName))
        {
            if (s == null) throw new IOException("Embedded DLL is missing. Rebuild using BUILD_INSTALLER.bat.");
            using (MemoryStream m = new MemoryStream()) { s.CopyTo(m); return m.ToArray(); }
        }
    }

    static bool IsPE32Dll(byte[] bytes)
    {
        if (bytes.Length < 512 || bytes[0] != 'M' || bytes[1] != 'Z') return false;
        int pe = BitConverter.ToInt32(bytes, 0x3c);
        if (pe < 0x40 || pe > bytes.Length - 24) return false;
        if (bytes[pe] != 'P' || bytes[pe + 1] != 'E' || bytes[pe + 2] != 0 || bytes[pe + 3] != 0) return false;
        int machine = BitConverter.ToUInt16(bytes, pe + 4);
        int properties = BitConverter.ToUInt16(bytes, pe + 22);
        return machine == 0x014c && (properties & 0x2000) != 0; // x86 DLL, not x64 or EXE
    }

    string CheckGame()
    {
        string root = folderBox.Text.Trim().Trim('"');
        if (!ValidGame(root)) throw new IOException("Select a valid Rayman 2 folder containing Rayman2.exe.");
        return Path.GetFullPath(root);
    }

    static string Timestamp() { return DateTime.UtcNow.ToString("yyyyMMdd-HHmmss"); }

    // Backups are once-only: future updates NEVER overwrite the pre-mod files.
    static void BackupOnce(string backupDir, string file, string backupName)
    {
        string backup = Path.Combine(backupDir, backupName);
        string missing = backup + ".original_missing";
        if (File.Exists(backup) || File.Exists(missing)) return;
        if (File.Exists(file)) File.Copy(file, backup, false);
        else File.WriteAllText(missing, "No original file was present before installation.");
    }

    // Install the third-party Twofold loader from its official GitHub release ONLY when
    // the user explicitly approves. Never replace files owned by another installation.
    // Fail closed on unexpected archives, URLs, or filesystem conflicts.
    static void InstallTwofoldFromOfficialRelease(string root)
    {
        // TLS1.2 is needed for GitHub on Windows with older .NET defaults.
        ServicePointManager.SecurityProtocol |= (SecurityProtocolType)3072;
        string json;
        using (WebClient client = new WebClient())
        {
            client.Headers[HttpRequestHeader.UserAgent] = "Rayman2Randomizer-Beta015-Installer";
            client.Headers[HttpRequestHeader.Accept] = "application/vnd.github+json";
            json = client.DownloadString(TwofoldApi);
        }
        MatchCollection urls = Regex.Matches(json,
            "\\\"browser_download_url\\\"\\s*:\\s*\\\"([^\\\"]+\\.zip)\\\"",
            RegexOptions.IgnoreCase);
        string asset = null;
        foreach (Match m in urls)
        {
            string candidate = m.Groups[1].Value.Replace("\\/", "/");
            Uri uri;
            if (!Uri.TryCreate(candidate, UriKind.Absolute, out uri) ||
                uri.Scheme != Uri.UriSchemeHttps ||
                !uri.Host.Equals("github.com", StringComparison.OrdinalIgnoreCase) ||
                !uri.AbsolutePath.StartsWith("/spitfirex86/Twofold/releases/download/", StringComparison.OrdinalIgnoreCase) ||
                !uri.AbsolutePath.EndsWith(".zip", StringComparison.OrdinalIgnoreCase)) continue;
            asset = candidate;
            break;
        }
        if (asset == null) throw new IOException("No valid ZIP found in the latest official Twofold release. See: " + TwofoldReleases);
        byte[] data;
        using (WebClient client = new WebClient())
        {
            client.Headers[HttpRequestHeader.UserAgent] = "Rayman2Randomizer-Beta015-Installer";
            data = client.DownloadData(asset);
        }
        if (data.Length < 512 || data.Length > 25000000)
            throw new IOException("Unexpected Twofold archive size. Nothing was installed.");
        using (MemoryStream ms = new MemoryStream(data))
        using (ZipArchive archive = new ZipArchive(ms, ZipArchiveMode.Read))
        {
            string prefix = null;
            foreach (ZipArchiveEntry entry in archive.Entries)
            {
                string normalized = entry.FullName.Replace('\\', '/');
                if (normalized.Equals("Ray2x.exe", StringComparison.OrdinalIgnoreCase)) { prefix = ""; break; }
                if (normalized.EndsWith("/Ray2x.exe", StringComparison.OrdinalIgnoreCase))
                { prefix = normalized.Substring(0, normalized.Length - "Ray2x.exe".Length); break; }
            }
            if (prefix == null) throw new IOException("Twofold ZIP is missing Ray2x.exe.");
            List<Tuple<ZipArchiveEntry, string>> plan = new List<Tuple<ZipArchiveEntry, string>>();
            long total = 0;
            foreach (ZipArchiveEntry entry in archive.Entries)
            {
                string full = entry.FullName.Replace('\\', '/');
                if (!full.StartsWith(prefix, StringComparison.OrdinalIgnoreCase)) continue;
                string relative = full.Substring(prefix.Length);
                if (relative.Length == 0 || relative.EndsWith("/")) continue;
                string[] parts = relative.Split('/');
                bool bad = relative.StartsWith("/") || relative.Contains(":") || parts.Length > 8;
                foreach (string part in parts)
                    if (part.Length == 0 || part == "." || part == ".." || part.EndsWith(".") || part.EndsWith(" ")) bad = true;
                if (bad) throw new IOException("Unsafe file name in Twofold ZIP. Nothing was installed.");
                total += entry.Length;
                if (total > 50000000 || plan.Count >= 100) throw new IOException("Twofold ZIP is too large.");
                string destination = Path.GetFullPath(Path.Combine(root, relative.Replace('/', Path.DirectorySeparatorChar)));
                string rootPrefix = Path.GetFullPath(root).TrimEnd(Path.DirectorySeparatorChar) + Path.DirectorySeparatorChar;
                if (!destination.StartsWith(rootPrefix, StringComparison.OrdinalIgnoreCase)) throw new IOException("Unsafe Twofold file path.");
                if (File.Exists(destination) || Directory.Exists(destination))
                    throw new IOException("Twofold appears partially installed (existing file: " + relative + "). Existing files will not be overwritten. " + TwofoldReleases);
                plan.Add(Tuple.Create(entry, destination));
            }
            if (plan.Count == 0) throw new IOException("Twofold ZIP empty");
            List<string> written = new List<string>();
            try
            {
                foreach (Tuple<ZipArchiveEntry, string> job in plan)
                {
                    Directory.CreateDirectory(Path.GetDirectoryName(job.Item2));
                    // Register the created file for rollback before writing its contents.
                    using (FileStream output = new FileStream(job.Item2, FileMode.CreateNew, FileAccess.Write, FileShare.None))
                    {
                        written.Add(job.Item2);
                        using (Stream input = job.Item1.Open()) input.CopyTo(output);
                    }
                }
            }
            catch
            {
                foreach (string file in written) { try { File.Delete(file); } catch { } }
                throw;
            }
        }
        if (!File.Exists(Path.Combine(root, "Ray2x.exe"))) throw new IOException("Twofold install is incomplete.");
    }

    void InstallAndPlay()
    {
        installButton.Enabled = false;
        try
        {
            string root = CheckGame();
            int seed;
            if (!Int32.TryParse(seedBox.Text.Trim(), out seed) || seed <= 0) throw new IOException("Seed must be an integer from 1 to 2147483647.");
            string loader = Path.Combine(root, "Ray2x.exe");
            if (!File.Exists(loader))
            {
                DialogResult yes = MessageBox.Show(this,
                    "Twofold / Ray2x.exe was not found.\n\nWould you like to download and install the latest official release from github.com/spitfirex86/Twofold?\n\nAn internet connection is required. Existing files will not be overwritten.",
                    "Install the official Twofold loader", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                if (yes != DialogResult.Yes)
                    throw new IOException("Installation canceled: Twofold is required to launch this mod. " + TwofoldReleases);
                status.Text = "Downloading the official Twofold loader...";
                Application.DoEvents();
                InstallTwofoldFromOfficialRelease(root);
            }
            byte[] dll = EmbeddedDll();
            if (!IsPE32Dll(dll)) throw new IOException("The embedded DLL is not a valid Win32 x86 DLL.");
            if (items.Checked && MessageBox.Show(this, "Item Randomizer is EXPERIMENTAL. Some items may be missing or block progression. Continue?", "Warning - Experimental feature", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) != DialogResult.Yes) return;
            string mods = Path.Combine(root, "Mods");
            string backupDir = Path.Combine(root, BackupFolder);
            Directory.CreateDirectory(mods);
            Directory.CreateDirectory(backupDir);
            string modPath = Path.Combine(mods, ModuleFile);
            string configPath = Path.Combine(root, IniFile);
            string markerPath = Path.Combine(backupDir, InstallMarker);
            BackupOnce(backupDir, modPath, "Rayman2Randomizer.dll.before-R2R");
            BackupOnce(backupDir, configPath, "Rayman2Randomizer.ini.before-R2R");
            // Write to a temp file then swap in. If a write fails, avoid a partially-written DLL.
            string staging = modPath + ".r2r-install.tmp";
            try
            {
                File.WriteAllBytes(staging, dll);
                // Replace atomically; never delete the existing DLL before the new one is ready.
                if (File.Exists(modPath)) File.Replace(staging, modPath, modPath + ".previous-R2R-update", true);
                else File.Move(staging, modPath);
                string stagedIni = configPath + ".r2r-install.tmp";
                try
                {
                    File.WriteAllText(stagedIni, MakeIni(seed), new UTF8Encoding(false));
                    if (File.Exists(configPath)) File.Replace(stagedIni, configPath, configPath + ".previous-R2R-update", true);
                    else File.Move(stagedIni, configPath);
                }
                finally { if (File.Exists(stagedIni)) File.Delete(stagedIni); }
                File.WriteAllText(markerPath, "Rayman 2 Randomizer Beta 0.1.5\r\nInstalled UTC: " + DateTime.UtcNow.ToString("o") + "\r\n", Encoding.UTF8);
            }
            finally { if (File.Exists(staging)) File.Delete(staging); }
            status.Text = "Mod installed. Launching Ray2x.exe...";
            Process.Start(new ProcessStartInfo { FileName = loader, WorkingDirectory = root, UseShellExecute = true });
        }
        catch (Exception e)
        {
            status.Text = "Error: " + e.Message;
            MessageBox.Show(this, e.Message, "Installation failed", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
        finally { installButton.Enabled = true; }
    }

    void Restore()
    {
        try
        {
            string root = CheckGame();
            string backupDir = Path.Combine(root, BackupFolder);
            string marker = Path.Combine(backupDir, InstallMarker);
            if (!File.Exists(marker)) throw new IOException("No previous beta installation was found. No files were removed.");
            string[] backups = { "Rayman2Randomizer.dll.before-R2R", "Rayman2Randomizer.ini.before-R2R" };
            foreach (string name in backups)
            {
                if (!File.Exists(Path.Combine(backupDir, name)) && !File.Exists(Path.Combine(backupDir, name + ".original_missing")))
                    throw new IOException("Incomplete backup. No files restored (missing: " + name + ").");
            }
            if (MessageBox.Show(this, "Restore the DLL and INI file backed up before this beta?", "Restore", MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes) return;
            RestoreOne(backupDir, "Rayman2Randomizer.dll.before-R2R", Path.Combine(root, "Mods", ModuleFile));
            RestoreOne(backupDir, "Rayman2Randomizer.ini.before-R2R", Path.Combine(root, IniFile));
            File.Delete(marker);
            status.Text = "Restore complete. All other game files were left unchanged.";
        }
        catch (Exception e) { MessageBox.Show(this, e.Message, "Restore failed", MessageBoxButtons.OK, MessageBoxIcon.Error); }
    }

    static void RestoreOne(string backupDir, string backupName, string dest)
    {
        string backup = Path.Combine(backupDir, backupName);
        string missing = backup + ".original_missing";
        if (File.Exists(backup)) File.Copy(backup, dest, true);
        else if (File.Exists(missing) && File.Exists(dest)) File.Delete(dest);
        else throw new IOException("No backup found for " + backupName + ": no files were deleted.");
    }

    void OpenLogs()
    {
        try { string dir = CheckGame(); Process.Start(new ProcessStartInfo { FileName = dir, UseShellExecute = true }); }
        catch (Exception e) { MessageBox.Show(this, e.Message); }
    }
}
