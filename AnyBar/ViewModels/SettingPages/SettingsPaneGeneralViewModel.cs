using System;
using System.Collections.Generic;
using System.Windows;
using CommunityToolkit.Mvvm.ComponentModel;
using AnyBar.Enums;
using AnyBar.Helpers.Startup;
using AnyBar.Helpers.Windows;
using AnyBar.Interfaces;
using AnyBar.Models.Language;
using AnyBar.Models.UserSettings;
using Windows.Win32;

namespace AnyBar.ViewModels;

public partial class SettingsPaneGeneralViewModel(Settings settings, Internationalization translater) : ObservableObject, INavigationHeader
{
    private static readonly string ClassName = nameof(SettingsPaneGeneralViewModel);

    public Settings Settings { get; init; } = settings;

    #region Language

    private readonly Internationalization _translater = translater;

    public List<Language> Languages => _translater.LoadAvailableLanguages();

    public string Language
    {
        get => Settings.Language;
        set
        {
            _translater.ChangeLanguage(value);
            UpdateTranslations();
        }
    }

    private void UpdateTranslations()
    {
        WindowBackdropTypeLocalized.UpdateLabels(AllWindowBackdropTypes);
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
                App.API.ShowMsgError(Localize.App_FailedToSetAutoStartup());
                App.API.LogFatal(ClassName, $"Failed to set auto startup", e);
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
                    App.API.ShowMsgError(Localize.App_FailedToSetAutoStartup());
                    App.API.LogFatal(ClassName, $"Failed to set auto startup", e);
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
                    App.API.ShowMsgError(Localize.App_FailedToSetAutoStartup());
                    App.API.LogFatal(ClassName, $"Failed to set auto startup", e);
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

    #region Window Backdrop Type

    public List<WindowBackdropTypeLocalized> AllWindowBackdropTypes { get; } = WindowBackdropTypeLocalized.GetValues();

    public WindowBackdropType WindowBackdropType
    {
        get => Settings.WindowBackdropType;
        set
        {
            if (Settings.WindowBackdropType != value)
            {
                Settings.WindowBackdropType = value;
                UpdateWindowBackdropType(value);
            }
        }
    }

    private static void UpdateWindowBackdropType(WindowBackdropType type)
    {
        WindowBackdropHelper.SetWindowBackdrop(type, WindowTracker.GetActiveWindows());
    }

    #endregion

    #region INavigationHeader

    public string? GetHeaderKey()
    {
        return nameof(Localize.SettingWindow_General);
    }

    public string GetHeaderValue()
    {
        throw new NotImplementedException();
    }

    #endregion
}
