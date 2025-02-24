using System;
using System.Text.Json;
using System.Text.RegularExpressions;

using MongoDB.Bson;
using MongoDB.Bson.Serialization;

namespace Arad.Portal.GeneralLibrary.CustomAttributes;

public class CustomSerializer : IBsonSerializer
{
    public Type ValueType => typeof(object);

    public object Deserialize(BsonDeserializationContext context, BsonDeserializationArgs args)
    {
        if (context.Reader.GetCurrentBsonType() != BsonType.Document)
        {
            throw new BsonSerializationException("Expected a document.");
        }

        BsonDocument bsonDocument = BsonSerializer.Deserialize<BsonDocument>(context.Reader);
        string json = bsonDocument.ToJson();
        string cleanJson = Regex.Replace(json, @"ObjectId\((.[a-f0-9]{24}.)\)", m => m.Groups[1].Value);

        return JsonSerializer.Deserialize<object>(cleanJson);
    }

    public void Serialize(BsonSerializationContext context, BsonSerializationArgs args, object value)
    {
        string json = value == null ? "{}" : JsonSerializer.Serialize(value);
        BsonDocument document = BsonDocument.Parse(json);
        BsonSerializer.Serialize(context.Writer, document);
    }
}