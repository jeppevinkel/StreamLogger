using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
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
                if (Main.Instance.Config.EnabledEvents.MessageWithBitsEvent && File.Exists(Main.Instance.StyleManager.NotificationStyles.MessageWithBitsNotification.ImagePath))
                {
                    notification = CreateMessageWithBitsNotification(e.Message);
                    style = Main.Instance.StyleManager.NotificationStyles.MessageWithBitsNotification.PipeStyle;
                }
            }
            else
            {
                if (Main.Instance.Config.EnabledEvents.MessageEvent && File.Exists(Main.Instance.StyleManager.NotificationStyles.MessageNotification.ImagePath))
                {
                    notification = CreateMessageNotification(e.Message);
                    style = Main.Instance.StyleManager.NotificationStyles.MessageNotification.PipeStyle;
                    
                }
            }

            if (notification is null || style is null) return;

            Main.Instance.PipeManager.SendImage(notification, style);
            notification.Dispose();
        }

        private static Image CreateMessageWithBitsNotification(ChatMessage msg)
        {
            MessageWithBitsNotificationStyle style = Main.Instance.StyleManager.NotificationStyles.MessageWithBitsNotification;
            Image b = Bitmap.FromFile(style.ImagePath);
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
                    Image avatar = Extensions.DownloadImage(msg.AvatarUrl);

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

                string nameDisplay = msg.DisplayName;
                if (Main.Instance.Config.AppendChannel) nameDisplay += $" [{msg.Channel}]";

                g.DrawTextBox(nameDisplay, style.NameBox, StringAlignment.Center);
                g.DrawTextBox(msg.MessageContent, style.MessageBox);
                g.DrawTextBox(msg.Bits.ToString(), style.BitsBox, StringAlignment.Center);
            }
            return b;
        }

        private static Image CreateMessageNotification(ChatMessage msg)
        {
            MessageNotificationStyle style = Main.Instance.StyleManager.NotificationStyles.MessageNotification;
            Image b = Bitmap.FromFile(style.ImagePath);
            using (Graphics g = Graphics.FromImage(b))
            {
                if (Main.Instance.Config.ShowDebugOutline)
                {
                    g.DrawRectangle(new Pen(Color.Black, 2), style.NameBox.Position.ToRectangle());
                    g.DrawRectangle(new Pen(Color.Black, 2), style.MessageBox.Position.ToRectangle());
                }

                if (!string.IsNullOrEmpty(msg.AvatarUrl))
                {
                    Image avatar = Extensions.DownloadImage(msg.AvatarUrl);

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

                string nameDisplay = msg.DisplayName;
                if (Main.Instance.Config.AppendChannel) nameDisplay += $" [{msg.Channel}]";

                g.DrawTextBox(nameDisplay, style.NameBox, StringAlignment.Center);
                g.DrawTextBox(msg.MessageContent, style.MessageBox);
            }
            return b;
        }
    }
}