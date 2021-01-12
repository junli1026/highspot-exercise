using Newtonsoft.Json;
using System.Collections.Generic;

namespace HighspotExercise.Models 
{
    public class Playlist
    {
        [JsonProperty(PropertyName = "id")]
        public int Id { get; set; }

        [JsonProperty(PropertyName = "user_id")]
        public int UserId { get; set; }

        [JsonProperty(PropertyName = "song_ids")]
        public List<int> SongIds  { get; set; }
    }
}