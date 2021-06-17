﻿//using RedipalCore.Attributes;
//using RedipalCore.Objects;
//using RedipalCore.TestObjects;
//using StackExchange.Redis;
//using System;
//using System.Collections.Generic;
//using System.Diagnostics;
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
//            //    Password = "*******",
//            //    User = ""
//            //};
//            //redisConfig.EndPoints.Add("redis-******************.cloud.redislabs.com");

//            //var redi = new Redipal(redisConfig, new RediConfig { UnThrottleCPU = true });

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
//            var redi = new Redipal("roc-redis");

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


//            redi.SetTypeDefaults<OperatorConfig>(config =>
//            {
//                config.DefaultSet = "operators";
//                config.KeySpace = "operator";
//                config.AddParameterModifier((x, a) => a.AppendPostID($"config"));
//            });
            

//            // Reading Works
//            var opConfigs = redi.Read.Dictionary<string, OperatorConfig>();

//            var bulb_1a = opConfigs!["bulb-1a"];
//            // Writing Works
//            bulb_1a.Redi_Write(action => action.EmptyDollyTimer++, x => x.EmptyDollyTimer);

//            // Subscribing Works
//            var sub = redi.Subscribe.ToDictionary<string, OperatorConfig>();
//            if (sub is not null)
//            {
//                var opconfigs = sub.Read().Task.Result;
//                sub.OnValueUpdate += (key, value) =>
//                {
//                    Console.WriteLine($"{key} updated");
//                };
//            }




//            Console.WriteLine();



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



//            //// Task Stuff

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
