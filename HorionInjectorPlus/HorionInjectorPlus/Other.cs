using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace HorionInjectorPlus
{
    public partial class Other : Form
    {
        public Other()
        {
            InitializeComponent();
        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {
            if(!File.Exists(textBox1.Text))
            Form1.DownloadDirectory = textBox1.Text;
        }
        bool hasInjected = false;
        private void things_Tick(object sender, EventArgs e)
        {
            if (Process.GetProcessesByName("Minecraft.Windows").Length > 0 && checkBox2.Checked == true && hasInjected == false)
            {
                Form1.downloadAndOrInjectRecommended();
                hasInjected = true;
            }
            else
            {
                hasInjected = false;
            }
        }
        public static bool ischecked = false;
        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {
            ischecked = checkBox1.Checked;
            if (checkBox1.Checked == true) this.TopMost = true; else this.TopMost = false;
        }

        private void button4_Click(object sender, EventArgs e)
        {
            Form1.DownloadAll();
        }

        private void button3_Click(object sender, EventArgs e)
        {
            Form1.deleteAll();
        }
        public
const int WM_NCLBUTTONDOWN = 0xA1;
        public
        const int HT_CAPTION = 0x2;

        [DllImportAttribute("user32.dll")]
        public static extern int SendMessage(IntPtr hWnd, int Msg, int wParam, int lParam);
        [DllImportAttribute("user32.dll")]
        public static extern bool ReleaseCapture();

        private void panel1_MouseMove(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                ReleaseCapture();
                SendMessage(Handle, WM_NCLBUTTONDOWN, HT_CAPTION, 0);
            }
        }
    }
}
