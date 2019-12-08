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
    using System.Collections.Generic;
    using System.Diagnostics;
    using System;
    using System.Collections.ObjectModel;
    using System.Collections.Specialized;
    using System.Windows.Media;

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
        public static string StorePropertyAutoScrollName = "AutoScroll";
        public static string StorePropertyLevelWidthName = "LevelColumnWidth";
        public static string StorePropertyTimeWidthName = "TimeColumnWidth";
        public static string StorePropertyPidWidthName = "PidColumnWidth";
        public static string StorePropertyTagWidthName = "TagColumnWidth";
        public static string StorePropertyTextWidthName = "TextColumnWidth";
        public static string StoreFilterCollectionName = "LogcatOutputToolSettings\\Filter";
        public static string StorePropertyFilterTagName = "Tag";
        public static string StorePropertyFilterLevelName = "Level";
        public static string StorePropertyFilterPidName = "Pid";
        public static string StorePropertyFilterMsgName = "Msg";
        public static string StorePropertyFilterPackageName = "Pkg";

        public static RoutedCommand DeleteFilter = new RoutedCommand();
        public static RoutedCommand EditFilter = new RoutedCommand();

        bool HasLoaded = false;
        bool IsAutoScroll = false;

        public uint[] ColumnWidth = new uint[5];

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
            public string RawContent { get; set; }
        }
        //List<LogcatItem> DeferredLogs = new List<LogcatItem>();
        ObservableCollection<LogcatItem> LogsToView;
        /// <summary>
        /// Initializes a new instance of the <see cref="LogcatOutputToolWindowControl"/> class.
        /// </summary>
        public LogcatOutputToolWindowControl()
        {
            this.Initialized += OnInitializedHandler;
            this.InitializeComponent();
            //this.Loaded += new RoutedEventHandler(OnLoadedHandler);
            //this.Unloaded += new RoutedEventHandler(OnUnloadedHandler);

            // attach CommandBinding to root window
            this.CommandBindings.Add(new CommandBinding(
                          DeleteFilter, ExecuteDeleteFilterCommand, CanExecuteCustomCommand));
            this.CommandBindings.Add(new CommandBinding(
                          EditFilter, ExecuteEditFilterCommand, CanExecuteCustomCommand));
            //MenuItem it = FilterTemplateItem.ContextMenu.Items[0] as MenuItem;
            //it.Command = DeleteFilter;
            LogsToView = new ObservableCollection<LogcatItem>();
            LogcatList.DataContext = LogsToView;
            LogsToView.CollectionChanged += new NotifyCollectionChangedEventHandler(OnCollectionChanged);
        }
        ~LogcatOutputToolWindowControl()
        {
            adb.StopAdbLogcat();
        }
        void OnInitializedHandler(object sender, EventArgs ev)
        {
            if (HasLoaded) return;
            LoadSettings();
            LoadFilterStoreData();
            AdbAgent.ToOpenSettingDlg += OpenSettingDialog;
            AdbAgent.OnDeviceChecked += OnDeviceChecked;
            AdbAgent.OnOutputLogcat += OnLogcatOutput;
            adb.CheckAdbDevice();
            //LogsToView = new ObservableCollection<LogcatItem>();
            //LogcatList.DataContext = LogsToView;
            //((INotifyCollectionChanged)LogcatList.ItemsSource).CollectionChanged += new NotifyCollectionChangedEventHandler(OnCollectionChanged);
            HasLoaded = true;
        }

        void OnLoadedHandler(object sender, RoutedEventArgs ev)
        {
            if (HasLoaded) return;
            LoadSettings();
            LoadFilterStoreData();
            AdbAgent.ToOpenSettingDlg += OpenSettingDialog;
            AdbAgent.OnDeviceChecked += OnDeviceChecked;
            AdbAgent.OnOutputLogcat += OnLogcatOutput;
            Dispatcher.InvokeAsync(() => adb.CheckAdbDevice());
            HasLoaded = true;
        }

        void OnUnloadedHandler(object sender, RoutedEventArgs ev)
        {
            AdbAgent.ToOpenSettingDlg -= OpenSettingDialog;
            AdbAgent.OnOutputLogcat -= OnLogcatOutput;
            AdbAgent.OnDeviceChecked -= OnDeviceChecked;
            /*Dispatcher.InvokeAsync(() =>
            {
                adb.StopAdbLogcat();
                ClearFilterItems();
            });*/
            HasLoaded = false;
        }

        void LoadSettings()
        {
            SettingsManager settingsManager = new ShellSettingsManager(LogcatOutputToolWindowCommand.Instance.ServiceProvider);
            WritableSettingsStore configurationSettingsStore = settingsManager.GetWritableSettingsStore(SettingsScope.UserSettings);
            string adb_path = configurationSettingsStore.GetString(StoreCategoryName, StorePropertyAdbPathName, "");
            uint log_limit = configurationSettingsStore.GetUInt32(StoreCategoryName, StorePropertyLogsLimitName, 20000);
            bool is_auto = configurationSettingsStore.GetBoolean(StoreCategoryName, StorePropertyAutoScrollName, false);
            LogLimitCount = log_limit;
            adb.AdbExePath = adb_path;
            IsAutoScroll = is_auto;
            ColumnWidth[0] = configurationSettingsStore.GetUInt32(StoreCategoryName, StorePropertyLevelWidthName, 60);
            ColumnWidth[1] = configurationSettingsStore.GetUInt32(StoreCategoryName, StorePropertyTimeWidthName, 120);
            ColumnWidth[2] = configurationSettingsStore.GetUInt32(StoreCategoryName, StorePropertyPidWidthName, 60);
            ColumnWidth[3] = configurationSettingsStore.GetUInt32(StoreCategoryName, StorePropertyTagWidthName, 120);
            ColumnWidth[4] = configurationSettingsStore.GetUInt32(StoreCategoryName, StorePropertyTextWidthName, 600);

            if (IsAutoScroll)
            {
                Dispatcher.InvokeAsync(() => { AutoScrollLabel.Content = "Auto Scroll On"; });
            }
            else
            {
                Dispatcher.InvokeAsync(() => { AutoScrollLabel.Content = "Auto Scroll Off"; });
            }
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
                Dispatcher.InvokeAsync(() => adb.StartAdbLogcat());
            }
            //Instance.DeviceStateLabel.Content = "Device"; // device_name as object;

        }

        void OnLogcatOutput(LogcatItem.Level level_token, string time_token, int pid_token,
            string tag_token, string msg_token, string raw_msg)
        {
            LogcatItem logcat_item = new LogcatItem()
            {
                LevelToken = level_token,
                TimeToken = time_token,
                PidToken = pid_token,
                TagToken = tag_token,
                TextToken = msg_token,
                RawContent = raw_msg
            };
            Dispatcher.InvokeAsync(() =>
            {
                LogsToView.Add(logcat_item);
                if (LogsToView.Count > LogLimitCount)
                {
                    LogsToView.RemoveAt(0);
                }
            });
            /*lock (DeferredLogs)
            {
                DeferredLogs.Add(new LogcatItem()
                {
                    LevelToken = level_token,
                    TimeToken = time_token,
                    PidToken = pid_token,
                    TagToken = tag_token,
                    TextToken = msg_token
                });
            }
            if (DeferredLogs.Count < 1) return;
            List<LogcatItem> collectLogs;
            lock (DeferredLogs)
            {
                collectLogs = new List<LogcatItem>(DeferredLogs);
                DeferredLogs.Clear();
            }
            Dispatcher.InvokeAsync(() =>
            {
                foreach (var log_item in collectLogs)
                {
                    LogsToView.Add(log_item);
                }
            });
                /*bool needRefresh = false;
                foreach (LogcatItem log_item in collectLogs)
                {
                    if (LogcatList.Items.Count > LogLimitCount)
                    {
                        LogcatList.Items.RemoveAt(0);
                    }
                    LogcatList.Items.Add(log_item);
                    if (LogcatList.Items.Filter != null)
                    {
                        if (!LogcatList.Items.Filter(log_item))
                        {
                            needRefresh = true;
                        }
                    }
                }
                if (needRefresh) LogcatList.Items.Refresh();*/
                /*if ((IsAutoScroll) && (LogcatList.Items.Count > 0))
                {
                    LogcatList.SelectedIndex = LogcatList.Items.Count - 1;
                    LogcatList.ScrollIntoView(LogcatList.SelectedItem);
                }
                /*if (LogcatList.Items.Count > LogLimitCount)
                {
                    LogcatList.Items.RemoveAt(0);
                }
                LogcatItem log_item = new LogcatItem()
                {
                    LevelToken = level_token,
                    TimeToken = time_token,
                    PidToken = pid_token,
                    TagToken = tag_token,
                    TextToken = msg_token
                };
                LogcatList.Items.Add(log_item);
                if (LogcatList.Items.Filter != null)
                {
                    if (!LogcatList.Items.Filter(log_item))
                    {
                        LogcatList.Items.Refresh();
                    }
                }*/
                //LogcatList.Items.Refresh();
            //});
        }

        private void OnCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (!IsAutoScroll) return;
            if (VisualTreeHelper.GetChildrenCount(LogcatList) > 0)
            {
                Decorator border = VisualTreeHelper.GetChild(LogcatList, 0) as Decorator;
                if (border != null)
                {
                    ScrollViewer scroll = border.Child as ScrollViewer;
                    if (scroll != null) scroll.ScrollToBottom();
                }
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
            if (!adb.IsDeviceReady)
            {
                adb.CheckAdbDevice();
            }
            else
            {
                adb.StartAdbLogcat();
            }
        }

        private void CancelLogcat_OnClick(object sender, RoutedEventArgs e)
        {
            adb.StopAdbLogcat();
        }

        private void ClearLogs_OnClick(object sender, RoutedEventArgs e)
        {
            Dispatcher.InvokeAsync(() =>
            {
                LogsToView.Clear();
                //LogcatList.Items.Clear();
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

        public void AddFilterItem(string name, string tag, int pid, string text, string package,
            LogcatItem.Level level, bool isNew)
        {
            string xaml_item = XamlWriter.Save(FilterTemplateItem);
            StringReader stringReader = new StringReader(xaml_item);
            XmlReader xmlReader = XmlReader.Create(stringReader);
            CheckBox newCheckbox = (CheckBox)XamlReader.Load(xmlReader);
            TextBlock tb = new TextBlock();
            tb.Text = name;
            tb.Foreground = Brushes.MediumSeaGreen;
            newCheckbox.Name = name;
            newCheckbox.Content = tb; //name;
            
            newCheckbox.Visibility = Visibility.Visible;
            (newCheckbox.ContextMenu.Items[0] as MenuItem).Command = DeleteFilter;
            (newCheckbox.ContextMenu.Items[1] as MenuItem).Command = EditFilter;
            newCheckbox.Click += new RoutedEventHandler(FilterTemplate_Clicked);
            newCheckbox.DataContext = new LogFilterData()
            {
                FilterName = name,
                TokenByLevel = level,
                TokenByPid = pid,
                TokenByTag = tag,
                TokenByText = text,
                TokenByPackage = package
            };
            FilterListBox.Items.Add(newCheckbox);

            if (isNew)
            {
                CreateFilterStoreData(name, tag, pid, text, package, level);
            }
        }
        public void ChangeFilterItem(CheckBox chk_box, string tag, int pid, string text, string package,
            LogcatItem.Level level)
        {
            LogFilterData filter_data = chk_box.DataContext as LogFilterData;
            filter_data.TokenByTag = tag;
            filter_data.TokenByPid = pid;
            filter_data.TokenByText = text;
            filter_data.TokenByPackage = package;
            filter_data.TokenByLevel = level;

            CreateFilterStoreData(chk_box.Name, tag, pid, text, package, level);
        }

        public void CreateFilterStoreData(string name, string tag, int pid, string text, string package,
            LogcatItem.Level level)
        {
            SettingsManager settingsManager = new ShellSettingsManager(LogcatOutputToolWindowCommand.Instance.ServiceProvider);
            WritableSettingsStore configurationSettingsStore = settingsManager.GetWritableSettingsStore(SettingsScope.UserSettings);
            configurationSettingsStore.CreateCollection(LogcatOutputToolWindowControl.StoreFilterCollectionName);
            string filter_sub_collection = LogcatOutputToolWindowControl.StoreFilterCollectionName
                + "\\" + name;
            configurationSettingsStore.CreateCollection(filter_sub_collection);
            configurationSettingsStore.SetString(filter_sub_collection,
                LogcatOutputToolWindowControl.StorePropertyFilterTagName, tag);
            configurationSettingsStore.SetInt32(filter_sub_collection,
                LogcatOutputToolWindowControl.StorePropertyFilterPidName, pid);
            configurationSettingsStore.SetString(filter_sub_collection,
                LogcatOutputToolWindowControl.StorePropertyFilterMsgName, text);
            configurationSettingsStore.SetString(filter_sub_collection,
                LogcatOutputToolWindowControl.StorePropertyFilterPackageName, package);
            configurationSettingsStore.SetInt32(filter_sub_collection,
                LogcatOutputToolWindowControl.StorePropertyFilterLevelName, (int)level);
        }

        public void LoadFilterStoreData()
        {
            SettingsManager settingsManager = new ShellSettingsManager(LogcatOutputToolWindowCommand.Instance.ServiceProvider);
            WritableSettingsStore settingsStore = settingsManager.GetWritableSettingsStore(SettingsScope.UserSettings);
            if (settingsStore.CollectionExists(StoreFilterCollectionName))
            {
                int count = settingsStore.GetSubCollectionCount(StoreFilterCollectionName);
                IEnumerable<string> filter_name_list = settingsStore.GetSubCollectionNames(LogcatOutputToolWindowControl.StoreFilterCollectionName);
                if (filter_name_list == null) return;
                foreach (string name in filter_name_list)
                {
                    string filter_sub_collection = StoreFilterCollectionName
                        + "\\" + name;
                    string tag = settingsStore.GetString(filter_sub_collection,
                        LogcatOutputToolWindowControl.StorePropertyFilterTagName, "");
                    int pid = settingsStore.GetInt32(filter_sub_collection,
                        LogcatOutputToolWindowControl.StorePropertyFilterPidName, 0);
                    string msg = settingsStore.GetString(filter_sub_collection,
                        LogcatOutputToolWindowControl.StorePropertyFilterMsgName, "");
                    string pkg = settingsStore.GetString(filter_sub_collection,
                        LogcatOutputToolWindowControl.StorePropertyFilterPackageName, "");
                    int level = settingsStore.GetInt32(filter_sub_collection,
                        LogcatOutputToolWindowControl.StorePropertyFilterLevelName, 0);
                    AddFilterItem(name, tag, pid, msg, pkg,
                        (LogcatOutputToolWindowControl.LogcatItem.Level)level, false);
                }
            }
        }
        public void DeleteFilterStoreData(string name)
        {
            SettingsManager settingsManager = new ShellSettingsManager(LogcatOutputToolWindowCommand.Instance.ServiceProvider);
            WritableSettingsStore settingsStore = settingsManager.GetWritableSettingsStore(SettingsScope.UserSettings);
            if (settingsStore.CollectionExists(StoreFilterCollectionName))
            {
                string filter_sub_collection = StoreFilterCollectionName
                    + "\\" + name;
                settingsStore.DeleteCollection(filter_sub_collection);
            }
        }
        void ClearFilterItems()
        {
            while (FilterListBox.Items.Count > 2)
            {
                FilterListBox.Items.RemoveAt(2);
            }
            LogcatList.Items.Filter = null;
        }
        void ExecuteDeleteFilterCommand(object sender, ExecutedRoutedEventArgs ev)
        {
            ListBoxItem it = ev.OriginalSource as ListBoxItem;
            CheckBox chk_box = it.Content as CheckBox;
            if (chk_box != null)
            {
                DeleteFilterStoreData(chk_box.Name);
            }
            int index = FilterListBox.Items.IndexOf(it.Content);
            FilterListBox.Items.Remove(it.Content);
            MessageBox.Show("Delete Filter");
        }
        void ExecuteEditFilterCommand(object sender, ExecutedRoutedEventArgs ev)
        {
            ListBoxItem it = ev.OriginalSource as ListBoxItem;
            CheckBox chk_box = it.Content as CheckBox;
            if (chk_box == null) return;
            LogFilterData filter_data = chk_box.DataContext as LogFilterData;
            if (filter_data == null) return;
            EditFilterDialog dlg = new EditFilterDialog(this);
            dlg.InitData(chk_box);
            dlg.ShowModal();
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

        private void FilterTemplate_Clicked(object sender, RoutedEventArgs e)
        {
            CheckBox chk_box = sender as CheckBox;
            LogFilterData filter_data = chk_box.DataContext as LogFilterData;
            if (chk_box.IsChecked == true)
            {
                filter_data.RetrievePackagePid(adb);
                LogcatList.Items.Filter += filter_data.IsFilterSelected;
            }
            else
            {
                LogcatList.Items.Filter -= filter_data.IsFilterSelected;
            }
        }

        private void AutoScroll_OnClick(object sender, RoutedEventArgs e)
        {
            IsAutoScroll = !IsAutoScroll;
            if (IsAutoScroll)
            {
                Dispatcher.InvokeAsync(() => { AutoScrollLabel.Content = "Auto Scroll On"; });
            }
            else
            {
                Dispatcher.InvokeAsync(() => { AutoScrollLabel.Content = "Auto Scroll Off"; });
            }
            SettingsManager settingsManager = new ShellSettingsManager(LogcatOutputToolWindowCommand.Instance.ServiceProvider);
            WritableSettingsStore configurationSettingsStore = settingsManager.GetWritableSettingsStore(SettingsScope.UserSettings);
            configurationSettingsStore.CreateCollection(LogcatOutputToolWindowControl.StoreCategoryName);
            configurationSettingsStore.SetBoolean(StoreCategoryName, StorePropertyAutoScrollName, IsAutoScroll);
        }

        public void CopyLogItem_OnClick(object sender, RoutedEventArgs e)
        {
            LogcatItem item = LogcatList.SelectedItem as LogcatItem;
            if (item == null) return;
            Clipboard.SetText(item.RawContent);
            MessageBox.Show("Logcat Copied");
        }

        public void RefreshColumnsWidth()
        {
            GridView grid = LogcatList.View as GridView;
            if (grid == null) return;
            for (int i = 0; i < 5; i++)
            {
                grid.Columns[i].Width = ColumnWidth[i];
            }
        }
    }
}