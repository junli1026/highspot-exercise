using System;
using System.IO;
using System.Collections.Generic;
using Newtonsoft.Json;
using HighspotExercise.Models;
using HighspotExercise.Operations;
using Newtonsoft.Json.Linq;
using System.Linq;

namespace HighspotExercise
{
    /*
     * Represents the handler for ingestion data, including mixtape data and changes
     */
    public class IngestionHandler 
    {
        public Dictionary<int, Song> Songs { get; private set; }

        public Dictionary<int, User> Users { get; private set; }

        public Dictionary<int, Playlist> Playlists { get; private set;}
        public SortedSet<int> AvailablePlaylistIds {get; private set; }
        public IngestionHandler(int maxPlaylistId) 
        {
            this.MaxPlaylistId = maxPlaylistId;
               
        }
        public int MaxPlaylistId { get; private set;}

        public IngestionHandler()
        {
            Songs = new Dictionary<int, Song>();
            Users = new Dictionary<int, User>();
            Playlists = new Dictionary<int, Playlist>();
            AvailablePlaylistIds = new SortedSet<int>();
        }

        // Read mixtape file and parse the file into objects, store them into dictionary.
        public void LoadData (string filePath) 
        {
            Console.WriteLine("Loading " + filePath.ToString());
            using (StreamReader file = File.OpenText(filePath))
            {
                JsonSerializer serializer = new JsonSerializer();
                MixTape tape = (MixTape)serializer.Deserialize(file, typeof(MixTape));

                foreach(var user in tape.Users)
                {
                    Users.Add(user.Id, user);
                }

                foreach(var song in tape.Songs)
                {
                    Songs.Add(song.Id, song);
                }

                int minPlaylistId = Int32.MaxValue;
                foreach(var playlist in tape.Playlists)
                {
                    Playlists.Add(playlist.Id, playlist);
                    minPlaylistId = Math.Min(minPlaylistId, playlist.Id);
                    MaxPlaylistId = Math.Max(MaxPlaylistId, playlist.Id);
                }

                /*
                 * Find the missing ids and put them into available list.
                 * For example, if the ids are 1, 2, 3, 6, 7, 9.  The missing ids [4, 5, 8] will 
                 * be added to availablePlaylistId, for future use -- mainly for creation.
                 */
                int prev = minPlaylistId;
                for (int i = minPlaylistId+1; i <= MaxPlaylistId; i++)
                {
                    if (i != prev + 1)
                    {
                        for (int available = prev+1; available < i; available++)
                        {
                            AvailablePlaylistIds.Add(available);
                        }
                    }
                    prev = i;
                }
            }
            Console.WriteLine(string.Format("{0} users loaded.", Users.Count));
            Console.WriteLine(string.Format("{0} playlists loaded.", Playlists.Count));
            Console.WriteLine(string.Format("{0} songs loaded.", Songs.Count));
        }

        public void ApplyChanges(string changeFile) 
        {
            using (StreamReader file = File.OpenText(changeFile))
            {
                JsonSerializer serializer = new JsonSerializer();
                serializer.Converters.Add(new OperationConverter());
                var operations = (List<Operation>)serializer.Deserialize(file, typeof(List<Operation>));
                foreach (var op in operations) 
                {
                    try 
                    {
                        if (op.Payload is AddPlaylist) {
                            addPlaylist(op.Payload as AddPlaylist);
                        } else if (op.Payload is RemovePlaylist) {
                            removePlaylist(op.Payload as RemovePlaylist);
                        } else if (op.Payload is LinkSongToPlaylist) {
                            addSong(op.Payload as LinkSongToPlaylist);
                        } else {
                            throw new ApplicationException(string.Format("unsupported payload type {0}", op.Payload.GetType()));
                        }
                    }
                    catch(Exception ex) 
                    {
                        Console.WriteLine(ex);
                    }
                }
            }
        }

        /*
         * Create a new playlist, search in the AvailablePlaylistId for available id,
         * if it is empty, use the MaxPlaylistId
         */ 
        private void addPlaylist(Operations.AddPlaylist payload)
        {
            Console.WriteLine("add-playlist");
            if (payload.SongIds.Count == 0)
                throw new ApplicationException("playlist should contain at least one song!");

            var playlist = new Playlist();
            playlist.SongIds = payload.SongIds;
            playlist.UserId = payload.UserId;

            // no available playlist id, so increace the MaxPlaylistId
            if (AvailablePlaylistIds.Count == 0) {
                MaxPlaylistId ++;
                playlist.Id = MaxPlaylistId;
                Playlists.Add(playlist.Id, playlist);
            } else {
                // get the min value from AvailablePlaylistIds
                int id = AvailablePlaylistIds.Min;
                playlist.Id = id;
                Playlists.Add(playlist.Id, playlist);
                AvailablePlaylistIds.Remove(id);
            }
            Console.WriteLine(string.Format("playlist {0} added", playlist.Id));
        }

        /*
         * Remove a playlist from Playlists dictionary. Update  the AvailablePlaylistIds or MaxPlaylistId accordingly.
         */
        private void removePlaylist(Operations.RemovePlaylist payload)
        {
            Console.WriteLine("remove-playlist");
            int id = payload.Id;
            if (!Playlists.ContainsKey(id)) {
                // playlist doesn't exist, write log and return
                Console.WriteLine(string.Format("playlist {0} not found", id));
                return;
            }

            Playlists.Remove(id);
            if (id == MaxPlaylistId) {
                MaxPlaylistId --;
            } else {
                AvailablePlaylistIds.Add(id);
            }
            Console.WriteLine(string.Format("playlist {0} removed", id));
        }

        private void addSong(Operations.LinkSongToPlaylist payload)
        {
            Console.WriteLine("add-song");
            int playlistId = payload.PlayListId;
            int songId = payload.SongId;
 
            if (!Playlists.ContainsKey(playlistId)) {
                Console.WriteLine(string.Format("playlist id {0} not found", playlistId));
                return;
            }

            if (!Songs.ContainsKey(songId)) {
                Console.WriteLine(string.Format("song id {0} not found", songId));
                return;
            }
             
            var playlist = Playlists[playlistId];
            if (playlist.SongIds.Contains(songId)) {
                Console.WriteLine(string.Format("playlist {0} already contaons song {1}", playlistId, songId));
                return;
            }

            playlist.SongIds.Add(songId);
            Console.WriteLine(string.Format("song {0} added.", songId));
        }

        public void Dump (string filePath) 
        {
            var tape = new MixTape();
            tape.Users = Users.Values.ToList();
            tape.Songs = Songs.Values.ToList();
            tape.Playlists = Playlists.Values.ToList();

            using (StreamWriter file = File.CreateText(filePath))
            {
                JsonSerializer serializer = new JsonSerializer();
                serializer.Serialize(file, tape);
            }
        }

    }
}