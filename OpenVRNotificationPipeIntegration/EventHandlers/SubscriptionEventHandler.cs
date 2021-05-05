using System.Drawing;
using System.Drawing.Drawing2D;
using StreamLogger.Api.EventArgs;
using StreamLogger.Api.MessageTypes;

namespace OpenVRNotificationPipeIntegration.EventHandlers
{
    public static class SubscriptionEventHandler
    {
        public static void OnReSubscriptionEvent(ReSubscriptionEventArgs e)
        {
            Image notification = null;
            PipeStyle style = null;
            
            notification = CreateReSubscriptionNotification(e.Subscription);
            style = Main.Instance.StyleManager.NotificationStyles.ReSubscription.PipeStyle;
            
            if (notification is null || style is null) return;
            Main.Instance.PipeManager.SendImage(notification, style);
            
            notification.Dispose();

            if (!string.IsNullOrEmpty(e.Subscription.MessageContent))
            {
                MessageEventHandler.OnMessageEvent(new ChatMessageEventArgs(e.Subscription.ToChatMessage()));
            }
        }
        
        public static void OnNewSubscriptionEvent(NewSubscriptionEventArgs e)
        {
            Image notification = null;
            PipeStyle style = null;
            
            notification = CreateNewSubscriptionNotification(e.Subscription);
            style = Main.Instance.StyleManager.NotificationStyles.NewSubscription.PipeStyle;
            
            if (notification is null || style is null) return;
            Main.Instance.PipeManager.SendImage(notification, style);
            
            notification.Dispose();

            if (!string.IsNullOrEmpty(e.Subscription.MessageContent))
            {
                MessageEventHandler.OnMessageEvent(new ChatMessageEventArgs(e.Subscription.ToChatMessage()));
            }
        }
        
        internal static Image CreateReSubscriptionNotification(Subscription subscription)
        {
            ReSubscriptionNotificationStyle style = Main.Instance.StyleManager.NotificationStyles.ReSubscription;
            Image b = Bitmap.FromFile(style.ImagePath);
            using (Graphics g = Graphics.FromImage(b))
            {
                if (Main.Instance.Config.ShowDebugOutline)
                {
                    g.DrawRectangle(new Pen(Color.Black, 2), style.NameBox.Position.ToRectangle());
                    g.DrawRectangle(new Pen(Color.Black, 2), style.MessageBox.Position.ToRectangle());
                }

                if (!string.IsNullOrEmpty(subscription.AvatarUrl))
                {
                    Image avatar = Extensions.DownloadImage(subscription.AvatarUrl);

                    if (style.AvatarBox.Rounded)
                    {
                        if (style.AvatarBox.Outlined) g.DrawEllipse(new Pen(ColorTranslator.FromHtml(subscription.Color), 8), style.AvatarBox.Position.ToRectangle());
                        
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
                        if (style.AvatarBox.Outlined) g.DrawRectangle(new Pen(ColorTranslator.FromHtml(subscription.Color), 8), style.AvatarBox.Position.ToRectangle());
                        g.DrawImage(avatar, style.AvatarBox.Position.ToRectangle());
                    }
                    
                    avatar.Dispose();
                }
                
                string nameDisplay = subscription.DisplayName;
                if (Main.Instance.Config.AppendChannel) nameDisplay += $" [{subscription.Channel}]";

                g.DrawTextBox(nameDisplay, style.NameBox, StringAlignment.Center);
                g.DrawTextBox(subscription.SystemMessage, style.MessageBox);
            }
            return b;
        }
        
        internal static Image CreateNewSubscriptionNotification(Subscription subscription)
        {
            NewSubscriptionNotificationStyle style = Main.Instance.StyleManager.NotificationStyles.NewSubscription;
            Image b = Bitmap.FromFile(style.ImagePath);
            using (Graphics g = Graphics.FromImage(b))
            {
                if (Main.Instance.Config.ShowDebugOutline)
                {
                    g.DrawRectangle(new Pen(Color.Black, 2), style.NameBox.Position.ToRectangle());
                    g.DrawRectangle(new Pen(Color.Black, 2), style.MessageBox.Position.ToRectangle());
                }

                if (!string.IsNullOrEmpty(subscription.AvatarUrl))
                {
                    Image avatar = Extensions.DownloadImage(subscription.AvatarUrl);

                    if (style.AvatarBox.Rounded)
                    {
                        if (style.AvatarBox.Outlined) g.DrawEllipse(new Pen(ColorTranslator.FromHtml(subscription.Color), 8), style.AvatarBox.Position.ToRectangle());
                        
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
                        if (style.AvatarBox.Outlined) g.DrawRectangle(new Pen(ColorTranslator.FromHtml(subscription.Color), 8), style.AvatarBox.Position.ToRectangle());
                        g.DrawImage(avatar, style.AvatarBox.Position.ToRectangle());
                    }
                    
                    avatar.Dispose();
                }
                
                string nameDisplay = subscription.DisplayName;
                if (Main.Instance.Config.AppendChannel) nameDisplay += $" [{subscription.Channel}]";

                g.DrawTextBox(nameDisplay, style.NameBox, StringAlignment.Center);
                g.DrawTextBox(subscription.SystemMessage, style.MessageBox);
            }
            return b;
        }

        public static ChatMessage ToChatMessage(this Subscription s)
        {
            var msg = new ChatMessage(s.Badges, s.Color, s.DisplayName, s.Emotes, s.Flags, s.Mod, s.Subscriber,
                false, s.Timestamp, s.UserType, s.Username, s.UserId, s.Channel, 0, s.AvatarUrl, s.MessageContent);

            return msg;
        }
    }
}