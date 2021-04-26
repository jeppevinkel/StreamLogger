using System.Collections.Generic;
using TwitchImplementation.TwitchBot.Client.Enums;

namespace TwitchImplementation.TwitchBot.Client.Models
{
    public class IrcMessage
    {
        public string Channel => Params.StartsWith("#") ? Params.Remove(0, 1) : Params;
        public string Params => _parameters != null && _parameters.Length > 0 ? _parameters[0] : "";

        public string Message => Trailing;

        public string Trailing =>
            _parameters != null && _parameters.Length > 1 ? _parameters[_parameters.Length - 1] : "";

        private readonly string[] _parameters;

        public readonly string User;

        public readonly string Hostmask;

        public readonly IrcCommand Command;

        public readonly Dictionary<string, string> Tags;

        public readonly string RawIrc;

        public IrcMessage(IrcCommand command,
            string[] parameters,
            string hostmask,
            Dictionary<string, string> tags,
            string rawIrc)
        {
            var idx = hostmask.IndexOf('!');
            User = idx != -1 ? hostmask.Substring(0, idx) : hostmask;
            Hostmask = hostmask;
            _parameters = parameters;
            Command = command;
            Tags = tags;
            RawIrc = rawIrc;
        }
    }
}