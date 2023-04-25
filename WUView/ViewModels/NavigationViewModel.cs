﻿// Copyright (c) Tim Kennedy. All Rights Reserved. Licensed under the MIT License.

namespace WUView.ViewModels;

public partial class NavigationViewModel : ObservableObject
{
    #region MainWindow Instance
    private static readonly MainWindow _mainWindow = Application.Current.MainWindow as MainWindow;
    #endregion MainWindow Instance

    #region Constructor
    public NavigationViewModel()
    {
        if (CurrentViewModel == null)
        {
            NavigateToPage(NavPage.Viewer);
        }
    }
    #endregion Constructor

    #region Properties
    [ObservableProperty]
    private object _currentViewModel;

    [ObservableProperty]
    private string _pageTitle;
    #endregion Properties

    #region List of navigation items
    public static List<NavigationItem> NavigationViewModelTypes { get; set; } = new List<NavigationItem>
        (new List<NavigationItem>
            {
                new NavigationItem
                {
                    Name="Updates",
                    NavPage = NavPage.Viewer,
                    ViewModelType= typeof(MainViewModel),
                    IconKind=PackIconKind.ViewList,
                    PageTitle="Windows Update Viewer"
                },
                 new NavigationItem
                {
                    Name="Settings",
                    NavPage=NavPage.Settings,
                    ViewModelType= typeof(SettingsViewModel),
                    IconKind=PackIconKind.SettingsOutline,
                    PageTitle = "Settings"
                },
                new NavigationItem
                {
                    Name="About",
                    NavPage=NavPage.About,
                    ViewModelType= typeof(AboutViewModel),
                    IconKind=PackIconKind.AboutCircleOutline,
                    PageTitle = "About Windows Update Viewer"
                },
                new NavigationItem
                {
                    Name="Exit",
                    IconKind=PackIconKind.ExitToApp,
                    IsExit=true
                }
            }
        );
    #endregion List of navigation items

    #region Navigation Methods
    public void NavigateToPage(NavPage page)
    {
        Navigate(FindNavPage(page));
    }

    private static NavigationItem FindNavPage(NavPage page)
    {
        return NavigationViewModelTypes.Find(x => x.NavPage == page);
    }
    #endregion Navigation Methods

    #region Relay Commands
    // Keep in mind that the community toolkit will add "Command" to the end of the method name.

    #region Navigate Command
    [RelayCommand]
    internal void Navigate(object param)
    {
        if (param is NavigationItem item)
        {
            if (item.IsExit)
            {
                Application.Current.Shutdown();
            }
            if (item.ViewModelType is not null)
            {
                PageTitle = item.PageTitle;
                CurrentViewModel = null;
                CurrentViewModel = Activator.CreateInstance((Type)item.ViewModelType);
                if (_mainWindow.NavDrawer != null)
                {
                    _mainWindow.NavDrawer.IsLeftDrawerOpen = false;
                }
            }
        }
    }
    #endregion Navigate Command

    #region Exit the exclude file
    [RelayCommand]
    public static void EditExclude()
    {
        if (Keyboard.Modifiers == ModifierKeys.Shift)
        {
            TextFileViewer.ViewTextFile(FileHelpers.GetExcludesFile());
        }
        else
        {
            MainViewModel.EditExcludes();
        }
    }
    #endregion Exit the exclude file

    #region Open the About page
    [RelayCommand]
    public void OpenAbout()
    {
        NavigateToPage(NavPage.About);
    }
    #endregion Open the About page

    #region Open the Settings page
    [RelayCommand]
    public void OpenSettings()
    {
        NavigateToPage(NavPage.Settings);
    }
    #endregion Open the Settings page

    #region View log file
    [RelayCommand]
    public static void ViewLogFile()
    {
        SnackbarMsg.ClearAndQueueMessage("Opening the Log File", 2000);
        TextFileViewer.ViewTextFile(NLogHelpers.GetLogfileName());
    }
    #endregion View log file

    #region View readme file
    [RelayCommand]
    public static void ViewReadMeFile()
    {
        SnackbarMsg.ClearAndQueueMessage("Opening the ReadMe Document", 2000);
        TextFileViewer.ViewTextFile(Path.Combine(AppInfo.AppDirectory, "readme.txt"));
    }
    #endregion View readme file

    #region Toggle details
    [RelayCommand]
    public static void ToggleDetails()
    {
        UserSettings.Setting.ShowDetails = !UserSettings.Setting.ShowDetails;
    }
    #endregion Toggle details

    #region Toggle excluded
    [RelayCommand]
    public static void ToggleExcluded()
    {
        UserSettings.Setting.HideExcluded = !UserSettings.Setting.HideExcluded;
    }
    #endregion Toggle excluded

    #region Remove column sort
    [RelayCommand]
    public static void RemoveSort()
    {
        MainPage.Instance.ClearColumnSort();
    }
    #endregion Remove column sort

    #region UI Smaller and Larger
    [RelayCommand]
    public static void UILarger()
    {
        MainWindowUIHelpers.EverythingLarger();
        SnackbarMsg.ClearAndQueueMessage($"Size set to {UserSettings.Setting.UISize}");
    }

    [RelayCommand]
    public static void UISmaller()
    {
        MainWindowUIHelpers.EverythingSmaller();
        SnackbarMsg.ClearAndQueueMessage($"Size set to {UserSettings.Setting.UISize}");
    }
    #endregion UI Smaller and Larger

    #region Application Shutdown
    [RelayCommand]
    public static void Exit()
    {
        Application.Current.Shutdown();
    }
    #endregion Application Shutdown

    #region Launch Windows Update
    [RelayCommand]
    public static void OpenWindowsUpdate()
    {
        SnackbarMsg.ClearAndQueueMessage("Opening Windows Update");
        using Process procWU = new();
        procWU.StartInfo.FileName = "ms-settings:windowsupdate";
        procWU.StartInfo.UseShellExecute = true;
        _ = procWU.Start();
    }
    #endregion Launch Windows Update

    #region Save to CSV file
    [RelayCommand]
    public static void SaveCSV()
    {
        FileHelpers.SaveToCSV();
    }
    #endregion Save to CSV file

    #region Save details to text file
    [RelayCommand]
    public static void SaveText()
    {
        FileHelpers.SaveToFile();
    }
    #endregion Save details to text file

    #region Copy to clipboard
    [RelayCommand]
    internal static void CopyClipboard()
    {
        MainPage.Instance.Copy2Clipboard(true);
    }
    #endregion Copy to clipboard

    #endregion Relay Commands

    #region Key down events
    /// <summary>
    /// Keyboard events
    /// </summary>
    public void KeyDown(object sender, KeyEventArgs e)
    {
        #region Keys without modifiers
        if (e.Key == Key.F1)
        {
            _mainWindow.NavigationListBox.SelectedValue = FindNavPage(NavPage.About);
        }
        #endregion Keys without modifiers

        #region Keys with Ctrl
        if (e.KeyboardDevice.Modifiers == ModifierKeys.Control)
        {
            if (e.Key == Key.M)
            {
                _mainWindow.NavDrawer.IsLeftDrawerOpen = !_mainWindow.NavDrawer.IsLeftDrawerOpen;
            }
            if (e.Key == Key.OemComma)
            {
                _mainWindow.NavigationListBox.SelectedValue = FindNavPage(NavPage.Settings);
            }
            if (e.Key == Key.U)
            {
                _mainWindow.NavigationListBox.SelectedValue = FindNavPage(NavPage.Viewer);
            }
            if (e.Key == Key.L)
            {
                MainViewModel.EditExcludes();
            }
            if (e.Key == Key.D)
            {
                UserSettings.Setting.ShowDetails = !UserSettings.Setting.ShowDetails;
            }
            if (e.Key == Key.E)
            {
                UserSettings.Setting.HideExcluded = !UserSettings.Setting.HideExcluded;
            }
            if (e.Key == Key.R)
            {
                MainPage.Instance.ClearColumnSort();
            }
            if (e.Key == Key.T)
            {
                if (UserSettings.Setting.DateFormat >= 4)
                {
                    UserSettings.Setting.DateFormat = 0;
                }
                else
                {
                    UserSettings.Setting.DateFormat++;
                }
                MainPage.Instance.UpdateGrid();
                SnackbarMsg.ClearAndQueueMessage("Date format changed", 2000);
            }
            if (e.Key == Key.Add)
            {
                MainWindowUIHelpers.EverythingLarger();
                SnackbarMsg.ClearAndQueueMessage($"Size set to {UserSettings.Setting.UISize}", 2000);
            }
            if (e.Key == Key.Subtract)
            {
                MainWindowUIHelpers.EverythingSmaller();
                SnackbarMsg.ClearAndQueueMessage($"Size set to {UserSettings.Setting.UISize}", 2000);
            }
        }
        #endregion Keys with Ctrl

        #region Keys with Ctrl and Shift
        if (e.KeyboardDevice.Modifiers == (ModifierKeys.Control | ModifierKeys.Shift))
        {
            if (e.Key == Key.T)
            {
                switch (UserSettings.Setting.UITheme)
                {
                    case ThemeType.Light:
                        UserSettings.Setting.UITheme = ThemeType.Dark;
                        break;
                    case ThemeType.Dark:
                        UserSettings.Setting.UITheme = ThemeType.Darker;
                        break;
                    case ThemeType.Darker:
                        UserSettings.Setting.UITheme = ThemeType.System;
                        break;
                    case ThemeType.System:
                        UserSettings.Setting.UITheme = ThemeType.Light;
                        break;
                }
                string theme = Converters.EnumDescConverter.GetEnumDescription(UserSettings.Setting.UITheme);
                SnackbarMsg.ClearAndQueueMessage($"Theme set to {theme}", 2000);
            }
            if (e.Key == Key.C)
            {
                if (UserSettings.Setting.PrimaryColor >= AccentColor.White)
                {
                    UserSettings.Setting.PrimaryColor = AccentColor.Red;
                }
                else
                {
                    UserSettings.Setting.PrimaryColor++;
                }
                string color = Converters.EnumDescConverter.GetEnumDescription(UserSettings.Setting.PrimaryColor);
                SnackbarMsg.ClearAndQueueMessage($"Accent color set to {color}");
            }
            if (e.Key == Key.S)
            {
                TextFileViewer.ViewTextFile(ConfigHelpers.SettingsFileName);
                SnackbarMsg.ClearAndQueueMessage("Opening settings file", 2000);
            }
        }
        #endregion
    }
    #endregion Key down events
}
