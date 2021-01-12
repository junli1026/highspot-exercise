using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace HighspotExercise.Operations
{
    public class OperationConverter : JsonConverter<Operation>
    {
        private Dictionary<string, Func<string, JObject, Operation>> operationBuilder;

        public OperationConverter()
        {
            operationBuilder = new Dictionary<string, Func<string, JObject, Operation>> {
                {
                    "remove_playlist",
                    (string t, JObject json) => new Operation(t, json["payload"].ToObject<RemovePlaylist>())
                },
                {
                    "add_playlist",
                    (string t, JObject json) => new Operation(t, json["payload"].ToObject<AddPlaylist>())
                },
                {
                    "add_song",
                    (string t, JObject json) => new Operation(t, json["payload"].ToObject<LinkSongToPlaylist>())
                }
            };
        }

        public override Operation ReadJson(JsonReader reader, Type objectType, Operation existingValue, bool hasExistingValue, JsonSerializer serializer)
        {
            JObject jsonObject = JObject.Load(reader);
            string type = (string)jsonObject["type"];
            if (operationBuilder.ContainsKey(type))
            {
                return operationBuilder[type](type, jsonObject);
            }
            throw new ApplicationException(string.Format("unsupported operation type '{0}'"));
        }

        public override void WriteJson(JsonWriter writer, [AllowNull] Operation value, JsonSerializer serializer)
        {
            writer.WriteValue(value.ToString());
        }
    }
}