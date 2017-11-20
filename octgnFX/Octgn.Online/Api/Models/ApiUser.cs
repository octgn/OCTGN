using System;
using System.Collections.Generic;

namespace Octgn.Site.Api.Models
{
    public class ApiUser
    {
        public int Id { get; set; }
        public string UserName { get; set; }
        public bool IsSubscribed { get; set; }
        public string Tier { get; set; }
        public string IconUrl { get; set; }
        public string ImageUrl { get; set; }
        public int DisconnectPercent { get; set; }
        public List<ApiUserExperience> Experience { get; set; }
        public int TotalSecondsPlayed { get; set; }
        public int AverageGameLength { get; set; }
        public int TotalGamesPlayed { get; set; }
        public int Level { get; set; }
    }

    public class ApiUserExperience
    {
        public int TotalSecondsPlayed { get; set; }
        public int AverageGameLength { get; set; }
        public int TotalGamesPlayed { get; set; }
        public int Level { get; set; }
        public ApiGameInfo Game { get; set; }
    }

    public class ApiGameInfo
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string IconUrl { get; set; }
    }
}
