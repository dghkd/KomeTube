using System;
using System.Text;
using System.Drawing;
using System.Diagnostics;
using System.IO;
using System.Collections;
using System.Runtime.InteropServices;
using System.Threading;

namespace HtmlTextBlock
{
    public class ProcessInfo
    {
        public IntPtr hProcess;
        public IntPtr hThread;
        public Int32 ProcessID;
        public Int32 ThreadID;
    }

    public class Header
    {
        private const string user32 = "user32.dll";
        private const string kernel32 = "kernel32.dll";

        [DllImport(kernel32)]
        public static extern Int32 CreateProcess(string appName,
            string cmdLine, IntPtr lpProcessAttributes, IntPtr lpThreadAttributes,
            Int32 boolInheritHandles, Int32 dwCreationFlags, IntPtr lpEnvironment,
            IntPtr lpszCurrentDir, Byte[] si, ProcessInfo pi);

        [DllImport(kernel32)]
        public static extern Int32 WaitForSingleObject(IntPtr handle, Int32 wait);

        [DllImport(kernel32)]
        public static extern Int32 GetLastError();

        [DllImport(kernel32)]
        public static extern Int32 CloseHandle(IntPtr handle);
    }
}