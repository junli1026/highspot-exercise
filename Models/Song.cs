using Newtonsoft.Json;

namespace HighspotExercise.Models 
{
    public class Song 
    {
        [JsonProperty(PropertyName = "id")]
        public int Id { get; set; }

        [JsonProperty(PropertyName = "artist")]
        public string Artist { get; set; }

        [JsonProperty(PropertyName = "title")]
        public string Title  { get; set; }
    }
}