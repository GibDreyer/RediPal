using RedipalCore;
using System.Diagnostics;
using RediPal.TestObjects;
using System.Drawing;
using System.Drawing.Imaging;
using StackExchange.Redis;
using Newtonsoft.Json;
using RediPal.Messages;
using RocKer;
using AG.ROC.Core;
using AG.RocCore.Objects;
using Pastel;
using System;
using System.Threading.Tasks;
using System.Linq;
using System.Collections.Generic;
using System.Threading;
using System.IO;
using System.IO.Compression;
using System.Text;
using RedipalCore.Attributes;

namespace RediPalTester
{
    class Program
    {
        static async Task Main()
        {

            //var newFile = new List<string>();


            //ProcessDirectory(@"C:\Temp\three\src");
            //// Process all files in the directory passed in, recurse on any directories
            //// that are found, and process the files they contain.
            //void ProcessDirectory(string targetDirectory)
            //{
            //    // Process the list of files found in the directory.
            //    string[] fileEntries = Directory.GetFiles(targetDirectory);
            //    foreach (string fileName in fileEntries)
            //        ProcessFile(fileName);

            //    // Recurse into subdirectories of this directory.
            //    string[] subdirectoryEntries = Directory.GetDirectories(targetDirectory);
            //    foreach (string subdirectory in subdirectoryEntries)
            //        ProcessDirectory(subdirectory);
            //}

            //// Insert logic for processing found files here.
            //void ProcessFile(string path)
            //{
            //    Console.WriteLine("Processing: " + path);

            //    var contents = File.ReadAllLines(path);
            //    var newLines = contents.Where(line => !line.Contains("import"));

            //    foreach (var line in contents)
            //    {
            //        if (line.Contains("import")) continue;
            //        if (line.Contains("export {")) continue;
            //        if (line.Contains("export *")) continue;

            //        var l = line.Replace("export", "declare");

            //        newFile.Add(l);
            //    }

            //    newFile.Add("");
            //    newFile.Add("");
            //    newFile.Add("");
            //}

            //File.WriteAllLines("c:/Temp/three/src/Three.d.ts", newFile);


            Console.WriteLine();






            //var redisConfig = new ConfigurationOptions
            //{
            //    Password = "Itunes96",
            //    User = ""
            //};
            //redisConfig.EndPoints.Add("redis-19940.c100.us-east-1-4.ec2.cloud.redislabs.com:19940");
            //var redi = new Redipal(redisConfig);

            var redi = new Redipal("rocii.ag:6379", new()
            {
                UnThrottleCPU = true,
                Default_MaxDegreeOfParallelism = -1
            });


            redi.SetTypeDefaults<CradlePosition>(x =>
            {
                x.DefaultID = "yep";
                x.KeySpace = "test";
            });

            CradlePosition x = new();

            new CradlePosition().Redi_Write();

            //redi.Write.Object(x);

            CradlePosition? y = redi.Read.Object<CradlePosition>();





            Console.WriteLine();



            //redi.Read.List<string>("discardedtasks")
            //    .ForEach(x => redi.Eradicate.Object<TaskPlan>(x));










            var zones = redi.Read.List<string>($"config:bridge:setting:south:allowedzones");

            while (true)
            {
                Thread.Sleep(5000);
            }



            Console.WriteLine();

















            //var sfdfdf = redi.Subscribe.ToDictionary<string, Location>();

            //var locations = redi.Read.Dictionary<string, Location>();
            //var cradles = redi.Read.Dictionary<string, Cradle>();

            //var backup = new LocationData();

            //backup.Set(locations, cradles);

            //var data = backup.Zip();


            //redi.SetTypeDefaults<Cradle>(x =>
            //            {
            //                x.DefaultSet = "cradles";
            //                x.KeySpace = "cradle";
            //            });

            //redi.SetTypeDefaults<Location>(x =>
            //{
            //    x.DefaultSet = "locations";
            //    x.KeySpace = "location";
            //});

            //var cradle = redi.Read.Dictionary<string, Cradle>();





            //_ = redi.SetTypeDefaults<ServiceLog>(log =>
            //{
            //    log.AddModifier((x, a) =>
            //    {
            //        a.AppendToSet($"roccore:{x.Issuer.ToLower()}:logs", true);

            //        if (x.LogType is LogType.Error)
            //            a.AppendToSet($"roccore:{x.Issuer.ToLower()}:active-errors");
            //    });

            //    log.AddConditional(x => x.LogType is LogType.Error, x => x.Expiration = TimeSpan.FromDays(3));
            //    log.AddConditional(x => x.LogType is LogType.Warning, x => x.Expiration = TimeSpan.FromDays(1));
            //    log.AddConditional(x => x.LogType is not LogType.Error and not LogType.Warning, x => x.Expiration = TimeSpan.FromDays(1));
            //    log.SetSearchScoreProperty(x => x.DateTime);
            //    log.KeySpace = "roccore:log";
            //});

            //redi.SetTypeDefaults<TaskPlan>(x =>
            //{
            //    x.KeySpace = "task";
            //});

            //redi.SetTypeDefaults<Cradle>(x =>
            //{
            //    x.DefaultSet = "cradles";
            //    x.KeySpace = "cradle";
            //});


            //redi.SetTypeDefaults<Location>(x =>
            //{
            //    x.DefaultSet = "locations";
            //    x.KeySpace = "location";
            //});






            //var serviceLogSub = redi.Subscribe.ToDictionary<string, ServiceLog>("roccore:service:logs", x =>
            //{
            //    x.WatchForRemove = false;
            //    x.SubscribeToSetMembers = false;
            //    x.SubscribeToSubObjects = false;
            //});

            //if (serviceLogSub != null)
            //{
            //    serviceLogSub.OnAdded += (s, l) => Console.Write(l?.Message);
            //}

            ////var northBridgeLogSub = redi.Subscribe.ToDictionary<string, ServiceLog>("roccore:north:logs", x =>
            ////{
            ////    x.WatchForRemove = false;
            ////    x.SubscribeToSetMembers = false;
            ////    x.SubscribeToSubObjects = false;
            ////});
            ////if (northBridgeLogSub != null)
            ////{
            ////    northBridgeLogSub.OnAdded += (s, l) => Console.Write("n");
            ////}

            ////var southBridgeLogSub = redi.Subscribe.ToDictionary<string, ServiceLog>("roccore:south:logs", x =>
            ////{
            ////    x.WatchForRemove = false;
            ////    x.SubscribeToSetMembers = false;
            ////    x.SubscribeToSubObjects = false;
            ////});

            ////if (southBridgeLogSub != null)
            ////{
            ////    southBridgeLogSub.OnAdded += (s, l) => Console.Write("s");
            ////}



            //while (true)
            //{
            //    await Task.Delay(500);
            //}








            //var locationSubscription = redi.Subscribe.ToDictionary<string, Location>();


            //for (int i = 0; i < 500; i++)
            //{
            //    var reader = locationSubscription.Read();

            //    reader.OnProggress += (key, value) => Console.WriteLine("Read: " + key + "  -  Percent Complete: " + reader.TotalRead / reader.TotalItems);

            //    var result = await reader.Task;

            //}









            //while (true)
            //{
            //    await Task.Delay(500);
            //}







            ////var temp = "<div><div>what is it? {0} </div><div>When do we want it?  {1} </div><div> Where? {2} </div><div> 123456789 0 {3}  </div></div>";
            ////var p = new[] { "A test", "Right now !", "here", "987654321" };
            ////var test = string.Format(temp, p);


            //var northPos = redi.Read.Property<Location, double>("northbridge", x => x.Position.X);
            //var southPos = redi.Read.Property<Location, Position>("southbridge", x => x.Position);


            //var bridges = redi.Subscribe.ToDictionary<string, Location, Position>("location", x => x.Position, "bridges");
            //bridges.OnValueUpdate += (key, value) =>
            //{
            //    Console.WriteLine(key + "   " + value.X);
            //};












            //while (true)
            //{
            //    await Task.Delay(500);
            //}











            //var readActive = Task.Run(() => redi.Read.Dictionary<string, TaskPlan>("activetasks"));
            //Console.WriteLine("Reading ActiveTasks");
            //var readrunning = Task.Run(() => redi.Read.Dictionary<string, TaskPlan>("runningtasks"));
            //Console.WriteLine("Reading RunningTasks");
            //var readcradles = Task.Run(() => redi.Read.Dictionary<string, Cradle>());
            //Console.WriteLine("Reading Cradles");
            //var readlocations = Task.Run(() => redi.Read.Dictionary<string, Location>());
            //Console.WriteLine("Reading Locations");

            //await Task.WhenAll(readActive, readrunning, readcradles, readlocations);
            //Console.WriteLine($"Active: {readActive.Result.Count}  Running: {readrunning.Result.Count}  Cradles: {readcradles.Result.Count}  Locations: {readlocations.Result.Count}");

            //Console.WriteLine();

            //var updateCount = 0;

            //void AddCount()
            //{
            //    Console.BackgroundColor = ConsoleColor.Red;
            //    Console.WriteLine("Update Count: " + updateCount++);
            //}

            //var locationSub = redi.Subscribe.ToDictionary<string, Location>();
            //if (locationSub is not null)
            //{
            //    locations = await locationSub.Read().Task ?? new();
            //    locationSub.OnValueUpdate += (key, value) =>
            //    {
            //        if (value is not null && locations.ContainsKey(key))
            //        {
            //            Console.BackgroundColor = ConsoleColor.Blue;
            //            Console.WriteLine("Location Updated: " + value.ID);
            //            locations[key] = value;
            //            AddCount();
            //        }
            //    };
            //    locationSub.OnAdded += (key, value) =>
            //    {
            //        if (value is not null && !locations.ContainsKey(key))
            //        {
            //            locations.Add(key, value);
            //            AddCount();
            //        }
            //    };
            //    locationSub.OnRemoved += (key) => locations.Remove(key);
            //}


            //Dictionary<string, TaskPlan>? activeTasks;

            //var activeTasksSub = redi.Subscribe.ToDictionary<string, TaskPlan>("activetasks");
            //if (activeTasksSub is not null)
            //{
            //    activeTasks = await activeTasksSub.Read().Task ?? new();
            //    activeTasksSub.OnValueUpdate += (key, value) =>
            //    {
            //        if (value is not null && activeTasks.ContainsKey(key))
            //        {
            //            Console.BackgroundColor = ConsoleColor.Green;
            //            Console.WriteLine("Active Task Updated: " + value.Motion_ID);
            //            activeTasks[key] = value;
            //            AddCount();
            //        }
            //    };
            //    activeTasksSub.OnAdded += (key, value) =>
            //    {
            //        Console.BackgroundColor = ConsoleColor.Green;
            //        Console.WriteLine("Active Task Added: " + value.Motion_ID);
            //        AddCount();
            //        if (value is not null && !activeTasks.ContainsKey(key))
            //        {
            //            activeTasks.Add(key, value);
            //        }
            //    };
            //    activeTasksSub.OnRemoved += (key) =>
            //    {
            //        Console.BackgroundColor = ConsoleColor.Green;
            //        Console.WriteLine("Active Task Removed: " + key);
            //        activeTasks.Remove(key);
            //        AddCount();
            //    };
            //}






            //Dictionary<string, TaskPlan>? runningTasks;

            //var runningTasksSub = redi.Subscribe.ToDictionary<string, TaskPlan>("runningtasks");
            //if (runningTasksSub is not null)
            //{
            //    runningTasks = await runningTasksSub.Read().Task ?? new();
            //    runningTasksSub.OnValueUpdate += (key, value) =>
            //    {
            //        if (value is not null && runningTasks.ContainsKey(key))
            //        {
            //            Console.BackgroundColor = ConsoleColor.Black;
            //            Console.WriteLine("Running Task Updated: " + value.Motion_ID);
            //            runningTasks[key] = value;
            //            AddCount();
            //        }
            //    };
            //    runningTasksSub.OnAdded += (key, value) =>
            //    {
            //        if (value is not null && !runningTasks.ContainsKey(key))
            //        {
            //            Console.BackgroundColor = ConsoleColor.Black;
            //            Console.WriteLine("Running Task Added: " + value.Motion_ID);
            //            runningTasks.Add(key, value);
            //            AddCount();
            //        }
            //    };
            //    runningTasksSub.OnRemoved += (key) =>
            //    {
            //        runningTasks.Remove(key);
            //        Console.BackgroundColor = ConsoleColor.Black;
            //        Console.WriteLine("Running Task Running: " + key);
            //        AddCount();
            //    };
            //}



            //while (true)
            //{
            //    await Task.Delay(500);
            //}



            ////var json = new HttpClient().GetStringAsync("http://10.0.192.54:4264/api/server/locations");
            ////var actualLocations = JsonConvert.DeserializeObject<List<Location>>(json.Result).ToDictionary(x=> x.ID, x=> x);
            ////redi.Write.Dictionary(actualLocations, "locations");

            ////var locationsddd = redi.Read.Dictionary<string, Location>();




            ////var activeTasks = redi.Subscribe.ToDictionary<string, TaskPlan>("activetasks");
            ////var dicardedTasks = redi.Subscribe.ToDictionary<string, TaskPlan>("dicardedtasks");
            ////var completedTasks = redi.Subscribe.ToDictionary<string, TaskPlan>("completedtasks");

            //// Console.WriteLine("Read: {0}    {1} Times", (await locationSubs.Read().Task).Count, count++);





            //while (true)
            //{
            //    Thread.Sleep(5000);
            //}

            //var subscription = redi.Subscribe.ToDictionary<string, OperatorConfig>("operators");
            //if (subscription != null)
            //{
            //    subscription.OnValueUpdate += (key, value) =>
            //    {
            //        Console.WriteLine(key + "   " + value.AutoStore);
            //    };
            //}
            //while (true)
            //{
            //    Thread.Sleep(5000);
            //}



            //var opConfig = redi.Read.Object<OperatorConfig>("saw-5");
            //opConfig.Redi_Write(x => x.AutoStore = true, x => x.AutoStore);

            ////var subscription = redi.Subscribe.ToObject<OperatorConfig>("saw-5");
            ////if (subscription != null)
            ////{
            ////    subscription.OnChange += (s) =>
            ////    {
            ////        Console.WriteLine(s.AutoStore);
            ////    };
            ////}




            //var subfgfgf = redi.Subscribe.ToMessages("operators");
            //var readgfg = subfgfgf.Read().Task.Result;
            //Console.WriteLine();


            //var tasks = redi.Subscribe.ToDictionary<string, TaskPlan>("activetasks");

            //tasks.OnValueUpdate += (key, value) =>
            //{
            //    Console.WriteLine("Task: " + key + "   Was updated");
            //};


            //while (true)
            //{
            //    Thread.Sleep(5000);
            //}

        }
    }



    public enum LocationZone
    {
        EastWest,
        NorthSouth,
        Paint
    }
    public enum LocationType
    {
        Input,
        InputOutput,
        Storage,
        OperatorBuffer,
        LongTermStorage,
        Disabled,
        Unknown,
        DisabledOperator
    }



    public enum LocationCradleState
    {
        Open,
        Inlocation,
        InUse
    }


    //[RediKeySpace("test"),
    //    RediDefaultID("Yoo")]
    public class CradlePosition : RediBase
    {
        public int X { get; set; } = 885;
        public int Y { get; set; } = 534;
        public int R { get; set; } = -1;
        public double? Z { get; set; } = null;
    }



    //[Flags]
    //public enum LocationAttributes
    //{
    //    NoPreference = 0,
    //    OperatorBuffer = 1,
    //    OutGoing = 2,
    //    Empties = 4,
    //    Scrap = 8,
    //    PendingScrap = 16
    //}



    public class LocationData
    {
        public List<LocationHistory> Locations { get; set; } = new();
        public List<CradleHistory> Cradles { get; set; } = new();

        public void Set(Dictionary<string, AG.RocCore.Objects.Location> l, Dictionary<string, AG.RocCore.Objects.Cradle> c)
        {
            foreach (var location in l.Values)
            {
                Locations.Add(new()
                {
                    CradleID = location.CradleID,
                    ID = location.ID
                });
            }

            foreach (var cradle in c.Values)
            {
                Cradles.Add(new()
                {
                    CradleClass = cradle.CradleClass,
                    ID = cradle.ID,
                    EstRetrievalDate = cradle.EstRetrievalDate,
                    EstRetrievalLocation = cradle.EstRetrievalLocation,
                    Jobs = cradle.Jobs,
                    LastUpdated = cradle.LastUpdated,
                    State = cradle.State,
                    Tags = cradle.Tags
                });
            }
        }

        public string Zip()
        {
            var json = JsonConvert.SerializeObject(this);
            byte[] encoded = Encoding.UTF8.GetBytes(json);
            byte[] compressed = Compress(encoded);
            return Convert.ToBase64String(compressed);
        }

        public static LocationData? Unzip(string json)
        {
            byte[] compressed = Convert.FromBase64String(json);
            byte[] decompressed = Decompress(compressed);

            return JsonConvert.DeserializeObject<LocationData>(Encoding.UTF8.GetString(decompressed));
        }


        public static byte[] Decompress(byte[] input)
        {
            using MemoryStream source = new(input);
            byte[] lengthBytes = new byte[4];
            source.Read(lengthBytes, 0, 4);

            int length = BitConverter.ToInt32(lengthBytes, 0);
            using GZipStream decompressionStream = new(source, CompressionMode.Decompress);
            byte[] result = new byte[length];
            decompressionStream.Read(result, 0, length);
            return result;
        }

        public static byte[] Compress(byte[] input)
        {
            using MemoryStream result = new();
            byte[] lengthBytes = BitConverter.GetBytes(input.Length);
            result.Write(lengthBytes, 0, 4);

            using (GZipStream compressionStream = new(result, CompressionMode.Compress))
            {
                compressionStream.Write(input, 0, input.Length);
                compressionStream.Flush();
            }
            return result.ToArray();
        }
    }

    public class LocationHistory
    {
        public string ID { get; set; }
        public string CradleID { get; set; }
    }

    public class CradleHistory
    {
        public string ID { get; set; }
        public string EstRetrievalLocation { get; set; } = "";
        public DateTime EstRetrievalDate { get; set; }
        public AG.RocCore.Objects.CradleClass CradleClass { get; set; }
        public AG.RocCore.Objects.CradleState State { get; set; }
        public DateTime LastUpdated { get; set; }

        public List<string>? Tags { get; set; }
        public List<string>? Jobs { get; set; }
    }
}