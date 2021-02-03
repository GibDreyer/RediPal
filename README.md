# RediPal

RediPal gives you the ability to read and write dotnet objects in and out of redis


## Getting Started

- [Basic Usage](#Basic-Usage)
- [Creating the instance](#Creating-the-RediPal-instance)


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





## Creating the RediPal instance

Create using an anonymous connection
```c#
var redi = new Redipal("Redis-Address");
```




## Contributing
Pull requests are welcome. For major changes, please open an issue first to discuss what you would like to change.


## License
[MIT](https://choosealicense.com/licenses/mit/)
