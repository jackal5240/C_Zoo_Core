using ANB_SSZ.Models;
using ANB_UI.Tools;
using GAIA.MainObject;
using Newtonsoft.Json;
using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Net.Http;
using System.Security.Policy;
using System.Text;
using System.Windows;

namespace GAIA.Module_DataReceiver
{
    class ReceiveCameraWorkerClass
    {
        // 外部的各項屬性
        public static MainWindow? mainWin;
        public static BackgroundWorker? bgWorker;
        public static MajorMethod? majorMethod;
        public static ClassDR? classDR;

        // 是否開始進行資料的接收
        public static Boolean doWork = false;

        // Http Client 相關物件
        private static readonly HttpClient httpClient = new HttpClient();

        // private static readonly HttpClient clientObjectDetect = new HttpClient();
        // private static readonly HttpClient clientLocal = new HttpClient();

        private static readonly string urlSendKey = "http://127.0.0.1:5000/detect_module_api/send_key";
        private static readonly string urlObjectDetection = "http://127.0.0.1:5000/detect_module_api/object_detection";
        private static readonly string urlLocalization = "http://127.0.0.1:5000/detect_module_api/localization";

        // 和 SendKey 相關的變數
        private static int SendKeyTotal = 4 * 60 * 60 * 1; // 時 * 分 * 秒 * 每秒幾次
        private static int SendKeyCount = 0;
        
        // 直接用布林變數來確認只呼叫一次
        private static bool haveCallSendKey = false;

        public ReceiveCameraWorkerClass()
        {
            // 建構式

        }

        public static void DoWorkHandler(object? sender, DoWorkEventArgs e)
        {
            // 主要工作內容
            while (true)
            {
                if (doWork == true) {

                    try
                    {
                        // 用非同步方式處理 SendKey，只呼叫第一次
                        if (haveCallSendKey == false)
                        {
                            _ = SendKey_Async();

                            haveCallSendKey = true;
                        }

                        //// ObjectDetection - 同步模式
                        //HttpResponseMessage resObjectDetection = clientObjectDetect.GetAsync(urlObjectDetection).Result;
                        //resObjectDetection.EnsureSuccessStatusCode();
                        //string resObjectDetectionBody = resObjectDetection.Content.ReadAsStringAsync().Result;
                        //classDR.CameraData = resObjectDetectionBody.Trim('\n');

                        //// Localization - 同步模式
                        //HttpResponseMessage resLocalization = clientLocal.GetAsync(urlLocalization).Result;
                        //resLocalization.EnsureSuccessStatusCode();
                        //string resLocalizationBody = resLocalization.Content.ReadAsStringAsync().Result;
                        //classDR.LocalizationData = resLocalizationBody.Trim('\n');

                        // 同步模式呼叫
                        // ObjectDetection_Sync();

                        // Localization_Sync();

                        // 非同步模式呼叫
                        _ = ObjectDetection_Async();

                        _ = Localization_Async();

                        // 非同步模式，對 ReportProgress 有問題，要加入 ConfigureAwait(false)
                        //HttpResponseMessage response = await client.GetAsync(urlObjectDetection);
                        //response.EnsureSuccessStatusCode();
                        //string responseBody = await response.Content.ReadAsStringAsync();

                        // 將 responseBody 的 Json 反解析到 CameraData 中
                        // 在 SendDataWorker 中執行
                        // CameraData camera_data = JsonConvert.DeserializeObject<CameraData>(responseBody);

                        // 將資料傳到 ClassDR 中
                        // classDR.CameraData = JsonConvert.SerializeObject(camera_data.Data);
                        // classDR.CameraData = responseBody;

                        // 回報 Progress，觸發 ProgressChanged
                        bgWorker?.ReportProgress(0);

                        // 顯示回應資料
                        // Debug.WriteLine($"時間: {DateTime.Now}, 回應: {responseBody}");
                    }
                    catch (HttpRequestException exception)
                    {
                        TextLog.WriteLog($"錯誤: {exception.Message}");
                    }
                }

                // 寫一下 Log 檔
                TextLog.WriteLog("多工 Input(黑熊偵測):" + classDR.CameraData);
                // TextLog.WriteLog("多工 Input(裝置高度角度):" + classDR.LocalizationData);

                // 暫停 0.1 秒後再呼叫
                Thread.Sleep(250);
            }
        }

        public static void ProgressChangedHandler(object? sender, ProgressChangedEventArgs e)
        {
            // 在此對外進行工作內容報告(會大量耗費資源，先不顯示)
            // mainWin?.WriteCameraLog(classDR.CameraData);
            // mainWin?.WriteCameraLog(classDR.LocalizationData);
        }

        public static void RunWorkerCompletedHandler(object? sender, RunWorkerCompletedEventArgs e)
        {
            // 當 Worker 執行完畢後，進行資源回收與關閉

        }

        // 物件偵測的同步模式
        private static void ObjectDetection_Sync()
        {
            // 同步呼叫
            HttpResponseMessage resObjectDetection = httpClient.GetAsync(urlObjectDetection).Result;
            resObjectDetection.EnsureSuccessStatusCode();
            string resObjectDetectionBody = resObjectDetection.Content.ReadAsStringAsync().Result;
            classDR.CameraData = resObjectDetectionBody.Trim('\n');
        }

        // 位置的同步模式
        private static void Localization_Sync()
        {
            // 同步呼叫
            HttpResponseMessage resLocalization = httpClient.GetAsync(urlLocalization).Result;
            resLocalization.EnsureSuccessStatusCode();
            string resLocalizationBody = resLocalization.Content.ReadAsStringAsync().Result;
            classDR.LocalizationData = resLocalizationBody.Trim('\n');
        }

        // 物件偵測的非同步模式
        private static async Task ObjectDetection_Async()
        {
            // 非同步呼叫
            HttpResponseMessage response = await httpClient.GetAsync(urlObjectDetection).ConfigureAwait(false);
            response.EnsureSuccessStatusCode();
            string resObjectDetectionBody = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
            classDR.CameraData = resObjectDetectionBody.Trim('\n');
        }

        // 位置的非同步模式
        private static async Task Localization_Async()
        {
            HttpResponseMessage resLocalization = await httpClient.GetAsync(urlLocalization).ConfigureAwait(false);
            resLocalization.EnsureSuccessStatusCode();
            string resLocalizationBody = await resLocalization.Content.ReadAsStringAsync().ConfigureAwait(false);
            classDR.LocalizationData = resLocalizationBody.Trim('\n');
        }

        private static async Task SendKey_Async()
        {
            using (var client = new HttpClient())
            {
                client.Timeout = TimeSpan.FromSeconds(10);

                try
                {
                    var apiCallTask = CallApiAsync(client);

                    TextLog.WriteLog("SendKey API 呼叫已發送，繼續執行其他工作");

                    await apiCallTask;
                }
                catch (TaskCanceledException)
                {
                    TextLog.WriteLog("SendKey API 呼叫超時或被取消");
                }
                catch (Exception ex)
                {
                    TextLog.WriteLog($"SendKey API 發生錯誤: {ex.Message}");
                }
            }
        }

        static async Task CallApiAsync(HttpClient client)
        {
            // 準備 JSON 數據 - key
            var data = new { key = majorMethod.SendKeyValue };
            var json = JsonConvert.SerializeObject(data);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            // 發送 POST 請求
            await client.PostAsync(urlSendKey, content);

            // 注意：這兒不處理回應，目前 SendKey 也不回應

        }

    }
}
