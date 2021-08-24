using RedipalCore.Attributes;
using System;
using System.Collections.Generic;

namespace RedipalCore.TestObjects
{
    [RediDefaultSet("tests")]
    [RediKeySpace("bbbbb")]
    public class TestObject : RediBase
    {
        private int coolNumber = 420;

        [RediWriteName]
        public string ID { get; set; } = "Gilbert";
        public DateTime TimeofDay { get; set; } = DateTime.Now;

        public int CoolNumber
        {
            get => coolNumber; set
            {
                coolNumber = value;
                FirePropertyChanged(this, x => x.coolNumber);
            }
        }

        public bool IsIKool { get; set; } = false;

        //   public TestObject2 NestedObject { get; set; } = new TestObject2();
    }

    public class TestObject2
    {
        public string ID { get; set; } = "Was nested";
        public DateTime TimeofDay { get; set; } = DateTime.Now;
        public int CoolNumber { get; set; } = 69420;
        public bool IsIKool { get; set; } = false;
        public TestObject3 NestedObject3 { get; set; } = new TestObject3();

    }

    public class TestObject3
    {
        public string ID { get; set; } = "Gilbert";
        public DateTime TimeofDay { get; set; } = DateTime.Now;
        public int CoolNumber { get; set; } = 420;
        public bool IsIKool { get; set; } = false;

        [RediIgnore]
        public Dictionary<string, TestObject4> Dictionaries { get; set; } = new Dictionary<string, TestObject4>
        {
            {"test1", new TestObject4() },
            {"test2", new TestObject4() },
            {"test3", new TestObject4() },
            {"test4", new TestObject4() },
            {"test5", new TestObject4() },
            {"test6", new TestObject4() },
            {"test7", new TestObject4() },
            {"test8", new TestObject4() },
            {"test9", new TestObject4() },
        };
        [RediIgnore]
        public List<TestObject4> Lists { get; set; } = new List<TestObject4>
        {
           new TestObject4(),
           new TestObject4(),
           new TestObject4(),
           new TestObject4(),
           new TestObject4(),
           new TestObject4(),
        };
    }

    [RediWriteAsJson]
    public class TestObject4
    {
        public DateTime TimeofDay { get; set; } = DateTime.Now;
        [RediWriteName]
        public int CoolNumber { get; set; } = 420;
    }






    public class TestObject1Unpop
    {
        public string? ID { get; set; }
        public DateTime TimeofDay { get; set; }
        public int CoolNumber { get; set; }
        public bool IsIKool { get; set; }

        //public TestObject2Unpop? NestTest { get; set; }
    }

    public class TestObject2Unpop
    {
        public string? ID { get; set; }
        public DateTime TimeofDay { get; set; }
        public int CoolNumber { get; set; }
        public bool IsIKool { get; set; }

        public List<TestObject3Unpop>? ListObjects { get; set; }
    }

    public class TestObject3Unpop
    {
        public string? ID { get; set; }
        public DateTime TimeofDay { get; set; }
        public int CoolNumber { get; set; }
        public bool IsIKool { get; set; }

        //[RediPal.Attributes.RediIgnore]
        public Dictionary<string, TestObject4Unpop>? Dictionaries { get; set; }
    }

    public class TestObject4Unpop
    {
        public string? ID { get; set; }
        public DateTime TimeofDay { get; set; }
        public int CoolNumber { get; set; }
        public bool IsIKool { get; set; }
    }
}
