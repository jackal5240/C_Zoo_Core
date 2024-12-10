using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace ANB_SSZ.Models
{
    public class ReceiveData
    {
        [JsonProperty("camera_data")]
        public string CameraData { get; set; }

        [JsonProperty("machine_data")]
        public string MachineData { get; set; }
    }

}
