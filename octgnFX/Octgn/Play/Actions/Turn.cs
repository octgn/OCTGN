using System.Diagnostics;

namespace Octgn.Play.Actions
{
    internal sealed class Turn : ActionBase
    {
        private readonly Card card;
        private readonly bool up;
        private readonly Player who;

        public Turn(Player who, Card card, bool up)
        {
            this.who = who;
            this.card = card;
            this.up = up;
        }

        public override void Do()
        {
            base.Do();
            card.SetFaceUp(up);
            if (up) card.Reveal();
            Program.Trace.TraceEvent(TraceEventType.Information, EventIds.Event | EventIds.PlayerFlag(who),
                                     "{0} turns '{1}' face {2}", who, card, up ? "up" : "down");

            // Turning an aliased card face up will change its id,
            // which can create bugs if one tries to execute other actions using its current id.
            // That's why scripts have to be suspended until the card is revealed.
            //if (up && card.Type.alias && Script.ScriptEngine.CurrentScript != null)
            //   card.Type.revealSuspendedScript = Script.ScriptEngine.Suspend();
        }
    }
}