namespace Octgn.DataNew.Entities
{
    using System.Collections.Generic;

    public interface IPlayer
    {
        IEnumerable<Counter> Counters { get; set; }
        IEnumerable<Group> Groups { get; set; } 
    }

    public class Player : IPlayer
    {
        public IEnumerable<Counter> Counters { get; set; }
        public IEnumerable<Group> Groups { get; set; }
        public string IndicatorsFormat { get; set; }
        public Group Hand { get; set; }
        public List<GlobalVariable> GlobalVariables { get; set; } 
    }

    public class GlobalPlayer : IPlayer
    {
        public IEnumerable<Counter> Counters { get; set; }
        public IEnumerable<Group> Groups { get; set; }
    }
}