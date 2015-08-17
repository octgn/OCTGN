using System;

namespace Octgn.DataNew.Entities
{
    public class GameBoard
    {
        public string Name { get; set; }
        public double XPos { get; set; }
        public double YPos { get; set; }
        public double Width { get; set; }
        public double Height { get; set; }
        public string Source { get; set; }

        public GameBoard()
        {

        }
    }
}