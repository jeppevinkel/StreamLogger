using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;
using StreamLogger;
using StreamLogger.Api;

namespace OpenVRNotificationPipeIntegration
{
    public class StyleManager
    {
        public string BaseFolder;
        public NotificationStyles NotificationStyles = new NotificationStyles();

        private readonly JsonSerializerOptions _options = new JsonSerializerOptions
        {
            WriteIndented = true,
        };
        
        public StyleManager(string baseFolder)
        {
            BaseFolder = Path.Combine(baseFolder, "Notifications");
            LoadStyles();
        }

        public void LoadStyles()
        {
            NotificationStyles.MessageNotification = LoadStyle(NotificationStyles.MessageNotification);
            NotificationStyles.MessageWithBitsNotification = LoadStyle(NotificationStyles.MessageWithBitsNotification);
            NotificationStyles.FollowNotification = LoadStyle(NotificationStyles.FollowNotification);
            NotificationStyles.ReSubscription = LoadStyle(NotificationStyles.ReSubscription);
            NotificationStyles.NewSubscription = LoadStyle(NotificationStyles.NewSubscription);
            NotificationStyles.RaidNotification = LoadStyle(NotificationStyles.RaidNotification);
        }

        private void SaveStyle<T>(T style) where T : IBaseNotificationStyle
        {
            Log.Debug("Saving " + style.BasePath);
            if (!Directory.Exists(style.BasePath)) Directory.CreateDirectory(style.BasePath);
            string stylePath = Path.Combine(style.BasePath, "style.json");
            File.WriteAllText(stylePath, JsonSerializer.Serialize(style, _options));
        }

        private static T LoadStyle<T>(string path) where T : IBaseNotificationStyle
        {
            return JsonSerializer.Deserialize<T>(File.ReadAllText(path));
        }

        private T LoadStyle<T>(T style) where T : IBaseNotificationStyle, new()
        {
            if (CheckFileExistence(style.BasePath, new T()))
            {
                bool errorsOccurred = false;
                string stylePath = Path.Combine(style.BasePath, "style.json");
                try
                {
                    style = LoadStyle<T>(stylePath);
                }
                catch (Exception e)
                {
                    errorsOccurred = true;
                    Log.Error($"Something went wrong while reading notification style for {style.GetType()}. Please validate file structure.\n{e}");
                }

                if (!errorsOccurred)
                {
                    SaveStyle(style);
                }
            }

            return style;
        }

        private bool CheckFileExistence(string dir, object fallback)
        {
            string stylePath = Path.Combine(dir, "style.json");
            if (!Directory.Exists(dir))
            {
                Directory.CreateDirectory(dir);
                File.WriteAllText(stylePath, JsonSerializer.Serialize(fallback, _options));
                return false;
            }

            if (File.Exists(stylePath)) return true;
            File.WriteAllText(stylePath, JsonSerializer.Serialize(fallback, _options));
            return false;
        }
    }

    public class NotificationStyles
    {
        public const int Length = 6;

        public IBaseNotificationStyle this[int index]
        {
            get
            {
                return index switch
                {
                    0 => MessageNotification,
                    1 => MessageWithBitsNotification,
                    2 => FollowNotification,
                    3 => ReSubscription,
                    4 => NewSubscription,
                    5 => RaidNotification,
                    _ => throw new IndexOutOfRangeException("You are out of bounds for the notification index!")
                };
            }
            set
            {
                switch (index)
                {
                    case 0:
                        MessageNotification = (MessageNotificationStyle)value;
                        break;
                    case 1:
                        MessageWithBitsNotification = (MessageWithBitsNotificationStyle)value;
                        break;
                    case 2:
                        FollowNotification = (FollowNotificationStyle)value;
                        break;
                    case 3:
                        ReSubscription = (ReSubscriptionNotificationStyle)value;
                        break;
                    case 4:
                        NewSubscription = (NewSubscriptionNotificationStyle)value;
                        break;
                    case 5:
                        RaidNotification = (RaidNotificationStyle)value;
                        break;
                    default:
                        throw new IndexOutOfRangeException("You are out of bounds for the notification index!");
                }
            }
        }
        public IBaseNotificationStyle this[string notification]
        {
            get
            {
                return notification switch
                {
                    "Message" => MessageNotification,
                    "MessageWithBits" => MessageWithBitsNotification,
                    "Follow" => FollowNotification,
                    "ReSubscription" => ReSubscription,
                    "NewSubscription" => NewSubscription,
                    "Raid" => RaidNotification,
                    _ => throw new KeyNotFoundException("The requested notification couldn't be found!")
                };
            }
            set
            {
                switch (notification)
                {
                    case "Message":
                        MessageNotification = (MessageNotificationStyle)value;
                        break;
                    case "MessageWithBits":
                        MessageWithBitsNotification = (MessageWithBitsNotificationStyle)value;
                        break;
                    case "Follow":
                        FollowNotification = (FollowNotificationStyle)value;
                        break;
                    case "ReSubscription":
                        ReSubscription = (ReSubscriptionNotificationStyle)value;
                        break;
                    case "NewSubscription":
                        NewSubscription = (NewSubscriptionNotificationStyle)value;
                        break;
                    case "Raid":
                        RaidNotification = (RaidNotificationStyle)value;
                        break;
                    default:
                        throw new KeyNotFoundException("The requested notification couldn't be found!");
                }
            }
        }

        public MessageNotificationStyle MessageNotification { get; set; } = new();
        public MessageWithBitsNotificationStyle MessageWithBitsNotification { get; set; } = new();
        public FollowNotificationStyle FollowNotification { get; set; } = new();
        public ReSubscriptionNotificationStyle ReSubscription { get; set; } = new();
        public NewSubscriptionNotificationStyle NewSubscription { get; set; } = new();
        public RaidNotificationStyle RaidNotification { get; set; } = new();
    }

    public interface IBaseNotificationStyle
    {
        [JsonIgnore] public string BasePath { get; }
        
        public string ImagePath { get; set; }
    }

    public class MessageNotificationStyle : IBaseNotificationStyle
    {
        [JsonIgnore] public string BasePath { get; }
        public string ImagePath { get; set; }
        public TextBox NameBox { get; set; } = new TextBox();
        public TextBox MessageBox { get; set; } = new TextBox();
        public AvatarBox AvatarBox { get; set; } = new AvatarBox();
        public PipeStyle PipeStyle { get; set; } = new PipeStyle();

        public MessageNotificationStyle()
        {
            if (string.IsNullOrEmpty(Main.Instance.CurrentGame))
            {
                BasePath = Path.Combine(Path.Combine(Path.Combine(Paths.Integrations, Main.Instance.Name), "Notifications"),
                    "Message");
            }
            else
            {
                BasePath = Path.Combine(Path.Combine(Path.Combine(Path.Combine(Path.Combine(Paths.Integrations, Main.Instance.Name), "Games"), Main.Instance.CurrentGame), "Notifications"),
                    "Message");
            }
            
            ImagePath = Path.Combine(BasePath, "bg.png");
        }
        public MessageNotificationStyle(string gameId)
        {
            if (string.IsNullOrEmpty(Main.Instance.CurrentGame))
            {
                BasePath = Path.Combine(Path.Combine(Path.Combine(Paths.Integrations, Main.Instance.Name), "Notifications"),
                    "Message");
            }
            else
            {
                BasePath = Path.Combine(Path.Combine(Path.Combine(Path.Combine(Path.Combine(Paths.Integrations, Main.Instance.Name), "Games"), Main.Instance.CurrentGame), "Notifications"),
                    "Message");
            }
            
            ImagePath = Path.Combine(BasePath, "bg.png");
        }
    }

    public class MessageWithBitsNotificationStyle : IBaseNotificationStyle
    {
        [JsonIgnore] public string BasePath { get; }
        public string ImagePath { get; set; }
        public TextBox NameBox { get; set; } = new TextBox();
        public TextBox MessageBox { get; set; } = new TextBox();
        public TextBox BitsBox { get; set; } = new TextBox();
        public AvatarBox AvatarBox { get; set; } = new AvatarBox();
        public PipeStyle PipeStyle { get; set; } = new PipeStyle();

        public MessageWithBitsNotificationStyle()
        {
            if (string.IsNullOrEmpty(Main.Instance.CurrentGame))
            {
                BasePath = Path.Combine(Path.Combine(Path.Combine(Paths.Integrations, Main.Instance.Name), "Notifications"),
                    "MessageWithBits");
            }
            else
            {
                BasePath = Path.Combine(Path.Combine(Path.Combine(Path.Combine(Path.Combine(Paths.Integrations, Main.Instance.Name), "Games"), Main.Instance.CurrentGame), "Notifications"),
                    "MessageWithBits");
            }
            
            ImagePath = Path.Combine(BasePath, "bg.png");
        }
        public MessageWithBitsNotificationStyle(string gameId)
        {
            BasePath = Path.Combine(Path.Combine(Path.Combine(Path.Combine(Path.Combine(Paths.Integrations, Main.Instance.Name), "Games"), gameId), "Notifications"),
                "MessageWithBits");
            ImagePath = Path.Combine(BasePath, "bg.png");
        }
    }

    public class FollowNotificationStyle : IBaseNotificationStyle
    {
        [JsonIgnore] public string BasePath { get; set; }
        public string ImagePath { get; set; }
        public string FollowMessage { get; set; } = "{displayName} is now following!";
        public TextBox MessageBox { get; set; } = new TextBox();
        public PipeStyle PipeStyle { get; set; } = new PipeStyle();

        public FollowNotificationStyle()
        {
            if (string.IsNullOrEmpty(Main.Instance.CurrentGame))
            {
                BasePath = Path.Combine(Path.Combine(Path.Combine(Paths.Integrations, Main.Instance.Name), "Notifications"),
                    "Follow");
            }
            else
            {
                BasePath = Path.Combine(Path.Combine(Path.Combine(Path.Combine(Path.Combine(Paths.Integrations, Main.Instance.Name), "Games"), Main.Instance.CurrentGame), "Notifications"),
                    "Follow");
            }
            
            ImagePath = Path.Combine(BasePath, "bg.png");
        }
        public FollowNotificationStyle(string gameId)
        {
            BasePath = Path.Combine(Path.Combine(Path.Combine(Path.Combine(Path.Combine(Paths.Integrations, Main.Instance.Name), "Games"), gameId), "Notifications"),
                "Follow");
            ImagePath = Path.Combine(BasePath, "bg.png");
        }
    }

    public class ReSubscriptionNotificationStyle : IBaseNotificationStyle
    {
        [JsonIgnore] public string BasePath { get; set; }
        public string ImagePath { get; set; }
        public TextBox NameBox { get; set; } = new TextBox();
        public TextBox MessageBox { get; set; } = new TextBox();
        public AvatarBox AvatarBox { get; set; } = new AvatarBox();
        public PipeStyle PipeStyle { get; set; } = new PipeStyle();

        public ReSubscriptionNotificationStyle()
        {
            
            if (string.IsNullOrEmpty(Main.Instance.CurrentGame))
            {
                BasePath = Path.Combine(Path.Combine(Path.Combine(Paths.Integrations, Main.Instance.Name), "Notifications"),
                    "ReSubscription");
            }
            else
            {
                BasePath = Path.Combine(Path.Combine(Path.Combine(Path.Combine(Path.Combine(Paths.Integrations, Main.Instance.Name), "Games"), Main.Instance.CurrentGame), "Notifications"),
                    "ReSubscription");
            }
            
            ImagePath = Path.Combine(BasePath, "bg.png");
        }
        public ReSubscriptionNotificationStyle(string gameId)
        {
            if (string.IsNullOrEmpty(Main.Instance.CurrentGame))
            {
                BasePath = Path.Combine(Path.Combine(Path.Combine(Paths.Integrations, Main.Instance.Name), "Notifications"),
                    "ReSubscription");
            }
            else
            {
                BasePath = Path.Combine(Path.Combine(Path.Combine(Path.Combine(Path.Combine(Paths.Integrations, Main.Instance.Name), "Games"), Main.Instance.CurrentGame), "Notifications"),
                    "ReSubscription");
            }
            
            ImagePath = Path.Combine(BasePath, "bg.png");
        }
    }

    public class NewSubscriptionNotificationStyle : IBaseNotificationStyle
    {
        [JsonIgnore] public string BasePath { get; set; }
        public string ImagePath { get; set; }
        public TextBox NameBox { get; set; } = new TextBox();
        public TextBox MessageBox { get; set; } = new TextBox();
        public AvatarBox AvatarBox { get; set; } = new AvatarBox();
        public PipeStyle PipeStyle { get; set; } = new PipeStyle();

        public NewSubscriptionNotificationStyle()
        {
            if (string.IsNullOrEmpty(Main.Instance.CurrentGame))
            {
                BasePath = Path.Combine(Path.Combine(Path.Combine(Paths.Integrations, Main.Instance.Name), "Notifications"),
                    "NewSubscription");
            }
            else
            {
                BasePath = Path.Combine(Path.Combine(Path.Combine(Path.Combine(Path.Combine(Paths.Integrations, Main.Instance.Name), "Games"), Main.Instance.CurrentGame), "Notifications"),
                    "NewSubscription");
            }

            ImagePath = Path.Combine(BasePath, "bg.png");
        }
        public NewSubscriptionNotificationStyle(string gameId)
        {
            BasePath = Path.Combine(Path.Combine(Path.Combine(Path.Combine(Path.Combine(Paths.Integrations, Main.Instance.Name), "Games"), gameId), "Notifications"),
                "NewSubscription");
            ImagePath = Path.Combine(BasePath, "bg.png");
        }
    }
    
    public class RaidNotificationStyle : IBaseNotificationStyle
    {
        [JsonIgnore] public string BasePath { get; }
        public string ImagePath { get; set; }
        public TextBox NameBox { get; set; } = new TextBox();
        public TextBox MessageBox { get; set; } = new TextBox();
        public AvatarBox AvatarBox { get; set; } = new AvatarBox();
        public PipeStyle PipeStyle { get; set; } = new PipeStyle();

        public RaidNotificationStyle()
        {
            if (string.IsNullOrEmpty(Main.Instance.CurrentGame))
            {
                BasePath = Path.Combine(Path.Combine(Path.Combine(Paths.Integrations, Main.Instance.Name), "Notifications"),
                    "Raid");
            }
            else
            {
                BasePath = Path.Combine(Path.Combine(Path.Combine(Path.Combine(Path.Combine(Paths.Integrations, Main.Instance.Name), "Games"), Main.Instance.CurrentGame), "Notifications"),
                    "Raid");
            }
            
            ImagePath = Path.Combine(BasePath, "bg.png");
        }
        public RaidNotificationStyle(string gameId)
        {
            if (string.IsNullOrEmpty(Main.Instance.CurrentGame))
            {
                BasePath = Path.Combine(Path.Combine(Path.Combine(Paths.Integrations, Main.Instance.Name), "Notifications"),
                    "Raid");
            }
            else
            {
                BasePath = Path.Combine(Path.Combine(Path.Combine(Path.Combine(Path.Combine(Paths.Integrations, Main.Instance.Name), "Games"), Main.Instance.CurrentGame), "Notifications"),
                    "Raid");
            }
            
            ImagePath = Path.Combine(BasePath, "bg.png");
        }
    }

    public class TextBox
    {
        public string FontName { get; set; } = "Verdana";
        public float FontSize { get; set; } = 16;
        public FontColor FontColor { get; set; } = new FontColor();
        public bool Bold { get; set; } = false;
        public bool Italic { get; set; } = false;
        public bool Underline { get; set; } = false;
        public bool StrikeThrough { get; set; } = false;
        public bool Centered { get; set; } = false;
        public int Padding { get; set; } = 0;
        public Rect Position { get; set; } = new Rect(10, 10, 400, 100);
    }

    public class AvatarBox
    {
        public bool Rounded { get; set; } = false;
        public bool Outlined { get; set; } = false;
        public float OutlineThickness { get; set; } = 6;
        public bool OutlineUseChatColor { get; set; } = true;
        public FontColor ManualOutlineColor { get; set; } = new FontColor();
        public Rect Position { get; set; } = new Rect(6, 6, 75, 75);
    }

    public struct Rect
    {
        public int X { get; set; }
        public int Y { get; set; }
        public int Width { get; set; }
        public int Height { get; set; }

        public Rect(int x = 0, int y = 0, int width = 0, int height = 0)
        {
            X = x;
            Y = y;
            Width = width;
            Height = height;
        }

        public Rectangle ToRectangle()
        {
            return new Rectangle(X, Y, Width, Height);
        }
    }

    public class FontColor
    {
        public int R { get; set; }
        public int G { get; set; }
        public int B { get; set; }
        public int A { get; set; }

        public FontColor()
        {
            R = 0;
            G = 0;
            B = 0;
            A = 255;
        }

        public FontColor(int r, int g, int b, int a = 255)
        {
            R = r;
            G = g;
            B = b;
            A = a;
        }
    }

    public class PipeStyle
    {
        [JsonPropertyName("properties")]
        public Properties MyProperties { get; set; } = new Properties();
        [JsonPropertyName("transition")]
        public Transition TransitionIn { get; set; } = new Transition();
        [JsonPropertyName("transition2")]
        public Transition TransitionOut { get; set; } = new Transition();
        
        public class Properties
        {
            [JsonPropertyName("headset")]
            public bool Headset { get; set; } = false;
            [JsonPropertyName("horizontal")]
            public bool Horizontal { get; set; } = true;
            [JsonPropertyName("channel")]
            public int Channel { get; set; } = 0;
            [JsonPropertyName("hz")]
            public int Hz { get; set; } = -1;
            [JsonPropertyName("duration")]
            public int Duration { get; set; } = 4000;
            [JsonPropertyName("width")]
            public float Width { get; set; } = 1;
            [JsonPropertyName("distance")]
            public float Distance { get; set; } = 1.3f;
            [JsonPropertyName("pitch")]
            public float Pitch { get; set; } = -30;
            [JsonPropertyName("yaw")]
            public float Yaw { get; set; } = 0;
        }
        
        public class Transition
        {
            [JsonPropertyName("scale")]
            public float Scale { get; set; } = 1;
            [JsonPropertyName("opacity")]
            public float Opacity { get; set; } = 1;
            [JsonPropertyName("vertical")]
            public float Vertical { get; set; } = -2;
            [JsonPropertyName("horizontal")]
            public float Horizontal { get; set; } = 0;
            [JsonPropertyName("spin")]
            public float Spin { get; set; } = 0;
            [JsonPropertyName("tween")]
            public Tween Tween { get; set; } = Tween.Quadratic;
            [JsonPropertyName("duration")]
            public int Duration { get; set; } = 100;
        }
        
        public enum Tween
        {
            Linear,
            Sine,
            Quadratic,
            Cubic,
            Quartic,
            Quintic,
            Circle,
            Back,
            Elastic,
            Bounce
        }
    }
}