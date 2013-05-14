namespace Octgn.Play
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Windows.Input;

    using Octgn.DataNew.Entities;

    public interface IPlayGroup : IPlayControllableObject,IEnumerable<IPlayCard>
    {
        Dictionary<int, List<IPlayCard>> LookedAt { get; }
        DataNew.Entities.Group Definition { get; }

        KeyGesture MoveToShortcut { get; }

        GroupVisibility Visibility { get; }

        bool Ordered { get; }

        ObservableCollection<IPlayCard> Cards { get; }

        int Count { get; }

        bool Locked { get; set; }

        IPlayCard this[int idx] { get; }

        void AddAt(IPlayCard card, int idx);

        void Remove(IPlayCard card);

        string ToString();

        event EventHandler<TraceEventArgs> ShuffledTrace;

        event EventHandler Shuffled;

        void FreezeCardsVisibility(bool notifyServer);

        void SetVisibility(bool? visible, bool notifyServer);

        void AddViewer(IPlayPlayer player, bool notifyServer);

        void RemoveViewer(IPlayPlayer player, bool notifyServer);

        void OnShuffled();

        void SetCardIndex(IPlayCard card, int idx);

        int GetCardIndex(IPlayCard card);

        IPlayCard FindByCardIdentity(CardIdentity identity);

        void ResetVisibility();

        void Reset();

        int FindNextFreeSlot(int slot);
    }
}