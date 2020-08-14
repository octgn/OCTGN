﻿namespace Octgn.DataNew.Entities
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
        public Dictionary<string, GlobalVariable> GlobalVariables { get; set; } 

        public Player()
        {
            Groups = new List<Group>();
            Counters = new List<Counter>();
            GlobalVariables = new Dictionary<string, GlobalVariable>();
        }
    }

    public class GlobalPlayer : IPlayer
    {
        public IEnumerable<Counter> Counters { get; set; }
        public IEnumerable<Group> Groups { get; set; }
        public string IndicatorsFormat { get; set; }
        public GlobalPlayer()
        {
            Groups = new List<Group>();
            Counters = new List<Counter>();
        }
    }
}