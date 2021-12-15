using System.Collections.Generic;
using System.Threading.Tasks;
using System.IO.Compression;
using AG.RocCore.Objects;
using System.Threading;
using Newtonsoft.Json;
using RedipalCore;
using System.Text;
using System.IO;
using System;
using RedipalCore.Objects;
using System.Linq;

namespace RediPalTester
{
    class Program
    {
        static async Task Main()
        {
            // Zero width char test
            //Console.WriteLine("Test");​



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

            var redi = new Redipal("roc-redis.ag:6379", new()
            {
                UnThrottleCPU = true,
                Default_MaxDegreeOfParallelism = -1
            });

            redi.SetTypeDefaults<TaskPlan>(x =>
            {
                x.KeySpace = "task";
            });

            var tasksSubs = redi.Subscribe.ToDictionary<string, TaskPlan>("activetasks");
            if (tasksSubs is not null)
            {
                var read = tasksSubs.Read();
                if (read is not null && read.Task is not null)
                {
                    tasksSubs.OnValueUpdate += (key, value) => Console.WriteLine(key + ": was updated");
                    tasksSubs.OnAdded += (key, value) => Console.WriteLine(key + ":             was Added");
                    tasksSubs.OnRemoved += (key) => Console.WriteLine(key + ":                                 was Removed");
                }
            }

            Task.Delay(5000000).Wait();














            Console.WriteLine();









            redi.SetTypeDefaults<CradlePosition>(pos =>
            {
                pos.DisableKeySpace = true;
                pos.DefaultID = "not-given";

                pos.AddConditional(a => a.User is User.TestCradlePos, x => x.DefaultID = "test-cradle-pos");
                pos.AddConditional(a => a.User is User.DevCradlePos, x => x.DefaultID = "dev-cradle-pos");
                pos.AddConditional(a => a.User is User.ProdCradlePos, x => x.DefaultID = "prod-cradle-pos");
            });

            _ = new CradlePosition().Redi_Write(); //  not-given
            _ = new CradlePosition() { User = User.TestCradlePos }.Redi_Write(); // test-cradle-pos
            _ = new CradlePosition() { User = User.DevCradlePos }.Redi_Write(); // dev-cradle-pos
            _ = new CradlePosition() { User = User.ProdCradlePos }.Redi_Write(); // prod-cradle-pos

            var notSet = redi.Read.Object<CradlePosition>();
            var test = redi.Read.Object<CradlePosition>("test-cradle-pos");
            var dev = redi.Read.Object<CradlePosition>("dev-cradle-pos");
            var prod = redi.Read.Object<CradlePosition>("prod-cradle-pos");



            CradlePosition? y = redi.Read.Object<CradlePosition>("test-cradle-pos2");

            _ = new CradlePosition().Redi_Write();
            CradlePosition? xx = redi.Read.Object<CradlePosition>();



            Console.WriteLine();



            //redi.Read.List<string>("discardedtasks")
            //    .ForEach(x => redi.Eradicate.Object<TaskPlan>(x));










            var zones = redi.Read.List<string>($"config:bridge:setting:south:allowedzones");

            while (true)
            {
                Thread.Sleep(5000);
            }



            Console.WriteLine();





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






    public class ServiceLog2 : RediBase
    {
        public string Issuer = string.Empty;

        public ServiceLog2()
        {
        }
        public ServiceLog2(string issuer)
        {
            Issuer = issuer;
        }
        public DateTime DateTime { get; set; }
        public long ID { get; set; }
        public LogType LogType { get; set; }
        public string Message { get; set; } = string.Empty;
        public int RetunCode { get; set; }
    }




    //[RediKeySpace(false),
    //    RediDefaultID("Item1")]
    public class CradlePosition : RediBase
    {
        public int X { get; set; } = 885;
        public int Y { get; set; } = 534;
        public int R { get; set; } = -1;
        public double? Z { get; set; } = null;

        internal User User = User.Unset;
    }

    public enum User
    {
        Unset,
        TestCradlePos,
        DevCradlePos,
        ProdCradlePos,
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







    public class LightCurtainConfig
    {
        public string ID { get; set; } = string.Empty;
        public string Armed { get; set; } = string.Empty;
        public string Reset { get; set; } = string.Empty;
        public string SoftBreak { get; set; } = string.Empty;
        public string UserDisarm { get; set; } = string.Empty;
        public string SensorState { get; set; } = string.Empty;
        public string ResetBreakOk { get; set; } = string.Empty;
        public string InputProcessData { get; set; } = string.Empty;
        public string OutputProcessData { get; set; } = string.Empty;
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