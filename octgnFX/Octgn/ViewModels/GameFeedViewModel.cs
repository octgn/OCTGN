namespace Octgn.ViewModels
{
    using System;
    using System.Collections.Generic;

    using NuGet;

    public class GameFeedViewModel
    {
        public Version Version { get; set; }

        public IEnumerable<string> Authors { get; set; }

        public string Description { get; set; }

        public int DownloadCount { get; set; }

        public Uri IconUrl { get; set; }

        public string Id { get; set; }

        public IEnumerable<string> Owners { get; set; }

        public Uri ProjectUrl { get; set; }

        public DateTimeOffset? Published { get; set; }

        public string ReleaseNotes { get; set; }

        public string Summary { get; set; }

        public string Tags { get; set; }

        public string Title { get; set; }
    }
}