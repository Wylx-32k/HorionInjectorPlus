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
                Process minecraftProcess = dllInjector.GetOrCreateMinecraftProcess();

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
    public partial class CustomScrollbarUserControl : UserControl
    {
        private int thumbPosition;
        public int thumbSize;
        private int thumbMinimumSize = 20;
        private bool isDraggingThumb;
        private int thumbOffsetOnClick;
        private int maxValue = 150;

        public event EventHandler<int> ScrollChanged;

        private readonly Color scrollbarColor = Color.FromArgb(20, 20, 20);
        private readonly Color thumbColor = Color.FromArgb(40, 40, 40);
        public
        const int scrollbarWidth = 10;
        private
        const int scrollbarMargin = 2;
        private
        const int scrollbarRadius = 1;

        public void setMaxValue(int value)
        {
            maxValue = value;
        }

        public CustomScrollbarUserControl()
        {
            DoubleBuffered = true;
            thumbPosition = 0;
            thumbSize = 50;

            this.Size = new Size(scrollbarWidth, 200);
            this.BackColor = Color.Transparent;
        }

        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);
            Refresh();
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            DrawScrollbar(e.Graphics);
        }

        private void DrawScrollbar(Graphics g)
        {
            Rectangle thumbRect = new Rectangle(scrollbarMargin, thumbPosition, scrollbarWidth - 2 * scrollbarMargin, thumbSize);
            g.Clear(scrollbarColor);

            using (SolidBrush thumbBrush = new SolidBrush(thumbColor))
            {
                FillRoundedRectangle(g, thumbBrush, thumbRect, scrollbarRadius);
            }
        }

        protected override void OnMouseDown(MouseEventArgs e)
        {
            base.OnMouseDown(e);
            if (e.Button == MouseButtons.Left && GetThumbRectangle().Contains(e.Location))
            {
                isDraggingThumb = true;
                thumbOffsetOnClick = e.Y - thumbPosition;
            }
        }

        protected override void OnMouseUp(MouseEventArgs e)
        {
            base.OnMouseUp(e);
            isDraggingThumb = false;
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);
            if (isDraggingThumb)
            {
                int newPosition = e.Y - thumbOffsetOnClick;
                thumbPosition = Math.Max(0, Math.Min(Height - thumbSize, newPosition));
                OnScrollChanged();
                Invalidate();
            }
        }

        private void OnScrollChanged()
        {
            int maxScrollValue = Height - thumbSize;
            int scrollValue = (maxScrollValue > 0) ? thumbPosition * maxValue / maxScrollValue : 0;
            ScrollChanged?.Invoke(this, scrollValue);
        }

        protected override void OnMouseWheel(MouseEventArgs e)
        {
            base.OnMouseWheel(e);

            int scrollChange = e.Delta / SystemInformation.MouseWheelScrollDelta;
            int maxScrollValue = Height - thumbSize;

            if (maxScrollValue > 0)
            {
                thumbPosition = Math.Max(0, Math.Min(maxScrollValue, thumbPosition - scrollChange));
                OnScrollChanged();
                Invalidate();
            }
        }

        private Rectangle GetThumbRectangle()
        {
            return new Rectangle(scrollbarMargin, thumbPosition, scrollbarWidth - 2 * scrollbarMargin, thumbSize);
        }
        public int ThumbPosition
        {
            get
            {
                return thumbPosition;
            }
            set
            {
                int maxScrollValue = Height - thumbSize;
                thumbPosition = Math.Max(0, Math.Min(maxScrollValue, value));
                OnScrollChanged();
                Invalidate();
            }
        }

        public void TriggerScrollChanged()
        {
            OnScrollChanged();
            Invalidate();
        }
        private void FillRoundedRectangle(Graphics g, Brush brush, Rectangle bounds, int radius)
        {
            int diameter = radius * 2;
            Rectangle arcRect = new Rectangle(bounds.Location, new Size(diameter, diameter));

            g.FillPie(brush, arcRect, 180, 90);

            arcRect.X = bounds.Right - diameter;
            g.FillPie(brush, arcRect, 270, 90);

            arcRect.Y = bounds.Bottom - diameter;
            g.FillPie(brush, arcRect, 0, 90);

            arcRect.X = bounds.Left;
            g.FillPie(brush, arcRect, 90, 90);

            g.FillRectangle(brush, bounds.Left + radius, bounds.Top, bounds.Width - diameter, bounds.Height);
            g.FillRectangle(brush, bounds.Left, bounds.Top + radius, bounds.Width, bounds.Height - diameter);
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

        public Process GetOrCreateMinecraftProcess()
        {
            Process[] processes = Process.GetProcessesByName("Minecraft.Windows");
            return processes.Length > 0 ? processes[0] : InstallAndLaunchMinecraftApp();
        }
        static bool IsMinecraftInstalled()
        {
            string script = "$packageName = '*Minecraft*'; $packageFound = Get-AppxPackage | Where-Object { $_.Name -like $packageName }; [bool]($packageFound -ne $null)";
            ProcessStartInfo psi = new ProcessStartInfo("powershell.exe", $"-Command \"{script}\"")
            {
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            Process process = new Process
            {
                StartInfo = psi
            };
            process.Start();
            string output = process.StandardOutput.ReadToEnd();
            process.WaitForExit();
            return bool.Parse(output.Trim());
        }
        public Process InstallAndLaunchMinecraftApp()
        {
            if (!IsMinecraftInstalled())
            {
                throw new Exception("Please install Minecraft before using HorionInjectorPlus.");
            }
            Process.Start("explorer.exe", "shell:appsFolder\\Microsoft.MinecraftUWP_8wekyb3d8bbwe!App");
            int t = 0;
            Process[] processes;
            while ((processes = Process.GetProcessesByName("Minecraft.Windows")).Length == 0)
            {
                if (++t > 200)
                {
                    throw new Exception("Unable to launch Minecraft, launch it yourself.");
                }
                Thread.Sleep(10);
            }
            Thread.Sleep(3000);
            return processes[0];
        }

        public void CheckIfAlreadyInjected(Process process, string path)
        {
            foreach (ProcessModule module in process.Modules)
            {
                if (module.FileName == path)
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