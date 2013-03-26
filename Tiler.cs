using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using System.ComponentModel;

namespace LeafletPano
{
    public class Tiler
    {
        public string ImageFile { get; private set; }

        public int OrgLevel { get; private set; }

        public int MaxLevel { get; set; }

        public int MinLevel { get; set; }

        public Image SourceImage { get; private set; }

        public Color BackgroundColor { get; set; }

        public Tiler(string imageFile)
        {
            this.ImageFile = imageFile;

            SourceImage = Image.FromFile(imageFile);

            int width = SourceImage.Width;
            int height = SourceImage.Height;
            int size = Math.Max(width, height);
            double numTiles = size / 256.0;

            OrgLevel = (int)Math.Ceiling(Math.Log(numTiles, 2)); // the level where the image is 100%
            MinLevel = 0; // the minimum tile level
            MaxLevel = OrgLevel; // the maximum tile level
        }

        public event EventHandler<SendFeedbackEventArgs> SendFeedback;
        public event EventHandler<CompletedEventArgs> Completed;

        BackgroundWorker bw;
        public void CreateTiles()
        {
            bw = new BackgroundWorker();
            bw.DoWork += new DoWorkEventHandler(bw_DoWork);
            bw.WorkerReportsProgress = true;

            bw.ProgressChanged += new ProgressChangedEventHandler(bw_ProgressChanged);
            bw.RunWorkerCompleted += new RunWorkerCompletedEventHandler(bw_RunWorkerCompleted);
            bw.RunWorkerAsync();
        }

        void bw_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            if (Completed != null)
                Completed(this, new CompletedEventArgs { HtmlFile = e.Result as string });
        }

        void bw_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            if (SendFeedback != null)
                SendFeedback(this, e.UserState as SendFeedbackEventArgs);
        }  

        void bw_DoWork(object sender, DoWorkEventArgs e)
        {
            string directoryName = Path.GetDirectoryName(ImageFile);
            string imageName = Path.GetFileNameWithoutExtension(ImageFile);

            Directory.CreateDirectory(directoryName + "/" + imageName);
      
            int width = SourceImage.Width;
            int height = SourceImage.Height;
            int size = Math.Max(width, height);
            double numTiles = size / 256.0;
             
            for (int level = MinLevel; level <= MaxLevel; level++)
            {
                int logTileSize = (OrgLevel > level) ? 256 << (OrgLevel - level) : 256 >> (level - OrgLevel);
                int numTilesX = (width % logTileSize == 0) ? width / logTileSize : width / logTileSize + 1;
                int numTilesY = (height % logTileSize == 0) ? height / logTileSize : height / logTileSize + 1;

                for (int tx = 0; tx < numTilesX; tx++)
                    for (int ty = 0; ty < numTilesY; ty++)
                    {
                        using(var tileImg = new Bitmap(256, 256))
                        using(var graphcis = Graphics.FromImage(tileImg))
                        {
                            graphcis.FillRectangle(new SolidBrush(BackgroundColor), 0, 0, 256, 256);
                            graphcis.DrawImage(SourceImage, new Rectangle(0, 0, 256, 256),
                                new Rectangle(tx * logTileSize, ty * logTileSize, logTileSize, logTileSize),
                                GraphicsUnit.Pixel);

                            tileImg.Save(string.Format("{0}/{1}/{2}-{3}-{4}.jpg", directoryName, imageName, level, tx, ty),
                                System.Drawing.Imaging.ImageFormat.Jpeg);
                          
                            bw.ReportProgress(0, new SendFeedbackEventArgs { Level = level, X = tx, Y = ty, Image = tileImg.Clone() as Bitmap});
                        }
                    }
            }

            string template;
            using (var reader = new StreamReader(Application.StartupPath + "\\Template.html"))
            {
                template = reader.ReadToEnd();
                reader.Close();
            }

            template = template.Replace("//image//", imageName);

            template = template.Replace("//width//", width.ToString());
            template = template.Replace("//height//", height.ToString());

            template = template.Replace("//maxLevel//", MaxLevel.ToString());
            template = template.Replace("//minLevel//", MinLevel.ToString());
            template = template.Replace("//orgLevel//", OrgLevel.ToString());
           
            template = template.Replace("//bgColor//", ColorTranslator.ToHtml(BackgroundColor));

            string htmlFile = directoryName + "/" + imageName + ".html";

            using (var writer = new StreamWriter(htmlFile))
            {
                writer.Write(template);
                writer.Close();
            }

            e.Result = htmlFile;
        }

        public void Dispose()
        {
            SourceImage.Dispose();
        }
    }

    public class SendFeedbackEventArgs : EventArgs
    {
        public int Level { get; set; }
        public int X { get; set; }
        public int Y { get; set; }
        public Bitmap Image { get; set; }
    }

    public class CompletedEventArgs : EventArgs
    {
        public string HtmlFile { get; set; }
    }
}
