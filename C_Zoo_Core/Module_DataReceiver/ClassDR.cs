using GAIA.MainObject;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GAIA.Module_DataReceiver
{
    class ClassDR
    {
        // 架構物件
        MajorMethod? majorMethod;

        // 主視窗物件
        MainWindow? mainWin;

        // 記錄從 Camera 模組取得的即時資料
        public string CameraData = "";
        public string LocalizationData = "";

        // 記錄從 升降裝置 取得的即時資料
        public string MachineData = "";

        // 在此建立多執行序物件，包含主要的 Camera 與 透顯裝置物件，以及一個常駐執行緒，負責統整資料
        public BackgroundWorker? SendDataWorker;
        public BackgroundWorker? ReceiveCameraDataWorker;
        public BackgroundWorker? ReceiveMonitorWorker;

        // 本物件第一個被呼叫的程式，用來建構相關的物件或方法
        public void StartMethod(MainWindow mainWin, MajorMethod majorMethod)
        {
            // 紀錄主要視窗
            this.mainWin = mainWin;

            // 紀錄 MajorMethod 物件，以便需要時可以呼叫
            this.majorMethod = majorMethod;

            // 建立各執行緒與相關物件
            CreateWorker();

            // 設定 Worker 的各項屬性
            //ReceiveMonitorWorkerClass.mainWin = mainWin;
            //ReceiveMonitorWorkerClass.bgWorker = ReceiveMonitorWorker;
            //ReceiveMonitorWorkerClass.majorMethod = majorMethod;
            //ReceiveMonitorWorkerClass.classDR = this;

            //ReceiveCameraWorkerClass.mainWin = mainWin;
            //ReceiveCameraWorkerClass.bgWorker = ReceiveCameraDataWorker;
            //ReceiveCameraWorkerClass.majorMethod = majorMethod;
            //ReceiveCameraWorkerClass.classDR = this;

            SendDataWorkerClass.mainWin = mainWin;
            SendDataWorkerClass.bgWorker = SendDataWorker;
            SendDataWorkerClass.majorMethod = majorMethod;
            SendDataWorkerClass.classDR = this;

            // 開始啟動各執行緒
            SendDataWorker?.RunWorkerAsync();
            mainWin?.WriteReceiveDataLog("SendDataWorker RunWorkerAsync");

            // 若在正式環境，則自動啟動
            //ReceiveCameraDataWorker?.RunWorkerAsync();
            //mainWin?.WriteCameraLog("ReceiveCameraDataWorker RunWorkerAsync");

            //ReceiveMonitorWorker?.RunWorkerAsync();
            //mainWin?.WriteMachineLog("ReceiveMonitorWorker RunWorkerAsync");
        }

        public void CreateWorker()
        {
            // 常駐執行緒，負責統整資料
            SendDataWorker = new BackgroundWorker();
            SendDataWorker.WorkerSupportsCancellation = true;
            SendDataWorker.WorkerReportsProgress = true;

            SendDataWorker.DoWork += SendDataWorkerClass.DoWorkHandler;
            SendDataWorker.ProgressChanged += SendDataWorkerClass.ProgressChangedHandler;
            SendDataWorker.RunWorkerCompleted += SendDataWorkerClass.RunWorkerCompletedHandler;
        }

        // 停止 ReceiveCameraDataWorker
        public void StopReceiveCameraDataWorker()
        {
            ReceiveCameraWorkerClass.doWork = false;
        }
        // 停止 ReceiveMonitorWorker
        public void StopReceiveMonitorWorker()
        {
            ReceiveMonitorWorkerClass.doWork = false;
        }

        // 系統結束時，呼叫的方法，用來釋放資源
        public void StopMethod()
        {
            // 停止三個 Worker 與其資源
            SendDataWorker?.Dispose();
            ReceiveCameraDataWorker?.Dispose();
            ReceiveMonitorWorker?.Dispose();
        }

    }
}
