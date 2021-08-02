//using RediPal.TestObjects;
//using RedipalCore.Attributes;
//using RedipalCore.Objects;
//using RedipalCore.TestObjects;
//using StackExchange.Redis;
//using System;
//using System.Collections.Generic;
//using System.Diagnostics;
//using System.Drawing;
//using System.Linq;
//using System.Threading;
//using System.Threading.Tasks;
//using System.Xml.Schema;

//namespace RedipalCore
//{
//    class Program
//    {
//        private int myProp = 0;
//        public int MyProperty
//        {
//            get => myProp; set
//            {
//                myProp = value;

//            }
//        }

//        private Dictionary<string, Dolly> _dollies = new();


//        static void Main()
//        {
//            //var redisConfig = new ConfigurationOptions
//            //{
//            //    Password = "Itunes96",
//            //    User = ""
//            //};
//            //redisConfig.EndPoints.Add("redis-19940.c100.us-east-1-4.ec2.cloud.redislabs.com:19940");
//            //var redi = new Redipal(redisConfig);
//            var redi = new Redipal("roc-redis.ag");

//            Bitmap? bitmap = new(@"C:\Temp\Section_8_LightCurtain__637635049833672911.bmp");

//            StatusMessage message = new(bitmap)
//            {
//                Status = "This is a test message",
//                Type = StatusMessageType.Warning
//            };

//            // redi.Write.Object(message, x => x.ID = "saw-1");
//            var subs = redi.Subscribe.ToDictionary<string, StatusMessage>("operators");

//            subs.OnValueUpdate += (key, value) =>
//            {
//                if (value is null)
//                {
//                    Console.WriteLine($"{key} was deleted");
//                }
//                else
//                {
//                    Console.WriteLine(value.Status);
//                }
//            };

//            redi.Write.Object(message, x => x.ID = "saw-1");


//            while (true)
//            {
//                Thread.Sleep(100);
//            }

//            var geos = new Dictionary<string, GeoLocation>
//            {
//                {
//                    "fort_scott",
//                    new("Fort Scott")
//                    {
//                        Longitude = -94.7040076,
//                        Latitude = 37.8101345,
//                        Type = GeoLocationType.Facility,
//                        Items = new()
//                        {
//                            {
//                                "roc_1",
//                                new("Roc 1")
//                                {
//                                    Type = GeoLocationType.Equipment,
//                                    Items = new()
//                                    {
//                                        {
//                                            "saw_1",
//                                            new("Saw 1")
//                                            {
//                                                  Type = GeoLocationType.Operator
//                                            }
//                                        },
//                                        {
//                                            "bdm_1",
//                                            new("BDM 1")
//                                            {
//                                                  Type = GeoLocationType.Operator
//                                            }
//                                        },
//                                        {
//                                            "btm_1_strip1",
//                                            new("BTM 1 Strip 1")
//                                            {
//                                                  Type = GeoLocationType.Operator
//                                            }
//                                        }
//                                    }
//                                }
//                            },
//                            {
//                                "roc_2",
//                                new("Roc 2")
//                                {
//                                    Type = GeoLocationType.Equipment,
//                                }
//                            },
//                            {
//                                "warehouse_paint",
//                                new("Paint Warehouse")
//                            }
//                        }
//                    }
//                },
//                {
//                    "nevada",
//                    new("Nevada ISG")
//                    {
//                        Longitude = -94.342674,
//                        Latitude = 37.855694,
//                        Type = GeoLocationType.Facility,
//                        Items = new()
//                        {
//                            {
//                                "roc_7",
//                                new("Roc 7")
//                                {
//                                    Type = GeoLocationType.Equipment,
//                                    Items = new()
//                                    {
//                                        {
//                                            "saw_1",
//                                            new("Saw 1")
//                                            {
//                                                  Type = GeoLocationType.Operator
//                                            }
//                                        },
//                                        {
//                                            "bdm_1",
//                                            new("BDM 1")
//                                            {
//                                                  Type = GeoLocationType.Operator
//                                            }
//                                        },
//                                        {
//                                            "btm_1_strip1",
//                                            new("BTM 1 Strip 1")
//                                            {
//                                                  Type = GeoLocationType.Operator
//                                            }
//                                        }
//                                    }
//                                }
//                            },
//                            {
//                                "roc_12",
//                                new("Roc 12")
//                                {
//                                    Type = GeoLocationType.Equipment,
//                                    Items = new()
//                                    {
//                                        {
//                                            "paint_1",
//                                            new("Paint 1")
//                                            {
//                                                  Type = GeoLocationType.Operator
//                                            }
//                                        },
//                                        {
//                                            "con_1",
//                                            new("Consolidation 1")
//                                            {
//                                                  Type = GeoLocationType.Operator
//                                            }
//                                        },
//                                        {
//                                            "con_2",
//                                            new("Consolidation 2")
//                                            {
//                                                  Type = GeoLocationType.Operator
//                                            }
//                                        }
//                                    }
//                                }
//                            },
//                            {
//                                "warehouse_paint",
//                                new("Paint Warehouse")
//                                {
//                                    Type = GeoLocationType.Warehouse
//                                }
//                            },
//                            {
//                                "warehouse_cage",
//                                new("Cage Warehouse")
//                                {
//                                   Type = GeoLocationType.Warehouse,
//                                   Items= new(){
//                                        {
//                                            "north_1",
//                                            new("North 1")
//                                            {
//                                                  Type = GeoLocationType.Operator
//                                            }
//                                        },
//                                        {
//                                            "south_1",
//                                            new("South 1")
//                                            {
//                                                  Type = GeoLocationType.Operator
//                                            }
//                                        },
//                                        {
//                                            "south_2",
//                                            new("South 2")
//                                            {
//                                                  Type = GeoLocationType.Operator
//                                            }
//                                        }
//                                   }
//                                }
//                            }
//                        }
//                    }
//                },
//                {
//                     "truck_1",
//                     new("Truck 1")
//                     {
//                          Type= GeoLocationType.Transport
//                     }
//                }
//            };

//            var geos2 = new Dictionary<string, GeoLocation>
//            {
//                {
//                    "fort_scott",
//                    new("Fort Scott")
//                    {
//                        Items = new()
//                        {
//                            {
//                                "roc_1",
//                                new("Roc 1")
//                                {
//                                    Items = new()
//                                    {
//                                        {
//                                            "saw_1",
//                                            new("Saw 1")
//                                        }
//                                    }
//                                }
//                            }
//                        }
//                    }
//                }
//            };



//            //redi.Eradicate.Key("geos");

//            // redi.Write.Dictionary(geos, "locations");

//            var stopwatch = Stopwatch.StartNew();
//            var list = redi.Read.Dictionary<string, GeoLocation>("locations");
//            stopwatch.Stop();
//            Console.WriteLine("Write Time:  " + stopwatch.ElapsedMilliseconds);
//            stopwatch.Restart();





//            //"geo:fort_scott"
//            //"geo:fort_scott[]"
//            //"geo:fort_scott[roc_1]"
//            //"geo:fort_scott[roc_1][]"


//            // var x = new GeoLocation();

//            // var test = x[""][""];



//            // var ft = redi.Read.List<GeoLocation>("geo:fort_scott:child", "roc_1", "roc_2");


//            // var redi = new Redipal("roc-redis");
//            //var count = -1;

//            // redi.Read.List<Location>("operatorlocations")!.OrderBy(x=> x.Position.X).ToList().ForEach(x => x.Redi_WriteAs<Dolly, int, Location>(x.ID, x => x.Index = count++, x => x.Index));

//            //redi.Factory!.TypeDescriptor.TryGetDescriptor(typeof(Location));

//            var stopWatch = new Stopwatch();
//            stopWatch.Start();


//            //var tasks = redi.Read.List<TaskPlan, List<MotionPlan>>("activetasks", x=> x.MotionPlans);

//            //var ops = redi.Read.List<string>("operatorlocations");


//            // var opIDs = redi.Read.List<Location, string>("operatorlocations", x => x.ID!);


//            //var locationSub = redi.Subscribe.ToDictionary<string, Location>(x =>
//            //                       {
//            //                           x.WatchForAdd = false;
//            //                           x.WatchForRemove = false;
//            //                       });





//            // Create the Redis Connection
//            //var redi = new Redipal("roc-redis");

//            //var opsID = new List<string>
//            //{
//            //    "north-1",
//            //    "btm-2-strip1",
//            //    "btm-1-ext",
//            //    "bulb-2a",
//            //    "saw-10",
//            //    "saw-7",
//            //    "btm-2-int",
//            //    "saw-5",
//            //    "btm-2-strip2",
//            //    "paint-2",
//            //    "bulb-1a",
//            //    "saw-1",
//            //    "btm-1-strip2",
//            //    "btm-1-out",
//            //    "bulb-1b",
//            //    "btm-2-out",
//            //    "btm-2-ext",
//            //    "btm-1-strip1",
//            //    "saw-11",
//            //    "bulb-2b",
//            //    "saw-9",
//            //    "bdm-1",
//            //    "btm-1-int",
//            //    "paint-3",
//            //    "paint-1",
//            //    "saw-3",
//            //    "paint-4",
//            //};

//            //redi.Write.List(opsID, "operators");
//            //redi.Write.List(opsID, "operatorlocations");


//            //redi.SetTypeDefaults<OperatorConfig>(config =>
//            //{
//            //    config.DefaultSet = "operators";
//            //    config.KeySpace = "operator";
//            //    config.AddParameterModifier((x, a) => a.AppendPostID($"config"));
//            //});


//            //// Reading Works
//            //var opConfigs = redi.Read.Dictionary<string, OperatorConfig>();

//            //var bulb_1a = opConfigs!["bulb-1a"];
//            //// Writing Works
//            //bulb_1a.Redi_Write(action => action.EmptyDollyTimer++, x => x.EmptyDollyTimer);

//            //// Subscribing Works
//            //var sub = redi.Subscribe.ToDictionary<string, OperatorConfig>();
//            //if (sub is not null)
//            //{
//            //    var opconfigs = sub.Read().Task.Result;
//            //    sub.OnValueUpdate += (key, value) =>
//            //    {
//            //        Console.WriteLine($"{key} updated");
//            //    };
//            //}





//            //Console.WriteLine();



//            //// Operator Locaitons
//            //Dictionary<string, Location>? operatorLocations;

//            //// Create the Subscription To Operator Locations
//            //var operatorSub = redi.Subscribe.ToDictionary<string, Location>("operatorlocations", x =>
//            //{
//            //    x.WatchForAdd = false;
//            //    x.WatchForRemove = false;
//            //});

//            //// Confirm the Subscription was successful
//            //if (operatorSub is not null)
//            //{
//            //    // Build your local copy of locations if needed
//            //    var read = operatorSub.Read();

//            //    read.OnProggress += (a, b) =>
//            //    {
//            //        Console.WriteLine($"Read {a} of {b}");
//            //    };

//            //    operatorLocations = read.Task.Result;

//            //    // When something about that operator changes
//            //    operatorSub.OnValueUpdate += (key, value) =>
//            //    {
//            //        Console.WriteLine($"{key} was just updated");
//            //        operatorLocations[key] = value;
//            //    };
//            //}



//            ////// Task Stuff

//            //Dictionary<string, TaskPlan>? activeTasks;

//            //// Get Active Tasks
//            //var activeTaskSub = redi.Subscribe.ToDictionary<string, TaskPlan>("activetasks");
//            //// Confirm the Subscription was successful
//            //if (activeTaskSub is not null)
//            //{
//            //    // Build your local copy of locations if needed
//            //    activeTasks = activeTaskSub.Read().Task.Result;

//            //    // Add the event handler
//            //    activeTaskSub.OnValueUpdate += (key, value) =>
//            //    {
//            //        Console.WriteLine($"atask: {key} was just updated");
//            //        activeTasks[key] = value;
//            //    };

//            //    // When a new task is added
//            //    activeTaskSub.OnAdded += (key, value) =>
//            //    {
//            //        if (value is not null)
//            //        {
//            //            Console.WriteLine($"Task: {key} was just added");
//            //            activeTasks.Add(key, value);
//            //        }
//            //    };

//            //    // When a task is no longer active
//            //    activeTaskSub.OnRemoved += (key) =>
//            //    {
//            //        Console.WriteLine($"Task: {key} is no longer active");
//            //        activeTasks.Remove(key);
//            //    };
//            //}


//            //// Get Active Tasks
//            //var runningTaskSub = redi.Subscribe.ToDictionary<string, TaskPlan>("runningtasks");
//            //// Confirm the Subscription was successful
//            //if (runningTaskSub is not null)
//            //{
//            //    // When a task is running
//            //    runningTaskSub.OnAdded += (key, value) =>
//            //    {
//            //        Console.WriteLine($"Task: {key} is not running");
//            //    };

//            //    // When a task is no longer running
//            //    runningTaskSub.OnRemoved += (key) =>
//            //    {
//            //        Console.WriteLine($"Task: {key} is no longer running");
//            //    };
//            //}





//            //Dictionary<string, TaskPlan>? completedTasks;
//            //// Get Active Tasks
//            //var completedtasksTaskSub = redi.Subscribe.ToDictionary<string, TaskPlan>("completedtasks", x =>
//            //{
//            //    x.MaxMembers = 20000;
//            //});

//            //// Confirm the Subscription was successful
//            //if (completedtasksTaskSub is not null)
//            //{
//            //    var readTask = completedtasksTaskSub.Read();

//            //    readTask.OnProggress += (read, total) =>
//            //    {
//            //        Console.WriteLine($"{read} read of {total}");
//            //    };

//            //    readTask.OnComplete += (dict) =>
//            //    {
//            //        Console.WriteLine("Read Complete  " + dict.Count);
//            //    };

//            //    completedTasks = readTask.Task.Result;

//            //    // When a task is running
//            //    completedtasksTaskSub.OnAdded += (key, value) =>
//            //    {
//            //        Console.WriteLine($"Task: {key} was complete");
//            //    };
//            //}


//            stopWatch.Stop();

//            // RediBase.Redi_Write<Dolly, int>("bdm-1", x => x.Index = 5, x => x.Index);

//            //RediBase.Redi_Write<Dolly, int>("test", x => x.Index = 6, x => x.Index);
//            Console.WriteLine("\n\n\n\n Time: " + stopWatch.ElapsedMilliseconds);

//            Console.WriteLine("done");
//            while (true)
//            {
//                Thread.Sleep(100);
//            }
//        }
//    }
//}
