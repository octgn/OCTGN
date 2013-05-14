namespace Octgn.Core
{
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Text.RegularExpressions;

    using Octgn.Core.Play;
    using Octgn.Play;

    public class GameplayTrace:TraceSource
    {
        public GameplayTrace()
            : base("MainTrace", SourceLevels.Information)
        {
            
        }
        internal void Print(IPlayPlayer player, string text)
        {
            string finalText = text;
            int i = 0;
            var args = new List<object>(2);
            Match match = Regex.Match(text, "{([^}]*)}");
            while (match.Success)
            {
                string token = match.Groups[1].Value;
                finalText = finalText.Replace(match.Groups[0].Value, "##$$%%^^LEFTBRACKET^^%%$$##" + i + "##$$%%^^RIGHTBRACKET^^%%$$##");
                i++;
                object tokenValue = token;
                switch (token)
                {
                    case "me":
                        tokenValue = player;
                        break;
                    default:
                        if (token.StartsWith("#"))
                        {
                            int id;
                            if (!int.TryParse(token.Substring(1), out id)) break;
                            IPlayControllableObject obj = K.C.Get<ControllableObjectStateMachine>().Find(id);
                            if (obj == null) break;
                            tokenValue = obj;
                            break;
                        }
                        break;
                }
                args.Add(tokenValue);
                match = match.NextMatch();
            }
            args.Add(player);
            finalText = finalText.Replace("{", "").Replace("}", "");
            finalText = finalText.Replace("##$$%%^^LEFTBRACKET^^%%$$##", "{").Replace(
                "##$$%%^^RIGHTBRACKET^^%%$$##", "}");
            this.TraceEvent(TraceEventType.Information,
                             EventIds.Event | EventIds.PlayerFlag(player) | EventIds.Explicit, finalText, args.ToArray());
        }

        internal void TracePlayerEvent(IPlayPlayer player, string message, params object[] args)
        {
            var args1 = new List<object>(args) { player };
            this.TraceEvent(TraceEventType.Information, EventIds.Event | EventIds.PlayerFlag(player), message,
                             args1.ToArray());
        }

        internal void TraceWarning(string message)
        {
            this.TraceEvent(TraceEventType.Warning, EventIds.NonGame, message);
        }

        internal void TraceWarning(string message, params object[] args)
        {
            this.TraceEvent(TraceEventType.Warning, EventIds.NonGame, message, args);
        }
    }
}