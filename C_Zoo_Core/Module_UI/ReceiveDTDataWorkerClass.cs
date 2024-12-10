using ANB_SSZ.Module_UI;
using ANB_UI.Tools;
using GAIA.MainObject;
using System.ComponentModel;
using System.Diagnostics;
using System.Net.Http;

namespace GAIA.Module_UI
{
    class ReceiveDTDataWorkerClass
    {
        // 外部的各項屬性
        public static MainWindow? mainWin;
        public static GuideWindow? guideWindow;
        public static BackgroundWorker? bgWorker;
        public static MajorMethod? majorMethod;
        public static ClassUI? classUI;

        // 是否開始進行資料的接收
        public static bool doWork = false;

        public ReceiveDTDataWorkerClass()
        {
            // 建構式

        }

        public static void DoWorkHandler(object? sender, DoWorkEventArgs e)
        {
            // 從 DT 收到的字串
            // string DTJsonString = "";

            // 主要工作內容
            while (true)
            {
                if (doWork == true) {

                    try
                    {
                        /* 這段不用做事，將 Json 變成物件的部分，由 guideWindow 的 repaint 方法來做
                        // 取得 DT 的字串
                        DTJsonString = majorMethod.GetDTResultData();

                        // 序列化到 guideWindow 的 bearObject 中
                        guideWindow.bearObject = JsonConvert.DeserializeObject<BearObject>(DTJsonString);
                        */

                        // 回報 Progress，觸發 ProgressChanged
                        bgWorker?.ReportProgress(0);

                        // 顯示回應資料
                        // Debug.WriteLine($"時間: {DateTime.Now}, 回應: {DTJsonString}");
                    }
                    catch (HttpRequestException exception)
                    {
                        TextLog.WriteLog($"錯誤: {exception.Message}");
                    }
                }

                // 暫停 0.1 秒後再呼叫
                Thread.Sleep(100);
            }
        }

        public static void ProgressChangedHandler(object? sender, ProgressChangedEventArgs e)
        {
            // 在此對外進行工作內容報告
            string DTData = majorMethod?.GetDTResultData();

            if ((DTData != null) && (DTData != "")) {

                // 紀錄每次收到的 AR 資料和時間
                TextLog.WriteLog("UI Input:" + DTData);

                guideWindow?.RepaintScreen(DTData);
            }
        }

        public static void RunWorkerCompletedHandler(object? sender, RunWorkerCompletedEventArgs e)
        {
            // 當 Worker 執行完畢後，進行資源回收與關閉

        }

    }
}
