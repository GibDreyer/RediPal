# RediPal

RediPal gives you the ability to read and write dotnet objects in and out of redis


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


## Contributing
Pull requests are welcome. For major changes, please open an issue first to discuss what you would like to change.


## License
[MIT](https://choosealicense.com/licenses/mit/)
