//------------------------------------------------------------------------------
// <copyright file="LogcatOutputToolWindowControl.xaml.cs" company="Company">
//     Copyright (c) Company.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

namespace LogcatToolWin
{
    using Microsoft.VisualStudio.Settings;
    using Microsoft.VisualStudio.Shell;
    using Microsoft.VisualStudio.Shell.Settings;
    using System.Diagnostics.CodeAnalysis;
    using System.IO;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Markup;
    using System.Xml;
    using System.Windows.Input;

    /// <summary>
    /// Interaction logic for LogcatOutputToolWindowControl.
    /// </summary>
    public partial class LogcatOutputToolWindowControl : UserControl
    {
        public AdbAgent adb = new AdbAgent();

        public uint LogLimitCount = 20000;

        public static string StoreCategoryName = "LogcatOutputToolSettings";
        public static string StorePropertyAdbPathName = "AdbPath";
        public static string StorePropertyLogsLimitName = "LogsLimit";

        public static RoutedCommand DeleteFilter = new RoutedCommand();
        public static RoutedCommand EditFilter = new RoutedCommand();
        public class LogcatItem
        {
            public enum Level
            {
                Verbose,
                Debug,
                Info,
                Warning,
                Error,
                Fatal
            };
            public Level LevelToken { get; set; }
            public string TimeToken { get; set; }
            public int PidToken { get; set; }
            public string TagToken { get; set; }
            public string TextToken { get; set; }
        }
        /// <summary>
        /// Initializes a new instance of the <see cref="LogcatOutputToolWindowControl"/> class.
        /// </summary>
        public LogcatOutputToolWindowControl()
        {
            this.InitializeComponent();
            this.Loaded += new RoutedEventHandler(OnLoadedHandler);
            this.Unloaded += new RoutedEventHandler(OnUnloadedHandler);

            // attach CommandBinding to root window
            this.CommandBindings.Add(new CommandBinding(
                          DeleteFilter, ExecuteDeleteFilterCommand, CanExecuteCustomCommand));
            this.CommandBindings.Add(new CommandBinding(
                          EditFilter, ExecuteEditFilterCommand, CanExecuteCustomCommand));
            //MenuItem it = FilterTemplateItem.ContextMenu.Items[0] as MenuItem;
            //it.Command = DeleteFilter;
        }

        void OnLoadedHandler(object sender, RoutedEventArgs ev)
        {
            LoadSettings();
            AdbAgent.ToOpenSettingDlg += OpenSettingDialog;
            AdbAgent.OnDeviceChecked += OnDeviceChecked;
            AdbAgent.OnOutputLogcat += OnLogcatOutput;
            adb.CheckAdbDevice();
        }

        void OnUnloadedHandler(object sender, RoutedEventArgs ev)
        {
            AdbAgent.ToOpenSettingDlg -= OpenSettingDialog;
            AdbAgent.OnOutputLogcat -= OnLogcatOutput;
            AdbAgent.OnDeviceChecked -= OnDeviceChecked;
            adb.StopAdbLogcat();
        }

        void LoadSettings()
        {
            SettingsManager settingsManager = new ShellSettingsManager(LogcatOutputToolWindowCommand.Instance.ServiceProvider);
            WritableSettingsStore configurationSettingsStore = settingsManager.GetWritableSettingsStore(SettingsScope.UserSettings);
            string adb_path = configurationSettingsStore.GetString(StoreCategoryName, StorePropertyAdbPathName, "");
            uint log_limit = configurationSettingsStore.GetUInt32(StoreCategoryName, StorePropertyLogsLimitName, 20000);
            LogLimitCount = log_limit;
            adb.AdbExePath = adb_path;
        }

        void OnDeviceChecked(string device_name, bool is_ready)
        {
            string msg = device_name;
            if (is_ready)
            {
                msg += " (Online)";
            }
            else
            {
                msg += " (Offline)";
            }
            //if ((!Dispatcher.HasShutdownStarted) && (!Dispatcher.HasShutdownFinished))
            {
                Dispatcher.InvokeAsync(() => { DeviceStateLabel.Content = msg; });
            }
            if (is_ready)
            {
                adb.StartAdbLogcat();
            }
            //Instance.DeviceStateLabel.Content = "Device"; // device_name as object;

        }

        void OnLogcatOutput(LogcatItem.Level level_token, string time_token, int pid_token,
            string tag_token, string msg_token)
        {
            Dispatcher.InvokeAsync(() =>
            {
                if (LogcatList.Items.Count > LogLimitCount)
                {
                    LogcatList.Items.RemoveAt(0);
                }
                LogcatList.Items.Add(new LogcatItem()
                {
                    LevelToken = level_token, TimeToken = time_token,
                    PidToken = pid_token, TagToken = tag_token, TextToken = msg_token
                });
            });
            //if ((!Dispatcher.HasShutdownStarted) && (!Dispatcher.HasShutdownFinished))
            {
                //Dispatcher.InvokeAsync(() => { OutputLogTextBlock.Text = msg + "\n"; });
            }
        }

        /// <summary>
        /// Handles click on the button by displaying a message box.
        /// </summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The event args.</param>
        [SuppressMessage("Microsoft.Globalization", "CA1300:SpecifyMessageBoxOptions", Justification = "Sample code")]
        [SuppressMessage("StyleCop.CSharp.NamingRules", "SA1300:ElementMustBeginWithUpperCaseLetter", Justification = "Default event handler naming pattern")]
        private void button1_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show(
                string.Format(System.Globalization.CultureInfo.CurrentUICulture, "Invoked '{0}'", this.ToString()),
                "LogcatOutputToolWindow");
        }

        private void ConnectDevice_OnClick(object sender, RoutedEventArgs e)
        {
            adb.CheckAdbDevice();
        }

        private void CancelLogcat_OnClick(object sender, RoutedEventArgs e)
        {
            adb.StopAdbLogcat();
        }

        private void ClearLogs_OnClick(object sender, RoutedEventArgs e)
        {
            Dispatcher.InvokeAsync(() =>
            {
                LogcatList.Items.Clear();
            });
        }

        private void TestFilter_OnClick(object sender, RoutedEventArgs e)
        {
            if (LogcatList.Items.Filter == null)
            {
                LogcatList.Items.Filter = (item) =>
                {
                    LogcatItem log = item as LogcatItem;
                    if (log == null) return false;
                    if (log.LevelToken == LogcatItem.Level.Debug) return true;
                    return false;
                };
            }
            else
            {
                LogcatList.Items.Filter = null;
            }
        }

        private void Setting_OnClick(object sender, RoutedEventArgs e)
        {
            OpenSettingDialog();
        }

        void OpenSettingDialog()
        {
            SettingDialog dlg = new SettingDialog(this);
            dlg.ShowModal();
        }

        private void AddFilter_OnClick(object sender, RoutedEventArgs e)
        {
            EditFilterDialog dlg = new EditFilterDialog(this);
            dlg.ShowModal();
            /*string xaml_item = XamlWriter.Save(FilterTemplateItem);
            StringReader stringReader = new StringReader(xaml_item);
            XmlReader xmlReader = XmlReader.Create(stringReader);
            CheckBox newItem = (CheckBox)XamlReader.Load(xmlReader);
            newItem.Name = "FilterClone1";
            newItem.Content = "FilterClone1";
            newItem.Visibility = Visibility.Visible;
            MenuItem it = newItem.ContextMenu.Items[0] as MenuItem;
            it.Command = DeleteFilter;
            it = newItem.ContextMenu.Items[1] as MenuItem;
            it.Command = EditFilter;
            FilterListBox.Items.Add(newItem);*/
        }

        public void AddNewFilter(string name, string tag, int pid, string text,
            LogcatItem.Level level)
        {
            string xaml_item = XamlWriter.Save(FilterTemplateItem);
            StringReader stringReader = new StringReader(xaml_item);
            XmlReader xmlReader = XmlReader.Create(stringReader);
            CheckBox newCheckbox = (CheckBox)XamlReader.Load(xmlReader);
            newCheckbox.Name = name;
            newCheckbox.Content = name;
            newCheckbox.Visibility = Visibility.Visible;
            (newCheckbox.ContextMenu.Items[0] as MenuItem).Command = DeleteFilter;
            (newCheckbox.ContextMenu.Items[1] as MenuItem).Command = EditFilter;
            newCheckbox.DataContext = new LogFilterData()
            {
                FilterName = name,
                TokenByLevel = level,
                TokenByPid = pid,
                TokenByTag = tag,
                TokenByText = text
            };
            FilterListBox.Items.Add(newCheckbox);
        }

        void ExecuteDeleteFilterCommand(object sender, ExecutedRoutedEventArgs ev)
        {
            ListBoxItem it = ev.OriginalSource as ListBoxItem;
            int index = FilterListBox.Items.IndexOf(it.Content);
            FilterListBox.Items.Remove(it.Content);
            MessageBox.Show("Delete Filter");
        }
        void ExecuteEditFilterCommand(object sender, ExecutedRoutedEventArgs ev)
        {
            MessageBox.Show("Edit Filter");
        }
        private void CanExecuteCustomCommand(object sender,
                    CanExecuteRoutedEventArgs e)
        {
            Control target = e.Source as Control;

            if (target != null)
            {
                e.CanExecute = true;
            }
            else
            {
                e.CanExecute = false;
            }
        }
    }
}