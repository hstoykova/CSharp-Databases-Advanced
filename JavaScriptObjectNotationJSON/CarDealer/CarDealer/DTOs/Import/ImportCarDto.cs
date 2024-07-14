using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CarDealer.DTOs.Import
{
    public class ImportCarDto
    {
        public string Make { get; set; } = null!;

        public string Model { get; set; } = null!;

        [JsonProperty("traveledDistance")] //misspelled in the json
        public long TravelledDistance { get; set; }

        public ICollection<int> PartsId { get; set; } = new List<int>();
    }
}
