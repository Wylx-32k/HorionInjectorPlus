using Microsoft.VisualBasic;
using System;
using System.Diagnostics;
using System.Drawing;
using System.Runtime.InteropServices;
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

    public static class Injector
    {
        public static void InjectDLL(string path)
        {
            try
            {
                var dllInjector = new DLLInjector();
                var minecraftProcess = dllInjector.GetMinecraftProcess();

                if (minecraftProcess == null)
                {
                    dllInjector.InstallMinecraftApp();
                    minecraftProcess = dllInjector.WaitForMinecraftProcess();
                }

                dllInjector.CheckIfAlreadyInjected(minecraftProcess, path);
                dllInjector.Inject(minecraftProcess, path);
                dllInjector.FocusMinecraftWindow();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "HorionInjectorPlus");
            }
        }
    }

    public class DLLInjector
    {
        private const int PROCESS_VM_OPERATION = 0x0008;
        private const int PROCESS_VM_READ = 0x0010;
        private const int PROCESS_VM_WRITE = 0x0020;
        private const int MEM_COMMIT = 0x1000;
        private const int MEM_RELEASE = 0x8000;
        private const int PAGE_READWRITE = 0x04;

        [DllImport("kernel32.dll")]
        private static extern IntPtr OpenProcess(int dwDesiredAccess, bool bInheritHandle, uint processId);

        [DllImport("kernel32.dll")]
        private static extern bool CloseHandle(IntPtr hObject);

        [DllImport("kernel32.dll")]
        private static extern IntPtr VirtualAllocEx(IntPtr hProcess, IntPtr lpAddress, uint dwSize, int flAllocationType, int flProtect);

        [DllImport("kernel32.dll")]
        private static extern bool WriteProcessMemory(IntPtr hProcess, IntPtr lpBaseAddress, byte[] lpBuffer, int nSize, out IntPtr lpNumberOfBytesWritten);

        [DllImport("kernel32.dll")]
        private static extern IntPtr GetProcAddress(IntPtr hModule, string procName);

        [DllImport("kernel32.dll")]
        private static extern IntPtr GetModuleHandle(string lpModuleName);

        [DllImport("kernel32.dll")]
        private static extern IntPtr CreateRemoteThread(IntPtr hProcess, IntPtr lpThreadAttributes, uint dwStackSize, IntPtr lpStartAddress, IntPtr lpParameter, uint dwCreationFlags, ref IntPtr lpThreadId);

        [DllImport("kernel32.dll")]
        private static extern uint WaitForSingleObject(IntPtr handle, uint milliseconds);

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern bool VirtualFreeEx(IntPtr hProcess, IntPtr lpAddress, int dwSize, int dwFreeType);

        [DllImport("user32.dll")]
        private static extern IntPtr FindWindow(string lpClassName, string lpWindowName);

        [DllImport("user32.dll")]
        private static extern bool SetForegroundWindow(IntPtr hWnd);

        public Process GetMinecraftProcess()
        {
            var processes = Process.GetProcessesByName("Minecraft.Windows");
            return processes.Length > 0 ? processes[0] : null;
        }

        public void InstallMinecraftApp()
        {
            if (Interaction.Shell("explorer.exe shell:appsFolder\\Microsoft.MinecraftUWP_8wekyb3d8bbwe!App", Wait: false) == 0)
            {
                throw new Exception("Please install Minecraft before using HorionInjectorPlus.");
            }
        }

        public Process WaitForMinecraftProcess()
        {
            int t = 0;
            Process[] processes;
            while ((processes = Process.GetProcessesByName("Minecraft.Windows")).Length == 0)
            {
                if (++t > 200)
                {
                    throw new Exception("Unable to launch Minecraft.");
                }
                Thread.Sleep(10);
            }
            Thread.Sleep(3000);
            return processes[0];
        }

        public void CheckIfAlreadyInjected(Process process, string path)
        {
            for (int i = 0; i < process.Modules.Count; i++)
            {
                if (process.Modules[i].FileName == path)
                {
                    throw new Exception("Horion is already injected!");
                }
            }
        }

        public void Inject(Process process, string path)
        {
            IntPtr handle = OpenProcess(PROCESS_VM_OPERATION | PROCESS_VM_READ | PROCESS_VM_WRITE, false, (uint)process.Id);
            if (handle == IntPtr.Zero || !process.Responding)
            {
                throw new Exception("An error occurred while opening the Minecraft process.");
            }

            try
            {
                byte[] dllBytes = Encoding.UTF8.GetBytes(path);
                IntPtr allocatedMemory = VirtualAllocEx(handle, IntPtr.Zero, (uint)(dllBytes.Length + 1), MEM_COMMIT, PAGE_READWRITE);
                if (allocatedMemory == IntPtr.Zero)
                {
                    throw new Exception("An error occurred while allocating memory for the injection.");
                }

                IntPtr bytesWritten;
                if (!WriteProcessMemory(handle, allocatedMemory, dllBytes, dllBytes.Length, out bytesWritten))
                {
                    throw new Exception("An error occurred while writing to the process memory.");
                }

                IntPtr loadLibraryAddress = GetProcAddress(GetModuleHandle("kernel32.dll"), "LoadLibraryA");
                if (loadLibraryAddress == IntPtr.Zero)
                {
                    throw new Exception("Failed to get the 'LoadLibraryA' address from the kernel32.dll.");
                }

                IntPtr threadId = IntPtr.Zero;
                IntPtr remoteThread = CreateRemoteThread(handle, IntPtr.Zero, 0U, loadLibraryAddress, allocatedMemory, 0U, ref threadId);
                if (remoteThread == IntPtr.Zero)
                {
                    throw new Exception("Failed to create a remote thread for the injection.");
                }

                uint waitResult = WaitForSingleObject(remoteThread, 5000);
                if (waitResult == 128L || waitResult == 258L)
                {
                    CloseHandle(remoteThread);
                }
                else
                {
                    VirtualFreeEx(handle, allocatedMemory, 0, MEM_RELEASE);
                    if (remoteThread != IntPtr.Zero)
                        CloseHandle(remoteThread);
                    if (handle != IntPtr.Zero)
                        CloseHandle(handle);
                }
            }
            catch
            {
                if (handle != IntPtr.Zero)
                    CloseHandle(handle);
                throw;
            }
        }

        public void FocusMinecraftWindow()
        {
            IntPtr windowH = FindWindow(null, "Minecraft");
            if (windowH == IntPtr.Zero)
                throw new Exception("Failed to find the Minecraft window.");
            else
                SetForegroundWindow(windowH);
        }
    }

}
