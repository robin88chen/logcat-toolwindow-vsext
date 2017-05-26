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
    /// Interaction logic for EditFilterDialogControl.xaml
    /// </summary>
    public partial class EditFilterDialogControl : UserControl
    {
        LogcatOutputToolWindowControl ToolCtrl;
        public static Action ToClose;
        public EditFilterDialogControl(LogcatOutputToolWindowControl tool_ctrl)
        {
            InitializeComponent();
            ToolCtrl = tool_ctrl;
        }

        private void OK_OnClick(object sender, RoutedEventArgs e)
        {
            if ((FilterNameText.Text == null) || (FilterNameText.Text.Length <= 0)) return;
            int pid = 0;
            if ((FilterByPidText.Text != null) && (FilterByPidText.Text.Length > 0))
            {
                pid = System.Convert.ToInt32(FilterByPidText.Text);
            }
            LogcatOutputToolWindowControl.LogcatItem.Level level = LogcatOutputToolWindowControl.LogcatItem.Level.Verbose;
            int sel_index = FilterByLevelCombo.SelectedIndex;
            ToolCtrl.AddNewFilter(FilterNameText.Text, FilterByTagText.Text, pid,
                FilterByMsgText.Text,
                (LogcatOutputToolWindowControl.LogcatItem.Level)FilterByLevelCombo.SelectedIndex);
            ToClose?.Invoke();
        }
    }
}
