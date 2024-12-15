using ANB_SSZ.Models;
using ANB_UI.Tools;
using GAIA;
using GAIA.MainObject;
using GAIA.Models;
using GAIA.Module_UI;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Buffers;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using Point = System.Windows.Point;

namespace ANB_SSZ.Module_UI
{
    /// <summary>
    /// GuideWindow.xaml 的互動邏輯
    /// </summary>
    public partial class GuideWindow : Window
    {
        // 主視窗物件
        public MainWindow? mainWin;

        // MajorMethod 物件
        public MajorMethod? majorMethod;

        // UIClass 物件
        public ClassUI? classUI;

        // 從 DT 而來的物件
        public BearObject? bearObject;

        // 目前的畫面模式與次模式
        public ModeKind currentMode = ModeKind.None;

        // 目前的語言模式，初始值為 中文
        public LanguageMode languageModel = LanguageMode.Chinese;

        // RTSP 相關設定
        private List<MediaPlayer> _mediaPlayers;

        // 用來計算即時影像的 FPS
        private DateTime LastFpsUpdate_Cam0 = DateTime.Now;
        private DateTime LastFpsUpdate_Cam1 = DateTime.Now;
        private DateTime LastFpsUpdate_Cam2 = DateTime.Now;
        private DateTime LastFpsUpdate_Cam3 = DateTime.Now;
        private DateTime LastFpsUpdate_Cam4 = DateTime.Now;
        private DateTime LastFpsUpdate_Cam5 = DateTime.Now;
        private DateTime LastFpsUpdate_Cam6 = DateTime.Now;
        private DateTime LastFpsUpdate_Cam7 = DateTime.Now;
        private DateTime LastFpsUpdate_Cam8 = DateTime.Now;

        private int FrameCount_Cam0 = 0;
        private int FrameCount_Cam1 = 0;
        private int FrameCount_Cam2 = 0;
        private int FrameCount_Cam3 = 0;
        private int FrameCount_Cam4 = 0;
        private int FrameCount_Cam5 = 0;
        private int FrameCount_Cam6 = 0;
        private int FrameCount_Cam7 = 0;
        private int FrameCount_Cam8 = 0;

        // 目前即時影像的畫面編號
        public int liveCamIndex = 0;

        // 即時影像重連的相關設定
        private CancellationTokenSource reconnectCts;
        private readonly int maxReconnectAttempts = 5;
        private readonly int reconnectDelayMs = 3000;

        // 盲區一很特別，只是顯示文字，不改變原來的 Mode
        public bool IsModeSP_01 = false;

        private int _currentStreamIndex = -1;
        private string _networkInterfaceIp = "192.168.2.1"; // 指定使用的網卡 IP

        public GuideWindow()
        {
            InitializeComponent();

            // 設定媒體撥放器可受程式控制
            mdeMoviePlayer.LoadedBehavior = MediaState.Manual;
            mdeMoviePlayer_B20.LoadedBehavior = MediaState.Manual;
            mdeMoviePlayer_B30.LoadedBehavior = MediaState.Manual;
        }

        // 視窗載入時
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            // 建立與啟動即時攝影機的連結
            // InitCamAsync();

            // 先關閉所有的 StackPanel
            //HideAndResetAllPanel();

            // 啟動即時影像
            //ConnectVideoCam();
        }

        // 用中介的方式來非同步啟動攝影機
        private async Task InitCamAsync()
        {
            // var result = await ConnectVideoCam();
        }

        // 在此進行語言切換
        public void SetLanguageMode(LanguageMode lanModel)
        {
            if (lanModel == LanguageMode.Chinese)
            {
                languageModel = LanguageMode.Englisg;
            }
            else
            {
                languageModel = LanguageMode.Chinese;
            }
        }

        // 作為攫取 AR 資料後，被通知，進行畫面繪製的主要程式
        public void RepaintScreen(string DTString)
        {
            // 序列化到 bearObject 中
            bearObject = JsonConvert.DeserializeObject<BearObject>(DTString);

            // 將 DT 的字串傳到畫面 Log 中
            // mainWin?.WriteDTDataLog(DTString);

            // 連續不斷的計算熊在地圖中的位置，並放到存放區
            calcMapBearPosition();

            // 進行畫面繪製，要特別注意畫面閃動的問題
            changeModeByARData();

            // 在此進行 AR 框與花絮的繪製
            drawARData(majorMethod.showARData);
        }

        // 連續不斷的計算熊在地圖中的位置，並放到 Value 存放區
        public void calcMapBearPosition()
        {
            // 地圖左上 X = 210, Y = 58
            // 比例 X = 60 / 0.1, Y = 43 / 0.1
            // 廊道固定為 X=225, Y=350 當 Cam9 大於閥值時，就直接顯示在廊道


            //const int OriginX = 210;

            // 2024/12/05 更改為 205
            const int OriginX = 205;

            //const int OriginY = 58;
            // 2024/12/05 更改為 65
            const int OriginY = 65;
 
            const double MapRatioX = 60.0;
            const double MapRatioY = 45.0;

            int bearXPos = -1;
            int bearYPos = -1;

            // 檢查熊有沒有在廊道
            dynamic cam9 = bearObject.DataArray[8 + 3];
            double cam9Conf = 0.0;
            if (cam9 != null)
            {
                cam9Conf = cam9[0];

                // Cam9 有抓到熊
                if (cam9Conf >= majorMethod.confThresholdList[8])
                {
                    // 儲放定值
                    majorMethod.MapBearIconX = 215;
                    majorMethod.MapBearIconY = 350;

                    //if (spnlMapIcon.Visibility == Visibility.Visible)
                    //{
                    //    // 利用動畫移動
                    //    MoveImageByAnimation(spnlMapIcon, majorMethod.MapBearIconX, majorMethod.MapBearIconY);
                    //}
                    //else
                    //{
                    //    // 直接指定位置
                    //    Canvas.SetLeft(spnlMapIcon, majorMethod.MapBearIconX);
                    //    Canvas.SetTop(spnlMapIcon, majorMethod.MapBearIconY);
                    //}

                    return;
                }
            }

            // 沒有傳來 Ratio 時，也不用計算
            if ((bearObject.MapXRatio == "-1") && (bearObject.MapYRatio == "-1"))
            {
                return;
            }

            // 水池特定
            dynamic ratio_x = bearObject.DataArray[0];
            dynamic ratio_y = bearObject.DataArray[1];

            if ((ratio_x >= 0.4) && (ratio_x <= 0.5) &&
                (ratio_y >= 0.47) && (ratio_y <= 0.56))
            {
                majorMethod.MapBearIconX = 440;
                majorMethod.MapBearIconY = 280;

                //if (spnlMapIcon.Visibility == Visibility.Visible)
                //{
                //    // 利用動畫移動
                //    MoveImageByAnimation(spnlMapIcon, majorMethod.MapBearIconX, majorMethod.MapBearIconY);
                //}
                //else
                //{
                //    // 直接指定位置
                //    Canvas.SetLeft(spnlMapIcon, majorMethod.MapBearIconX);
                //    Canvas.SetTop(spnlMapIcon, majorMethod.MapBearIconY);
                //}

                return;
            }

            // 計算熊的位置
            double bearXRatio = double.Parse(bearObject.MapXRatio);
            double bearYRatio = double.Parse(bearObject.MapYRatio);

            majorMethod.MapBearIconX = (int)(bearXRatio * MapRatioX * 10) + OriginX - 35;  // -35 為把熊置中
            majorMethod.MapBearIconY = (int)(bearYRatio * MapRatioY * 10) + OriginY - 45;  // -45 為把熊置中

            //if (spnlMapIcon.Visibility == Visibility.Visible)
            //{
            //    // 利用動畫移動
            //    MoveImageByAnimation(spnlMapIcon, majorMethod.MapBearIconX, majorMethod.MapBearIconY);
            //}
            //else
            //{
            //    // 直接指定位置
            //    Canvas.SetLeft(spnlMapIcon, majorMethod.MapBearIconX);
            //    Canvas.SetTop(spnlMapIcon, majorMethod.MapBearIconY);
            //}
        }

        public void changeModeByARData()
        {
            // Screen Left Top
            int ScreenLeft = 0;
            int ScreenTop = 0;

            // Bear X, Y
            int BearLeft = -1;
            int BearTop = -1;
            int BearRight = -1;
            int BearBottom = -1;

            // 顯示 screen_left_top 資料
            ScreenLeft = bearObject.ScreenLeftTop[0];
            ScreenTop = bearObject.ScreenLeftTop[1];

            // 顯示 bear_x_y 資料
            BearLeft = bearObject.BearXY[0] - ScreenLeft;
            BearTop = bearObject.BearXY[1] - ScreenTop;
            BearRight = bearObject.BearXY[2] - ScreenLeft;
            BearBottom = bearObject.BearXY[3] - ScreenTop;

            int BearCenter = (int)((BearLeft + BearRight) / 2);

            // Bear 的 Pose
            string bearMode = bearObject.BearMode;

            // ScreenMode
            string screenMode = bearObject.ScreenMode;

            // 在此處理盲區的特別模式，唯有在花絮模式下才做盲區切換
            if (((bearObject.ScreenMode == "0") || (bearObject.ScreenMode == "-1")) &&
                (currentMode == ModeKind.Mode15_B10))
            {
                doSpecialPositionHandler();
            }

            // 依照目前的 ScreenMode 來分別處理
            switch (currentMode)
            {
                // 空模式
                case ModeKind.None:
                    break;

                // Mode0
                case ModeKind.Mode0:

                    // 當在首頁時，利用熊的 BBOX 中心點，切換 ScreenMode

                    // 當熊在兩側時，螢幕切換到 ZOO_A_10 的狀態
                    if ((BearCenter <= 240) && (BearCenter >= 0) || ((BearCenter >= 720) && (BearCenter <= 960)))
                    {
                        SetScreenMode(ModeKind.Mode1);

                        return;
                    }

                    // 當熊在螢幕外時，就切換到黑熊花絮畫面
                    if (((BearLeft + BearRight) / 2 < 0) || ((BearLeft + BearRight) / 2 > 960))
                    {
                        SetScreenMode(ModeKind.Mode15_B10);

                        return;
                    }

                    // 當熊在螢幕中心時，依照姿勢，切換到不同的 站立，趴臥，四足
                    if (((BearLeft + BearRight) / 2 >= 240) && ((BearLeft + BearRight) / 2 <= 720))
                    {
                        switch (bearMode)
                        {
                            // 站立系列
                            case "stand":
                                SetScreenMode(ModeKind.Mode15_A20_1);
                                break;

                            // 趴臥系列
                            case "lie":
                                SetScreenMode(ModeKind.Mode15_A30_1);
                                break;

                            // 四足系列
                            case "walk":
                                SetScreenMode(ModeKind.Mode15_A40_1);
                                break;

                            // 坐 系列
                            case "sit":
                                SetScreenMode(ModeKind.Mode15_A50_1);
                                break;
                        }
                        return;
                    }
                    break;

                // 轉圈圈頁
                case ModeKind.Mode1:

                    // 情況一：當熊在螢幕外時，就切換到黑熊花絮畫面
                    if (((BearLeft + BearRight) / 2 < 0) || ((BearLeft + BearRight) / 2 > 960))
                    {
                        SetScreenMode(ModeKind.Mode15_B10);

                        return;
                    }

                    // 當熊在螢幕中心時，依照姿勢，切換到不同的 站立，趴臥，四足
                    if (((BearLeft + BearRight) / 2 >= 240) && ((BearLeft + BearRight) / 2 <= 720))
                    {
                        switch (bearMode)
                        {
                            // 站立系列
                            case "stand":
                                SetScreenMode(ModeKind.Mode15_A20_1);
                                break;

                            // 趴臥系列
                            case "lie":
                                SetScreenMode(ModeKind.Mode15_A30_1);
                                break;

                            // 四足系列
                            case "walk":
                                SetScreenMode(ModeKind.Mode15_A40_1);
                                break;

                            // 坐 系列
                            case "sit":
                                SetScreenMode(ModeKind.Mode15_A50_1);
                                break;
                        }
                        return;
                    }
                    break;

                // 站立系列
                case ModeKind.Mode15_A20_1:

                    // 當熊在兩側時，螢幕切換到 ZOO_A_10 的狀態
                    if (((BearLeft + BearRight) / 2 <= 240) || ((BearLeft + BearRight) / 2 >= 720))
                    {
                        SetScreenMode(ModeKind.Mode1);

                        return;
                    }

                    // 在此顯示 AR 框
                    ImageBrush imgb_ar_a22 = new();
                    imgb_ar_a22.ImageSource = new BitmapImage(new Uri(".\\Image\\ZOO_A_22_1_AR.png", UriKind.RelativeOrAbsolute));
                    spnlMode3A22_A_AR.Background = imgb_ar_a22;

                    // 設定左上與寬高
                    Canvas.SetLeft(spnlMode3A22_A_AR, BearLeft);
                    Canvas.SetTop(spnlMode3A22_A_AR, BearTop);
                    spnlMode3A22_A_AR.Width = BearRight - BearLeft;
                    spnlMode3A22_A_AR.Height = BearBottom - BearTop;

                    spnlMode3A22_A_AR.Visibility = Visibility.Visible;

                    break;

                case ModeKind.Mode2_A21_1:
                    break;

                case ModeKind.Mode3_A22_1:

                    //// 在此顯示 AR 框
                    //ImageBrush imgb_ar_a22 = new();
                    //imgb_ar_a22.ImageSource = new BitmapImage(new Uri(".\\Image\\ZOO_A_22_1_AR.png", UriKind.RelativeOrAbsolute));
                    //spnlMode3A22_A_AR.Background = imgb_ar_a22;

                    //Canvas.SetLeft(spnlMode3A22_A_AR, BearLeft);
                    //Canvas.SetTop(spnlMode3A22_A_AR, BearTop);
                    //Canvas.SetRight(spnlMode3A22_A_AR, BearRight);
                    //Canvas.SetBottom(spnlMode3A22_A_AR, BearBottom);

                    break;

                // 趴臥系列
                case ModeKind.Mode15_A30_1:

                    // 當熊在兩側時，螢幕切換到 ZOO_A_10 的狀態
                    if (((BearLeft + BearRight) / 2 <= 240) || ((BearLeft + BearRight) / 2 >= 720))
                    {
                        SetScreenMode(ModeKind.Mode0);

                        return;
                    }

                    // 在此顯示 AR 框
                    ImageBrush imgb_ar_a32 = new();
                    imgb_ar_a32.ImageSource = new BitmapImage(new Uri(".\\Image\\ZOO_A_22_1_AR.png", UriKind.RelativeOrAbsolute));
                    spnlMode3A32_A_AR.Background = imgb_ar_a32;

                    // 設定左上與寬高
                    Canvas.SetLeft(spnlMode3A32_A_AR, BearLeft);
                    Canvas.SetTop(spnlMode3A32_A_AR, BearTop);
                    spnlMode3A32_A_AR.Width = BearRight - BearLeft;
                    spnlMode3A32_A_AR.Height = BearBottom - BearTop;

                    spnlMode3A32_A_AR.Visibility = Visibility.Visible;

                    break;

                case ModeKind.Mode2_A31_1:
                    break;

                case ModeKind.Mode3_A32_1:

                    //// 在此顯示 AR 框
                    //ImageBrush imgb_ar_a32 = new();
                    //imgb_ar_a32.ImageSource = new BitmapImage(new Uri(".\\Image\\ZOO_A_22_1_AR.png", UriKind.RelativeOrAbsolute));
                    //spnlMode3A32_A_AR.Background = imgb_ar_a32;

                    //Canvas.SetLeft(spnlMode3A32_A_AR, BearLeft);
                    //Canvas.SetTop(spnlMode3A32_A_AR, BearTop);
                    //Canvas.SetRight(spnlMode3A32_A_AR, BearRight);
                    //Canvas.SetBottom(spnlMode3A32_A_AR, BearBottom);

                    break;

                // 四足系列
                case ModeKind.Mode15_A40_1:

                    // 當熊在兩側時，螢幕切換到 ZOO_A_10 的狀態
                    if (((BearLeft + BearRight) / 2 <= 240) || ((BearLeft + BearRight) / 2 >= 720))
                    {
                        SetScreenMode(ModeKind.Mode0);

                        return;
                    }

                    // 在此顯示 AR 框
                    ImageBrush imgb_ar_a42 = new();
                    imgb_ar_a42.ImageSource = new BitmapImage(new Uri(".\\Image\\ZOO_A_22_1_AR.png", UriKind.RelativeOrAbsolute));
                    spnlMode3A42_A_AR.Background = imgb_ar_a42;

                    // 設定左上與寬高
                    Canvas.SetLeft(spnlMode3A42_A_AR, BearLeft);
                    Canvas.SetTop(spnlMode3A42_A_AR, BearTop);
                    spnlMode3A42_A_AR.Width = BearRight - BearLeft;
                    spnlMode3A42_A_AR.Height = BearBottom - BearTop;

                    spnlMode3A42_A_AR.Visibility = Visibility.Visible;

                    break;

                case ModeKind.Mode2_A41_1:
                    break;

                case ModeKind.Mode3_A42_1:

                    //// 在此顯示 AR 框
                    //ImageBrush imgb_ar_a42 = new();
                    //imgb_ar_a42.ImageSource = new BitmapImage(new Uri(".\\Image\\ZOO_A_22_1_AR.png", UriKind.RelativeOrAbsolute));
                    //spnlMode3A42_A_AR.Background = imgb_ar_a42;

                    //Canvas.SetLeft(spnlMode3A42_A_AR, BearLeft);
                    //Canvas.SetTop(spnlMode3A42_A_AR, BearTop);
                    //Canvas.SetRight(spnlMode3A42_A_AR, BearRight);
                    //Canvas.SetBottom(spnlMode3A42_A_AR, BearBottom);

                    break;

                // 坐 系列
                case ModeKind.Mode15_A50_1:

                    // 當熊在兩側時，螢幕切換到 ZOO_A_10 的狀態
                    if (((BearLeft + BearRight) / 2 <= 240) || ((BearLeft + BearRight) / 2 >= 720))
                    {
                        SetScreenMode(ModeKind.Mode0);

                        return;
                    }

                    // 在此顯示 AR 框
                    ImageBrush imgb_ar_a52 = new();
                    imgb_ar_a52.ImageSource = new BitmapImage(new Uri(".\\Image\\ZOO_A_22_1_AR.png", UriKind.RelativeOrAbsolute));
                    spnlMode3A52_A_AR.Background = imgb_ar_a52;

                    // 設定左上與寬高
                    Canvas.SetLeft(spnlMode3A52_A_AR, BearLeft);
                    Canvas.SetTop(spnlMode3A52_A_AR, BearTop);
                    spnlMode3A52_A_AR.Width = BearRight - BearLeft;
                    spnlMode3A52_A_AR.Height = BearBottom - BearTop;

                    spnlMode3A52_A_AR.Visibility = Visibility.Visible;

                    break;

                case ModeKind.Mode2_A51_1:
                    break;

                case ModeKind.Mode3_A52_1:

                    //// 在此顯示 AR 框
                    //ImageBrush imgb_ar_a52 = new();
                    //imgb_ar_a52.ImageSource = new BitmapImage(new Uri(".\\Image\\ZOO_A_22_1_AR.png", UriKind.RelativeOrAbsolute));
                    //spnlMode3A52_A_AR.Background = imgb_ar_a52;

                    //Canvas.SetLeft(spnlMode3A52_A_AR, BearLeft);
                    //Canvas.SetTop(spnlMode3A52_A_AR, BearTop);
                    //Canvas.SetRight(spnlMode3A52_A_AR, BearRight);
                    //Canvas.SetBottom(spnlMode3A52_A_AR, BearBottom);

                    break;


                // 花絮部分
                case ModeKind.Mode15_B10:

                    // 當熊在兩側時，螢幕切換到 ZOO_A_10 的狀態
                    if ((BearCenter <= 240) && (BearCenter >=0) || ((BearCenter >= 720) && (BearCenter <= 960)))
                    {
                        SetScreenMode(ModeKind.Mode1);

                        return;
                    }

                    // 當熊在螢幕中心時，依照姿勢，切換到不同的 站立，趴臥，四足
                    if (((BearLeft + BearRight) / 2 >= 240) && ((BearLeft + BearRight) / 2 <= 720))
                    {
                        switch (bearMode)
                        {
                            // 站立系列
                            case "stand":
                                SetScreenMode(ModeKind.Mode15_A20_1);
                                break;

                            // 趴臥系列
                            case "lie":
                                SetScreenMode(ModeKind.Mode15_A30_1);
                                break;

                            // 四足系列
                            case "walk":
                                SetScreenMode(ModeKind.Mode15_A40_1);
                                break;
                            
                            // 坐 系列
                            case "sit":
                                SetScreenMode(ModeKind.Mode15_A50_1);
                                break;
                        }
                        return;
                    }


                    drawMode15B10Icon();

                    break;

                case ModeKind.Mode2_B20_1:
                    break;

                case ModeKind.Mode2_B30_1:
                    break;

                // 小地圖
                case ModeKind.Mode2_MAP_10:

                    drawMapIcon();

                    break;

                // Live 影像
                case ModeKind.Mode2_LIVE_10:

                    // 顯示即時影像中，按鈕的圖式
                    // clearLiveCamButtonIcon();

                    drawLiveCamButton();

                    break;

                // 盲區系列，做復原
                case ModeKind.ModeSP_01:

                    // 熊在廊道，或是 ScreenMode == 1 時，回到 Mode0 取消盲區
                    if ((BearInCamera9() == true) || (bearObject.ScreenMode == "1"))
                        SetScreenMode(ModeKind.Mode0);

                    break;

                case ModeKind.ModeSP_02:

                    // 熊在廊道，或是 ScreenMode == 1 時，回到 Mode0 取消盲區
                    if ((BearInCamera9() == true) || (bearObject.ScreenMode == "1"))
                        SetScreenMode(ModeKind.Mode0);

                    break;

                case ModeKind.ModeSP_03:

                    // 熊在廊道，或是 ScreenMode == 1 時，回到 Mode0 取消盲區
                    if ((BearInCamera9() == true) || (bearObject.ScreenMode == "1"))
                        SetScreenMode(ModeKind.Mode0);

                    break;

                case ModeKind.ModeSP_04:

                    // 熊在廊道，或是 ScreenMode == 1 時，回到 Mode0 取消盲區
                    if ((BearInCamera9() == true) || (bearObject.ScreenMode == "1"))
                        SetScreenMode(ModeKind.Mode0);

                    break;

                case ModeKind.ModeSP_05:

                    // 熊在廊道，或是 ScreenMode == 1 時，回到 Mode0 取消盲區
                    if ((BearInCamera9() == true) || (bearObject.ScreenMode == "1"))
                        SetScreenMode(ModeKind.Mode0);

                    break;

                case ModeKind.ModeSP_06:

                    // 熊在廊道，或是 ScreenMode == 1 時，回到 Mode0 取消盲區
                    if ((BearInCamera9() == true) || (bearObject.ScreenMode == "1"))
                        SetScreenMode(ModeKind.Mode0);

                    break;

                case ModeKind.ModeSP_07:

                    // 熊在廊道，或是 ScreenMode == 1 時，回到 Mode0 取消盲區
                    if ((BearInCamera9() == true) || (bearObject.ScreenMode == "1"))
                        SetScreenMode(ModeKind.Mode0);

                    break;

                case ModeKind.ModeSP_09:

                    // 熊在廊道，或是 ScreenMode == 1 時，回到 Mode0 取消盲區
                    if ((BearInCamera9() == true) || (bearObject.ScreenMode == "1"))
                        SetScreenMode(ModeKind.Mode0);

                    break;

                case ModeKind.ModeSP_10:

                    // 熊在廊道，或是 ScreenMode == 1 時，回到 Mode0 取消盲區
                    if ((BearInCamera9() == true) || (bearObject.ScreenMode == "1"))
                        SetScreenMode(ModeKind.Mode0);

                    break;

                case ModeKind.ModeSP_11:

                    // 熊在廊道，或是 ScreenMode == 1 時，回到 Mode0 取消盲區
                    if ((BearInCamera9() == true) || (bearObject.ScreenMode == "1"))
                        SetScreenMode(ModeKind.Mode0);

                    break;

                case ModeKind.ModeSP_12:

                    // 熊在廊道，或是 ScreenMode == 1 時，回到 Mode0 取消盲區
                    if ((BearInCamera9() == true) || (bearObject.ScreenMode == "1"))
                        SetScreenMode(ModeKind.Mode0);

                    break;
            }
        }

        // 單獨判斷熊是否在廊道
        public bool BearInCamera9()
        {
            dynamic cameraData = bearObject.DataArray[11];

            if ((cameraData != null) && (cameraData[0] >= majorMethod.confThresholdList[8]))
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        // 盲區處理程式
        public void doSpecialPositionHandler()
        {
            TextLog.WriteLog("進入盲區處理模式");

            // No1
            CheckModeSP_01();

            // No2
            CheckModeSP_02();

            // No3
            CheckModeSP_03();

            // No4
            CheckModeSP_04();

            // No5
            CheckModeSP_05();

            // No6
            CheckModeSP_06();

            // No7
            CheckModeSP_07();

            // No9
            CheckModeSP_09();

            // No10
            CheckModeSP_10();

            // No11
            CheckModeSP_11();

            // No12
            CheckModeSP_12();
        }

        // 盲區判斷程式 SP01
        public void CheckModeSP_01()
        {
            // 九號相機有熊時，就啟動這邊的處理
            dynamic cam9 = bearObject.DataArray[8 + 3];
            double cam9Conf = 0.0;
            if (cam9 != null)
            {
                cam9Conf = cam9[0];
            }

            // Cam9 有抓到熊
            if (cam9Conf >= majorMethod.confThresholdList[8])
            {
                IsModeSP_01 = true;
            }
            else
            {
                IsModeSP_01 = false;
            }
        }

        // 盲區判斷程式 SP02
        public void CheckModeSP_02()
        {
            // 若目前不是在花絮模式，就不用偵測與跳畫面
            if (currentMode != ModeKind.Mode15_B10)
                return;

            // 在棲架與展台間的走廊
            // 7 號攝影機的信心值，超過特定值(個別參數設定，目前設定 0.6)，
            // 與 7 號攝影機的 BBox 值(中心點)，在 X(0.8~1) 與 Y(0.3~1) 之間(如下圖紅色區域)

            // 取得 7 號相機的 BBox 中心點
            List<double> Cam7Center = GetCamCordCenter(6);

            // 如果熊中心點在 X(0.8~1) 與 Y(0.3~1) 間，就啟動 No2 盲區
            if ((Cam7Center[0] >= 0.8) && (Cam7Center[0] <= 1.0) && (Cam7Center[1] >= 0.3) && (Cam7Center[1] <= 1.0))
            {
                // 顯示盲區 2
                SetScreenMode(ModeKind.ModeSP_02);

                // 做一下 Log 紀錄
                TextLog.WriteLog($"盲區 2 啟動: XCenter={Cam7Center[0]}, YCenter={Cam7Center[0]}");
            }
        }

        // 盲區判斷程式 SP03
        public void CheckModeSP_03()
        {
            // 在棲架更高層(爬樹)
            // 對 1 / 7 / 8 相機的記憶功能，描述和圖形如下
            // (記憶功能) 當熊原來在黃色區域，消失一定秒數(個別變數設定，先設定 5 秒)，其他區域也沒有出現時，判斷已經爬到樹上


        }

        // 盲區判斷程式 SP04
        public void CheckModeSP_04()
        {
            // 若目前不是在花絮模式，就不用偵測與跳畫面
            if (currentMode != ModeKind.Mode15_B10)
                return;

            // 右側玻璃靠近廊道的走廊
            // 3 號攝影機的信心值，超過特定值(個別參數設定，目前設定 0.6)。
            // BBox 值，在 X(0.2~0.4) 與 Y(0~0.1) 之間(如下圖紅色區域)。 
            // 或 4 號攝影機的信心值，超過特定值(個別參數設定，目前設定 0.6)。
            // Bbox 值，在 X(0~0.2) 與 Y(0~0.1) 之間(如下圖紅色區域)。

            // 取得 3 號相機的 BBox 中心點
            List<double> Cam3Center = GetCamCordCenter(2);

            // 如果熊中心點在 X(0.2~0.4) 與 Y(0~0.1) 間，就啟動 No4 盲區
            if ((Cam3Center[0] >= 0.2) && (Cam3Center[0] <= 0.4) && (Cam3Center[1] >= 0.0) && (Cam3Center[1] <= 0.1))
            {
                // 顯示盲區 4
                SetScreenMode(ModeKind.ModeSP_04);

                // 做一下 Log 紀錄
                TextLog.WriteLog($"盲區 4 啟動(3 號相機): XCenter={Cam3Center[0]}, YCenter={Cam3Center[1]}");
                
                return;
            }

            // 取得 4 號相機的 BBox 中心點
            List<double> Cam4Center = GetCamCordCenter(3);

            // 如果熊中心點在 X(0~0.2) 與 Y(0~0.1) 間，就啟動 No4 盲區(By Cam4)
            if ((Cam4Center[0] >= 0.0) && (Cam4Center[0] <= 0.2) && (Cam4Center[1] >= 0.0) && (Cam4Center[1] <= 0.1))
            {
                // 顯示盲區 4
                SetScreenMode(ModeKind.ModeSP_04);

                // 做一下 Log 紀錄
                TextLog.WriteLog($"盲區 4 啟動(4 號相機): XCenter={Cam4Center[0]}, YCenter={Cam4Center[0]}");

                return;
            }
        }

        // 盲區判斷程式 SP05
        public void CheckModeSP_05()
        {
            // 若目前不是在花絮模式，就不用偵測與跳畫面
            if (currentMode != ModeKind.Mode15_B10)
                return;

            // 右側玻璃靠近廊道的走廊
            // 3 號攝影機的信心值，超過特定值(個別參數設定，目前設定 0.6)。
            // BBox 值，在 X(0.25~0.35) 與 Y(0~0.1) 之間(如下圖紅色區域)。 
            // 或 4 號攝影機的信心值，超過特定值(個別參數設定，目前設定 0.6)。
            // Bbox 值，在 X(0~0.1) 與 Y(0~0.1) 之間(如下圖紅色區域)。

            // 取得 3 號相機的 BBox 中心點
            List<double> Cam3Center = GetCamCordCenter(2);

            // 如果熊中心點在 X(0.25~0.35) 與 Y(0~0.1) 間，就啟動 No5 盲區
            if ((Cam3Center[0] >= 0.25) && (Cam3Center[0] <= 0.35) && (Cam3Center[1] >= 0.0) && (Cam3Center[1] <= 0.1))
            {
                // 顯示盲區 5
                SetScreenMode(ModeKind.ModeSP_05);

                // 做一下 Log 紀錄
                TextLog.WriteLog($"盲區 5 啟動(3 號相機): XCenter={Cam3Center[0]}, YCenter={Cam3Center[1]}");

                return;
            }

            // 取得 4 號相機的 BBox 中心點
            List<double> Cam4Center = GetCamCordCenter(3);

            // 如果熊中心點在 X(0~0.1) 與 Y(0~0.1) 間，就啟動 No3 盲區(By Cam4)
            if ((Cam4Center[0] >= 0.0) && (Cam4Center[0] <= 0.1) && (Cam4Center[1] >= 0.0) && (Cam4Center[1] <= 0.1))
            {
                // 顯示盲區 5
                SetScreenMode(ModeKind.ModeSP_05);

                // 做一下 Log 紀錄
                TextLog.WriteLog($"盲區 5 啟動(4 號相機): XCenter={Cam4Center[0]}, YCenter={Cam4Center[1]}");

                return;
            }
        }

        // 盲區判斷程式 SP06
        public void CheckModeSP_06()
        {
            // 被樹擋住的後方(位置：棲架下)
            // 啟動條件
            // 熊在 1 / 7 / 8 的特定位置(下三張圖紅色部分)，信心值與 BBox 規範與前類似
            // (記憶功能) 當熊原來在黃色區域，消失一定秒數(個別變數設定，先設定 5 秒)，其他區域也沒有出現時，判斷在棲架下

            // 被樹擋住的後方(位置：棲架後)
            // 啟動條件
            // 熊在 6 / 7 的特定位置(下兩張圖紅色部分)，信心值與 BBox 規範與前類似

            // 被樹擋住的後方(位置：樹木後圍牆旁)
            // 啟動條件
            // 熊在 3 / 4 的特定位置(下兩張圖紅色部分)，信心值與 BBox 規範與前類似

            // 取得 3 號相機的 BBox 中心點
            List<double> Cam3Center = GetCamCordCenter(2);

            // 如果熊中心點在 X(0.8 ~ 1.0) 與 Y(0.2 ~ 1.0) 間，就啟動 No5 盲區
            if ((Cam3Center[0] >= 0.8) && (Cam3Center[0] <= 1.0) && (Cam3Center[1] >= 0.2) && (Cam3Center[1] <= 1.0))
            {
                // 顯示盲區 6
                SetScreenMode(ModeKind.ModeSP_06);

                // 做一下 Log 紀錄
                TextLog.WriteLog($"盲區 6 啟動(3 號相機): XCenter={Cam3Center[0]}, YCenter={Cam3Center[1]}");

                return;
            }

            // 取得 4 號相機的 BBox 中心點
            List<double> Cam4Center = GetCamCordCenter(3);

            // 如果熊中心點在 X(0.8 ~ 0.9) 與 Y(0.1 ~ 0.6) 間，就啟動 No5 盲區
            if ((Cam4Center[0] >= 0.8) && (Cam4Center[0] <= 0.9) && (Cam4Center[1] >= 0.1) && (Cam4Center[1] <= 0.6))
            {
                // 顯示盲區 6
                SetScreenMode(ModeKind.ModeSP_06);

                // 做一下 Log 紀錄
                TextLog.WriteLog($"盲區 6 啟動(4 號相機): XCenter={Cam4Center[0]}, YCenter={Cam4Center[1]}");

                return;
            }

            // 取得 6 號相機的 BBox 中心點
            List<double> Cam6Center = GetCamCordCenter(5);

            // 如果熊中心點在 X(0.6 ~ 0.9) 與 Y(0.2 ~ 0.3) 間，就啟動 No6 盲區
            if ((Cam6Center[0] >= 0.6) && (Cam6Center[0] <= 0.9) && (Cam6Center[1] >= 0.2) && (Cam6Center[1] <= 0.3))
            {
                // 顯示盲區 6
                SetScreenMode(ModeKind.ModeSP_06);

                // 做一下 Log 紀錄
                TextLog.WriteLog($"盲區 6 啟動(6 號相機): XCenter={Cam6Center[0]}, YCenter={Cam6Center[1]}");

                return;
            }

            // 取得 7 號相機的 BBox 中心點
            List<double> Cam7Center = GetCamCordCenter(6);

            // 如果熊中心點在 X(0.2 ~ 0.6) 與 Y(0.2 ~ 0.6) 間，就啟動 No5 盲區
            if ((Cam7Center[0] >= 0.2) && (Cam7Center[0] <= 0.9) && (Cam7Center[1] >= 0.2) && (Cam7Center[1] <= 0.3))
            {
                // 顯示盲區 6
                SetScreenMode(ModeKind.ModeSP_06);

                // 做一下 Log 紀錄
                TextLog.WriteLog($"盲區 6 啟動(7 號相機): XCenter={Cam7Center[0]}, YCenter={Cam7Center[1]}");

                return;
            }

        }

        // 盲區判斷程式 SP07
        public void CheckModeSP_07()
        {
            // 和 No4 類似
            // 3 號攝影機的信心值，超過特定值(個別參數設定，目前設定 0.6)。

            // 取得 3 號相機的 BBox 中心點
            List<double> Cam3Center = GetCamCordCenter(2);

            // 如果熊中心點在 X(0.2~0.4) 與 Y(0~0.1) 間，就啟動 No4 盲區
            if ((Cam3Center[0] >= 0.2) && (Cam3Center[0] <= 0.4) && (Cam3Center[1] >= 0.0) && (Cam3Center[1] <= 0.1))
            {
                // 顯示盲區 7
                SetScreenMode(ModeKind.ModeSP_07);

                // 做一下 Log 紀錄
                TextLog.WriteLog($"盲區 7 啟動(3 號相機): XCenter={Cam3Center[0]}, YCenter={Cam3Center[1]}");

                return;
            }

            // 取得 4 號相機的 BBox 中心點
            List<double> Cam4Center = GetCamCordCenter(3);

            // 如果熊中心點在 X(0~0.2) 與 Y(0~0.1) 間，就啟動 No4 盲區(By Cam4)
            if ((Cam4Center[0] >= 0.0) && (Cam4Center[0] <= 0.2) && (Cam4Center[1] >= 0.0) && (Cam4Center[1] <= 0.1))
            {
                // 顯示盲區 7
                SetScreenMode(ModeKind.ModeSP_07);

                // 做一下 Log 紀錄
                TextLog.WriteLog($"盲區 7 啟動(4 號相機): XCenter={Cam4Center[0]}, YCenter={Cam4Center[0]}");

                return;
            }
        }

        // 盲區判斷程式 SP09
        public void CheckModeSP_09()
        {


        }

        // 盲區判斷程式 SP09
        public void CheckModeSP_10()
        {


        }

        // 盲區判斷程式 SP11
        public void CheckModeSP_11()
        {


        }

        // 盲區判斷程式 SP12
        public void CheckModeSP_12()
        {


        }

        // 取得 CamIndex 號相機(1號相機的 Index=0, 2號相機的 Index=1...)的 BBox 中心值
        public List<double> GetCamCordCenter(int CamIndex)
        {
            // 取得 CamIndex 號相機
            dynamic cam = bearObject.DataArray[CamIndex + 3];
            double camConf = 0.0;

            if (cam != null)
            {
                camConf = cam[0];
            }
            else
            {
                return new List<double> { -1.0, -1.0 };
            }

            // 若信心值是 -9999，代表沒有偵測到
            if (camConf == -9999.0)
            {
                return new List<double>{ -1.0, -1.0 };
            }

            // 若信心值太低，就沒偵測到
            if (camConf <= majorMethod.confThreshold)
            {
                return new List<double> { -1.0, -1.0 };
            }

            // 取得相機的 BBox 值
            //List<double> cord = cam[1];
            Newtonsoft.Json.Linq.JArray cord = (Newtonsoft.Json.Linq.JArray)cam[1];

            // 如果沒有 BBox
            if (cord.Count < 4)
            {
                return new List<double> { -1.0, -1.0 };
            }

            double BBoxX1 = cord[0].Value<double>();
            double BBoxY1 = cord[1].Value<double>();
            double BBoxX2 = cord[2].Value<double>();
            double BBoxY2 = cord[3].Value<double>();

            // 若傳來的是空值
            if (BBoxX1 == -9999.0)
            {
                return new List<double> { -1.0, -1.0 };
            }

            double XCenter = (BBoxX1 + BBoxX2) / 2;
            double YCenter = (BBoxY1 + BBoxY2) / 2;

            return new List<double> { XCenter, YCenter };
        }

        // 畫花絮的 Icon
        public void drawMode15B10Icon()
        {
            // Screen Left Top
            int ScreenLeft = 0;
            int ScreenTop = 0;

            // Bear X, Y
            int BearLeft = -1;
            int BearTop = -1;
            int BearRight = -1;
            int BearBottom = -1;

            // 顯示 screen_left_top 資料
            ScreenLeft = bearObject.ScreenLeftTop[0];
            ScreenTop = bearObject.ScreenLeftTop[1];

            // 顯示 bear_x_y 資料
            BearLeft = bearObject.BearXY[0] - ScreenLeft;
            BearTop = bearObject.BearXY[1] - ScreenTop;
            BearRight = bearObject.BearXY[2] - ScreenLeft;
            BearBottom = bearObject.BearXY[3] - ScreenTop;

            int PosX = 0;
            int PosY = 0;

            // 顯示 花絮 area_egg 資料與圖式

            // Egg_0
            PosX = bearObject.AreaEgg[0].PositionXY[0] - ScreenLeft;
            PosY = bearObject.AreaEgg[0].PositionXY[1] - ScreenTop;

            ImageBrush imgb_Egg_0 = new();
            imgb_Egg_0.ImageSource = new BitmapImage(new Uri(".\\Image\\ZOO_B_10_icon.png", UriKind.RelativeOrAbsolute));
            spnlB10Icon0.Background = imgb_Egg_0;

            Canvas.SetLeft(spnlB10Icon0, PosX);
            Canvas.SetTop(spnlB10Icon0, PosY);

            // Egg_1
            PosX = bearObject.AreaEgg[1].PositionXY[0] - ScreenLeft;
            PosY = bearObject.AreaEgg[1].PositionXY[1] - ScreenTop;

            ImageBrush imgb_Egg_1 = new();
            imgb_Egg_1.ImageSource = new BitmapImage(new Uri(".\\Image\\ZOO_B_10_icon.png", UriKind.RelativeOrAbsolute));
            spnlB10Icon1.Background = imgb_Egg_1;

            Canvas.SetLeft(spnlB10Icon1, PosX);
            Canvas.SetTop(spnlB10Icon1, PosY);

            // Egg_2
            PosX = bearObject.AreaEgg[2].PositionXY[0] - ScreenLeft;
            PosY = bearObject.AreaEgg[2].PositionXY[1] - ScreenTop;

            ImageBrush imgb_Egg_2 = new();
            imgb_Egg_2.ImageSource = new BitmapImage(new Uri(".\\Image\\ZOO_B_10_icon.png", UriKind.RelativeOrAbsolute));
            spnlB10Icon2.Background = imgb_Egg_2;

            Canvas.SetLeft(spnlB10Icon2, PosX);
            Canvas.SetTop(spnlB10Icon2, PosY);

            // Egg_3
            PosX = bearObject.AreaEgg[3].PositionXY[0] - ScreenLeft;
            PosY = bearObject.AreaEgg[3].PositionXY[1] - ScreenTop;

            ImageBrush imgb_Egg_3 = new();
            imgb_Egg_3.ImageSource = new BitmapImage(new Uri(".\\Image\\ZOO_B_10_icon.png", UriKind.RelativeOrAbsolute));
            spnlB10Icon3.Background = imgb_Egg_3;

            Canvas.SetLeft(spnlB10Icon3, PosX);
            Canvas.SetTop(spnlB10Icon3, PosY);

            // Egg_4
            PosX = bearObject.AreaEgg[4].PositionXY[0] - ScreenLeft;
            PosY = bearObject.AreaEgg[4].PositionXY[1] - ScreenTop;

            ImageBrush imgb_Egg_4 = new();
            imgb_Egg_4.ImageSource = new BitmapImage(new Uri(".\\Image\\ZOO_B_10_icon.png", UriKind.RelativeOrAbsolute));
            spnlB10Icon4.Background = imgb_Egg_4;

            Canvas.SetLeft(spnlB10Icon4, PosX);
            Canvas.SetTop(spnlB10Icon4, PosY);

            // Egg_5
            PosX = bearObject.AreaEgg[5].PositionXY[0] - ScreenLeft;
            PosY = bearObject.AreaEgg[5].PositionXY[1] - ScreenTop;

            ImageBrush imgb_Egg_5 = new();
            imgb_Egg_5.ImageSource = new BitmapImage(new Uri(".\\Image\\ZOO_B_10_icon.png", UriKind.RelativeOrAbsolute));
            spnlB10Icon5.Background = imgb_Egg_5;

            Canvas.SetLeft(spnlB10Icon5, PosX);
            Canvas.SetTop(spnlB10Icon5, PosY);

            // Egg_6
            PosX = bearObject.AreaEgg[6].PositionXY[0] - ScreenLeft;
            PosY = bearObject.AreaEgg[6].PositionXY[1] - ScreenTop;

            ImageBrush imgb_Egg_6 = new();
            imgb_Egg_6.ImageSource = new BitmapImage(new Uri(".\\Image\\ZOO_B_10_icon.png", UriKind.RelativeOrAbsolute));
            spnlB10Icon6.Background = imgb_Egg_6;

            Canvas.SetLeft(spnlB10Icon6, PosX);
            Canvas.SetTop(spnlB10Icon6, PosY);

            // Egg_7
            PosX = bearObject.AreaEgg[7].PositionXY[0] - ScreenLeft;
            PosY = bearObject.AreaEgg[7].PositionXY[1] - ScreenTop;

            ImageBrush imgb_Egg_7 = new();
            imgb_Egg_7.ImageSource = new BitmapImage(new Uri(".\\Image\\ZOO_B_10_icon.png", UriKind.RelativeOrAbsolute));
            spnlB10Icon7.Background = imgb_Egg_7;

            Canvas.SetLeft(spnlB10Icon7, PosX);
            Canvas.SetTop(spnlB10Icon7, PosY);

        }

        public void drawMapIcon()
        {
            // 20241204 修改，直接拿 calc 的值來放熊頭

            if ((majorMethod.MapBearIconX == -1) || (majorMethod.MapBearIconY == -1))
            {
                spnlMapIcon.Visibility = Visibility.Hidden;
            }
            else
            {
                // 加上多國語言
                ImageBrush imgb_map_button = new();

                if (languageModel == LanguageMode.Chinese)
                {
                    imgb_map_button.ImageSource = new BitmapImage(new Uri(".\\Image\\ZOO_map_10_icon.png", UriKind.RelativeOrAbsolute));
                }
                else
                {
                    imgb_map_button.ImageSource = new BitmapImage(new Uri(".\\Image\\ZOO_E_map_10_icon.png", UriKind.RelativeOrAbsolute));
                }

                spnlMapIcon.Background = imgb_map_button;
                spnlMapIcon.Visibility = Visibility.Visible;

                // 2024/12/05 直接指定位置
                Canvas.SetLeft(spnlMapIcon, majorMethod.MapBearIconX);
                Canvas.SetTop(spnlMapIcon, majorMethod.MapBearIconY);

                //// 設定動畫移動
                //MoveImageByAnimation(spnlMapIcon, majorMethod.MapBearIconX, majorMethod.MapBearIconY);
            }
        }

        public void drawLiveCamButton()
        {
            // 若第一支攝影機是放映者
            // 加入時間阻尼，當還沒到啟動時間，就返回
            //if ((DateTime.Now - majorMethod.LiveButtonTime_01) >= MajorMethod.LiveButtonTimeSpan)
            //{



            //    // 儲存更新時間
            //    majorMethod.LiveButtonTime_01 = DateTime.Now;
            //}


            // spnlLiveBtn0 --------------------------------
            dynamic cameraData = bearObject.DataArray[3];

            // 若第一支攝影機是放映者
            // 加入時間阻尼，當還沒到啟動時間，就返回
            if ((DateTime.Now - majorMethod.LiveButtonTime_01) >= MajorMethod.LiveButtonTimeSpan)
            {
                if (liveCamIndex == 0)
                {
                    // 當第一支攝影機有熊，就放有熊且放映的圖標
                    if ((cameraData != null) && (cameraData[0] >= majorMethod.confThreshold))
                    {
                        ImageBrush imgb_live_button = new();
                        imgb_live_button.ImageSource = new BitmapImage(new Uri(".\\Image\\ZOO_live_10_31.png", UriKind.RelativeOrAbsolute));
                        spnlLiveBtn0.Background = imgb_live_button;
                    }
                    else
                    {
                        // 只放沒有熊，放映的圖標
                        ImageBrush imgb_live_button = new();
                        imgb_live_button.ImageSource = new BitmapImage(new Uri(".\\Image\\ZOO_live_10_11.png", UriKind.RelativeOrAbsolute));
                        spnlLiveBtn0.Background = imgb_live_button;
                    }
                }
                else
                {
                    // 不是放映者，放有熊的圖標
                    if ((cameraData != null) && (cameraData[0] >= majorMethod.confThreshold))
                    {
                        ImageBrush imgb_live_button = new();
                        imgb_live_button.ImageSource = new BitmapImage(new Uri(".\\Image\\ZOO_live_10_21.png", UriKind.RelativeOrAbsolute));
                        spnlLiveBtn0.Background = imgb_live_button;
                    }
                    else
                    {
                        // 沒有熊也沒有放映，放最一般的圖標
                        ImageBrush imgb_live_button = new();
                        imgb_live_button.ImageSource = new BitmapImage(new Uri(".\\Image\\ZOO_live_10_01.png", UriKind.RelativeOrAbsolute));
                        spnlLiveBtn0.Background = imgb_live_button;
                    }
                }

                // 儲存更新時間
                majorMethod.LiveButtonTime_01 = DateTime.Now;
            }

            // spnlLiveBtn1 --------------------------------
            cameraData = bearObject.DataArray[4];

            // 若第二支攝影機是放映者
            if ((DateTime.Now - majorMethod.LiveButtonTime_02) >= MajorMethod.LiveButtonTimeSpan)
            {
                if (liveCamIndex == 1)
                {
                    // 當第一支攝影機有熊，就放有熊且放映的圖標
                    if ((cameraData != null) && (cameraData[0] >= majorMethod.confThreshold))
                    {
                        ImageBrush imgb_live_button = new();
                        imgb_live_button.ImageSource = new BitmapImage(new Uri(".\\Image\\ZOO_live_10_32.png", UriKind.RelativeOrAbsolute));
                        spnlLiveBtn1.Background = imgb_live_button;
                    }
                    else
                    {
                        // 只放沒有熊，放映的圖標
                        ImageBrush imgb_live_button = new();
                        imgb_live_button.ImageSource = new BitmapImage(new Uri(".\\Image\\ZOO_live_10_12.png", UriKind.RelativeOrAbsolute));
                        spnlLiveBtn1.Background = imgb_live_button;
                    }
                }
                else
                {
                    // 不是放映者，放有熊的圖標
                    if ((cameraData != null) && (cameraData[0] >= majorMethod.confThreshold))
                    {
                        ImageBrush imgb_live_button = new();
                        imgb_live_button.ImageSource = new BitmapImage(new Uri(".\\Image\\ZOO_live_10_22.png", UriKind.RelativeOrAbsolute));
                        spnlLiveBtn1.Background = imgb_live_button;
                    }
                    else
                    {
                        // 沒有熊也沒有放映，放最一般的圖標
                        ImageBrush imgb_live_button = new();
                        imgb_live_button.ImageSource = new BitmapImage(new Uri(".\\Image\\ZOO_live_10_02.png", UriKind.RelativeOrAbsolute));
                        spnlLiveBtn1.Background = imgb_live_button;
                    }
                }

                // 儲存更新時間
                majorMethod.LiveButtonTime_02 = DateTime.Now;
            }

            // spnlLiveBtn2 --------------------------------
            cameraData = bearObject.DataArray[5];

            // No.3 攝影機是放映者
            if ((DateTime.Now - majorMethod.LiveButtonTime_03) >= MajorMethod.LiveButtonTimeSpan)
            {
                if (liveCamIndex == 2)
                {
                    // 當第一支攝影機有熊，就放有熊且放映的圖標
                    if ((cameraData != null) && (cameraData[0] >= majorMethod.confThreshold))
                    {
                        ImageBrush imgb_live_button = new();
                        imgb_live_button.ImageSource = new BitmapImage(new Uri(".\\Image\\ZOO_live_10_33.png", UriKind.RelativeOrAbsolute));
                        spnlLiveBtn2.Background = imgb_live_button;
                    }
                    else
                    {
                        // 只放沒有熊，放映的圖標
                        ImageBrush imgb_live_button = new();
                        imgb_live_button.ImageSource = new BitmapImage(new Uri(".\\Image\\ZOO_live_10_13.png", UriKind.RelativeOrAbsolute));
                        spnlLiveBtn2.Background = imgb_live_button;
                    }
                }
                else
                {
                    // 不是放映者，放有熊的圖標
                    if ((cameraData != null) && (cameraData[0] >= majorMethod.confThreshold))
                    {
                        ImageBrush imgb_live_button = new();
                        imgb_live_button.ImageSource = new BitmapImage(new Uri(".\\Image\\ZOO_live_10_23.png", UriKind.RelativeOrAbsolute));
                        spnlLiveBtn2.Background = imgb_live_button;
                    }
                    else
                    {
                        // 沒有熊也沒有放映，放最一般的圖標
                        ImageBrush imgb_live_button = new();
                        imgb_live_button.ImageSource = new BitmapImage(new Uri(".\\Image\\ZOO_live_10_03.png", UriKind.RelativeOrAbsolute));
                        spnlLiveBtn2.Background = imgb_live_button;
                    }
                }

                // 儲存更新時間
                majorMethod.LiveButtonTime_03 = DateTime.Now;
            }

            // spnlLiveBtn3 --------------------------------
            cameraData = bearObject.DataArray[6];

            // No.4 攝影機是放映者
            if ((DateTime.Now - majorMethod.LiveButtonTime_04) >= MajorMethod.LiveButtonTimeSpan)
            {
                if (liveCamIndex == 3)
                {
                    // 當第一支攝影機有熊，就放有熊且放映的圖標
                    if ((cameraData != null) && (cameraData[0] >= majorMethod.confThreshold))
                    {
                        ImageBrush imgb_live_button = new();
                        imgb_live_button.ImageSource = new BitmapImage(new Uri(".\\Image\\ZOO_live_10_34.png", UriKind.RelativeOrAbsolute));
                        spnlLiveBtn3.Background = imgb_live_button;
                    }
                    else
                    {
                        // 只放沒有熊，放映的圖標
                        ImageBrush imgb_live_button = new();
                        imgb_live_button.ImageSource = new BitmapImage(new Uri(".\\Image\\ZOO_live_10_14.png", UriKind.RelativeOrAbsolute));
                        spnlLiveBtn3.Background = imgb_live_button;
                    }
                }
                else
                {
                    // 不是放映者，放有熊的圖標
                    if ((cameraData != null) && (cameraData[0] >= majorMethod.confThreshold))
                    {
                        ImageBrush imgb_live_button = new();
                        imgb_live_button.ImageSource = new BitmapImage(new Uri(".\\Image\\ZOO_live_10_24.png", UriKind.RelativeOrAbsolute));
                        spnlLiveBtn3.Background = imgb_live_button;
                    }
                    else
                    {
                        // 沒有熊也沒有放映，放最一般的圖標
                        ImageBrush imgb_live_button = new();
                        imgb_live_button.ImageSource = new BitmapImage(new Uri(".\\Image\\ZOO_live_10_04.png", UriKind.RelativeOrAbsolute));
                        spnlLiveBtn3.Background = imgb_live_button;
                    }
                }

                // 儲存更新時間
                majorMethod.LiveButtonTime_04 = DateTime.Now;
            }

            // spnlLiveBtn4 --------------------------------
            cameraData = bearObject.DataArray[7];

            if ((DateTime.Now - majorMethod.LiveButtonTime_05) >= MajorMethod.LiveButtonTimeSpan)
            {
                // No.5 攝影機是放映者
                if (liveCamIndex == 4)
                {
                    // 當第一支攝影機有熊，就放有熊且放映的圖標
                    if ((cameraData != null) && (cameraData[0] >= majorMethod.confThreshold))
                    {
                        ImageBrush imgb_live_button = new();
                        imgb_live_button.ImageSource = new BitmapImage(new Uri(".\\Image\\ZOO_live_10_35.png", UriKind.RelativeOrAbsolute));
                        spnlLiveBtn4.Background = imgb_live_button;
                    }
                    else
                    {
                        // 只放沒有熊，放映的圖標
                        ImageBrush imgb_live_button = new();
                        imgb_live_button.ImageSource = new BitmapImage(new Uri(".\\Image\\ZOO_live_10_15.png", UriKind.RelativeOrAbsolute));
                        spnlLiveBtn4.Background = imgb_live_button;
                    }
                }
                else
                {
                    // 不是放映者，放有熊的圖標
                    if ((cameraData != null) && (cameraData[0] >= majorMethod.confThreshold))
                    {
                        ImageBrush imgb_live_button = new();
                        imgb_live_button.ImageSource = new BitmapImage(new Uri(".\\Image\\ZOO_live_10_25.png", UriKind.RelativeOrAbsolute));
                        spnlLiveBtn4.Background = imgb_live_button;
                    }
                    else
                    {
                        // 沒有熊也沒有放映，放最一般的圖標
                        ImageBrush imgb_live_button = new();
                        imgb_live_button.ImageSource = new BitmapImage(new Uri(".\\Image\\ZOO_live_10_05.png", UriKind.RelativeOrAbsolute));
                        spnlLiveBtn4.Background = imgb_live_button;
                    }
                }

                // 儲存更新時間
                majorMethod.LiveButtonTime_05 = DateTime.Now;
            }

            // spnlLiveBtn6 --------------------------------
            cameraData = bearObject.DataArray[8];

            if ((DateTime.Now - majorMethod.LiveButtonTime_06) >= MajorMethod.LiveButtonTimeSpan)
            {
                // No.6 攝影機是放映者
                if (liveCamIndex == 5)
                {
                    // 當第一支攝影機有熊，就放有熊且放映的圖標
                    if ((cameraData != null) && (cameraData[0] >= majorMethod.confThreshold))
                    {
                        ImageBrush imgb_live_button = new();
                        imgb_live_button.ImageSource = new BitmapImage(new Uri(".\\Image\\ZOO_live_10_36.png", UriKind.RelativeOrAbsolute));
                        spnlLiveBtn5.Background = imgb_live_button;
                    }
                    else
                    {
                        // 只放沒有熊，放映的圖標
                        ImageBrush imgb_live_button = new();
                        imgb_live_button.ImageSource = new BitmapImage(new Uri(".\\Image\\ZOO_live_10_16.png", UriKind.RelativeOrAbsolute));
                        spnlLiveBtn5.Background = imgb_live_button;
                    }
                }
                else
                {
                    // 不是放映者，放有熊的圖標
                    if ((cameraData != null) && (cameraData[0] >= majorMethod.confThreshold))
                    {
                        ImageBrush imgb_live_button = new();
                        imgb_live_button.ImageSource = new BitmapImage(new Uri(".\\Image\\ZOO_live_10_26.png", UriKind.RelativeOrAbsolute));
                        spnlLiveBtn5.Background = imgb_live_button;
                    }
                    else
                    {
                        // 沒有熊也沒有放映，放最一般的圖標
                        ImageBrush imgb_live_button = new();
                        imgb_live_button.ImageSource = new BitmapImage(new Uri(".\\Image\\ZOO_live_10_06.png", UriKind.RelativeOrAbsolute));
                        spnlLiveBtn5.Background = imgb_live_button;
                    }
                }

                // 儲存更新時間
                majorMethod.LiveButtonTime_06 = DateTime.Now;
            }

            // spnlLiveBtn6 --------------------------------
            cameraData = bearObject.DataArray[9];

            // No.7 攝影機是放映者
            if ((DateTime.Now - majorMethod.LiveButtonTime_07) >= MajorMethod.LiveButtonTimeSpan)
            {
                if (liveCamIndex == 6)
                {
                    // 當第一支攝影機有熊，就放有熊且放映的圖標
                    if ((cameraData != null) && (cameraData[0] >= majorMethod.confThreshold))
                    {
                        ImageBrush imgb_live_button = new();
                        imgb_live_button.ImageSource = new BitmapImage(new Uri(".\\Image\\ZOO_live_10_37.png", UriKind.RelativeOrAbsolute));
                        spnlLiveBtn6.Background = imgb_live_button;
                    }
                    else
                    {
                        // 只放沒有熊，放映的圖標
                        ImageBrush imgb_live_button = new();
                        imgb_live_button.ImageSource = new BitmapImage(new Uri(".\\Image\\ZOO_live_10_17.png", UriKind.RelativeOrAbsolute));
                        spnlLiveBtn6.Background = imgb_live_button;
                    }
                }
                else
                {
                    // 不是放映者，放有熊的圖標
                    if ((cameraData != null) && (cameraData[0] >= majorMethod.confThreshold))
                    {
                        ImageBrush imgb_live_button = new();
                        imgb_live_button.ImageSource = new BitmapImage(new Uri(".\\Image\\ZOO_live_10_27.png", UriKind.RelativeOrAbsolute));
                        spnlLiveBtn6.Background = imgb_live_button;
                    }
                    else
                    {
                        // 沒有熊也沒有放映，放最一般的圖標
                        ImageBrush imgb_live_button = new();
                        imgb_live_button.ImageSource = new BitmapImage(new Uri(".\\Image\\ZOO_live_10_07.png", UriKind.RelativeOrAbsolute));
                        spnlLiveBtn6.Background = imgb_live_button;
                    }
                }

                // 儲存更新時間
                majorMethod.LiveButtonTime_07 = DateTime.Now;
            }

            // spnlLiveBtn7 --------------------------------
            cameraData = bearObject.DataArray[10];

            // No.8 攝影機是放映者
            if ((DateTime.Now - majorMethod.LiveButtonTime_08) >= MajorMethod.LiveButtonTimeSpan)
            {
                if (liveCamIndex == 7)
                {
                    // 當第一支攝影機有熊，就放有熊且放映的圖標
                    if ((cameraData != null) && (cameraData[0] >= majorMethod.confThreshold))
                    {
                        ImageBrush imgb_live_button = new();
                        imgb_live_button.ImageSource = new BitmapImage(new Uri(".\\Image\\ZOO_live_10_38.png", UriKind.RelativeOrAbsolute));
                        spnlLiveBtn7.Background = imgb_live_button;
                    }
                    else
                    {
                        // 只放沒有熊，放映的圖標
                        ImageBrush imgb_live_button = new();
                        imgb_live_button.ImageSource = new BitmapImage(new Uri(".\\Image\\ZOO_live_10_18.png", UriKind.RelativeOrAbsolute));
                        spnlLiveBtn7.Background = imgb_live_button;
                    }
                }
                else
                {
                    // 不是放映者，放有熊的圖標
                    if ((cameraData != null) && (cameraData[0] >= majorMethod.confThreshold))
                    {
                        ImageBrush imgb_live_button = new();
                        imgb_live_button.ImageSource = new BitmapImage(new Uri(".\\Image\\ZOO_live_10_28.png", UriKind.RelativeOrAbsolute));
                        spnlLiveBtn7.Background = imgb_live_button;
                    }
                    else
                    {
                        // 沒有熊也沒有放映，放最一般的圖標
                        ImageBrush imgb_live_button = new();
                        imgb_live_button.ImageSource = new BitmapImage(new Uri(".\\Image\\ZOO_live_10_08.png", UriKind.RelativeOrAbsolute));
                        spnlLiveBtn7.Background = imgb_live_button;
                    }
                }

                // 儲存更新時間
                majorMethod.LiveButtonTime_08 = DateTime.Now;
            }

            // spnlLiveBtn8 --------------------------------
            cameraData = bearObject.DataArray[11];

            // No.9 攝影機是放映者
            if ((DateTime.Now - majorMethod.LiveButtonTime_09) >= MajorMethod.LiveButtonTimeSpan)
            {
                if (liveCamIndex == 8)
                {
                    // 當第一支攝影機有熊，就放有熊且放映的圖標
                    if ((cameraData != null) && (cameraData[0] >= majorMethod.confThreshold))
                    {
                        ImageBrush imgb_live_button = new();
                        imgb_live_button.ImageSource = new BitmapImage(new Uri(".\\Image\\ZOO_live_10_39.png", UriKind.RelativeOrAbsolute));
                        spnlLiveBtn8.Background = imgb_live_button;
                    }
                    else
                    {
                        // 只放沒有熊，放映的圖標
                        ImageBrush imgb_live_button = new();
                        imgb_live_button.ImageSource = new BitmapImage(new Uri(".\\Image\\ZOO_live_10_19.png", UriKind.RelativeOrAbsolute));
                        spnlLiveBtn8.Background = imgb_live_button;
                    }
                }
                else
                {
                    // 不是放映者，放有熊的圖標
                    if ((cameraData != null) && (cameraData[0] >= majorMethod.confThreshold))
                    {
                        ImageBrush imgb_live_button = new();
                        imgb_live_button.ImageSource = new BitmapImage(new Uri(".\\Image\\ZOO_live_10_29.png", UriKind.RelativeOrAbsolute));
                        spnlLiveBtn8.Background = imgb_live_button;
                    }
                    else
                    {
                        // 沒有熊也沒有放映，放最一般的圖標
                        ImageBrush imgb_live_button = new();
                        imgb_live_button.ImageSource = new BitmapImage(new Uri(".\\Image\\ZOO_live_10_09.png", UriKind.RelativeOrAbsolute));
                        spnlLiveBtn8.Background = imgb_live_button;
                    }
                }

                // 儲存更新時間
                majorMethod.LiveButtonTime_09 = DateTime.Now;
            }
        }

        // 此為工程測試用，在螢幕上顯示框與花絮圖
        public void drawARData(bool showAR)
        {
            // Screen Left Top
            int ScreenLeft = 0;
            int ScreenTop = 0;

            // Bear X, Y
            int BearLeft = -1;
            int BearTop = -1;
            int BearRight = -1;
            int BearBottom = -1;
            
            // 依照 showAR 來開關圖層
            spnlShowAR.Visibility = showAR ? Visibility.Visible: Visibility.Collapsed;

            if (showAR == false)
                return;

            // 顯示 AR 模組傳來的地圖資訊
            double bearXRatio = double.Parse(bearObject.MapXRatio);
            double bearYRatio = double.Parse(bearObject.MapYRatio);

            //int bearXPos = (int)(bearXRatio * 580) + 0;
            //int bearYPos = (int)(bearYRatio * 470) + 0;

            lblMapPosition.Content = $"bearXRatio:{bearXRatio, 5:N3}, bearYRatio:{bearYRatio, 5:N3}";

            // 顯示 screen_left_top 資料
            ScreenLeft = bearObject.ScreenLeftTop[0];
            ScreenTop = bearObject.ScreenLeftTop[1];

            lblScreen_left_top.Content = $"Screen_left_top: [{ScreenLeft}, {ScreenTop}]";

            // 顯示 bear_x_y 資料
            BearLeft = bearObject.BearXY[0] - ScreenLeft;
            BearTop = bearObject.BearXY[1] - ScreenTop; 
            BearRight = bearObject.BearXY[2] - ScreenLeft;
            BearBottom = bearObject.BearXY[3] - ScreenTop;

            lblBear_x_y.Content = $"Bear_x_y - Screen_left_top: [{BearLeft}, {BearTop}, {BearRight}, {BearBottom}]";

            // 顯示螢幕的 Degree 與 Height
            lblScreenDegree.Content = $"裝置角度: {majorMethod.machineDegree}";
            lblScreenHeight.Content = $"裝置高度: {majorMethod.machineHeight}";

            // 顯示電腦的時間
            lblNowTime.Content = "Now: " + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff");
            lblNowTime_red.Content = "Now: " + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff");

            // 顯示 Screen Mode
            lblScreenMode.Content = $"Screen Mode:{bearObject.ScreenMode}";

            // 畫 bear_x_y 的框
            brdBear.Margin = new Thickness(BearLeft, BearTop, 0, 0);
            brdBear.Width = BearRight - BearLeft;
            brdBear.Height = BearBottom - BearTop;

            // 辨識模組傳來的 Ratio
            dynamic ratio_x = bearObject.DataArray[0];
            dynamic ratio_y = bearObject.DataArray[1];

            lblMapRatio.Content = $"RatioX:{ratio_x,5:N3}, RatioY:{ratio_y,5:N3}";

            // 標示所有 Cam 的 Conf
            dynamic cam1 = bearObject.DataArray[0 + 3];
            if (cam1 != null)
            {
                lblCam1Conf.Content = $"Cam1：{cam1[0], 5:N3}, Pose:{cam1[2]}";
                lblCam1Time.Content = $"Time：{cam1[3]}";
            }
            else
            {
                lblCam1Conf.Content = $"Cam1：null";
            }

            dynamic cam2 = bearObject.DataArray[1 + 3];
            if (cam2 != null)
            {
                lblCam2Conf.Content = $"Cam2：{cam2[0], 5:N3} , Pose:{cam2[2]}";
                lblCam2Time.Content = $"Time：{cam2[3]}";
            }
            else
            {
                lblCam2Conf.Content = $"Cam2：null";
            }

            dynamic cam3 = bearObject.DataArray[2 + 3];
            if (cam3 != null)
            {
                lblCam3Conf.Content = $"Cam3：{cam3[0], 5:N3} , Pose:{cam3[2]}";
                lblCam3Time.Content = $"Time：{cam3[3]}";
            }
            else
            {
                lblCam3Conf.Content = $"Cam3：null";
            }

            dynamic cam4 = bearObject.DataArray[3 + 3];
            if (cam4 != null)
            {
                lblCam4Conf.Content = $"Cam4：{cam4[0], 5:N3} , Pose:{cam4[2]}";
                lblCam4Time.Content = $"Time：{cam4[3]}";
            }
            else
            {
                lblCam4Conf.Content = $"Cam4：null";
            }

            dynamic cam5 = bearObject.DataArray[4 + 3];
            if (cam5 != null)
            {
                lblCam5Conf.Content = $"Cam5：{cam5[0], 5:N3}  , Pose:{cam5[2]}";
                lblCam5Time.Content = $"Time：{cam5[3]}";
            }
            else
            {
                lblCam5Conf.Content = $"Cam5：null";
            }

            dynamic cam6 = bearObject.DataArray[5 + 3];
            if (cam6 != null)
            {
                lblCam6Conf.Content = $"Cam6：{cam6[0], 5:N3}  , Pose:{cam6[2]}";
                lblCam6Time.Content = $"Time：{cam6[3]}";
            }
            else
            {
                lblCam6Conf.Content = $"Cam6：null";
            }

            dynamic cam7 = bearObject.DataArray[6 + 3];
            if (cam7 != null)
            {
                lblCam7Conf.Content = $"Cam7：{cam7[0], 5:N3}  , Pose:{cam7[2]}";
                lblCam7Time.Content = $"Time：{cam7[3]}";
            }
            else
            {
                lblCam7Conf.Content = $"Cam7：null";
            }

            dynamic cam8 = bearObject.DataArray[7 + 3];
            if (cam8 != null)
            {
                lblCam8Conf.Content = $"Cam8：{cam8[0], 5:N3}, Pose:{cam8[2]}";
                lblCam8Time.Content = $"Time：{cam8[3]}";
            }
            else
            {
                lblCam8Conf.Content = $"Cam8：null";
            }

            dynamic cam9 = bearObject.DataArray[8 + 3];
            if (cam9 != null)
            {
                lblCam9Conf.Content = $"Cam9：{cam9[0], 5:N3}  , Pose:{cam9[2]}";
                lblCam9Time.Content = $"Time：{cam9[3]}";
            }
            else
            {
                lblCam9Conf.Content = $"Cam9：null";
            }

            // SelectData
            dynamic selectedData = bearObject.SelectedData;
            try
            {
                if (selectedData != null)
                {
                    if (selectedData[0] != null)
                    {
                        lblSelectedData.Content = $"choice：Conf {selectedData[0], 5:N3}, Index:{selectedData[3]}, Pose:{selectedData[2]}";
                        lblSelectedDataTime.Content = $"Time：{selectedData[4]}";
                    }
                }
            }
            catch (Exception ex)
            {
                lblSelectedData.Content = $"choice：null";
            }

            // 顯示 Screen Degree & Height 
            lblScreenDegree_Yellow.Content = $"裝置角度:{majorMethod.machineDegree}";
            lblScreenHeight_Yellow.Content = $"裝置高度:{majorMethod.machineHeight}";

            // 紀錄 9 之相機
            //TextLog.WriteLog("UI Cam1:" + lblCam1Conf.Content);
            //TextLog.WriteLog("UI Cam2:" + lblCam2Conf.Content);
            //TextLog.WriteLog("UI Cam3:" + lblCam3Conf.Content);
            //TextLog.WriteLog("UI Cam4:" + lblCam4Conf.Content);
            //TextLog.WriteLog("UI Cam5:" + lblCam5Conf.Content);
            //TextLog.WriteLog("UI Cam6:" + lblCam6Conf.Content);
            //TextLog.WriteLog("UI Cam7:" + lblCam7Conf.Content);
            //TextLog.WriteLog("UI Cam8:" + lblCam8Conf.Content);
            //TextLog.WriteLog("UI Cam9:" + lblCam9Conf.Content);
            //TextLog.WriteLog("SelectData:" + lblSelectedData.Content);

        }


        // 在此進行畫面切換
        public void SetScreenMode(ModeKind screenMode, int additionalArgs = 0)
        {
            // 如果與 Mode 相同，則畫面不做變化
            if (currentMode == screenMode)
            {
                return;
            }

            // 若是 SPMode_01，就改變變數值，不做畫面的改變
            if (screenMode == ModeKind.ModeSP_01)
            {
                IsModeSP_01 = !IsModeSP_01;

                return;
            }

            // 阻尼功能
            // 一般狀態(Mode1/Mode15_B10) 與 發現熊狀態(Mode15_A20_1, Mode15_A30_1, Mode15_A40_1, Mode15_A50_1)
            // 以上兩個狀態，若要作切換時，時間要大於 ModeInterval 才作切換

            // 從 一般模式 要切到 發現熊
            if ((currentMode == ModeKind.Mode1 || currentMode == ModeKind.Mode15_B10) &&
                (screenMode == ModeKind.Mode15_A20_1 || screenMode == ModeKind.Mode15_A30_1 || screenMode == ModeKind.Mode15_A40_1 || screenMode == ModeKind.Mode15_A50_1))
            {
                // 當還沒到啟動時間，就返回
                if ((DateTime.Now - majorMethod.ModeLastChangeTime) <= MajorMethod.ModeInterval)
                {
                    return;
                }
            }

            // 在發現熊模式，當熊消失，要回到一般模式時，也是要阻尼 3 秒，防止閃爍
            if ((screenMode == ModeKind.Mode1 || screenMode == ModeKind.Mode15_B10) &&
                (currentMode == ModeKind.Mode15_A20_1 || currentMode == ModeKind.Mode15_A30_1 || currentMode == ModeKind.Mode15_A40_1 || currentMode == ModeKind.Mode15_A50_1))
            {
                // 當還沒到啟動時間，就返回
                if ((DateTime.Now - majorMethod.ModeLastChangeTime) <= MajorMethod.ModeInterval)
                {
                    return;
                }
            }

            // 一般模式(轉圈圈)與花絮模式，也回有互換的情況，因此要阻尼 3 秒，防止閃爍
            if ((screenMode == ModeKind.Mode1 && screenMode == ModeKind.Mode15_B10) ||
                (screenMode == ModeKind.Mode15_B10 && screenMode == ModeKind.Mode1))
            {
                // 當還沒到啟動時間，就返回
                if ((DateTime.Now - majorMethod.ModeLastChangeTime) <= MajorMethod.ModeInterval)
                {
                    return;
                }
            }

            // 先關閉所有的 StackPanel
            HideAndResetAllPanel();

            // 紀錄切換時間
            majorMethod.ModeLastChangeTime = DateTime.Now;

            // 儲存 Mode 和 SubMode
            currentMode = screenMode;

            // 進行各 Mode 的切換
            switch (currentMode)
            {
                case ModeKind.None:
                    break;

                // Mode0 是隱藏的首頁
                case ModeKind.Mode0:
                    Show_Mode0();
                    break;

                // 轉圈圈頁
                case ModeKind.Mode1:
                    Show_Mode1();
                    break;

                // 站立系列
                case ModeKind.Mode15_A20_1:
                    Show_Mode15_A20_1();
                    break;

                case ModeKind.Mode2_A21_1:
                    Show_Mode2_A21_1();
                    break;

                case ModeKind.Mode3_A22_1:
                    Show_Mode3_A22_1(additionalArgs);
                    break;

                // 趴臥系列
                case ModeKind.Mode15_A30_1:
                    Show_Mode15_A30_1();
                    break;

                case ModeKind.Mode2_A31_1:
                    Show_Mode2_A31_1();
                    break;

                case ModeKind.Mode3_A32_1:
                    Show_Mode3_A32_1(additionalArgs);
                    break;

                // 四足系列
                case ModeKind.Mode15_A40_1:
                    Show_Mode15_A40_1();
                    break;

                case ModeKind.Mode2_A41_1:
                    Show_Mode2_A41_1();
                    break;

                case ModeKind.Mode3_A42_1:
                    Show_Mode3_A42_1(additionalArgs);
                    break;

                // 坐系列
                case ModeKind.Mode15_A50_1:
                    Show_Mode15_A50_1();
                    break;

                case ModeKind.Mode2_A51_1:
                    Show_Mode2_A51_1();
                    break;

                case ModeKind.Mode3_A52_1:
                    Show_Mode3_A52_1(additionalArgs);
                    break;

                // 花絮部分
                case ModeKind.Mode15_B10:
                    Show_Mode15_B10();
                    break;

                case ModeKind.Mode2_B20_1:
                    Show_Mode2_B20(additionalArgs);
                    break;

                case ModeKind.Mode2_B30_1:
                    Show_Mode2_B30(additionalArgs);
                    break;

                // 小地圖
                case ModeKind.Mode2_MAP_10:
                    Show_Mode2_Map();
                    break;

                // Live 影像
                case ModeKind.Mode2_LIVE_10:
                    Show_Mode2_Live();
                    break;

                // 盲區 1
                case ModeKind.ModeSP_01:
                    break;

                // 盲區 2
                case ModeKind.ModeSP_02:
                    Show_ModeSP_02();
                    break;

                // 盲區 3
                case ModeKind.ModeSP_03:
                    Show_ModeSP_03();
                    break;

                // 盲區 4
                case ModeKind.ModeSP_04:
                    Show_ModeSP_04();
                    break;

                // 盲區 5
                case ModeKind.ModeSP_05:
                    Show_ModeSP_05();
                    break;

                // 盲區 6
                case ModeKind.ModeSP_06:
                    Show_ModeSP_06();
                    break;

                // 盲區 7
                case ModeKind.ModeSP_07:
                    Show_ModeSP_07();
                    break;

                // 盲區 9
                case ModeKind.ModeSP_09:
                    Show_ModeSP_09();
                    break;

                // 盲區 10
                case ModeKind.ModeSP_10:
                    Show_ModeSP_10();
                    break;

                // 盲區 11
                case ModeKind.ModeSP_11:
                    Show_ModeSP_11();
                    break;

                // 盲區 12
                case ModeKind.ModeSP_12:
                    Show_ModeSP_12();
                    break;
            }
        }

        // 關閉所有的 spnl
        private void HideAndResetAllPanel()
        {
            // Mode 1 首頁
            spnlMode1.Visibility = Visibility.Collapsed;
            mdeMoviePlayer.Visibility = Visibility.Collapsed;

            // Mode 1.5 站立
            spnlMode15A20.Visibility = Visibility.Collapsed;

            // Mode 2 站立 Q
            spnlMode2A21.Visibility = Visibility.Collapsed;

            spnlMode2A21_Q1.Visibility = Visibility.Collapsed;
            spnlMode2A21_Q2.Visibility = Visibility.Collapsed;
            spnlMode2A21_Q3.Visibility = Visibility.Collapsed;
            spnlMode2A21_Q4.Visibility = Visibility.Collapsed;
            spnlMode2A21_Q5.Visibility = Visibility.Collapsed;

            // Mode 3 站立 A
            spnlMode3A22.Visibility = Visibility.Collapsed;
            spnlMode3A22_A.Visibility = Visibility.Collapsed;
            spnlMode3A22_A_AR.Visibility = Visibility.Collapsed;

            // Mode 1.5 趴臥
            spnlMode15A30.Visibility = Visibility.Collapsed;

            // Mode 2 趴臥 Q
            spnlMode2A31.Visibility = Visibility.Collapsed;

            spnlMode2A31_Q1.Visibility = Visibility.Collapsed;
            spnlMode2A31_Q2.Visibility = Visibility.Collapsed;
            spnlMode2A31_Q3.Visibility = Visibility.Collapsed;
            spnlMode2A31_Q4.Visibility = Visibility.Collapsed;
            spnlMode2A31_Q5.Visibility = Visibility.Collapsed;

            // Mode 3 趴臥 A
            spnlMode3A32.Visibility = Visibility.Collapsed;
            spnlMode3A32_A.Visibility = Visibility.Collapsed;
            spnlMode3A32_A_AR.Visibility = Visibility.Collapsed;

            // Mode 1.5 四足
            spnlMode15A40.Visibility = Visibility.Collapsed;

            // Mode 2 四足 Q
            spnlMode2A41.Visibility = Visibility.Collapsed;

            spnlMode2A41_Q1.Visibility = Visibility.Collapsed;
            spnlMode2A41_Q2.Visibility = Visibility.Collapsed;
            spnlMode2A41_Q3.Visibility = Visibility.Collapsed;
            spnlMode2A41_Q4.Visibility = Visibility.Collapsed;
            spnlMode2A41_Q5.Visibility = Visibility.Collapsed;

            // Mode 3 四足 A
            spnlMode3A42.Visibility = Visibility.Collapsed;
            spnlMode3A42_A.Visibility = Visibility.Collapsed;
            spnlMode3A42_A_AR.Visibility = Visibility.Collapsed;

            // Mode 1.5 坐
            spnlMode15A50.Visibility = Visibility.Collapsed;

            // Mode 2 坐 Q
            spnlMode2A51.Visibility = Visibility.Collapsed;

            spnlMode2A51_Q1.Visibility = Visibility.Collapsed;
            spnlMode2A51_Q2.Visibility = Visibility.Collapsed;
            spnlMode2A51_Q3.Visibility = Visibility.Collapsed;
            spnlMode2A51_Q4.Visibility = Visibility.Collapsed;
            spnlMode2A51_Q5.Visibility = Visibility.Collapsed;

            // Mode 3 坐 A
            spnlMode3A52.Visibility = Visibility.Collapsed;
            spnlMode3A52_A.Visibility = Visibility.Collapsed;
            spnlMode3A52_A_AR.Visibility = Visibility.Collapsed;

            // Mode 1.5 花絮
            spnlMode15B10.Visibility = Visibility.Collapsed;

            // Mode 2 B20 花絮
            spnlMode2B20.Visibility = Visibility.Collapsed;
            mdeMoviePlayer_B20.Visibility = Visibility.Collapsed;

            // Mode 2 B30 花絮
            spnlMode2B30.Visibility = Visibility.Collapsed;
            mdeMoviePlayer_B30.Visibility = Visibility.Collapsed;

            // Mode 2 Map
            spnlMode2Map.Visibility = Visibility.Collapsed;
            spnlMapIcon.Visibility = Visibility.Hidden;

            // Mode 2 Live
            spnlMode2Live.Visibility = Visibility.Hidden;
            cnsLive.Visibility = Visibility.Hidden;
            //mainVideoView.Visibility = Visibility.Hidden;

        }

        // 顯示 Mode 0 的畫面(ZOO_A_10)
        private void Show_Mode0()
        {
            // 這是 Mode1 和 Mode15_B10 上方的隱藏頁，因此什麼都不用做喔
            // 接下來即時部分，changeModeByARData 馬上會跳到正確的 mode 去
            // 因此避免畫面閃動

        }

        // 顯示 Mode 1 的畫面(ZOO_A_10)
        private void Show_Mode1()
        {
            // 顯示底層
            ImageBrush imgb_c_mode_01 = new();

            // 中英文之圖型
            if (languageModel == LanguageMode.Chinese)
            {
                imgb_c_mode_01.ImageSource = new BitmapImage(new Uri(".\\Image\\ZOO_A_10.png", UriKind.RelativeOrAbsolute));
            }
            else
            {
                imgb_c_mode_01.ImageSource = new BitmapImage(new Uri(".\\Image\\ZOO_E_A_10.png", UriKind.RelativeOrAbsolute));
            }

            spnlMode1.Visibility = Visibility.Visible;
            spnlMode1.Background = imgb_c_mode_01;

            // 顯示動畫部份
            Uri Mode1MoviePath = new Uri(".\\Image\\ZOO_A_10.mp4", UriKind.RelativeOrAbsolute);
            mdeMoviePlayer.Visibility = Visibility.Visible;
            mdeMoviePlayer.Source = Mode1MoviePath;
            mdeMoviePlayer.Play();
        }

        // 影片撥放到最後時
        private void mdeMoviePlayer_MediaEnded(object sender, RoutedEventArgs e)
        {
            // 媒體播放結束時觸發這個事件，將媒體的位置設定為0，以重新開始播放
            mdeMoviePlayer.Position = TimeSpan.Zero;
            mdeMoviePlayer.Play();
        }

        // 顯示 Mode 1.5 站立
        private void Show_Mode15_A20_1()
        {
            // 顯示圖層
            ImageBrush imgb_c_mode_15_a20 = new();

            // 中英文之圖型
            if (languageModel == LanguageMode.Chinese)
            {
                imgb_c_mode_15_a20.ImageSource = new BitmapImage(new Uri(".\\Image\\ZOO_A_20_1.png", UriKind.RelativeOrAbsolute));
            }
            else
            {
                imgb_c_mode_15_a20.ImageSource = new BitmapImage(new Uri(".\\Image\\ZOO_E_A_20_1.png", UriKind.RelativeOrAbsolute));
            }

            spnlMode15A20.Visibility = Visibility.Visible;
            spnlMode15A20.Background = imgb_c_mode_15_a20;
        }

        // 顯示 Mode 2 站立 Q
        private void Show_Mode2_A21_1()
        {
            // 設定問題的初始位置
            int QPosX = 50;
            int QPosY = 10;
            
            // 顯示圖層
            ImageBrush imgb_c_mode_2_a21 = new();

            // 中英文之圖型
            if (languageModel == LanguageMode.Chinese)
            {
                imgb_c_mode_2_a21.ImageSource = new BitmapImage(new Uri(".\\Image\\ZOO_A_21_1.png", UriKind.RelativeOrAbsolute));
            }
            else
            {
                imgb_c_mode_2_a21.ImageSource = new BitmapImage(new Uri(".\\Image\\ZOO_E_A_21_1.png", UriKind.RelativeOrAbsolute));
            }

            spnlMode2A21.Visibility = Visibility.Visible;
            spnlMode2A21.Background = imgb_c_mode_2_a21;

            // 隨機五選三個值
            int[] selectedNumbers = select3by5();

            // Q1
            if (Array.IndexOf(selectedNumbers, 1) != -1)
            {
                ImageBrush imgb_c_mode_2_a21_q1 = new();

                if (languageModel == LanguageMode.Chinese)
                {
                    imgb_c_mode_2_a21_q1.ImageSource = new BitmapImage(new Uri(".\\Image\\ZOO_A_21_11.png", UriKind.RelativeOrAbsolute));
                }
                else
                {
                    imgb_c_mode_2_a21_q1.ImageSource = new BitmapImage(new Uri(".\\Image\\ZOO_E_A_21_11.png", UriKind.RelativeOrAbsolute));
                }

                // 設定問題位置
                Canvas.SetLeft(spnlMode2A21_Q1, QPosX);
                Canvas.SetTop(spnlMode2A21_Q1, QPosY);

                // 設定下一個問題位置
                QPosY += 70;

                spnlMode2A21_Q1.Visibility = Visibility.Visible;
                spnlMode2A21_Q1.Background = imgb_c_mode_2_a21_q1;
            }
            else
            {
                spnlMode2A21_Q1.Visibility = Visibility.Collapsed;
            }

            // Q2
            if (Array.IndexOf(selectedNumbers, 2) != -1)
            {
                ImageBrush imgb_c_mode_2_a21_q2 = new();

                if (languageModel == LanguageMode.Chinese)
                {
                    imgb_c_mode_2_a21_q2.ImageSource = new BitmapImage(new Uri(".\\Image\\ZOO_A_21_12.png", UriKind.RelativeOrAbsolute));
                }
                else
                {
                    imgb_c_mode_2_a21_q2.ImageSource = new BitmapImage(new Uri(".\\Image\\ZOO_E_A_21_12.png", UriKind.RelativeOrAbsolute));
                }

                Canvas.SetLeft(spnlMode2A21_Q2, QPosX);
                Canvas.SetTop(spnlMode2A21_Q2, QPosY);

                // 設定下一個問題位置
                QPosY += 70;

                spnlMode2A21_Q2.Visibility = Visibility.Visible;
                spnlMode2A21_Q2.Background = imgb_c_mode_2_a21_q2;
            }
            else
            {
                spnlMode2A21_Q2.Visibility = Visibility.Collapsed;
            }

            // Q3
            if (Array.IndexOf(selectedNumbers, 3) != -1)
            {
                ImageBrush imgb_c_mode_2_a21_q3 = new();

                if (languageModel == LanguageMode.Chinese)
                {
                    imgb_c_mode_2_a21_q3.ImageSource = new BitmapImage(new Uri(".\\Image\\ZOO_A_21_13.png", UriKind.RelativeOrAbsolute));
                }
                else
                {
                    imgb_c_mode_2_a21_q3.ImageSource = new BitmapImage(new Uri(".\\Image\\ZOO_E_A_21_13.png", UriKind.RelativeOrAbsolute));
                }

                Canvas.SetLeft(spnlMode2A21_Q3, QPosX);
                Canvas.SetTop(spnlMode2A21_Q3, QPosY);

                // 設定下一個問題位置
                QPosY += 70;

                spnlMode2A21_Q3.Visibility = Visibility.Visible;
                spnlMode2A21_Q3.Background = imgb_c_mode_2_a21_q3;
            }
            else
            {
                spnlMode2A21_Q3.Visibility = Visibility.Collapsed;
            }

            // Q4
            if (Array.IndexOf(selectedNumbers, 4) != -1)
            {
                ImageBrush imgb_c_mode_2_a21_q4 = new();

                if (languageModel == LanguageMode.Chinese)
                {
                    imgb_c_mode_2_a21_q4.ImageSource = new BitmapImage(new Uri(".\\Image\\ZOO_A_21_14.png", UriKind.RelativeOrAbsolute));
                }
                else
                {
                    imgb_c_mode_2_a21_q4.ImageSource = new BitmapImage(new Uri(".\\Image\\ZOO_E_A_21_14.png", UriKind.RelativeOrAbsolute));
                }

                Canvas.SetLeft(spnlMode2A21_Q4, QPosX);
                Canvas.SetTop(spnlMode2A21_Q4, QPosY);

                // 設定下一個問題位置
                QPosY += 70;

                spnlMode2A21_Q4.Visibility = Visibility.Visible;
                spnlMode2A21_Q4.Background = imgb_c_mode_2_a21_q4;
            }
            else
            {
                spnlMode2A21_Q4.Visibility = Visibility.Collapsed;
            }

            // Q5
            if (Array.IndexOf(selectedNumbers, 5) != -1)
            {
                ImageBrush imgb_c_mode_2_a21_q5 = new();

                if (languageModel == LanguageMode.Chinese)
                {
                    imgb_c_mode_2_a21_q5.ImageSource = new BitmapImage(new Uri(".\\Image\\ZOO_A_21_15.png", UriKind.RelativeOrAbsolute));
                }
                else
                {
                    imgb_c_mode_2_a21_q5.ImageSource = new BitmapImage(new Uri(".\\Image\\ZOO_E_A_21_15.png", UriKind.RelativeOrAbsolute));
                }

                Canvas.SetLeft(spnlMode2A21_Q5, QPosX);
                Canvas.SetTop(spnlMode2A21_Q5, QPosY);

                // 設定下一個問題位置
                QPosY += 70;

                spnlMode2A21_Q5.Visibility = Visibility.Visible;
                spnlMode2A21_Q5.Background = imgb_c_mode_2_a21_q5;
            }
            else
            {
                spnlMode2A21_Q5.Visibility = Visibility.Collapsed;
            }
        }

        // 顯示 Mode 3 站立 A
        private void Show_Mode3_A22_1(int args)
        {
            // 顯示圖層
            ImageBrush imgb_c_mode_3_a22 = new();
            imgb_c_mode_3_a22.Stretch = Stretch.Uniform;
            imgb_c_mode_3_a22.AlignmentX = AlignmentX.Center;
            imgb_c_mode_3_a22.AlignmentY = AlignmentY.Center;

            if (languageModel == LanguageMode.Chinese)
            {
                imgb_c_mode_3_a22.ImageSource = new BitmapImage(new Uri(".\\Image\\ZOO_A_22_1.png", UriKind.RelativeOrAbsolute));
            }
            else
            {
                imgb_c_mode_3_a22.ImageSource = new BitmapImage(new Uri(".\\Image\\ZOO_E_A_22_1.png", UriKind.RelativeOrAbsolute));
            }

            spnlMode3A22.Visibility = Visibility.Visible;
            spnlMode3A22.Background = imgb_c_mode_3_a22;

            // 顯示答案
            ImageBrush imgb_c_mode_3_a22_a = new();
            if (languageModel == LanguageMode.Chinese)
            {
                switch (args)
                {
                    case 1:
                        imgb_c_mode_3_a22_a.ImageSource = new BitmapImage(new Uri(".\\Image\\ZOO_R_A_22_11.png", UriKind.RelativeOrAbsolute));
                        break;

                    case 2:
                        imgb_c_mode_3_a22_a.ImageSource = new BitmapImage(new Uri(".\\Image\\ZOO_R_A_22_12.png", UriKind.RelativeOrAbsolute));
                        break;

                    case 3:
                        imgb_c_mode_3_a22_a.ImageSource = new BitmapImage(new Uri(".\\Image\\ZOO_R_A_22_13.png", UriKind.RelativeOrAbsolute));
                        break;

                    case 4:
                        imgb_c_mode_3_a22_a.ImageSource = new BitmapImage(new Uri(".\\Image\\ZOO_R_A_22_14.png", UriKind.RelativeOrAbsolute));
                        break;

                    case 5:
                        imgb_c_mode_3_a22_a.ImageSource = new BitmapImage(new Uri(".\\Image\\ZOO_R_A_22_15.png", UriKind.RelativeOrAbsolute));
                        break;
                }
            }
            else
            {
                switch (args)
                {
                    case 1:
                        imgb_c_mode_3_a22_a.ImageSource = new BitmapImage(new Uri(".\\Image\\ZOO_R_E_A_22_11.png", UriKind.RelativeOrAbsolute));
                        break;

                    case 2:
                        imgb_c_mode_3_a22_a.ImageSource = new BitmapImage(new Uri(".\\Image\\ZOO_R_E_A_22_12.png", UriKind.RelativeOrAbsolute));
                        break;

                    case 3:
                        imgb_c_mode_3_a22_a.ImageSource = new BitmapImage(new Uri(".\\Image\\ZOO_R_E_A_22_13.png", UriKind.RelativeOrAbsolute));
                        break;

                    case 4:
                        imgb_c_mode_3_a22_a.ImageSource = new BitmapImage(new Uri(".\\Image\\ZOO_R_E_A_22_14.png", UriKind.RelativeOrAbsolute));
                        break;

                    case 5:
                        imgb_c_mode_3_a22_a.ImageSource = new BitmapImage(new Uri(".\\Image\\ZOO_R_E_A_22_15.png", UriKind.RelativeOrAbsolute));
                        break;
                }
            }
            spnlMode3A22_A.Visibility = Visibility.Visible;
            spnlMode3A22_A.Background = imgb_c_mode_3_a22_a;
        }

        // 顯示 Mode 1.5 趴臥
        private void Show_Mode15_A30_1()
        {
            // 顯示圖層
            ImageBrush imgb_c_mode_15_a30 = new();

            // 中英文之圖型
            if (languageModel == LanguageMode.Chinese)
            {
                imgb_c_mode_15_a30.ImageSource = new BitmapImage(new Uri(".\\Image\\ZOO_A_30_1.png", UriKind.RelativeOrAbsolute));
            }
            else
            {
                imgb_c_mode_15_a30.ImageSource = new BitmapImage(new Uri(".\\Image\\ZOO_E_A_30_1.png", UriKind.RelativeOrAbsolute));
            }

            spnlMode15A30.Visibility = Visibility.Visible;
            spnlMode15A30.Background = imgb_c_mode_15_a30;
        }

        // 顯示 Mode 2 趴臥 Q
        private void Show_Mode2_A31_1()
        {
            // 設定問題的初始位置
            int QPosX = 560;
            int QPosY = 10;

            // 顯示圖層
            ImageBrush imgb_c_mode_2_a31 = new();

            // 中英文之圖型
            if (languageModel == LanguageMode.Chinese)
            {
                imgb_c_mode_2_a31.ImageSource = new BitmapImage(new Uri(".\\Image\\ZOO_A_31_1.png", UriKind.RelativeOrAbsolute));
            }
            else
            {
                imgb_c_mode_2_a31.ImageSource = new BitmapImage(new Uri(".\\Image\\ZOO_E_A_31_1.png", UriKind.RelativeOrAbsolute));
            }

            spnlMode2A31.Visibility = Visibility.Visible;
            spnlMode2A31.Background = imgb_c_mode_2_a31;

            // 隨機五選三個值
            int[] selectedNumbers = select3by5();

            // Q1
            if (Array.IndexOf(selectedNumbers, 1) != -1)
            {
                ImageBrush imgb_c_mode_2_a31_q1 = new();

                if (languageModel == LanguageMode.Chinese)
                {
                    imgb_c_mode_2_a31_q1.ImageSource = new BitmapImage(new Uri(".\\Image\\ZOO_A_31_11.png", UriKind.RelativeOrAbsolute));
                }
                else
                {
                    imgb_c_mode_2_a31_q1.ImageSource = new BitmapImage(new Uri(".\\Image\\ZOO_E_A_31_11.png", UriKind.RelativeOrAbsolute));
                }

                // 設定問題位置
                Canvas.SetLeft(spnlMode2A31_Q1, QPosX);
                Canvas.SetTop(spnlMode2A31_Q1, QPosY);

                // 設定下一個問題位置
                QPosY += 70;

                spnlMode2A31_Q1.Visibility = Visibility.Visible;
                spnlMode2A31_Q1.Background = imgb_c_mode_2_a31_q1;
            }
            else
            {
                spnlMode2A31_Q1.Visibility = Visibility.Collapsed;
            }

            // Q2
            if (Array.IndexOf(selectedNumbers, 2) != -1)
            {
                ImageBrush imgb_c_mode_2_a31_q2 = new();

                if (languageModel == LanguageMode.Chinese)
                {
                    imgb_c_mode_2_a31_q2.ImageSource = new BitmapImage(new Uri(".\\Image\\ZOO_A_31_12.png", UriKind.RelativeOrAbsolute));
                }
                else
                {
                    imgb_c_mode_2_a31_q2.ImageSource = new BitmapImage(new Uri(".\\Image\\ZOO_E_A_31_12.png", UriKind.RelativeOrAbsolute));
                }

                Canvas.SetLeft(spnlMode2A31_Q2, QPosX);
                Canvas.SetTop(spnlMode2A31_Q2, QPosY);

                // 設定下一個問題位置
                QPosY += 70;

                spnlMode2A31_Q2.Visibility = Visibility.Visible;
                spnlMode2A31_Q2.Background = imgb_c_mode_2_a31_q2;
            }
            else
            {
                spnlMode2A31_Q2.Visibility = Visibility.Collapsed;
            }

            // Q3
            if (Array.IndexOf(selectedNumbers, 3) != -1)
            {
                ImageBrush imgb_c_mode_2_a31_q3 = new();

                if (languageModel == LanguageMode.Chinese)
                {
                    imgb_c_mode_2_a31_q3.ImageSource = new BitmapImage(new Uri(".\\Image\\ZOO_A_31_13.png", UriKind.RelativeOrAbsolute));
                }
                else
                {
                    imgb_c_mode_2_a31_q3.ImageSource = new BitmapImage(new Uri(".\\Image\\ZOO_E_A_31_13.png", UriKind.RelativeOrAbsolute));
                }

                Canvas.SetLeft(spnlMode2A31_Q3, QPosX);
                Canvas.SetTop(spnlMode2A31_Q3, QPosY);

                // 設定下一個問題位置
                QPosY += 70;

                spnlMode2A31_Q3.Visibility = Visibility.Visible;
                spnlMode2A31_Q3.Background = imgb_c_mode_2_a31_q3;
            }
            else
            {
                spnlMode2A31_Q3.Visibility = Visibility.Collapsed;
            }


            // Q4
            if (Array.IndexOf(selectedNumbers, 4) != -1)
            {
                ImageBrush imgb_c_mode_2_a31_q4 = new();

                if (languageModel == LanguageMode.Chinese)
                {
                    imgb_c_mode_2_a31_q4.ImageSource = new BitmapImage(new Uri(".\\Image\\ZOO_A_31_14.png", UriKind.RelativeOrAbsolute));
                }
                else
                {
                    imgb_c_mode_2_a31_q4.ImageSource = new BitmapImage(new Uri(".\\Image\\ZOO_E_A_31_14.png", UriKind.RelativeOrAbsolute));
                }

                Canvas.SetLeft(spnlMode2A31_Q4, QPosX);
                Canvas.SetTop(spnlMode2A31_Q4, QPosY);

                // 設定下一個問題位置
                QPosY += 70;

                spnlMode2A31_Q4.Visibility = Visibility.Visible;
                spnlMode2A31_Q4.Background = imgb_c_mode_2_a31_q4;
            }
            else
            {
                spnlMode2A31_Q4.Visibility = Visibility.Collapsed;
            }

            // Q5
            if (Array.IndexOf(selectedNumbers, 5) != -1)
            {
                ImageBrush imgb_c_mode_2_a31_q5 = new();

                if (languageModel == LanguageMode.Chinese)
                {
                    imgb_c_mode_2_a31_q5.ImageSource = new BitmapImage(new Uri(".\\Image\\ZOO_A_31_15.png", UriKind.RelativeOrAbsolute));
                }
                else
                {
                    imgb_c_mode_2_a31_q5.ImageSource = new BitmapImage(new Uri(".\\Image\\ZOO_E_A_31_15.png", UriKind.RelativeOrAbsolute));
                }

                Canvas.SetLeft(spnlMode2A31_Q5, QPosX);
                Canvas.SetTop(spnlMode2A31_Q5, QPosY);

                // 設定下一個問題位置
                QPosY += 70;

                spnlMode2A31_Q5.Visibility = Visibility.Visible;
                spnlMode2A31_Q5.Background = imgb_c_mode_2_a31_q5;
            }
            else
            {
                spnlMode2A31_Q5.Visibility = Visibility.Collapsed;
            }

        }

        // 顯示 Mode 3 趴臥 A
        private void Show_Mode3_A32_1(int args)
        {
            // 顯示圖層
            ImageBrush imgb_c_mode_3_a32 = new();
            imgb_c_mode_3_a32.Stretch = Stretch.Uniform;
            imgb_c_mode_3_a32.AlignmentX = AlignmentX.Center;
            imgb_c_mode_3_a32.AlignmentY = AlignmentY.Center;

            if (languageModel == LanguageMode.Chinese)
            {
                imgb_c_mode_3_a32.ImageSource = new BitmapImage(new Uri(".\\Image\\ZOO_A_32_1.png", UriKind.RelativeOrAbsolute));
            }
            else
            {
                imgb_c_mode_3_a32.ImageSource = new BitmapImage(new Uri(".\\Image\\ZOO_E_A_32_1.png", UriKind.RelativeOrAbsolute));
            }

            spnlMode3A32.Visibility = Visibility.Visible;
            spnlMode3A32.Background = imgb_c_mode_3_a32;

            // 顯示答案
            ImageBrush imgb_c_mode_3_a32_a = new();

            if (languageModel == LanguageMode.Chinese)
            {
                switch (args)
                {
                    case 1:
                        imgb_c_mode_3_a32_a.ImageSource = new BitmapImage(new Uri(".\\Image\\ZOO_A_32_11.png", UriKind.RelativeOrAbsolute));
                        break;

                    case 2:
                        imgb_c_mode_3_a32_a.ImageSource = new BitmapImage(new Uri(".\\Image\\ZOO_A_32_12.png", UriKind.RelativeOrAbsolute));
                        break;

                    case 3:
                        imgb_c_mode_3_a32_a.ImageSource = new BitmapImage(new Uri(".\\Image\\ZOO_A_32_13.png", UriKind.RelativeOrAbsolute));
                        break;

                    case 4:
                        imgb_c_mode_3_a32_a.ImageSource = new BitmapImage(new Uri(".\\Image\\ZOO_A_32_14.png", UriKind.RelativeOrAbsolute));
                        break;

                    case 5:
                        imgb_c_mode_3_a32_a.ImageSource = new BitmapImage(new Uri(".\\Image\\ZOO_A_32_15.png", UriKind.RelativeOrAbsolute));
                        break;
                }
            }
            else
            {
                switch (args)
                {
                    case 1:
                        imgb_c_mode_3_a32_a.ImageSource = new BitmapImage(new Uri(".\\Image\\ZOO_E_A_32_11.png", UriKind.RelativeOrAbsolute));
                        break;

                    case 2:
                        imgb_c_mode_3_a32_a.ImageSource = new BitmapImage(new Uri(".\\Image\\ZOO_E_A_32_12.png", UriKind.RelativeOrAbsolute));
                        break;

                    case 3:
                        imgb_c_mode_3_a32_a.ImageSource = new BitmapImage(new Uri(".\\Image\\ZOO_E_A_32_13.png", UriKind.RelativeOrAbsolute));
                        break;

                    case 4:
                        imgb_c_mode_3_a32_a.ImageSource = new BitmapImage(new Uri(".\\Image\\ZOO_E_A_32_14.png", UriKind.RelativeOrAbsolute));
                        break;

                    case 5:
                        imgb_c_mode_3_a32_a.ImageSource = new BitmapImage(new Uri(".\\Image\\ZOO_E_A_32_15.png", UriKind.RelativeOrAbsolute));
                        break;
                }
            }

            spnlMode3A32_A.Visibility = Visibility.Visible;
            spnlMode3A32_A.Background = imgb_c_mode_3_a32_a;
        }

        // 顯示 Mode 1.5 四足
        private void Show_Mode15_A40_1()
        {
            // 顯示圖層
            ImageBrush imgb_c_mode_15_a40 = new();

            // 中英文之圖型
            if (languageModel == LanguageMode.Chinese)
            {
                imgb_c_mode_15_a40.ImageSource = new BitmapImage(new Uri(".\\Image\\ZOO_A_40_1.png", UriKind.RelativeOrAbsolute));
            }
            else
            {
                imgb_c_mode_15_a40.ImageSource = new BitmapImage(new Uri(".\\Image\\ZOO_E_A_40_1.png", UriKind.RelativeOrAbsolute));
            }

            spnlMode15A40.Visibility = Visibility.Visible;
            spnlMode15A40.Background = imgb_c_mode_15_a40;
        }

        // 顯示 Mode 2 四足 Q
        private void Show_Mode2_A41_1()
        {
            // 設定問題的初始位置
            int QPosX = 560;
            int QPosY = 10;

            // 顯示圖層
            ImageBrush imgb_c_mode_2_a41 = new();

            // 中英文之圖型
            if (languageModel == LanguageMode.Chinese)
            {
                imgb_c_mode_2_a41.ImageSource = new BitmapImage(new Uri(".\\Image\\ZOO_A_41_1.png", UriKind.RelativeOrAbsolute));
            }
            else
            {
                imgb_c_mode_2_a41.ImageSource = new BitmapImage(new Uri(".\\Image\\ZOO_E_A_41_1.png", UriKind.RelativeOrAbsolute));
            }

            spnlMode2A41.Visibility = Visibility.Visible;
            spnlMode2A41.Background = imgb_c_mode_2_a41;

            // 隨機五選三個值
            int[] selectedNumbers = select3by5();

            // Q1
            if (Array.IndexOf(selectedNumbers, 1) != -1)
            {
                ImageBrush imgb_c_mode_2_a41_q1 = new();

                if (languageModel == LanguageMode.Chinese)
                {
                    imgb_c_mode_2_a41_q1.ImageSource = new BitmapImage(new Uri(".\\Image\\ZOO_A_41_11.png", UriKind.RelativeOrAbsolute));
                }
                else
                {
                    imgb_c_mode_2_a41_q1.ImageSource = new BitmapImage(new Uri(".\\Image\\ZOO_E_A_41_11.png", UriKind.RelativeOrAbsolute));
                }

                // 設定問題位置
                Canvas.SetLeft(spnlMode2A41_Q1, QPosX);
                Canvas.SetTop(spnlMode2A41_Q1, QPosY);

                // 設定下一個問題位置
                QPosY += 70;

                spnlMode2A41_Q1.Visibility = Visibility.Visible;
                spnlMode2A41_Q1.Background = imgb_c_mode_2_a41_q1;
            }
            else
            {
                spnlMode2A41_Q1.Visibility = Visibility.Collapsed;
            }

            // Q2
            if (Array.IndexOf(selectedNumbers, 2) != -1)
            {
                ImageBrush imgb_c_mode_2_a41_q2 = new();

                if (languageModel == LanguageMode.Chinese)
                {
                    imgb_c_mode_2_a41_q2.ImageSource = new BitmapImage(new Uri(".\\Image\\ZOO_A_41_12.png", UriKind.RelativeOrAbsolute));
                }
                else
                {
                    imgb_c_mode_2_a41_q2.ImageSource = new BitmapImage(new Uri(".\\Image\\ZOO_E_A_41_12.png", UriKind.RelativeOrAbsolute));
                }

                Canvas.SetLeft(spnlMode2A41_Q2, QPosX);
                Canvas.SetTop(spnlMode2A41_Q2, QPosY);

                // 設定下一個問題位置
                QPosY += 70;

                spnlMode2A41_Q2.Visibility = Visibility.Visible;
                spnlMode2A41_Q2.Background = imgb_c_mode_2_a41_q2;
            }
            else
            {
                spnlMode2A41_Q2.Visibility = Visibility.Collapsed;
            }

            // Q3
            if (Array.IndexOf(selectedNumbers, 3) != -1)
            {
                ImageBrush imgb_c_mode_2_a41_q3 = new();

                if (languageModel == LanguageMode.Chinese)
                {
                    imgb_c_mode_2_a41_q3.ImageSource = new BitmapImage(new Uri(".\\Image\\ZOO_A_41_13.png", UriKind.RelativeOrAbsolute));
                }
                else
                {
                    imgb_c_mode_2_a41_q3.ImageSource = new BitmapImage(new Uri(".\\Image\\ZOO_E_A_41_13.png", UriKind.RelativeOrAbsolute));
                }

                Canvas.SetLeft(spnlMode2A41_Q3, QPosX);
                Canvas.SetTop(spnlMode2A41_Q3, QPosY);

                // 設定下一個問題位置
                QPosY += 70;

                spnlMode2A41_Q3.Visibility = Visibility.Visible;
                spnlMode2A41_Q3.Background = imgb_c_mode_2_a41_q3;
            }
            else
            {
                spnlMode2A41_Q3.Visibility = Visibility.Collapsed;
            }

            // Q4
            if (Array.IndexOf(selectedNumbers, 4) != -1)
            {
                ImageBrush imgb_c_mode_2_a41_q4 = new();

                if (languageModel == LanguageMode.Chinese)
                {
                    imgb_c_mode_2_a41_q4.ImageSource = new BitmapImage(new Uri(".\\Image\\ZOO_A_41_14.png", UriKind.RelativeOrAbsolute));
                }
                else
                {
                    imgb_c_mode_2_a41_q4.ImageSource = new BitmapImage(new Uri(".\\Image\\ZOO_E_A_41_14.png", UriKind.RelativeOrAbsolute));
                }

                Canvas.SetLeft(spnlMode2A41_Q4, QPosX);
                Canvas.SetTop(spnlMode2A41_Q4, QPosY);

                // 設定下一個問題位置
                QPosY += 70;

                spnlMode2A41_Q4.Visibility = Visibility.Visible;
                spnlMode2A41_Q4.Background = imgb_c_mode_2_a41_q4;
            }
            else
            {
                spnlMode2A41_Q4.Visibility = Visibility.Collapsed;
            }

            // Q5
            if (Array.IndexOf(selectedNumbers, 5) != -1)
            {
                ImageBrush imgb_c_mode_2_a41_q5 = new();

                if (languageModel == LanguageMode.Chinese)
                {
                    imgb_c_mode_2_a41_q5.ImageSource = new BitmapImage(new Uri(".\\Image\\ZOO_A_41_15.png", UriKind.RelativeOrAbsolute));
                }
                else
                {
                    imgb_c_mode_2_a41_q5.ImageSource = new BitmapImage(new Uri(".\\Image\\ZOO_E_A_41_15.png", UriKind.RelativeOrAbsolute));
                }

                Canvas.SetLeft(spnlMode2A41_Q5, QPosX);
                Canvas.SetTop(spnlMode2A41_Q5, QPosY);

                // 設定下一個問題位置
                QPosY += 70;

                spnlMode2A41_Q5.Visibility = Visibility.Visible;
                spnlMode2A41_Q5.Background = imgb_c_mode_2_a41_q5;
            }
            else
            {
                spnlMode2A41_Q5.Visibility = Visibility.Collapsed;
            }
        }

        // 顯示 Mode 3 四足 A
        private void Show_Mode3_A42_1(int args)
        {
            // 顯示圖層
            ImageBrush imgb_c_mode_3_a42 = new();
            imgb_c_mode_3_a42.Stretch = Stretch.Uniform;
            imgb_c_mode_3_a42.AlignmentX = AlignmentX.Center;
            imgb_c_mode_3_a42.AlignmentY = AlignmentY.Center;

            if (languageModel == LanguageMode.Chinese)
            {
                imgb_c_mode_3_a42.ImageSource = new BitmapImage(new Uri(".\\Image\\ZOO_A_42_1.png", UriKind.RelativeOrAbsolute));
            }
            else
            {
                imgb_c_mode_3_a42.ImageSource = new BitmapImage(new Uri(".\\Image\\ZOO_E_A_42_1.png", UriKind.RelativeOrAbsolute));
            }

            spnlMode3A42.Visibility = Visibility.Visible;
            spnlMode3A42.Background = imgb_c_mode_3_a42;

            // 顯示答案
            ImageBrush imgb_c_mode_3_a42_a = new();

            if (languageModel == LanguageMode.Chinese)
            {
                switch (args)
                {
                    case 1:
                        imgb_c_mode_3_a42_a.ImageSource = new BitmapImage(new Uri(".\\Image\\ZOO_L_A_42_11.png", UriKind.RelativeOrAbsolute));
                        break;

                    case 2:
                        imgb_c_mode_3_a42_a.ImageSource = new BitmapImage(new Uri(".\\Image\\ZOO_L_A_42_12.png", UriKind.RelativeOrAbsolute));
                        break;

                    case 3:
                        imgb_c_mode_3_a42_a.ImageSource = new BitmapImage(new Uri(".\\Image\\ZOO_L_A_42_13.png", UriKind.RelativeOrAbsolute));
                        break;

                    case 4:
                        imgb_c_mode_3_a42_a.ImageSource = new BitmapImage(new Uri(".\\Image\\ZOO_L_A_42_14.png", UriKind.RelativeOrAbsolute));
                        break;

                    case 5:
                        imgb_c_mode_3_a42_a.ImageSource = new BitmapImage(new Uri(".\\Image\\ZOO_L_A_42_15.png", UriKind.RelativeOrAbsolute));
                        break;
                }
            }
            else
            {
                switch (args)
                {
                    case 1:
                        imgb_c_mode_3_a42_a.ImageSource = new BitmapImage(new Uri(".\\Image\\ZOO_L_E_A_42_11.png", UriKind.RelativeOrAbsolute));
                        break;

                    case 2:
                        imgb_c_mode_3_a42_a.ImageSource = new BitmapImage(new Uri(".\\Image\\ZOO_L_E_A_42_12.png", UriKind.RelativeOrAbsolute));
                        break;

                    case 3:
                        imgb_c_mode_3_a42_a.ImageSource = new BitmapImage(new Uri(".\\Image\\ZOO_L_E_A_42_13.png", UriKind.RelativeOrAbsolute));
                        break;

                    case 4:
                        imgb_c_mode_3_a42_a.ImageSource = new BitmapImage(new Uri(".\\Image\\ZOO_L_E_A_42_14.png", UriKind.RelativeOrAbsolute));
                        break;

                    case 5:
                        imgb_c_mode_3_a42_a.ImageSource = new BitmapImage(new Uri(".\\Image\\ZOO_L_E_A_42_15.png", UriKind.RelativeOrAbsolute));
                        break;
                }
            }

            spnlMode3A42_A.Visibility = Visibility.Visible;
            spnlMode3A42_A.Background = imgb_c_mode_3_a42_a;
        }

        // 顯示 Mode 1.5 坐
        private void Show_Mode15_A50_1()
        {
            // 顯示圖層
            ImageBrush imgb_c_mode_15_a50 = new();

            // 中英文之圖型
            if (languageModel == LanguageMode.Chinese)
            {
                imgb_c_mode_15_a50.ImageSource = new BitmapImage(new Uri(".\\Image\\ZOO_A_50_1.png", UriKind.RelativeOrAbsolute));
            }
            else
            {
                imgb_c_mode_15_a50.ImageSource = new BitmapImage(new Uri(".\\Image\\ZOO_E_A_50_1.png", UriKind.RelativeOrAbsolute));
            }

            spnlMode15A50.Visibility = Visibility.Visible;
            spnlMode15A50.Background = imgb_c_mode_15_a50;
        }

        // 顯示 Mode 2 坐 Q
        private void Show_Mode2_A51_1()
        {
            // 設定問題的初始位置
            int QPosX = 50;
            int QPosY = 10;

            // 顯示圖層
            ImageBrush imgb_c_mode_2_a51 = new();

            // 中英文之圖型
            if (languageModel == LanguageMode.Chinese)
            {
                imgb_c_mode_2_a51.ImageSource = new BitmapImage(new Uri(".\\Image\\ZOO_A_51_1.png", UriKind.RelativeOrAbsolute));
            }
            else
            {
                imgb_c_mode_2_a51.ImageSource = new BitmapImage(new Uri(".\\Image\\ZOO_E_A_51_1.png", UriKind.RelativeOrAbsolute));
            }

            spnlMode2A51.Visibility = Visibility.Visible;
            spnlMode2A51.Background = imgb_c_mode_2_a51;

            // 隨機五選三個值
            int[] selectedNumbers = select3by5();

            // Q1
            if (Array.IndexOf(selectedNumbers, 1) != -1)
            {
                ImageBrush imgb_c_mode_2_a51_q1 = new();

                if (languageModel == LanguageMode.Chinese)
                {
                    imgb_c_mode_2_a51_q1.ImageSource = new BitmapImage(new Uri(".\\Image\\ZOO_A_51_11.png", UriKind.RelativeOrAbsolute));
                }
                else
                {
                    imgb_c_mode_2_a51_q1.ImageSource = new BitmapImage(new Uri(".\\Image\\ZOO_E_A_51_11.png", UriKind.RelativeOrAbsolute));
                }

                // 設定問題位置
                Canvas.SetLeft(spnlMode2A51_Q1, QPosX);
                Canvas.SetTop(spnlMode2A51_Q1, QPosY);

                // 設定下一個問題位置
                QPosY += 70;

                spnlMode2A51_Q1.Visibility = Visibility.Visible;
                spnlMode2A51_Q1.Background = imgb_c_mode_2_a51_q1;
            }
            else
            {
                spnlMode2A51_Q1.Visibility = Visibility.Collapsed;
            }

            // Q2
            if (Array.IndexOf(selectedNumbers, 2) != -1)
            {
                ImageBrush imgb_c_mode_2_a51_q2 = new();

                if (languageModel == LanguageMode.Chinese)
                {
                    imgb_c_mode_2_a51_q2.ImageSource = new BitmapImage(new Uri(".\\Image\\ZOO_A_51_12.png", UriKind.RelativeOrAbsolute));
                }
                else
                {
                    imgb_c_mode_2_a51_q2.ImageSource = new BitmapImage(new Uri(".\\Image\\ZOO_E_A_51_12.png", UriKind.RelativeOrAbsolute));
                }

                Canvas.SetLeft(spnlMode2A51_Q2, QPosX);
                Canvas.SetTop(spnlMode2A51_Q2, QPosY);

                // 設定下一個問題位置
                QPosY += 70;

                spnlMode2A51_Q2.Visibility = Visibility.Visible;
                spnlMode2A51_Q2.Background = imgb_c_mode_2_a51_q2;
            }
            else
            {
                spnlMode2A51_Q2.Visibility = Visibility.Collapsed;
            }

            // Q3
            if (Array.IndexOf(selectedNumbers, 3) != -1)
            {
                ImageBrush imgb_c_mode_2_a51_q3 = new();

                if (languageModel == LanguageMode.Chinese)
                {
                    imgb_c_mode_2_a51_q3.ImageSource = new BitmapImage(new Uri(".\\Image\\ZOO_A_51_13.png", UriKind.RelativeOrAbsolute));
                }
                else
                {
                    imgb_c_mode_2_a51_q3.ImageSource = new BitmapImage(new Uri(".\\Image\\ZOO_E_A_51_13.png", UriKind.RelativeOrAbsolute));
                }

                Canvas.SetLeft(spnlMode2A51_Q3, QPosX);
                Canvas.SetTop(spnlMode2A51_Q3, QPosY);

                // 設定下一個問題位置
                QPosY += 70;

                spnlMode2A51_Q3.Visibility = Visibility.Visible;
                spnlMode2A51_Q3.Background = imgb_c_mode_2_a51_q3;
            }
            else
            {
                spnlMode2A51_Q3.Visibility = Visibility.Collapsed;
            }

            // Q4
            if (Array.IndexOf(selectedNumbers, 4) != -1)
            {
                ImageBrush imgb_c_mode_2_a51_q4 = new();

                if (languageModel == LanguageMode.Chinese)
                {
                    imgb_c_mode_2_a51_q4.ImageSource = new BitmapImage(new Uri(".\\Image\\ZOO_A_51_14.png", UriKind.RelativeOrAbsolute));
                }
                else
                {
                    imgb_c_mode_2_a51_q4.ImageSource = new BitmapImage(new Uri(".\\Image\\ZOO_E_A_51_14.png", UriKind.RelativeOrAbsolute));
                }

                Canvas.SetLeft(spnlMode2A51_Q4, QPosX);
                Canvas.SetTop(spnlMode2A51_Q4, QPosY);

                // 設定下一個問題位置
                QPosY += 70;

                spnlMode2A51_Q4.Visibility = Visibility.Visible;
                spnlMode2A51_Q4.Background = imgb_c_mode_2_a51_q4;
            }
            else
            {
                spnlMode2A51_Q4.Visibility = Visibility.Collapsed;
            }

            // Q5
            if (Array.IndexOf(selectedNumbers, 5) != -1)
            {
                ImageBrush imgb_c_mode_2_a51_q5 = new();

                if (languageModel == LanguageMode.Chinese)
                {
                    imgb_c_mode_2_a51_q5.ImageSource = new BitmapImage(new Uri(".\\Image\\ZOO_A_51_15.png", UriKind.RelativeOrAbsolute));
                }
                else
                {
                    imgb_c_mode_2_a51_q5.ImageSource = new BitmapImage(new Uri(".\\Image\\ZOO_E_A_51_15.png", UriKind.RelativeOrAbsolute));
                }

                Canvas.SetLeft(spnlMode2A51_Q5, QPosX);
                Canvas.SetTop(spnlMode2A51_Q5, QPosY);

                // 設定下一個問題位置
                QPosY += 70;

                spnlMode2A51_Q5.Visibility = Visibility.Visible;
                spnlMode2A51_Q5.Background = imgb_c_mode_2_a51_q5;
            }
            else
            {
                spnlMode2A51_Q5.Visibility = Visibility.Collapsed;
            }
        }

        // 顯示 Mode 3 坐 A
        private void Show_Mode3_A52_1(int args)
        {
            // 顯示圖層
            ImageBrush imgb_c_mode_3_a52 = new();
            imgb_c_mode_3_a52.Stretch = Stretch.Uniform;
            imgb_c_mode_3_a52.AlignmentX = AlignmentX.Center;
            imgb_c_mode_3_a52.AlignmentY = AlignmentY.Center;

            if (languageModel == LanguageMode.Chinese)
            {
                imgb_c_mode_3_a52.ImageSource = new BitmapImage(new Uri(".\\Image\\ZOO_A_52_1.png", UriKind.RelativeOrAbsolute));
            }
            else
            {
                imgb_c_mode_3_a52.ImageSource = new BitmapImage(new Uri(".\\Image\\ZOO_E_A_52_1.png", UriKind.RelativeOrAbsolute));
            }

            spnlMode3A52.Visibility = Visibility.Visible;
            spnlMode3A52.Background = imgb_c_mode_3_a52;

            // 顯示答案
            ImageBrush imgb_c_mode_3_a52_a = new();

            if (languageModel == LanguageMode.Chinese)
            {
                switch (args)
                {
                    case 1:
                        imgb_c_mode_3_a52_a.ImageSource = new BitmapImage(new Uri(".\\Image\\ZOO_R_A_52_11.png", UriKind.RelativeOrAbsolute));
                        break;

                    case 2:
                        imgb_c_mode_3_a52_a.ImageSource = new BitmapImage(new Uri(".\\Image\\ZOO_R_A_52_12.png", UriKind.RelativeOrAbsolute));
                        break;

                    case 3:
                        imgb_c_mode_3_a52_a.ImageSource = new BitmapImage(new Uri(".\\Image\\ZOO_R_A_52_13.png", UriKind.RelativeOrAbsolute));
                        break;

                    case 4:
                        imgb_c_mode_3_a52_a.ImageSource = new BitmapImage(new Uri(".\\Image\\ZOO_R_A_52_14.png", UriKind.RelativeOrAbsolute));
                        break;

                    case 5:
                        imgb_c_mode_3_a52_a.ImageSource = new BitmapImage(new Uri(".\\Image\\ZOO_R_A_52_15.png", UriKind.RelativeOrAbsolute));
                        break;
                }
            }
            else
            {
                switch (args)
                {
                    case 1:
                        imgb_c_mode_3_a52_a.ImageSource = new BitmapImage(new Uri(".\\Image\\ZOO_R_E_A_52_11.png", UriKind.RelativeOrAbsolute));
                        break;

                    case 2:
                        imgb_c_mode_3_a52_a.ImageSource = new BitmapImage(new Uri(".\\Image\\ZOO_R_E_A_52_12.png", UriKind.RelativeOrAbsolute));
                        break;

                    case 3:
                        imgb_c_mode_3_a52_a.ImageSource = new BitmapImage(new Uri(".\\Image\\ZOO_R_E_A_52_13.png", UriKind.RelativeOrAbsolute));
                        break;

                    case 4:
                        imgb_c_mode_3_a52_a.ImageSource = new BitmapImage(new Uri(".\\Image\\ZOO_R_E_A_52_14.png", UriKind.RelativeOrAbsolute));
                        break;

                    case 5:
                        imgb_c_mode_3_a52_a.ImageSource = new BitmapImage(new Uri(".\\Image\\ZOO_R_E_A_52_15.png", UriKind.RelativeOrAbsolute));
                        break;
                }
            }

            spnlMode3A52_A.Visibility = Visibility.Visible;
            spnlMode3A52_A.Background = imgb_c_mode_3_a52_a;
        }

        // 顯示 Mode 1.5 黑熊花絮
        private void Show_Mode15_B10()
        {
            // 顯示圖層
            ImageBrush imgb_c_mode_15_b10 = new();

            // 中英文之圖型
            if (languageModel == LanguageMode.Chinese)
            {
                imgb_c_mode_15_b10.ImageSource = new BitmapImage(new Uri(".\\Image\\ZOO_A_10.png", UriKind.RelativeOrAbsolute));
            }
            else
            {
                imgb_c_mode_15_b10.ImageSource = new BitmapImage(new Uri(".\\Image\\ZOO_E_A_10.png", UriKind.RelativeOrAbsolute));
            }

            spnlMode15B10.Visibility = Visibility.Visible;
            spnlMode15B10.Background = imgb_c_mode_15_b10;
        }

        // 動物園科普 - args 會傳入 1, 2, 3, 5
        private void Show_Mode2_B20(int args)
        {
            // 顯示底層
            ImageBrush imgb_c_mode_02_b20 = new();

            // 動畫路徑
            Uri? ModeB20MoviePath = null;

            // 中英文之圖型
            if (languageModel == LanguageMode.Chinese)
            {
                imgb_c_mode_02_b20.ImageSource = new BitmapImage(new Uri(".\\Image\\ZOO_B_20_1.png", UriKind.RelativeOrAbsolute));

                // 顯示動畫部份
                switch (args)
                {
                    case 1:
                        ModeB20MoviePath = new Uri(".\\Image\\ZOO_B_20_1_mp4.mp4", UriKind.RelativeOrAbsolute);
                        break;

                    case 2:
                        ModeB20MoviePath = new Uri(".\\Image\\ZOO_B_20_2_mp4.mp4", UriKind.RelativeOrAbsolute);
                        break;

                    case 3:
                        ModeB20MoviePath = new Uri(".\\Image\\ZOO_B_20_3_mp4.mp4", UriKind.RelativeOrAbsolute);
                        break;

                    case 5:
                        ModeB20MoviePath = new Uri(".\\Image\\ZOO_B_20_4_mp4.mp4", UriKind.RelativeOrAbsolute);
                        break;
                }
            }
            else
            {
                imgb_c_mode_02_b20.ImageSource = new BitmapImage(new Uri(".\\Image\\ZOO_E_B_20_1.png", UriKind.RelativeOrAbsolute));

                // 顯示動畫部份
                switch (args)
                {
                    case 1:
                        ModeB20MoviePath = new Uri(".\\Image\\ZOO_B_20_1_mp4.mp4", UriKind.RelativeOrAbsolute);
                        break;

                    case 2:
                        ModeB20MoviePath = new Uri(".\\Image\\ZOO_B_20_2_mp4.mp4", UriKind.RelativeOrAbsolute);
                        break;

                    case 3:
                        ModeB20MoviePath = new Uri(".\\Image\\ZOO_B_20_3_mp4.mp4", UriKind.RelativeOrAbsolute);
                        break;

                    case 5:
                        ModeB20MoviePath = new Uri(".\\Image\\ZOO_B_20_4_mp4.mp4", UriKind.RelativeOrAbsolute);
                        break;
                }
            }

            spnlMode2B20.Visibility = Visibility.Visible;
            spnlMode2B20.Background = imgb_c_mode_02_b20;

            // 顯示動畫部份
            mdeMoviePlayer_B20.Visibility = Visibility.Visible;
            mdeMoviePlayer_B20.Source = ModeB20MoviePath;
            mdeMoviePlayer_B20.Play();
        }

        // B20 Media Player
        private void mdeMoviePlayer_B20_MediaEnded(object sender, RoutedEventArgs e)
        {
            // 媒體播放結束時觸發這個事件，將媒體的位置設定為0，以重新開始播放
            mdeMoviePlayer_B20.Position = TimeSpan.Zero;
            mdeMoviePlayer_B20.Play();
        }

        // 花絮影片 - args 會傳入 8, 14, 15, 16
        private void Show_Mode2_B30(int args)
        {
            // 顯示底層
            ImageBrush imgb_c_mode_02_b30 = new();

            // 動畫路徑
            Uri? ModeB30MoviePath = null;

            // 中英文之圖型
            if (languageModel == LanguageMode.Chinese)
            {
                imgb_c_mode_02_b30.ImageSource = new BitmapImage(new Uri(".\\Image\\ZOO_B_30_1.png", UriKind.RelativeOrAbsolute));

                // 顯示動畫部份
                switch (args)
                {
                    case 8:
                        ModeB30MoviePath = new Uri(".\\Image\\ZOO_B_30_05.mp4", UriKind.RelativeOrAbsolute);
                        break;

                    case 14:
                        ModeB30MoviePath = new Uri(".\\Image\\ZOO_B_30_07.mp4", UriKind.RelativeOrAbsolute);
                        break;

                    case 15:
                        ModeB30MoviePath = new Uri(".\\Image\\ZOO_B_30_08.mp4", UriKind.RelativeOrAbsolute);
                        break;

                    case 16:
                        ModeB30MoviePath = new Uri(".\\Image\\ZOO_B_30_06.mp4", UriKind.RelativeOrAbsolute);
                        break;
                }
            }
            else
            {
                imgb_c_mode_02_b30.ImageSource = new BitmapImage(new Uri(".\\Image\\ZOO_E_B_30_1.png", UriKind.RelativeOrAbsolute));

                // 顯示動畫部份
                switch (args)
                {
                    case 8:
                        ModeB30MoviePath = new Uri(".\\Image\\ZOO_E_B_30_05.mp4", UriKind.RelativeOrAbsolute);
                        break;

                    case 14:
                        ModeB30MoviePath = new Uri(".\\Image\\ZOO_E_B_30_07.mp4", UriKind.RelativeOrAbsolute);
                        break;

                    case 15:
                        ModeB30MoviePath = new Uri(".\\Image\\ZOO_E_B_30_08.mp4", UriKind.RelativeOrAbsolute);
                        break;

                    case 16:
                        ModeB30MoviePath = new Uri(".\\Image\\ZOO_E_B_30_06.mp4", UriKind.RelativeOrAbsolute);
                        break;
                }
            }

            spnlMode2B30.Visibility = Visibility.Visible;
            spnlMode2B30.Background = imgb_c_mode_02_b30;

            // 顯示動畫部份
            mdeMoviePlayer_B30.Visibility = Visibility.Visible;
            mdeMoviePlayer_B30.Source = ModeB30MoviePath;
            mdeMoviePlayer_B30.Play();
        }

        // B30 Media Player
        private void mdeMoviePlayer_B30_MediaEnded(object sender, RoutedEventArgs e)
        {
            // 媒體播放結束時觸發這個事件，將媒體的位置設定為0，以重新開始播放
            mdeMoviePlayer_B30.Position = TimeSpan.Zero;
            mdeMoviePlayer_B30.Play();
        }

        // 顯示 Map 圖層
        private void Show_Mode2_Map()
        {
            // 顯示底層
            ImageBrush imgb_c_mode_2_map = new();

            // 中英文之圖型
            if (languageModel == LanguageMode.Chinese)
            {
                imgb_c_mode_2_map.ImageSource = new BitmapImage(new Uri(".\\Image\\ZOO_map_10.png", UriKind.RelativeOrAbsolute));
            }
            else
            {
                imgb_c_mode_2_map.ImageSource = new BitmapImage(new Uri(".\\Image\\ZOO_E_map_10.png", UriKind.RelativeOrAbsolute));
            }

            spnlMode2Map.Visibility = Visibility.Visible;
            spnlMode2Map.Background = imgb_c_mode_2_map;
        }

        // 顯示 Live 圖層
        private void Show_Mode2_Live()
        {
            // 顯示底層
            ImageBrush imgb_c_mode_2_live = new();

            // 中英文之圖型
            if (languageModel == LanguageMode.Chinese)
            {
                imgb_c_mode_2_live.ImageSource = new BitmapImage(new Uri(".\\Image\\ZOO_live_10.png", UriKind.RelativeOrAbsolute));
            }
            else
            {
                imgb_c_mode_2_live.ImageSource = new BitmapImage(new Uri(".\\Image\\ZOO_E_live_10.png", UriKind.RelativeOrAbsolute));
            }

            spnlMode2Live.Visibility = Visibility.Visible;
            spnlMode2Live.Background = imgb_c_mode_2_live;

            cnsLive.Visibility = Visibility.Visible;

            // 顯示 RTSP 內容
            // vdwMode2Live.Visibility = Visibility.Visible;

            // 使用 AR 模組傳來的 live_cam_id
            if ((bearObject == null) || (bearObject.LiveCamId == "1"))
            {
                // 當 LiveCamId == "1" 時，代表 1 - 8 沒有熊，但 9 號仍可能有，所以還要判斷一下
                dynamic cameraData = bearObject.DataArray[11];

                // No.9 攝影機是放映者
                if ((cameraData != null) && (cameraData[0] >= majorMethod.confThresholdList[8]))
                {
                    playRTSPStream(8);
                }
                else
                {
                    playRTSPStream(0);
                }
            }
            else
            {
                playRTSPStream(int.Parse(bearObject.LiveCamId) - 1);
            }
        }

        public void playRTSPStream(int index)
        {
            // 相同的就不變
            //if (index == liveCamIndex)
            //    return;

            // 隱藏所有畫面
            hideAllRTSPStream();

            //try
            //{
            //    // 如果當前有顯示的串流，先解除綁定
            //    if (liveCamIndex >= 0)
            //    {
            //        mainVideoView.MediaPlayer = null;
            //    }

            //    // 設定新的串流
            //    mainVideoView.MediaPlayer = _mediaPlayers[index];
            //    // _currentStreamIndex = index;

            //    // statusText.Text = $"目前顯示: Camera {index + 1}";

            //    // 紀錄目前的畫面編號
            //    liveCamIndex = index;
            //}
            //catch (Exception ex)
            //{
            //    Debug.WriteLine($"切換串流時發生錯誤: {ex.Message}");
            //}

            // 紀錄目前的畫面編號
            liveCamIndex = index;

            // 根據 camIndex 選擇相機
            switch (liveCamIndex)
            {
                //case 0:
                //    vdwMode2LiveCam0.Visibility = Visibility.Visible;
                //    break;

                //case 1:
                //    vdwMode2LiveCam1.Visibility = Visibility.Visible;
                //    break;

                //case 2:
                //    vdwMode2LiveCam2.Visibility = Visibility.Visible;
                //    break;

                //case 3:
                //    vdwMode2LiveCam3.Visibility = Visibility.Visible;
                //    break;

                //case 4:
                //    vdwMode2LiveCam4.Visibility = Visibility.Visible;
                //    break;

                //case 5:
                //    vdwMode2LiveCam5.Visibility = Visibility.Visible;
                //    break;

                //case 6:
                //    vdwMode2LiveCam6.Visibility = Visibility.Visible;
                //    break;

                //case 7:
                //    vdwMode2LiveCam7.Visibility = Visibility.Visible;
                //    break;

                //case 8:
                //    vdwMode2LiveCam8.Visibility = Visibility.Visible;
                //    break;

                case 100:
                    break;
            }
        }

        // 盲區 SP_02
        private void Show_ModeSP_02()
        {
        }

        // 盲區 SP_03
        private void Show_ModeSP_03()
        {
        }

        // 盲區 SP_04
        private void Show_ModeSP_04()
        {
        }

        // 盲區 SP_05
        private void Show_ModeSP_05()
        {
        }

        // 盲區 SP_06
        private void Show_ModeSP_06()
        {
        }

        // 盲區 SP_07
        private void Show_ModeSP_07()
        {
        }

        // 盲區 SP_09
        private void Show_ModeSP_09()
        {
        }

        // 盲區 SP_10
        private void Show_ModeSP_10()
        {
        }

        // 盲區 SP_11
        private void Show_ModeSP_11()
        {
        }

        // 盲區 SP_12
        private void Show_ModeSP_12()
        {
        }

        private void hideAllRTSPStream()
        {
        }

        // 相機重新連接
        public void ReConnectVideoCam()
        {
            // 測試用，先停 3 秒
            Thread.Sleep(3000);
        }

        // 建立與場域攝影機的連結
        //public async Task<int> ConnectVideoCam()
        public void ConnectVideoCam()
        {
            try
            {
                
            }
            catch (Exception ex)
            {
                TextLog.WriteLog($"發生即時影像撥放錯誤!! {ex.Message}");
            }

        }

        private void grdMain_MouseDown(object sender, MouseButtonEventArgs e)
        {
            // 獲取相對於 Grid 的座標
            Point position = e.GetPosition((UIElement)sender);
            int x = (int)position.X;
            int y = (int)position.Y;

            // MainForm 上顯示座標
            mainWin?.WriteMonitorLog($"Mouse clicked at: X={x}, Y={y}");

            // 測試用，右下角一律回首頁
            //if ((x >= 850) && (x <= 960) && (y >= 470) && (y <= 540))
            //{
            //    SetScreenMode(ModeKind.Mode1);
            //    return;
            //}

            // 進行座標判斷，與模式轉換
            switch (currentMode)
            {
                // 空模式
                case ModeKind.None:
                    break;

                // 首頁
                case ModeKind.Mode1:

                    // 按下小地圖時
                    if ((x >= 0) && (x <= 160) && (y >= 450) && (y <= 540)) 
                    {
                        SetScreenMode(ModeKind.Mode2_MAP_10);
                        break;
                    }

                    // 按下 Live 時
                    if ((x >=175) && (x <= 330) && (y >= 450) && (y <= 540))
                    {
                        SetScreenMode(ModeKind.Mode2_LIVE_10);
                        break;
                    }

                    // 按下中英文切換位置時
                    if ((x >= 880) && (x <= 960) && (y >= 450) && (y <= 540))
                    {
                        // 中英文紀錄
                        SetLanguageMode(languageModel);

                        // 更新本頁畫面
                        Show_Mode1();

                        break;
                    }

                    // 按下圈圈時，切換到發現熊 - 站立(此為測試用)
                    // SetScreenMode(ModeKind.Mode15_A20_1);

                    break;

                // 站立系列
                case ModeKind.Mode15_A20_1:

                    // 按下小地圖時
                    if ((x >= 0) && (x <= 160) && (y >= 450) && (y <= 540))
                    {
                        SetScreenMode(ModeKind.Mode2_MAP_10);
                        break;
                    }

                    // 按下 Live 時
                    if ((x >= 175) && (x <= 330) && (y >= 450) && (y <= 540))
                    {
                        SetScreenMode(ModeKind.Mode2_LIVE_10);
                        break;
                    }

                    // 按下中英文切換位置時
                    if ((x >= 880) && (x <= 960) && (y >= 450) && (y <= 540))
                    {
                        // 中英文紀錄
                        SetLanguageMode(languageModel);

                        // 更新本頁畫面
                        Show_Mode15_A20_1();

                        break;
                    }

                    // 按下問問黑熊
                    if ((x >= 80) && (x <= 880) && (y >= 80) && (y <= 400))
                    {
                        SetScreenMode(ModeKind.Mode2_A21_1);
                        break;
                    }

                    break;

                case ModeKind.Mode2_A21_1:

                    // 問問黑熊的題庫，直接用 StackPanel 的 OnClick 來抓事件

                    // 右下角回首頁
                    if ((x >= 850) && (x <= 960) && (y >= 470) && (y <= 540))
                    {
                        SetScreenMode(ModeKind.Mode0);
                        return;
                    }

                    break;

                case ModeKind.Mode3_A22_1:

                    // 右下角回首頁
                    if ((x >= 850) && (x <= 960) && (y >= 470) && (y <= 540))
                    {
                        SetScreenMode(ModeKind.Mode0);
                        return;
                    }

                    break;

                // 趴臥系列
                case ModeKind.Mode15_A30_1:

                    // 按下小地圖時
                    if ((x >= 0) && (x <= 160) && (y >= 450) && (y <= 540))
                    {
                        SetScreenMode(ModeKind.Mode2_MAP_10);
                        break;
                    }

                    // 按下 Live 時
                    if ((x >= 175) && (x <= 330) && (y >= 450) && (y <= 540))
                    {
                        SetScreenMode(ModeKind.Mode2_LIVE_10);
                        break;
                    }

                    // 按下中英文切換位置時
                    if ((x >= 880) && (x <= 960) && (y >= 450) && (y <= 540))
                    {
                        // 中英文紀錄
                        SetLanguageMode(languageModel);

                        // 更新本頁畫面
                        Show_Mode15_A30_1();

                        break;
                    }

                    // 按下問問黑熊
                    if ((x >= 80) && (x <= 880) && (y >= 80) && (y <= 400))
                    {
                        SetScreenMode(ModeKind.Mode2_A31_1);
                        break;
                    }

                    break;

                case ModeKind.Mode2_A31_1:

                    // 右下角回首頁
                    if ((x >= 850) && (x <= 960) && (y >= 470) && (y <= 540))
                    {
                        SetScreenMode(ModeKind.Mode0);
                        return;
                    }

                    break;

                case ModeKind.Mode3_A32_1:

                    // 右下角回首頁
                    if ((x >= 850) && (x <= 960) && (y >= 470) && (y <= 540))
                    {
                        SetScreenMode(ModeKind.Mode0);
                        return;
                    }

                    break;

                // 四足系列
                case ModeKind.Mode15_A40_1:

                    // 按下小地圖時
                    if ((x >= 0) && (x <= 160) && (y >= 450) && (y <= 540))
                    {
                        SetScreenMode(ModeKind.Mode2_MAP_10);
                        break;
                    }

                    // 按下 Live 時
                    if ((x >= 175) && (x <= 330) && (y >= 450) && (y <= 540))
                    {
                        SetScreenMode(ModeKind.Mode2_LIVE_10);
                        break;
                    }

                    // 按下中英文切換位置時
                    if ((x >= 880) && (x <= 960) && (y >= 450) && (y <= 540))
                    {
                        // 中英文紀錄
                        SetLanguageMode(languageModel);

                        // 更新本頁畫面
                        Show_Mode15_A40_1();

                        break;
                    }

                    // 按下問問黑熊
                    if ((x >= 80) && (x <= 880) && (y >= 80) && (y <= 400))
                    {
                        SetScreenMode(ModeKind.Mode2_A41_1);
                        break;
                    }

                    break;

                case ModeKind.Mode2_A41_1:

                    // 右下角回首頁
                    if ((x >= 850) && (x <= 960) && (y >= 470) && (y <= 540))
                    {
                        SetScreenMode(ModeKind.Mode0);
                        return;
                    }

                    break;

                case ModeKind.Mode3_A42_1:

                    // 右下角回首頁
                    if ((x >= 850) && (x <= 960) && (y >= 470) && (y <= 540))
                    {
                        SetScreenMode(ModeKind.Mode0);
                        return;
                    }

                    break;

                // 坐 系列
                case ModeKind.Mode15_A50_1:

                    // 按下小地圖時
                    if ((x >= 0) && (x <= 160) && (y >= 450) && (y <= 540))
                    {
                        SetScreenMode(ModeKind.Mode2_MAP_10);
                        break;
                    }

                    // 按下 Live 時
                    if ((x >= 175) && (x <= 330) && (y >= 450) && (y <= 540))
                    {
                        SetScreenMode(ModeKind.Mode2_LIVE_10);
                        break;
                    }

                    // 按下中英文切換位置時
                    if ((x >= 880) && (x <= 960) && (y >= 450) && (y <= 540))
                    {
                        // 中英文紀錄
                        SetLanguageMode(languageModel);

                        // 更新本頁畫面
                        Show_Mode15_A50_1();

                        break;
                    }

                    // 按下問問黑熊
                    if ((x >= 80) && (x <= 880) && (y >= 80) && (y <= 400))
                    {
                        SetScreenMode(ModeKind.Mode2_A51_1);
                        break;
                    }

                    break;

                case ModeKind.Mode2_A51_1:

                    // 右下角回首頁
                    if ((x >= 850) && (x <= 960) && (y >= 470) && (y <= 540))
                    {
                        SetScreenMode(ModeKind.Mode0);
                        return;
                    }

                    break;

                case ModeKind.Mode3_A52_1:

                    // 右下角回首頁
                    if ((x >= 850) && (x <= 960) && (y >= 470) && (y <= 540))
                    {
                        SetScreenMode(ModeKind.Mode0);
                        return;
                    }

                    break;

                // 花絮部分
                case ModeKind.Mode15_B10:

                    // 按下小地圖時
                    if ((x >= 0) && (x <= 160) && (y >= 450) && (y <= 540))
                    {
                        SetScreenMode(ModeKind.Mode2_MAP_10);
                        break;
                    }

                    // 按下 Live 時
                    if ((x >= 175) && (x <= 330) && (y >= 450) && (y <= 540))
                    {
                        SetScreenMode(ModeKind.Mode2_LIVE_10);
                        break;
                    }

                    // 按下中英文切換位置時
                    if ((x >= 880) && (x <= 960) && (y >= 450) && (y <= 540))
                    {
                        // 中英文紀錄
                        SetLanguageMode(languageModel);

                        // 更新本頁畫面
                        Show_Mode15_B10();

                        break;
                    }

                    break;

                case ModeKind.Mode2_B20_1:

                    // 右下角回首頁
                    if ((x >= 850) && (x <= 960) && (y >= 470) && (y <= 540))
                    {
                        SetScreenMode(ModeKind.Mode0);
                        return;
                    }

                    break;

                case ModeKind.Mode2_B30_1:

                    // 右下角回首頁
                    if ((x >= 850) && (x <= 960) && (y >= 470) && (y <= 540))
                    {
                        SetScreenMode(ModeKind.Mode0);
                        return;
                    }

                    break;

                // 小地圖
                case ModeKind.Mode2_MAP_10:

                    // 右下角回首頁
                    if ((x >= 850) && (x <= 960) && (y >= 470) && (y <= 540))
                    {
                        SetScreenMode(ModeKind.Mode0);
                        return;
                    }
                    break;

                // Live 影像
                case ModeKind.Mode2_LIVE_10:

                    // 右下角回首頁
                    if ((x >= 850) && (x <= 960) && (y >= 470) && (y <= 540))
                    {
                        SetScreenMode(ModeKind.Mode0);
                        return;
                    }

                    // 按鈕 1 的位置
                    if ((x >= 148) && (x <= 213) && (y >= 440) && (y <= 515))
                    {
                        playRTSPStream(0);
                        break;
                    }

                    // 按鈕 2 的位置
                    if ((x >= 223) && (x <= 288) && (y >= 440) && (y <= 515))
                    {
                        playRTSPStream(1);
                        break;
                    }

                    // 按鈕 3 的位置
                    if ((x >= 298) && (x <= 363) && (y >= 440) && (y <= 515))
                    {
                        playRTSPStream(2);
                        break;
                    }

                    // 按鈕 4 的位置
                    if ((x >= 373) && (x <= 438) && (y >= 440) && (y <= 515))
                    {
                        playRTSPStream(3);
                        break;
                    }

                    // 按鈕 5 的位置
                    if ((x >= 448) && (x <= 513) && (y >= 440) && (y <= 515))
                    {
                        playRTSPStream(4);
                        break;
                    }

                    // 按鈕 6 的位置
                    if ((x >= 523) && (x <= 588) && (y >= 440) && (y <= 515))
                    {
                        playRTSPStream(5);
                        break;
                    }

                    // 按鈕 7 的位置
                    if ((x >= 598) && (x <= 663) && (y >= 440) && (y <= 515))
                    {
                        playRTSPStream(6);
                        break;
                    }

                    // 按鈕 8 的位置
                    if ((x >= 673) && (x <= 738) && (y >= 440) && (y <= 515))
                    {
                        playRTSPStream(7);
                        break;
                    }

                    // 按鈕 9 的位置
                    if ((x >= 748) && (x <= 813) && (y >= 440) && (y <= 515))
                    {
                        playRTSPStream(8);
                        break;
                    }

                    break;

                // 盲區部分
                case ModeKind.ModeSP_01:
                    break;

                case ModeKind.ModeSP_02:

                    // 按下小地圖時
                    if ((x >= 0) && (x <= 160) && (y >= 450) && (y <= 540))
                    {
                        SetScreenMode(ModeKind.Mode2_MAP_10);
                        break;
                    }

                    // 按下 Live 時
                    if ((x >= 175) && (x <= 330) && (y >= 450) && (y <= 540))
                    {
                        SetScreenMode(ModeKind.Mode2_LIVE_10);
                        break;
                    }

                    // 按下中英文切換位置時
                    if ((x >= 880) && (x <= 960) && (y >= 450) && (y <= 540))
                    {
                        // 中英文紀錄
                        SetLanguageMode(languageModel);

                        // 更新本頁畫面
                        Show_ModeSP_02();

                        break;
                    }

                    break;

                case ModeKind.ModeSP_03:

                    // 按下小地圖時
                    if ((x >= 0) && (x <= 160) && (y >= 450) && (y <= 540))
                    {
                        SetScreenMode(ModeKind.Mode2_MAP_10);
                        break;
                    }

                    // 按下 Live 時
                    if ((x >= 175) && (x <= 330) && (y >= 450) && (y <= 540))
                    {
                        SetScreenMode(ModeKind.Mode2_LIVE_10);
                        break;
                    }

                    // 按下中英文切換位置時
                    if ((x >= 880) && (x <= 960) && (y >= 450) && (y <= 540))
                    {
                        // 中英文紀錄
                        SetLanguageMode(languageModel);

                        // 更新本頁畫面
                        Show_ModeSP_03();

                        break;
                    }

                    break;

                case ModeKind.ModeSP_04:

                    // 按下小地圖時
                    if ((x >= 0) && (x <= 160) && (y >= 450) && (y <= 540))
                    {
                        SetScreenMode(ModeKind.Mode2_MAP_10);
                        break;
                    }

                    // 按下 Live 時
                    if ((x >= 175) && (x <= 330) && (y >= 450) && (y <= 540))
                    {
                        SetScreenMode(ModeKind.Mode2_LIVE_10);
                        break;
                    }

                    // 按下中英文切換位置時
                    if ((x >= 880) && (x <= 960) && (y >= 450) && (y <= 540))
                    {
                        // 中英文紀錄
                        SetLanguageMode(languageModel);

                        // 更新本頁畫面
                        Show_ModeSP_04();

                        break;
                    }

                    break;

                case ModeKind.ModeSP_05:

                    // 按下小地圖時
                    if ((x >= 0) && (x <= 160) && (y >= 450) && (y <= 540))
                    {
                        SetScreenMode(ModeKind.Mode2_MAP_10);
                        break;
                    }

                    // 按下 Live 時
                    if ((x >= 175) && (x <= 330) && (y >= 450) && (y <= 540))
                    {
                        SetScreenMode(ModeKind.Mode2_LIVE_10);
                        break;
                    }

                    // 按下中英文切換位置時
                    if ((x >= 880) && (x <= 960) && (y >= 450) && (y <= 540))
                    {
                        // 中英文紀錄
                        SetLanguageMode(languageModel);

                        // 更新本頁畫面
                        Show_ModeSP_05();

                        break;
                    }

                    break;

                case ModeKind.ModeSP_06:

                    // 按下小地圖時
                    if ((x >= 0) && (x <= 160) && (y >= 450) && (y <= 540))
                    {
                        SetScreenMode(ModeKind.Mode2_MAP_10);
                        break;
                    }

                    // 按下 Live 時
                    if ((x >= 175) && (x <= 330) && (y >= 450) && (y <= 540))
                    {
                        SetScreenMode(ModeKind.Mode2_LIVE_10);
                        break;
                    }

                    // 按下中英文切換位置時
                    if ((x >= 880) && (x <= 960) && (y >= 450) && (y <= 540))
                    {
                        // 中英文紀錄
                        SetLanguageMode(languageModel);

                        // 更新本頁畫面
                        Show_ModeSP_06();

                        break;
                    }

                    break;

                case ModeKind.ModeSP_07:

                    // 按下小地圖時
                    if ((x >= 0) && (x <= 160) && (y >= 450) && (y <= 540))
                    {
                        SetScreenMode(ModeKind.Mode2_MAP_10);
                        break;
                    }

                    // 按下 Live 時
                    if ((x >= 175) && (x <= 330) && (y >= 450) && (y <= 540))
                    {
                        SetScreenMode(ModeKind.Mode2_LIVE_10);
                        break;
                    }

                    // 按下中英文切換位置時
                    if ((x >= 880) && (x <= 960) && (y >= 450) && (y <= 540))
                    {
                        // 中英文紀錄
                        SetLanguageMode(languageModel);

                        // 更新本頁畫面
                        Show_ModeSP_07();

                        break;
                    }

                    break;

                case ModeKind.ModeSP_09:

                    // 按下小地圖時
                    if ((x >= 0) && (x <= 160) && (y >= 450) && (y <= 540))
                    {
                        SetScreenMode(ModeKind.Mode2_MAP_10);
                        break;
                    }

                    // 按下 Live 時
                    if ((x >= 175) && (x <= 330) && (y >= 450) && (y <= 540))
                    {
                        SetScreenMode(ModeKind.Mode2_LIVE_10);
                        break;
                    }

                    // 按下中英文切換位置時
                    if ((x >= 880) && (x <= 960) && (y >= 450) && (y <= 540))
                    {
                        // 中英文紀錄
                        SetLanguageMode(languageModel);

                        // 更新本頁畫面
                        Show_ModeSP_09();

                        break;
                    }

                    break;

                case ModeKind.ModeSP_10:

                    // 按下小地圖時
                    if ((x >= 0) && (x <= 160) && (y >= 450) && (y <= 540))
                    {
                        SetScreenMode(ModeKind.Mode2_MAP_10);
                        break;
                    }

                    // 按下 Live 時
                    if ((x >= 175) && (x <= 330) && (y >= 450) && (y <= 540))
                    {
                        SetScreenMode(ModeKind.Mode2_LIVE_10);
                        break;
                    }

                    // 按下中英文切換位置時
                    if ((x >= 880) && (x <= 960) && (y >= 450) && (y <= 540))
                    {
                        // 中英文紀錄
                        SetLanguageMode(languageModel);

                        // 更新本頁畫面
                        Show_ModeSP_10();

                        break;
                    }

                    break;

                case ModeKind.ModeSP_11:

                    // 按下小地圖時
                    if ((x >= 0) && (x <= 160) && (y >= 450) && (y <= 540))
                    {
                        SetScreenMode(ModeKind.Mode2_MAP_10);
                        break;
                    }

                    // 按下 Live 時
                    if ((x >= 175) && (x <= 330) && (y >= 450) && (y <= 540))
                    {
                        SetScreenMode(ModeKind.Mode2_LIVE_10);
                        break;
                    }

                    // 按下中英文切換位置時
                    if ((x >= 880) && (x <= 960) && (y >= 450) && (y <= 540))
                    {
                        // 中英文紀錄
                        SetLanguageMode(languageModel);

                        // 更新本頁畫面
                        Show_ModeSP_11();

                        break;
                    }

                    break;

                case ModeKind.ModeSP_12:

                    // 按下小地圖時
                    if ((x >= 0) && (x <= 160) && (y >= 450) && (y <= 540))
                    {
                        SetScreenMode(ModeKind.Mode2_MAP_10);
                        break;
                    }

                    // 按下 Live 時
                    if ((x >= 175) && (x <= 330) && (y >= 450) && (y <= 540))
                    {
                        SetScreenMode(ModeKind.Mode2_LIVE_10);
                        break;
                    }

                    // 按下中英文切換位置時
                    if ((x >= 880) && (x <= 960) && (y >= 450) && (y <= 540))
                    {
                        // 中英文紀錄
                        SetLanguageMode(languageModel);

                        // 更新本頁畫面
                        Show_ModeSP_12();

                        break;
                    }

                    break;
            }

            //// 如果需要獲取相對於整個視窗的座標
            //Point absolutePosition = e.GetPosition(this);
            //double absoluteX = absolutePosition.X;
            //double absoluteY = absolutePosition.Y;
        }

        // 2024/12/03 動畫功能
        private void MoveImageByAnimation(object Target, int NewXPos, int NewYPos, int MoveSeconds = 1)
        {
            // 建立移動物件的動畫
            var storyboard = new Storyboard();

            // X 座標動畫
            var animationX = new DoubleAnimation
            {
                To = NewXPos,
                Duration = TimeSpan.FromSeconds(MoveSeconds)
            };
            Storyboard.SetTarget(animationX, (DependencyObject)Target);
            Storyboard.SetTargetProperty(animationX, new PropertyPath(Canvas.LeftProperty));
            storyboard.Children.Add(animationX);

            // Y 座標動畫
            var animationY = new DoubleAnimation
            {
                To = NewYPos,
                Duration = TimeSpan.FromSeconds(MoveSeconds)
            };
            Storyboard.SetTarget(animationY, (DependencyObject)Target);
            Storyboard.SetTargetProperty(animationY, new PropertyPath(Canvas.TopProperty));
            storyboard.Children.Add(animationY);

            // 開始動畫
            storyboard.Begin();
        }

        // 隨機五選三
        private int[] select3by5()
        {
            // 創建一個包含5個數字的陣列
            int[] numbers = { 1, 2, 3, 4, 5 };

            // 創建一個隨機數生成器
            Random random = new Random();

            // 使用LINQ來隨機選擇3個數字
            return numbers.OrderBy(x => random.Next()).Take(3).ToArray();
        }

        // 站立熊的 5 個問題，按下之後
        private void spnlMode2A21_Q1_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            SetScreenMode(ModeKind.Mode3_A22_1, 1);
        }

        private void spnlMode2A21_Q2_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            SetScreenMode(ModeKind.Mode3_A22_1, 2);
        }

        private void spnlMode2A21_Q3_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            SetScreenMode(ModeKind.Mode3_A22_1, 3);
        }

        private void spnlMode2A21_Q4_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            SetScreenMode(ModeKind.Mode3_A22_1, 4);
        }

        private void spnlMode2A21_Q5_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            SetScreenMode(ModeKind.Mode3_A22_1, 5);
        }

        private void spnlMode2A31_Q1_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            SetScreenMode(ModeKind.Mode3_A32_1, 1);
        }

        private void spnlMode2A31_Q2_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            SetScreenMode(ModeKind.Mode3_A32_1, 2);
        }

        private void spnlMode2A31_Q3_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            SetScreenMode(ModeKind.Mode3_A32_1, 3);
        }

        private void spnlMode2A31_Q4_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            SetScreenMode(ModeKind.Mode3_A32_1, 4);
        }

        private void spnlMode2A31_Q5_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            SetScreenMode(ModeKind.Mode3_A32_1, 5);
        }

        private void spnlMode2A41_Q1_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            SetScreenMode(ModeKind.Mode3_A42_1, 1);
        }

        private void spnlMode2A41_Q2_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            SetScreenMode(ModeKind.Mode3_A42_1, 2);
        }

        private void spnlMode2A41_Q3_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            SetScreenMode(ModeKind.Mode3_A42_1, 3);
        }

        private void spnlMode2A41_Q4_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            SetScreenMode(ModeKind.Mode3_A42_1, 4);
        }

        private void spnlMode2A41_Q5_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            SetScreenMode(ModeKind.Mode3_A42_1, 5);
        }

        // 坐 的問題按鈕
        private void spnlMode2A51_Q1_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            SetScreenMode(ModeKind.Mode3_A52_1, 1);
        }

        private void spnlMode2A51_Q2_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            SetScreenMode(ModeKind.Mode3_A52_1, 2);
        }

        private void spnlMode2A51_Q3_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            SetScreenMode(ModeKind.Mode3_A52_1, 3);
        }

        private void spnlMode2A51_Q4_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            SetScreenMode(ModeKind.Mode3_A52_1, 4);
        }

        private void spnlMode2A51_Q5_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            SetScreenMode(ModeKind.Mode3_A52_1, 5);
        }

        // 花絮(影片)的按鈕
        private void spnlB10Icon0_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            SetScreenMode(ModeKind.Mode2_B20_1, 1);
        }

        private void spnlB10Icon1_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            SetScreenMode(ModeKind.Mode2_B20_1, 2);
        }

        private void spnlB10Icon2_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            SetScreenMode(ModeKind.Mode2_B20_1, 3);
        }

        private void spnlB10Icon3_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            SetScreenMode(ModeKind.Mode2_B20_1, 5);
        }

        // 動物園科普
        private void spnlB10Icon4_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            SetScreenMode(ModeKind.Mode2_B30_1, 8);
        }

        private void spnlB10Icon5_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            SetScreenMode(ModeKind.Mode2_B30_1, 14);
        }

        private void spnlB10Icon6_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            SetScreenMode(ModeKind.Mode2_B30_1, 15);
        }

        private void spnlB10Icon7_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            SetScreenMode(ModeKind.Mode2_B30_1, 16);
        }

        // 盲區的 Event
        private void spnlModeSP_02_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            // 行走 - 四足
            SetScreenMode(ModeKind.Mode2_A41_1);
        }

        private void spnlModeSP_03_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            // 站立
            SetScreenMode(ModeKind.Mode2_A21_1);
        }

        private void spnlModeSP_04_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            // 行走 - 四足
            SetScreenMode(ModeKind.Mode2_A41_1);
        }

        private void spnlModeSP_05_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            // 行走 - 四足
            SetScreenMode(ModeKind.Mode2_A41_1);
        }

        private void spnlModeSP_06_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            // 站立
            SetScreenMode(ModeKind.Mode2_A21_1);
        }

        private void spnlModeSP_07_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            // 行走 - 四足
            SetScreenMode(ModeKind.Mode2_A41_1);
        }

        private void spnlModeSP_09_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            // 坐
            SetScreenMode(ModeKind.Mode2_A51_1);
        }

        private void spnlModeSP_10_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            // 坐
            SetScreenMode(ModeKind.Mode2_A51_1);
        }

        private void spnlModeSP_11_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            // 坐
            SetScreenMode(ModeKind.Mode2_A51_1);
        }

        private void spnlModeSP_12_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            // 坐
            SetScreenMode(ModeKind.Mode2_A51_1);
        }
    }
}
