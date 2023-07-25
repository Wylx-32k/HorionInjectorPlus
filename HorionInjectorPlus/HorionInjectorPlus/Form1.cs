using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace HorionInjectorPlus
{

    public partial class Form1 : Form
    {


        public
        const int WM_NCLBUTTONDOWN = 0xA1;
        public
        const int HT_CAPTION = 0x2;

        [DllImportAttribute("user32.dll")]
        public static extern int SendMessage(IntPtr hWnd, int Msg, int wParam, int lParam);
        [DllImportAttribute("user32.dll")]
        public static extern bool ReleaseCapture();
        public CustomScrollbarUserControl customScrollbar = new CustomScrollbarUserControl();
        private static List<commit> commits = new List<commit>();
        private Panel itemsContainer;
        private int lastScrollValue = 0;
        private static string downloadDirectory = "C:/HorionPlus/download";
        public static string DownloadDirectory
        {
            get { return downloadDirectory; }
            set { downloadDirectory = value; }
        }
        public Form1()
        {
            InitializeComponent();
            DoubleBuffered = true;
            if (MessageBox.Show("Go back in time with HorionInjector+!\n\nCredits\n\n- Created by Chocolate Milk\n- DLLs provided by USSR and Shuit\n\nNotes\n\nIf a button is green, it is recommended based on your current Minecraft Version.\nRight click the button to delete or update.\n\nNeed assistance? Join our Discord community! https://discord.gg/hhBbNXVjgh\nGitHub repository: https://github.com/Wylx-32k/HorionInjectorPlus/\n\nYour current Minecraft Version: " + GetVersion() + "\nClick 'Yes' to open our Discord and GitHub links\nClick 'No' to proceed without links", "HorionInjectorPlus", MessageBoxButtons.YesNo) == DialogResult.Yes)
            {
                Process.Start("https://discord.gg/hhBbNXVjgh");
                Process.Start("https://github.com/Wylx-32k/HorionInjectorPlus/");
            }

            itemsContainer = new Panel();
            itemsContainer.Location = new Point(0, 0);
            itemsContainer.Size = new Size(ClientSize.Width - customScrollbar.Width, ClientSize.Height);
            Controls.Add(itemsContainer);

            LoadDataAsync();
        }
        public static string GetVersion()
        {
            string script = "((Get-AppPackage -name Microsoft.MinecraftUWP).Version) -replace [Environment]::NewLine, ''";
            ProcessStartInfo psi = new ProcessStartInfo("powershell.exe", "-Command " + script)
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
            return output;
        }
        static List<T> FlipList<T>(List<T> inputList)
        {
            inputList.Reverse();
            return inputList;
        }
        private async Task LoadDataAsync()
        {
            WebClient wb = new WebClient();
            string text = await wb.DownloadStringTaskAsync("https://raw.githubusercontent.com/Wylx-32k/HorionInjectorPlus/main/dlls/downloadDLLRaw");

            foreach (string line in text.Split('\n'))
            {
                var values = line.Split('|');

                foreach (string commit in values.Skip(1))
                {
                    commit com = new commit();
                    if (commit.Contains(';'))
                    {
                        com.name = values[0] + " - " + commit.Split(';')[0].Substring(1);
                        com.commitName = commit.Split(';')[1].Split('-')[0].Replace(" ", "");
                    }
                    else
                    {
                        com.name = values[0] + " - " + commit.Split('-')[0].Substring(1);

                        com.commitName = "No commit ID provided";
                    }
                    com.downloadLink = commit.Substring(commit.IndexOf('-') + 1).Replace(" ", "");
                    com.uniqueID = Base64Encode(com.name);
                    com.version = values[0];
                    commits.Add(com);
                }
            }

            int startupY = 26;
            int containerHeight = 0;
            if (commits.Count == 0)
            {
                MessageBox.Show("The GitHub page is updating! Please try again later, or maybe its just your wifi.", "HorionInjector+");
                Application.Exit();
            }
            
            foreach (var value in FlipList(commits))
            {
                Button b = new Button();
                b.Font = new Font("Segoe UI Semibold", 7, FontStyle.Bold);
                b.Location = new Point(12, startupY);
                b.Text = "Download";
                b.Size = new Size(61, 22);
                b.BackColor = Color.FromArgb(54, 137, 182);
                b.FlatStyle = FlatStyle.Flat;
                b.FlatAppearance.BorderSize = 0;
                b.ForeColor = Color.White;
                b.Name = value.downloadLink;
                b.Click += downloadButton_Click;

                Label name = new Label();
                name.Font = new Font("Segoe UI Semibold", 8, FontStyle.Bold);
                name.Location = new Point(12 + b.Width, startupY);
                name.ForeColor = Color.White;
                name.Text = value.name;
                name.AutoSize = true;
                name.Anchor = AnchorStyles.Left | AnchorStyles.Top;
                name.Size = new Size(0, 0);
                name.UseMnemonic = true;
                name.BackColor = Color.Transparent;

                Label commitLabel = new Label();
                commitLabel.Font = new Font("Segoe UI Semibold", 6.5f, FontStyle.Bold);
                commitLabel.Location = new Point(12 + b.Width, startupY + 11);
                commitLabel.ForeColor = Color.White;
                commitLabel.Text = value.commitName;
                commitLabel.AutoSize = true;
                commitLabel.Anchor = AnchorStyles.Left | AnchorStyles.Top;
                commitLabel.Size = new Size(0, 0);
                commitLabel.UseMnemonic = true;
                commitLabel.BackColor = Color.Transparent;
                ContextMenuStrip cms = new ContextMenuStrip();
                cms.Items.Add("Delete");
                cms.Items[0].Click += delete_Click;
                cms.Items[0].BackColor = Color.FromArgb(30, 30, 30);
                cms.Items[0].ForeColor = Color.White;
                cms.Items[0].Name = value.downloadLink;
                cms.Items[0].Font = new Font("Segoe UI Semibold", 8f, FontStyle.Bold);
                cms.Items.Add("Update");
                cms.Items[1].Click += update_Click;
                cms.Items[1].BackColor = Color.FromArgb(30, 30, 30);
                cms.Items[1].ForeColor = Color.White;
                cms.Items[1].Name = value.downloadLink;
                cms.Items[1].Font = new Font("Segoe UI Semibold", 8f, FontStyle.Bold);
                cms.Name = value.downloadLink;

                b.ContextMenuStrip = cms;

                itemsContainer.Controls.Add(b);
                itemsContainer.Controls.Add(name);
                itemsContainer.Controls.Add(commitLabel);

                startupY += 25;
                containerHeight += 25;

            }

            itemsContainer.Size = new Size(ClientSize.Width - customScrollbar.Width, containerHeight);

            customScrollbar.Dock = DockStyle.Right;
            customScrollbar.ScrollChanged += CustomScrollbar_ScrollChanged;
            customScrollbar.Layout += CustomScrollbar_Layout;
            customScrollbar.setMaxValue(containerHeight - 490);
            Controls.Add(customScrollbar);
        }

        private void CustomScrollbar_ScrollChanged(object sender, int newValue)
        {
            int deltaScrollValue = newValue - lastScrollValue;

            itemsContainer.Location = new Point(itemsContainer.Location.X, itemsContainer.Location.Y - deltaScrollValue);

            foreach (Control control in itemsContainer.Controls)
            {
                if (control is Button button)
                {
                    bool isVisible = button.Top + itemsContainer.Top >= 0 && button.Bottom + itemsContainer.Top <= ClientSize.Height;
                    button.Visible = isVisible;
                }
            }

            lastScrollValue = newValue;
        }

        private void CustomScrollbar_Layout(object sender, LayoutEventArgs e) { }

        protected override void OnMouseWheel(MouseEventArgs e)
        {
            base.OnMouseWheel(e);

            int maxScrollValue = itemsContainer.Height - customScrollbar.thumbSize;
            int scrollChangeAmount = 10;

            if (maxScrollValue > 0)
            {
                int scrollChange = (e.Delta / SystemInformation.MouseWheelScrollDelta) * scrollChangeAmount;
                customScrollbar.ThumbPosition = Math.Max(0, Math.Min(maxScrollValue, customScrollbar.ThumbPosition - scrollChange));
            }
        }

        protected override void OnLayout(LayoutEventArgs e)
        {
            base.OnLayout(e);

            customScrollbar.Location = new Point(customScrollbar.Location.X, customScrollbar.Location.Y + 25);
        }

        private void Form1_Load(object sender, EventArgs e)
        {

        }

        private void button1_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        private void downloadButton_Click(object sender, EventArgs e)
        {

            string name = ((Button)sender).Name;
            string text = ((Button)sender).Text;
            string filename = "";


            if (!Directory.Exists(downloadDirectory))
            {
                Directory.CreateDirectory(downloadDirectory);
            }

            foreach (var com in commits)
            {
                if (com.downloadLink == name)
                {
                    filename = Path.Combine(downloadDirectory, com.uniqueID.Replace("/", "").Replace("\\", "") + ".dll");
                    break;
                }
            }
            if (text.Equals("Download") || text.Equals("Error"))
            {
                if (!string.IsNullOrEmpty(filename))
                {
                    ((Button)sender).Text = "Downloading";
                    ((Button)sender).Font = new Font("Segoe UI Semibold", 5, FontStyle.Bold);
                    ((Button)sender).BackColor = Color.FromArgb(212, 175, 55);
                    Task.Run(async () => {
                        using (WebClient wb = new WebClient())
                        {
                            try
                            {
                                await wb.DownloadFileTaskAsync(name, filename);
                                ((Button)sender).Font = new Font("Segoe UI Semibold", 7, FontStyle.Bold);
                                ((Button)sender).Text = "Finished";
                            }
                            catch (Exception ex)
                            {

                                Console.WriteLine("An error occurred during download: {0}. Download Link: {1}, Filename: {2}", ex, name, filename);
                                ((Button)sender).Font = new Font("Segoe UI Semibold", 7, FontStyle.Bold);
                                ((Button)sender).Text = "Error";

                                ((Button)sender).BackColor = Color.FromArgb(179, 0, 0);
                                if (File.Exists(filename)) File.Delete(filename);
                            }
                        }
                    });
                }
            }
            else
            {

                Injector.InjectDLL(filename);
            }
        }
        private void button2_Click(object sender, EventArgs e)
        {
            this.WindowState = FormWindowState.Minimized;
        }
        static string version = GetVersion();
        public static void downloadAndOrInjectRecommended()
        {
            commit latestCommit = commits
    .Where(c => c.version.Contains(version.Substring(0, 6)))
    .FirstOrDefault();
            if (latestCommit != null)
                Task.Run(async () =>
                {
                    using (WebClient wb = new WebClient())
                    {
                            await wb.DownloadFileTaskAsync(latestCommit.downloadLink, Path.Combine(downloadDirectory, latestCommit.uniqueID.Replace("/", "").Replace("\\", "") + ".dll"));
                            Injector.InjectDLL(Path.Combine(downloadDirectory, latestCommit.uniqueID.Replace("/", "").Replace("\\", "") + ".dll"));
                    }
                });
            else MessageBox.Show("Err");
        }
        private void updateButtons_Tick(object sender, EventArgs e)
        {
            if (Other.ischecked == true && this.TopMost==false) this.TopMost = true; else if(this.TopMost!=false && Other.ischecked ==false)this.TopMost = false;
            if (itemsContainer != null)
                foreach (Button button in itemsContainer.Controls.OfType<Button>().ToList())
                {

                    if (!Directory.Exists(downloadDirectory))
                    {
                        Directory.CreateDirectory(downloadDirectory);
                    }
                    button.Font = new Font("Segoe UI Semibold", 7, FontStyle.Bold);
                    foreach (var com in commits)
                    {
                        if (com.downloadLink == button.Name)
                        {
                            FileInfo fi = new FileInfo(Path.Combine(downloadDirectory, com.uniqueID.Replace("/", "").Replace("\\", "") + ".dll"));
                            if (File.Exists(Path.Combine(downloadDirectory, com.uniqueID.Replace("/", "").Replace("\\", "") + ".dll")) && fi.Length > 100000)
                            {
                                button.Text = "Inject";
                                button.BackColor = Color.FromArgb(28, 42, 77);
                            }
                            else
                            {
                                if (!button.Text.Contains("Download") && button.Text != "Error")
                                {

                                    button.Text = "Download";
                                    button.BackColor = Color.FromArgb(54, 137, 182);
                                }

                            }
                            if (com.version != null)
                            {

                                if (com.version.Contains(version.Substring(0, 6)) && button.Text != "Downloading")
                                    button.BackColor = Color.FromArgb(76, 175, 80);

                                else if (!button.Text.Contains("Download") && button.Text != "Inject" && button.Text != "Error") button.BackColor = Color.FromArgb(54, 137, 182);
                            }
                        }
                    }
                }
        }
        public static string Base64Encode(string plainText)
        {
            var plainTextBytes = System.Text.Encoding.UTF8.GetBytes(plainText);
            return System.Convert.ToBase64String(plainTextBytes);
        }

        private void Form1_Load_1(object sender, EventArgs e)
        {

        }

        private void titleBar_MouseMove(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                ReleaseCapture();
                SendMessage(Handle, WM_NCLBUTTONDOWN, HT_CAPTION, 0);
            }
        }

        private void label1_MouseMove(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                ReleaseCapture();
                SendMessage(Handle, WM_NCLBUTTONDOWN, HT_CAPTION, 0);
            }
        }

        private void Discord_Click(object sender, EventArgs e)
        {
            Process.Start("https://discord.gg/hhBbNXVjgh");
        }

        private void GitHub_Click(object sender, EventArgs e)
        {
            Process.Start("https://github.com/Wylx-32k/HorionInjectorPlus/");
        }

        private void delete_Click(object sender, EventArgs e)
        {
            string filename = "";

            if (!Directory.Exists(downloadDirectory))
            {
                Directory.CreateDirectory(downloadDirectory);
            }

            foreach (var com in commits)
            {
                if (com.downloadLink == ((ToolStripMenuItem)sender).Name)
                {
                    filename = Path.Combine(downloadDirectory, com.uniqueID.Replace("/", "").Replace("\\", "") + ".dll");
                    break;
                }
            }

            if (File.Exists(filename))
            {
                File.Delete(filename);
                MessageBox.Show("Successfully deleted");
            }
            else
            {
                MessageBox.Show("No DLL to delete.");
            }
        }

        private void update_Click(object sender, EventArgs e)
        {
            string filename = "";

            if (!Directory.Exists(downloadDirectory))
            {
                Directory.CreateDirectory(downloadDirectory);
            }

            foreach (var com in commits)
            {
                if (com.downloadLink == ((ToolStripMenuItem)sender).Name)
                {
                    filename = Path.Combine(downloadDirectory, com.uniqueID.Replace("/", "").Replace("\\", "") + ".dll");
                    break;
                }
            }

            if (File.Exists(filename))
            {
                File.Delete(filename);
                WebClient wb = new WebClient();
                wb.DownloadFile(((ToolStripMenuItem)sender).Name, filename);
                MessageBox.Show("Updated!");
            }
            else
            {
                MessageBox.Show("No DLL to update.");
            }
        }

        public static void deleteAll()
        {
            if (!isDownloading)
            {
                foreach (var com in commits)
                {
                    string filename = Path.Combine(downloadDirectory, com.uniqueID.Replace("/", "").Replace("\\", "") + ".dll");

                    if (!Directory.Exists(downloadDirectory))
                    {
                        Directory.CreateDirectory(downloadDirectory);
                    }

                    if (File.Exists(filename)) File.Delete(filename);
                }
                MessageBox.Show("Operation Complete");
            }
            else
            {
                MessageBox.Show("Wait till downloading finishes");
            }

        }
        static bool isDownloading = false;
        public static void DownloadAll()
        {
            Task.Run(async () => {
                List<Task> tasks = new List<Task>();
                isDownloading = true;
                foreach (var com in commits)
                {
                    string filename = Path.Combine(downloadDirectory, com.uniqueID.Replace("/", "").Replace("\\", "") + ".dll");

                    if (!Directory.Exists(downloadDirectory))
                    {
                        Directory.CreateDirectory(downloadDirectory);
                    }

                    if (!File.Exists(filename))
                        tasks.Add(Task.Run(() => {
                            using (WebClient wb = new WebClient())
                            {
                                try
                                {
                                    wb.DownloadFile(com.downloadLink, filename);
                                }
                                catch (Exception)
                                {
                                    MessageBox.Show("An error occured when downloading " + com.name);
                                }
                            }
                        }));

                }
                await Task.WhenAll(tasks);
                MessageBox.Show("Operation Complete");
                isDownloading = false;
            });

        }
        private Other otherForm;
        private void other_Click(object sender, EventArgs e)
        {
            if (otherForm == null)
                otherForm = new Other();

            if (otherForm.Visible)
                otherForm.Hide();
            else
                otherForm.Show();
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

}