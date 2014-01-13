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
            Program.GameMess.PlayerEvent(_who,"sets '{0}' orientation to {1}",_card, _rot);
        }
    }
}