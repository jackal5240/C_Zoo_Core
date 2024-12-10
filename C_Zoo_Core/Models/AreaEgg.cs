using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GAIA.Models
{
    public class AreaEgg
    {
        [JsonProperty("egg_id")]
        public string EggId { get; set; }

        [JsonProperty("position_x_y")]
        public List<int> PositionXY { get; set; }
    }
}
