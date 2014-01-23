using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using Octgn.Core;
using Octgn.Core.DataExtensionMethods;
using Octgn.Core.Util;
using Octgn.Extentions;
using Octgn.Networking;
using Octgn.Play;
using Octgn.Play.Actions;
using Octgn.Play.Gui;
using Octgn.Scripting.Controls;
using Octgn.Utils;

namespace Octgn.Scripting.Versions
{
    public class Script_3_1_0_1 : ScriptBase
    {
        private static readonly Version _version = new Version(3, 1, 0, 1);
        private static readonly DateTime _releaseDate = new DateTime(2014, 1, 22, 13, 0, 0);
        private static readonly DateTime _deprecatedDate = /*StartAutoFill*/ DateTime.MaxValue /*EndAutoFill*/;
        private static readonly ScriptBase _inherits = new Script_3_1_0_0();
        private const ReleaseMode _releaseMode = ReleaseMode.Test;

        #region Overrides of ScriptBase

        public override Version Version
        {
            get { return _version; }
        }

        public override DateTime ReleaseDate
        {
            get { return _releaseDate; }
        }

        public override DateTime DeprecatedDate
        {
            get { return _deprecatedDate; }
        }

        public override ReleaseMode ReleaseMode
        {
            get { return _releaseMode; }
        }

        public override ScriptBase Inherits
        {
            get { return _inherits; }
        }

        #endregion

        #region Counter API

        public void CounterSet(int id, int value)
        {
            Counter counter = Counter.Find(id);
            QueueAction(
                () =>
                {
                    Program.GameEngine.EventProxy.MuteEvents = true;
                    counter.Value = value;
                    Program.GameEngine.EventProxy.MuteEvents = false;
                });
        }

        #endregion Counter API

        #region Group API

        public bool GroupGetCollapsed(int id)
        {
            var g = Group.Find(id);
            if (!(g is Pile)) return false;
            Pile pile = (Pile)g;
            return pile.Collapsed;
        }
        public void GroupSetCollapsed(int id, bool value)
        {
            var g = Group.Find(id);
            if (!(g is Pile)) return;
            Pile pile = (Pile)g;
            QueueAction(() => pile.Collapsed = value);
        }
        public void GroupLookAtTop(int id, int value)
        {
            var g = (Pile)Group.Find(id);
            if (g.Controller != Player.LocalPlayer)
            {
                Program.GameMess.Warning(String.Format("{0} can't look at {1} because they don't control it.", Player.LocalPlayer.Name, g.Name));
            }
            PlayWindow playWindow = WindowManager.PlayWindow;
            if (playWindow == null) return;
            Octgn.Controls.ChildWindowManager manager = playWindow.wndManager;
            if (value != 0) QueueAction(() => manager.Show(new GroupWindow(@g, PilePosition.Top, value)));
            else QueueAction(() => manager.Show(new GroupWindow(@g, PilePosition.All, 0)));
        }

        #endregion Group API

        #region Card API

        public void CardMoveTo(int cardId, int groupId, int? position)
        {
            Card card = Card.Find(cardId);
            Group group = Group.Find(groupId);

            if (card.Controller != Player.LocalPlayer)
                Program.GameMess.Warning(String.Format("{0} Can't move {1} to {2} because they don't control {1}.", Player.LocalPlayer.Name, card.Name, card.Name));

            if (group.Controller != Player.LocalPlayer)
                Program.GameMess.Warning(String.Format("{0} Can't move {1} to {2} because they don't control {1}.", Player.LocalPlayer.Name, card.Name, group.Name));

            if (card.Group != Program.GameEngine.Table && card.Group.Controller != Player.LocalPlayer)
                Program.GameMess.Warning(String.Format("{0} Can't move {1} from {2} because they don't control it.", Player.LocalPlayer.Name, card, card.Group));

            QueueAction(() =>
            {
                Program.GameEngine.EventProxy.MuteEvents = true;
                if (position == null) card.MoveTo(group, true, true);
                else card.MoveTo(group, true, position.Value, true);
                Program.GameEngine.EventProxy.MuteEvents = false;
            });
        }

        public void CardMoveToTable(int cardId, double x, double y, bool forceFaceDown)
        {
            Card card = Card.Find(cardId);

            if (card.Controller != Player.LocalPlayer)
                Program.GameMess.Warning(String.Format("{0} Can't move {1} to Table because they don't control {1}.", Player.LocalPlayer.Name, card.Name));

            if (card.Group != Program.GameEngine.Table && card.Group.Controller != Player.LocalPlayer)
                Program.GameMess.Warning(String.Format("{0} Can't move {1} from {2} because they don't control it.", Player.LocalPlayer.Name, card, card.Group));

            bool faceUp = !forceFaceDown && (!(card.Group is Table) || card.FaceUp);
            QueueAction(
                () =>
                {
                    Program.GameEngine.EventProxy.MuteEvents = true;
                    card.MoveToTable((int)x, (int)y, faceUp, Program.GameEngine.Table.Count, true);
                    Program.GameEngine.EventProxy.MuteEvents = false;
                });
        }

        public void CardSetIndex(int CardId, int idx, bool TableOnly = false)
        {
            if (idx < 0)
            {
                Program.GameMess.Warning("Cannot setIndex({0}), number is less than 0", idx);
                return;
            }
            Card card = Card.Find(CardId);

            if (card.Controller != Player.LocalPlayer)
                Program.GameMess.Warning(String.Format("{0} Can't set index of {1} to Table because they don't control {1}.", Player.LocalPlayer.Name, card.Name));

            if (card.Group != Program.GameEngine.Table && card.Group.Controller != Player.LocalPlayer)
                Program.GameMess.Warning(String.Format("{0} Can't set index of {1} in {2} because they don't control it.", Player.LocalPlayer.Name, card, card.Group));

            if (TableOnly)
            {
                if (card.Group is Table)
                    QueueAction(
                        () =>
                        {
                            Program.GameEngine.EventProxy.MuteEvents = true;
                            card.MoveToTable((int)card.X, (int)card.Y, card.FaceUp, idx, true);
                            Program.GameEngine.EventProxy.MuteEvents = false;
                        });
            }
            else
                QueueAction(
                    () =>
                    {
                        Program.GameEngine.EventProxy.MuteEvents = true;
                        card.MoveToTable((int)card.X, (int)card.Y, card.FaceUp, idx, true);
                        Program.GameEngine.EventProxy.MuteEvents = false;
                    });
        }

        public void CardTarget(int id, bool active)
        {
            Card c = Card.Find(id);
            QueueAction(() =>
            {
                Program.GameEngine.EventProxy.MuteEvents = true;
                if (active) c.Target();
                else c.Untarget();
                Program.GameEngine.EventProxy.MuteEvents = false;
            });
        }

        public void CardTargetArrow(int id, int targetId, bool active)
        {
            Card c = Card.Find(id);
            Card target = Card.Find(targetId);
            QueueAction(() =>
            {
                Program.GameEngine.EventProxy.MuteEvents = true;
                if (active) c.Target(target);
                else c.Untarget();
                Program.GameEngine.EventProxy.MuteEvents = false;
            });
        }



        #endregion Card API

    }
}