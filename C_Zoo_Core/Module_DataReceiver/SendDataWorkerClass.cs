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

namespace GAIA.Module_DataReceiver
{
    class SendDataWorkerClass
    {
        // 外部的各項屬性
        public static MainWindow? mainWin;
        public static BackgroundWorker? bgWorker;
        public static MajorMethod? majorMethod;
        public static ClassDR? classDR;

        // 是否開始進行資料的轉換
        public static Boolean doWork = true;

        // 合成之後的 Json
        public static string tempJson = "";

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
                    // 建立 Socket 連接
                    using (var client = new TcpClient("127.0.0.1", 6666))
                    using (var stream = client.GetStream())
                    {
                        Console.WriteLine("Connected to server. Listening for JSON data...");

                        // 緩衝區與資料累積
                        var buffer = new byte[4096];
                        var jsonBuilder = new StringBuilder();

                        while (true)
                        {
                            // 從 Socket 中接收資料
                            int bytesRead = stream.Read(buffer, 0, buffer.Length);
                            if (bytesRead > 0)
                            {
                                // 將收到的資料轉換為字串
                                string receivedData = Encoding.UTF8.GetString(buffer, 0, bytesRead);
                                jsonBuilder.Append(receivedData);

                                // 嘗試解析完整 JSON 資料
                                try
                                {
                                    string jsonString = jsonBuilder.ToString();
                                    //jsonString = "{\"timestamp\": \"2024-12-09 22:37:42\", \"object_detection\": [{\"data\": [{\"class\": \"None\", \"conf\": [], \"coordinates\": [], \"pose\": \"Undetermined\", \"timestamp\": \"2024-12-09 22:37:43.032886\"}], \"rtsp_url\": \"rtsp://admin:123456@192.168.86.31:554/stream0\"}, {\"data\": [{\"class\": \"None\", \"conf\": [], \"coordinates\": [], \"pose\": \"Undetermined\", \"timestamp\": \"2024-12-09 22:37:42.901195\"}], \"rtsp_url\": \"rtsp://admin:123456@192.168.86.32:554/stream0\"}, {\"data\": [{\"class\": \"None\", \"conf\": [], \"coordinates\": [], \"pose\": \"Undetermined\", \"timestamp\": \"2024-12-09 22:37:42.942125\"}], \"rtsp_url\": \"rtsp://admin:123456@192.168.86.33:554/stream0\"}, {\"data\": [{\"class\": \"None\", \"conf\": [], \"coordinates\": [], \"pose\": \"Undetermined\", \"timestamp\": \"2024-12-09 22:37:42.991679\"}], \"rtsp_url\": \"rtsp://admin:123456@192.168.86.34:554/stream0\"}, {\"data\": [{\"class\": \"None\", \"conf\": [], \"coordinates\": [], \"pose\": \"Undetermined\", \"timestamp\": \"2024-12-09 22:37:42.846299\"}], \"rtsp_url\": \"rtsp://admin:123456@192.168.86.35:554/stream0\"}, {\"data\": [{\"class\": \"None\", \"conf\": [], \"coordinates\": [], \"pose\": \"Undetermined\", \"timestamp\": \"2024-12-09 22:37:42.834889\"}], \"rtsp_url\": \"rtsp://admin:123456@192.168.86.36:554/stream0\"}, {\"data\": [{\"class\": \"None\", \"conf\": [], \"coordinates\": [], \"pose\": \"Undetermined\", \"timestamp\": \"2024-12-09 22:37:43.022988\"}], \"rtsp_url\": \"rtsp://admin:123456@192.168.86.37:554/stream0\"}, {\"data\": [{\"class\": \"None\", \"conf\": [], \"coordinates\": [], \"pose\": \"Undetermined\", \"timestamp\": \"2024-12-09 22:37:42.900613\"}], \"rtsp_url\": \"rtsp://admin:123456@192.168.86.38:554/stream0\"}, {\"data\": [{\"class\": \"None\", \"conf\": [], \"coordinates\": [], \"pose\": \"Undetermined\", \"timestamp\": \"2024-12-09 22:37:42.954781\"}], \"rtsp_url\": \"rtsp://admin:123456@192.168.86.39:554/stream0\"}], \"localization\": [0, 0, 0], \"camera_monitor\": [1, 1, 1, 1, 1, 1, 1, 1, 1], \"area_cal\": [{\"area\": 0, \"camera\": 1, \"conf\": 0.0}, {\"area\": 0, \"camera\": 2, \"conf\": 0.0}, {\"area\": 0, \"camera\": 3, \"conf\": 0.0}, {\"area\": 0, \"camera\": 4, \"conf\": 0.0}, {\"area\": 0, \"camera\": 5, \"conf\": 0.0}, {\"area\": 0, \"camera\": 6, \"conf\": 0.0}, {\"area\": 0, \"camera\": 7, \"conf\": 0.0}, {\"area\": 0, \"camera\": 8, \"conf\": 0.0}, {\"area\": 0, \"camera\": 9, \"conf\": 0.0}], \"rotate\": \"{\"height\":0.90000000000000002,\"degree\":779,\"timestamp\":1733755090}\"}";
                                    SourceObject sourceObject = JsonConvert.DeserializeObject<SourceObject>(jsonString);

                                    majorMethod.SetSourceObject(sourceObject);
                                    //// 輸出解析後的物件內容
                                    //Console.WriteLine("Received and parsed JSON:");
                                    //Console.WriteLine($"Timestamp: {sourceObject.Timestamp}");
                                    //Console.WriteLine($"Object Detection Count: {sourceObject.ObjectDetection.Count}");
                                    //Console.WriteLine($"Localization: {string.Join(", ", sourceObject.Localization)}");
                                    //Console.WriteLine($"Camera Monitor: {string.Join(", ", sourceObject.CameraMonitor)}");
                                    //Console.WriteLine($"Area Cal: {sourceObject.AreaCal.Count} entries");

                                    // 清空 StringBuilder 準備接收下一筆資料
                                    jsonBuilder.Clear();
                                }
                                catch (JsonReaderException)
                                {
                                    // 如果 JSON 資料尚未完整，繼續累積
                                    continue;
                                }
                            }
                        }
                    }
                }
                catch (SocketException ex)
                {
                    string jsonString = "{\"timestamp\": \"2024-12-09 22:37:42\", \"object_detection\": [{\"data\": [{\"class\": \"None\", \"conf\": [], \"coordinates\": [], \"pose\": \"Undetermined\", \"timestamp\": \"2024-12-09 22:37:43.032886\"}], \"rtsp_url\": \"rtsp://admin:123456@192.168.86.31:554/stream0\"}, {\"data\": [{\"class\": \"None\", \"conf\": [], \"coordinates\": [], \"pose\": \"Undetermined\", \"timestamp\": \"2024-12-09 22:37:42.901195\"}], \"rtsp_url\": \"rtsp://admin:123456@192.168.86.32:554/stream0\"}, {\"data\": [{\"class\": \"None\", \"conf\": [], \"coordinates\": [], \"pose\": \"Undetermined\", \"timestamp\": \"2024-12-09 22:37:42.942125\"}], \"rtsp_url\": \"rtsp://admin:123456@192.168.86.33:554/stream0\"}, {\"data\": [{\"class\": \"None\", \"conf\": [], \"coordinates\": [], \"pose\": \"Undetermined\", \"timestamp\": \"2024-12-09 22:37:42.991679\"}], \"rtsp_url\": \"rtsp://admin:123456@192.168.86.34:554/stream0\"}, {\"data\": [{\"class\": \"None\", \"conf\": [], \"coordinates\": [], \"pose\": \"Undetermined\", \"timestamp\": \"2024-12-09 22:37:42.846299\"}], \"rtsp_url\": \"rtsp://admin:123456@192.168.86.35:554/stream0\"}, {\"data\": [{\"class\": \"None\", \"conf\": [], \"coordinates\": [], \"pose\": \"Undetermined\", \"timestamp\": \"2024-12-09 22:37:42.834889\"}], \"rtsp_url\": \"rtsp://admin:123456@192.168.86.36:554/stream0\"}, {\"data\": [{\"class\": \"None\", \"conf\": [], \"coordinates\": [], \"pose\": \"Undetermined\", \"timestamp\": \"2024-12-09 22:37:43.022988\"}], \"rtsp_url\": \"rtsp://admin:123456@192.168.86.37:554/stream0\"}, {\"data\": [{\"class\": \"None\", \"conf\": [], \"coordinates\": [], \"pose\": \"Undetermined\", \"timestamp\": \"2024-12-09 22:37:42.900613\"}], \"rtsp_url\": \"rtsp://admin:123456@192.168.86.38:554/stream0\"}, {\"data\": [{\"class\": \"None\", \"conf\": [], \"coordinates\": [], \"pose\": \"Undetermined\", \"timestamp\": \"2024-12-09 22:37:42.954781\"}], \"rtsp_url\": \"rtsp://admin:123456@192.168.86.39:554/stream0\"}], \"localization\": [0, 0, 0], \"camera_monitor\": [1, 1, 1, 1, 1, 1, 1, 1, 1], \"area_cal\": [{\"area\": 0, \"camera\": 1, \"conf\": 0.0}, {\"area\": 0, \"camera\": 2, \"conf\": 0.0}, {\"area\": 0, \"camera\": 3, \"conf\": 0.0}, {\"area\": 0, \"camera\": 4, \"conf\": 0.0}, {\"area\": 0, \"camera\": 5, \"conf\": 0.0}, {\"area\": 0, \"camera\": 6, \"conf\": 0.0}, {\"area\": 0, \"camera\": 7, \"conf\": 0.0}, {\"area\": 0, \"camera\": 8, \"conf\": 0.0}, {\"area\": 0, \"camera\": 9, \"conf\": 0.0}], \"rotate\": \"{\"height\":0.90000000000000002,\"degree\":779,\"timestamp\":1733755090}\"}";
                    SourceObject sourceObject = JsonConvert.DeserializeObject<SourceObject>(jsonString);

                    majorMethod.SetSourceObject(sourceObject);
                    Console.WriteLine($"Socket error: {ex.Message}");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Unexpected error: {ex.Message}");
                }

                // 暫停 0.25 秒後再轉換
                Thread.Sleep(250);
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
