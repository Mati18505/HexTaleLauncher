using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using System.Windows;

namespace HexTaleLauncherLibrary
{
    public enum LauncherStatus
    {
        ready,
        failed,
        downloadingGame,
        downloadingUpdate,
        empty
    }

    public class Config
    {
        public string gameDir { get; set; } = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "HexTale");
        public string gameExe { get; set; } = "HexTale.exe";
        public string remoteURL { get; set; } = "https://hextale.xyz";
        public string remoteRootDir { get; set; } = "files";
        public string remoteGameDownloadLink { get; set; } = "DownloadLinks.json";
        public string remoteVersionControlFile { get; set; } = "versionControl.txt";
    }

    [JsonSerializable(typeof(Config))]
    public partial class ApiJsonSerializerContext : JsonSerializerContext
    {}

    class LauncherLogic
    {
        private string rootPath;
        private string versionControlFileDir;
        VersionControl versionControl;
        WebRemote remote;

        private LauncherStatus _status;
        internal LauncherStatus Status
        {
            get => _status;
            set
            {
                _status = value;
                string text = "";
                switch (_status)
                {
                    case LauncherStatus.ready:
                        text = "Play";
                        break;
                    case LauncherStatus.failed:
                        text = "Update Failed - Retry";
                        break;
                    case LauncherStatus.downloadingGame:
                        text = "Downloading Game";
                        break;
                    case LauncherStatus.downloadingUpdate:
                        text = "Downloading Update";
                        break;
                    case LauncherStatus.empty:
                        text = "Download";
                        break;
                    default:
                        break;
                }
                API.Status(text);
            }

        }
        class DownloadFileLinks
        {
            public string? clientDownload { get; set; }
        }

        Config config;
        Logger logger;

        public LauncherLogic()
        {
            logger = new Logger();

            rootPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "HexTaleLauncher");
            Directory.CreateDirectory(rootPath);
            versionControlFileDir = Path.Combine(rootPath, "versionControl.txt");

            JsonSerializerOptions opt = new JsonSerializerOptions();
            opt.TypeInfoResolver = new ApiJsonSerializerContext();
            opt.WriteIndented = true;
            config = LoadConfig(opt);

            remote = new(new Uri(config.remoteURL), config.remoteRootDir);

            var versionControlText = File.Exists(versionControlFileDir) ? File.ReadAllLines(versionControlFileDir) : Array.Empty<string>();
            versionControl = new(new List<string>(versionControlText), config.gameDir);
            Status = Verify() ? LauncherStatus.ready : LauncherStatus.empty;
        }

        private Config CreateConfig(JsonSerializerOptions opt, string configPath)
        {
            Config config = new Config();
            string text = JsonSerializer.Serialize(config, opt);
            Directory.CreateDirectory(Path.GetDirectoryName(configPath));
            File.WriteAllText(configPath, text);
            return config;
        }

        private Config LoadConfig(JsonSerializerOptions opt)
        {
            Config config;
            string configPath = Path.Combine(rootPath, "config.json");

            if (!File.Exists(configPath))
                config = CreateConfig(opt, configPath);
            else
            {
                string configFile = File.ReadAllText(configPath);

                try
                {
                    config = JsonSerializer.Deserialize<Config>(configFile, opt);
                }
                catch (JsonException ex)
                {
                    throw new LauncherException(LauncherException.Type.BadConfig, "Config file can't be deserialized!", ex);
                }
            }

            if (config == null)
                throw new LauncherException(LauncherException.Type.BadConfig, "Config file can't be deserialized!");
            return config;
        }

        CancellationTokenSource installationCancel = new CancellationTokenSource();

        public async Task CheckForUpdates()
        {
            try
            {
                Debug.WriteLine("Checking for updates...");
                if (versionControl.Empty())
                    Status = LauncherStatus.downloadingGame;
                else
                    Status = LauncherStatus.downloadingUpdate;

                var remoteVCString = remote.DownloadFileText(config.remoteVersionControlFile);
                var remoteVC = new VersionControl(remoteVCString.Split(Environment.NewLine).ToList());
                var toDownload = versionControl.GetFilePathsToDownload(remoteVC);
                var toRemove = versionControl.GetFilePathsToRemove(remoteVC);

                if (toDownload.Count > 0 || toRemove.Count > 0)
                {
                    logger.WriteLine("Files to download: " + toDownload.Count);
                    logger.WriteLine("Files to remove: " + toRemove.Count);
                    await CriticalAsync(InstallGameFiles(toDownload, toRemove, remoteVC, installationCancel.Token));
                }
                else
                {
                    Status = LauncherStatus.ready;
                }
            }
            catch (Exception ex)
            {
                Status = LauncherStatus.failed;
                throw new LauncherException(LauncherException.Type.General, "Error checking for game updates: ", ex);
            }
        }

        private async Task InstallGameFiles(List<string> toDownload, List<string> toRemove, VersionControl remoteVC, CancellationToken cancellationToken)
        {
            try
            {
                logger.WriteLine("Downloading files...");

                int max = toDownload.Count + toRemove.Count;
                int processed = 0;
                await API.ProgressBarValue(0);
                await API.ProgressBarVisibility(true);

                foreach (string pathToDownload in toDownload)
                {
                    string remoteFilePath = Path.Combine("HexTale", pathToDownload);
                    string localFilePath = Path.Combine(config.gameDir, pathToDownload);
                    logger.WriteLine("Downloading " + pathToDownload);

                    async Task DownloadFile()
                    {
                        await Task.Run(() => remote.DownloadFile(remoteFilePath, localFilePath));
                        versionControl.AddToVersionControl(localFilePath);
                    }

                    if(!File.Exists(localFilePath))
                    {
                        await DownloadFile();
                    }
                    else
                    {
                        versionControl.AddToVersionControl(localFilePath);
                        if(remoteVC.vc.hashes[pathToDownload] != versionControl.vc.hashes[pathToDownload])
                        {
                            await DownloadFile();
                        }
                    }

                    cancellationToken.ThrowIfCancellationRequested();

                    processed++;
                    int progress = (processed * 100) / max;
                    await API.ProgressBarValue(progress);
                }

                logger.WriteLine("Deleting files...");
                foreach (string pathToRemove in toRemove)
                {
                    logger.WriteLine("Deleting " + pathToRemove);
                    File.Delete(Path.Combine(config.gameDir, pathToRemove));

                    versionControl.DeleteFromVersionControl(pathToRemove);

                    processed++;
                    int progress = (processed * 100) / max;
                    await API.ProgressBarValue(progress);
                }

                await API.ProgressBarVisibility(false);
            }
            catch (OperationCanceledException)
            {
                Status = LauncherStatus.failed;
                logger.WriteLine("Install operation canceled.");
            }
            catch (Exception ex)
            {
                Status = LauncherStatus.failed;
                throw new LauncherException(LauncherException.Type.General, "Error installing game files: ", ex);
            }
            finally
            {
                await DownloadGameCompletedCallback();
            }
        }
        
        private async Task CriticalAsync(Task critical)
        {
            CriticalTasks.Add(critical);
            await critical;
        }
        
        private async Task CriticalAsync(Action action)
        {
            await CriticalAsync(Task.Run(action));
        }

        public static bool IsGameRunning() => Process.GetProcessesByName("HexTale").Length != 0;

        private void CalculateAndSaveVersionControlFile()
        {
            Directory.CreateDirectory(config.gameDir);
            versionControl.CalculateVersionControl();
            versionControl.WriteToFile(versionControlFileDir);
        }

        private void SaveVersionControlFile()
        {
            versionControl.WriteToFile(versionControlFileDir);
        }

        private async Task DownloadGameCompletedCallback()
        {
            try
            {
                logger.WriteLine("Saving version control file...");
                SaveVersionControlFile();

                logger.WriteLine("Verifying");
                bool success = Verify();
                string result = success ? "Success" : "Failed";
                logger.WriteLine(result);
                if (success)
                    Status = LauncherStatus.ready;
                else
                    Status = LauncherStatus.failed;
            }
            catch (Exception ex)
            {
                Status = LauncherStatus.failed;
                throw new LauncherException(LauncherException.Type.General, "Error finishing download:", ex);
            }
        }

        private bool Verify()
        {
            var localVC = versionControl;
            var remoteVCString = remote.DownloadFileText(config.remoteVersionControlFile);
            var remoteVC = new VersionControl(remoteVCString.Split(Environment.NewLine).ToList());
            return VersionControl.Equal(localVC, remoteVC);
        }

        private void RunGame(string gameExeDir)
        {
            var processStartInfo = new ProcessStartInfo
            {
                FileName = gameExeDir,
                Verb = "runas",
                UseShellExecute = true,
            };

            Process.Start(processStartInfo);
        }

        public void Run()
        {
            string gameExeDir = Path.Combine(config.gameDir, Path.GetFileName(config.gameExe));
            if (File.Exists(gameExeDir))
            {
                try
                {
                    RunGame(gameExeDir);
                }
                catch (Exception ex)
                {
                    throw new LauncherException(LauncherException.Type.General, "Could not run game", ex);
                }
            }
            else
                throw new LauncherException(LauncherException.Type.General, "Game exe file doesn't exist!");
        }

        public void OnClose()
        {
            installationCancel.Cancel();
            CriticalTasks.WaitOnExit();
        }

        public static void RecursiveDelete(DirectoryInfo baseDir)
        {
            if (!baseDir.Exists)
                return;

            foreach (var dir in baseDir.EnumerateDirectories())
            {
                RecursiveDelete(dir);
            }
            baseDir.Delete(true);
        }

        public void OnPlayButtonClick()
        {
            if (Status == LauncherStatus.ready)
            {
                Run();
            }
            else if (Status == LauncherStatus.failed || Status == LauncherStatus.empty)
            {
                CheckForUpdates().Wait();
            }
            else
                throw new LauncherException(LauncherException.Type.InvalidOperation);
        }

        public void OpenGameDirInFileExplorer()
        {
            Process.Start("explorer.exe", config.gameDir);
        }

        public async Task CheckAndRepair()
        {
            //Powiadamiaj o statusie, blokuj możliwość zrobienia dopóki nie skończy, nie obliczaj nowego versionControl jeśli nic nie było do pobrania i usunięcia
            if(Status == LauncherStatus.downloadingUpdate || Status == LauncherStatus.downloadingGame || IsGameRunning())
                throw new LauncherException(LauncherException.Type.InvalidOperation);

            Status = LauncherStatus.downloadingUpdate;
            logger.WriteLine("Calculating version control file...");
            await Task.Run(() => CalculateAndSaveVersionControlFile());
            logger.WriteLine("Checking for updates...");
            await CheckForUpdates();
        }

        public async Task Uninstall()
        {
            if(Status == LauncherStatus.ready && !IsGameRunning())
            {
                Status = LauncherStatus.empty;
                File.Delete(versionControlFileDir);
                versionControl.Clear();
                await Task.Run(() => RecursiveDelete(new DirectoryInfo(config.gameDir)));
            }
            else
                throw new LauncherException(LauncherException.Type.InvalidOperation);
        }

        private bool VerifyFiles()
        {
            versionControl.CalculateVersionControl();

            return Verify();
        }
    }
}
 