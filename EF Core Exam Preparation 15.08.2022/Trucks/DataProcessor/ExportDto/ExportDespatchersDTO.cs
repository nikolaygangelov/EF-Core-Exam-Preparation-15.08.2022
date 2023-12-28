
using System.ComponentModel.DataAnnotations;
using System.Xml.Serialization;
using Trucks.Data.Models;
using Trucks.DataProcessor.ImportDto;

namespace Trucks.DataProcessor.ExportDto
{
    [XmlType("Despatcher")]
    public class ExportDespatchersDTO
    {
        [XmlAttribute("TrucksCount")]
        public int TrucksCount { get; set; }

        [Required]
        [MaxLength(40)]
        [MinLength(2)]
        [XmlElement("DespatcherName")]
        public string Name { get; set; }

        [XmlArray("Trucks")]
        public ExportDespatcherTrucksDTO[] Trucks { get; set; }
    }
}
