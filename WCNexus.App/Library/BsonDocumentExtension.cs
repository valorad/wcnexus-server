using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Driver;

namespace WCNexus.App.Library
{
    public static class BsonDocumentExtension
    {
        public static BsonDocument RenderToBsonDocument<T>(this FilterDefinition<T> filter)
        {
            var serializerRegistry = BsonSerializer.SerializerRegistry;
            var documentSerializer = serializerRegistry.GetSerializer<T>();
            return filter.Render(documentSerializer, serializerRegistry);
        }

        public static BsonDocument RenderToBsonDocument<T>(this UpdateDefinition<T> token)
        {
            var serializerRegistry = BsonSerializer.SerializerRegistry;
            var documentSerializer = serializerRegistry.GetSerializer<T>();
            return token.Render(documentSerializer, serializerRegistry).ToBsonDocument();
        }
    }
}
