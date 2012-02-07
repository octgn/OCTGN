using System;
using System.Globalization;
using System.IO;
using System.Net.Sockets;
using System.Text;
using System.Windows.Media;
using System.Xml;
using Octgn.Play;
using Octgn.Server;

namespace Octgn.Networking
{
    internal abstract class BaseXmlStub : IServerCalls
    {
        protected XmlWriterSettings xmlSettings = new XmlWriterSettings();

        protected BaseXmlStub()
        {
            xmlSettings.OmitXmlDeclaration = true;
        }

        #region IServerCalls Members

        public void Binary()
        {
            var sb = new StringBuilder();
            XmlWriter writer = XmlWriter.Create(sb, xmlSettings);

            writer.WriteStartElement("Binary");
            if (Program.Client.Muted != 0)
                writer.WriteAttributeString("muted", Program.Client.Muted.ToString(CultureInfo.InvariantCulture));
            writer.WriteEndElement();
            writer.Close();
            Send(sb.ToString());
        }

        public void Ping()
        {
            var sb = new StringBuilder();
            XmlWriter writer = XmlWriter.Create(sb, xmlSettings);

            writer.WriteStartElement("Ping");
            if (Program.Client.Muted != 0)
                writer.WriteAttributeString("muted", Program.Client.Muted.ToString(CultureInfo.InvariantCulture));
            writer.WriteEndElement();
            writer.Close();
            Send(sb.ToString());
        }

        public void IsAlternateImage(Card c, bool isAlternateImage)
        {
            var sb = new StringBuilder();
            XmlWriter writer = XmlWriter.Create(sb, xmlSettings);

            writer.WriteStartElement("IsAlternateImage");
            if (Program.Client.Muted != 0)
                writer.WriteAttributeString("muted", Program.Client.Muted.ToString(CultureInfo.InvariantCulture));
            writer.WriteElementString("cardid", c.Id.ToString(CultureInfo.InvariantCulture));
            writer.WriteElementString("isalternateimage", isAlternateImage.ToString(CultureInfo.InvariantCulture));
            writer.WriteEndElement();
            writer.Close();
            Send(sb.ToString());
        }

        public void Error(string msg)
        {
            var sb = new StringBuilder();
            XmlWriter writer = XmlWriter.Create(sb, xmlSettings);

            writer.WriteStartElement("Error");
            if (Program.Client.Muted != 0)
                writer.WriteAttributeString("muted", Program.Client.Muted.ToString(CultureInfo.InvariantCulture));
            writer.WriteElementString("msg", msg);
            writer.WriteEndElement();
            writer.Close();
            Send(sb.ToString());
        }

        public void Hello(string nick, ulong pkey, string client, Version clientVer, Version octgnVer, Guid gameId,
                          Version gameVersion)
        {
            var sb = new StringBuilder();
            XmlWriter writer = XmlWriter.Create(sb, xmlSettings);

            writer.WriteStartElement("Hello");
            if (Program.Client.Muted != 0)
                writer.WriteAttributeString("muted", Program.Client.Muted.ToString(CultureInfo.InvariantCulture));
            writer.WriteElementString("nick", nick);
            writer.WriteElementString("pkey", pkey.ToString(CultureInfo.InvariantCulture));
            writer.WriteElementString("client", client);
            writer.WriteElementString("clientVer", clientVer.ToString());
            writer.WriteElementString("octgnVer", octgnVer.ToString());
            writer.WriteElementString("gameId", gameId.ToString());
            writer.WriteElementString("gameVersion", gameVersion.ToString());
            writer.WriteEndElement();
            writer.Close();
            Send(sb.ToString());
        }

        public void Settings(bool twoSidedTable)
        {
            var sb = new StringBuilder();
            XmlWriter writer = XmlWriter.Create(sb, xmlSettings);

            writer.WriteStartElement("Settings");
            if (Program.Client.Muted != 0)
                writer.WriteAttributeString("muted", Program.Client.Muted.ToString(CultureInfo.InvariantCulture));
            writer.WriteElementString("twoSidedTable", twoSidedTable.ToString(CultureInfo.InvariantCulture));
            writer.WriteEndElement();
            writer.Close();
            Send(sb.ToString());
        }

        public void PlayerSettings(Player playerId, bool invertedTable)
        {
            var sb = new StringBuilder();
            XmlWriter writer = XmlWriter.Create(sb, xmlSettings);

            writer.WriteStartElement("PlayerSettings");
            if (Program.Client.Muted != 0)
                writer.WriteAttributeString("muted", Program.Client.Muted.ToString(CultureInfo.InvariantCulture));
            writer.WriteElementString("playerId", playerId.Id.ToString(CultureInfo.InvariantCulture));
            writer.WriteElementString("invertedTable", invertedTable.ToString(CultureInfo.InvariantCulture));
            writer.WriteEndElement();
            writer.Close();
            Send(sb.ToString());
        }

        public void NickReq(string nick)
        {
            var sb = new StringBuilder();
            XmlWriter writer = XmlWriter.Create(sb, xmlSettings);

            writer.WriteStartElement("NickReq");
            if (Program.Client.Muted != 0)
                writer.WriteAttributeString("muted", Program.Client.Muted.ToString(CultureInfo.InvariantCulture));
            writer.WriteElementString("nick", nick);
            writer.WriteEndElement();
            writer.Close();
            Send(sb.ToString());
        }

        public void Start()
        {
            var sb = new StringBuilder();
            XmlWriter writer = XmlWriter.Create(sb, xmlSettings);

            writer.WriteStartElement("Start");
            if (Program.Client.Muted != 0)
                writer.WriteAttributeString("muted", Program.Client.Muted.ToString(CultureInfo.InvariantCulture));
            writer.WriteEndElement();
            writer.Close();
            Send(sb.ToString());
        }

        public void ResetReq()
        {
            var sb = new StringBuilder();
            XmlWriter writer = XmlWriter.Create(sb, xmlSettings);

            writer.WriteStartElement("ResetReq");
            if (Program.Client.Muted != 0)
                writer.WriteAttributeString("muted", Program.Client.Muted.ToString(CultureInfo.InvariantCulture));
            writer.WriteEndElement();
            writer.Close();
            Send(sb.ToString());
        }

        public void NextTurn(Player nextPlayer)
        {
            var sb = new StringBuilder();
            XmlWriter writer = XmlWriter.Create(sb, xmlSettings);

            writer.WriteStartElement("NextTurn");
            if (Program.Client.Muted != 0)
                writer.WriteAttributeString("muted", Program.Client.Muted.ToString(CultureInfo.InvariantCulture));
            writer.WriteElementString("nextPlayer", nextPlayer.Id.ToString(CultureInfo.InvariantCulture));
            writer.WriteEndElement();
            writer.Close();
            Send(sb.ToString());
        }

        public void StopTurnReq(int turnNumber, bool stop)
        {
            var sb = new StringBuilder();
            XmlWriter writer = XmlWriter.Create(sb, xmlSettings);

            writer.WriteStartElement("StopTurnReq");
            if (Program.Client.Muted != 0)
                writer.WriteAttributeString("muted", Program.Client.Muted.ToString(CultureInfo.InvariantCulture));
            writer.WriteElementString("turnNumber", turnNumber.ToString(CultureInfo.InvariantCulture));
            writer.WriteElementString("stop", stop.ToString(CultureInfo.InvariantCulture));
            writer.WriteEndElement();
            writer.Close();
            Send(sb.ToString());
        }

        public void ChatReq(string text)
        {
            var sb = new StringBuilder();
            XmlWriter writer = XmlWriter.Create(sb, xmlSettings);

            writer.WriteStartElement("ChatReq");
            if (Program.Client.Muted != 0)
                writer.WriteAttributeString("muted", Program.Client.Muted.ToString(CultureInfo.InvariantCulture));
            writer.WriteElementString("text", text);
            writer.WriteEndElement();
            writer.Close();
            Send(sb.ToString());
        }

        public void PrintReq(string text)
        {
            var sb = new StringBuilder();
            XmlWriter writer = XmlWriter.Create(sb, xmlSettings);

            writer.WriteStartElement("PrintReq");
            if (Program.Client.Muted != 0)
                writer.WriteAttributeString("muted", Program.Client.Muted.ToString(CultureInfo.InvariantCulture));
            writer.WriteElementString("text", text);
            writer.WriteEndElement();
            writer.Close();
            Send(sb.ToString());
        }

        public void RandomReq(int id, int min, int max)
        {
            var sb = new StringBuilder();
            XmlWriter writer = XmlWriter.Create(sb, xmlSettings);

            writer.WriteStartElement("RandomReq");
            if (Program.Client.Muted != 0)
                writer.WriteAttributeString("muted", Program.Client.Muted.ToString(CultureInfo.InvariantCulture));
            writer.WriteElementString("id", id.ToString(CultureInfo.InvariantCulture));
            writer.WriteElementString("min", min.ToString(CultureInfo.InvariantCulture));
            writer.WriteElementString("max", max.ToString(CultureInfo.InvariantCulture));
            writer.WriteEndElement();
            writer.Close();
            Send(sb.ToString());
        }

        public void RandomAnswer1Req(int id, ulong value)
        {
            var sb = new StringBuilder();
            XmlWriter writer = XmlWriter.Create(sb, xmlSettings);

            writer.WriteStartElement("RandomAnswer1Req");
            if (Program.Client.Muted != 0)
                writer.WriteAttributeString("muted", Program.Client.Muted.ToString(CultureInfo.InvariantCulture));
            writer.WriteElementString("id", id.ToString(CultureInfo.InvariantCulture));
            writer.WriteElementString("value", value.ToString(CultureInfo.InvariantCulture));
            writer.WriteEndElement();
            writer.Close();
            Send(sb.ToString());
        }

        public void RandomAnswer2Req(int id, ulong value)
        {
            var sb = new StringBuilder();
            XmlWriter writer = XmlWriter.Create(sb, xmlSettings);

            writer.WriteStartElement("RandomAnswer2Req");
            if (Program.Client.Muted != 0)
                writer.WriteAttributeString("muted", Program.Client.Muted.ToString(CultureInfo.InvariantCulture));
            writer.WriteElementString("id", id.ToString(CultureInfo.InvariantCulture));
            writer.WriteElementString("value", value.ToString(CultureInfo.InvariantCulture));
            writer.WriteEndElement();
            writer.Close();
            Send(sb.ToString());
        }

        public void CounterReq(Counter counter, int value)
        {
            var sb = new StringBuilder();
            XmlWriter writer = XmlWriter.Create(sb, xmlSettings);

            writer.WriteStartElement("CounterReq");
            if (Program.Client.Muted != 0)
                writer.WriteAttributeString("muted", Program.Client.Muted.ToString(CultureInfo.InvariantCulture));
            writer.WriteElementString("counter", counter.Id.ToString(CultureInfo.InvariantCulture));
            writer.WriteElementString("value", value.ToString(CultureInfo.InvariantCulture));
            writer.WriteEndElement();
            writer.Close();
            Send(sb.ToString());
        }

        public void LoadDeck(int[] id, ulong[] type, Group[] group)
        {
            var sb = new StringBuilder();
            XmlWriter writer = XmlWriter.Create(sb, xmlSettings);

            writer.WriteStartElement("LoadDeck");
            if (Program.Client.Muted != 0)
                writer.WriteAttributeString("muted", Program.Client.Muted.ToString(CultureInfo.InvariantCulture));
            foreach (int p in id)
                writer.WriteElementString("id", p.ToString(CultureInfo.InvariantCulture));
            foreach (ulong p in type)
                writer.WriteElementString("type", p.ToString(CultureInfo.InvariantCulture));
            foreach (Group g in group)
                writer.WriteElementString("group", g.Id.ToString(CultureInfo.InvariantCulture));
            writer.WriteEndElement();
            writer.Close();
            Send(sb.ToString());
        }

        public void CreateCard(int[] id, ulong[] type, Group group)
        {
            var sb = new StringBuilder();
            XmlWriter writer = XmlWriter.Create(sb, xmlSettings);

            writer.WriteStartElement("CreateCard");
            if (Program.Client.Muted != 0)
                writer.WriteAttributeString("muted", Program.Client.Muted.ToString(CultureInfo.InvariantCulture));
            foreach (int p in id)
                writer.WriteElementString("id", p.ToString(CultureInfo.InvariantCulture));
            foreach (ulong p in type)
                writer.WriteElementString("type", p.ToString(CultureInfo.InvariantCulture));
            writer.WriteElementString("group", group.Id.ToString(CultureInfo.InvariantCulture));
            writer.WriteEndElement();
            writer.Close();
            Send(sb.ToString());
        }

        public void CreateCardAt(int[] id, ulong[] key, Guid[] modelId, int[] x, int[] y, bool faceUp, bool persist)
        {
            var sb = new StringBuilder();
            XmlWriter writer = XmlWriter.Create(sb, xmlSettings);

            writer.WriteStartElement("CreateCardAt");
            if (Program.Client.Muted != 0)
                writer.WriteAttributeString("muted", Program.Client.Muted.ToString(CultureInfo.InvariantCulture));
            foreach (int p in id)
                writer.WriteElementString("id", p.ToString(CultureInfo.InvariantCulture));
            foreach (ulong p in key)
                writer.WriteElementString("key", p.ToString(CultureInfo.InvariantCulture));
            foreach (Guid g in modelId)
                writer.WriteElementString("modelId", g.ToString());
            foreach (int p in x)
                writer.WriteElementString("x", p.ToString(CultureInfo.InvariantCulture));
            foreach (int p in y)
                writer.WriteElementString("y", p.ToString(CultureInfo.InvariantCulture));
            writer.WriteElementString("faceUp", faceUp.ToString(CultureInfo.InvariantCulture));
            writer.WriteElementString("persist", persist.ToString(CultureInfo.InvariantCulture));
            writer.WriteEndElement();
            writer.Close();
            Send(sb.ToString());
        }

        public void CreateAlias(int[] id, ulong[] type)
        {
            var sb = new StringBuilder();
            XmlWriter writer = XmlWriter.Create(sb, xmlSettings);

            writer.WriteStartElement("CreateAlias");
            if (Program.Client.Muted != 0)
                writer.WriteAttributeString("muted", Program.Client.Muted.ToString(CultureInfo.InvariantCulture));
            foreach (int p in id)
                writer.WriteElementString("id", p.ToString(CultureInfo.InvariantCulture));
            foreach (ulong p in type)
                writer.WriteElementString("type", p.ToString(CultureInfo.InvariantCulture));
            writer.WriteEndElement();
            writer.Close();
            Send(sb.ToString());
        }

        public void MoveCardReq(Card card, Group group, int idx, bool faceUp)
        {
            var sb = new StringBuilder();
            XmlWriter writer = XmlWriter.Create(sb, xmlSettings);

            writer.WriteStartElement("MoveCardReq");
            if (Program.Client.Muted != 0)
                writer.WriteAttributeString("muted", Program.Client.Muted.ToString(CultureInfo.InvariantCulture));
            writer.WriteElementString("card", card.Id.ToString(CultureInfo.InvariantCulture));
            writer.WriteElementString("group", group.Id.ToString(CultureInfo.InvariantCulture));
            writer.WriteElementString("idx", idx.ToString(CultureInfo.InvariantCulture));
            writer.WriteElementString("faceUp", faceUp.ToString(CultureInfo.InvariantCulture));
            writer.WriteEndElement();
            writer.Close();
            Send(sb.ToString());
        }

        public void MoveCardAtReq(Card card, int x, int y, int idx, bool faceUp)
        {
            var sb = new StringBuilder();
            XmlWriter writer = XmlWriter.Create(sb, xmlSettings);

            writer.WriteStartElement("MoveCardAtReq");
            if (Program.Client.Muted != 0)
                writer.WriteAttributeString("muted", Program.Client.Muted.ToString(CultureInfo.InvariantCulture));
            writer.WriteElementString("card", card.Id.ToString(CultureInfo.InvariantCulture));
            writer.WriteElementString("x", x.ToString(CultureInfo.InvariantCulture));
            writer.WriteElementString("y", y.ToString(CultureInfo.InvariantCulture));
            writer.WriteElementString("idx", idx.ToString(CultureInfo.InvariantCulture));
            writer.WriteElementString("faceUp", faceUp.ToString(CultureInfo.InvariantCulture));
            writer.WriteEndElement();
            writer.Close();
            Send(sb.ToString());
        }

        public void Reveal(Card card, ulong revealed, Guid guid)
        {
            var sb = new StringBuilder();
            XmlWriter writer = XmlWriter.Create(sb, xmlSettings);

            writer.WriteStartElement("Reveal");
            if (Program.Client.Muted != 0)
                writer.WriteAttributeString("muted", Program.Client.Muted.ToString(CultureInfo.InvariantCulture));
            writer.WriteElementString("card", card.Id.ToString(CultureInfo.InvariantCulture));
            writer.WriteElementString("revealed", revealed.ToString(CultureInfo.InvariantCulture));
            writer.WriteElementString("guid", guid.ToString());
            writer.WriteEndElement();
            writer.Close();
            Send(sb.ToString());
        }

        public void RevealToReq(Player sendTo, Player[] revealTo, Card card, ulong[] encrypted)
        {
            var sb = new StringBuilder();
            XmlWriter writer = XmlWriter.Create(sb, xmlSettings);

            writer.WriteStartElement("RevealToReq");
            if (Program.Client.Muted != 0)
                writer.WriteAttributeString("muted", Program.Client.Muted.ToString(CultureInfo.InvariantCulture));
            writer.WriteElementString("sendTo", sendTo.Id.ToString(CultureInfo.InvariantCulture));
            foreach (Player p in revealTo)
                writer.WriteElementString("revealTo", p.Id.ToString(CultureInfo.InvariantCulture));
            writer.WriteElementString("card", card.Id.ToString(CultureInfo.InvariantCulture));
            foreach (ulong p in encrypted)
                writer.WriteElementString("encrypted", p.ToString(CultureInfo.InvariantCulture));
            writer.WriteEndElement();
            writer.Close();
            Send(sb.ToString());
        }

        public void PeekReq(Card card)
        {
            var sb = new StringBuilder();
            XmlWriter writer = XmlWriter.Create(sb, xmlSettings);

            writer.WriteStartElement("PeekReq");
            if (Program.Client.Muted != 0)
                writer.WriteAttributeString("muted", Program.Client.Muted.ToString(CultureInfo.InvariantCulture));
            writer.WriteElementString("card", card.Id.ToString(CultureInfo.InvariantCulture));
            writer.WriteEndElement();
            writer.Close();
            Send(sb.ToString());
        }

        public void UntargetReq(Card card)
        {
            var sb = new StringBuilder();
            XmlWriter writer = XmlWriter.Create(sb, xmlSettings);

            writer.WriteStartElement("UntargetReq");
            if (Program.Client.Muted != 0)
                writer.WriteAttributeString("muted", Program.Client.Muted.ToString(CultureInfo.InvariantCulture));
            writer.WriteElementString("card", card.Id.ToString(CultureInfo.InvariantCulture));
            writer.WriteEndElement();
            writer.Close();
            Send(sb.ToString());
        }

        public void TargetReq(Card card)
        {
            var sb = new StringBuilder();
            XmlWriter writer = XmlWriter.Create(sb, xmlSettings);

            writer.WriteStartElement("TargetReq");
            if (Program.Client.Muted != 0)
                writer.WriteAttributeString("muted", Program.Client.Muted.ToString(CultureInfo.InvariantCulture));
            writer.WriteElementString("card", card.Id.ToString(CultureInfo.InvariantCulture));
            writer.WriteEndElement();
            writer.Close();
            Send(sb.ToString());
        }

        public void TargetArrowReq(Card card, Card otherCard)
        {
            var sb = new StringBuilder();
            XmlWriter writer = XmlWriter.Create(sb, xmlSettings);

            writer.WriteStartElement("TargetArrowReq");
            if (Program.Client.Muted != 0)
                writer.WriteAttributeString("muted", Program.Client.Muted.ToString(CultureInfo.InvariantCulture));
            writer.WriteElementString("card", card.Id.ToString(CultureInfo.InvariantCulture));
            writer.WriteElementString("otherCard", otherCard.Id.ToString(CultureInfo.InvariantCulture));
            writer.WriteEndElement();
            writer.Close();
            Send(sb.ToString());
        }

        public void Highlight(Card card, Color? color)
        {
            var sb = new StringBuilder();
            XmlWriter writer = XmlWriter.Create(sb, xmlSettings);

            writer.WriteStartElement("Highlight");
            if (Program.Client.Muted != 0)
                writer.WriteAttributeString("muted", Program.Client.Muted.ToString(CultureInfo.InvariantCulture));
            writer.WriteElementString("card", card.Id.ToString(CultureInfo.InvariantCulture));
            writer.WriteElementString("color", color == null ? "" : color.ToString());
            writer.WriteEndElement();
            writer.Close();
            Send(sb.ToString());
        }

        public void TurnReq(Card card, bool up)
        {
            var sb = new StringBuilder();
            XmlWriter writer = XmlWriter.Create(sb, xmlSettings);

            writer.WriteStartElement("TurnReq");
            if (Program.Client.Muted != 0)
                writer.WriteAttributeString("muted", Program.Client.Muted.ToString(CultureInfo.InvariantCulture));
            writer.WriteElementString("card", card.Id.ToString(CultureInfo.InvariantCulture));
            writer.WriteElementString("up", up.ToString(CultureInfo.InvariantCulture));
            writer.WriteEndElement();
            writer.Close();
            Send(sb.ToString());
        }

        public void RotateReq(Card card, CardOrientation rot)
        {
            var sb = new StringBuilder();
            XmlWriter writer = XmlWriter.Create(sb, xmlSettings);

            writer.WriteStartElement("RotateReq");
            if (Program.Client.Muted != 0)
                writer.WriteAttributeString("muted", Program.Client.Muted.ToString(CultureInfo.InvariantCulture));
            writer.WriteElementString("card", card.Id.ToString(CultureInfo.InvariantCulture));
            writer.WriteElementString("rot", rot.ToString());
            writer.WriteEndElement();
            writer.Close();
            Send(sb.ToString());
        }

        public void Shuffle(Group group, int[] card)
        {
            var sb = new StringBuilder();
            XmlWriter writer = XmlWriter.Create(sb, xmlSettings);

            writer.WriteStartElement("Shuffle");
            if (Program.Client.Muted != 0)
                writer.WriteAttributeString("muted", Program.Client.Muted.ToString(CultureInfo.InvariantCulture));
            writer.WriteElementString("group", group.Id.ToString(CultureInfo.InvariantCulture));
            foreach (int p in card)
                writer.WriteElementString("card", p.ToString(CultureInfo.InvariantCulture));
            writer.WriteEndElement();
            writer.Close();
            Send(sb.ToString());
        }

        public void Shuffled(Group group, int[] card, short[] pos)
        {
            var sb = new StringBuilder();
            XmlWriter writer = XmlWriter.Create(sb, xmlSettings);

            writer.WriteStartElement("Shuffled");
            if (Program.Client.Muted != 0)
                writer.WriteAttributeString("muted", Program.Client.Muted.ToString(CultureInfo.InvariantCulture));
            writer.WriteElementString("group", group.Id.ToString(CultureInfo.InvariantCulture));
            foreach (int p in card)
                writer.WriteElementString("card", p.ToString(CultureInfo.InvariantCulture));
            foreach (short p in pos)
                writer.WriteElementString("pos", p.ToString(CultureInfo.InvariantCulture));
            writer.WriteEndElement();
            writer.Close();
            Send(sb.ToString());
        }

        public void UnaliasGrp(Group group)
        {
            var sb = new StringBuilder();
            XmlWriter writer = XmlWriter.Create(sb, xmlSettings);

            writer.WriteStartElement("UnaliasGrp");
            if (Program.Client.Muted != 0)
                writer.WriteAttributeString("muted", Program.Client.Muted.ToString(CultureInfo.InvariantCulture));
            writer.WriteElementString("group", group.Id.ToString(CultureInfo.InvariantCulture));
            writer.WriteEndElement();
            writer.Close();
            Send(sb.ToString());
        }

        public void Unalias(int[] card, ulong[] type)
        {
            var sb = new StringBuilder();
            XmlWriter writer = XmlWriter.Create(sb, xmlSettings);

            writer.WriteStartElement("Unalias");
            if (Program.Client.Muted != 0)
                writer.WriteAttributeString("muted", Program.Client.Muted.ToString(CultureInfo.InvariantCulture));
            foreach (int p in card)
                writer.WriteElementString("card", p.ToString(CultureInfo.InvariantCulture));
            foreach (ulong p in type)
                writer.WriteElementString("type", p.ToString(CultureInfo.InvariantCulture));
            writer.WriteEndElement();
            writer.Close();
            Send(sb.ToString());
        }

        public void AddMarkerReq(Card card, Guid id, string name, ushort count)
        {
            var sb = new StringBuilder();
            XmlWriter writer = XmlWriter.Create(sb, xmlSettings);

            writer.WriteStartElement("AddMarkerReq");
            if (Program.Client.Muted != 0)
                writer.WriteAttributeString("muted", Program.Client.Muted.ToString(CultureInfo.InvariantCulture));
            writer.WriteElementString("card", card.Id.ToString(CultureInfo.InvariantCulture));
            writer.WriteElementString("id", id.ToString());
            writer.WriteElementString("name", name);
            writer.WriteElementString("count", count.ToString(CultureInfo.InvariantCulture));
            writer.WriteEndElement();
            writer.Close();
            Send(sb.ToString());
        }

        public void RemoveMarkerReq(Card card, Guid id, string name, ushort count)
        {
            var sb = new StringBuilder();
            XmlWriter writer = XmlWriter.Create(sb, xmlSettings);

            writer.WriteStartElement("RemoveMarkerReq");
            if (Program.Client.Muted != 0)
                writer.WriteAttributeString("muted", Program.Client.Muted.ToString(CultureInfo.InvariantCulture));
            writer.WriteElementString("card", card.Id.ToString(CultureInfo.InvariantCulture));
            writer.WriteElementString("id", id.ToString());
            writer.WriteElementString("name", name);
            writer.WriteElementString("count", count.ToString(CultureInfo.InvariantCulture));
            writer.WriteEndElement();
            writer.Close();
            Send(sb.ToString());
        }

        public void SetMarkerReq(Card card, Guid id, string name, ushort count)
        {
            var sb = new StringBuilder();
            XmlWriter writer = XmlWriter.Create(sb, xmlSettings);

            writer.WriteStartElement("SetMarkerReq");
            if (Program.Client.Muted != 0)
                writer.WriteAttributeString("muted", Program.Client.Muted.ToString(CultureInfo.InvariantCulture));
            writer.WriteElementString("card", card.Id.ToString(CultureInfo.InvariantCulture));
            writer.WriteElementString("id", id.ToString());
            writer.WriteElementString("name", name);
            writer.WriteElementString("count", count.ToString(CultureInfo.InvariantCulture));
            writer.WriteEndElement();
            writer.Close();
            Send(sb.ToString());
        }

        public void TransferMarkerReq(Card from, Card to, Guid id, string name, ushort count)
        {
            var sb = new StringBuilder();
            XmlWriter writer = XmlWriter.Create(sb, xmlSettings);

            writer.WriteStartElement("TransferMarkerReq");
            if (Program.Client.Muted != 0)
                writer.WriteAttributeString("muted", Program.Client.Muted.ToString(CultureInfo.InvariantCulture));
            writer.WriteElementString("from", from.Id.ToString(CultureInfo.InvariantCulture));
            writer.WriteElementString("to", to.Id.ToString(CultureInfo.InvariantCulture));
            writer.WriteElementString("id", id.ToString());
            writer.WriteElementString("name", name);
            writer.WriteElementString("count", count.ToString(CultureInfo.InvariantCulture));
            writer.WriteEndElement();
            writer.Close();
            Send(sb.ToString());
        }

        public void PassToReq(ControllableObject id, Player to, bool requested)
        {
            var sb = new StringBuilder();
            XmlWriter writer = XmlWriter.Create(sb, xmlSettings);

            writer.WriteStartElement("PassToReq");
            if (Program.Client.Muted != 0)
                writer.WriteAttributeString("muted", Program.Client.Muted.ToString(CultureInfo.InvariantCulture));
            writer.WriteElementString("id", id.Id.ToString(CultureInfo.InvariantCulture));
            writer.WriteElementString("to", to.Id.ToString(CultureInfo.InvariantCulture));
            writer.WriteElementString("requested", requested.ToString(CultureInfo.InvariantCulture));
            writer.WriteEndElement();
            writer.Close();
            Send(sb.ToString());
        }

        public void TakeFromReq(ControllableObject id, Player from)
        {
            var sb = new StringBuilder();
            XmlWriter writer = XmlWriter.Create(sb, xmlSettings);

            writer.WriteStartElement("TakeFromReq");
            if (Program.Client.Muted != 0)
                writer.WriteAttributeString("muted", Program.Client.Muted.ToString(CultureInfo.InvariantCulture));
            writer.WriteElementString("id", id.Id.ToString(CultureInfo.InvariantCulture));
            writer.WriteElementString("from", from.Id.ToString(CultureInfo.InvariantCulture));
            writer.WriteEndElement();
            writer.Close();
            Send(sb.ToString());
        }

        public void DontTakeReq(ControllableObject id, Player to)
        {
            var sb = new StringBuilder();
            XmlWriter writer = XmlWriter.Create(sb, xmlSettings);

            writer.WriteStartElement("DontTakeReq");
            if (Program.Client.Muted != 0)
                writer.WriteAttributeString("muted", Program.Client.Muted.ToString(CultureInfo.InvariantCulture));
            writer.WriteElementString("id", id.Id.ToString(CultureInfo.InvariantCulture));
            writer.WriteElementString("to", to.Id.ToString(CultureInfo.InvariantCulture));
            writer.WriteEndElement();
            writer.Close();
            Send(sb.ToString());
        }

        public void FreezeCardsVisibility(Group group)
        {
            var sb = new StringBuilder();
            XmlWriter writer = XmlWriter.Create(sb, xmlSettings);

            writer.WriteStartElement("FreezeCardsVisibility");
            if (Program.Client.Muted != 0)
                writer.WriteAttributeString("muted", Program.Client.Muted.ToString(CultureInfo.InvariantCulture));
            writer.WriteElementString("group", group.Id.ToString(CultureInfo.InvariantCulture));
            writer.WriteEndElement();
            writer.Close();
            Send(sb.ToString());
        }

        public void GroupVisReq(Group group, bool defined, bool visible)
        {
            var sb = new StringBuilder();
            XmlWriter writer = XmlWriter.Create(sb, xmlSettings);

            writer.WriteStartElement("GroupVisReq");
            if (Program.Client.Muted != 0)
                writer.WriteAttributeString("muted", Program.Client.Muted.ToString(CultureInfo.InvariantCulture));
            writer.WriteElementString("group", group.Id.ToString(CultureInfo.InvariantCulture));
            writer.WriteElementString("defined", defined.ToString(CultureInfo.InvariantCulture));
            writer.WriteElementString("visible", visible.ToString(CultureInfo.InvariantCulture));
            writer.WriteEndElement();
            writer.Close();
            Send(sb.ToString());
        }

        public void GroupVisAddReq(Group group, Player who)
        {
            var sb = new StringBuilder();
            XmlWriter writer = XmlWriter.Create(sb, xmlSettings);

            writer.WriteStartElement("GroupVisAddReq");
            if (Program.Client.Muted != 0)
                writer.WriteAttributeString("muted", Program.Client.Muted.ToString(CultureInfo.InvariantCulture));
            writer.WriteElementString("group", group.Id.ToString(CultureInfo.InvariantCulture));
            writer.WriteElementString("who", who.Id.ToString(CultureInfo.InvariantCulture));
            writer.WriteEndElement();
            writer.Close();
            Send(sb.ToString());
        }

        public void GroupVisRemoveReq(Group group, Player who)
        {
            var sb = new StringBuilder();
            XmlWriter writer = XmlWriter.Create(sb, xmlSettings);

            writer.WriteStartElement("GroupVisRemoveReq");
            if (Program.Client.Muted != 0)
                writer.WriteAttributeString("muted", Program.Client.Muted.ToString(CultureInfo.InvariantCulture));
            writer.WriteElementString("group", group.Id.ToString(CultureInfo.InvariantCulture));
            writer.WriteElementString("who", who.Id.ToString(CultureInfo.InvariantCulture));
            writer.WriteEndElement();
            writer.Close();
            Send(sb.ToString());
        }

        public void LookAtReq(int uid, Group group, bool look)
        {
            var sb = new StringBuilder();
            XmlWriter writer = XmlWriter.Create(sb, xmlSettings);

            writer.WriteStartElement("LookAtReq");
            if (Program.Client.Muted != 0)
                writer.WriteAttributeString("muted", Program.Client.Muted.ToString(CultureInfo.InvariantCulture));
            writer.WriteElementString("uid", uid.ToString(CultureInfo.InvariantCulture));
            writer.WriteElementString("group", group.Id.ToString(CultureInfo.InvariantCulture));
            writer.WriteElementString("look", look.ToString(CultureInfo.InvariantCulture));
            writer.WriteEndElement();
            writer.Close();
            Send(sb.ToString());
        }

        public void LookAtTopReq(int uid, Group group, int count, bool look)
        {
            var sb = new StringBuilder();
            XmlWriter writer = XmlWriter.Create(sb, xmlSettings);

            writer.WriteStartElement("LookAtTopReq");
            if (Program.Client.Muted != 0)
                writer.WriteAttributeString("muted", Program.Client.Muted.ToString(CultureInfo.InvariantCulture));
            writer.WriteElementString("uid", uid.ToString(CultureInfo.InvariantCulture));
            writer.WriteElementString("group", group.Id.ToString(CultureInfo.InvariantCulture));
            writer.WriteElementString("count", count.ToString(CultureInfo.InvariantCulture));
            writer.WriteElementString("look", look.ToString(CultureInfo.InvariantCulture));
            writer.WriteEndElement();
            writer.Close();
            Send(sb.ToString());
        }

        public void LookAtBottomReq(int uid, Group group, int count, bool look)
        {
            var sb = new StringBuilder();
            XmlWriter writer = XmlWriter.Create(sb, xmlSettings);

            writer.WriteStartElement("LookAtBottomReq");
            if (Program.Client.Muted != 0)
                writer.WriteAttributeString("muted", Program.Client.Muted.ToString(CultureInfo.InvariantCulture));
            writer.WriteElementString("uid", uid.ToString(CultureInfo.InvariantCulture));
            writer.WriteElementString("group", group.Id.ToString(CultureInfo.InvariantCulture));
            writer.WriteElementString("count", count.ToString(CultureInfo.InvariantCulture));
            writer.WriteElementString("look", look.ToString(CultureInfo.InvariantCulture));
            writer.WriteEndElement();
            writer.Close();
            Send(sb.ToString());
        }

        public void StartLimitedReq(Guid[] packs)
        {
            var sb = new StringBuilder();
            XmlWriter writer = XmlWriter.Create(sb, xmlSettings);

            writer.WriteStartElement("StartLimitedReq");
            if (Program.Client.Muted != 0)
                writer.WriteAttributeString("muted", Program.Client.Muted.ToString(CultureInfo.InvariantCulture));
            foreach (Guid g in packs)
                writer.WriteElementString("packs", g.ToString());
            writer.WriteEndElement();
            writer.Close();
            Send(sb.ToString());
        }

        public void CancelLimitedReq()
        {
            var sb = new StringBuilder();
            XmlWriter writer = XmlWriter.Create(sb, xmlSettings);

            writer.WriteStartElement("CancelLimitedReq");
            if (Program.Client.Muted != 0)
                writer.WriteAttributeString("muted", Program.Client.Muted.ToString(CultureInfo.InvariantCulture));
            writer.WriteEndElement();
            writer.Close();
            Send(sb.ToString());
        }

        public void PlayerSetGlobalVariable(Player p, string n, string v)
        {
            var sb = new StringBuilder();
            XmlWriter writer = XmlWriter.Create(sb, xmlSettings);

            writer.WriteStartElement("PlayerSetGlobalVariable");
            if (Program.Client.Muted != 0)
                writer.WriteAttributeString("muted", Program.Client.Muted.ToString(CultureInfo.InvariantCulture));
            writer.WriteElementString("who", p.Id.ToString(CultureInfo.InvariantCulture));
            writer.WriteElementString("name", n);
            writer.WriteElementString("value", v);
            writer.WriteEndElement();
            writer.Close();
            Send(sb.ToString());
        }

        public void SetGlobalVariable(string n, string v)
        {
            var sb = new StringBuilder();
            XmlWriter writer = XmlWriter.Create(sb, xmlSettings);

            writer.WriteStartElement("SetGlobalVariable");
            if (Program.Client.Muted != 0)
                writer.WriteAttributeString("muted", Program.Client.Muted.ToString(CultureInfo.InvariantCulture));
            writer.WriteElementString("name", n);
            writer.WriteElementString("value", v);
            writer.WriteEndElement();
            writer.Close();
            Send(sb.ToString());
        }

        #endregion

        protected abstract void Send(string xml);
    }

    internal class XmlSenderStub : BaseXmlStub
    {
        private readonly TcpClient to;
        private byte[] buffer = new byte[1024];

        public XmlSenderStub(TcpClient to)
        {
            this.to = to;
        }

        protected override void Send(string xml)
        {
            int length = Encoding.UTF8.GetByteCount(xml) + 1;
            if (length > buffer.Length) buffer = new byte[length];
            Encoding.UTF8.GetBytes(xml, 0, xml.Length, buffer, 0);
            buffer[length - 1] = 0;
            try
            {
                Stream stream = to.GetStream();
                stream.Write(buffer, 0, length);
                stream.Flush();
            }
            catch
            {
                Program.Client.Disconnected();
            }
        }
    }
}