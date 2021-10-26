using System;
using System.IO;
using System.Linq;
using Microsoft.Win32.TaskScheduler;
using Serilog.Core;

namespace SwitchApps_Library
{


    public class TaskSchedulerMaker
    {
        private readonly string _installedDir;
        private readonly string _loginUsername;

        private readonly Logger _logger;

        public TaskSchedulerMaker(
            string installedDir,
            string loginUsername,
            Logger logger
        )
        {
            _installedDir = installedDir;
            _loginUsername = loginUsername;
            _logger = logger;
        }



        public void CreateTaskSchedulerTaskAndRun()
        {
            TaskDefinition td = TaskService.Instance.NewTask();

            td.Principal.UserId = _loginUsername;
            td.Principal.LogonType = TaskLogonType.InteractiveToken;
            td.Principal.RunLevel = TaskRunLevel.Highest;

            LogonTrigger logonTrigger = new LogonTrigger();
            logonTrigger.UserId = _loginUsername;
            td.Triggers.Add(logonTrigger);

            var pathToExe = Path.Combine(_installedDir, "SwitchApps.exe");
            td.Actions.Add(new ExecAction(pathToExe));
            td.Settings.Priority = System.Diagnostics.ProcessPriorityClass.RealTime;
            td.Settings.AllowDemandStart = true;
            td.Settings.AllowHardTerminate = true;
            td.Settings.DisallowStartIfOnBatteries = false;
            td.Settings.ExecutionTimeLimit = TimeSpan.Zero;
            td.Settings.MultipleInstances = TaskInstancesPolicy.IgnoreNew;

            var task = TaskService.Instance.RootFolder
                .CreateFolder("SwitchApps", exceptionOnExists: false)
                .RegisterTaskDefinition("SwitchApps autostart", td);

            task.Run();
        }



        public void DeleteTaskSchedulerTask()
        {
            TaskService.Instance.RootFolder
                .SubFolders.Where(f => f.Name == "SwitchApps")
                .FirstOrDefault().Tasks.ToList().ForEach(task =>
                {
                    task.Stop();
                    task.Folder.DeleteTask(task.Name);
                });

            TaskService.Instance.RootFolder.DeleteFolder("SwitchApps");
        }
    }
}