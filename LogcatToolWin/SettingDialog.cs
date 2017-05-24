using Microsoft.VisualStudio.PlatformUI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LogcatToolWin
{
    class SettingDialog : DialogWindow
    {
        public SettingDialog(LogcatOutputToolWindowControl tool_ctrl) : base()
        {
            Title = "Adb Logcat Setting";
            Height = 180;
            Width = 320;
            SettingDialogControl.ToClose = CloseDialog;
            Content = new SettingDialogControl(tool_ctrl);
        }

        void CloseDialog()
        {
            Close();
        }
    }
}
