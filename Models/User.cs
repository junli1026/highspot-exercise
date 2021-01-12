using Newtonsoft.Json;

namespace HighspotExercise.Models 
{
    public class User 
    {
        [JsonProperty(PropertyName = "id")]
        public int Id { get; set; }

        [JsonProperty(PropertyName = "name")]
        public string Name { get; set; }
    }
}