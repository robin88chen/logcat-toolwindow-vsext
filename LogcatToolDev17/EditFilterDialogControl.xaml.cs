using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
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
    /// Interaction logic for EditFilterDialogControl.xaml
    /// </summary>
    public partial class EditFilterDialogControl : UserControl
    {
        LogcatOutputToolWindowControl ToolCtrl;
        public static Action ToClose;
        bool IsNew = true;
        CheckBox checkBox;
        public EditFilterDialogControl(LogcatOutputToolWindowControl tool_ctrl)
        {
            InitializeComponent();
            ToolCtrl = tool_ctrl;
        }

        private void OK_OnClick(object sender, RoutedEventArgs e)
        {
            if ((FilterNameText.Text == null) || (FilterNameText.Text.Length <= 0)) return;
            Regex reg = new Regex("^[A-Za-z][A-Za-z0-9]*$");
            if (!reg.IsMatch(FilterNameText.Text))
            {
                MessageBox.Show("Invalid Filter Name");
                return;
            }
            int pid = 0;
            if ((FilterByPidText.Text != null) && (FilterByPidText.Text.Length > 0))
            {
                pid = System.Convert.ToInt32(FilterByPidText.Text);
            }
            LogcatOutputToolWindowControl.LogcatItem.Level level = LogcatOutputToolWindowControl.LogcatItem.Level.Verbose;
            int sel_index = FilterByLevelCombo.SelectedIndex;
            if (IsNew)
            {
                ToolCtrl.AddFilterItem(FilterNameText.Text, FilterByTagText.Text, pid, 
                   FilterByMsgText.Text, FilterByPackageText.Text,
                    (LogcatOutputToolWindowControl.LogcatItem.Level)FilterByLevelCombo.SelectedIndex,
                    IsNew);
            }
            else
            {
                ToolCtrl.ChangeFilterItem(checkBox, FilterByTagText.Text, pid,
                   FilterByMsgText.Text, FilterByPackageText.Text,
                    (LogcatOutputToolWindowControl.LogcatItem.Level)FilterByLevelCombo.SelectedIndex);
            }
            ToClose?.Invoke();
        }

        public void InitData(CheckBox chk_box)
        {
            LogFilterData filter_data = chk_box.DataContext as LogFilterData;
            if (filter_data == null) return;
            checkBox = chk_box;
            FilterNameText.Text = chk_box.Name;
            FilterNameText.IsReadOnly = true;
            FilterByTagText.Text = filter_data.TokenByTag;
            FilterByPidText.Text = filter_data.TokenByPid.ToString();
            FilterByMsgText.Text = filter_data.TokenByText;
            FilterByLevelCombo.SelectedIndex = (int)filter_data.TokenByLevel;
            FilterByPackageText.Text = filter_data.TokenByPackage;
            IsNew = false;
        }

    }
}
