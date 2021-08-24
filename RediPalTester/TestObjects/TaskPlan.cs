using RedipalCore.Attributes;
using System;
using System.Collections.Generic;

namespace RedipalCore.TestObjects
{
    [RediKeySpace("task")]
    [RediSearchSet("completedtasks")]
    [RediDefaultSet("completedtasks")]
    public class TaskPlan : RediBase
    {
        //Task information
        [RediWriteName]
        public string? Task_ID { get; set; }
        public TaskType TaskType { get; set; }
        public string? CradleID { get; set; }
        public string? OperatorID { get; set; }
        public string? Status { get; set; }
        public TaskStatus TaskStatus { get; set; }
        public string? Motion_ID { get; set; }

        [RediSearchScore]
        public DateTime CreationDate { get; set; }

        public string? CreationDateRead { get; set; }
        public double EstWaitTime { get; set; }
        public double Priority { get; set; }
        public TaskPermissions Permissions { get; set; }
        public string? PreferedBridge { get; set; }
        public bool CradleStarted180 { get; set; }
        public double PercentComplete { get; set; }
        public CradleState CradleStateRequirment { get; set; }

        //Task Active Checks 
        public bool CradlePickedUp { get; set; }
        public bool TryLater { get; set; }
        public bool PlanActive { get; set; }
        public bool TaskComplete { get; set; }

        //Motion Information
        public bool UnCoveringRequired { get; set; }
        public int TotalSteps { get; set; }
        public double EstimatedRunTime { get; set; }
        public string? MoveFrom { get; set; }
        public string? MoveTo { get; set; }
        public double EstTimeToTake { get; set; }
        public double TravelDistance { get; set; }
        public double RemainingTravelDistance { get; set; }
        public bool RotationRequired { get; set; }
        public string? MotionStatus { get; set; }
        public bool HandOffRequired { get; set; }

        // Task Options
        public Cradle? StoreCradle { get; set; }
        public string? RelocateLocation { get; set; }
        public string? FromReplacementLocation { get; set; }
        public string? ReplacementCradleID { get; set; }

        //Motion Plans
        public List<MotionPlan>? MotionPlans { get; set; }

        //After completed Statistics
        public Statistics? Statistics { get; set; }

        //Task Validation
        public int TryCount { get; set; }
        public int RejectCount { get; set; }
        public int MotionPlanValidationFailure { get; set; }

        public List<string>? RejectionMessages { get; set; }
    }


    [RediKeySpace("taskstats")]
    public class Statistics
    {
        public Statistics()
        {
            CompletedByBridge = "";
        }

        public double TimeWatingOnOperator { get; set; }
        public string? CompletedByBridge { get; set; }
        public double ActualRunTime { get; set; }
        public double TotalDistanceTravled { get; set; }
        public double TimeFromCreationToComplete { get; set; }
        public double ActualTravelDistance { get; set; }
        public List<TaskLog>? TaskLog { get; set; }
        public int CradlesMoved { get; set; }
        public Cradle? TaskCradle { get; set; }
        public Cradle? ReplacementCradle { get; set; }
    }

    public class MotionPlan
    {
        public MotionPlan()
        {
            Steps = new List<Step>();
            PastCancelPoint = false;
            CradleID = "";
            PlanID = "";
            MoveFrom = "";
            MoveTo = "";
            ReservedLocation = "";
            OperatorID = "";
        }

        public TaskType TaskType { get; set; }
        public string? PlanID { get; set; }
        public bool PastCancelPoint { get; set; }
        public string? CradleID { get; set; }
        public bool UnCoveringRequired { get; set; }

        public List<Step>? Steps { get; set; }

        public string? MoveFrom { get; set; }
        public string? MoveTo { get; set; }
        public bool PlanComplete { get; set; }
        public string? ReservedLocation { get; set; }

        public bool RotationRequired { get; set; }
        public string? OperatorID { get; set; }
    }

    public enum TaskStatus
    {
        NotSet,
        Running,
        Active,
        Discarded,
        Completed
    }

    public enum TaskType
    {
        Store,
        Retrieve,
        PassThrough,
        Relocate,
        LocationRotate,
        Replace,
        BridgeCommand,
        Unkown
    }

    public enum TaskPermissions
    {
        CanAddCradles,
        CanRemoveCradles,
        CanAddAndRemoveCradles,
        PositionUpdateOnly
    }

    public enum MoveType
    {
        FromLocation,
        ToLocation,
    }

    public class TaskLog
    {
        public string? MotionID { get; set; }
        public int ReturnCode { get; set; }
        public string? Message { get; set; }
        public Currentstatus Status { get; set; }
        public double RunTime { get; set; }
        public double CradleWeight { get; set; }
        public CradleClass CradleClass { get; set; }
    }

    public enum Currentstatus
    {
        Error,
        Active,
        Qued,
        Complete,
        canceled,
        SwitchTasks,
        SafetyViolation,
        Discarded,
        ProxRemoved,
        OverWeight,
        OffBalance,
        GrippersFailedToClose
    }

    [RediWriteAsJson]
    public class Step
    {
        public Step()
        {
            Description = "";
        }

        public AxisToMove Axis { get; set; }
        public Profiles Profiles { get; set; } = new Profiles();
        public double EstSecondsToTake { get; set; }
        public bool StepComplete { get; set; }
        public double TravelDistance { get; set; }
        public string Description { get; set; }
        public bool Rotate { get; set; }
    }

    public class Profiles
    {
        public Profile X { get; set; } = new Profile();
        public Profile Y { get; set; } = new Profile();
        public Profile R { get; set; } = new Profile();
        public Hoist H { get; set; } = new Hoist();
        public GripperAction E { get; set; }
    }

    public class Profile
    {
        public double Coordinate { get; set; }
        public double Coordinate2 { get; set; }
        public float Velocity { get; set; }
        public float Accel { get; set; }
        public float Deccel { get; set; }
        public double TravelDistance { get; set; }
    }

    public class Hoist
    {
        public double Coordinate { get; set; }
        public double Coordinate2 { get; set; }
        public float Velocity { get; set; }
        public float Accel { get; set; }
        public float Deccel { get; set; }
        public bool UseInitial { get; set; }
        public bool RelieveTorque { get; set; }
        public bool CheckWeight { get; set; }
        public bool CheckProperRealease { get; set; }
        public bool CheckPlaceAtOperator { get; set; }
        public double TravelDistance { get; set; }
        public bool MonitorProx { get; set; }
        public string? ProxID { get; set; }
        public string? CradleCheckProxID { get; set; }
    }

    public enum GripperAction
    {
        Open,
        Close
    }

    public enum AxisToMove
    {
        X,
        Y,
        R,
        H,
        E,
        X_Y,
        X_Y_R,
    }

    public enum EndEffectorPosition
    {
        Unknown = -1,
        Open = 0,
        Closed = 1,
        ErrorState = 2,
    }

    public enum AxisTargetState
    {
        ErrorStop,
        Disabled,
        Standstill,
        Stopping,
        InMotion,
        Homing
    }

    public class GotoResult
    {
        public AxisToMove Axis { get; set; }
        public bool WriteSuccess { get; set; }
        public Step? Step { get; set; }
        public string? Message { get; set; }
    }

}
