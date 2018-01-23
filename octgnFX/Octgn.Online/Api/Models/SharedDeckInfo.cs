namespace Octgn.Site.Api.Models
{
    using System;

    public class SharedDeckInfo
    {
		public string Username { get; set; }
		public string Name { get; set; }
		public Guid GameId { get; set; }
		public string OctgnUrl { get; set; }
    }
}