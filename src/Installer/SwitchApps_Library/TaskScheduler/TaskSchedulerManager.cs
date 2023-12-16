using System;
using System.IO;
using System.Linq;
using Microsoft.Win32.TaskScheduler;
using Serilog.Core;
using SwitchApps.Library._Helpers;

namespace SwitchApps.Library.TaskScheduler
{
    public class TaskSchedulerManager
    {
        // Init:

        private readonly Logger _logger;

        public TaskSchedulerManager(Logger logger)
        {
            this._logger = logger;
        }

        // Public methods:

        public Task CreateTask()
        {
            TaskDefinition td = TaskService.Instance.NewTask();

            td.Principal.LogonType = TaskLogonType.InteractiveToken;
            td.Principal.RunLevel = TaskRunLevel.Highest;
            td.Principal.UserId = InstallerHelper.LoginUsername;

            LogonTrigger logonTrigger = new LogonTrigger();

            logonTrigger.UserId = InstallerHelper.LoginUsername;

            td.Triggers.Add(logonTrigger);

            string pathToExe = Path.Combine(InstallerHelper.InstalledDir, "SwitchApps.exe");

            td.Actions.Add(new ExecAction(pathToExe));

            td.Settings.AllowDemandStart = true;
            td.Settings.AllowHardTerminate = true;
            td.Settings.DisallowStartIfOnBatteries = false;
            td.Settings.ExecutionTimeLimit = TimeSpan.Zero;
            td.Settings.MultipleInstances = TaskInstancesPolicy.IgnoreNew;
            td.Settings.Priority = System.Diagnostics.ProcessPriorityClass.RealTime;
            td.Settings.StopIfGoingOnBatteries = false;

            Task task = TaskService.Instance.RootFolder.CreateFolder("SwitchApps", exceptionOnExists: false)
                .RegisterTaskDefinition("SwitchApps autostart", td);

            this._logger.Information("The Task Scheduler task was created.");

            return task;
        }

        public void DeleteTask()
        {
            TaskService.Instance.RootFolder.SubFolders.Where(f => f.Name == "SwitchApps")
                .FirstOrDefault()
                .Tasks.ToList()
                .ForEach(task =>
                {
                    task.Stop();
                    task.Folder.DeleteTask(task.Name);
                });

            TaskService.Instance.RootFolder.DeleteFolder("SwitchApps");

            this._logger.Information("The Task Scheduler task and folder were deleted.");
        }
    }
}