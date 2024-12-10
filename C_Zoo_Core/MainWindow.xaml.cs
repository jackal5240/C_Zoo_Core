using GAIA.MainObject;
using GAIA.Models;
using GAIA.Module_DataReceiver;
using GAIA.Module_DataTransformer;
using GAIA.Module_UI;
using Newtonsoft.Json;
using System.IO;
using System.Windows;
using System.Windows.Input;


namespace GAIA
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        // 在此建立主要物件，與各層物件
        MajorMethod majorMethod = new();

        ClassDR DRObject = new();
        ClassDT DTObject = new();
        ClassUI UIObject = new();

        public MainWindow()
        {
            InitializeComponent();

            // 讀取 Settings.json 的內容
            GetSettingsJsonToObject();

            // 進行系統建置
            CallModule();
        }

        private void GetSettingsJsonToObject()
        {
            try
            {
                string jsonString = File.ReadAllText("Settings.json");
                majorMethod.settingsObject = JsonConvert.DeserializeObject<SettingsObject>(jsonString);

                // 顯示讀取到的資料
                WriteSettingsObjectLog(JsonConvert.SerializeObject(majorMethod.settingsObject));
            }
            catch (Exception ex)
            {
                WriteSettingsObjectLog($"讀取 Setting.Json 發生錯誤: {ex.Message}");
            }
        }

        private void CallModule()
        {
            // 呼叫 MajorMethod 物件
            majorMethod.StartMethod();

            // 呼叫 DataReceiver 層物件的進入點
            DRObject.StartMethod(this, majorMethod);

            // 呼叫 DataTransformer 層物件的進入點
            DTObject.StartMethod(this, majorMethod);

            // 呼叫 UI 層物件的進入點
            UIObject.StartMethod(this, majorMethod);

        }
        // 啟動與 AR_Computing 模組的串接
        private void btnStartARComputing_Click(object sender, RoutedEventArgs e)
        {
            DTObject.StartZooCoreWorker();
        }

        // 停止與 AR_Computing 模組的串接
        private void btnStopARComputing_Click(object sender, RoutedEventArgs e)
        {
            DTObject.StopZooCoreWorker();
        }

        // 讓螢幕觸控模組生效
        private void btnStartMonitor_Click(object sender, RoutedEventArgs e)
        {

        }

        // 讓螢幕觸控模組無效
        private void btnStopMonitor_Click(object sender, RoutedEventArgs e)
        {

        }

        // 顯示透顯畫面
        private void btnShowScreen_Click(object sender, RoutedEventArgs e)
        {
            UIObject.ShowGuideWindow();

            // 開啟視窗後，就跳到 Mode 0 去
            UIObject.guideWindow.SetScreenMode(ModeKind.Mode0);
        }

        // 隱藏透顯畫面
        private void btnHideScreen_Click(object sender, RoutedEventArgs e)
        {
            UIObject.HideGuideWindow();
        }

        // 記錄觸控的時間與 X, Y 值
        public void WriteMonitorLog(string message)
        {
            // 取得當下的時間
            string nowTimeString = DateTime.Now.ToString("yyyy/MM/dd tt hh:mm:ss.fff");

            // 如果超過 1000 行，就清除 ListBox
            if (lsbMonitor.Items.Count > 1000)
            {
                lsbMonitor.Items.Clear();
            }

            // 將 Log 加到 ListBox 中
            lsbMonitor.Items.Add("[" + nowTimeString + "] " + message);

            // 捲到最後一行
            // lsbMonitor.ScrollIntoView(lsbMonitor.Items[lsbMonitor.Items.Count - 1]);
            lsbMonitor.ScrollIntoView(lsbMonitor.Items[^1]);
        }

        // 視窗要關閉時
        private void winMain_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            // 關閉資源
            UIObject.StopMethod();
        }

        // 紀錄 ARComputing 的 Log
        public void WriteMutiDataReceiveLog(string message)
        {
            // 取得當下的時間
            string nowTimeString = DateTime.Now.ToString("yyyy/MM/dd tt hh:mm:ss.fff");

            // 如果超過 1000 行，就清除 ListBox
            if (this.mutiDataReceiveLog.Items.Count > 1000)
            {
                mutiDataReceiveLog.Items.Clear();
            }

            // 將 Log 加到 ListBox 中
            this.mutiDataReceiveLog.Items.Add("[" + nowTimeString + "] " + message);

            // 捲到最後一行
            this.mutiDataReceiveLog.ScrollIntoView(this.mutiDataReceiveLog.Items[this.mutiDataReceiveLog.Items.Count - 1]);
        }

        // 紀錄 ARComputing 的 Log
        public void WriteARComputingLog(string message)
        {
            // 取得當下的時間
            string nowTimeString = DateTime.Now.ToString("yyyy/MM/dd tt hh:mm:ss.fff");

            // 如果超過 1000 行，就清除 ListBox
            if (this.arComputingLog.Items.Count > 1000)
            {
                arComputingLog.Items.Clear();
            }

            // 將 Log 加到 ListBox 中
            this.arComputingLog.Items.Add("[" + nowTimeString + "] " + message);

            // 捲到最後一行
            this.arComputingLog.ScrollIntoView(this.arComputingLog.Items[this.arComputingLog.Items.Count - 1]);
        }

        // 紀錄 ReceiveData 整合成 Json 的 Log
        public void WriteReceiveDataLog(string message)
        {
            // 取得當下的時間
            string nowTimeString = DateTime.Now.ToString("yyyy/MM/dd tt hh:mm:ss.fff");

            // 如果超過 1000 行，就清除 ListBox
            if (this.lsbReceiveData.Items.Count > 1000)
            {
                lsbReceiveData.Items.Clear();
            }

            // 將 Log 加到 ListBox 中
            this.lsbReceiveData.Items.Add("[" + nowTimeString + "] " + message);

            // 捲到最後一行
            this.lsbReceiveData.ScrollIntoView(this.lsbReceiveData.Items[this.lsbReceiveData.Items.Count - 1]);
        }

        // 紀錄 SettingsObject 的 Log
        public void WriteSettingsObjectLog(string message)
        {
            // 取得當下的時間
            string nowTimeString = DateTime.Now.ToString("yyyy/MM/dd tt hh:mm:ss.fff");

            // 如果超過 1000 行，就清除 ListBox
            if (this.lsbSettingsObject.Items.Count > 1000)
            {
                lsbSettingsObject.Items.Clear();
            }

            // 將 Log 加到 ListBox 中
            this.lsbSettingsObject.Items.Add("[" + nowTimeString + "] " + message);

            // 捲到最後一行
            this.lsbSettingsObject.ScrollIntoView(this.lsbSettingsObject.Items[this.lsbSettingsObject.Items.Count - 1]);
        }

        // 紀錄 DT Data 的 Log
        public void WriteDTDataLog(string message)
        {
            // 取得當下的時間
            string nowTimeString = DateTime.Now.ToString("yyyy/MM/dd tt hh:mm:ss.fff");

            // 如果超過 1000 行，就清除 ListBox
            if (this.lsbDTData.Items.Count > 1000)
            {
                lsbDTData.Items.Clear();
            }

            // 將 Log 加到 ListBox 中
            this.lsbDTData.Items.Add("[" + nowTimeString + "] " + message);

            // 捲到最後一行
            this.lsbDTData.ScrollIntoView(this.lsbDTData.Items[this.lsbDTData.Items.Count - 1]);
        }

        // 按下 UI 頁中的按鈕
        private void btnShowMode1_Click(object sender, RoutedEventArgs e)
        {
            UIObject.guideWindow.SetScreenMode(ModeKind.Mode1);
        }

        private void btnShowMode15_A20_1_Click(object sender, RoutedEventArgs e)
        {
            UIObject.guideWindow.SetScreenMode(ModeKind.Mode15_A20_1);
        }

        private void btnShowMode2_A21_1_Click(object sender, RoutedEventArgs e)
        {
            UIObject.guideWindow.SetScreenMode(ModeKind.Mode2_A21_1);
        }

        private void btnShowMode3_A22_1_Click(object sender, RoutedEventArgs e)
        {
            UIObject.guideWindow.SetScreenMode(ModeKind.Mode3_A22_1);
        }

        private void btnShowMode15_A30_1_Click(object sender, RoutedEventArgs e)
        {
            UIObject.guideWindow.SetScreenMode(ModeKind.Mode15_A30_1);
        }

        private void btnShowMode2_A31_1_Click(object sender, RoutedEventArgs e)
        {
            UIObject.guideWindow.SetScreenMode(ModeKind.Mode2_A31_1);
        }

        private void btnShowMode3_A32_1_Click(object sender, RoutedEventArgs e)
        {
            UIObject.guideWindow.SetScreenMode(ModeKind.Mode3_A32_1);
        }

        private void btnShowMode15_A40_1_Click(object sender, RoutedEventArgs e)
        {
            UIObject.guideWindow.SetScreenMode(ModeKind.Mode15_A40_1);
        }

        private void btnShowMode2_A41_1_Click(object sender, RoutedEventArgs e)
        {
            UIObject.guideWindow.SetScreenMode(ModeKind.Mode2_A41_1);
        }

        private void btnShowMode3_A42_1_Click(object sender, RoutedEventArgs e)
        {
            UIObject.guideWindow.SetScreenMode(ModeKind.Mode3_A42_1);
        }

        private void btnShowMode15_B_10_Click(object sender, RoutedEventArgs e)
        {
            UIObject.guideWindow.SetScreenMode(ModeKind.Mode15_B10);
        }

        private void btnShowMode2_B_20_1_Click(object sender, RoutedEventArgs e)
        {
            UIObject.guideWindow.SetScreenMode(ModeKind.Mode2_B20_1);
        }

        private void btnShowMode2_B_30_1_Click(object sender, RoutedEventArgs e)
        {
            UIObject.guideWindow.SetScreenMode(ModeKind.Mode2_B30_1);
        }

        private void btnShowModeMap_Click(object sender, RoutedEventArgs e)
        {
            UIObject.guideWindow.SetScreenMode(ModeKind.Mode2_MAP_10);
        }

        private void btnShowModeLive_Click(object sender, RoutedEventArgs e)
        {
            UIObject.guideWindow.SetScreenMode(ModeKind.Mode2_LIVE_10);
        }

        private void TextBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            e.Handled = new System.Text.RegularExpressions.Regex("[^0-9]+").IsMatch(e.Text);
        }

        private void btnReadSettingsJson_Click(object sender, RoutedEventArgs e)
        {
            GetSettingsJsonToObject();
        }

        private void cmbShowARBox_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            if (cmbShowARBox.SelectedIndex == 0)
            {
                majorMethod.showARData = false;
            }
            else
            {
                majorMethod.showARData = true;
            }
        }

        private void btnStartDTDataReceive_Click(object sender, RoutedEventArgs e)
        {
            UIObject.StartReceiveDTDataWorker();
        }

        private void btnStopDTDataReceive_Click(object sender, RoutedEventArgs e)
        {
            UIObject.StopReceiveDTDataWorker();
        }

        private void btnCam0_Click(object sender, RoutedEventArgs e)
        {
            UIObject.guideWindow.playRTSPStream(0);
        }

        private void btnCam1_Click(object sender, RoutedEventArgs e)
        {
            UIObject.guideWindow.playRTSPStream(1);
        }

        private void btnCam2_Click(object sender, RoutedEventArgs e)
        {
            UIObject.guideWindow.playRTSPStream(2);
        }

        private void btnCam3_Click(object sender, RoutedEventArgs e)
        {
            UIObject.guideWindow.playRTSPStream(3);
        }

        private void btnCam4_Click(object sender, RoutedEventArgs e)
        {
            UIObject.guideWindow.playRTSPStream(4);
        }

        private void btnCam5_Click(object sender, RoutedEventArgs e)
        {
            UIObject.guideWindow.playRTSPStream(5);
        }

        private void btnCam6_Click(object sender, RoutedEventArgs e)
        {
            UIObject.guideWindow.playRTSPStream(6);
        }

        private void btnCam7_Click(object sender, RoutedEventArgs e)
        {
            UIObject.guideWindow.playRTSPStream(7);
        }

        private void btnCam8_Click(object sender, RoutedEventArgs e)
        {
            UIObject.guideWindow.playRTSPStream(8);
        }

        private void btnCamLab_Click(object sender, RoutedEventArgs e)
        {
            // UIObject.guideWindow.playRTSPStream(100);
        }
        private void btnLogRecord_Click(object sender, RoutedEventArgs e)
        {
            DTObject.RecordData();
        }

        private void btnCamReconnect_Click(object sender, RoutedEventArgs e)
        {
            // 相機重新連接
            UIObject.guideWindow.ReConnectVideoCam();
        }

        private void btnShowMode15_A50_1_Click(object sender, RoutedEventArgs e)
        {
            // 新增"坐"姿態
            UIObject.guideWindow.SetScreenMode(ModeKind.Mode15_A50_1);
        }

        private void btnShowMode2_A51_1_Click(object sender, RoutedEventArgs e)
        {
            // 新增"坐"姿態
            UIObject.guideWindow.SetScreenMode(ModeKind.Mode2_A51_1);
        }

        private void btnShowMode3_A52_1_Click(object sender, RoutedEventArgs e)
        {
            // 新增"坐"姿態
            UIObject.guideWindow.SetScreenMode(ModeKind.Mode3_A52_1);
        }

        private void btnCamConnect_Click(object sender, RoutedEventArgs e)
        {
            // 啟動即時影像
            UIObject.guideWindow.ConnectVideoCam();
        }

        private void btnShowModeSP_01_Click(object sender, RoutedEventArgs e)
        {
            UIObject.guideWindow.SetScreenMode(ModeKind.ModeSP_01);
        }

        private void btnShowModeSP_02_Click(object sender, RoutedEventArgs e)
        {
            UIObject.guideWindow.SetScreenMode(ModeKind.ModeSP_02);
        }

        private void btnShowModeSP_03_Click(object sender, RoutedEventArgs e)
        {
            UIObject.guideWindow.SetScreenMode(ModeKind.ModeSP_03);
        }

        private void btnShowModeSP_04_Click(object sender, RoutedEventArgs e)
        {
            UIObject.guideWindow.SetScreenMode(ModeKind.ModeSP_04);
        }

        private void btnShowModeSP_05_Click(object sender, RoutedEventArgs e)
        {
            UIObject.guideWindow.SetScreenMode(ModeKind.ModeSP_05);
        }

        private void btnShowModeSP_06_Click(object sender, RoutedEventArgs e)
        {
            UIObject.guideWindow.SetScreenMode(ModeKind.ModeSP_06);
        }

        private void btnShowModeSP_07_Click(object sender, RoutedEventArgs e)
        {
            UIObject.guideWindow.SetScreenMode(ModeKind.ModeSP_07);
        }

        private void btnShowModeSP_09_Click(object sender, RoutedEventArgs e)
        {
            UIObject.guideWindow.SetScreenMode(ModeKind.ModeSP_09);
        }

        private void btnShowModeSP_10_Click(object sender, RoutedEventArgs e)
        {
            UIObject.guideWindow.SetScreenMode(ModeKind.ModeSP_10);
        }

        private void btnShowModeSP_11_Click(object sender, RoutedEventArgs e)
        {
            UIObject.guideWindow.SetScreenMode(ModeKind.ModeSP_11);
        }

        private void btnShowModeSP_12_Click(object sender, RoutedEventArgs e)
        {
            UIObject.guideWindow.SetScreenMode(ModeKind.ModeSP_12);
        }
    }
}