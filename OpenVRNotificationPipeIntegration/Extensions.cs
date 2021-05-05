using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Net;

namespace OpenVRNotificationPipeIntegration
{
    public static class Extensions
    {
        public static void DrawTextBox(this Graphics g, string text, TextBox textBox, StringAlignment lineAlignment = StringAlignment.Near)
        {
            FontStyle fontStyle = (textBox.Bold ? FontStyle.Bold : FontStyle.Regular) |
                                  (textBox.Italic ? FontStyle.Italic : FontStyle.Regular) |
                                  (textBox.Underline ? FontStyle.Underline : FontStyle.Regular) |
                                  (textBox.StrikeThrough ? FontStyle.Strikeout : FontStyle.Regular);
            
            Color fontColor = Color.FromArgb(textBox.FontColor.A, textBox.FontColor.R,
                textBox.FontColor.G, textBox.FontColor.B);
            
            var font = new Font(textBox.FontName, textBox.FontSize, fontStyle);
            
            var fontFormat = new StringFormat
            {
                Alignment = textBox.Centered ? StringAlignment.Center : StringAlignment.Near,
                LineAlignment = lineAlignment,
                Trimming = StringTrimming.EllipsisCharacter
            };
            
            g.DrawString(text, font, new SolidBrush(fontColor), textBox.Position.ToRectangle(), fontFormat);
        }
        
        public static string ToBase64String(this Image img, ImageFormat imageFormat)
        {
            string base64String = string.Empty;

 
            MemoryStream memoryStream = new MemoryStream();
            img.Save(memoryStream, imageFormat);


            memoryStream.Position = 0;
            byte[] byteBuffer = memoryStream.ToArray();

  
            memoryStream.Close();

  
            base64String = Convert.ToBase64String(byteBuffer);
            byteBuffer = null;

  
            return base64String;
        }

        public static Image DownloadImage(string fromUrl)
        {
            using (WebClient webClient = new WebClient())
            {
                using (Stream stream = webClient.OpenRead(fromUrl))
                {
                    return Image.FromStream(stream);
                }
            }
        }
    }
}