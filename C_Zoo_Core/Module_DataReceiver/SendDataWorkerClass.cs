using ANB_SSZ.Models;
using GAIA.MainObject;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using GAIA.Models;
using System.Diagnostics;
using static System.Windows.Forms.Design.AxImporter;
using ANB_UI.Tools;
using System.Net.Sockets;
using System.Net;
using System.Windows;
using C_Zoo_Core.Models;
using Newtonsoft.Json.Linq;
using System.Net.Http;
using System.Diagnostics.Contracts;

namespace GAIA.Module_DataReceiver
{
    class SendDataWorkerClass
    {
        // 外部的各項屬性
        public static MainWindow? mainWin;
        public static BackgroundWorker? bgWorker;
        public static MajorMethod? majorMethod;
        public static ClassDR? classDR;
        public static HttpClient? httpClient = new HttpClient();

        // 是否開始進行資料的轉換
        public static Boolean doWork = true;

        // 合成之後的 Json
        public static string tempJson = "";
        public static DataCollectionObject tempDCO = new DataCollectionObject();
        public static SourceObject sourceObject = new SourceObject();
        public static List<object> selectedData;
        public static List<object> data_array_item_bear;

        public static int dataIndex = 0;
        public static int highestIndex = 0;
        public static double currentConf = -9999.0;
        public static double highestConf = -9999.0;
        public static string apiUrl = "http://127.0.0.1:6666/data_process/combined_results";

        public SendDataWorkerClass()
        {
            // 建構式

        }

        public static void DoWorkHandler(object? sender, DoWorkEventArgs e)
        {
            // 主要工作內容
            while (doWork)
            {
                try
                {
                    var response = httpClient.GetStringAsync(apiUrl).Result;
                    if (response != null && !"".Equals(response))
                    {
                        sourceObject = JsonConvert.DeserializeObject<SourceObject>(response);
                        majorMethod.SetSourceObject(sourceObject);
                        SetDRResultData();
                    }
                }
                catch (Exception ex)
                {
                    // 目前因為沒有  【辨識模組】+【旋轉機構】 資料，所以http請求會掛掉，跑到下面這行，來做假資料的串接
                    string jsonString = "{\"timestamp\": \"2024-12-09 22:37:42\", \"object_detection\": [{\"data\":[{\"class\":\"None\",\"conf\":[],\"coordinates\":[],\"pose\":\"Undetermined\"}],\"rtsp_url\":\"rtsp://admin:123456@192.168.86.31:554/stream2\"},{\"data\":[{\"class\":\"None\",\"conf\":[],\"coordinates\":[],\"pose\":\"Undetermined\"}],\"rtsp_url\":\"rtsp://admin:123456@192.168.86.32:554/stream2\"},{\"data\":[{\"class\":\"None\",\"conf\":[],\"coordinates\":[],\"pose\":\"Undetermined\"}],\"rtsp_url\":\"rtsp://admin:123456@192.168.86.33:554/stream2\"},{\"data\":[{\"class\":\"None\",\"conf\":[],\"coordinates\":[],\"pose\":\"Undetermined\"}],\"rtsp_url\":\"rtsp://admin:123456@192.168.86.34:554/stream2\"},{\"data\":[{\"class\":\"None\",\"conf\":[],\"coordinates\":[],\"pose\":\"Undetermined\"}],\"rtsp_url\":\"rtsp://admin:123456@192.168.86.35:554/stream2\"},{\"data\":[{\"class\":\"None\",\"conf\":[],\"coordinates\":[],\"pose\":\"Undetermined\"}],\"rtsp_url\":\"rtsp://admin:123456@192.168.86.36:554/stream2\"},{\"data\":[{\"class\":\"None\",\"conf\":[],\"coordinates\":[],\"pose\":\"Undetermined\"}],\"rtsp_url\":\"rtsp://admin:123456@192.168.86.37:554/stream2\"},{\"data\":[{\"class\":\"bear\",\"conf\":0.9242663383483887,\"coordinates\":[0.8250488638877869,0.7502825856208801,0.9711800217628479,0.992681622505188],\"pose\":\"walk\"}],\"rtsp_url\":\"rtsp://admin:123456@192.168.86.38:554/stream2\"},{\"data\":[{\"class\":\"None\",\"conf\":[],\"coordinates\":[],\"pose\":\"Undetermined\"}],\"rtsp_url\":\"rtsp://admin:123456@192.168.86.39:554/stream2\"}], \"localization\": [0.4, 0.5, 0], \"camera_monitor\": [1, 1, 1, 1, 1, 1, 1, 1, 1], \"area_cal\": [{\"area\": 0, \"camera\": 1, \"conf\": 0.0}, {\"area\": 0, \"camera\": 2, \"conf\": 0.0}, {\"area\": 0, \"camera\": 3, \"conf\": 0.0}, {\"area\": 0, \"camera\": 4, \"conf\": 0.0}, {\"area\": 0, \"camera\": 5, \"conf\": 0.0}, {\"area\": 0, \"camera\": 6, \"conf\": 0.0}, {\"area\": 0, \"camera\": 7, \"conf\": 0.0}, {\"area\": 0, \"camera\": 8, \"conf\": 0.0}, {\"area\": 0, \"camera\": 9, \"conf\": 0.0}], \"rotate\": {\"height\":0.90000000000000002,\"degree\":779,\"timestamp\":1733755090}}";
                    sourceObject = JsonConvert.DeserializeObject<SourceObject>(jsonString);

                    majorMethod.SetSourceObject(sourceObject);
                    SetDRResultData();
                    Console.WriteLine($"Unexpected error: {ex.Message}");
                }

                // 暫停 0.25 秒後再轉換
                Thread.Sleep(250);
            }
        }

        private static void SetDRResultData()
        {
            try
            {
                // 許多的內容，來自 Settings.json 的內容
                tempDCO.Version = majorMethod.settingsObject.Version;

                DateTime unixEpoch = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
                long unixTimestamp = (long)(DateTime.Now.ToUniversalTime() - unixEpoch).TotalMicroseconds;

                tempDCO.Timestamp = unixTimestamp;

                tempDCO.ProductName = majorMethod.settingsObject.ProductName;
                tempDCO.Env = majorMethod.settingsObject.Env;
                tempDCO.ResolutionX = majorMethod.settingsObject.ResolutionX;
                tempDCO.ResolutionY = majorMethod.settingsObject.ResolutionY;
                tempDCO.Width = majorMethod.settingsObject.Width;
                tempDCO.Height = majorMethod.settingsObject.Height;

                // 黑熊花絮
                tempDCO.AreaEgg = majorMethod.settingsObject.AreaEgg;

                // 升降裝置部分，加上校正值
                tempDCO.Machine = new Machine();
                tempDCO.Machine.Height = Convert.ToDouble(majorMethod.GetSourceObject().Rotate.Height);

                // 進行角度的轉換
                tempDCO.Machine.Degree = Convert.ToDouble(majorMethod.GetSourceObject().Rotate.Degree);

                // 把 Degree 和 Height 記錄到 majorMethod 中
                majorMethod.machineHeight = tempDCO.Machine.Height;
                majorMethod.machineDegree = tempDCO.Machine.Degree;

                // 相機-黑熊資料部分
                tempDCO.DataArray = new List<object>();

                // 地圖 X, Y 座標 Ratio
                double data_array_item_1 = majorMethod.Ratio_X;
                tempDCO.DataArray.Add(majorMethod.GetSourceObject().Localization[0]);

                double data_array_item_2 = majorMethod.Ratio_Y;
                tempDCO.DataArray.Add(majorMethod.GetSourceObject().Localization[1]);

                string data_array_item_3 = "";
                tempDCO.DataArray.Add(data_array_item_3);

                // selectData 的初始值
                selectedData = new List<object>();

                selectedData.Add(-9999.0);
                selectedData.Add(new List<double> { -9999.0, -9999.0, -9999.0, -9999.0 });
                selectedData.Add("-9999.0");
                selectedData.Add("-1");

                // 把值傳給 AR
                tempDCO.SelectedData = selectedData;

                // dataIndex 的部份
                dataIndex = 0;
                highestIndex = -1;
                highestConf = -9999.0;

                foreach (ObjectDetection cameraInfo in majorMethod.GetSourceObject().ObjectDetection)
                {
                    // 需要新增一個 List 來加入物件
                    data_array_item_bear = new List<object>();

                    if (cameraInfo == null)
                    {
                        // 如果有一個 Object 是 null，則此 Object 塞 -9999 之值
                        data_array_item_bear.Add(-9999.0);
                        data_array_item_bear.Add(new List<double> { -9999.0, -9999.0, -9999.0, -9999.0 });
                        data_array_item_bear.Add("-9999.0");

                        // 這是要記錄 timpstamp 的部分
                        data_array_item_bear.Add("Timestamp:-9999.0");

                        // 紀錄目前的 conf 值
                        currentConf = -9999.0;

                        tempDCO.DataArray.Add(data_array_item_bear);
                    }
                    else
                    {
                        Data detectionData = cameraInfo.Data[0];

                        if (detectionData.Conf.GetType().Name == "JArray")
                        {
                            data_array_item_bear.Add(-9999.0);

                            // 紀錄目前的 conf 值
                            currentConf = -9999.0;
                        }
                        else
                        {
                            data_array_item_bear.Add(detectionData.Conf);

                            // 紀錄目前的 conf 值
                            currentConf = (double)detectionData.Conf;
                        }

                        // Coordinates 的部份
                        if (detectionData.Coordinates.Count == 0)
                        {
                            data_array_item_bear.Add(new List<double> { -9999.0, -9999.0, -9999.0, -9999.0 });
                        }
                        else
                        {
                            data_array_item_bear.Add(detectionData.Coordinates);
                        }

                        // Pose 的部份
                        data_array_item_bear.Add(detectionData.Pose);

                        // Timestamp 的部分
                        data_array_item_bear.Add(detectionData.Timestamp);

                        // 將相機資料，加到 DataArray 中 
                        tempDCO.DataArray.Add(data_array_item_bear);

                        // 找出最高者，放到 selected_data 中
                        if ((currentConf >= majorMethod.confThreshold) && (currentConf > highestConf))
                        {
                            // 新增一個物件
                            selectedData = new List<object>();

                            // 紀錄目前最高者
                            highestConf = currentConf;

                            selectedData.Add(currentConf);
                            selectedData.Add(detectionData.Coordinates);
                            selectedData.Add(detectionData.Pose);
                            selectedData.Add(dataIndex);

                            // 新增的 Timestamp 要放在最後
                            selectedData.Add(detectionData.Timestamp);

                            highestIndex = dataIndex;

                            // 把值傳給 AR
                            tempDCO.SelectedData = selectedData;
                        }
                    }

                    // 記錄下一個位置
                    dataIndex += 1;
                }

                // 若 9 號相機是最高值，或是 1-8 都沒有值，則 SelectedData 變成空值
                if ((highestIndex == 8) || (highestIndex == -1))
                {
                    // selectData 的初始值
                    selectedData = new List<object>();

                    selectedData.Add(-8888.0);
                    selectedData.Add(new List<double> { -9999.0, -9999.0, -9999.0, -9999.0 });
                    selectedData.Add("-8888.0");
                    selectedData.Add("-1");

                    // 這是要記錄 timpstamp 的部分
                    selectedData.Add("Timestamp:-8888.0");

                    // 把值傳給 AR
                    tempDCO.SelectedData = selectedData;
                }

                // 相機基本資料部分(Camera Mapping)
                tempDCO.CameraMapping = majorMethod.settingsObject.CameraMapping;

                // 相機基本資料部分(Camera 角度)
                tempDCO.Camera1 = majorMethod.settingsObject.Camera1;
                tempDCO.Camera2 = majorMethod.settingsObject.Camera2;
                tempDCO.Camera3 = majorMethod.settingsObject.Camera3;
                tempDCO.Camera4 = majorMethod.settingsObject.Camera4;
                tempDCO.Camera5 = majorMethod.settingsObject.Camera5;
                tempDCO.Camera6 = majorMethod.settingsObject.Camera6;
                tempDCO.Camera7 = majorMethod.settingsObject.Camera7;
                tempDCO.Camera8 = majorMethod.settingsObject.Camera8;
                tempDCO.Camera9 = majorMethod.settingsObject.Camera9;

                // 化成 Json 字串
                tempJson = JsonConvert.SerializeObject(tempDCO);

                majorMethod.SetDRResultData(tempJson);

                // 回報 Progress，觸發 ProgressChanged
                //bgWorker?.ReportProgress(0);

                // 紀錄正確的 Json
                TextLog.WriteLog("多工 Output:" + majorMethod.GetDRResultData());
            }
            catch (Exception exception)
            {
                TextLog.WriteLog("Deserialize 錯誤:" + exception.Message);
            }
        }

        public static void ProgressChangedHandler(object? sender, ProgressChangedEventArgs e)
        {
            // 在此對外進行工作內容報告
            // mainWin.WriteReceiveDataLog(majorMethod.GetDRResultData());

            // 寫 Log
            // TextLog.WriteLog("DR Module: " + majorMethod.GetDRResultData());
        }

        public static void RunWorkerCompletedHandler(object? sender, RunWorkerCompletedEventArgs e)
        {
            // 當 Worker 執行完畢後，進行資源回收與關閉

        }

    }
}
