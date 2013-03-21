using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;
using System.IO;
using System.Windows.Forms;

namespace LeafletPano
{
    public class Tiler
    {
        public static string CreateTiles(string imageFile, Color backgroundColor)
        {
            string directoryName = Path.GetDirectoryName(imageFile);
            string imageName = Path.GetFileNameWithoutExtension(imageFile);

            Directory.CreateDirectory(directoryName + "/" + imageName);

            var srcImg = Image.FromFile(imageFile);

            int width = srcImg.Width;
            int height = srcImg.Height;
            int size = Math.Max(width, height);
            double numTiles = size / 256.0;

            int orgLevel = (int)Math.Ceiling(Math.Log(numTiles, 2)); // the level where the image is 100%
            int minLevel = 0; // the minimum tile level
            int maxLevel = orgLevel; // the maximum tile level

            for (int level = minLevel; level <= maxLevel; level++)
            {
                int logTileSize = (orgLevel > level) ? 256 << (orgLevel - level) : 256 >> (level - orgLevel);
                int numTilesX = (width % logTileSize == 0) ? width / logTileSize : width / logTileSize + 1;
                int numTilesY = (height % logTileSize == 0) ? height / logTileSize : height / logTileSize + 1;

                for (int tx = 0; tx < numTilesX; tx++)
                    for (int ty = 0; ty < numTilesY; ty++)
                    {
                        using (var tileImg = new Bitmap(256, 256))
                        using (var graphcis = Graphics.FromImage(tileImg))
                        {
                            graphcis.FillRectangle(new SolidBrush(backgroundColor), 0, 0, 256, 256);
                            graphcis.DrawImage(srcImg, new Rectangle(0, 0, 256, 256),
                                new Rectangle(tx * logTileSize, ty * logTileSize, logTileSize, logTileSize),
                                GraphicsUnit.Pixel);

                            tileImg.Save(string.Format("{0}/{1}/{2}-{3}-{4}.jpg", directoryName, imageName, level, tx, ty),
                                System.Drawing.Imaging.ImageFormat.Jpeg);
                        }
                    }
            }
            srcImg.Dispose();

            string template;
            using (var reader = new StreamReader(Application.StartupPath + "\\Template.html"))
            {
                template = reader.ReadToEnd();
                reader.Close();
            }

            template = template.Replace("//image//", imageName);

            template = template.Replace("//width//", width.ToString());
            template = template.Replace("//height//", height.ToString());

            template = template.Replace("//maxLevel//", maxLevel.ToString());
            template = template.Replace("//minLevel//", minLevel.ToString());
            template = template.Replace("//orgLevel//", orgLevel.ToString());
           
            template = template.Replace("//bgColor//", ColorTranslator.ToHtml(backgroundColor));

            string htmlFile = directoryName + "/" + imageName + ".html";

            using (var writer = new StreamWriter(htmlFile))
            {
                writer.Write(template);
                writer.Close();
            }

            return htmlFile;
        }
    }
}
