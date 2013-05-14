using System.Diagnostics;

namespace Octgn.Play.Actions
{
    using Octgn.Core;

    internal sealed class Turn : ActionBase
    {
        private readonly IPlayCard _card;
        private readonly bool _up;
        private readonly IPlayPlayer _who;

        public Turn(IPlayPlayer who, IPlayCard card, bool up)
        {
            _who = who;
            _card = card;
            _up = up;
        }

        public override void Do()
        {
            base.Do();
            _card.SetFaceUp(_up);
            if (_up) _card.Reveal();
            K.C.Get<GameplayTrace>().TraceEvent(TraceEventType.Information, EventIds.Event | EventIds.PlayerFlag(_who),
                                     "{0} turns '{1}' face {2}", _who, _card, _up ? "up" : "down");

            // Turning an aliased card face up will change its id,
            // which can create bugs if one tries to execute other actions using its current id.
            // That's why scripts have to be suspended until the card is revealed.
            //if (up && card.Type.alias && Script.ScriptEngine.CurrentScript != null)
            //   card.Type.revealSuspendedScript = Script.ScriptEngine.Suspend();
        }
    }
}