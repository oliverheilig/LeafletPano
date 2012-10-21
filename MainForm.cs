using System;
using System.Drawing;
using System.Windows.Forms;
using System.IO;
using LeafletPano.Properties;

namespace LeafletPano
{
    public partial class MainForm : Form
    {
        public MainForm()
        {
            InitializeComponent();

            colorPanel.BackColor = Settings.Default.BackgroundColor;
            if (string.IsNullOrEmpty(Properties.Settings.Default.InitialPath))
                textBox1.Text = Application.StartupPath + "\\DemoData\\Rothenburg.jpg";
            else
                textBox1.Text = Properties.Settings.Default.InitialPath;
        }

        private void startButton_Click(object sender, EventArgs e)
        {
            string htmlFile = Tiler.CreateTiles(textBox1.Text, colorPanel.BackColor);

            System.Diagnostics.Process.Start(htmlFile);
        }

        private void textBox1_DragDrop(object sender, DragEventArgs e)
        {
            string[] s = (string[])e.Data.GetData(DataFormats.FileDrop, false);
            if(s.Length == 1)
                textBox1.Text = s[0];
        }

        private void textBox1_DragEnter(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
                e.Effect = DragDropEffects.All;
            else
                e.Effect = DragDropEffects.None;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            var cd = new ColorDialog();
            cd.Color = Properties.Settings.Default.BackgroundColor;
            if (cd.ShowDialog() == DialogResult.OK)
                colorPanel.BackColor = cd.Color;
    
            cd.Dispose();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            var fd = new OpenFileDialog();
            if (fd.ShowDialog() == DialogResult.OK)
                this.textBox1.Text = fd.FileName;

            fd.Dispose();
        }

        private void MainForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            Settings.Default.BackgroundColor = colorPanel.BackColor;
            Settings.Default.InitialPath = textBox1.Text;
            Settings.Default.Save();
        }
    }
}
