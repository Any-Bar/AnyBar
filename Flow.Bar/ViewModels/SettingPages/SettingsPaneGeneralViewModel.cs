using System;
using System.Collections.Generic;
using System.Windows;
using CommunityToolkit.Mvvm.ComponentModel;
using Flow.Bar.Helpers.Startup;
using Flow.Bar.Models.Language;
using Flow.Bar.Models.UserSettings;
using Windows.Win32;

namespace Flow.Bar.ViewModels;

public partial class SettingsPaneGeneralViewModel(Settings settings, Internationalization translater) : ObservableObject
{
    public Settings Settings { get; init; } = settings;

    #region Language

    private readonly Internationalization _translater = translater;

    public List<Language> Languages => _translater.LoadAvailableLanguages();

    public string Language
    {
        get => Settings.Language;
        set => _translater.ChangeLanguage(value);
    }

    #endregion

    #region Behaviour

    public bool StartOnSystemStartup
    {
        get => Settings.StartOnSystemStartup;
        set
        {
            if (StartOnSystemStartup == value) return;

            Settings.StartOnSystemStartup = value;

            try
            {
                if (value)
                {
                    if (UseLogonTaskForStartup)
                    {
                        AutoStartupHelper.ChangeToViaLogonTask(AlwaysRunAsAdministrator);
                    }
                    else
                    {
                        AutoStartupHelper.ChangeToViaRegistry();
                    }
                }
                else
                {
                    AutoStartupHelper.DisableViaLogonTaskAndRegistry();
                }
            }
            catch (Exception e)
            {
                App.API.ShowMsg(Localize.App_FailedToSetAutoStartup(), e.Message);
            }

            // If we have enabled logon task startup, we need to check if we need to restart the app
            // even if we encounter an error while setting the startup method
            if (value && UseLogonTaskForStartup)
            {
                CheckAdminChangeAndAskForRestart();
            }
        }
    }

    public bool UseLogonTaskForStartup
    {
        get => Settings.UseLogonTaskForStartup;
        set
        {
            if (UseLogonTaskForStartup == value) return;

            Settings.UseLogonTaskForStartup = value;

            if (StartOnSystemStartup)
            {
                try
                {
                    if (value)
                    {
                        AutoStartupHelper.ChangeToViaLogonTask(AlwaysRunAsAdministrator);
                    }
                    else
                    {
                        AutoStartupHelper.ChangeToViaRegistry();
                    }
                }
                catch (Exception e)
                {
                    App.API.ShowMsg(Localize.App_FailedToSetAutoStartup(), e.Message);
                }
            }

            // If we have enabled logon task startup, we need to check if we need to restart the app
            // even if we encounter an error while setting the startup method
            if (StartOnSystemStartup && value)
            {
                CheckAdminChangeAndAskForRestart();
            }
        }
    }

    public bool AlwaysRunAsAdministrator
    {
        get => Settings.AlwaysRunAsAdministrator;
        set
        {
            if (AlwaysRunAsAdministrator == value) return;

            Settings.AlwaysRunAsAdministrator = value;

            if (StartOnSystemStartup && UseLogonTaskForStartup)
            {
                try
                {
                    AutoStartupHelper.ChangeToViaLogonTask(value);
                }
                catch (Exception e)
                {
                    App.API.ShowMsg(Localize.App_FailedToSetAutoStartup(), e.Message);
                }

                // If we have enabled logon task startup, we need to check if we need to restart the app
                // even if we encounter an error while setting the startup method
                CheckAdminChangeAndAskForRestart();
            }
        }
    }

    private void CheckAdminChangeAndAskForRestart()
    {
        // When we change from non-admin to admin, we need to restart the app as administrator to apply the changes
        // Under non-administrator, we cannot delete or set the logon task which is run as administrator
        if (AlwaysRunAsAdministrator && !PInvokeHelper.IsAdministrator())
        {
            if (App.API.ShowMsgBox(
                Localize.SettingPaneGeneral_RestartToApplyChangeDescription(),
                Localize.SettingPaneGeneral_RestartToApplyChange(),
                MessageBoxButton.YesNo) == MessageBoxResult.Yes)
            {
                // Restart the app as administrator
                App.API.RestartApp(true);
            }
        }
    }

    #endregion
}
