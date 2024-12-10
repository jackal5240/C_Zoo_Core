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
            // 主要工作內容，將 classDR 中的 CameraData 和 MachineData 包成 Json
            // 用 MajorMethod 的 SetDRResultData 存檔起來，以便 DT 作處理
            // CameraData camera_data;
            // List<RootObject> camera_data;

            // CameraDataRoot
            List<CameraInfo>? camera_data;
            
            MachineData machine_data;

            int dataIndex = 0;
            int highestIndex = 0;
            double currentConf = -9999.0;
            double highestConf = -9999.0;

            // Localization 的拆分變數
            string[] LocalParts;
            List<double> PartsNumber = new List<double>();

            DataCollectionObject tempDCO = new DataCollectionObject();

            // List<object> data_array_item_bear = new List<object>();

            List<object> data_array_item_bear;
            List<object> selectedData;

            // 主要工作內容
            while (true)
            {
                if (doWork == true)
                {
                    // 測試用的假資料
                    // classDR.LocalizationData = "[0, 0, 0]";

                    // 全為 null 型態
                    // classDR.CameraData = "[null, null, null, null, null, null, null, null, null]";

                    // 有些是 null 的方式
                    // classDR.CameraData = "[null,{\"data\":[{\"class\":\"None\",\"conf\":[],\"coordinates\":[],\"pose\":\"Undetermined\"}],\"rtsp_url\":\"rtsp://admin:123456@192.168.86.32:554/stream2\"},null,null,null,null,null,null,null]";

                    // 全部物件都有的方式
                    // classDR.CameraData = "[{\"data\":[{\"class\":\"None\",\"conf\":[],\"coordinates\":[],\"pose\":\"Undetermined\"}],\"rtsp_url\":\"rtsp://admin:123456@192.168.86.31:554/stream2\"},{\"data\":[{\"class\":\"None\",\"conf\":[],\"coordinates\":[],\"pose\":\"Undetermined\"}],\"rtsp_url\":\"rtsp://admin:123456@192.168.86.32:554/stream2\"},{\"data\":[{\"class\":\"None\",\"conf\":[],\"coordinates\":[],\"pose\":\"Undetermined\"}],\"rtsp_url\":\"rtsp://admin:123456@192.168.86.33:554/stream2\"},{\"data\":[{\"class\":\"None\",\"conf\":[],\"coordinates\":[],\"pose\":\"Undetermined\"}],\"rtsp_url\":\"rtsp://admin:123456@192.168.86.34:554/stream2\"},{\"data\":[{\"class\":\"None\",\"conf\":[],\"coordinates\":[],\"pose\":\"Undetermined\"}],\"rtsp_url\":\"rtsp://admin:123456@192.168.86.35:554/stream2\"},{\"data\":[{\"class\":\"None\",\"conf\":[],\"coordinates\":[],\"pose\":\"Undetermined\"}],\"rtsp_url\":\"rtsp://admin:123456@192.168.86.36:554/stream2\"},{\"data\":[{\"class\":\"None\",\"conf\":[],\"coordinates\":[],\"pose\":\"Undetermined\"}],\"rtsp_url\":\"rtsp://admin:123456@192.168.86.37:554/stream2\"},{\"data\":[{\"class\":\"None\",\"conf\":[],\"coordinates\":[],\"pose\":\"Undetermined\"}],\"rtsp_url\":\"rtsp://admin:123456@192.168.86.38:554/stream2\"},{\"data\":[{\"class\":\"None\",\"conf\":[],\"coordinates\":[],\"pose\":\"Undetermined\"}],\"rtsp_url\":\"rtsp://admin:123456@192.168.86.39:554/stream2\"}]";

                    // 全部物件都有的方式 - 有值
                    // classDR.CameraData = "[{\"data\":[{\"class\":\"None\",\"conf\":[ 0.85 ],\"coordinates\":[ 0.5, 0.6, 0.7, 0.8],\"pose\":\"sit\"}],\"rtsp_url\":\"rtsp://admin:123456@192.168.86.31:554/stream2\"},{\"data\":[{\"class\":\"None\",\"conf\":[0,87],\"coordinates\":[0.2, 0.3, 0.4, 0.7],\"pose\":\"Undetermined\"}],\"rtsp_url\":\"rtsp://admin:123456@192.168.86.32:554/stream2\"},{\"data\":[{\"class\":\"None\",\"conf\":[0.89],\"coordinates\":[0.1, 0.1, 0.2, 0.5],\"pose\":\"Undetermined\"}],\"rtsp_url\":\"rtsp://admin:123456@192.168.86.33:554/stream2\"},{\"data\":[{\"class\":\"None\",\"conf\":[0.88],\"coordinates\":[1.0, 2.0, 0.9, 0.8],\"pose\":\"Undetermined\"}],\"rtsp_url\":\"rtsp://admin:123456@192.168.86.34:554/stream2\"},{\"data\":[{\"class\":\"None\",\"conf\":[0.97],\"coordinates\":[0.7, 0.9, 0.6, 0.9],\"pose\":\"Undetermined\"}],\"rtsp_url\":\"rtsp://admin:123456@192.168.86.35:554/stream2\"},{\"data\":[{\"class\":\"None\",\"conf\":[],\"coordinates\":[],\"pose\":\"Undetermined\"}],\"rtsp_url\":\"rtsp://admin:123456@192.168.86.36:554/stream2\"},{\"data\":[{\"class\":\"None\",\"conf\":[],\"coordinates\":[],\"pose\":\"Undetermined\"}],\"rtsp_url\":\"rtsp://admin:123456@192.168.86.37:554/stream2\"},{\"data\":[{\"class\":\"None\",\"conf\":[],\"coordinates\":[],\"pose\":\"Undetermined\"}],\"rtsp_url\":\"rtsp://admin:123456@192.168.86.38:554/stream2\"},{\"data\":[{\"class\":\"None\",\"conf\":[0.99],\"coordinates\":[0.1, 0.5, 0.7, 0.9],\"pose\":\"Undetermined\"}],\"rtsp_url\":\"rtsp://admin:123456@192.168.86.39:554/stream2\"}]";

                    // 全部物件都有的方式 - 但 conf 格式錯誤 空值為 Array，有值為 float
                    // classDR.CameraData = "[{\"data\":[{\"class\":\"None\",\"conf\":[],\"coordinates\":[],\"pose\":\"Undetermined\"}],\"rtsp_url\":\"rtsp://admin:123456@192.168.86.31:554/stream2\"},{\"data\":[{\"class\":\"None\",\"conf\":[],\"coordinates\":[],\"pose\":\"Undetermined\"}],\"rtsp_url\":\"rtsp://admin:123456@192.168.86.32:554/stream2\"},{\"data\":[{\"class\":\"None\",\"conf\":[],\"coordinates\":[],\"pose\":\"Undetermined\"}],\"rtsp_url\":\"rtsp://admin:123456@192.168.86.33:554/stream2\"},{\"data\":[{\"class\":\"None\",\"conf\":[],\"coordinates\":[],\"pose\":\"Undetermined\"}],\"rtsp_url\":\"rtsp://admin:123456@192.168.86.34:554/stream2\"},{\"data\":[{\"class\":\"None\",\"conf\":[],\"coordinates\":[],\"pose\":\"Undetermined\"}],\"rtsp_url\":\"rtsp://admin:123456@192.168.86.35:554/stream2\"},{\"data\":[{\"class\":\"None\",\"conf\":[],\"coordinates\":[],\"pose\":\"Undetermined\"}],\"rtsp_url\":\"rtsp://admin:123456@192.168.86.36:554/stream2\"},{\"data\":[{\"class\":\"None\",\"conf\":[],\"coordinates\":[],\"pose\":\"Undetermined\"}],\"rtsp_url\":\"rtsp://admin:123456@192.168.86.37:554/stream2\"},{\"data\":[{\"class\":\"bear\",\"conf\":0.8781145215034485,\"coordinates\":[0.771659791469574,0.6603772044181824,0.9490691423416138,0.9926785826683044],\"pose\":\"walk\"}],\"rtsp_url\":\"rtsp://admin:123456@192.168.86.38:554/stream2\"},{\"data\":[{\"class\":\"None\",\"conf\":[],\"coordinates\":[],\"pose\":\"Undetermined\"}],\"rtsp_url\":\"rtsp://admin:123456@192.168.86.39:554/stream2\"}]";

                    // 2024/10/27 09:19:57 直接給 Cam8 有辨識到熊
                    // classDR.CameraData = "[{\"data\":[{\"class\":\"None\",\"conf\":[],\"coordinates\":[],\"pose\":\"Undetermined\"}],\"rtsp_url\":\"rtsp://admin:123456@192.168.86.31:554/stream2\"},{\"data\":[{\"class\":\"None\",\"conf\":[],\"coordinates\":[],\"pose\":\"Undetermined\"}],\"rtsp_url\":\"rtsp://admin:123456@192.168.86.32:554/stream2\"},{\"data\":[{\"class\":\"None\",\"conf\":[],\"coordinates\":[],\"pose\":\"Undetermined\"}],\"rtsp_url\":\"rtsp://admin:123456@192.168.86.33:554/stream2\"},{\"data\":[{\"class\":\"None\",\"conf\":[],\"coordinates\":[],\"pose\":\"Undetermined\"}],\"rtsp_url\":\"rtsp://admin:123456@192.168.86.34:554/stream2\"},{\"data\":[{\"class\":\"None\",\"conf\":[],\"coordinates\":[],\"pose\":\"Undetermined\"}],\"rtsp_url\":\"rtsp://admin:123456@192.168.86.35:554/stream2\"},{\"data\":[{\"class\":\"None\",\"conf\":[],\"coordinates\":[],\"pose\":\"Undetermined\"}],\"rtsp_url\":\"rtsp://admin:123456@192.168.86.36:554/stream2\"},{\"data\":[{\"class\":\"None\",\"conf\":[],\"coordinates\":[],\"pose\":\"Undetermined\"}],\"rtsp_url\":\"rtsp://admin:123456@192.168.86.37:554/stream2\"},{\"data\":[{\"class\":\"bear\",\"conf\":0.9242663383483887,\"coordinates\":[0.8250488638877869,0.7502825856208801,0.9711800217628479,0.992681622505188],\"pose\":\"walk\"}],\"rtsp_url\":\"rtsp://admin:123456@192.168.86.38:554/stream2\"},{\"data\":[{\"class\":\"None\",\"conf\":[],\"coordinates\":[],\"pose\":\"Undetermined\"}],\"rtsp_url\":\"rtsp://admin:123456@192.168.86.39:554/stream2\"}]";

                    if ((classDR.CameraData != "") && (classDR.MachineData != ""))
                    {
                        try
                        {
                            // 將 Localization 的資料，直接提取到 MajorMethod 中，格式為 [0.4029791081592655, 0.8637927860803238, 4.907711957285735]
                            // 移除方括號並分割字串
                            LocalParts = classDR.LocalizationData.Trim('[', ']').Split(',');

                            // 轉換為 double 並放到 List 中
                            PartsNumber.Clear();
                            PartsNumber = LocalParts.Select(p => double.Parse(p.Trim())).ToList();

                            majorMethod.Ratio_X = PartsNumber[0];
                            majorMethod.Ratio_Y = PartsNumber[1];
                            majorMethod.Meter_Z = PartsNumber[2];

                            //// 將 Camera 資料，Json 反解析到 CameraData 中
                            // camera_data = JsonConvert.DeserializeObject<CameraData>(classDR.CameraData);
                            // camera_data = JsonConvert.DeserializeObject<List<RootObject>>(classDR.CameraData);
                            camera_data = JsonConvert.DeserializeObject<List<CameraInfo>>(classDR.CameraData);

                            // 將 Machine 的資料，Json 反解析到 MachineData 中
                            machine_data = JsonConvert.DeserializeObject<MachineData>(classDR.MachineData);

                            // 組成 DataCollectionObject 的內容，與填入資料
                            // DataCollectionObject tempDCO = new DataCollectionObject();

                            // 許多的內容，來自 Settings.json 的內容
                            tempDCO.Version = majorMethod.settingsObject.Version;

                            DateTime unixEpoch = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
                            long unixTimestamp = (long)(DateTime.Now.ToUniversalTime() - unixEpoch).TotalMicroseconds;

                            tempDCO.Timestamp = unixTimestamp;

                            tempDCO.ProductName = majorMethod.settingsObject.ProductName;
                            tempDCO.Env = majorMethod.settingsObject.Env;
                            tempDCO.ResolutionX = majorMethod.settingsObject.ResolutionX;
                            tempDCO.ResolutionY = majorMethod.settingsObject.ResolutionY;
                            tempDCO.Width = majorMethod.settingsObject.Width;
                            tempDCO.Height = majorMethod.settingsObject.Height;

                            // 黑熊花絮
                            tempDCO.AreaEgg = majorMethod.settingsObject.AreaEgg;

                            // 升降裝置部分，加上校正值
                            tempDCO.Machine = new Machine();
                            tempDCO.Machine.Height = machine_data.height + majorMethod.adjustHeight;

                            // 進行角度的轉換
                            tempDCO.Machine.Degree = ((machine_data.degree - MajorMethod.machineAbsDegreeLeft) / MajorMethod.machineAbsDegreeWhole) * MajorMethod.machineRelativeDegreeWhole + MajorMethod.machineRelativeDegreeLeft;
                            tempDCO.Machine.Degree += majorMethod.adjustDegree;

                            // 把 Degree 和 Height 記錄到 majorMethod 中
                            majorMethod.machineHeight = tempDCO.Machine.Height;
                            majorMethod.machineDegree = tempDCO.Machine.Degree;

                            // 相機-黑熊資料部分
                            tempDCO.DataArray = new List<object>();

                            // 地圖 X, Y 座標 Ratio
                            double data_array_item_1 = majorMethod.Ratio_X;
                            tempDCO.DataArray.Add(data_array_item_1);

                            double data_array_item_2 = majorMethod.Ratio_Y;
                            tempDCO.DataArray.Add(data_array_item_2);

                            string data_array_item_3 = "";
                            tempDCO.DataArray.Add(data_array_item_3);

                            // selectData 的初始值
                            selectedData = new List<object>();

                            selectedData.Add(-9999.0);
                            selectedData.Add(new List<double> { -9999.0, -9999.0, -9999.0, -9999.0 });
                            selectedData.Add("-9999.0");
                            selectedData.Add("-1");

                            // 把值傳給 AR
                            tempDCO.SelectedData = selectedData;

                            // dataIndex 的部份
                            dataIndex = 0;
                            highestIndex = -1;
                            highestConf = -9999.0;

                            // 取得每一個 CameraInfo

                            if (camera_data == null)
                                continue;

                            foreach (CameraInfo cameraInfo in camera_data)
                            {
                                // 需要新增一個 List 來加入物件
                                data_array_item_bear = new List<object>();

                                if (cameraInfo == null)
                                {
                                    // 如果有一個 Object 是 null，則此 Object 塞 -9999 之值
                                    data_array_item_bear.Add(-9999.0);
                                    data_array_item_bear.Add(new List<double> { -9999.0, -9999.0, -9999.0, -9999.0 });
                                    data_array_item_bear.Add("-9999.0");

                                    // 這是要記錄 timpstamp 的部分
                                    data_array_item_bear.Add("Timestamp:-9999.0");

                                    // 紀錄目前的 conf 值
                                    currentConf = -9999.0;

                                    tempDCO.DataArray.Add(data_array_item_bear);
                                }
                                else
                                {
                                    DetectionData detectionData = cameraInfo.Data[0];

                                    // 放入 Conf 的部分
                                    // 由於 200 的模式有錯誤，Conf 在空值時，回傳 Array，但有值時，回傳 float，造成困擾
                                    // 目前先採用這樣的處理方式
                                    // TODO 若 200 將資料格式改回來之後，這兒再做修改
                                    if (detectionData.Conf.GetType().Name == "JArray")
                                    {
                                        data_array_item_bear.Add(-9999.0);

                                        // 紀錄目前的 conf 值
                                        currentConf = -9999.0;
                                    }
                                    else
                                    {
                                        data_array_item_bear.Add(detectionData.Conf);

                                        // 紀錄目前的 conf 值
                                        currentConf = (double)detectionData.Conf;
                                    }

                                    // Coordinates 的部份
                                    if (detectionData.Coordinates.Count == 0)
                                    {
                                        data_array_item_bear.Add(new List<double> { -9999.0, -9999.0, -9999.0, -9999.0 });
                                    }
                                    else
                                    {
                                        data_array_item_bear.Add(detectionData.Coordinates);
                                    }

                                    // Pose 的部份
                                    data_array_item_bear.Add(detectionData.Pose);

                                    // Timestamp 的部分
                                    data_array_item_bear.Add(detectionData.Timestamp);

                                    // 將相機資料，加到 DataArray 中 
                                    tempDCO.DataArray.Add(data_array_item_bear);

                                    // 找出最高者，放到 selected_data 中
                                    if ((currentConf >= majorMethod.confThreshold) && (currentConf > highestConf))
                                    {
                                        // 新增一個物件
                                        selectedData = new List<object>();

                                        // 紀錄目前最高者
                                        highestConf = currentConf;

                                        selectedData.Add(currentConf);
                                        selectedData.Add(detectionData.Coordinates);
                                        selectedData.Add(detectionData.Pose);
                                        selectedData.Add(dataIndex);

                                        // 新增的 Timestamp 要放在最後
                                        selectedData.Add(detectionData.Timestamp);

                                        highestIndex = dataIndex;

                                        // 把值傳給 AR
                                        tempDCO.SelectedData = selectedData;
                                    }
                                }

                                // 記錄下一個位置
                                dataIndex += 1;
                            }

                            // 若 9 號相機是最高值，或是 1-8 都沒有值，則 SelectedData 變成空值
                            if ((highestIndex == 8) || (highestIndex == -1))
                            {
                                // selectData 的初始值
                                selectedData = new List<object>();

                                selectedData.Add(-8888.0);
                                selectedData.Add(new List<double> { -9999.0, -9999.0, -9999.0, -9999.0 });
                                selectedData.Add("-8888.0");
                                selectedData.Add("-1");

                                // 這是要記錄 timpstamp 的部分
                                selectedData.Add("Timestamp:-8888.0");

                                // 把值傳給 AR
                                tempDCO.SelectedData = selectedData;
                            }

                            // 相機基本資料部分(Camera Mapping)
                            tempDCO.CameraMapping = majorMethod.settingsObject.CameraMapping;

                            // 相機基本資料部分(Camera 角度)
                            tempDCO.Camera1 = majorMethod.settingsObject.Camera1;
                            tempDCO.Camera2 = majorMethod.settingsObject.Camera2;
                            tempDCO.Camera3 = majorMethod.settingsObject.Camera3;
                            tempDCO.Camera4 = majorMethod.settingsObject.Camera4;
                            tempDCO.Camera5 = majorMethod.settingsObject.Camera5;
                            tempDCO.Camera6 = majorMethod.settingsObject.Camera6;
                            tempDCO.Camera7 = majorMethod.settingsObject.Camera7;
                            tempDCO.Camera8 = majorMethod.settingsObject.Camera8;
                            tempDCO.Camera9 = majorMethod.settingsObject.Camera9;

                            // 化成 Json 字串
                            tempJson = JsonConvert.SerializeObject(tempDCO);

                            majorMethod.SetDRResultData(tempJson);

                            // 回報 Progress，觸發 ProgressChanged
                            bgWorker?.ReportProgress(0);

                            // 紀錄正確的 Json
                            TextLog.WriteLog("多工 Output:" + majorMethod.GetDRResultData());
                        }
                        catch (Exception exception)
                        {
                            TextLog.WriteLog("Deserialize 錯誤:" + exception.Message);
                            TextLog.WriteLog(classDR.CameraData);
                            // return;
                        }
                    }
                }

                // 暫停 0.1 秒後再轉換
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
