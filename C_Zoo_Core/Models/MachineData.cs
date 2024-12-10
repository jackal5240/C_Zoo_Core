using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace ANB_SSZ.Models
{
    public class MachineData
    {
        [JsonProperty("timestamp")]
        public long timestamp { get; set; }

        [JsonProperty("height")]
        public double height { get; set; }

        [JsonProperty("degree")]
        public double degree { get; set; }
    }
}
