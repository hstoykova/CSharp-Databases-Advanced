using System.ComponentModel.DataAnnotations;
using System.Xml.Serialization;

namespace Cadastre.DataProcessor.ImportDtos
{
    [XmlType("Property")]
    public class PropertyDto
    {
        [XmlElement("PropertyIdentifier")]
        [MinLength(16)]
        [MaxLength(20)]
        [Required]
        public string PropertyIdentifier { get; set; }

        [XmlElement("Area")]
        [Range(0, int.MaxValue)]
        [Required]
        public int Area { get; set; } //possible mistake here for the uint

        [XmlElement("Details")]
        [MinLength(5)]
        [MaxLength(500)]
        public string? Details { get; set; }

        [XmlElement("Address")]
        [MinLength(5)]
        [MaxLength(200)]
        [Required]
        public string Address { get; set; }

        [XmlElement("DateOfAcquisition")]
        [Required]
        public string DateOfAcquisition { get; set; } //poissible mistake, the type may be string
    }
}
