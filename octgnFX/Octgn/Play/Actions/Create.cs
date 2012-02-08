using System;
using Octgn.Data;

namespace Octgn.Play.Actions
{
    public class CreateCard : ActionBase
    {
        private readonly bool deletesWhenLeavesGroup;
        private readonly bool faceUp;
        private readonly int id;
        private readonly ulong key;
        private readonly CardModel model;
        private readonly Player owner;
        private readonly int x;
        private readonly int y;
        internal Card card;

        public CreateCard(Player owner, int id, ulong key, bool faceUp, CardModel model, int x, int y,
                          bool deletesWhenLeavesGroup)
        {
            this.owner = owner;
            this.id = id;
            this.key = key;
            this.faceUp = faceUp;
            this.deletesWhenLeavesGroup = deletesWhenLeavesGroup;
            this.model = model;
            this.x = x;
            this.y = y;
        }

        internal static event EventHandler Done;

        public override void Do()
        {
            base.Do();

            card =
                new Card(owner, id, key, Program.Game.Definition.CardDefinition, faceUp ? model : null, false)
                    {X = x, Y = y, DeleteWhenLeavesGroup = deletesWhenLeavesGroup};
            card.SetFaceUp(faceUp);
            Program.Game.Table.AddAt(card, Program.Game.Table.Count);

            if (Done != null) Done(this, EventArgs.Empty);
        }
    }
}