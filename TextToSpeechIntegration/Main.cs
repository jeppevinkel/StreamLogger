﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using StreamLogger;
using StreamLogger.Api;
using StreamLogger.Api.EventArgs;
using StreamLogger.Api.MessageTypes.MiscData;

namespace TextToSpeechIntegration
{
    public class Main : Integration<Config>
    {
        private SpeechManager _manager;
        
        private static Main singleton = new Main();

        private Main()
        {
        }
        public static Main Instance => singleton;
        
        /// <inheritdoc/>
        public override void Init()
        {
            base.Init();

            VoiceManager.LoadProfiles();
            _manager = new SpeechManager();
            if (Config.ReadAllChat)
            {
                EventManager.ChatMessageEvent += OnChatMessageEvent;
            }
            else
            {
                EventManager.ChatMessageWithRewardEvent += OnRewardEvent;
            }
        }

        private void OnChatMessageEvent(ChatMessageEventArgs e)
        {
            if (Config.ExcludePrefix.Contains(e.Message.MessageContent[0]))
            {
                return;
            }

            VoiceManager.VoiceSettings voiceSettings = VoiceManager.LoadProfile(e.Message.Username, e.Message.DisplayName);
            
            var keyValuePairs = e.Message.MessageContent.Split(' ')
                .Select(x => x.Split(':'))
                .Where(x => x.Length == 2)
                .ToDictionary(x => x.First(), x => x.Last());

            foreach (KeyValuePair<string,string> keyValuePair in keyValuePairs)
            {
                string substr = $"{keyValuePair.Key}:{keyValuePair.Value}";
                switch (keyValuePair.Key)
                {
                    case "lang":
                        if (keyValuePair.Value == "reset")
                        {
                            voiceSettings.LanguageCode = "en-US";
                            e.Message.MessageContent = e.Message.MessageContent.Remove(e.Message.MessageContent.IndexOf(substr), substr.Length);
                            break;
                        }
                        voiceSettings.LanguageCode = keyValuePair.Value;
                        e.Message.MessageContent = e.Message.MessageContent.Remove(e.Message.MessageContent.IndexOf(substr), substr.Length);
                        break;
                    case "name":
                        if (keyValuePair.Value == "reset")
                        {
                            voiceSettings.Name = "";
                            e.Message.MessageContent = e.Message.MessageContent.Remove(e.Message.MessageContent.IndexOf(substr), substr.Length);
                            break;
                        }
                        voiceSettings.Name = keyValuePair.Value;
                        e.Message.MessageContent = e.Message.MessageContent.Remove(e.Message.MessageContent.IndexOf(substr), substr.Length);
                        break;
                    case "gender":
                        if (keyValuePair.Value == "reset")
                        {
                            voiceSettings.VoiceGender = SpeechManager.VoiceSelectionParams.SsmlVoiceGender.SSML_VOICE_GENDER_UNSPECIFIED;
                            e.Message.MessageContent = e.Message.MessageContent.Remove(e.Message.MessageContent.IndexOf(substr), substr.Length);
                            break;
                        }
                        if (SpeechManager.VoiceSelectionParams.SsmlVoiceGender.TryParse(keyValuePair.Value, true,
                            out SpeechManager.VoiceSelectionParams.SsmlVoiceGender gender))
                        {
                            voiceSettings.VoiceGender = gender;
                        }
                        e.Message.MessageContent = e.Message.MessageContent.Remove(e.Message.MessageContent.IndexOf(substr), substr.Length);
                        break;
                    case "pitch":
                        if (keyValuePair.Value == "reset")
                        {
                            voiceSettings.Pitch = 0;
                            e.Message.MessageContent =
                                e.Message.MessageContent.Remove(e.Message.MessageContent.IndexOf(substr),substr.Length);
                            break;
                        }
                        if (float.TryParse(keyValuePair.Value, out float pitch))
                        {
                            voiceSettings.Pitch = pitch;
                            e.Message.MessageContent =
                                e.Message.MessageContent.Remove(e.Message.MessageContent.IndexOf(substr),
                                    substr.Length);
                        }
                        break;
                    case "rate":
                        if (keyValuePair.Value == "reset")
                        {
                            voiceSettings.SpeakingRate = 1;
                            e.Message.MessageContent =
                                e.Message.MessageContent.Remove(e.Message.MessageContent.IndexOf(substr),
                                    substr.Length);
                            break;
                        }
                        if (float.TryParse(keyValuePair.Value, out float rate))
                        {
                            voiceSettings.SpeakingRate = rate;
                            e.Message.MessageContent =
                                e.Message.MessageContent.Remove(e.Message.MessageContent.IndexOf(substr),
                                    substr.Length);
                        }
                        break;
                    case "all":
                        if (keyValuePair.Value == "reset")
                        {
                            voiceSettings.Reset();
                            e.Message.MessageContent =
                                e.Message.MessageContent.Remove(e.Message.MessageContent.IndexOf(substr),
                                    substr.Length);
                            break;
                        }
                        break;
                }
            }
            
            VoiceManager.SaveProfile(e.Message.Username, voiceSettings);

            if (string.IsNullOrWhiteSpace(e.Message.MessageContent))
            {
                return;
            }

            StringBuilder sb = new StringBuilder();
            
            if (e.Message.Flags.Contains("IsMe"))
            {
                sb.Append($"{e.Message.DisplayName} ");
            }
            else if (Config.MentionNames && !Config.UseReadableNames)
            {
                sb.Append($"{e.Message.DisplayName} said ");
            }else if (Config.MentionNames && Config.UseReadableNames)
            {
                sb.Append($"{voiceSettings.ReadableName} said ");
            }

            if (Config.StripEmotes)
            {
                string msg = e.Message.MessageContent;
                List<Emote> emotes = e.Message.Emotes;
                emotes.Sort((e1, e2) => e2.Start - e1.Start);

                msg = emotes.Aggregate(msg, (current, emote) => current.Remove(emote.Start, emote.End - emote.Start + 1));

                if (string.IsNullOrWhiteSpace(msg))
                {
                    return;
                }

                sb.Append(msg);
            }
            else
            {
                sb.Append(e.Message.MessageContent);
            }

            Log.Info(e.Message);

            _manager.CustomSynthesize(sb.ToString(), voiceSettings.LanguageCode, voiceSettings.Name, voiceSettings.VoiceGender, voiceSettings.SpeakingRate, voiceSettings.Pitch);
        }
        
        private void OnRewardEvent(ChatMessageWithRewardEventArgs e)
        {
            if (e.Message.RewardId != Config.RewardId)
            {
                return;
            }
            
            VoiceManager.VoiceSettings voiceSettings = VoiceManager.LoadProfile(e.Message.Username, e.Message.DisplayName);
            
            var keyValuePairs = e.Message.MessageContent.Split(' ')
                .Select(x => x.Split(':'))
                .Where(x => x.Length == 2)
                .ToDictionary(x => x.First(), x => x.Last());

            foreach (KeyValuePair<string,string> keyValuePair in keyValuePairs)
            {
                string substr = $"{keyValuePair.Key}:{keyValuePair.Value}";
                switch (keyValuePair.Key)
                {
                    case "lang":
                        if (keyValuePair.Value == "reset")
                        {
                            voiceSettings.LanguageCode = "en-US";
                            e.Message.MessageContent = e.Message.MessageContent.Remove(e.Message.MessageContent.IndexOf(substr), substr.Length);
                            break;
                        }
                        voiceSettings.LanguageCode = keyValuePair.Value;
                        e.Message.MessageContent = e.Message.MessageContent.Remove(e.Message.MessageContent.IndexOf(substr), substr.Length);
                        break;
                    case "name":
                        if (keyValuePair.Value == "reset")
                        {
                            voiceSettings.Name = "";
                            e.Message.MessageContent = e.Message.MessageContent.Remove(e.Message.MessageContent.IndexOf(substr), substr.Length);
                            break;
                        }
                        voiceSettings.Name = keyValuePair.Value;
                        e.Message.MessageContent = e.Message.MessageContent.Remove(e.Message.MessageContent.IndexOf(substr), substr.Length);
                        break;
                    case "gender":
                        if (keyValuePair.Value == "reset")
                        {
                            voiceSettings.VoiceGender = SpeechManager.VoiceSelectionParams.SsmlVoiceGender.SSML_VOICE_GENDER_UNSPECIFIED;
                            e.Message.MessageContent = e.Message.MessageContent.Remove(e.Message.MessageContent.IndexOf(substr), substr.Length);
                            break;
                        }
                        if (SpeechManager.VoiceSelectionParams.SsmlVoiceGender.TryParse(keyValuePair.Value, true,
                            out SpeechManager.VoiceSelectionParams.SsmlVoiceGender gender))
                        {
                            voiceSettings.VoiceGender = gender;
                        }
                        e.Message.MessageContent = e.Message.MessageContent.Remove(e.Message.MessageContent.IndexOf(substr), substr.Length);
                        break;
                    case "pitch":
                        if (keyValuePair.Value == "reset")
                        {
                            voiceSettings.Pitch = 0;
                            e.Message.MessageContent =
                                e.Message.MessageContent.Remove(e.Message.MessageContent.IndexOf(substr),substr.Length);
                            break;
                        }
                        if (float.TryParse(keyValuePair.Value, out float pitch))
                        {
                            voiceSettings.Pitch = pitch;
                            e.Message.MessageContent =
                                e.Message.MessageContent.Remove(e.Message.MessageContent.IndexOf(substr),
                                    substr.Length);
                        }
                        break;
                    case "rate":
                        if (keyValuePair.Value == "reset")
                        {
                            voiceSettings.SpeakingRate = 1;
                            e.Message.MessageContent =
                                e.Message.MessageContent.Remove(e.Message.MessageContent.IndexOf(substr),
                                    substr.Length);
                            break;
                        }
                        if (float.TryParse(keyValuePair.Value, out float rate))
                        {
                            voiceSettings.SpeakingRate = rate;
                            e.Message.MessageContent =
                                e.Message.MessageContent.Remove(e.Message.MessageContent.IndexOf(substr),
                                    substr.Length);
                        }
                        break;
                    case "all":
                        if (keyValuePair.Value == "reset")
                        {
                            voiceSettings.Reset();
                            e.Message.MessageContent =
                                e.Message.MessageContent.Remove(e.Message.MessageContent.IndexOf(substr),
                                    substr.Length);
                            break;
                        }
                        break;
                }
            }
            
            VoiceManager.SaveProfile(e.Message.Username, voiceSettings);

            if (string.IsNullOrWhiteSpace(e.Message.MessageContent))
            {
                return;
            }

            StringBuilder sb = new StringBuilder();
            
            if (e.Message.Flags.Contains("IsMe"))
            {
                sb.Append($"{e.Message.DisplayName} ");
            }
            else if (Config.MentionNames && !Config.UseReadableNames)
            {
                sb.Append($"{e.Message.DisplayName} said ");
            }else if (Config.MentionNames && Config.UseReadableNames)
            {
                sb.Append($"{voiceSettings.ReadableName} said ");
            }

            if (Config.StripEmotes)
            {
                string msg = e.Message.MessageContent;
                List<Emote> emotes = e.Message.Emotes;
                emotes.Sort((e1, e2) => e2.Start - e1.Start);

                msg = emotes.Aggregate(msg, (current, emote) => current.Remove(emote.Start, emote.End - emote.Start + 1));

                if (string.IsNullOrWhiteSpace(msg))
                {
                    return;
                }

                sb.Append(msg);
            }
            else
            {
                sb.Append(e.Message.MessageContent);
            }

            Log.Info(e.Message);

            _manager.CustomSynthesize(sb.ToString(), voiceSettings.LanguageCode, voiceSettings.Name, voiceSettings.VoiceGender, voiceSettings.SpeakingRate, voiceSettings.Pitch);
        }

        private static readonly Dictionary<string, string> numToChar = new()
        {
            {"0", "o"},
            {"1", "i"},
            {"3", "e"},
            {"4", "a"},
            {"5", "s"},
            {"6", "g"},
            {"7", "t"}
        };
        public static string CleanName(string name)
        {
            var nameArr = name.ToLower().Split("_");
            var namePart = nameArr.OrderByDescending(str => str.Length).First();
            namePart = Regex.Replace(namePart, "[0-9]{2,}", "");
            var numToCharRe = string.Join('|', numToChar.Keys) ;
            var result = new Regex(numToCharRe, RegexOptions.IgnoreCase).Replace(namePart, matched =>
            {
                return numToChar[matched.Value];
            });
            
            return result.Length > 0 ? result : name;
        }
    }
}