﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;
using System.Configuration;

namespace PowershellStarter
{

    
    public partial class PowershellStarterService : ServiceBase
    {

        
        public string ScriptPath { get; set; }
        public string ScriptArguments { get; set; }
        public string output;
        public string errorOutput;
        public ProcessStartInfo process = new ProcessStartInfo();

        public PowershellStarterService()
        {
            InitializeComponent();
            // Set eventlog
            if (!System.Diagnostics.EventLog.SourceExists(ConfigurationManager.AppSettings["EventLogSource"]))
            {
                System.Diagnostics.EventLog.CreateEventSource(ConfigurationManager.AppSettings["EventLogSource"], ConfigurationManager.AppSettings["EventLog"]);

            }
            eventLog1.Source = ConfigurationManager.AppSettings["EventLogSource"];
            eventLog1.Log = ConfigurationManager.AppSettings["EventLog"];
        }

        // For fetching exited event from script and forcing service to terminate
        protected virtual void onScriptExited(object sender, EventArgs e)
        {

            eventLog1.WriteEntry("Script is no longer running, terminating service...");

            // This sends and error på event log.
            // Dirty as hell but works
            Environment.FailFast("Script no longer running, service stopped.");


        }

        protected override void OnStart(string[] args)
        {

            string ScriptPath = ConfigurationManager.AppSettings["ScriptPath"];
            string ScriptParameters = ConfigurationManager.AppSettings["ScriptParameters"];
            
            // Define process startinfo

            process.CreateNoWindow = true;
            process.UseShellExecute = false;
            process.WorkingDirectory = ConfigurationManager.AppSettings["WorkingDirectory"];
            process.RedirectStandardOutput = true;
            process.RedirectStandardError = true;
            process.FileName = "C:\\windows\\system32\\windowspowershell\\v1.0\\powershell.exe";
            process.Arguments = "-ExecutionPolicy Unrestricted -File " + ScriptPath + " " + ScriptParameters;

            // Define process error/output event handling and start it.

            Process PSProcess = new System.Diagnostics.Process();
            PSProcess.StartInfo = process;
            PSProcess.EnableRaisingEvents = true;
            PSProcess.Exited += new System.EventHandler(onScriptExited);
            
            PSProcess.OutputDataReceived += (sender, EventArgs) => eventLog1.WriteEntry(EventArgs.Data);
            PSProcess.ErrorDataReceived += (sender, EventArgs) => eventLog1.WriteEntry(EventArgs.Data);
            PSProcess.Start();

            // Begin*ReadLine must be set after process has executed.

            PSProcess.BeginOutputReadLine();
            PSProcess.BeginErrorReadLine();

            eventLog1.WriteEntry("PowershellStarter Started powershell service with scriptpath:" + ScriptPath + " and parameter: " + ScriptParameters);

            


        }
        
        protected override void OnStop()
        {
            // If script hasn't already exited, kill it
            if (!p.HasExited) {

                p.Kill();
            }

            eventLog1.WriteEntry("PowershellStarter Stopped.");

        }

        protected override void OnContinue()
        {
            eventLog1.WriteEntry("PowershellStarter does not support continue...");
        }

        private void eventLog1_EntryWritten(object sender, EntryWrittenEventArgs e)
        {

        }
    }
}
