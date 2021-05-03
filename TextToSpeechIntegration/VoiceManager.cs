using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using StreamLogger;
using StreamLogger.Api;

namespace TextToSpeechIntegration
{
    public static class VoiceManager
    {
        private static string saveDir = Path.Combine(Paths.Integrations, "TextToSpeech");
        private static string savePath = Path.Combine(saveDir, "voiceProfiles");
        
        public static Dictionary<string, VoiceSettings> VoiceProfiles { get; private set; } = new Dictionary<string, VoiceSettings>();

        public static void SaveProfile(string userId, VoiceSettings voiceSettings)
        {
            if (VoiceProfiles.ContainsKey(userId))
            {
                VoiceProfiles[userId].Name = voiceSettings.Name;
                VoiceProfiles[userId].LanguageCode = voiceSettings.LanguageCode;
                VoiceProfiles[userId].VoiceGender = voiceSettings.VoiceGender;
            }
            else
            {
                VoiceProfiles.Add(userId, voiceSettings);
            }
            SaveProfiles();
        }

        public static VoiceSettings LoadProfile(string userId, string displayName = null)
        {
            if (VoiceProfiles.TryGetValue(userId, out VoiceSettings res))
            {
                if (res.ReadableName == null)
                {
                    res.ReadableName = Main.CleanName(string.IsNullOrEmpty(displayName) ? userId : displayName);
                }
                return res;
            }
            else
            {
                var voiceSettings = new VoiceSettings();
                if (!string.IsNullOrEmpty(displayName))
                {
                    voiceSettings.ReadableName = Main.CleanName(displayName);
                }
                
                return voiceSettings;
            }
        }

        public static void SaveProfiles()
        {
            if (!Directory.Exists(saveDir))
            {
                Directory.CreateDirectory(saveDir);
            }
            
            File.WriteAllText(savePath, JsonSerializer.Serialize(VoiceProfiles));
        }

        public static void LoadProfiles()
        {
            if (!Directory.Exists(saveDir))
            {
                Directory.CreateDirectory(saveDir);
            }
            
            if (!File.Exists(savePath))
            {
                File.WriteAllText(savePath, JsonSerializer.Serialize(VoiceProfiles));
                return;
            }
            
            try
            {
                string rawData = File.ReadAllText(savePath);

                VoiceProfiles = JsonSerializer.Deserialize<Dictionary<string, VoiceSettings>>(rawData);
            }
            catch (Exception e)
            {
                File.WriteAllText(savePath, JsonSerializer.Serialize(VoiceProfiles));
                Log.Error(e);
            }
        }
        
        public class VoiceSettings
        {
            public string LanguageCode { get; set; } = "en-US";
            public string Name { get; set; } = "";

            public SpeechManager.VoiceSelectionParams.SsmlVoiceGender VoiceGender { get; set; } =
                SpeechManager.VoiceSelectionParams.SsmlVoiceGender.SSML_VOICE_GENDER_UNSPECIFIED;

            public float SpeakingRate { get; set; } = 1;
            public float Pitch { get; set; } = 0;
            
            public string ReadableName { get; set; }

            public void Reset()
            {
                LanguageCode = "en-US";
                Name = "";
                VoiceGender = SpeechManager.VoiceSelectionParams.SsmlVoiceGender.SSML_VOICE_GENDER_UNSPECIFIED;
                SpeakingRate = 1;
                Pitch = 0;
            }
        }
    }
}