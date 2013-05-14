namespace Octgn.Play
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.ComponentModel;
    using System.Windows.Media;

    using Octgn.DataNew.Entities;

    public interface IPlayCard : IPlayControllableObject
    {
        List<IPlayPlayer> PlayersLooking { get; }
        string RealName { get; }

        CardIdentity Type { get; set; }

        bool DeleteWhenLeavesGroup { get; set; }

        IPlayGroup Group { get; set; }

        bool FaceUp { get; set; }

        bool OverrideGroupVisibility { get; }

        CardOrientation Orientation { get; set; }

        bool Selected { get; set; }

        double X { get; set; }

        double Y { get; set; }

        IPlayPlayer TargetedBy { get; }

        bool TargetsOtherCards { get; set; }

        Color? HighlightColor { get; set; }

        bool IsHighlighted { get; }

        ObservableCollection<IPlayPlayer> PeekingPlayers { get; }

        string Picture { get; }

        IList<PlayMarker> Markers { get; }

        void SetFaceUp(bool lFaceUp);

        void ToggleTarget();

        void Target();

        void Untarget();

        void Target(IPlayCard otherCard);

        string ToString();

        string[] Alternates();

        string Alternate();

        void SwitchTo(IPlayPlayer player, string alternate = "");

        object GetProperty(string name);

        void MoveTo(IPlayGroup to, bool lFaceUp);

        void MoveTo(IPlayGroup to, bool lFaceUp, int idx);

        void MoveToTable(int x, int y, bool lFaceUp, int idx);

        int GetIndex();

        void SetIndex(int idx);

        void Peek();

        string GetPicture(bool up);

        void SetOrientation(CardOrientation value);

        void SetOverrideGroupVisibility(bool overrides);

        void SetTargetedBy(IPlayPlayer player);

        void SetHighlight(Color? value);

        void SetVisibility(GroupVisibility visibility, List<IPlayPlayer> viewers);

        void SetModel(DataNew.Entities.Card model);

        bool IsVisibleToAll();

        void Reveal();

        void RevealTo(IEnumerable<IPlayPlayer> players);

        void AddMarker(DataNew.Entities.Marker model, ushort count);

        void AddMarker(DataNew.Entities.Marker model);

        int RemoveMarker(PlayMarker marker, ushort count);

        void RemoveMarker(PlayMarker marker);

        PlayMarker FindMarker(Guid lId, string name);

        void SetMarker(IPlayPlayer player, Guid lId, string name, int count);

        event PropertyChangedEventHandler PropertyChanged;
    }
}