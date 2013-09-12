namespace Octgn.Extentions
{
    using Octgn.Core.DataExtensionMethods;
    using Octgn.Core.Util;
    using Octgn.DataNew.Entities;

    using Player = Octgn.Play.Player;

    public static class ExtensionMethods
    {
        /// <summary>
        /// Creates a <see cref="Octgn.Play.Card"/> from a <see cref="Octgn.DataNew.Entities.ICard"/> and stores its <see cref="Octgn.Play.CardIdentity"/>
        /// </summary>
        /// <param name="card"></param>
        /// <param name="player"></param>
        /// <returns></returns>
        public static Play.Card ToPlayCard(this ICard card, Play.Player player)
        {
            ulong key = card.GenerateKey();
            int id = card.GenerateCardId();
            var retCard = new Play.Card(player, id, key, Program.GameEngine.Definition.GetCardById(card.Id), true);
            return retCard;
        }

        public static ulong GenerateKey(this ICard card)
        {
            return ((ulong)Crypto.PositiveRandom()) << 32 | card.Id.Condense();
        }

        public static int GenerateCardId(this ICard card)
        {
            return (Player.LocalPlayer.Id) << 16 | Program.GameEngine.GetUniqueId();
        }

        internal static int GenerateCardId()
        {
            return (Player.LocalPlayer.Id) << 16 | Program.GameEngine.GetUniqueId();
        }

        public static Octgn.Play.CardIdentity CreateIdentity(this Play.Card card)
        {
            Play.CardIdentity ret = null;
            if (card.IsVisibleToAll())
            {
                ret = card.Type;
                ret.Visible = true;
            }
            else
            {
                ret = new Play.CardIdentity(GenerateCardId());
                //ret.Alias = ret.MySecret = true;
                ret.Key = ((ulong)Crypto.PositiveRandom()) << 32 | (uint)card.Type.Id;
                ret.Visible = false;
            }
            return ret;
        }
    }
}