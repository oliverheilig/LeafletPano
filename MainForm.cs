using System;
using System.Drawing;
using System.Windows.Forms;
using System.IO;
using LeafletPano.Properties;

namespace LeafletPano
{
    public partial class MainForm : Form
    {
        private Tiler tiler;

        public MainForm()
        {
            InitializeComponent();

            colorPanel.BackColor = Settings.Default.BackgroundColor;
            if (string.IsNullOrEmpty(Properties.Settings.Default.InitialPath))
                Initfile(Application.StartupPath + "\\DemoData\\Rothenburg.jpg");
            else
                Initfile(Properties.Settings.Default.InitialPath);
        }

        private void startButton_Click(object sender, EventArgs e)
        {
            tiler.MinLevel = (int)numericUpDown2.Value;
            tiler.MaxLevel = (int)numericUpDown3.Value;
            tiler.BackgroundColor = colorPanel.BackColor;
            startButton.Enabled = false;
            tiler.CreateTiles();
        }

        private void textBox1_DragDrop(object sender, DragEventArgs e)
        {
            string[] s = (string[])e.Data.GetData(DataFormats.FileDrop, false);
            if (s.Length == 1)
                Initfile(s[0]);
        }

        private void Initfile(string filePath)
        {
            textBox1.Text = filePath;

            if (tiler != null)
            {
                tiler.SendFeedback -= new EventHandler<SendFeedbackEventArgs>(tiler_SendFeedback);
                tiler.Completed -= new EventHandler<CompletedEventArgs>(tiler_Completed);
                tiler.Dispose();
                tiler = null;
            }

            tiler = new Tiler(textBox1.Text);
            tiler.SendFeedback += new EventHandler<SendFeedbackEventArgs>(tiler_SendFeedback);
            tiler.Completed += new EventHandler<CompletedEventArgs>(tiler_Completed);

            label5.Text = string.Format("{0}x{1}", tiler.SourceImage.Width, tiler.SourceImage.Height);

            var tileImg = new Bitmap(256, 256);
            using (var graphcis = Graphics.FromImage(tileImg))
            {
                int width, height;
                if (tiler.SourceImage.Width > tiler.SourceImage.Height)
                { 
                    width = 256; 
                    height = 256 * tiler.SourceImage.Height / tiler.SourceImage.Width; 
                }
                else
                { 
                    height = 256;
                    width = 256 * tiler.SourceImage.Width / tiler.SourceImage.Height; 
                }

                graphcis.FillRectangle(new SolidBrush(colorPanel.BackColor), 0, 0, 256, 256);
                graphcis.DrawImage(tiler.SourceImage, new Rectangle(0, 0, width, height),
                    new Rectangle(0, 0, tiler.SourceImage.Width, tiler.SourceImage.Height),
                    GraphicsUnit.Pixel);

            }

            pictureBox2.Image = tileImg;

            numericUpDown1.Value = tiler.OrgLevel;
            numericUpDown2.Value = tiler.MinLevel;
            numericUpDown3.Value = tiler.MaxLevel;
        }

        void tiler_Completed(object sender, CompletedEventArgs e)
        {
            System.Diagnostics.Process.Start(e.HtmlFile);

            startButton.Enabled = true;
        }

        void tiler_SendFeedback(object sender, SendFeedbackEventArgs e)
        {
            label4.Text = string.Format("{0} / {1} / {2}", e.Level, e.X, e.Y);
            pictureBox1.Image = e.Image;
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

            Initfile(this.textBox1.Text);
        }

        private void MainForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            Settings.Default.BackgroundColor = colorPanel.BackColor;
            Settings.Default.InitialPath = textBox1.Text;
            Settings.Default.Save();
        }

        private void textBox1_Leave(object sender, EventArgs e)
        {
            Initfile(this.textBox1.Text);
        }
    }
}
