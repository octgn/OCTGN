using System;
using System.Diagnostics;

namespace Octgn.Play.Actions
{
    internal sealed class Target : ActionBase
    {
        internal bool doTarget;
        internal Card fromCard, toCard;
        internal Player who;

        public Target(Player who, Card fromCard, Card toCard, bool doTarget)
        {
            this.who = who;
            this.fromCard = fromCard;
            this.toCard = toCard;
            this.doTarget = doTarget;
        }

        internal static event EventHandler CreatingArrow;
        internal static event EventHandler DeletingArrows;

        public override void Do()
        {
            base.Do();
            if (doTarget)
            {
                if (toCard == null) SingleTarget();
                else ArrowTarget();
            }
            else
                ClearTarget();
        }

        private void SingleTarget()
        {
            fromCard.SetTargetedBy(who);
            Program.Trace.TraceEvent(TraceEventType.Information, EventIds.Event | EventIds.PlayerFlag(who),
                                     "{0} targets '{1}'", who, fromCard);
        }

        private void ArrowTarget()
        {
            if (CreatingArrow != null) CreatingArrow(this, EventArgs.Empty);
            Program.Trace.TraceEvent(TraceEventType.Information, EventIds.Event | EventIds.PlayerFlag(who),
                                     "{0} targets '{2}' with '{1}'", who, fromCard, toCard);
        }

        private void ClearTarget()
        {
            if (fromCard.TargetsOtherCards && DeletingArrows != null)
                DeletingArrows(this, EventArgs.Empty);

            if (fromCard.TargetedBy != null)
                fromCard.SetTargetedBy(null);
        }
    }
}