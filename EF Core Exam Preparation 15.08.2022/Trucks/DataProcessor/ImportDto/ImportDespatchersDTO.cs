
using System.ComponentModel.DataAnnotations;
using System.Xml.Serialization;
using Trucks.Data.Models;

namespace Trucks.DataProcessor.ImportDto
{
    [XmlType("Despatcher")]
    public class ImportDespatchersDTO
    {
        [Required]
        [MaxLength(40)]
        [MinLength(2)]
        [XmlElement("Name")]
        public string Name { get; set; }

        [Required]
        [XmlElement("Position")]
        public string Position { get; set; }

        [XmlArray("Trucks")]
        public ImportDespatcherTrucksDTO[] Trucks { get; set; }
    }
}
