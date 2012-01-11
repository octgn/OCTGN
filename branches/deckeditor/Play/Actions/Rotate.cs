namespace Octgn.Play.Actions
{
    sealed class Rotate : ActionBase
    {
        private Player who;
        private Card card;
        private CardOrientation rot, oldRot;

        public Rotate(Player who, Card card, CardOrientation rot)
        {
            this.who = who; this.card = card; this.rot = rot;
            oldRot = card.Orientation;
        }

        public override void Do()
        {
            base.Do();
            card.SetOrientation(rot);
            Program.Trace.TraceEvent(System.Diagnostics.TraceEventType.Information, EventIds.Event | EventIds.PlayerFlag(who), "{0} sets '{1}' orientation to {2}", who, card, rot);
        }
    }
}