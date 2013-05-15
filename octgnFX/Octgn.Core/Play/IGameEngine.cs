namespace Octgn
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Windows.Media.Imaging;

    using Octgn.DataNew.Entities;
    using Octgn.Play;

    public interface IGameEngine
    {
        bool IsLocal { get; }

        int TurnNumber { get; set; }

        IPlayPlayer TurnPlayer { get; set; }

        bool StopTurn { get; set; }

        IPlayTable Table { get; }

        Game Definition { get; }

        //BitmapImage CardFrontBitmap { get; }

        //BitmapImage CardBackBitmap { get; }

        IList<RandomRequest> RandomRequests { get; }

        IList<DataNew.Entities.Marker> Markers { get; }

        IList<DataNew.Entities.Marker> RecentMarkers { get; }

        IList<DataNew.Entities.Card> RecentCards { get; }

        Dictionary<string, int> Variables { get; }

        Dictionary<string, string> GlobalVariables { get; }

        bool IsTableBackgroundFlipped { get; set; }

        event PropertyChangedEventHandler PropertyChanged;

        void Begin();

        void TestBegin();

        void Reset();

        void End();

        void StartLimited(IPlayPlayer player, Guid[] packs);

        ushort GetUniqueId();

        int GenerateCardId();

        RandomRequest FindRandomRequest(int id);

        void LoadDeck(IDeck deck);

        void AddRecentCard(DataNew.Entities.Card card);

        void AddRecentMarker(DataNew.Entities.Marker marker);

        DataNew.Entities.Marker GetMarkerModel(Guid id);

        void ComposeParts(params object[] attributedParts);
    }
}