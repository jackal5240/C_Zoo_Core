using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace GAIA.Models
{


    public class BearObject
    {
        [JsonProperty("version")]
        public string Version { get; set; }

        [JsonProperty("timestamp")]
        public long Timestamp { get; set; }

        [JsonProperty("screen_mode")]
        public string ScreenMode { get; set; }

        [JsonProperty("special_position")]
        public string SpecialPosition { get; set; }

        [JsonProperty("bear_x_y")]
        public List<int> BearXY { get; set; }

        [JsonProperty("screen_left_top")]
        public List<int> ScreenLeftTop { get; set; }

        [JsonProperty("map_x_ratio")]
        public string MapXRatio { get; set; }

        [JsonProperty("map_y_ratio")]
        public string MapYRatio { get; set; }

        [JsonProperty("live_cam_id")]
        public string LiveCamId { get; set; }

        [JsonProperty("bear_mode")]
        public string BearMode { get; set; }

        [JsonProperty("area_egg")]
        public List<AreaEgg> AreaEgg { get; set; }

        [JsonProperty("data_array")]
        public List<object> DataArray { get; set; }

        [JsonProperty("selected_data")]
        public List<object> SelectedData { get; set; }
    }
}
