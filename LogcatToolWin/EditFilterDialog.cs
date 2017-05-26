using Microsoft.VisualStudio.PlatformUI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LogcatToolWin
{
    class EditFilterDialog : DialogWindow
    {
        public EditFilterDialog(LogcatOutputToolWindowControl tool_ctrl) : base()
        {
            Title = "Edit Filter";
            Height = 320;
            Width = 320;
            EditFilterDialogControl.ToClose = CloseDialog;
            Content = new EditFilterDialogControl(tool_ctrl);
        }
        void CloseDialog()
        {
            Close();
        }
    }
}
