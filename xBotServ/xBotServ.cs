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
        private const string recipient = "rickard.cronholm@skane.se";
        private Timer timer = new Timer();
        private int serviceFailCount;
        public xBotServ()
        {

            InitializeComponent();
        }

        protected override void OnStart(string[] args)
        {
            serviceFailCount = 0;
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
            DateTime now = DateTime.Now;
            // skip sundays and 00:00 - 05:00 since the VSS service shuts down, for approx 90 mins on sundays at 00:00. and reboots daily at 04.00
            if (now.DayOfWeek == DayOfWeek.Sunday || now.Hour < 5)
                return;
            // verify that the ARIADBDaemon Windows Service is running
            string serviceName = "vmsdicom_ARIADB";
            ServiceController sc = new ServiceController(serviceName);
            if (sc.Status == ServiceControllerStatus.Running)
            {
                serviceFailCount = 0;
                timer.Stop(); // Stop timer during execution of xBot.Main()
                // Logging
                Log(now.ToString("yyyy-MM-dd HH:mm:ss") + ": Taking xBot for a spin.");

                // Execute main of xBot
                try
                {
                    var task = Task.Run(() => xBot.Program.Main());
                    if (!task.Wait(TimeSpan.FromHours(1)))
                        throw new Exception("Timed out");
                    //xBot.Program.Main();
                }
                catch (Exception e)
                {

                    // send mail
                    sendMail.Program.send(recipient, "xBot reports issue", now.ToString("yyyy-MM-dd HH:mm:ss") + ": " + e.Message);
                    // stop xBot Service
                    ServiceController xBotServ = new ServiceController("xBot");
                    xBotServ.Stop();
                    OnStop();
                }

                // Logging
                now = DateTime.Now;
                Log(now.ToString("yyyy-MM-dd HH:mm:ss") + ": xBot spin completed.");
                timer.Start(); // Restart timer after execution of xBot.Main()
            }
            else
            {
                // Logging
                Log(now.ToString("yyyy-MM-dd HH:mm:ss") + ": " + serviceName + " not running, xBot not deployed.");
                // restart sc
                sc.Start();
                // send mail
                sendMail.Program.send(recipient, "xBot not running", now.ToString("yyyy-MM-dd HH:mm:ss") + ": " + serviceName + " not running - I've restarted it for you, xBot not deployed.");
                serviceFailCount += 1;
                if (serviceFailCount == 5)
                { 
                    ServiceController xBotServ = new ServiceController("xBot");
                    xBotServ.Stop();
                    OnStop();
                }

            }
        }

        protected override void OnStop()
        {
            // Logging
            DateTime now = DateTime.Now;
            Log(now.ToString("yyyy-MM-dd HH:mm:ss") + ": Service stopped");
            // send mail
            sendMail.Program.send(recipient, "xBot stopped", now.ToString("yyyy-MM-dd HH:mm:ss") + " xBot stopped.");
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
