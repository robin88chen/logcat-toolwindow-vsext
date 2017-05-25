using Microsoft.VisualStudio.Settings;
using Microsoft.VisualStudio.Shell.Settings;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace LogcatToolWin
{
    /// <summary>
    /// Interaction logic for SettingDialogControl.xaml
    /// </summary>
    public partial class SettingDialogControl : UserControl
    {
        public static Action ToClose;
        LogcatOutputToolWindowControl ToolCtrl;
        public SettingDialogControl(LogcatOutputToolWindowControl tool_ctrl)
        {
            InitializeComponent();
            ToolCtrl = tool_ctrl;
        }

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
            Dispatcher.InvokeAsync(() =>
            {
                LogLimitText.Text = ToolCtrl.LogLimitCount.ToString();
                AdbPathText.Text = ToolCtrl.adb.AdbExePath;
            });
        }

        private void OK_OnClick(object sender, RoutedEventArgs e)
        {
            SettingsManager settingsManager = new ShellSettingsManager(LogcatOutputToolWindowCommand.Instance.ServiceProvider);
            WritableSettingsStore configurationSettingsStore = settingsManager.GetWritableSettingsStore(SettingsScope.UserSettings);
            configurationSettingsStore.CreateCollection("LogcatSettings");
            if (AdbPathText.Text.Length == 0)
            {
                configurationSettingsStore.DeleteProperty("LogcatSettings", "AdbPath");
            }
            else
            {
                configurationSettingsStore.SetString("LogcatSettings", "AdbPath", AdbPathText.Text);
            }
            uint log_limit = System.Convert.ToUInt32(LogLimitText.Text);
            configurationSettingsStore.SetUInt32("LogcatSettings", "LogsLimit", log_limit);
            ToolCtrl.LogLimitCount = log_limit;
            ToolCtrl.adb.AdbExePath = AdbPathText.Text;
            ToClose?.Invoke();
        }

        private void Cancel_OnClick(object sender, RoutedEventArgs e)
        {

        }

        private void Browse_OnClick(object sender, RoutedEventArgs e)
        {
            OpenFileDialog dlg = new OpenFileDialog();
            dlg.DefaultExt = "exe";
            dlg.Filter = "Adb Exe|adb.exe";
            bool? file_ok = dlg.ShowDialog();
            if (file_ok == true)
            {
                Dispatcher.InvokeAsync(() => AdbPathText.Text = dlg.FileName);
            }
        }
    }
}
