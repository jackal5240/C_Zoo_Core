using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace C_Zoo_Core.Models
{
    public class SourceObject
    {
        [JsonProperty("timestamp")]
        public string Timestamp { get; set; }

        [JsonProperty("object_detection")]
        public List<ObjectDetection> ObjectDetection { get; set; }

        [JsonProperty("localization")]
        public List<int> Localization { get; set; }

        [JsonProperty("camera_monitor")]
        public List<int> CameraMonitor { get; set; }

        [JsonProperty("area_cal")]
        public List<AreaCal> AreaCal { get; set; }

        [JsonProperty("rotate")]
        public Rotate Rotate { get; set; }
    }
    public class ObjectDetection
    {
        [JsonProperty("rtsp_url")]
        public string RtspUrl { get; set; }

        [JsonProperty("data")]
        public List<Data> Data { get; set; }
    }
    public class AreaCal
    {
        [JsonProperty("area")]
        public int Area { get; set; }

        [JsonProperty("camera")]
        public int Camera { get; set; }

        [JsonProperty("conf")]
        public double Conf { get; set; }
    }
    public class Data
    {
        [JsonProperty("class")]
        public string Class { get; set; }

        [JsonProperty("conf")]
        public List<object> Conf { get; set; }

        [JsonProperty("coordinates")]
        public List<object> Coordinates { get; set; }

        [JsonProperty("pose")]
        public string Pose { get; set; }

        [JsonProperty("timestamp")]
        public string Timestamp { get; set; }
    }
    public class Rotate
    {
        [JsonProperty("height")]
        public string Height { get; set; }

        [JsonProperty("degree")]
        public string Degree { get; set; }

        [JsonProperty("timestamp")]
        public string Timestamp { get; set; }
    }
}
