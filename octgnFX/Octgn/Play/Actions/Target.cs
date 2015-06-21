using System;
using System.Diagnostics;

namespace Octgn.Play.Actions
{
    internal sealed class Target : ActionBase
    {
        internal bool DoTarget;
        internal Card FromCard, ToCard;
        internal Player Who;
        internal bool IsScriptChange;

        public Target(Player who, Card fromCard, Card toCard, bool doTarget, bool isScriptChange)
        {
            Who = who;
            FromCard = fromCard;
            ToCard = toCard;
            DoTarget = doTarget;
            IsScriptChange = isScriptChange;
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
            Program.GameEngine.EventProxy.OnTargetCard_3_1_0_1(Who, FromCard, true);
            Program.GameEngine.EventProxy.OnCardTargeted_3_1_0_2(Who, FromCard, true, IsScriptChange);
            Program.GameMess.PlayerEvent(Who, "targets '{0}'", FromCard);
        }

        private void ArrowTarget()
        {
            if (CreatingArrow != null) CreatingArrow(this, EventArgs.Empty);
            Program.GameEngine.EventProxy.OnTargetCardArrow_3_1_0_0(Who, FromCard, ToCard, true);
            Program.GameEngine.EventProxy.OnTargetCardArrow_3_1_0_1(Who, FromCard, ToCard, true);
            Program.GameEngine.EventProxy.OnCardArrowTargeted_3_1_0_2(Who, FromCard, ToCard, true, IsScriptChange);
            Program.GameMess.PlayerEvent(Who,"targets '{1}' with '{0}'", FromCard, ToCard);
        }

        private void ClearTarget()
        {
            if (FromCard.TargetsOtherCards && DeletingArrows != null)
            {
                DeletingArrows(this, EventArgs.Empty);
                Program.GameEngine.EventProxy.OnTargetCardArrow_3_1_0_0(Who, FromCard, ToCard, false);
                Program.GameEngine.EventProxy.OnTargetCardArrow_3_1_0_1(Who, FromCard, ToCard, false);
                Program.GameEngine.EventProxy.OnCardArrowTargeted_3_1_0_2(Who, FromCard, ToCard, false, IsScriptChange);
            }

            if (FromCard.TargetedBy != null)
            {
                FromCard.SetTargetedBy(null);
                Program.GameEngine.EventProxy.OnTargetCard_3_1_0_0(Who, FromCard, false);
                Program.GameEngine.EventProxy.OnTargetCard_3_1_0_1(Who, FromCard, false);
                Program.GameEngine.EventProxy.OnCardTargeted_3_1_0_2(Who, FromCard, false, IsScriptChange);
            }
        }
    }
}