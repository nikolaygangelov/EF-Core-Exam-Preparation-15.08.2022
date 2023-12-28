

using Newtonsoft.Json;
using System.ComponentModel.DataAnnotations;
using Trucks.Data.Models;

namespace Trucks.DataProcessor.ImportDto
{
    public class ImportClientsDTO
    {
        [Required]
        [MaxLength(40)]
        [MinLength(3)]
        [JsonProperty("Name")]
        public string Name { get; set; }

        [Required]
        [MaxLength(40)]
        [MinLength(2)]
        [JsonProperty("Nationality")]
        public string Nationality { get; set; }

        [Required]
        [JsonProperty("Type")]
        public string Type { get; set; }

        [JsonProperty("Trucks")]
        public int[] ClientsTrucks { get; set; }
    }
}
