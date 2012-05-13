using System.Diagnostics;

namespace Octgn.Play.Actions
{
    internal sealed class Rotate : ActionBase
    {
        private readonly Card _card;
        private readonly CardOrientation _rot;
        private readonly Player _who;

        public Rotate(Player who, Card card, CardOrientation rot)
        {
            _who = who;
            _card = card;
            _rot = rot;
        }

        public override void Do()
        {
            base.Do();
            _card.SetOrientation(_rot);
            Program.Trace.TraceEvent(TraceEventType.Information, EventIds.Event | EventIds.PlayerFlag(_who),
                                     "{0} sets '{1}' orientation to {2}", _who, _card, _rot);
        }
    }
}