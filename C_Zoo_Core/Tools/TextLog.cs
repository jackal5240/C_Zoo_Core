using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ANB_UI.Tools
{
    public class TextLog
    {
        // 相關變數，使用單例模式
        private static readonly object _lock = new object();
        private static volatile TextLog _instance;
        private string _currentFileName;
        private StreamWriter _writer;
        private int _currentHour;

        // 私有構造函數確保只能通過 Instance 屬性訪問
        private TextLog()
        {
            UpdateFileNameAndWriter();
        }

        public static TextLog Instance
        {
            get
            {
                if (_instance == null)
                {
                    lock (_lock)
                    {
                        if (_instance == null)
                        {
                            _instance = new TextLog();
                        }
                    }
                }
                return _instance;
            }
        }

        private void UpdateFileNameAndWriter()
        {
            var now = DateTime.Now;
            _currentHour = now.Hour;

            // 建立日誌檔案路徑
            // string logDirectory = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Logs");
            string logDirectory = @"D:\Logs\Zoo\";
            if (!Directory.Exists(logDirectory))
            {
                Directory.CreateDirectory(logDirectory);
            }

            // 產生檔名 YYYYMMDD_HH.txt
            _currentFileName = Path.Combine(logDirectory, $"{now:yyyyMMdd}_{now.Hour:D2}.txt");

            // 如果寫入器存在則關閉
            if (_writer != null)
            {
                _writer.Dispose();
            }

            // 建立新的寫入器，設定為 append 模式
            _writer = new StreamWriter(_currentFileName, true, Encoding.UTF8);
            _writer.AutoFlush = true;
        }

        // 應用程式結束時呼叫，確保資源正確釋放
        public void Dispose()
        {
            if (_writer != null)
            {
                _writer.Flush();
                _writer.Dispose();
                _writer = null;
            }
        }

        // 寫入 Log
        public void Log(string message)
        {
            try
            {
                lock (_lock)
                {
                    var now = DateTime.Now;

                    // 檢查是否需要切換到新的日誌檔案
                    if (now.Hour != _currentHour)
                    {
                        UpdateFileNameAndWriter();
                    }

                    // 寫入日誌，格式：[時間] 訊息
                    _writer.WriteLine($"[{now:yyyy-MM-dd HH:mm:ss.fff}] {message}");
                }
            }
            catch (Exception ex)
            {
                // 處理可能的錯誤，例如檔案存取權限問題
                Console.WriteLine($"Logger error: {ex.Message}");
            }
        }


        public static void WriteLog(string message)
        {
            // 訊息內容修整
            message = message.Replace("\r\n", "");
            message = message.Replace(" ", "");

            // 改用以上的單例寫法
            TextLog.Instance.Log(message);

            //string DIRNAME = @"D:\Logs\Zoo\";
            ////string DIRNAME = MainForm.configFileContent.Logs;
            //// 改成以每小時一個檔案
            //string FILENAME = DIRNAME + DateTime.Now.ToString("yyyyMMdd") + "_" + DateTime.Now.Hour.ToString("D2") + ".txt";

            //if (!Directory.Exists(DIRNAME))
            //    Directory.CreateDirectory(DIRNAME);

            //if (!File.Exists(FILENAME))
            //{
            //    // The File.Create method creates the file and opens a FileStream on the file. You neeed to close it.
            //    File.Create(FILENAME).Close();
            //}

            //try
            //{
            //    using (StreamWriter sw = File.AppendText(FILENAME))
            //    {
            //        message = message.Replace("\r\n", "");
            //        message = message.Replace(" ", "");
            //        Log(message, sw);
            //    }
            //}
            //catch { }
        }

        private static void Log(string logMessage, TextWriter w)
        {
            w.Write("\r\nLog Entry : ");
            w.WriteLine("{0} {1}", DateTime.Now.ToString("HH:mm:ss.fff"), DateTime.Now.ToLongDateString());
            w.WriteLine("  :");
            w.WriteLine("  :{0}", logMessage);
            w.WriteLine("-------------------------------");
        }

        public static void ReadLog(string Date_yyyyMMdd)
        {
            //string DIRNAME = Application.StartupPath + @"\Log\";
            string DIRNAME = @"D:\Log\ANB_UI\";
            string FILENAME = DIRNAME + Date_yyyyMMdd + ".txt";

            if (File.Exists(FILENAME))
            {
                using (StreamReader r = File.OpenText(FILENAME))
                {
                    DumpLog(r);
                }
            }
            else
            {
                Console.WriteLine(Date_yyyyMMdd + ": No Data!");
            }
        }

        private static void DumpLog(StreamReader r)
        {
            string line;
            while ((line = r.ReadLine()) != null)
            {
                Console.WriteLine(line);
            }
        }
    }
}
