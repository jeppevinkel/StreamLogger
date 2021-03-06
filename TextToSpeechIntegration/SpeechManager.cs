using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using LibVLCSharp.Shared;
using StreamLogger;

namespace TextToSpeechIntegration
{
    public class SpeechManager
    {
        private static Config Config => Main.Instance.Config;

        private const string ApiUrl = "https://texttospeech.googleapis.com/v1/text:synthesize";

        static JsonSerializerOptions options = new JsonSerializerOptions
        {
            NumberHandling = JsonNumberHandling.AllowReadingFromString | 
                             JsonNumberHandling.WriteAsString
        };
        
        private LibVLC libvlc;
        private MemoryStream stream;
        MediaPlayer mediaPlayer;
        private Action _playFinishedCallback;

        private Task _queueChecker;

        private bool _mediaPlayerIdle = true;

        private readonly Queue<Audio> SpeechQueue = new ();
        
        private static readonly HttpClient Client = new();
        
        public SpeechManager()
        {
            Core.Initialize(@"C:\Users\jeppe\Documents\RiderProjects\StreamLogger\TextToSpeechIntegration\bin\Debug\net5.0\libvlc\win-x64");
            libvlc = new LibVLC(false);
            libvlc.Log += (sender, args) =>
            {
                if (args.Module == "imem") return;
                if (args.Level == LogLevel.Error)
                {
                    Log.Error($"[LibVLC] {args.FormattedLog}");
                }
            };
            stream = new MemoryStream();
            mediaPlayer = new MediaPlayer(libvlc);
            mediaPlayer.Stopped += MediaPlayerStopped;
            mediaPlayer.Playing += MediaPlayerPlaying;

            _queueChecker = Task.Run(CheckAudioQueue);
        }
        
        private async Task CheckAudioQueue()
        {
            while (true)
            {
                await Task.Delay(200);
                if (!_mediaPlayerIdle || !SpeechQueue.TryDequeue(out Audio audio))
                {
                    continue;
                }
                Log.Debug("Playing next audio from queue!");
                PlayMp3Data(audio.Data);
                _playFinishedCallback = audio.Callback;
            }
        }

        public void CustomSynthesize(string str, string languageCode = "en-US", string name = "", VoiceSelectionParams.SsmlVoiceGender voiceGender = VoiceSelectionParams.SsmlVoiceGender.SSML_VOICE_GENDER_UNSPECIFIED)
        {
            var voice = new VoiceSelectionParams(languageCode, name, voiceGender);
            var input = new SynthesisInput(str);
            var audioConfig = new AudioConfig();

            var requestContent = new SynthesisRequestContent(input, voice, audioConfig);
            
            var content = JsonContent.Create(requestContent);
            
            HttpResponseMessage response = Client.PostAsync($"{ApiUrl}?key={Config.ApiKey}", content).Result;
            string responseString = response.Content.ReadAsStringAsync().Result;
            var responseData = JsonSerializer.Deserialize<SynthesisResponse>(responseString, options);
            byte[] soundData = string.IsNullOrEmpty(responseData?.AudioContent) ? Array.Empty<byte>() : Convert.FromBase64String(responseData.AudioContent);

            EnqueueSpeech(soundData);
        }

        public void CustomSynthesize(string str, VoiceManager.VoiceSettings settings, Action callback = null)
        {
            CustomSynthesize(str, settings.LanguageCode, settings.Name, settings.VoiceGender, settings.SpeakingRate, settings.Pitch, callback);
        }

        public void CustomSynthesize(string str, string languageCode = "en-US", string name = "",
            VoiceSelectionParams.SsmlVoiceGender voiceGender =
                VoiceSelectionParams.SsmlVoiceGender.SSML_VOICE_GENDER_UNSPECIFIED, float speakingRate = 1,
            float pitch = 0, Action callback = null)
        {
            var voice = new VoiceSelectionParams(languageCode, name, voiceGender);
            var input = new SynthesisInput(str);
            var audioConfig = new AudioConfig(pitch, speakingRate);

            var requestContent = new SynthesisRequestContent(input, voice, audioConfig);
            
            var content = JsonContent.Create(requestContent);
            
            HttpResponseMessage response = Client.PostAsync($"{ApiUrl}?key={Config.ApiKey}", content).Result;
            string responseString = response.Content.ReadAsStringAsync().Result;
            var responseData = JsonSerializer.Deserialize<SynthesisResponse>(responseString, options);
            if (responseData?.AudioContent is not null)
            {
                byte[] soundData = string.IsNullOrEmpty(responseData?.AudioContent) ? Array.Empty<byte>() : Convert.FromBase64String(responseData.AudioContent);

                EnqueueSpeech(soundData, callback);
            }
        }

        public void EnqueueSpeech(byte[] data, Action callback = null)
        {
            SpeechQueue.Enqueue(new Audio(data, callback));
            Log.Debug($"Queue now has {SpeechQueue.Count} elements.");
        }

        public void PlayMp3Data(byte[] data)
        {
            stream.Write(data, 0, data.Length);
            stream.Position = 0;
            Media media;
            media = new Media(libvlc, new StreamMediaInput(stream));
            mediaPlayer.Play(media);
        }

        private void MediaPlayerPlaying(object sender, EventArgs e)
        {
            _mediaPlayerIdle = false;
            Log.Debug("MediaPLayerPlaying!");
        }

        private void MediaPlayerStopped(object sender, EventArgs e)
        {
            _mediaPlayerIdle = true;
            if (_playFinishedCallback is not null)
            {
                _playFinishedCallback.Invoke();
                _playFinishedCallback = null;
            }
            Log.Debug("MediaPLayerStopped!");
        }
        
        public class SynthesisResponse
        {
            [JsonPropertyName("audioContent")]
            public string AudioContent { get; set; }
        }
        
        public class SynthesisRequestContent
        {
            public SynthesisInput Input { get; set; }
            public VoiceSelectionParams Voice { get; set; }
            public AudioConfig AudioConfig { get; set; }

            public SynthesisRequestContent(SynthesisInput input, VoiceSelectionParams voice, AudioConfig audioConfig)
            {
                Input = input;
                Voice = voice;
                AudioConfig = audioConfig;
            }
        }

        public class SynthesisInput
        {
            [JsonPropertyName("text")]
            public string Text { get; set; }
        
            public SynthesisInput(string text)
            {
                Text = text;
            }
        }

        public class VoiceSelectionParams
        {
            [JsonPropertyName("languageCode")]
            public string LanguageCode { get; set; }
            [JsonPropertyName("name")]
            public string Name { get; set; }
            [JsonPropertyName("ssmlGender")]
            public SsmlVoiceGender SsmlGender { get; set; }
        
            public enum SsmlVoiceGender
            {
                SSML_VOICE_GENDER_UNSPECIFIED,
                MALE,
                FEMALE,
                NEUTRAL
            }
        
            public VoiceSelectionParams(string languageCode = "en-US", string name = "", SsmlVoiceGender ssmlGender = SsmlVoiceGender.SSML_VOICE_GENDER_UNSPECIFIED)
            {
                LanguageCode = languageCode;
                Name = name;
                SsmlGender = ssmlGender;
            }
        }

        public class AudioConfig
        {
            public AudioEncodingEnum AudioEncoding { get; set; } = AudioEncodingEnum.OGG_OPUS;
            public float SpeakingRate { get; set; } = 1;
            public float Pitch { get; set; } = 0;
            public float VolumeGainDb { get; set; } = 0;
        
            public AudioConfig(float pitch = 0, float speakingRate = 1, float gain = 0)
            {
                Pitch = pitch;
                SpeakingRate = speakingRate;
                VolumeGainDb = gain;
            }
            
            [SuppressMessage("ReSharper", "InconsistentNaming")]
            public enum AudioEncodingEnum
            {
                AUDIO_ENCODING_UNSPECIFIED,
                LINEAR16,
                MP3,
                OGG_OPUS
            }
        }
        
        public class Audio
        {
            public byte[] Data;
            public Action Callback;

            public Audio(byte[] data, Action callback = null)
            {
                Data = data;
                Callback = callback;
            }
        }
    }
}