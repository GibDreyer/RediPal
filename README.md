# RediPal

RediPal gives you the ability to read and write dotnet objects in and out of redis


### Nuget package
https://www.nuget.org/packages/RediPal/

## Getting Started

- [Basic Usage](#Basic-Usage)
- [Creating the instance](#Creating-the-RediPal-instance)
- [Writing Objects](#Writing-objects)
    - [Dictionaries](#Dictionaries)
    - [Lists](#Lists)
    - [Objects](#Objects)
    - [Updating Properties](#Updating-Properties)

## Basic Usage


1. Creating the Connection
```c#
var redi = new Redipal("Redis-Address");
```

2. Write your object
```c#
var obj = new TestObject
{
   ID = "test",
   NestedObject = new TestObject2
   {
      ID = "Was Nested"
   }
};

redi.Write.Object(obj, "keyspace", "objectid");
```
2. Read the object back
```c#
 var testObj = redi.Read.Object<TestObject>("keyspace", "objectid");

 if (testObj != null)
    Console.WriteLine($"ID: {testObj.ID}   NestID: {testObj.NestedObject.ID}");
```




## Creating the RediPal instance

Create using an anonymous connection
```c#
var redi = new Redipal("Redis-Address");
```
Create using an existing connection
```c#
var existingRedis =  ConnectionMultiplexer.Connect("Redis-Addres");
var redi = new Redipal(existingRedis);
```
Create using configuration options
```c#
var redisConfig = new ConfigurationOptions
{
    Password = "Example",
    User = "Example"
};
redisConfig.EndPoints.Add("Redis-Address");

var redi = new Redipal(redisConfig);
```








## Writing objects



### Dictionaries
```c#
var dictionary = new Dictionary<string, TestObject>()
{
    {"test1", new TestObject() },
    {"test2", new TestObject() },
    {"test3", new TestObject() }
};

redi.Write.Dictionary(dictionary, "testobjects");
```
This will write all the keys in the dictionary as a set to "testobjects" and write each object under the key of "testobject".





### Lists
```c#
var list = new List<TestObject>()
{
    new TestObject(),
    new TestObject(),
    new TestObject()
};

redi.Write.List(list, "testobjects");
```
This will write each object under the key of "testobject" and add an index to the set of "testobjects".











## Contributing
Pull requests are welcome. For major changes, please open an issue first to discuss what you would like to change.


## License
[MIT](https://choosealicense.com/licenses/mit/)
