﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.InteropServices;
using DWORD = System.UInt32;
using Microsoft.Win32.SafeHandles;
using System.Windows.Forms;
using System.Diagnostics;
using System.Windows.Threading;

namespace LogcatToolWin
{
    /*[System.Runtime.InteropServices.StructLayout(LayoutKind.Sequential)]
    public class SECURITY_ATTRIBUTES
    {
        public DWORD nLength;
        public IntPtr lpSecurityDescriptor;
        [MarshalAs(UnmanagedType.Bool)]
        public bool bInheritHandle;
    }*/

    class AdbAgent
    {
        public static Action<string, string, string, string, string> OnOutputLogcat;
        public bool IsDeviceReady = false;
        public string DeviceName;
        public static Action<string, bool> OnDeviceChecked;

        Process outputProcess;
        public AdbAgent()
        {
        }
        /*[DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        public static extern bool CreatePipe(ref IntPtr hReadPipe, ref IntPtr hWritePipe, IntPtr lpPipeAttributes, DWORD nSize);
        [DllImport("Kernel32.dll")]
        public static extern bool CloseHandle(System.IntPtr hObject);

        public string ProceedAdbCommand(string cmd)
        {
            SECURITY_ATTRIBUTES sa = new SECURITY_ATTRIBUTES();
            sa.nLength = (DWORD)Marshal.SizeOf(sa);
            sa.lpSecurityDescriptor = IntPtr.Zero;
            sa.bInheritHandle = true;

            IntPtr attr = Marshal.AllocHGlobal(Marshal.SizeOf(sa));
            Marshal.StructureToPtr(sa, attr, true);

            IntPtr hChildStd_OUT_Rd = new IntPtr();
            IntPtr hChildStd_OUT_Wr = new IntPtr();
            //HANDLE hChildStd_IN_Rd = NULL;
            //HANDLE hChildStd_IN_Wr = NULL;


            if (!CreatePipe(ref hChildStd_OUT_Rd, ref hChildStd_OUT_Wr, attr, 0))
            {
                MessageBox.Show("StdoutRd CreatePipe Fail");
                return "";
            }

            MessageBox.Show("StdoutRd CreatePipe OK");

            /*PROCESS_INFORMATION piProcInfo;
            STARTUPINFO siStartInfo;
            BOOL bSuccess = FALSE;

            ZeroMemory(&piProcInfo, sizeof(PROCESS_INFORMATION));
            ZeroMemory(&siStartInfo, sizeof(STARTUPINFO));
            siStartInfo.cb = sizeof(STARTUPINFO);
            siStartInfo.hStdError = hChildStd_OUT_Wr;
            siStartInfo.hStdOutput = hChildStd_OUT_Wr;
            //siStartInfo.hStdInput = hChildStd_IN_Rd;
            siStartInfo.dwFlags |= (STARTF_USESTDHANDLES | STARTF_USESHOWWINDOW);
            siStartInfo.wShowWindow = SW_HIDE;

            // Create the child process. 

            std::string full_cmd = "c:\\Develop\\Android\\SDK\\platform-tools\\adb.exe " + cmd;
            bSuccess = CreateProcess(NULL,
                (LPSTR)full_cmd.c_str(),     // command line 
                NULL,          // process security attributes 
                NULL,          // primary thread security attributes 
                TRUE,          // handles are inherited 
                0,             // creation flags 
                NULL,          // use parent's environment 
                NULL,          // use parent's current directory 
                &siStartInfo,  // STARTUPINFO pointer 
                &piProcInfo);  // receives PROCESS_INFORMATION 

            if (!bSuccess)
            {
                OutputDebugString(TEXT("CreateProcess"));
                return "";
            }

            WaitForSingleObject(piProcInfo.hProcess, INFINITE);
            DWORD dwRead;
            CHAR chBuf[2048];
            ZeroMemory(chBuf, 2048);
            bSuccess = FALSE;

            bSuccess = ReadFile(hChildStd_OUT_Rd, chBuf, 2048, &dwRead, NULL);
            std::string ret = "";
            if (bSuccess) ret = std::string(chBuf);
            OutputDebugString(chBuf);*/
        /*  CloseHandle(hChildStd_OUT_Rd);
          CloseHandle(hChildStd_OUT_Wr);
          return "";
      }*/
        public void ProceedAdbCommandToExit(string cmd, EventHandler handler)
        {
            Process process = new Process();
            string full_cmd = "c:/Develop/Android/SDK/platform-tools/adb.exe "; // + cmd;
            process.StartInfo.FileName = full_cmd;
            process.StartInfo.Arguments = cmd; // "/c DIR"; // Note the /c command (*)
            process.StartInfo.UseShellExecute = false;
            process.StartInfo.RedirectStandardOutput = true;
            process.StartInfo.RedirectStandardError = true;
            process.StartInfo.CreateNoWindow = true;
            process.Exited += handler;
            process.EnableRaisingEvents = true;
            //process.OutputDataReceived += new DataReceivedEventHandler(OutputHandler);
            //process.ErrorDataReceived += new DataReceivedEventHandler(OutputHandler);
            //* Start process and handlers
            process.Start();
            //process.BeginOutputReadLine();
            //process.BeginErrorReadLine();
            //process.WaitForExit();
            //* Read the output (or the error)
            //string output = process.StandardOutput.ReadToEnd();
            //MessageBox.Show(output);
            //Console.WriteLine(output);
            //string err = process.StandardError.ReadToEnd();
            //Console.WriteLine(err);
        }
        public void ProceedAdbCommandToOutput(string cmd, DataReceivedEventHandler handler)
        {
            outputProcess = new Process();
            string full_cmd = "c:/Develop/Android/SDK/platform-tools/adb.exe "; // + cmd;
            outputProcess.StartInfo.FileName = full_cmd;
            outputProcess.StartInfo.Arguments = cmd; // "/c DIR"; // Note the /c command (*)
            outputProcess.StartInfo.UseShellExecute = false;
            outputProcess.StartInfo.RedirectStandardOutput = true;
            outputProcess.StartInfo.RedirectStandardError = true;
            outputProcess.StartInfo.CreateNoWindow = true;
            outputProcess.OutputDataReceived += handler;
            outputProcess.ErrorDataReceived += handler;
            //* Start process and handlers
            outputProcess.Start();
            outputProcess.PriorityClass = ProcessPriorityClass.Idle;
            outputProcess.BeginOutputReadLine();
            outputProcess.BeginErrorReadLine();
            //process.WaitForExit();
            //* Read the output (or the error)
            //string output = process.StandardOutput.ReadToEnd();
            //MessageBox.Show(output);
            //Console.WriteLine(output);
            //string err = process.StandardError.ReadToEnd();
            //Console.WriteLine(err);
        }
        public void CheckAdbDevice()
        {
            ProceedAdbCommandToExit("devices", new EventHandler(AdbCheckDeviceHandler));
        }
        public void StartAdbLogcat()
        {
            ProceedAdbCommandToOutput("logcat -v time", new DataReceivedEventHandler(OutputHandler));
        }
        public void StopAdbLogcat()
        {
            if (outputProcess != null)
            {
                outputProcess.CancelErrorRead();
                outputProcess.CancelOutputRead();
                outputProcess.Close();
                outputProcess = null;
            }
        }
        void AdbCheckDeviceHandler(object sender, System.EventArgs ev)
        {
            Process process = sender as Process;
            string output = process.StandardOutput.ReadToEnd();
            string[] msg_line = output.Split(new char[] { '\n', '\r'});
            foreach (string msg in msg_line)
            {
                string[] tokens = msg.Split('\t');
                if (tokens.Length != 2) continue;
                DeviceName = tokens[0];
                if (tokens[1] == "device") IsDeviceReady = true; else IsDeviceReady = false;
            }
            if (OnDeviceChecked != null)
            {
                OnDeviceChecked(DeviceName, IsDeviceReady);
            }
            //MessageBox.Show(output);
        }
        static void OutputHandler(object sendingProcess, DataReceivedEventArgs outLine)
        {
            if (OnOutputLogcat == null) return;

            string msg = outLine.Data;
            if ((msg == null) || (msg.Length == 0)) return;
            int time_split = msg.IndexOf(' ', 7);
            if (time_split == -1) return;
            string time_token = msg.Substring(0, time_split);
            msg = msg.Substring(time_split + 1);
            string level_token = msg.Substring(0, 1);
            msg = msg.Substring(2);
            int tag_split = msg.IndexOf('(');
            if (tag_split == -1) return;
            string tag_token = msg.Substring(0, tag_split);
            msg = msg.Substring(tag_split + 1);
            int pid_split = msg.IndexOf(')');
            if (pid_split == -1) return;
            string pid_token = msg.Substring(0, pid_split);
            string msg_token = msg.Substring(pid_split + 2);
            OnOutputLogcat(level_token, time_token, pid_token, tag_token, msg_token);
        }
    }
}