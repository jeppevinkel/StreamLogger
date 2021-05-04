using System;
using System.Drawing;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;
using StreamLogger;

namespace OpenVRNotificationPipeIntegration
{
    public class StyleManager
    {
        public string StylePath;
        public NotificationStyles NotificationStyles;

        private readonly JsonSerializerOptions _options = new JsonSerializerOptions
        {
            WriteIndented = true,
        };
        
        public StyleManager(string baseFolder)
        {
            StylePath = Path.Combine(baseFolder, "Styles.json");
            if (CheckFileExistence())
            {
                LoadStyles();
            }
        }

        private void SaveStyles()
        {
            File.WriteAllText(StylePath, JsonSerializer.Serialize(NotificationStyles, _options));
        }

        public void LoadStyles()
        {
            try
            {
                NotificationStyles = JsonSerializer.Deserialize<NotificationStyles>(File.ReadAllText(StylePath));
            }
            catch (Exception e)
            {
                Log.Error($"Something went wrong while reading notification styles. Please validate file structure.\n{e}");
                NotificationStyles = new NotificationStyles();
                return;
            }
            SaveStyles();
        }

        private bool CheckFileExistence()
        {
            if (File.Exists(StylePath)) return true;
            NotificationStyles = new NotificationStyles();
            File.WriteAllText(StylePath, JsonSerializer.Serialize(NotificationStyles, _options));
            return false;
        }
    }

    public class NotificationStyles
    {
        public MessageNotificationStyle MessageNotificationStyle { get; set; } = new MessageNotificationStyle();
        public MessageWithBitsNotificationStyle MessageWithBitsNotificationStyle { get; set; } = new MessageWithBitsNotificationStyle();
    }

    public class MessageNotificationStyle
    {
        public TextBox NameBox { get; set; } = new TextBox();
        public TextBox MessageBox { get; set; } = new TextBox();
        public AvatarBox AvatarBox { get; set; } = new AvatarBox();

        public PipeStyle PipeStyle { get; set; } = new PipeStyle();
    }

    public class MessageWithBitsNotificationStyle
    {
        public TextBox NameBox { get; set; } = new TextBox();
        public TextBox MessageBox { get; set; } = new TextBox();
        public TextBox BitsBox { get; set; } = new TextBox();
        public AvatarBox AvatarBox { get; set; } = new AvatarBox();

        public PipeStyle PipeStyle { get; set; } = new PipeStyle();
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
        public Rect Position { get; set; } = new Rect();
    }

    public class AvatarBox
    {
        public bool Rounded { get; set; } = false;
        public bool Outlined { get; set; } = false;
        public float OutlineThickness { get; set; } = 6;
        public bool OutlineUseChatColor { get; set; } = true;
        public FontColor ManualOutlineColor { get; set; } = new FontColor();
        public Rect Position { get; set; } = new Rect();
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
            return new Rectangle(this.X, this.Y, this.Width, this.Height);
        }
    }

    public struct FontColor
    {
        public int R { get; set; }
        public int G { get; set; }
        public int B { get; set; }
        public int A { get; set; }

        public FontColor(int r = 0)
        {
            R = 0;
            G = 0;
            B = 0;
            A = 255;
        }

        public FontColor(int r, int g, int b, int a)
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