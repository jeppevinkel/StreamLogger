using System.Drawing;
using System.Drawing.Drawing2D;
using StreamLogger.Api.EventArgs;
using StreamLogger.Api.MessageTypes;

namespace OpenVRNotificationPipeIntegration.EventHandlers
{
    public static class RaidNotificationEventHandler
    {
        public static void OnRaidNotificationEventHandler(RaidNotificationEventArgs e)
        {
            if (!Main.MyConfig.EnabledEvents.RaidEvent) return;
            
            Image notification = null;
            PipeStyle style = null;
            
            notification = CreateMessageNotification(e.RaidNotification);
            style = Main.Instance.StyleManager.NotificationStyles.ReSubscription.PipeStyle;
            
            if (notification is null || style is null) return;
            Main.Instance.PipeManager.SendImage(notification, style);
            
            notification.Dispose();
        }
        
        private static Image CreateMessageNotification(RaidNotification raid)
        {
            RaidNotificationStyle style = Main.Instance.StyleManager.NotificationStyles.RaidNotification;
            Image b = Bitmap.FromFile(style.ImagePath);
            using (Graphics g = Graphics.FromImage(b))
            {
                if (Main.Instance.Config.ShowDebugOutline)
                {
                    g.DrawRectangle(new Pen(Color.Black, 2), style.NameBox.Position.ToRectangle());
                    g.DrawRectangle(new Pen(Color.Black, 2), style.MessageBox.Position.ToRectangle());
                }

                if (!string.IsNullOrEmpty(raid.AvatarUrl))
                {
                    Image avatar = Extensions.DownloadImage(raid.AvatarUrl);

                    if (style.AvatarBox.Rounded)
                    {
                        if (style.AvatarBox.Outlined) g.DrawEllipse(new Pen(ColorTranslator.FromHtml(raid.Color), 8), style.AvatarBox.Position.ToRectangle());
                        
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
                        if (style.AvatarBox.Outlined) g.DrawRectangle(new Pen(ColorTranslator.FromHtml(raid.Color), 8), style.AvatarBox.Position.ToRectangle());
                        g.DrawImage(avatar, style.AvatarBox.Position.ToRectangle());
                    }
                    
                    avatar.Dispose();
                }

                string nameDisplay = raid.RaidingChannel;

                g.DrawTextBox(nameDisplay, style.NameBox, StringAlignment.Center);
                g.DrawTextBox(raid.SystemMessage, style.MessageBox);
            }
            return b;
        }
    }
}