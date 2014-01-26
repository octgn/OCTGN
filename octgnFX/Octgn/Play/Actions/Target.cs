using System;
using System.Diagnostics;

namespace Octgn.Play.Actions
{
    internal sealed class Target : ActionBase
    {
        internal bool DoTarget;
        internal Card FromCard, ToCard;
        internal Player Who;

        public Target(Player who, Card fromCard, Card toCard, bool doTarget)
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
            Program.GameEngine.EventProxy.OnTargetCard_3_1_0_0(Who, FromCard, true);
            Program.GameMess.PlayerEvent(Who,"targets '{0}'", FromCard);
        }

        private void ArrowTarget()
        {
            if (CreatingArrow != null) CreatingArrow(this, EventArgs.Empty);
            Program.GameEngine.EventProxy.OnTargetCardArrow_3_1_0_0(Who, FromCard, ToCard, true);
            Program.GameMess.PlayerEvent(Who,"targets '{1}' with '{0}'", FromCard, ToCard);
        }

        private void ClearTarget()
        {
            if (FromCard.TargetsOtherCards && DeletingArrows != null)
            {
                DeletingArrows(this, EventArgs.Empty);
                Program.GameEngine.EventProxy.OnTargetCardArrow_3_1_0_0(Who, FromCard, ToCard, false);
            }

            if (FromCard.TargetedBy != null)
            {
                FromCard.SetTargetedBy(null);
                Program.GameEngine.EventProxy.OnTargetCard_3_1_0_0(Who, FromCard, false);
            }
        }
    }
}