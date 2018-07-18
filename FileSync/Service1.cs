using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using Newtonsoft.Json;
using Syroot.Windows.IO;
using Shell32;

namespace FileSync
{
    public partial class Service1 : ServiceBase
    {
        private FileSystemWatcher _fsWatcher;
        private FileSystemWatcher _configWatcher;

        private string _configPath { get; set; }
        private string _configName { get; set; }

        private string _defaultTakePath { get; set; }
        private string _takePath { get; set; }
        private string _putPath { get; set; }
        private string _extension { get; set; }

        public Service1()
        {
            InitializeComponent();
        }

        protected override void OnStart(string[] args)
        {
            _defaultTakePath = KnownFolders.Downloads.Path; // Environment.GetFolderPath(Environment.SpecialFolder.d)

            string _configPath = Path.Combine(KnownFolders.ProgramFiles.Path, "FileSyncConfig");
            string _configName = "fsconfig.json";

            SetConfig();

            _configWatcher = new FileSystemWatcher(_configPath, _configName);
            _configWatcher.Changed += OnConfigChanged;

            _fsWatcher = new FileSystemWatcher(_putPath);
            _fsWatcher.Created += OnDeviceInserted;
        }

        protected override void OnStop()
        {
        }

        private void OnDeviceInserted(object sender, EventArgs eventArgs) => SyncFiles();

        private void SyncFiles()
        {
            try
            {
                //Let's make this a fast operation with HashSet rather than List or Array
                var existingFileNames = new HashSet<string>(Directory.GetFiles(_putPath, "*." + _extension, SearchOption.TopDirectoryOnly).Select(f => Path.GetFileName(f)));
                var sourceFilesNames = Directory.GetFiles(_takePath, "*." + _extension, SearchOption.TopDirectoryOnly).Select(f => Path.GetFileName(f));

                //Figure out which ones are missing - we get the HashSet perfomance benefit here
                var missingFileNames = sourceFilesNames.Where(n => !existingFileNames.Contains(n)).ToList();

                //Copy without overwrite
                missingFileNames.ForEach(n => File.Copy(Path.Combine(_takePath, n), Path.Combine(_putPath, n)));

                //https://stackoverflow.com/a/42950627
                //This is dumb, but better than trying to set up WCF messaging just to tell them when it's done
                foreach (SHDocVw.InternetExplorer window in new SHDocVw.ShellWindows())
                {
                    if (Path.GetFileNameWithoutExtension(window.FullName).ToLowerInvariant() == "explorer")
                    {
                        if (Uri.IsWellFormedUriString(window.LocationURL, UriKind.Absolute))
                        {
                            string location = new Uri(window.LocationURL).LocalPath;

                            if (string.Equals(location, _putPath, StringComparison.OrdinalIgnoreCase))
                                window.Quit();
                        }
                    }
                }
            }
            catch (Exception e)
            {
                //My grandmother doesn't need error logs - just let her know something's wrong.
                throw e;
            }
        }

        private void OnConfigChanged(object sender, EventArgs eventArgs) => SetConfig();

        private void SetConfig()
        {
            string fullConfigPath = Path.Combine(_configPath, _configName);

            using (var reader = new StreamReader(fullConfigPath))
            {
                //It's fine to not do this async, it's going to be a tiny file
                var jsonSettings = reader.ReadToEnd();
                var parsedSettings = JsonConvert.DeserializeObject<Dictionary<string, string>>(jsonSettings);

                _takePath = parsedSettings.TryGetValue("SourcePath", out string sourcePath) ? sourcePath : _defaultTakePath;
                _putPath = parsedSettings.TryGetValue("DestinationPath", out string destinationPath) ? destinationPath : string.Empty;
                _extension = parsedSettings.TryGetValue("Extension", out string extension) ? extension : "mobi";
            }
        }
    }
}
