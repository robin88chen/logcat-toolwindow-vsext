using Microsoft.VisualStudio.PlatformUI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace LogcatToolWin
{
    class EditFilterDialog : DialogWindow
    {
        public EditFilterDialog(LogcatOutputToolWindowControl tool_ctrl) : base()
        {
            Title = "Add New Filter";
            Height = 370;
            Width = 320;
            EditFilterDialogControl.ToClose = CloseDialog;
            Content = new EditFilterDialogControl(tool_ctrl);
        }
        public void InitData(CheckBox chk_box)
        {
            Title = "Edit Filter";
            EditFilterDialogControl ctrl = Content as EditFilterDialogControl;
            ctrl.InitData(chk_box);
        }
        void CloseDialog()
        {
            Close();
        }
    }
}
