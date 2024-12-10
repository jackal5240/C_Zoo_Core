using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace ANB_SSZ.Models
{
    // 這是模擬器的資料格式
    public class CameraData
    {
        [JsonProperty("video_name")]
        public string VideoName { get; set; }

        [JsonProperty("data")]
        public List<BearData> Data { get; set; }

    }

    // 這是 ZOO 場域的資料格式
    public class DataItem
    {
        [JsonProperty("class")]
        public string Class_s { get; set; }

        [JsonProperty("conf")]
        public object Conf { get; set; }
        // public double Conf { get; set; }

        [JsonProperty("coordinates")]
        public List<double> Coordinates { get; set; }

        [JsonProperty("pose")]
        public string Pose { get; set; }
    }

    public class RootObject
    {
        [JsonProperty("data")]
        public List<DataItem> Data { get; set; }

        [JsonProperty("rtsp_url")]
        public string RtspUrl { get; set; }
    }

    // 20241025 新產出的格式
    public class CameraDataRoot
    {
        public List<CameraInfo> Cameras { get; set; }
    }

    public class CameraInfo
    {
        [JsonProperty("data")]
        public List<DetectionData> Data { get; set; }

        [JsonProperty("rtsp_url")]
        public string RtspUrl { get; set; }
    }

    public class DetectionData
    {
        [JsonProperty("class")]
        public string Class { get; set; }

        [JsonProperty("conf")]
        //public List<double> Conf { get; set; }
        public object Conf { get; set; }

        [JsonProperty("coordinates")]
        public List<double> Coordinates { get; set; }

        [JsonProperty("pose")]
        public string Pose { get; set; }

        // For 1.3.4 辨識模組
        [JsonProperty("timestamp")]
        public string Timestamp { get; set; }
    }

    // ===

    public class BearData
    {
        [JsonProperty("conf")]
        public double conf { get; set; }

        [JsonProperty("class")]
        public string bear_class { get; set; }

        [JsonProperty("coordinates")]
        public List<Double> coordinates { get; set; }

        [JsonProperty("pose")]
        public string pose { get; set; }

        [JsonProperty("timestamp")]
        public string timestamp { get; set; }
    }
}
