using System.Diagnostics;

namespace Octgn.Play.Actions
{
    internal sealed class Shake : ActionBase
    {
        private readonly Card _card;
        private readonly Player _who;

        public Shake(Player who, Card card)
        {
            _who = who;
            _card = card;
        }

        public override void Do()
        {
            base.Do();
            _card.DoShake();
            Program.GameMess.PlayerEvent(_who, "shakes '{0}'", _card);
        }
    }
}