using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LogcatToolWin
{
    class LogFilterData
    {
        public string FilterName;
        public string TokenByTag;
        public int TokenByPid;
        public string TokenByText;
        public LogcatOutputToolWindowControl.LogcatItem.Level TokenByLevel;
        public bool IsFilterSelected(object obj)
        {
            LogcatOutputToolWindowControl.LogcatItem item = obj as LogcatOutputToolWindowControl.LogcatItem;
            if (item == null) return false;
            if (TokenByLevel != LogcatOutputToolWindowControl.LogcatItem.Level.Verbose)
            {
                if (IsFilterOutByLevel(item)) return false;
            }
            if (TokenByPid != 0)
            {
                if (IsFilterOutByPid(item)) return false;
            }
            if ((TokenByTag != null) && (TokenByTag.Length > 0))
            {
                if (IsFilterOutByTag(item)) return false;
            }
            if ((TokenByText != null) && (TokenByText.Length > 0))
            {
                if (IsFilterOutByText(item)) return false;
            }
            return true;
        }
        bool IsFilterOutByTag(LogcatOutputToolWindowControl.LogcatItem item)
        {
            if (item.TagToken != TokenByTag) return true;
            return false;
        }
        bool IsFilterOutByPid(LogcatOutputToolWindowControl.LogcatItem item)
        {
            if (item.PidToken != TokenByPid) return true;
            return false;
        }
        bool IsFilterOutByLevel(LogcatOutputToolWindowControl.LogcatItem item)
        {
            if ((int)item.LevelToken < (int)TokenByLevel) return true;
            return false;
        }
        bool IsFilterOutByText(LogcatOutputToolWindowControl.LogcatItem item)
        {
            if (item.TextToken.Contains(TokenByText)) return false;
            return true;
        }
    }
}
