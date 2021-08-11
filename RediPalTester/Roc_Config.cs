using Opc.Ua;
using RedipalCore.Attributes;
using RedipalCore.TestObjects;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace RediPalTester
{
    public class Roc_Config
    {
        public static string SystemDirectory = @$"{Directory.GetCurrentDirectory()}\ProgramFiles\".Replace(@"\", "/");
        public readonly string Roc_Config_Path = @$"{SystemDirectory}\Roc_Configs\System_Config.json".Replace(@"\", "/");

        public SystemConfiguration SystemConfig { get; set; }
        public SafetySets SafetyConfig { get; set; }

        public TaskSets Task { get; set; }
        public AxesContainer Axis { get; set; }
        public Addresss Address { get; set; }

        [RediKeySpace("config:roccore")]
        public class SystemConfiguration
        {
            public SystemConfiguration()
            {
                BridgeData = new BridgeSettings();
            }
            public bool Disbale_AllMotion_TESTMODE { get; set; }
            public bool RunLocal { get; set; }
            public bool Disbale_TaskSerice_OnTaskComplete { get; set; }
            public bool Beep_On_New_Task { get; set; }
            public bool Disable_AutoMap_H { get; set; }
            public int SendBridgesHomewhenIdleFor { get; set; }
            public BridgeSettings BridgeData { get; set; }
        }

        public class BridgeSettings
        {
            public int MaxTryTaskDistance { get; set; }
            public int SafeRotateYMax { get; set; }
            public int SafeRotateYMin { get; set; }
            public int SafeRotateXMax { get; set; }
            public int SafeRotateXMin { get; set; }
            public double MaxInclinationX { get; set; }
            public double MaxInclinationY { get; set; }
            public double MaxInclinationX_Error { get; set; }
            public double MaxInclinationY_Error { get; set; }
            public int MinimumDistanceBetweenBridges { get; set; }
            public int MinimumDistanceBetweenBridgesOneAt90deg { get; set; }
            public int MinimumDistanceBetweenBridgesBothAt90deg { get; set; }
            public int MinimumHandOffChange { get; set; }
        }

        public class BridgeRules
        {
            public bool If_CurrentTask_InterferesWithOtherBridgeX_Cancel { get; set; }
        }

        public class AllowedBridgeZones
        {
            public BridgeZone MidZone { get; set; }
            public BridgeZone Paint { get; set; }
        }

        [RediKeySpace("config:roccore")]
        public class SafetySets
        {
            public SafetySets()
            {
                LightCurtain_Settings = new OnLightCurtainBroken();
            }

            public OnLightCurtainBroken LightCurtain_Settings { get; set; }
        }

        public class OnLightCurtainBroken
        {
            public OnLightCurtainBroken()
            {
                BeamProfiling = new LightCurtainBeamData();
            }

            public bool LightCurtain_Disable { get; set; }
            public int CradlePassingTimeOut { get; set; }
            public int DisarmTimeOut { get; set; }
            public bool SoundAlarmOnBridge { get; set; }
            public LightCurtainBeamData BeamProfiling { get; set; }

        }

        public class LightCurtainBeamData
        {
            public int ImageRate { get; set; }
            public int CaptureLength { get; set; }
            public int PixelSize { get; set; }
            public int CradleMaxRetries { get; set; }
           // public BeamProfile BeamProfile { get; set; }
            public List<LightCurtainBeamOffsetConfig> SectionOffsets { get; set; }
        }

        [RediWriteAsJson]
        public class LightCurtainBeamOffsetConfig
        {
            public string ID { get; set; }
            public int OffSet { get; set; }
        }

        public class UpdateService
        {
            public bool Disable_Updates_For_Paint1 { get; set; }
            public bool Disable_Updates_For_Paint2 { get; set; }
            public bool Disable_RunUpdate_OnTaskComplete { get; set; }
            public bool Disable_RunUpdate_CradleDateChange { get; set; }
            public bool Disable_Auto_RunUpdates { get; set; }
            public bool KeepSystemFlat { get; set; }
            public bool Optimize_BasedOnLowestCloseToOperator { get; set; }
            public int RunUpdate_Every_X_Minutes { get; set; }
            public bool OrderOnTopLevelByDate { get; set; }
            public int Max_Updates_PerOperator { get; set; }
        }

        [RediKeySpace("config:roccore")]
        public class Addresss
        {
            public API API { get; set; }
            public File File { get; set; }
            public string OPC_UA { get; set; }
            public string Hosting_Server { get; set; }
            public string Notification_Server_Address { get; set; }
            public int Notification_Server_Port { get; set; }
            public int GRPC_Hosting_Port { get; set; }
        }

        public class File
        {
            public File()
            {
                LoctionData = "";
                FlagData = "";
                ActiveTasks = "";
                CompletedTasks = "";
                DiscardedTasks = "";
                ErrorDump = "";
                OperatorData = "";
                SystemLogs = "";
                LightCurtainExports = "";
            }

            public string LoctionData { get; set; }
            public string FlagData { get; set; }
            public string ActiveTasks { get; set; }
            public string CompletedTasks { get; set; }
            public string DiscardedTasks { get; set; }
            public string ErrorDump { get; set; }
            public string OperatorData { get; set; }
            public string SystemLogs { get; set; }
            public string LightCurtainExports { get; set; }
            public string WallSectionConfig { get; set; }
        }

        public class API
        {
            public string LoctionAPI { get; set; }
            public string ServerAPI { get; set; }
            public string TaskAPI { get; set; }
            public string RocSignalR { get; set; }
        }

        [RediKeySpace("config:roccore")]
        public class TaskSets
        {

            public ReGenerationMode RegenerationMode { get; set; }

            public double Execute_E_TimeOut { get; set; }
            public double Execute_X_TimeOut { get; set; }
            public double Execute_Y_TimeOut { get; set; }
            public double Execute_H_TimeOut { get; set; }
            public double Execute_R_TimeOut { get; set; }
            public int Cradle_Max_Weight { get; set; }
            public int Cradle_Min_Weight { get; set; }
            public int HoistUltraSonicMin { get; set; }
            public int MaxLaserLocationDifferance { get; set; }
            public int MaxEWbeforeNS { get; set; }
            //public bool AutoPassEmpties { get; set; }

            //public bool SendEmptiesToPaint { get; set; }
            //public int DollyTimer { get; set; }
            //public List<string> SendEmpties { get; set; }
            //public int EmptyPriority { get; set; }
            //public int KeepEmptyPriority { get; set; }
            public int MinimumEmptiesToKeep { get; set; }
            //public bool SearchForOutOfPlaceEmpties { get; set; }

            //public List<string> KeepEmpty { get; set; }
            //public bool MoveOutGoingToMidZone { get; set; }
            public int OutGoingX { get; set; }
            //public List<string> OutGoingOps { get; set; }
        }

        public class AxisMax
        {
            public AxisToMove Axis { get; set; }

            public double MaxPos { get; set; }
            public double MinPos { get; set; }
            public float MaxAccel { get; set; }
            public float MinAccel { get; set; }
            public float MaxDeccel { get; set; }
            public float MinDeccel { get; set; }
            public float MaxVelocity { get; set; }
            public float MinVelocity { get; set; }
            public double CloseEnough { get; set; }

            public double MaxPos_90deg { get; set; }
            public double MinPos_90deg { get; set; }
            public float MaxAccel_90deg { get; set; }
            public float MinAccel_90deg { get; set; }
            public float MaxDeccel_90deg { get; set; }
            public float MinDeccel_90deg { get; set; }
            public float MaxVelocity_90deg { get; set; }
            public float MinVelocity_90deg { get; set; }
        }

        public class HoistSettings
        {
            public float Empty_CradleMulitpier { get; set; }
            public float Light_CradleMulitpier { get; set; }
            public float Medium_CradleMulitpier { get; set; }
            public float Heavy_CradleMulitpier { get; set; }

            public float Empty_CradleMulitpier_90 { get; set; }
            public float Light_CradleMulitpier_90 { get; set; }
            public float Medium_CradleMulitpier_90 { get; set; }
            public float Heavy_CradleMulitpier_90 { get; set; }

            public int Empty_CradleWeight { get; set; }
            public int Light_CradleWeight { get; set; }
            public int Medium_CradleWeight { get; set; }
            public int Heavy_CradleWeight { get; set; }
        }

        [RediKeySpace("config:roccore")]
        public class AxesContainer
        {
            public AxesContainer()
            {
                X = new AxisMax();
                Y = new AxisMax();
                H = new AxisMax();
                R = new AxisMax();
                HoistSettings = new HoistSettings();
            }

            public AxisMax X { get; set; }
            public AxisMax Y { get; set; }
            public AxisMax H { get; set; }
            public AxisMax R { get; set; }

            public HoistSettings HoistSettings { get; set; }

            public int Default_H_Home_pos { get; set; }
        }

        public enum ReGenerationMode
        {
            PerTask,
            PerAction,
            PerPlan
        }
    }
}
