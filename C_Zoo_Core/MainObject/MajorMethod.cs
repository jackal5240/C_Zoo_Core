using GAIA.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GAIA.MainObject
{
    // Language Mode
    public enum LanguageMode
    {
        Chinese,
        Englisg
    }

    // Mode 的列舉分類
    public enum ModeKind
    {
        None,
        Mode0,          // Mode0 位於 Mode1 和 Mode15_B10 的上層，用來解決跳到 Mode1 時，馬上跳到 Mode15_B10 的畫面閃爍
        Mode1,
        Mode15_A20_1,
        Mode15_A30_1,
        Mode15_A40_1,
        Mode15_A50_1,   // 新增"坐"姿態
        Mode15_B10,
        Mode2_A21_1,
        Mode2_A31_1,
        Mode2_A41_1,
        Mode2_A51_1,    // 新增"坐"姿態
        Mode2_B20_1,
        Mode2_B30_1,
        Mode2_MAP_10,
        Mode2_LIVE_10,
        Mode3_A22_1,
        Mode3_A32_1,
        Mode3_A42_1,
        Mode3_A52_1,    // 新增"坐"姿態
        ModeSP_01,      // 盲區部分，改為用 Mode 方式做切換
        ModeSP_02,
        ModeSP_03,
        ModeSP_04,
        ModeSP_05,
        ModeSP_06,
        ModeSP_0601,    // No6 次模式
        ModeSP_0602,
        ModeSP_0603,
        ModeSP_07,
        ModeSP_08,      // 20241205 新增的熊在牆左邊的盲區
        ModeSP_09,
        ModeSP_10,
        ModeSP_11,
        ModeSP_12,
        ModeSP_13,      // 水池區
    }

    // Action 的列舉分類
    public enum SubModeKine
    {
        None,
        SubMode221,
        SubMode222,
        SubMode223,
        SubMode224,
        SubMode231,
        SubMode232,
        SubMode233,
        SubMode234,
        SubMode241,     
        SubMode242,     
        SubMode243,     
        SubMode244,     
        SubMode245,     
        SubMode251,     
        SubMode252,     
        SubMode253,     
    }

    public class MajorMethod
    {
        // 目前在實驗室或是場域
        public Boolean inZoo = false;

        // 此為 Settings.json 的內容
        public SettingsObject settingsObject;

        // 此為系統之共用資料
        public string DRResultData = "";
        public string DTResultData = "";

        // 攝影機相關資料
        public static readonly string camUsername = "admin";
        public static readonly string camPassword = "Pass1234";

        // 實驗室之攝影機
        //public string camVideoUrl_lab = "rtsp://admin:Pass1234@192.168.101.200:554/stream0";
        //public string camVideoUrl_1 = "rtsp://admin:Pass1234@192.168.101.200:554/stream0";
        //public string camVideoUrl_2 = "rtsp://admin:Pass1234@192.168.101.200:554/stream0";
        //public string camVideoUrl_3 = "rtsp://admin:Pass1234@192.168.101.200:554/stream0";
        //public string camVideoUrl_4 = "rtsp://admin:Pass1234@192.168.101.200:554/stream0";
        //public string camVideoUrl_5 = "rtsp://admin:Pass1234@192.168.101.200:554/stream0";
        //public string camVideoUrl_6 = "rtsp://admin:Pass1234@192.168.101.200:554/stream0";
        //public string camVideoUrl_7 = "rtsp://admin:Pass1234@192.168.101.200:554/stream0";
        //public string camVideoUrl_8 = "rtsp://admin:Pass1234@192.168.101.200:554/stream0";
        //public string camVideoUrl_9 = "rtsp://admin:Pass1234@192.168.101.200:554/stream0";

        // 場域之攝影機 RTSP URL 配置
        /*
        public readonly string[] _rtspUrls = new string[]
        {
            "rtsp://admin:123456@192.168.86.31:554/stream0",
            "rtsp://admin:123456@192.168.86.32:554/stream0",
            "rtsp://admin:123456@192.168.86.33:554/stream0",
            "rtsp://admin:123456@192.168.86.34:554/stream0",
            "rtsp://admin:123456@192.168.86.35:554/stream0",
            "rtsp://admin:123456@192.168.86.36:554/stream0",
            "rtsp://admin:123456@192.168.86.37:554/stream0",
            "rtsp://admin:123456@192.168.86.38:554/stream0",
            "rtsp://admin:123456@192.168.86.39:554/stream0"
        };
        */

        // 場域之攝影機 RTSP URL 配置
        public string camVideoUrl_1 = "rtsp://admin:123456@192.168.86.31:554/stream0";
        public string camVideoUrl_2 = "rtsp://admin:123456@192.168.86.32:554/stream0";
        public string camVideoUrl_3 = "rtsp://admin:123456@192.168.86.33:554/stream0";
        public string camVideoUrl_4 = "rtsp://admin:123456@192.168.86.34:554/stream0";
        public string camVideoUrl_5 = "rtsp://admin:123456@192.168.86.35:554/stream0";
        public string camVideoUrl_6 = "rtsp://admin:123456@192.168.86.36:554/stream0";
        public string camVideoUrl_7 = "rtsp://admin:123456@192.168.86.37:554/stream0";
        public string camVideoUrl_8 = "rtsp://admin:123456@192.168.86.38:554/stream0";
        public string camVideoUrl_9 = "rtsp://admin:123456@192.168.86.39:554/stream0";

        // 用來設定是否顯示 AR 框線
        public bool showARData = false;
        public List<DataCollectionObject> dataBuffer = new List<DataCollectionObject>(); // 用來累積接收的資料
        public DataCollectionObject lastValidData = new DataCollectionObject(); // 儲存最後一筆有效資料
        public int arTime = 0;

        // 轉動裝置之角度與高度回報值
        public double machineDegree = 0.0;
        public double machineHeight = 0.0;

        // 轉動裝置之角度與高度校正值
        public double adjustDegree = 0.0;
        public double adjustHeight = 0.0;

        // 轉動裝置之角度序列化範圍
        public double adjustDegreeMin = 0.0;
        public double adjustDegreeMax = 0.0;

        // 裝置角度轉換
        public static double machineAbsDegreeLeft = 500.7;
        public static double machineAbsDegreeRight = 1362.0;
        public static double machineAbsDegreeWhole = machineAbsDegreeRight - machineAbsDegreeLeft;

        public static double machineRelativeDegreeLeft = -31.0;
        public static double machineRelativeDegreeRight = 111.0;
        public static double machineRelativeDegreeWhole = machineRelativeDegreeRight - machineRelativeDegreeLeft;

        // SendKey 的 Key 值
        public string SendKeyValue = "xheufSLPkwnEJjdwu";

        // Localization 中，X, Y Ratio 和 Z Meter 的值
        public double Ratio_X = 0.0;
        public double Ratio_Y = 0.0;
        public double Meter_Z = 0.0;

        // Cam 中 Conf 的閥值，此值以上代表有熊
        public double confThreshold = 0.6;
        public List<double> confThresholdList = [ 0.6, 0.6, 0.6, 0.6, 0.6, 0.6 , 0.6, 0.6, 0.6 ];

        // 從 Mode1/Mode花絮，與 發現熊 之間的切換延遲時間
        // 加入此延遲，是為了避免畫面閃動
        // 2024/11/16 先改為 0 秒
        public static TimeSpan ModeInterval = TimeSpan.FromMilliseconds(0);   // 發現熊/沒有熊 的阻尼時間
        // 上一次模式的更新時間
        public DateTime ModeLastChangeTime = DateTime.Now - TimeSpan.FromSeconds(2);

        // 裝置角度的換算資料
        //public static double MachineDegreeAbsMin = -1415.5;
        //public static double MachineDegreeAbsMax = 719.0;
        //public static double MachineDegreeMin = -30.0;
        //public static double MachineDegreeMax = 110.0;

        // RTSP 撥放器參考字串
        //public string[] options = new string[]
        //{
        //    "--network-caching=100",
        //    "--live-caching=100",
        //    "--file-caching=100",
        //    "--rtsp-tcp=0",
        //    "--clock-jitter=0",
        //    "--clock-synchro=0"
        //};

        // 黑熊在地圖上的 old value
        public int MapBearIconX = -1;   // 每天(次)的初始值皆為 -1
        public int MapBearIconY = -1;   // 每天(次)的初始值皆為 -1

        // 即時影像按鈕的時間阻尼
        public static TimeSpan LiveButtonTimeSpan = TimeSpan.FromMilliseconds(0);   // 按鈕的阻尼時間

        // 上一次模式的更新時間
        public DateTime LiveButtonTime_01 = DateTime.Now - TimeSpan.FromSeconds(2);
        public DateTime LiveButtonTime_02 = DateTime.Now - TimeSpan.FromSeconds(2);
        public DateTime LiveButtonTime_03 = DateTime.Now - TimeSpan.FromSeconds(2);
        public DateTime LiveButtonTime_04 = DateTime.Now - TimeSpan.FromSeconds(2);
        public DateTime LiveButtonTime_05 = DateTime.Now - TimeSpan.FromSeconds(2);
        public DateTime LiveButtonTime_06 = DateTime.Now - TimeSpan.FromSeconds(2);
        public DateTime LiveButtonTime_07 = DateTime.Now - TimeSpan.FromSeconds(2);
        public DateTime LiveButtonTime_08 = DateTime.Now - TimeSpan.FromSeconds(2);
        public DateTime LiveButtonTime_09 = DateTime.Now - TimeSpan.FromSeconds(2);

        // Mode SP1 的阻尼時間
        public static TimeSpan ModeSP1TimeSpan = TimeSpan.FromMilliseconds(1500);   // 按鈕的阻尼時間
        public DateTime ModeSP1Time = DateTime.Now - TimeSpan.FromSeconds(2);


        // 物件建構式
        public MajorMethod()
        { 
        
        }

        // 本物件第一個被呼叫的程式，用來建構相關的物件或方法
        public void StartMethod()
        {

        }

        // 系統結束時，呼叫的方法，用來釋放資源
        public void StopMethod()
        {
        
        }

        // 存放 DR 算結果之方法
        public void SetDRResultData(string DRResultData)
        {
            // 存放值
            this.DRResultData = DRResultData;
        }

        // 取得 DR 算結果之方法
        public string GetDRResultData()
        {
            // 取出值
            return DRResultData;
        }

        // 存放 DT 算結果之方法
        public void SetDTResultData(string DTResultData)
        {
            // 存放值
            this.DTResultData = DTResultData;
        }

        // 取得 DT 算結果之方法
        public string GetDTResultData()
        {
            // 取出值
            return DTResultData;
        }


    }
}
