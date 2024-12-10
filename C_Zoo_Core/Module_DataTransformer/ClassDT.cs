using ANB_UI.Tools;
using GAIA.MainObject;
using GAIA.Models;
using GAIA.Module_DataReceiver;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Media.Media3D;

namespace GAIA.Module_DataTransformer
{
    class ClassDT
    {
        // 架構物件
        MajorMethod? MajorMethod;

        // 主視窗物件
        MainWindow? mainWin;

        // 在此建立多執行序物件，包含主要的 Camera 與 透顯裝置物件，以及一個常駐執行緒，負責統整資料
        public BackgroundWorker ZooCoreWorker;

        // 本物件第一個被呼叫的程式，用來建構相關的物件或方法
        public void StartMethod(MainWindow mainWin, MajorMethod MajorMethod)
        {
            // 紀錄主要視窗
            this.mainWin = mainWin;

            // 紀錄 MajorMethod 物件，以便需要時可以呼叫
            this.MajorMethod = MajorMethod;

            // 1107 AR主控台 Mark
            // All View 顯示
            //ImageBrush imgb_all_view = new();
            //imgb_all_view.ImageSource = new BitmapImage(new Uri(".\\Image\\ZOO_ALL_View.png", UriKind.RelativeOrAbsolute));
            //mainWin.spnlAllView.Background = imgb_all_view;

            // 建立各執行緒與相關物件
            CreateWorker();

            // 設定 Worker 的各項屬性
            ZooCoreWorkerClass.mainWin = mainWin;
            ZooCoreWorkerClass.bgWorker = ZooCoreWorker;
            ZooCoreWorkerClass.majorMethod = MajorMethod;
            ZooCoreWorkerClass.classDT = this;

            // 開始啟動各執行緒
            //ZooCoreWorker?.RunWorkerAsync();
        }
        public void CreateWorker()
        {
            // 常駐執行緒，AR運算
            ZooCoreWorker = new BackgroundWorker();
            ZooCoreWorker.WorkerSupportsCancellation = true;
            ZooCoreWorker.WorkerReportsProgress = true;

            ZooCoreWorker.DoWork += ZooCoreWorkerClass.DoWorkHandler;
            ZooCoreWorker.ProgressChanged += ZooCoreWorkerClass.ProgressChangedHandler;
            ZooCoreWorker.RunWorkerCompleted += ZooCoreWorkerClass.RunWorkerCompletedHandler;

        }
        // 啟動 ZooCoreWorker
        public void StartZooCoreWorker()
        {
            ZooCoreWorkerClass.doWork = true;
            if (ZooCoreWorker?.IsBusy == false)
            {
                ZooCoreWorker?.RunWorkerAsync();

                mainWin.WriteMutiDataReceiveLog("AR Module MutiDataReceive RunWorkerAsync");
            }
        }

        // 停止 ZooCoreWorker
        public void StopZooCoreWorker()
        {
            ZooCoreWorkerClass.doWork = false;
            // 1107 AR主控台 Mark
            //mainWin.spnlAllView.Children.Clear();
        }
        // 系統結束時，呼叫的方法，用來釋放資源
        public void StopMethod()
        {
            // 停止三個 Worker 與其資源
            ZooCoreWorker?.Dispose();
        }

        public void RecordData()
        {
            ZooCoreWorkerClass.RecordData();
        }
    }
}
