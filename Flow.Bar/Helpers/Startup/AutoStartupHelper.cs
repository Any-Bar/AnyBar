using System;
using System.Linq;
using System.Security.Principal;
using Microsoft.Win32;
using Microsoft.Win32.TaskScheduler;
using Windows.Win32;
using LogonTaskAction = Microsoft.Win32.TaskScheduler.Action;

namespace Flow.Bar.Helpers.Startup;

public class AutoStartupHelper
{
    private static readonly string ClassName = nameof(AutoStartupHelper);

    private const string StartupPath = @"SOFTWARE\Microsoft\Windows\CurrentVersion\Run";
    private const string LogonTaskName = $"{Constants.FlowBar} Startup";
    private const string LogonTaskDesc = $"Automatically start {Constants.FlowBar} on system startup";

    public static void CheckIsEnabled(bool useLogonTaskForStartup, bool alwaysRunAsAdministrator)
    {
        var logonTaskEnabled = CheckLogonTask(alwaysRunAsAdministrator);
        var registryEnabled = CheckRegistry();
        if (useLogonTaskForStartup)
        {
            // Enable logon task
            if (!logonTaskEnabled)
            {
                Enable(true, alwaysRunAsAdministrator);
            }
            // Disable registry
            if (registryEnabled)
            {
                Disable(false);
            }
        }
        else
        {
            // Enable registry
            if (!registryEnabled)
            {
                Enable(false, alwaysRunAsAdministrator);
            }
            // Disable logon task
            if (logonTaskEnabled)
            {
                Disable(true);
            }
        }
    }

    private static bool CheckLogonTask(bool alwaysRunAsAdministrator)
    {
        using var taskService = new TaskService();
        var task = taskService.RootFolder.AllTasks.FirstOrDefault(t => t.Name == LogonTaskName);
        if (task != null)
        {
            try
            {
                if (task.Definition.Actions.FirstOrDefault() is LogonTaskAction taskAction)
                {
                    var action = taskAction.ToString().Trim();
                    var pathCorrect = action.Equals(Constants.ExecutablePath, StringComparison.OrdinalIgnoreCase);
                    var runLevelCorrect = CheckRunLevel(task.Definition.Principal.RunLevel, alwaysRunAsAdministrator);

                    if (PInvokeHelper.IsAdministrator())
                    {
                        // If path or run level is not correct, we need to unschedule and reschedule the task
                        if (!pathCorrect || !runLevelCorrect)
                        {
                            UnscheduleLogonTask();
                            ScheduleLogonTask(alwaysRunAsAdministrator);
                        }
                    }
                    else
                    {
                        // If run level is not correct, we cannot edit it because we are not administrator
                        // So we just throw an exception to let the user know
                        if (!runLevelCorrect)
                        {
                            throw new UnauthorizedAccessException("Cannot edit task run level because the app is not running as administrator.");
                        }

                        // If run level is correct and path is not correct, we need to unschedule and reschedule the task
                        if (!pathCorrect)
                        {
                            UnscheduleLogonTask();
                            ScheduleLogonTask(alwaysRunAsAdministrator);
                        }
                    }
                }

                return true;
            }
            catch (UnauthorizedAccessException e)
            {
                App.API.LogError(ClassName, $"Failed to check logon task: {e}");
                throw; // Throw exception so that App.AutoStartup can show error message
            }
            catch (Exception e)
            {
                App.API.LogError(ClassName, $"Failed to check logon task: {e}");
                throw; // Throw exception so that App.AutoStartup can show error message
            }
        }

        return false;
    }

    private static bool CheckRunLevel(TaskRunLevel rl, bool alwaysRunAsAdministrator)
    {
        return alwaysRunAsAdministrator ? rl == TaskRunLevel.Highest : rl != TaskRunLevel.Highest;
    }

    private static bool CheckRegistry()
    {
        try
        {
            using var key = Registry.CurrentUser.OpenSubKey(StartupPath, true);
            if (key != null)
            {
                // Check if the action is the same as the current executable path
                // If not, we need to unschedule and reschedule the task
                var action = (key.GetValue(Constants.FlowBar) as string) ?? string.Empty;
                if (!action.Equals(Constants.ExecutablePath, StringComparison.OrdinalIgnoreCase))
                {
                    UnscheduleRegistry();
                    ScheduleRegistry();
                }

                return true;
            }

            return false;
        }
        catch (Exception e)
        {
            App.API.LogError(ClassName, $"Failed to check registry: {e}");
            throw; // Throw exception so that App.AutoStartup can show error message
        }
    }

    public static void DisableViaLogonTaskAndRegistry()
    {
        Disable(true);
        Disable(false);
    }

    public static void ChangeToViaLogonTask(bool alwaysRunAsAdministrator)
    {
        Disable(false);
        Disable(true); // Remove old logon task so that we can create a new one
        Enable(true, alwaysRunAsAdministrator);
    }

    public static void ChangeToViaRegistry()
    {
        Disable(true);
        Disable(false); // Remove old registry so that we can create a new one
        // We do not need to use alwaysRunAsAdministrator for registry, so we just set false here
        Enable(false, false);
    }

    private static void Disable(bool logonTask)
    {
        try
        {
            if (logonTask)
            {
                UnscheduleLogonTask();
            }
            else
            {
                UnscheduleRegistry();
            }
        }
        catch (Exception e)
        {
            App.API.LogError(ClassName, $"Failed to disable auto-startup: {e}");
            throw;
        }
    }

    private static void Enable(bool logonTask, bool alwaysRunAsAdministrator)
    {
        try
        {
            if (logonTask)
            {
                ScheduleLogonTask(alwaysRunAsAdministrator);
            }
            else
            {
                ScheduleRegistry();
            }
        }
        catch (Exception e)
        {
            App.API.LogError(ClassName, $"Failed to enable auto-startup: {e}");
            throw;
        }
    }

    private static bool ScheduleLogonTask(bool alwaysRunAsAdministrator)
    {
        using var td = TaskService.Instance.NewTask();
        td.RegistrationInfo.Description = LogonTaskDesc;
        td.Triggers.Add(new LogonTrigger { UserId = WindowsIdentity.GetCurrent().Name, Delay = TimeSpan.FromSeconds(2) });
        td.Actions.Add(Constants.ExecutablePath);

        // Only if the app is running as administrator, we can set the run level to highest
        if (PInvokeHelper.IsAdministrator() && alwaysRunAsAdministrator)
        {
            td.Principal.RunLevel = TaskRunLevel.Highest;
        }

        td.Settings.StopIfGoingOnBatteries = false;
        td.Settings.DisallowStartIfOnBatteries = false;
        td.Settings.ExecutionTimeLimit = TimeSpan.Zero;

        try
        {
            TaskService.Instance.RootFolder.RegisterTaskDefinition(LogonTaskName, td);
            return true;
        }
        catch (Exception e)
        {
            App.API.LogError(ClassName, $"Failed to schedule logon task: {e}");
            return false;
        }
    }

    private static bool UnscheduleLogonTask()
    {
        using var taskService = new TaskService();
        try
        {
            taskService.RootFolder.DeleteTask(LogonTaskName);
            return true;
        }
        catch (Exception e)
        {
            App.API.LogError(ClassName, $"Failed to unschedule logon task: {e}");
            return false;
        }
    }

    private static bool UnscheduleRegistry()
    {
        using var key = Registry.CurrentUser.OpenSubKey(StartupPath, true);
        key?.DeleteValue(Constants.FlowBar, false);
        return true;
    }

    private static bool ScheduleRegistry()
    {
        using var key = Registry.CurrentUser.OpenSubKey(StartupPath, true);
        key?.SetValue(Constants.FlowBar, $"\"{Constants.ExecutablePath}\"");
        return true;
    }
}
