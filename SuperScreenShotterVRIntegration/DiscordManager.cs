using System.Collections.Generic;
using System.Net.Http;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace SuperScreenShotterVRIntegration
{
    public static class DiscordManager
    {
        public static void Send(Main main, string authorName, string avatarUrl, List<Embed> embeds, byte[] pngBytes = null)
        {
            var payload = new Payload {AuthorName = authorName, AuthorIconUrl = avatarUrl, Embeds = embeds};

            var formData = new MultipartFormDataContent();

            if (pngBytes is not null) formData.Add(new ByteArrayContent(pngBytes), "file", "image.png");
            formData.Add(new StringContent(JsonSerializer.Serialize(payload)), "payload_json");

            using var httpClient = new HttpClient();
            httpClient.PostAsync(main.Config.Webhook, formData).Wait();
            httpClient.Dispose();
        }
        
        public class Embed
        {
            [JsonPropertyName("image")]
            public EmbedImage Image { get; set; }
            
            [JsonPropertyName("description")]
            public string Description { get; set; }
            
            [JsonPropertyName("color")]
            public int Color { get; set; }
            
            [JsonPropertyName("timestamp")]
            public string Timestamp { get; set; }
            
            [JsonPropertyName("footer")]
            public EmbedFooter Footer { get; set; }
        }
        
        public class EmbedImage
        {
            [JsonPropertyName("url")]
            public string Url { get; set; }
        }
        
        public class EmbedFooter
        {
            [JsonPropertyName("text")] public string Text { get; set; }
        }

        public class Payload
        {
            [JsonPropertyName("username")]
            public string AuthorName { get; set; }
            [JsonPropertyName("avatar_url")]
            public string AuthorIconUrl { get; set; }
            [JsonPropertyName("embeds")]
            public List<Embed> Embeds { get; set; }
        }
    }
}