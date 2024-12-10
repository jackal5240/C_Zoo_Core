using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GAIA.Models
{
    public class SettingsObject
    {
        [JsonProperty("version")]
        public string Version { get; set; }

        [JsonProperty("timestamp")]
        public long Timestamp { get; set; }

        [JsonProperty("productName")]
        public string ProductName { get; set; }

        [JsonProperty("env")]
        public string Env { get; set; }

        [JsonProperty("resolutionX")]
        public int ResolutionX { get; set; }

        [JsonProperty("resolutionY")]
        public int ResolutionY { get; set; }

        [JsonProperty("width")]
        public double Width { get; set; }

        [JsonProperty("height")]
        public double Height { get; set; }

        [JsonProperty("area_egg")]
        public List<AreaEgg> AreaEgg { get; set; }

        [JsonProperty("camera_mapping")]
        public List<CameraMapping> CameraMapping { get; set; }

        [JsonProperty("camera1")]
        public Camera Camera1 { get; set; }

        [JsonProperty("camera2")]
        public Camera Camera2 { get; set; }

        [JsonProperty("camera3")]
        public Camera Camera3 { get; set; }

        [JsonProperty("camera4")]
        public Camera Camera4 { get; set; }

        [JsonProperty("camera5")]
        public Camera Camera5 { get; set; }

        [JsonProperty("camera6")]
        public Camera Camera6 { get; set; }

        [JsonProperty("camera7")]
        public Camera Camera7 { get; set; }

        [JsonProperty("camera8")]
        public Camera Camera8 { get; set; }

        [JsonProperty("camera9")]
        public Camera Camera9 { get; set; }
    }

}
