using Octgn.Core.Play;
using Octgn.Play;
using Octgn.Play.Gui;
using Octgn.Windows;
using System;
using System.Threading.Tasks;

namespace Octgn.Loaders
{
    public class GameMessageLoader : ILoader
    {
        private readonly log4net.ILog Log
            = log4net.LogManager.GetLogger(typeof(GameMessageLoader));

        public string Name { get; } = "Game messages";

        public Task Load(ILoadingView view) {
            return Task.Run(LoadSync);
        }

        private void LoadSync() {
            GameMessage.MuteChecker = () =>
            {
                if (Program.Client == null) return false;
                return Program.Client.Muted != 0;
            };

            Log.Info("Creating Game Message Dispatcher");
            var messageDispatcher = new GameMessageDispatcher();
            messageDispatcher.ProcessMessage(
                x =>
                {
                    for (var i = 0; i < x.Arguments.Length; i++)
                    {
                        var arg = x.Arguments[i];
                        var cardModel = arg as DataNew.Entities.Card;
                        var cardId = arg as CardIdentity;
                        var card = arg as Card;
                        if (card != null && (card.FaceUp || card.MayBeConsideredFaceUp))
                            cardId = card.Type;

                        if (cardId != null || cardModel != null)
                        {
                            ChatCard chatCard = null;
                            if (cardId != null)
                            {
                                chatCard = new ChatCard(cardId);
                            }
                            else
                            {
                                chatCard = new ChatCard(cardModel);
                            }
                            if (card != null)
                                chatCard.SetGameCard(card);
                            x.Arguments[i] = chatCard;
                        }
                        else
                        {
                            x.Arguments[i] = arg == null ? "[?]" : arg.ToString();
                        }
                    }
                    return x;
                });

            Program.GameMess = messageDispatcher;
        }
    }
}
