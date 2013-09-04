using System.Drawing;
using System.IO;
using System.Linq;
using log4net;
using Octgn.Core.DataExtensionMethods;
using Octgn.DataNew.Entities;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace Octgn.DeckBuilder
{
    public class MetaDeck : IDeck
    {
        internal static ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        public string Path { get; set; }
        public string Name { get; set; }
        public Guid GameId { get; private set; }
        public bool IsShared { get; set; }
        public string Notes { get; set; }
        public string CardBack { get; set; }
        public IEnumerable<ISection> Sections { get; private set; }
        public bool IsGameInstalled { get; set; }
        public bool IsCorrupt { get; set; }

        public MetaDeck()
        {
            IDeck d = null;
            this.Path = "c:\\test.o8g";
            this.Name = new FileInfo(Path).Name.Replace(new FileInfo(Path).Extension, "");
            CardBack = "../Resources/Back.jpg";
        }

        public MetaDeck(string path)
        {
            IDeck d = null;
            this.Path = path;
            this.Name = new FileInfo(Path).Name.Replace(new FileInfo(Path).Extension,"");
            CardBack = "pack://application:,,,/Resources/Back.jpg";
            try
            {
                d = this.Load(path,false);
            }
            catch (Exception e)
            {
                Log.Warn("New MetaDeck Error",e);
                IsCorrupt = true;
            }
            if (d == null) return;
            this.GameId = d.GameId;
            this.IsShared = d.IsShared;
            this.Notes = d.Notes;
            this.Sections = d.Sections;
            this.CardBack = DataNew.DbContext.Get().Games.First(x => x.Id == this.GameId).CardBack;
        }
    }
}
