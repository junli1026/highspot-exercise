## highspot-exercise

### How to run

The project is writtern with C# and .net5 SDK, the cross-plat is solved by the .net platform. To run the program you will need to install .net5. For more details visit this: https://docs.microsoft.com/en-us/dotnet/core/install/linux-ubuntu

My dev environemnt is WSL on windows and test env is Ubuntu 20.10, works well on both.

The executable accepct two input string as mixtape.json and changes.json , and an output string as the output file., like
 "./program mixtape.json changes.json output.json"

Pleaes let me know if you meet any problem running the program.

```
>> git clone https://github.com/junli1026/highspot-exercise
>> cd highspot-exercise

# install .net 5.0 SDK (runtime included) on Ubuntu 20.10
>> wget https://packages.microsoft.com/config/ubuntu/20.10/packages-microsoft-prod.deb -O packages-microsoft-prod.deb
>> sudo dpkg -i packages-microsoft-prod.deb
>> sudo apt-get update; \
   sudo apt-get install -y apt-transport-https && \
   sudo apt-get update && \
   sudo apt-get install -y dotnet-sdk-5.0

# build the project
>> dotnet restore
>> dotnet publish

# copy test file into binary folder 
>> cp ./TestData/mixtape-data.json ./bin/Debug/net5.0/publish/.
>> cp ./TestData/changes.json ./bin/Debug/net5.0/publish/.

# run the program and generate output.json
>> cd ./bin/Debug/net5.0/publish
>> ./highspot-exercise mixtape-data.json changes.json output.json
```

### A little design spec and explanation

The main class is in IngestionHandler.cs, whish has three methods mostly. The commnd-line tool just call the three methods;

```C#
    /*
     * Read data store file and parse the file into objects, store them into memory.
     * Similar with some nosql database, it reads the data store file from disk, and load them into memory.
     */
    public void LoadData (string mixTapeFilePath);

    /*
     * Read a change file (the example in in ./TestData/changes.json), and
     * apply the changes into the in-memory objects.
     */
    public void AcceptChanges (string changesFilePath);

    /*
     * Dump/serialize the in-memory objects into file stream and write to file.
     */
    public void Dump (string outputFile);
```

The changes is defined like this
```json
[
    {
        "type": "remove_playlist",
        "payload": {
            "id": 3
        }
    },
    {
        "type": "add_playlist",
        "payload": {
            "user_id": 1,
            "song_ids": [1, 3, 5, 7]
        }
    },
    {
        "type": "add_song",
        "payload": {
            "song_id": 1,
            "playlist_id": 2
        }
    }
]
```

### Scability

1. Ingestion challenge for a large file

For the problem of a very large single file, we need to be careful about the json parser, or any data parser. The case of loading everything into meory would exceed the maximum memory for a single machine.
We need a parser that can read partial JSON file and get some output, then store the object into data store like database, then keep parsing the remaining data.
Or try to implement a Splitter that can divide a large json file into chunks of small json files.

```
    { Super large json } ==> {chunk1.json} {chunk2.json} {chunk3.json}

    {
        users: [                       
            user1,
            usre2,
        ],              ===> {users: [user1]} {users:[user2]} {songs:[song1]}
        songs: [
            song1,
            song2
        ]

    }
```
JSON is a nested data structure, but for this specific case, we have Users array, PlayList array and Songs array, which is very "flat", so should be fine here.


2. Data capacity -- sharding 

Let's say we are now able to split a single large intoto many small files. The next problm we are facing is that the total size is too large to fit in any single machine. We need to distribe the data into multiple machines.

We can simply shard by hash(id) like the graph below.


```
                             |--- User Store 0---|
 hash(userId)%3 == 0  ===>   |--- id : int32  ---|
                             |--- name: string --|


                             |--- User Store 1---|
 hash(userId)%3 == 1  ===>   |--- id : int32  ---|
                             |--- name: string --|


                             |--- User Store 2---|
 hash(userId)%3 == 2  ===>   |--- id : int32  ---|
                             |--- name: string --|
```

Further issue is about how to runtime exapand the data capacity, like from 3 datebase to 30 database on the fly. But this is something we should avoid by estimating the total data capacity.


3. Data persistency

So far our system is all in memory, that is, the database is a list of in-mempry objects. This is good from the perspective of performace, any change will be applied very fast. But the distvantage is about data loss, thinking about if one of the database is down and then we lose all the changes.

So we need to solve the data persistency.

- 3.1 Disk based storage

Relationaly database is a choise. Our data model is very simple in this case, I believe most mature relational database should be enough.

Nosql is also a good choice, our case doesn't have join operation.


- 3.2 In-Mempory + Write-Ahead-Log + Sanpshot

Because of the speciality of this case, we can use in-memory and write-ahead-log and periodly snapshot to achieve a high performce datastore.

That is, for any "change" operation, we first append the opertaion to a WAL, then update the in-memory object. We periodly make a snapshot for current memory image. If the system is down, we can revover by Last-Snapshot + Write-Ahead-Log to revover to the right point.



4. High write traffic/throughput -- queue/message system

Let's say we solve all the data capacity issue by sharding and data persistency problem. We still need to think about high through/traffic issue.

Think about if our average daily "change" requests are about 100Gb/day, but it is not evenly distributed. That is, 90Gb of the equrests arrive between 1:00pm --- 1:30pm.

Although our system has enough capacity, the high traffic may cause service unavailable problem.

We might need a queue system to buffer the comming request, and digest them one by one.

```
                                                 | ---- data store 1
    ------------------------                     | 
-->                         --> [Application]  --| ---- data store 2
    ------------------------                     | ----- ......

```
