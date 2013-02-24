namespace Octgn.Online.Data
{
    using System.Linq;

    using MongoDB.Bson;
    using MongoDB.Bson.Serialization;
    using MongoDB.Bson.Serialization.IdGenerators;
    using MongoDB.Driver;
    using MongoDB.Driver.Linq;

    using Octgn.Online.Library.Models;

    public class DbContext
    {
        internal static bool MappingsSet;
        public static DbContext Get()
        {
            SetupMappings();
            return new DbContext();
        }

        internal static object MapLock = new object();

        internal static void SetupMappings()
        {
            lock (MapLock)
            {
                if (MappingsSet) return;

                BsonClassMap.RegisterClassMap<HostedGameState>(
                    cm =>
                        {
                            //cm.MapIdProperty(x => x.Id); //.SetRepresentation(BsonType.String);
                            cm.AutoMap();
                            cm.SetIdMember(cm.GetMemberMap(x => x.Id));
                        });

                MappingsSet = true;
            }
        }

        internal MongoServer Server { get; set; }

        internal DbContext()
        {
            var client = new MongoDB.Driver.MongoClient(new MongoUrl("mongodb://data.octgn.net:27017"));
            Server = client.GetServer();
        }

        public IQueryable<HostedGameState> Games
        {
            get
            {
                return Server.GetDatabase("Games").GetCollection<HostedGameState>("HostedGameStates").AsQueryable();
            }
        }

        public MongoCollection<HostedGameState> GameCollection
        {
            get
            {
                return Server.GetDatabase("Games").GetCollection<HostedGameState>("HostedGameStates");
            }
        } 
    }
}