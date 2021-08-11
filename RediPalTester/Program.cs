using RedipalCore;
using System;
using System.Threading;
using System.Collections.Generic;
using System.Diagnostics;
using RediPal.TestObjects;
using System.Drawing;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Drawing.Imaging;
using System.IO.Compression;
using System.Text;
using System.Threading.Tasks;
using RedipalCore.TestObjects;
using StackExchange.Redis;
using RedipalCore.Objects;
using System.Linq;
using RedipalCore.Interfaces;
using static Org.BouncyCastle.Math.EC.ECCurve;
using static RediPalTester.Roc_Config;
using Newtonsoft.Json;
using File = System.IO.File;
using RediPal.Messages;

namespace RediPalTester
{
    class Program
    {
        static async Task Main()
        {
            var redisConfig = new ConfigurationOptions
            {
                Password = "Itunes96",
                User = ""
            };
            redisConfig.EndPoints.Add("redis-19940.c100.us-east-1-4.ec2.cloud.redislabs.com:19940");
            var redi = new Redipal(redisConfig);
            //  var redi = new Redipal("roc-redis.ag:6379");

            //var temp = "<div><div>what is it? {0} </div><div>When do we want it?  {1} </div><div> Where? {2} </div><div> 123456789 0 {3}  </div></div>";
            //var p = new[] { "A test", "Right now !", "here", "987654321" };
            //var test = string.Format(temp, p);





            redi.Write.Value("<div><div>Cradle: {0} </div><div>Operator: {1} </div></div>", "status-message:template:task-store");

            var messageSub = redi.Subscribe.ToMessages("operators");
            messageSub.OnValueUpdate += (key, value) =>
            {
                Console.WriteLine(key + "   |   " + value);
            };
            // var test = await messageSub.Read().Task;


            redi.Write.Value("<div><div>what is it? {0} </div><div>When do we want it?  {1} </div><div> Where? {2} </div><div> 123456789 0 {3}  </div></div>", "status-message:template:test-message");
            redi.Write.Value("<div><div>{0} | {1} | {2} | {3}</div></div>", "status-message:template:test-message-2");


            new Task_Store_Message() { CradleID = "1120", OperatorID = "saw-5" }.Redi_Write("saw-5");
           // var messages = redi.Read.Messages("operators");

            new RediMessage("test-message", "A test", "Right now !", "here", "987654321").Redi_Write("saw-11");

            // Passing in the constructor
            new RediMessage("test-message", "A test", "Right now !", "here", "987654321").Redi_Write("saw-9");
            // passing as a property
            new RediMessage("test-message")
            {
                Parameters = new[] { "A test", "Right now !", "here", "987654321" }
            }.Redi_Write("saw-10");


            //await Task.Delay(150);

            var watch = Stopwatch.StartNew();
            var messagess = redi.Read.Messages("operators"); // set

            watch.Stop();
            Console.WriteLine(watch.Elapsed.TotalMilliseconds);
            new Task_Store_Message() { CradleID = "1120", OperatorID = "saw-1" }.Redi_Write("saw-1");


            // Read all operator messages
            

            var locationData = JsonConvert.DeserializeObject<LocationDataObect>(File.ReadAllText(@"C:\Users\gdreyer.PEERDOM\Desktop\LocationData.json"));
            locationData.StorageLocations["bdm-2"].Redi_Write();


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

            var subscription = redi.Subscribe.ToObject<OperatorConfig>("saw-5");

            if (subscription != null)
            {
                subscription.OnChange += (s) =>
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



            var geos = new Dictionary<string, GeoLocation>
            {
                {
                    "fort_scott",
                    new("Fort Scott")
                    {
                        Items = new()
                        {
                            {
                                "roc_1",
                                new("Roc 1")
                                {
                                    Items = new()
                                    {
                                        {
                                            "saw_1",
                                            new("Saw 1")
                                        },
                                        {
                                            "bdm_1",
                                            new("BDM 1")
                                        },
                                        {
                                            "btm_1_strip1",
                                            new("BTM 1 Strip 1")
                                        }
                                    }
                                }
                            },
                            {
                                "roc_2",
                                new("Roc 2")
                                {
                                    Items = new()
                                    {
                                        {
                                            "paint_1",
                                            new("Paint 1")
                                        },
                                        {
                                            "con_1",
                                            new("Consolidation 1")
                                        },
                                        {
                                            "con_2",
                                            new("Consolidation 2")
                                        }
                                    }
                                }
                            },
                            {
                                "warehouse_paint",
                                new("Paint Warehouse")
                            },
                            {
                                "warehouse_cage",
                                new("Cage Warehouse")
                                {
                                   Items= new(){
                                        {
                                            "north_1",
                                            new("North 1")
                                        },
                                        {
                                            "south_1",
                                            new("South 1")
                                        },
                                        {
                                            "south_2",
                                            new("South 2")
                                        }
                                   }
                                }
                            }
                        }
                    }
                },
                {
                    "nevada",
                    new("Nevada ISG")
                    {
                        Items = new()
                        {
                            {
                                "roc_7",
                                new("Roc 7")
                                {
                                    Items = new()
                                    {
                                        {
                                            "saw_1",
                                            new("Saw 1")
                                        },
                                        {
                                            "bdm_1",
                                            new("BDM 1")
                                        },
                                        {
                                            "btm_1_strip1",
                                            new("BTM 1 Strip 1")
                                        }
                                    }
                                }
                            },
                            {
                                "roc_12",
                                new("Roc 12")
                                {
                                    Items = new()
                                    {
                                        {
                                            "paint_1",
                                            new("Paint 1")
                                        },
                                        {
                                            "con_1",
                                            new("Consolidation 1")
                                        },
                                        {
                                            "con_2",
                                            new("Consolidation 2")
                                        }
                                    }
                                }
                            },
                            {
                                "warehouse_paint",
                                new("Paint Warehouse")
                            },
                            {
                                "warehouse_cage",
                                new("Cage Warehouse")
                                {
                                   Items= new(){
                                        {
                                            "north_1",
                                            new("North 1")
                                        },
                                        {
                                            "south_1",
                                            new("South 1")
                                        },
                                        {
                                            "south_2",
                                            new("South 2")
                                        }
                                   }
                                }
                            }
                        }
                    }
                },
                {
                     "truck_1",
                     new("Truck 1")
                }
            };


            redi.Write.Object(new StatusMessage() { Status = "test" }, "saw-9");

            var sub = redi.Subscribe.ToDictionary<string, StatusMessage>("operators");

            var mesages = sub.Read().Task.Result;


            sub.OnValueUpdate += (key, value) =>
            {
                if (value is null)
                {
                    Console.WriteLine($"{key} was deleted");
                }
                else
                {
                    Console.WriteLine($"{key}:     {value.Status}");
                }
            };

            while (true)
            {
                Thread.Sleep(100);
            }

            var stopwatch = Stopwatch.StartNew();
            redi.Write.Dictionary(geos);
            var list = redi.Read.Dictionary<string, GeoLocation>();
            stopwatch.Stop();
            Console.WriteLine("Write Time:  " + stopwatch.ElapsedMilliseconds);
            stopwatch.Restart();

            //var list = redi.Read.Dictionary<int, string>("mylists");

            //stopwatch.Stop();
            //Console.WriteLine("Read Time:  " + stopwatch.ElapsedMilliseconds);

            Console.WriteLine();

            //var sub = redi.Subscribe.ToDictionary<string, Dolly>();


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
}
