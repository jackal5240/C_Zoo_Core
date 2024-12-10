using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GAIA.Models
{
    public class DataCollectionObject : SettingsObject
    {
        [JsonProperty("machine")]
        public Machine Machine { get; set; }

        [JsonProperty("data_array")]
        public List<object> DataArray { get; set; }

        // 2024/10/20 新增
        [JsonProperty("selected_data")]
        public List<object> SelectedData { get; set; }
    }

    public class Machine
    {
        [JsonProperty("height")]
        public double Height { get; set; }

        [JsonProperty("degree")]
        public double Degree { get; set; }
    }

    public class Camera
    {
        [JsonProperty("rotate")]
        public double Rotate { get; set; }

        [JsonProperty("tilt")]
        public double Tilt { get; set; }
    }

    public class CameraMapping
    {
        [JsonProperty("camera_id")]
        public string CameraId { get; set; }

        [JsonProperty("camera_to_display_x_y")]
        public List<List<double>> CameraToDisplayXY { get; set; }
    }
}
