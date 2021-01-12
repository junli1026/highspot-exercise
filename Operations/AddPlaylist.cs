using System.Collections.Generic;
using Newtonsoft.Json;

namespace HighspotExercise.Operations
{
    /*
     * Represents the operation of add a new playlist, which should contains at least one song.
     */
    public class AddPlaylist : IPayload
    {
        [JsonProperty(PropertyName = "user_id")]
        public int UserId { get; set; }

        [JsonProperty(PropertyName = "song_ids")]
        public List<int> SongIds  { get; set; }
    }
}