using Newtonsoft.Json;

namespace HighspotExercise.Operations
{
    /*
     * Represents the operation of add an existing song to an existing playlist.
     */
    public class LinkSongToPlaylist : IPayload
    {
        [JsonProperty(PropertyName = "song_id")]
        public int SongId { get; set; }

        [JsonProperty(PropertyName = "playlist_id")]
        public int PlayListId { get; set; }
    }
}