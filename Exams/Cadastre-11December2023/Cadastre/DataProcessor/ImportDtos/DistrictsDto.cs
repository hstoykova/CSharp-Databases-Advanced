using Cadastre.Data.Models;
using System.ComponentModel.DataAnnotations;
using System.Net;
using System.Xml.Linq;
using System.Xml.Serialization;

namespace Cadastre.DataProcessor.ImportDtos
{
    //[XmlRoot("Districts")]
	[XmlType("District")]
    public class DistrictsDto
    {
        [XmlAttribute("Region")]
        //[Range(0, 3)]
        [Required]
        public string Region { get; set; } //changed from int to string

        [XmlElement("Name")]
        [MinLength(2)]
        [MaxLength(80)]
        [Required]
        public string Name { get; set; }

        [XmlElement("PostalCode")]
        [MinLength(8)]
        [MaxLength(8)] //possible mistake here
        [RegularExpression(@"[A-Z]{2}-[0-9]{5}")]
        [Required]
        public string PostalCode { get; set; }

        [XmlArray("Properties")]
        public PropertyDto[] Properties { get; set; }

    }
}
