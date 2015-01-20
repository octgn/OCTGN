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
            int id = card.GenerateCardId();
            var retCard = new Play.Card(player, id, Program.GameEngine.Definition.GetCardById(card.Id), true, card.Size.Name);            return retCard;
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
    }
}