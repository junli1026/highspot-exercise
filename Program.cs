using System;
using System.IO;
using Newtonsoft.Json;
using HighspotExercise.Models;
using Newtonsoft.Json.Linq;

namespace HighspotExercise
{
    class Program
    {

        static void Main(string[] args)
        {
            /*
            Test_LoadData();
            Test_AddRemovePlaylist();
            Test_AddSong();
            Test_Dump();
            */
            
            if (args.Length != 3) {
                Console.WriteLine("Usage: program mixtape-data.json change.json output.json");
                return;
            }

            string dataFile = args[0];
            string changeFile = args[1];
            string outputFile = args[2];

            try {
                var handler = new IngestionHandler();
                handler.LoadData(dataFile);
                handler.ApplyChanges(changeFile);
                handler.Dump(outputFile);
            }
            catch(Exception ex) {
                Console.WriteLine(ex);
            }
        }

        static void Assert(bool c)
        {
            if(!c)
                throw new Exception();
        }

        static void verify(IngestionHandler handler)
        {
            // verify users
            Assert(handler.Users.Count == 7);
            for (int i = 1; i <= 7; i++) 
            {
                Assert(handler.Users.ContainsKey(i));
            }
            Assert(handler.Users[1].Name == "Albin Jaye");
            Assert(handler.Users[2].Name == "Dipika Crescentia");

            // verify playlists
            Assert(handler.Playlists.Count == 3);
            for (int i = 1; i <= 3; i++) 
            {
                Assert(handler.Playlists.ContainsKey(i));
            }
            Assert(handler.Playlists[1].UserId == 2);
            Assert(handler.Playlists[1].SongIds.Count == 2);
            Assert(handler.Playlists[1].SongIds[0] == 8);
            Assert(handler.Playlists[1].SongIds[1] == 32);
            Assert(handler.AvailablePlaylistIds.Count == 0);
            Assert(handler.MaxPlaylistId == 3);

            // verify songs
            Assert(handler.Songs.Count == 40);
            for (int i = 1; i <= 40; i++) 
            {
                Assert(handler.Songs.ContainsKey(i));
            }
            Assert(handler.Songs[40].Artist == "Imagine Dragons");
            Assert(handler.Songs[40].Title == "Thunder");
        }

        // TODO: replace it with xUnit test framework
        static void Test_LoadData()
        {
            var handler = new IngestionHandler();
            handler.LoadData(@"./TestData/mixtape-data.json");
            verify(handler);
        }

        // TODO: replace it with xUnit test framework
        static void Test_AddRemovePlaylist() 
        {
            var handler = new IngestionHandler();
            handler.LoadData(@"./TestData/mixtape-data.json");
            handler.ApplyChanges(@"./TestData/add_remove_playlist.json");
            // verify playlists
            Assert(handler.Playlists.Count == 6);
            for (int i = 1; i <= 6; i++) 
            {
                Assert(handler.Playlists.ContainsKey(i));
            }

            Assert(handler.Playlists[6].UserId == 4);
            Assert(handler.Playlists[6].SongIds.Count == 4);
            Assert(handler.Playlists[6].SongIds.Contains(1));
            Assert(handler.Playlists[6].SongIds.Contains(3));
            Assert(handler.Playlists[6].SongIds.Contains(5));
            Assert(handler.Playlists[6].SongIds.Contains(7));
            Assert(handler.Playlists[5].UserId == 2);
        }

        // TODO: replace it with xUnit test framework
        static void Test_AddSong()
        {
            var handler = new IngestionHandler();
            handler.LoadData(@"./TestData/mixtape-data.json");
            handler.ApplyChanges(@"./TestData/link_song.json");
            verify(handler);          

            Assert(handler.Playlists[2].UserId == 3);
            Assert(handler.Playlists[2].SongIds.Count == 7);
            Assert(handler.Playlists[2].SongIds.Contains(6));
            Assert(handler.Playlists[2].SongIds.Contains(8));
            Assert(handler.Playlists[2].SongIds.Contains(11));
            Assert(handler.Playlists[2].SongIds.Contains(37));
            Assert(handler.Playlists[2].SongIds.Contains(38));
            Assert(handler.Playlists[2].SongIds.Contains(39));
            Assert(handler.Playlists[2].SongIds.Contains(40));
        }

       // TODO: replace it with xUnit test framework
        static void Test_Dump()
        {
            var handler1 = new IngestionHandler();
            handler1.LoadData(@"./TestData/mixtape-data.json");
            verify(handler1);
            handler1.Dump(@"./TestData/tmp.json");

            var handler2 = new IngestionHandler();
            handler2.LoadData(@"./TestData/tmp.json");

            Assert(handler1.Users.Count == handler2.Users.Count);
            foreach (int id in handler1.Users.Keys) {
                Assert(handler2.Users.ContainsKey(id));
                Assert(handler1.Users[id].Name == handler2.Users[id].Name);
            }

            Assert(handler1.Songs.Count == handler2.Songs.Count);
            foreach (int id in handler1.Songs.Keys) {
                Assert(handler2.Songs.ContainsKey(id));
                Assert(handler1.Songs[id].Title == handler2.Songs[id].Title);
                Assert(handler1.Songs[id].Artist == handler2.Songs[id].Artist);
            }

            Assert(handler1.Playlists.Count == handler2.Playlists.Count);
            foreach (int id in handler1.Playlists.Keys) {
                Assert(handler2.Playlists.ContainsKey(id));
                Assert(handler1.Playlists[id].UserId == handler2.Playlists[id].UserId);
            }
        }
        
    }
}
