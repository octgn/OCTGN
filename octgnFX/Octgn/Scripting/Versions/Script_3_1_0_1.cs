using System;
using System.Collections.Generic;
using System.Linq;
using Octgn.Play;
using Octgn.Play.Gui;

namespace Octgn.Scripting.Versions
{
	[Versioned("3.1.0.1")]
    public class Script_3_1_0_1 : Script_3_1_0_0
    {

        #region Group API
        new public string GroupGetVisibility(int id)
        {
            Group g = Group.Find(id);
            DataNew.Entities.GroupVisibility vis = g.Visibility;
            switch (vis)
            {
                case DataNew.Entities.GroupVisibility.Everybody:
                    return "all";
                case DataNew.Entities.GroupVisibility.Nobody:
                    return "none";
                case DataNew.Entities.GroupVisibility.Owner:
                    return "me";
                case DataNew.Entities.GroupVisibility.Undefined:
                    return "undefined";
                case DataNew.Entities.GroupVisibility.Custom:
                    if (g.Viewers.Count == 1 && g.Viewers[0] == g.Controller)
                        return "me";
                    else
                        return "custom";
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
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
        public int[] GroupViewers(int id)
        {
            return Group.Find(id).Viewers.Select(p => (int)p.Id).ToArray();
        }
        public void GroupAddViewer(int id, int pid)
        {
            Group group = Group.Find(id);
            Player player = Player.Find((byte)pid);
            if (group.Controller != Player.LocalPlayer)
            {
                Program.GameMess.Warning("{0} can't set visibility on {0} because they don't control it.", Player.LocalPlayer.Name, group.Name);
                return;
            }
            if (group.Viewers.Contains(player)) return;
            else
            {
                 QueueAction(() => group.AddViewer(player, false));
            }
        }
        public void GroupRemoveViewer(int id, int pid)
        {
            Group group = Group.Find(id);
            Player player = Player.Find((byte)pid);
            if (group.Controller != Player.LocalPlayer)
            {
                Program.GameMess.Warning("{0} can't set visibility on {0} because they don't control it.", Player.LocalPlayer.Name, group.Name);
                return;
            }
            if (!group.Viewers.Contains(player)) return;
            else
            {
                 QueueAction(() => group.RemoveViewer(player, false));
            }
        }


        #endregion Group API

    }
}