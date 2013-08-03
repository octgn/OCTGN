namespace Octgn.Play.State
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    using Octgn.Core.DataExtensionMethods;

    [Serializable]
    public class GameEngineSaveState : StateSave<GameEngine>
    {
        public GameEngineSaveState(GameEngine instance)
            : base(instance)
        {
        }

        public override void SaveState()
        {
            if (this.Instance.RandomRequests.Any())
                throw new InvalidOperationException("Cannot save GameEngine if there are outstanding RandomRequests");

            this.Save(x => x.Definition.Id);

            this.Save(x => x.RecentCards, this.Instance.RecentCards.Select(x => x.Id).ToArray());
            this.Save(x => x.RecentMarkers, this.Instance.RecentMarkers.Select(x => x.Id).ToArray());
            this.Save(x => x.Password);
            this.Save(x => x.Nickname);
            this.Save(x => x.IsLocal);
            this.Save(x => x.StopTurn);
            this.Save(x => x.TurnPlayer, this.Instance.TurnPlayer != null ? (byte?)this.Instance.TurnPlayer.Id : null);
            this.Save(x => x.CurrentUniqueId);
            this.Save(x => x.TurnNumber);
            this.Save(x => x.Variables, this.Instance.Variables.ToDictionary(x => x.Key.Clone() as string, x => x.Value));
            this.Save(x => x.GlobalVariables, this.Instance.GlobalVariables.ToDictionary(x => x.Key.Clone() as string, x => x.Value.Clone() as string));
            this.Save(x => x.IsTableBackgroundFlipped);
            this.Save(x => x.CardsRevertToOriginalOnGroupChange);
        }

        public override void LoadState()
        {
            var id = this.LoadAndReturn(x => x.Definition.Id);
            var game = Octgn.DataNew.DbContext
                            .Get()
                            .Games
                            .First(x => x.Id == id);
            var nickname = this.LoadAndReturn(x => x.Nickname);
            var pass = this.LoadAndReturn(x => x.Password);
            var isLocal = this.LoadAndReturn(x => x.IsLocal);
            this.Instance = new GameEngine(game, nickname, pass, isLocal);

            this.LoadCustomType<IList<DataNew.Entities.Card>, Guid[]>
                (x => x.RecentCards, (instance, value) =>
                    {
                        foreach (var c in value)
                        {
                            var newC = DataNew.DbContext.Get().Cards.First(x => x.Id == c);
                            instance.RecentCards.Add(newC);
                        }
                    });
            this.LoadCustomType<IList<DataNew.Entities.Marker>, Guid[]>
                (x => x.RecentMarkers, (instance, value) =>
                    {
                        var allMarkers = game.GetAllMarkers().ToArray();
                        foreach (var c in value)
                        {
                            var mark = allMarkers.First(x => x.Id == c);
                            instance.RecentMarkers.Add(mark);
                        }
                    });
            this.Load(x=>x.StopTurn);
            this.LoadCustomType<Player,byte?>
                (x=>x.TurnPlayer,
                    (instance, value) =>
                        {
                            if (value == null)
                            {
                                instance.TurnPlayer = null;
                            }
                            else
                            {
                                instance.TurnPlayer = Player.Find(value.Value);
                            }
                        }
                );
            this.Load(x=>x.CurrentUniqueId);
            this.Load(x=>x.TurnNumber);

            this.Load(x=>x.Variables);
            this.Load(x=>x.GlobalVariables);
            this.Load(x=>x.IsTableBackgroundFlipped);
            this.Load(x=>x.CardsRevertToOriginalOnGroupChange);

        }
    }
}