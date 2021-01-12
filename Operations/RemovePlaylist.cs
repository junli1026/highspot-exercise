using System.Collections.Generic;
using Newtonsoft.Json;

namespace HighspotExercise.Operations
{
    /*
     * Represents the operation of remmove a new playlist.
     */
    public class RemovePlaylist : IPayload
    {
        [JsonProperty(PropertyName = "id")]
        public int Id { get; set; }
    }
}