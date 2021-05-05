using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using StreamLogger;
using StreamLogger.Api;
using StreamLogger.Api.EventArgs;
using StreamLogger.Api.MessageTypes;

namespace OpenVRNotificationPipeIntegration.EventHandlers
{
    public static class MessageEventHandler
    {
        public static void OnMessageEvent(ChatMessageEventArgs e)
        {
            Image notification = null;
            PipeStyle style = null;
            if (e.Message.Bits > 0)
            {
                if (File.Exists(Main.Instance.StyleManager.NotificationStyles.MessageWithBitsNotification.ImagePath))
                {
                    notification = CreateMessageWithBitsNotification(e.Message);
                    style = Main.Instance.StyleManager.NotificationStyles.MessageWithBitsNotification.PipeStyle;
                }
            }
            else
            {
                if (File.Exists(Main.Instance.StyleManager.NotificationStyles.MessageNotification.ImagePath))
                {
                    notification = CreateMessageNotification(e.Message);
                    style = Main.Instance.StyleManager.NotificationStyles.MessageNotification.PipeStyle;
                    
                }
            }

            if (notification is null || style is null) return;

            Main.Instance.PipeManager.SendImage(notification, style);
            string filePath = Path.Combine(Paths.Integrations, "test.png");
            notification.Save(filePath, ImageFormat.Png);
            Log.Info("Saving test image at " + filePath);
            notification.Dispose();
        }

        private static Image CreateMessageWithBitsNotification(ChatMessage msg)
        {
            Image b = Bitmap.FromFile(Path.Combine(Main.Instance.BaseDir, "message.png"));
            MessageWithBitsNotificationStyle style = Main.Instance.StyleManager.NotificationStyles.MessageWithBitsNotification;
            using (Graphics g = Graphics.FromImage(b))
            {
                if (Main.Instance.Config.ShowDebugOutline)
                {
                    g.DrawRectangle(new Pen(Color.Black, 2), style.NameBox.Position.ToRectangle());
                    g.DrawRectangle(new Pen(Color.Black, 2), style.MessageBox.Position.ToRectangle());
                    g.DrawRectangle(new Pen(Color.Black, 2), style.BitsBox.Position.ToRectangle());
                }

                if (!string.IsNullOrEmpty(msg.AvatarUrl))
                {
                    Image avatar = DownloadImage(msg.AvatarUrl);

                    if (style.AvatarBox.Rounded)
                    {
                        if (style.AvatarBox.Outlined) g.DrawEllipse(new Pen(ColorTranslator.FromHtml(msg.Color), 8), style.AvatarBox.Position.ToRectangle());
                        
                        GraphicsPath gp = new GraphicsPath();
                        gp.AddEllipse(style.AvatarBox.Position.ToRectangle());
                        Region rg = new Region(gp);
                        var oldClip = g.Clip;
                        g.SetClip(rg, CombineMode.Replace);
                        g.DrawImage(avatar, style.AvatarBox.Position.ToRectangle());
                        g.SetClip(oldClip, CombineMode.Replace);
                    }
                    else
                    {
                        if (style.AvatarBox.Outlined) g.DrawRectangle(new Pen(ColorTranslator.FromHtml(msg.Color), 8), style.AvatarBox.Position.ToRectangle());
                        g.DrawImage(avatar, style.AvatarBox.Position.ToRectangle());
                    }
                    
                    avatar.Dispose();
                }

                g.DrawTextBox(msg.DisplayName, style.NameBox, StringAlignment.Center);
                g.DrawTextBox(msg.MessageContent, style.MessageBox);
                g.DrawTextBox(msg.Bits.ToString(), style.BitsBox, StringAlignment.Center);
            }
            return b;
        }

        private static Image CreateMessageNotification(ChatMessage msg)
        {
            Image b = Bitmap.FromFile(Path.Combine(Main.Instance.BaseDir, "message.png"));
            MessageNotificationStyle style = Main.Instance.StyleManager.NotificationStyles.MessageNotification;
            using (Graphics g = Graphics.FromImage(b))
            {
                if (Main.Instance.Config.ShowDebugOutline)
                {
                    g.DrawRectangle(new Pen(Color.Black, 2), style.NameBox.Position.ToRectangle());
                    g.DrawRectangle(new Pen(Color.Black, 2), style.MessageBox.Position.ToRectangle());
                }

                if (!string.IsNullOrEmpty(msg.AvatarUrl))
                {
                    Image avatar = DownloadImage(msg.AvatarUrl);

                    if (style.AvatarBox.Rounded)
                    {
                        if (style.AvatarBox.Outlined) g.DrawEllipse(new Pen(ColorTranslator.FromHtml(msg.Color), 8), style.AvatarBox.Position.ToRectangle());
                        
                        GraphicsPath gp = new GraphicsPath();
                        gp.AddEllipse(style.AvatarBox.Position.ToRectangle());
                        Region rg = new Region(gp);
                        var oldClip = g.Clip;
                        g.SetClip(rg, CombineMode.Replace);
                        g.DrawImage(avatar, style.AvatarBox.Position.ToRectangle());
                        g.SetClip(oldClip, CombineMode.Replace);
                    }
                    else
                    {
                        if (style.AvatarBox.Outlined) g.DrawRectangle(new Pen(ColorTranslator.FromHtml(msg.Color), 8), style.AvatarBox.Position.ToRectangle());
                        g.DrawImage(avatar, style.AvatarBox.Position.ToRectangle());
                    }
                    
                    avatar.Dispose();
                }

                g.DrawTextBox(msg.DisplayName, style.NameBox, StringAlignment.Center);
                g.DrawTextBox(msg.MessageContent, style.MessageBox);
            }
            return b;
        }

        private static void DrawTextBox(this Graphics g, string text, TextBox textBox, StringAlignment lineAlignment = StringAlignment.Near)
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
                LineAlignment = lineAlignment
            };
            
            g.DrawString(text, font, new SolidBrush(fontColor), textBox.Position.ToRectangle(), fontFormat);
        }
        
        public static Image DownloadImage(string fromUrl)
        {
            using (System.Net.WebClient webClient = new System.Net.WebClient())
            {
                using (Stream stream = webClient.OpenRead(fromUrl))
                {
                    return Image.FromStream(stream);
                }
            }
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
    }
}