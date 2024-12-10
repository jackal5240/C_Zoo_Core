using ANB_SSZ.Module_UI;
using GAIA.MainObject;
using GAIA.Module_DataReceiver;
using LibVLCSharp.Shared;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Forms;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace GAIA.Module_UI
{
    public class ClassUI
    {
        // 架構物件
        MajorMethod? majorMethod;

        // 主視窗物件
        MainWindow? mainWin;

        // 導覽視窗
        public GuideWindow guideWindow = new();

        // 負責向 DT 收取資料的物件
        public BackgroundWorker? ReceiveDTDataWorker;

        // 攝影機的 RTSP Media 物件
        // public List<Media> camVideoMedia = new();
        // public List<MediaPlayer> camVideoMediaPlayer = new();

        // 本物件第一個被呼叫的程式，用來建構相關的物件或方法
        public void StartMethod(MainWindow mainWin, MajorMethod majorMethod)
        {
            // 紀錄 MajorMethod 物件，以便需要時可以呼叫
            this.majorMethod = majorMethod;

            // 紀錄主要視窗
            this.mainWin = mainWin;

            // 於 Guide 中，紀錄主視窗
            guideWindow.mainWin = mainWin;
            guideWindow.majorMethod = majorMethod;
            guideWindow.classUI = this;

            // 啟動 ReceiveDTDataWorker
            ReceiveDTDataWorker = new BackgroundWorker();
            ReceiveDTDataWorker.WorkerSupportsCancellation = true;
            ReceiveDTDataWorker.WorkerReportsProgress = true;

            ReceiveDTDataWorker.DoWork += ReceiveDTDataWorkerClass.DoWorkHandler;
            ReceiveDTDataWorker.ProgressChanged += ReceiveDTDataWorkerClass.ProgressChangedHandler;
            ReceiveDTDataWorker.RunWorkerCompleted += ReceiveDTDataWorkerClass.RunWorkerCompletedHandler;

            ReceiveDTDataWorkerClass.mainWin = mainWin;
            ReceiveDTDataWorkerClass.guideWindow = guideWindow;
            ReceiveDTDataWorkerClass.bgWorker = ReceiveDTDataWorker;
            ReceiveDTDataWorkerClass.majorMethod = majorMethod;
            ReceiveDTDataWorkerClass.classUI = this;

            // 啟動執行緒
            ReceiveDTDataWorker?.RunWorkerAsync();
        }

        // 啟動 ReceiveDTDataWorker
        public void StartReceiveDTDataWorker()
        {
            ReceiveDTDataWorkerClass.doWork = true;

            if (ReceiveDTDataWorker?.IsBusy == false)
            {
                ReceiveDTDataWorker?.RunWorkerAsync();

                mainWin?.WriteDTDataLog("啟動 向 AR 取得資料");
            }
        }

        // 停止 ReceiveDTDataWorker
        public void StopReceiveDTDataWorker()
        {
            ReceiveDTDataWorkerClass.doWork = false;
        }

        // 顯示 GuideWindow
        public void ShowGuideWindow()
        {
            ShowGuideWindowInSecondScreen();

            // guideWindow.Show();
        }

        // 於第二個視窗上顯示
        private void ShowGuideWindowInSecondScreen()
        {
            // 取得全部螢幕
            var screens = System.Windows.Forms.Screen.AllScreens;

            // 若有兩個以上，則播放到第二個
            if (screens.Length >= 2)
            {
                // 取得第二個螢幕的資訊
                var secondScreen = screens[0];

                // 第二個螢幕的邊界值
                System.Drawing.Rectangle secondBounds = secondScreen.Bounds;

                // 設定導覽視窗到第二個螢幕的邊界值
                guideWindow.Left = secondBounds.Left;
                guideWindow.Top = secondBounds.Top;

                guideWindow.Width = secondBounds.Width;
                guideWindow.Height = secondBounds.Height;

                // 要先 Show，才能去設定放大與邊框
                guideWindow.Show();

                guideWindow.WindowState = WindowState.Maximized;
                guideWindow.ResizeMode = ResizeMode.NoResize;

                // WindowStyle 要在設計模式下就設定，不然螢幕上方會出現白色的線條
                guideWindow.WindowStyle = WindowStyle.None;
            }
            else
            {
                guideWindow.Show();

                guideWindow.WindowStyle = WindowStyle.SingleBorderWindow;
                guideWindow.WindowState = WindowState.Normal;
                guideWindow.ResizeMode = ResizeMode.CanResize;
            }
        }

        // 關閉 GuideWindow
        public void HideGuideWindow()
        {
            guideWindow.Hide();
        }

        // 系統結束時，呼叫的方法，用來釋放資源
        public void StopMethod()
        {
            // 關閉視窗
            guideWindow.Close();
        }

    }
}
