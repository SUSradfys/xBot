using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;
using System.Timers;

namespace xBotServ
{
    public partial class xBotServ : ServiceBase
    {
        private static string logFile = @"C:\Program Files\xBot\xBot.log";
        private Timer timer = new Timer();
        //private static string[] mainArgs;
        public xBotServ()
        {

            InitializeComponent();
            /*
            eventLog = new System.Diagnostics.EventLog();
            if (!System.Diagnostics.EventLog.SourceExists("MySource"))
            {
                System.Diagnostics.EventLog.CreateEventSource("MySource", "MynewLog");
            }
            eventLog.Source = "MySource";
            eventLog.Log = "MyNewLog";
            */
        }

        protected override void OnStart(string[] args)
        {
            // Logging
            DateTime now = DateTime.Now;
            //eventLog.WriteEntry(now.ToString("yyyy-MM-dd HH:mm:ss") + ": Service started");
            Log(now.ToString("yyyy-MM-dd HH:mm:ss") + ": Service started");

            // Set up a timer to trigger every 5 minutes
            int delay = 5; // 5 minutes
            timer.Interval = delay * 60 * 1000; // 5 minutes expressed in ms
            timer.Elapsed += new System.Timers.ElapsedEventHandler(this.OnTimer);
            timer.Start();
        }

        public void OnTimer(object sender, System.Timers.ElapsedEventArgs args)
        {
            timer.Stop(); // Stop timer during execution of xBot.Main()
            // Logging
            DateTime now = DateTime.Now;
            //eventLog.WriteEntry(now.ToString("yyyy-MM-dd HH:mm:ss") + ": Taking xBot for a spin.", EventLogEntryType.Information);
            Log(now.ToString("yyyy-MM-dd HH:mm:ss") + ": Taking xBot for a spin.");

            // Execute main of xBot
            xBot.Program.Main();

            // Logging
            now = DateTime.Now;
            //eventLog.WriteEntry(now.ToString("yyyy-MM-dd HH:mm:ss") + ": xBot spin completed.", EventLogEntryType.Information);
            Log(now.ToString("yyyy-MM-dd HH:mm:ss") + ": xBot spin completed.");
            timer.Start(); // Restart timer after execution of xBot.Main()
        }

        protected override void OnStop()
        {
            // Logging
            DateTime now = DateTime.Now;
            //eventLog.WriteEntry(now.ToString("yyyy-MM-dd HH:mm:ss") + ": Service Stopped");
            Log(now.ToString("yyyy-MM-dd HH:mm:ss") + ": Service stopped");
        }

        private void Log(string log)
        {
            using (System.IO.StreamWriter file =
                System.IO.File.AppendText(logFile))
            {
                file.WriteLine(log);
            }
        }
    }
}
