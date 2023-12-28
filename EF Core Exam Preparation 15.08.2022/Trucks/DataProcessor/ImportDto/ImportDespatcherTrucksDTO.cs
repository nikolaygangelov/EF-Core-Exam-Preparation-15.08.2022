using System.ComponentModel.DataAnnotations;
using System.Xml.Serialization;
using Trucks.Data.Models.Enums;

namespace Trucks.DataProcessor.ImportDto
{
    [XmlType("Truck")]
    public class ImportDespatcherTrucksDTO
    {
        [MaxLength(8)]
        [MinLength(8)]
        [RegularExpression(@"[A-Z][A-Z]\d{4}[A-Z][A-Z]")]
        [XmlElement("RegistrationNumber")]
        public string RegistrationNumber { get; set; }

        [Required]
        [MaxLength(17)]
        [MinLength(17)]
        [XmlElement("VinNumber")]
        public string VinNumber { get; set; }

        [Range(950, 1420)]
        [XmlElement("TankCapacity")]
        public int TankCapacity { get; set; }
        [Range(5000, 29000)]
        [XmlElement("CargoCapacity")]
        public int CargoCapacity { get; set; }

        [Required]
        [XmlElement("CategoryType")]
        public int CategoryType { get; set; }
        [Required]
        [XmlElement("MakeType")]
        public int MakeType { get; set; }
    }
}