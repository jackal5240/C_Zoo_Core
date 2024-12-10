using GAIA.MainObject;
using System.ComponentModel;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace GAIA.Module_DataReceiver
{
    class ReceiveMonitorWorkerClass
    {
        // 外部的各項屬性
        public static MainWindow? mainWin;
        public static BackgroundWorker? bgWorker;
        public static MajorMethod? majorMethod;
        public static ClassDR? classDR;

        // 是否開始進行資料的接收
        public static Boolean doWork = false;

        // Socket 相關物件
        // static string ipAddr = "127.0.0.1";
        static string ipAddr = "192.168.2.205";

        static IPAddress ipAddress = IPAddress.Parse(ipAddr);
        static IPEndPoint ipEndPoint = new(ipAddress, 5678);
        static Socket socketClient = new(ipEndPoint.AddressFamily, SocketType.Stream, ProtocolType.Tcp);

        public ReceiveMonitorWorkerClass()
        {
            // 建構式

        }

        public static void DoWorkHandler(object? sender, DoWorkEventArgs e)
        {
            // 主要工作內容
            // 升降 : 0 ~ 700mm，角度 : -30.0 ~ 110.0

            String MachineString = "";

            // 建立與升降裝置的連結
            socketClient.ConnectAsync(ipEndPoint);

            while (true) { 

                if (doWork == true)
                {
                    try
                    {
                        // 取得升降裝置的資料
                        var bufferString = new byte[1 * 1024];
                        var receivedString = socketClient.Receive(bufferString, SocketFlags.None);
                        var MachineList = Encoding.UTF8.GetString(bufferString, 0, receivedString).Split("\r\n");

                        // 倒數第一個可能還是有不完全的字串，所以要取倒數第二個
                        if (MachineList.Length >= 2)
                        {
                            MachineString = MachineList[MachineList.Length - 2];
                        }
                        else
                        {
                            MachineString = MachineList[0];
                        }

                        // 將資料傳到 MajorMethod 中
                        classDR.MachineData = MachineString;

                        // 回報 Progress，觸發 ProgressChanged
                        bgWorker?.ReportProgress(0);

                        // 顯示回應資料
                        // Debug.WriteLine($"時間: {DateTime.Now}, 回應: {MachineString}");
                    }
                    catch (Exception exception)
                    {
                        Debug.WriteLine($"錯誤: {exception.Message}");
                    }
                }
            }
        }

        public static void ProgressChangedHandler(object? sender, ProgressChangedEventArgs e)
        {
            // 在此對外進行工作內容報告
            // mainWin.WriteMachineLog(classDR.MachineData);
        }

        public static void RunWorkerCompletedHandler(object? sender, RunWorkerCompletedEventArgs e)
        {
            // 當 Worker 執行完畢後，進行資源回收與關閉

        }

    }
}
