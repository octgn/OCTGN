using System;
using System.Windows.Media;

namespace Octgn.Core.Play.Save
{
    [Serializable]
    public class PlayerSaveState : IPlayerSaveState
    {
        public byte Id { get; set; }
        public string Nickname { get; set; }
        public Color Color { get; set; }
    }
}