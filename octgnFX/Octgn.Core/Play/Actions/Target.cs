using System;
using System.Diagnostics;

namespace Octgn.Play.Actions
{
    using Octgn.Core;

    internal sealed class Target : ActionBase
    {
        internal bool DoTarget;
        internal IPlayCard FromCard, ToCard;
        internal IPlayPlayer Who;

        public Target(IPlayPlayer who, IPlayCard fromCard, IPlayCard toCard, bool doTarget)
        {
            Who = who;
            FromCard = fromCard;
            ToCard = toCard;
            DoTarget = doTarget;
        }

        internal static event EventHandler CreatingArrow;
        internal static event EventHandler DeletingArrows;

        public override void Do()
        {
            base.Do();
            if (DoTarget)
            {
                if (ToCard == null) SingleTarget();
                else ArrowTarget();
            }
            else
                ClearTarget();
        }

        private void SingleTarget()
        {
            FromCard.SetTargetedBy(Who);
            K.C.Get<GameplayTrace>().TraceEvent(TraceEventType.Information, EventIds.Event | EventIds.PlayerFlag(Who),
                                     "{0} targets '{1}'", Who, FromCard);
        }

        private void ArrowTarget()
        {
            if (CreatingArrow != null) CreatingArrow(this, EventArgs.Empty);
            K.C.Get<GameplayTrace>().TraceEvent(TraceEventType.Information, EventIds.Event | EventIds.PlayerFlag(Who),
                                     "{0} targets '{2}' with '{1}'", Who, FromCard, ToCard);
        }

        private void ClearTarget()
        {
            if (FromCard.TargetsOtherCards && DeletingArrows != null)
                DeletingArrows(this, EventArgs.Empty);

            if (FromCard.TargetedBy != null)
                FromCard.SetTargetedBy(null);
        }
    }
}