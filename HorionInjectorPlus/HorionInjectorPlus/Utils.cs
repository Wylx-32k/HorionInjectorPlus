using Microsoft.VisualBasic;
using System;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security.AccessControl;
using System.Security.Principal;
using System.Text;
using System.Threading;
using System.Windows.Forms;

namespace HorionInjectorPlus
{
    class commit
    {
        public string name { get; set; }
        public string commitName { get; set; }
        public string downloadLink { get; set; }
        public string version { get; set; }
        public string uniqueID { get; set; }
    }

    class DLLInjector
    {
        [DllImport("kernel32.dll")]
        public static extern IntPtr OpenProcess(IntPtr dwDesiredAccess, bool bInheritHandle, uint processId);

        [DllImport("kernel32.dll")]
        public static extern bool CloseHandle(IntPtr hObject);

        [DllImport("kernel32.dll")]
        public static extern IntPtr VirtualAllocEx(IntPtr hProcess, IntPtr lpAddress, uint dwSize, uint flAllocationType, uint flProtect);

        [DllImport("kernel32.dll")]
        public static extern bool WriteProcessMemory(IntPtr hProcess, IntPtr lpBaseAddress, byte[] lpBuffer, int nSize, out IntPtr lpNumberOfBytesWritten);

        [DllImport("kernel32.dll")]
        public static extern IntPtr GetProcAddress(IntPtr hModule, string procName);

        [DllImport("kernel32.dll")]
        public static extern IntPtr GetModuleHandle(string lpModuleName);

        [DllImport("kernel32.dll")]
        public static extern IntPtr CreateRemoteThread(IntPtr hProcess, IntPtr lpThreadAttributes, uint dwStackSize, IntPtr lpStartAddress, IntPtr lpParameter, uint dwCreationFlags, ref IntPtr lpThreadId);

        [DllImport("kernel32.dll")]
        public static extern uint WaitForSingleObject(IntPtr handle, uint milliseconds);

        [DllImport("user32.dll", SetLastError = true)]
        public static extern IntPtr FindWindow(string lpClassName, string lpWindowName);

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool SetForegroundWindow(IntPtr hWnd);

        [DllImport("kernel32.dll")]
        public static extern bool VirtualFreeEx(IntPtr hProcess, IntPtr lpAddress, int dwSize, IntPtr dwFreeType);

        public static void InjectDLL(IntPtr processHandle, string path)
        {
            byte[] dllBytes = Encoding.ASCII.GetBytes(path);
            IntPtr p1 = VirtualAllocEx(processHandle, IntPtr.Zero, (uint)(dllBytes.Length + 1), 12288U, 64U);
            WriteProcessMemory(processHandle, p1, dllBytes, dllBytes.Length, out IntPtr p2);
            IntPtr procAddress = GetProcAddress(GetModuleHandle("kernel32.dll"), "LoadLibraryA");
            IntPtr p3 = CreateRemoteThread(processHandle, IntPtr.Zero, 0U, procAddress, p1, 0U, ref p2);
            if (p3 == IntPtr.Zero)
            {
                throw new Exception("An error occurred while injecting the DLL.");
            }

            uint waitResult = WaitForSingleObject(p3, 5000);
            if (waitResult == 128L || waitResult == 258L)
            {
                CloseHandle(p3);
            }
            else
            {
                VirtualFreeEx(processHandle, p1, 0, (IntPtr)32768);
                if (p3 != IntPtr.Zero)
                    CloseHandle(p3);
            }
        }
    }

    class Injector
    {
        public static void InjectDLL(string path)
        {
            try
            {
                if (!File.Exists(path) || File.ReadAllBytes(path).Length < 10)
                {
                    throw new ArgumentException("Invalid or broken DLL file.", "path");
                }

                try
                {
                    var fileInfo = new FileInfo(path);
                    var accessControl = fileInfo.GetAccessControl();
                    accessControl.AddAccessRule(new FileSystemAccessRule(
                        new SecurityIdentifier("S-1-15-2-1"), FileSystemRights.FullControl,
                        InheritanceFlags.None, PropagationFlags.NoPropagateInherit, AccessControlType.Allow));
                    fileInfo.SetAccessControl(accessControl);
                }
                catch (Exception)
                {
                    throw new Exception("Run HorionInjectorPlus as Admin.");
                }

                var processes = Process.GetProcessesByName("Minecraft.Windows");
                if (processes.Length == 0)
                {
                    if (Interaction.Shell("explorer.exe shell:appsFolder\\Microsoft.MinecraftUWP_8wekyb3d8bbwe!App", Wait: false) == 0)
                    {
                        throw new Exception("Install Minecraft before using HorionInjectorPlus!");
                    }

                    int t = 0;
                    while (processes.Length == 0)
                    {
                        if (++t > 200)
                        {
                            throw new Exception("Cannot launch Minecraft.");
                        }

                        processes = Process.GetProcessesByName("Minecraft.Windows");
                        Thread.Sleep(10);
                    }
                    Thread.Sleep(3000);
                }

                var process = processes.First(p => p.Responding);

                for (int i = 0; i < process.Modules.Count; i++)
                {
                    if (process.Modules[i].FileName == path)
                    {
                        throw new Exception("Horion is already injected!");
                    }
                }

                IntPtr handle = DLLInjector.OpenProcess((IntPtr)2035711, false, (uint)process.Id);
                if (handle == IntPtr.Zero || !process.Responding)
                {
                    throw new Exception("An error occurred while opening the process.");
                }

                DLLInjector.InjectDLL(handle, path);

                IntPtr windowH = DLLInjector.FindWindow(null, "Minecraft");
                if (windowH == IntPtr.Zero)
                    throw new Exception("An error occurred while finding the Minecraft window.");
                else
                    DLLInjector.SetForegroundWindow(windowH);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "HorionInjectorPlus");
            }
        }
    }
}