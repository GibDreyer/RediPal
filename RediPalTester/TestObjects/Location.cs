using RedipalCore.Attributes;
using RedipalCore.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;

namespace RedipalCore.TestObjects
{
    public class Location : RediBase
    {
        public Location()
        {
            Position = new Position();
            Tags = new List<string>();
        }

        [RediWriteName]
        public string? ID { get; set; }
        public string? CradleID { get; set; }
        public LocationCradleState LocationCradleState
        {
            get
            {
                if (!Available)
                {
                    
                        return LocationCradleState.Inlocation;
                }
                else
                {
                    return LocationCradleState.Open;
                }
            }
        }

        public LocationZone Zone { get; set; }
        public LocationType Type { get; set; }
        public bool Available { get; set; }
        public bool Bespoken { get; set; }
        public bool EmptyStorage { get; set; }
        public string? BaseID { get; set; }
        public int Level { get; set; }
        public Position Position { get; set; }
        public bool HMapped { get; set; }
     
        public List<string> Tags { get; set; }
    }

    public class Position
    {
        public Position()
        {
            Z_History = new double[5];
            ZB_History = new double[5];
        }

        public double X { get; set; }
        public double Y { get; set; }
        public double R { get; set; }

        public double Z
        {
            get
            {
                if (Z_History.All(x => x == 0))
                {
                    return 0;
                }
                else
                {
                    return Z_History.Where(x => x > 0).Average();
                }
            }
            set
            {
                var newValue = Math.Round(value);
                if (!Z_History.Contains(newValue))
                {
                    var newZ = new double[5];
                    Array.Copy(Z_History, 0, newZ, 1, Z_History.Length - 1);
                    Z_History = newZ;
                    Z_History[0] = newValue;
                }
            }
        }
        public double[] Z_History { get; set; }
        public double ZB
        {
            get
            {
                if (ZB_History.All(x => x == 0))
                    ZB_History[0] = Z;

                if (ZB_History.All(x => x == 0))
                {
                    return 0;
                }
                else
                {
                    return ZB_History.Where(x => x > 0).Average();
                }
            }
            set
            {
                var newValue = Math.Round(value);
                if (!ZB_History.Contains(newValue))
                {
                    var newZ = new double[5];
                    Array.Copy(ZB_History, 0, newZ, 1, ZB_History.Length - 1);
                    ZB_History = newZ;
                    ZB_History[0] = newValue;
                }
            }
        }
        public double[] ZB_History { get; set; }

        public void CaptureH()
        {
            //      90 deg            270 deg            |                     0 deg
            //      [ ] A = South     [ ] B = South      |        East = A [ ]---------[ ] West = B
            //       |                 |                 |                  
            //       |                 |                 |      
            //       |                 |                 |                     180 deg
            //      [ ] B = North     [ ] A = North      |        East = B [ ]---------[ ] West = A

            var adder = 15;
            var currentR = 90;
            var currentH = 2200;
            var currentH2 = 2200;

            if (true)
            {

                if ((currentR <= 2 || currentR >= 358) || (currentR >= 88 && currentR <= 92))        // A then B 
                {
                    Z = currentH + adder;
                    ZB = currentH2 + adder;
                }

                else if ((currentR >= 178 && currentR <= 182) || (currentR >= 268 && currentR <= 272)) // B then A
                {
                    Z = currentH2 + adder;
                    ZB = currentH + adder;
                }
            }
        }

        public (double a, double b) GetH(double currentR)
        {
            //      90 deg            270 deg            |                     0 deg
            //      [ ] A = South     [ ] B = South      |        East = A [ ]---------[ ] West = B
            //       |                 |                 |                  
            //       |                 |                 |      
            //       |                 |                 |                     180 deg
            //      [ ] B = North     [ ] A = North      |        East = B [ ]---------[ ] West = A


            if ((currentR <= 2 || currentR >= 358) || (currentR >= 88 && currentR <= 92))        // A then B 
            {
                return (Z, ZB);
            }
            return (ZB, Z);
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
        Operator,
        InputOutput,
        Storage,
        OperatorBuffer,
        LongTermStorage,
        Disabled,
        Bridge,
        DisabledOperator
    }

    public enum LocationCradleState
    {
        Open,
        Inlocation,
        InUse
    }

    public class LocationDataObect
    {
        public LocationDataObect()
        {
            Log = "";
        }
        public string Log { get; set; }
        public Dictionary<string, Location>? StorageLocations { get; set; }
        public Dictionary<string, Cradle>? Cradles { get; set; }
    }
}