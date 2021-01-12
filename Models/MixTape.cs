using Newtonsoft.Json;
using System.Collections.Generic;

namespace HighspotExercise.Models 
{
    public class MixTape
    {
        [JsonProperty(PropertyName = "users")]
        public List<User> Users { get; set; }

        [JsonProperty(PropertyName = "playlists")]
        public List<Playlist> Playlists { get; set; }

        [JsonProperty(PropertyName = "songs")]
        public List<Song> Songs  { get; set; }
    }
}