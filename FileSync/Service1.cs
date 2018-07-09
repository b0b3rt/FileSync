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
            _defaultTakePath = Environment.GetFolderPath(Environment.SpecialFolder.d)
            string _configPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles), "FileSyncConfig");
            string _configName = "fsconfig.json";

            _configWatcher = new FileSystemWatcher(_configPath, _configName);
            _configWatcher.Changed += OnConfigChanged;

            string putPath = Environment.
            _fsWatcher = new FileSystemWatcher();
        }

        protected override void OnStop()
        {
        }

        private void OnConfigChanged(object sender, EventArgs eventArgs)
        {
            string fullConfigPath = Path.Combine(_configPath, _configName);

            using (var reader = new StreamReader(fullConfigPath))
            {
                //It's fine to not do this async, it's going to be a tiny file
                var jsonSettings = reader.ReadToEnd();
                var parsedSettings = JsonConvert.DeserializeObject<Dictionary<string, string>>(jsonSettings);

                _takePath = parsedSettings.TryGetValue("SourcePath", out string sourcePath)) ? sourcePath: 

            }
        }
    }
}
