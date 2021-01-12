using System.Collections.Generic;
using Newtonsoft.Json;

namespace HighspotExercise.Operations
{
    /*
     * Represents the operation of entity (song or playlist)
     */
    public class Operation
    {
        public Operation(string type, IPayload payload)
        {
            Type = type;
            Payload = payload;
        }

        [JsonProperty(PropertyName = "type")]
        public string Type { get; set; }
        
        [JsonProperty(PropertyName = "payload")]
        public IPayload Payload { get; set; }
    }

}