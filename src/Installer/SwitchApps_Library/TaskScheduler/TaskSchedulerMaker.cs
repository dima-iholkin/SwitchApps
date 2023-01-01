using System;
using System.IO;
using System.Linq;
using Microsoft.Win32.TaskScheduler;
using SwitchApps.Library._Helpers;



namespace SwitchApps.Library.TaskScheduler
{


    public class TaskSchedulerMaker
    {
        public TaskSchedulerMaker() { }



        public Task CreateTask()
        {
            TaskDefinition td = TaskService.Instance.NewTask();

            td.Principal.UserId = InstallerHelper.LoginUsername;
            td.Principal.LogonType = TaskLogonType.InteractiveToken;
            td.Principal.RunLevel = TaskRunLevel.Highest;

            LogonTrigger logonTrigger = new LogonTrigger();
            logonTrigger.UserId = InstallerHelper.LoginUsername;
            td.Triggers.Add(logonTrigger);

            var pathToExe = Path.Combine(InstallerHelper.InstalledDir, "SwitchApps.exe");
            td.Actions.Add(new ExecAction(pathToExe));
            td.Settings.Priority = System.Diagnostics.ProcessPriorityClass.RealTime;
            td.Settings.AllowDemandStart = true;
            td.Settings.AllowHardTerminate = true;
            td.Settings.DisallowStartIfOnBatteries = false;
            td.Settings.StopIfGoingOnBatteries = false;
            td.Settings.ExecutionTimeLimit = TimeSpan.Zero;
            td.Settings.MultipleInstances = TaskInstancesPolicy.IgnoreNew;

            var task = TaskService.Instance.RootFolder
                .CreateFolder("SwitchApps", exceptionOnExists: false)
                .RegisterTaskDefinition("SwitchApps autostart", td);

            return task;
        }



        public void DeleteTask()
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