using System.Diagnostics;

namespace Octgn.Play.Actions
{
    internal sealed class Rotate : ActionBase
    {
        private readonly Card _card;
        private readonly CardOrientation _rot;
        private readonly Player _who;
        private CardOrientation _oldRot;

        public Rotate(Player who, Card card, CardOrientation rot)
        {
            this._who = who;
            this._card = card;
            this._rot = rot;
            _oldRot = card.Orientation;
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