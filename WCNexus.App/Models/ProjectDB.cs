using System.Collections.Generic;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace WCNexus.App.Models {
    public class ProjectDB: IProjectDB
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string ID { get; set; }

        [BsonElement("dbname")]
        public string DBName { get; set; }

        [BsonElement("techs")]
        public IEnumerable<string> Techs { get; set; }

        [BsonElement("images")]
        public IEnumerable<string> Images { get; set; }
    }
}