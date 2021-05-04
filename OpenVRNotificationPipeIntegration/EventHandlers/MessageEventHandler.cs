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
            Image notification;
            if (e.Message.Bits > 0)
            {
                notification = CreateMessageWithBitsNotification(e.Message);
            }
            else
            {
                notification = CreateMessageNotification(e.Message);
                string message = PipeManager.FormatMessage(notification.ToBase64String(ImageFormat.Png),
                    Main.Instance.StyleManager.NotificationStyles.MessageNotificationStyle.PipeStyle);
                Main.Instance.PipeManager.Send(message);
            }

            string filePath = Path.Combine(Paths.Integrations, "test.png");
            notification.Save(filePath, ImageFormat.Png);
            Log.Info("Saving test image at " + filePath);
            notification.Dispose();
        }

        private static Bitmap CreateMessageWithBitsNotification(ChatMessage msg)
        {
            return null;
        }

        private static Image CreateMessageNotification(ChatMessage msg)
        {
            Image b = Bitmap.FromFile(Path.Combine(Main.Instance.BaseDir, "message.png"));
            MessageNotificationStyle style = Main.Instance.StyleManager.NotificationStyles.MessageNotificationStyle;
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

                FontStyle nameFontStyle = (style.NameBox.Bold ? FontStyle.Bold : FontStyle.Regular) |
                                          (style.NameBox.Italic ? FontStyle.Italic : FontStyle.Regular) |
                                          (style.NameBox.Underline ? FontStyle.Underline : FontStyle.Regular) |
                                          (style.NameBox.StrikeThrough ? FontStyle.Strikeout : FontStyle.Regular);
                FontStyle messageFontStyle = (style.MessageBox.Bold ? FontStyle.Bold : FontStyle.Regular) |
                                             (style.MessageBox.Italic ? FontStyle.Italic : FontStyle.Regular) |
                                             (style.MessageBox.Underline ? FontStyle.Underline : FontStyle.Regular) |
                                             (style.MessageBox.StrikeThrough ? FontStyle.Strikeout : FontStyle.Regular);

                Color nameFontColor = Color.FromArgb(style.NameBox.FontColor.A, style.NameBox.FontColor.R,
                    style.NameBox.FontColor.G, style.NameBox.FontColor.B);
                Color messageFontColor = Color.FromArgb(style.MessageBox.FontColor.A, style.MessageBox.FontColor.R,
                    style.MessageBox.FontColor.G, style.MessageBox.FontColor.B);

                var nameFont = new Font(style.NameBox.FontName, style.NameBox.FontSize, nameFontStyle);
                var messageFont = new Font(style.MessageBox.FontName, style.MessageBox.FontSize, messageFontStyle);

                var nameFontFormat = new StringFormat
                {
                    Alignment = style.NameBox.Centered ? StringAlignment.Center : StringAlignment.Near,
                    LineAlignment = StringAlignment.Center
                };

                var messageFontFormat = new StringFormat
                {
                    Alignment = style.NameBox.Centered ? StringAlignment.Center : StringAlignment.Near,
                    LineAlignment = StringAlignment.Near
                };

                Rectangle nameBoxRect = new Rectangle(style.NameBox.Position.X + style.NameBox.Padding,
                    style.NameBox.Position.Y + style.NameBox.Padding, style.NameBox.Position.Width - style.NameBox.Padding * 2,
                    style.NameBox.Position.Height - style.NameBox.Padding * 2);

                Rectangle messageBoxRect = new Rectangle(style.MessageBox.Position.X + style.MessageBox.Padding,
                    style.MessageBox.Position.Y + style.MessageBox.Padding,
                    style.MessageBox.Position.Width - style.MessageBox.Padding * 2,
                    style.MessageBox.Position.Height - style.MessageBox.Padding * 2);

                g.DrawString(msg.DisplayName, nameFont, new SolidBrush(ColorTranslator.FromHtml(msg.Color)), nameBoxRect, nameFontFormat);
                g.DrawString(msg.MessageContent, messageFont,
                    new SolidBrush(messageFontColor), messageBoxRect, messageFontFormat);
            }
            return b;
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