using System.Collections.Generic;
using TwitchImplementation.TwitchBot.Client.Enums;
using TwitchImplementation.TwitchBot.Client.Models;

namespace TwitchImplementation.TwitchBot.Client
{
    public static class IrcParser
    {
        public static IrcMessage ParseIrcMessage(string raw)
        {
            var tagDictionary = new Dictionary<string, string>();

            var state = State.StateNone;
            int[] starts = { 0, 0, 0, 0, 0, 0 };
            int[] lens = { 0, 0, 0, 0, 0, 0 };
            for (int i = 0; i < raw.Length; ++i)
            {
                lens[(int)state] = i - starts[(int)state] - 1;
                if (state == State.StateNone && raw[i] == '@')
                {
                    state = State.StateV3;
                    starts[(int)state] = ++i;

                    int start = i;
                    string key = null;
                    for (; i < raw.Length; ++i)
                    {
                        if (raw[i] == '=')
                        {
                            key = raw.Substring(start, i - start);
                            start = i + 1;
                        }
                        else if (raw[i] == ';')
                        {
                            if (key == null)
                                tagDictionary[raw.Substring(start, i - start)] = "1";
                            else
                                tagDictionary[key] = raw.Substring(start, i - start);
                            start = i + 1;
                        }
                        else if (raw[i] == ' ')
                        {
                            if (key == null)
                                tagDictionary[raw.Substring(start, i - start)] = "1";
                            else
                                tagDictionary[key] = raw.Substring(start, i - start);
                            break;
                        }
                    }
                }
                else if (state < State.StatePrefix && raw[i] == ':')
                {
                    state = State.StatePrefix;
                    starts[(int)state] = ++i;
                }
                else if (state < State.StateCommand)
                {
                    state = State.StateCommand;
                    starts[(int)state] = i;
                }
                else if (state < State.StateTrailing && raw[i] == ':')
                {
                    state = State.StateTrailing;
                    starts[(int)state] = ++i;
                    break;
                }
                else if (state < State.StateTrailing && raw[i] == '+' || state < State.StateTrailing && raw[i] == '-')
                {
                    state = State.StateTrailing;
                    starts[(int)state] = i;
                    break;
                }
                else if (state == State.StateCommand)
                {
                    state = State.StateParam;
                    starts[(int)state] = i;
                }

                while (i < raw.Length && raw[i] != ' ')
                    ++i;
            }
            
            lens[(int)state] = raw.Length - starts[(int)state];
            string cmd = raw.Substring(starts[(int)State.StateCommand],
                lens[(int)State.StateCommand]);

            IrcCommand command = cmd switch
            {
                "PRIVMSG" => IrcCommand.PrivMsg,
                "NOTICE" => IrcCommand.Notice,
                "PING" => IrcCommand.Ping,
                "PONG" => IrcCommand.Pong,
                "HOSTTARGET" => IrcCommand.HostTarget,
                "CLEARCHAT" => IrcCommand.ClearChat,
                "CLEARMSG" => IrcCommand.ClearMsg,
                "USERSTATE" => IrcCommand.UserState,
                "GLOBALUSERSTATE" => IrcCommand.GlobalUserState,
                "NICK" => IrcCommand.Nick,
                "JOIN" => IrcCommand.Join,
                "PART" => IrcCommand.Part,
                "PASS" => IrcCommand.Pass,
                "CAP" => IrcCommand.Cap,
                "001" => IrcCommand.RPL_001,
                "002" => IrcCommand.RPL_002,
                "003" => IrcCommand.RPL_003,
                "004" => IrcCommand.RPL_004,
                "353" => IrcCommand.RPL_353,
                "366" => IrcCommand.RPL_366,
                "372" => IrcCommand.RPL_372,
                "375" => IrcCommand.RPL_375,
                "376" => IrcCommand.RPL_376,
                "WHISPER" => IrcCommand.Whisper,
                "SERVERCHANGE" => IrcCommand.ServerChange,
                "RECONNECT" => IrcCommand.Reconnect,
                "ROOMSTATE" => IrcCommand.RoomState,
                "USERNOTICE" => IrcCommand.UserNotice,
                "MODE" => IrcCommand.Mode,
                _ => IrcCommand.Unknown
            };

            string parameters = raw.Substring(starts[(int)State.StateParam],
                lens[(int)State.StateParam]);
            string message = raw.Substring(starts[(int)State.StateTrailing],
                lens[(int)State.StateTrailing]);
            string hostMask = raw.Substring(starts[(int)State.StatePrefix],
                lens[(int)State.StatePrefix]);
            return new IrcMessage(command, new[] { parameters, message }, hostMask, tagDictionary, raw);
        }
        
        
        private enum State
        {
            /// <summary>
            /// The state none
            /// </summary>
            StateNone,
            /// <summary>
            /// The state v3
            /// </summary>
            StateV3,
            /// <summary>
            /// The state prefix
            /// </summary>
            StatePrefix,
            /// <summary>
            /// The state command
            /// </summary>
            StateCommand,
            /// <summary>
            /// The state parameter
            /// </summary>
            StateParam,
            /// <summary>
            /// The state trailing
            /// </summary>
            StateTrailing
        };
    }
}