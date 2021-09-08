using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System.Collections.Generic;

namespace WCNexus.App.Models {
    public class Nexus: INexus
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string ID { get; set; }

        [BsonElement("dbname")]
        public string DBName { get; set; }

        [BsonElement("name")]
        public string Name { get; set; }

        [BsonElement("description")]
        public string Description { get; set; }

        [BsonElement("url")]
        public string URL { get; set; }

        [BsonElement("logo")]
        public string Logo { get; set; }

        [BsonElement("cover")]
        public string Cover { get; set; }

        [BsonElement("avatar")]
        public string Avatar { get; set; }

        [BsonElement("photos")]
        public IEnumerable<string> Photos { get; set; }

        [BsonElement("type")]
        public string Type { get; set; }

    }
}