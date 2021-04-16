using System.Collections.Generic;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace WCNexus.App.Models {
    public class JointProject : IJointProject
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
        
        [BsonElement("type")]
        public string Type { get; set; }
        
        [BsonElement("techs")]
        public IEnumerable<string> Techs { get; set; }

        [BsonElement("images")]
        public IEnumerable<string> Images { get; set; }
    }
}