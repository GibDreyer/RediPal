using RedipalCore;
using System.Diagnostics;
using RediPal.TestObjects;
using System.Drawing;
using System.Drawing.Imaging;
using StackExchange.Redis;
using Newtonsoft.Json;
using File = System.IO.File;
using RediPal.Messages;
using RocKer;
using AG.ROC.Core;
using AG.RocCore.Objects;

namespace RediPalTester
{
    class Program
    {
        static async Task Main()
        {
            //var redisConfig = new ConfigurationOptions
            //{
            //    Password = "Itunes96",
            //    User = ""
            //};
            //redisConfig.EndPoints.Add("redis-19940.c100.us-east-1-4.ec2.cloud.redislabs.com:19940");
            //var redi = new Redipal(redisConfig);
            var redi = new Redipal("roc-redis.ag:6379");

            //var temp = "<div><div>what is it? {0} </div><div>When do we want it?  {1} </div><div> Where? {2} </div><div> 123456789 0 {3}  </div></div>";
            //var p = new[] { "A test", "Right now !", "here", "987654321" };
            //var test = string.Format(temp, p);
            redi.SetTypeDefaults<Location>(x =>
            {
                x.DefaultSet = "locations";
                x.KeySpace = "location";
            });



            redi.SetTypeDefaults<TaskPlan>(x =>
            {
                x.KeySpace = "task";
            });

            var runningTasks = redi.Subscribe.ToDictionary<string, TaskPlan>("activetasks");
            runningTasks.OnValueUpdate += (key, value) =>
            {
                if (!string.IsNullOrEmpty(value.RunningOn))
                {
                    Console.BackgroundColor = ConsoleColor.Green;
                    Console.WriteLine("active Task Updated: " + value.MotionPlans?.OrderBy(x => x.Key)
                        .FirstOrDefault(x => !x.Value.PlanComplete).Value.Steps?
                        .LastOrDefault(x => !x.StepComplete).Description);
                }
            };

            while (true)
            {
                await Task.Delay(500);
            }



            //var json = new HttpClient().GetStringAsync("http://10.0.192.54:4264/api/server/locations");
            //var actualLocations = JsonConvert.DeserializeObject<List<Location>>(json.Result).ToDictionary(x=> x.ID, x=> x);
            //redi.Write.Dictionary(actualLocations, "locations");

            //var locationsddd = redi.Read.Dictionary<string, Location>();



            while (true)
            {
                redi.Read.Object<Location>("northbridge").Redi_Write(x => x.HMapped = !x.HMapped, x => x.HMapped);
                await Task.Delay(500);
            }



            while (true)
            {
                await Task.Delay(500);
            }



            //var activeTasks = redi.Subscribe.ToDictionary<string, TaskPlan>("activetasks");
            //var dicardedTasks = redi.Subscribe.ToDictionary<string, TaskPlan>("dicardedtasks");
            //var completedTasks = redi.Subscribe.ToDictionary<string, TaskPlan>("completedtasks");







            // Console.WriteLine("Read: {0}    {1} Times", (await locationSubs.Read().Task).Count, count++);


            while (true)
            {
                Thread.Sleep(5000);
            }

            var subscription = redi.Subscribe.ToDictionary<string, OperatorConfig>("operators");
            if (subscription != null)
            {
                subscription.OnValueUpdate += (key, value) =>
                {
                    Console.WriteLine(key + "   " + value.AutoStore);
                };
            }
            while (true)
            {
                Thread.Sleep(5000);
            }



            var opConfig = redi.Read.Object<OperatorConfig>("saw-5");
            opConfig.Redi_Write(x => x.AutoStore = true, x => x.AutoStore);

            //var subscription = redi.Subscribe.ToObject<OperatorConfig>("saw-5");
            //if (subscription != null)
            //{
            //    subscription.OnChange += (s) =>
            //    {
            //        Console.WriteLine(s.AutoStore);
            //    };
            //}




            var subfgfgf = redi.Subscribe.ToMessages("operators");
            var readgfg = subfgfgf.Read().Task.Result;
            Console.WriteLine();


            var tasks = redi.Subscribe.ToDictionary<string, TaskPlan>("activetasks");

            tasks.OnValueUpdate += (key, value) =>
            {
                Console.WriteLine("Task: " + key + "   Was updated");
            };


            while (true)
            {
                Thread.Sleep(5000);
            }


            var watch = Stopwatch.StartNew();
            //   var tasks = redi.Search.AsDictionary<string, TaskPlan>(DateTime.Now.AddDays(-100), DateTime.Now);
            watch.Stop();
            Console.WriteLine(watch.Elapsed.TotalMilliseconds);
            Console.WriteLine();



            var sub = redi.Subscribe.ToDictionary<string, string>("status-message:templates");

            if (sub.Subscriptions.TryGetValue("test-message", out var template))
            {
                template.Dispose();
            }


            var result = await sub.Read().Task;
            sub.OnAdded += (key, value) =>
            {
                Console.WriteLine(key + " Was Added");
                Console.WriteLine(value);
            };
            sub.OnValueUpdate += (key, value) =>
            {
                Console.WriteLine(key + " Was Updated");
                Console.WriteLine(value);
            };
            sub.OnRemoved += (key) =>
            {
                Console.WriteLine(key + " Was Removed");
            };

            var messageSub = redi.Subscribe.ToMessages("operators");
            messageSub.OnValueUpdate += (key, value) =>
            {
                Console.WriteLine(key + "   |   " + value);
            };



            Console.WriteLine();

            var disarmSub = redi.Subscribe.ToDictionary<string, DisarmState>();



            //// This
            //redi.SetTypeDefaults<RediMessage>(x => x.Expiration = null);

            //// or this way
            //var message = new RediMessage("test-message", "A test", "Right now !", "here", "987654321");
            //redi.Write.Object(message, "saw-5", x => x.Expiration = TimeSpan.FromMinutes(120));


            //redi.Write.Value("<div><div>Cradle: {0} </div><div>Operator: {1} </div></div>", "status-message:template:task-store");

            //var messageSub = redi.Subscribe.ToMessages("operators");
            //messageSub.OnValueUpdate += (key, value) =>
            //{
            //    Console.WriteLine(key + "   |   " + value);
            //};
            //// var test = await messageSub.Read().Task;


            //redi.Write.Value("<div><div>what is it? {0} </div><div>When do we want it?  {1} </div><div> Where? {2} </div><div> 123456789 0 {3}  </div></div>", "status-message:template:test-message");
            //redi.Write.Value("<div><div>{0} | {1} | {2} | {3}</div></div>", "status-message:template:test-message-2");


            //new Task_Store_Message() { CradleID = "1120", OperatorID = "saw-5" }.Redi_Write("saw-5");
            //// var messages = redi.Read.Messages("operators");

            //new RediMessage("test-message", "A test", "Right now !", "here", "987654321").Redi_Write("saw-11");

            //// Passing in the constructor
            //new RediMessage("test-message", "A test", "Right now !", "here", "987654321").Redi_Write("saw-9");
            //// passing as a property
            //new RediMessage("test-message")
            //{
            //    Parameters = new[] { "A test", "Right now !", "here", "987654321" }
            //}.Redi_Write("saw-10");


            //await Task.Delay(150);

            //var watch = Stopwatch.StartNew();
            //var messagess = redi.Read.Messages("operators");
            //watch.Stop();
            //Console.WriteLine(watch.Elapsed.TotalMilliseconds);
            //new Task_Store_Message() { CradleID = "1120", OperatorID = "saw-1" }.Redi_Write("saw-1");


            //// Read all operator messages



            //var bmp = new Bitmap(@"C:\Users\gdreyer.PEERDOM\Desktop\BandR-AutomationCON-2019-9512 copy.jpeg");
            // var bmp = new Bitmap(@"C:\Users\gdreyer.PEERDOM\Documents\20180823_110843.jpg");
            //var bmp = new Bitmap(@"C:\Users\gdreyer.PEERDOM\Documents\20180823_110843.jpg");
            //var bmp = new Bitmap(@"C:\Users\gdreyer.PEERDOM\Desktop\pixel-yellow-duck-says-quack-8-bit-vector-23762574.jpg");
            //var bmp = new Bitmap(@"C:\Users\gdreyer.PEERDOM\Desktop\Legal_section_1_LightCurtain__637637453993433561.bmp");

            var stopwatchg = Stopwatch.StartNew();
            //new StatusMessage()
            //{
            //    Status = "This is a test 0",
            //    Type = StatusMessageType.Error
            //}.Redi_Write("saw-5");

            //var test = redi.Read.Object<StatusMessage>("saw-5");


            var ssuub = redi.Subscribe.ToDictionary<string, StatusMessage>("config:wallsections");

            ssuub.OnValueUpdate += (key, value) =>
                {
                    if (value is not null)
                    {
                        Console.WriteLine(key);
                    }
                    else
                    {
                        Console.WriteLine(key + ":  Was Deleted");
                    }
                };

            redi.Eradicate.Key("statusmessage:section_10");


            while (true)
            {
                Thread.Sleep(5000);
            }

            stopwatchg.Stop();
            Console.WriteLine("Write Time:  " + stopwatchg.ElapsedMilliseconds);
            var bmp1 = new Bitmap(@"C:\Users\gdreyer.PEERDOM\Documents\20180823_110843.jpg");

            //using (MemoryStream mss = new())
            //{
            //    EncoderParameter qualityParam = new(System.Drawing.Imaging.Encoder.Quality, 70l);
            //    ImageCodecInfo imageCodec = ImageCodecInfo.GetImageEncoders().FirstOrDefault(o => o.FormatID == ImageFormat.Jpeg.Guid);
            //    EncoderParameters parameters = new(1);
            //    parameters.Param[0] = qualityParam;
            //    bmp.Save(mss, imageCodec, parameters);
            //    Bitmap bpm = new(mss);

            //    using Stream s = new MemoryStream();
            //    BinaryFormatter formatter = new();
            //    formatter.Serialize(s, bmp);
            //    Console.WriteLine("BitMap: " + s.Length / 1000000);
            //}

            var dsdssdsd = redi.Subscribe.ToObject<OperatorConfig>("saw-5");

            if (dsdssdsd != null)
            {
                dsdssdsd.OnChange += (s) =>
                {
                    Console.WriteLine(s.AutoStore);
                };
            }


            redi.SetTypeDefaults<StatusMessage>(StatusMessage =>
            {
                StatusMessage.Expiration = TimeSpan.FromSeconds(45);
                StatusMessage.AddConditional(x => x.Status.Contains("10"), x => x.Expiration = TimeSpan.FromSeconds(10));
                StatusMessage.AddParameterModifier(x => x.Image, (instance, options) =>
                {
                    if (instance.ImagePath.Contains("png"))
                    {
                        options.CompressionLevel = 100;
                        options.ImageFormat = ImageFormat.Png;
                    }
                    else if (instance.ImagePath.Contains("jpg"))
                    {
                        options.CompressionLevel = 70;
                        options.ImageFormat = ImageFormat.Jpeg;
                    }
                });
            });




            //var bmp = new Bitmap(@"C:\Users\gdreyer.PEERDOM\Documents\20180823_110843.jpg");
            //var bmp = new Bitmap(@"C:\Users\gdreyer.PEERDOM\Desktop\Legal_section_1_LightCurtain__637637453993433561.bmp");
            //new StatusMessage()
            //{
            //    Status = "This is a test",
            //    Type = StatusMessageType.Error,
            //    Image = bmp
            //}.Redi_Write("saw-1");
            //var saw1Status = redi.Read.Object<StatusMessage>("saw-1");



            //var watch = Stopwatch.StartNew();


            //redi.Write.Object(new StatusMessage()
            //{
            //    Status = "This is a test",
            //    Type = StatusMessageType.Error,
            //    Image = bmp
            //}, "saw-1");



            //var subs = redi.Subscribe.ToDictionary<string, StatusMessage>("operators");
            //var all = await subs.Read().Task;

            //subs.OnValueUpdate += (key, value) =>
            //{
            //    if (value is not null)
            //    {
            //        Console.WriteLine();
            //    }
            //};

            //redi.Write.Object(new StatusMessage()
            //{
            //    Status = "This is a test",
            //    Type = StatusMessageType.Error,
            //    Image = bmp
            //}, "saw-1");

            //Console.WriteLine("Time: " + watch.Elapsed.TotalMilliseconds);

            Console.WriteLine();
            Console.WriteLine();























            var dollies = new Dictionary<string, double[]>();



            //redi.Write.Object(new StatusMessage() { Status = "test" }, "saw-9");

            //var sub = redi.Subscribe.ToDictionary<string, StatusMessage>("operators");

            //var mesages = sub.Read().Task.Result;


            //sub.OnValueUpdate += (key, value) =>
            //{
            //    if (value is null)
            //    {
            //        Console.WriteLine($"{key} was deleted");
            //    }
            //    else
            //    {
            //        Console.WriteLine($"{key}:     {value.Status}");
            //    }
            //};

            //while (true)
            //{
            //    Thread.Sleep(100);
            //}

            //var stopwatch = Stopwatch.StartNew();
            //redi.Write.Dictionary(geos);
            //var list = redi.Read.Dictionary<string, GeoLocation>();
            //stopwatch.Stop();
            //Console.WriteLine("Write Time:  " + stopwatch.ElapsedMilliseconds);
            //stopwatch.Restart();

            ////var list = redi.Read.Dictionary<int, string>("mylists");

            ////stopwatch.Stop();
            ////Console.WriteLine("Read Time:  " + stopwatch.ElapsedMilliseconds);

            //Console.WriteLine();

            ////var sub = redi.Subscribe.ToDictionary<string, Dolly>();


            //if (sub is not null)
            //{
            //    foreach (var dolly in sub.Subscriptions.Keys)
            //    {
            //        dollies.Add(dolly, new double[15]);
            //    }

            //    sub.OnValueUpdate += (key, value) =>
            //    {
            //        if (value.InMotion)
            //        {
            //            var dolly = dollies[key];

            //            double motor1 = Math.Round(Math.Abs(value.Motor1Current) / 100, 1);
            //            double motor2 = Math.Round(Math.Abs(value.Motor2Current) / 100, 1);
            //            double diff = Math.Round(Math.Abs(motor1 - motor2), 1);

            //            for (int i = 0; i < dolly.Length - 1; i++)
            //                dolly[i] = dolly[i + 1];

            //            dolly[^1] = diff;
            //            diff = dolly.Average();

            //        //Console.ForegroundColor = diff switch
            //        //{
            //        //    > 5 => ConsoleColor.DarkRed,
            //        //    > 4 => ConsoleColor.Red,
            //        //    > 3 => ConsoleColor.DarkYellow,
            //        //    > 2 => ConsoleColor.Yellow,
            //        //    > 1 => ConsoleColor.DarkBlue,
            //        //    _ => ConsoleColor.Green,
            //        //};

            //        Console.ForegroundColor = ConsoleColor.White;

            //            Console.BackgroundColor = diff switch
            //            {
            //                > 5 => ConsoleColor.DarkRed,
            //                > 4 => ConsoleColor.Red,
            //                > 3 => ConsoleColor.DarkMagenta,
            //                > 2 => ConsoleColor.DarkBlue,
            //                > 1 => ConsoleColor.DarkGray,
            //                _ => ConsoleColor.Black,
            //            };

            //            Console.WriteLine("{0,-25}{1,-20}{2,-20}{3,-20}{4,-20}",
            //                $"Dolly: {key}",
            //                $"Motor1: {motor1} A",
            //                $"Motor2: {motor2} A",
            //                $"Average: {Math.Round(diff, 2)} A",
            //                $"Current Pos: {Math.Round(value.CurrentPosition / 12, 1)} ft");
            //        }
            //    };
            //}

            //while (true)
            //{
            //    Thread.Sleep(5000);
            //}
        }
    }

    public class TestLocation
    {
        public LocationZone Zone { get; set; }
        public LocationType Type { get; set; }
        public bool Available { get; set; }
        public bool Reserved { get; set; }
        public bool Bespoken { get; set; }
        // public Cradle Cradle { get; set; }
        public string CradleID { get; set; }
        public string ID { get; set; }
        public int Level { get; set; }
        public CradlePosition Position { get; set; }
        public bool EmptyStorage { get; set; }



        public LocationCradleState LocationCradleState { get; set; }
        //public List<LocationAttributes> Attributes { get; set; }



        // This is a custom property added for UI.
        //public string Location_ID { get; set; }
        //public List<Flag> Flags { get; set; }
        public List<string> MotionPlanSequences { get; set; }
        public string LowerLocationId => ID.Replace($".{Level}", $".{Level - 1}");
        public string UpperLocationId => ID.Replace($".{Level}", $".{Level + 1}");
        public string Base => ID.Replace($".{Level}", "");

        public override string ToString()
        {
            var str = $"Location: {ID}\n  Position (x,y,r,h): {Position.X}, {Position.Y}, {Position.R}, {Position.Z}";
            if (EmptyStorage)
            {
                str += "\n(empty storage)";
            }
            //if (Cradle != null)
            //{
            //    str += $"\n{Cradle}";
            //}
            return str;
        }

        public override int GetHashCode()
        {
            var lst = new List<object>
                {
                    Available,
                    Base,
                    Bespoken,
                    CradleID,
                    ID,
                    Level,
                    Position,
                    Reserved,
                    Type,
                    Zone
                };
            //if (Cradle != null)
            //{



            //    lst.AddRange(new List<object>
            //    {
            //        Cradle.CradleClass,
            //        Cradle.EstRetrievalDate,
            //        string.IsNullOrEmpty(Cradle.EstRetrievalLocation) ? "" : Cradle.EstRetrievalLocation,
            //        Cradle.ID,
            //        Cradle.Tags,
            //        Cradle.Jobs,
            //        Cradle.State
            //    }); ;
            //}
            var serialized = Newtonsoft.Json.JsonConvert.SerializeObject(lst);
            return string.GetHashCode(serialized);
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

    public class CradlePosition
    {
        public int X { get; set; }
        public int Y { get; set; }
        public int R { get; set; }
        public double Z { get; set; }
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

}
