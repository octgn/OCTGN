namespace Octgn.DataNew
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Xml.Serialization;

    using Octgn.DataNew.Entities;
    using Octgn.DataNew.FileDB;

    public class GameSerializer : IFileDbSerializer
    {
        public object Deserialize(string fileName)
        {
            var serializer = new XmlSerializer(typeof(game));
            game g = null;
            using (var fs = File.Open(fileName,FileMode.Open,FileAccess.Read,FileShare.Read))
            {
                g = (game)serializer.Deserialize(fs);
                if (g == null)
                {
                    return null;
                }
            }
            var ret = new Game()
                          {
                              Id = new Guid(g.id),
                              Name = g.name,
                              CardBack = g.card.back,
                              CardHeight = int.Parse(g.card.height),
                              CardWidth = int.Parse(g.card.width),
                              Version = Version.Parse(g.version),
                              CustomProperties = new List<PropertyDef>(),
                              DeckSections = new List<string>(),
                              SharedDeckSections = new List<string>(),
                              FileHash = null,
                              Filename = fileName
                          };

            return ret;
        }

        public byte[] Serialize(object obj)
        {
            throw new System.NotImplementedException();
        }
    }
}