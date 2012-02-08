using System.Diagnostics;

namespace Octgn.Play.Actions
{
    internal sealed class Rotate : ActionBase
    {
        private readonly Card card;
        private readonly CardOrientation rot;
        private readonly Player who;
        private CardOrientation oldRot;

        public Rotate(Player who, Card card, CardOrientation rot)
        {
            this.who = who;
            this.card = card;
            this.rot = rot;
            oldRot = card.Orientation;
        }

        public override void Do()
        {
            base.Do();
            card.SetOrientation(rot);
            Program.Trace.TraceEvent(TraceEventType.Information, EventIds.Event | EventIds.PlayerFlag(who),
                                     "{0} sets '{1}' orientation to {2}", who, card, rot);
        }
    }
}