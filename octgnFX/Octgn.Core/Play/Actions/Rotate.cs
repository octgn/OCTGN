using System.Diagnostics;

namespace Octgn.Play.Actions
{
    using Octgn.Core;

    internal sealed class Rotate : ActionBase
    {
        private readonly IPlayCard _card;
        private readonly CardOrientation _rot;
        private readonly IPlayPlayer _who;

        public Rotate(IPlayPlayer who, IPlayCard card, CardOrientation rot)
        {
            _who = who;
            _card = card;
            _rot = rot;
        }

        public override void Do()
        {
            base.Do();
            _card.SetOrientation(_rot);
            K.C.Get<GameplayTrace>().TraceEvent(TraceEventType.Information, EventIds.Event | EventIds.PlayerFlag(_who),
                                     "{0} sets '{1}' orientation to {2}", _who, _card, _rot);
        }
    }
}