//------------------------------------------------------------------------------
// <copyright file="LogcatOutputToolWindowControl.xaml.cs" company="Company">
//     Copyright (c) Company.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

namespace LogcatToolWin
{
    using System.Diagnostics.CodeAnalysis;
    using System.Windows;
    using System.Windows.Controls;

    /// <summary>
    /// Interaction logic for LogcatOutputToolWindowControl.
    /// </summary>
    public partial class LogcatOutputToolWindowControl : UserControl
    {
        AdbAgent adb = new AdbAgent();

        int LogLimitCount = 20000;

        class LogcatItem
        {
            public string LevelToken { get; set; }
            public string TimeToken { get; set; }
            public string PidToken { get; set; }
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
        }

        void OnLoadedHandler(object sender, RoutedEventArgs ev)
        {
            AdbAgent.OnDeviceChecked += OnDeviceChecked;
            AdbAgent.OnOutputLogcat += OnLogcatOutput;
            adb.CheckAdbDevice();
        }

        void OnUnloadedHandler(object sender, RoutedEventArgs ev)
        {
            AdbAgent.OnOutputLogcat -= OnLogcatOutput;
            AdbAgent.OnDeviceChecked -= OnDeviceChecked;
            adb.StopAdbLogcat();
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
                Dispatcher.Invoke(() => { DeviceStateLabel.Content = msg; });
            }
            if (is_ready)
            {
                adb.StartAdbLogcat();
            }
            //Instance.DeviceStateLabel.Content = "Device"; // device_name as object;

        }

        void OnLogcatOutput(string level_token, string time_token, string pid_token,
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
                    if (log.LevelToken == "D") return true;
                    return false;
                };
            }
            else
            {
                LogcatList.Items.Filter = null;
            }
        }
        
    }
}