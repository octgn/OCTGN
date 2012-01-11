using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Octgn.Data;

namespace Octgn.Play.Actions
{
	public class CreateCard : ActionBase
	{
		internal static event EventHandler Done;

		private Player owner;
		private int id, x, y;
		private ulong key;
		private CardModel model;
		private bool faceUp, deletesWhenLeavesGroup;
		internal Card card;

		public CreateCard(Player owner, int id, ulong key, bool faceUp, CardModel model, int x, int y, bool deletesWhenLeavesGroup)
		{
			this.owner = owner;
			this.id = id; this.key = key;
			this.faceUp = faceUp; this.deletesWhenLeavesGroup = deletesWhenLeavesGroup;
			this.model = model;
			this.x = x; this.y = y;
		}

		public override void Do()
		{
			base.Do();

			card =
				new Card(owner, id, key, Program.Game.Definition.CardDefinition, faceUp ? model : null, false)
				{ X = x, Y = y, DeleteWhenLeavesGroup = deletesWhenLeavesGroup };
			card.SetFaceUp(faceUp);
			Program.Game.Table.AddAt(card, Program.Game.Table.Count);

			if (Done != null) Done(this, EventArgs.Empty);	
		}
	}
}
