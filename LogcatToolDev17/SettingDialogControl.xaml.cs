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

namespace LogcatToolDev17
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
            LevelWidthText.Text = ToolCtrl.ColumnWidth[0].ToString();
            TimeWidthText.Text = ToolCtrl.ColumnWidth[1].ToString();
            PIDWidthText.Text = ToolCtrl.ColumnWidth[2].ToString();
            TagWidthText.Text = ToolCtrl.ColumnWidth[3].ToString();
            TextWidthText.Text = ToolCtrl.ColumnWidth[4].ToString();
        }

        private void OK_OnClick(object sender, RoutedEventArgs e)
        {
            SettingsManager settingsManager = new ShellSettingsManager(LogcatOutputToolWindowCommand.Instance.ServiceProvider);
            WritableSettingsStore configurationSettingsStore = settingsManager.GetWritableSettingsStore(SettingsScope.UserSettings);
            configurationSettingsStore.CreateCollection(LogcatOutputToolWindowControl.StoreCategoryName);
            if (AdbPathText.Text.Length == 0)
            {
                configurationSettingsStore.DeleteProperty(LogcatOutputToolWindowControl.StoreCategoryName,
                    LogcatOutputToolWindowControl.StorePropertyAdbPathName);
            }
            else
            {
                configurationSettingsStore.SetString(LogcatOutputToolWindowControl.StoreCategoryName,
                    LogcatOutputToolWindowControl.StorePropertyAdbPathName, AdbPathText.Text);
            }
            uint log_limit = System.Convert.ToUInt32(LogLimitText.Text);
            configurationSettingsStore.SetUInt32(LogcatOutputToolWindowControl.StoreCategoryName,
                LogcatOutputToolWindowControl.StorePropertyLogsLimitName, log_limit);
            ToolCtrl.LogLimitCount = log_limit;
            ToolCtrl.adb.AdbExePath = AdbPathText.Text;

            uint level_width = Convert.ToUInt32(LevelWidthText.Text);
            configurationSettingsStore.SetUInt32(LogcatOutputToolWindowControl.StoreCategoryName,
                LogcatOutputToolWindowControl.StorePropertyLevelWidthName, level_width);
            uint time_width = Convert.ToUInt32(TimeWidthText.Text);
            configurationSettingsStore.SetUInt32(LogcatOutputToolWindowControl.StoreCategoryName,
                LogcatOutputToolWindowControl.StorePropertyTimeWidthName, time_width);
            uint pid_width = Convert.ToUInt32(PIDWidthText.Text);
            configurationSettingsStore.SetUInt32(LogcatOutputToolWindowControl.StoreCategoryName,
                LogcatOutputToolWindowControl.StorePropertyPidWidthName, pid_width);
            uint tag_width = Convert.ToUInt32(TagWidthText.Text);
            configurationSettingsStore.SetUInt32(LogcatOutputToolWindowControl.StoreCategoryName,
                LogcatOutputToolWindowControl.StorePropertyTagWidthName, tag_width);
            uint text_width = Convert.ToUInt32(TextWidthText.Text);
            configurationSettingsStore.SetUInt32(LogcatOutputToolWindowControl.StoreCategoryName,
                LogcatOutputToolWindowControl.StorePropertyTextWidthName, text_width);

            ToolCtrl.ColumnWidth[0] = level_width;
            ToolCtrl.ColumnWidth[1] = time_width;
            ToolCtrl.ColumnWidth[2] = pid_width;
            ToolCtrl.ColumnWidth[3] = tag_width;
            ToolCtrl.ColumnWidth[4] = text_width;
            ToolCtrl.RefreshColumnsWidth();

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
