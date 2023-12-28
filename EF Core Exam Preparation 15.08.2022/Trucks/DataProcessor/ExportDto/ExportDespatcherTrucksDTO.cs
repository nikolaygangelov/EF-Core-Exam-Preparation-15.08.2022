using System.ComponentModel.DataAnnotations;
using System.Xml.Serialization;
using Trucks.Data.Models.Enums;

namespace Trucks.DataProcessor.ExportDto
{
    [XmlType("Truck")]
    public class ExportDespatcherTrucksDTO
    {
        [MaxLength(8)]
        [MinLength(8)]
        [RegularExpression(@"[A-Z][A-Z]\d{4}[A-Z][A-Z]")]
        [XmlElement("RegistrationNumber")]
        public string RegistrationNumber { get; set; }

        [Required]
        [XmlElement("Make")]
        public MakeType MakeType { get; set; }
    }
}