using System.Drawing;
using StreamLogger.Api.EventArgs;
using StreamLogger.Api.MessageTypes;

namespace OpenVRNotificationPipeIntegration.EventHandlers
{
    public static class FollowEventHandler
    {
        public static void OnFollowEvent(FollowEventArgs e)
        {
            Image notification = null;
            PipeStyle style = null;
            
            notification = CreateFollowNotification(e.Follow);
            style = Main.Instance.StyleManager.NotificationStyles.FollowNotification.PipeStyle;
            
            if (notification is null || style is null) return;
            Main.Instance.PipeManager.SendImage(notification, style);
            
            notification.Dispose();
        }

        private static Image CreateFollowNotification(Follow follow)
        {
            FollowNotificationStyle style = Main.Instance.StyleManager.NotificationStyles.FollowNotification;
            Image b = Bitmap.FromFile(style.ImagePath);
            using (Graphics g = Graphics.FromImage(b))
            {
                if (Main.Instance.Config.ShowDebugOutline)
                {
                    g.DrawRectangle(new Pen(Color.Black, 2), style.MessageBox.Position.ToRectangle());
                }
                
                g.DrawTextBox(style.FollowMessage.Replace("{displayName}", follow.DisplayName), style.MessageBox);
            }

            return b;
        }
    }
}