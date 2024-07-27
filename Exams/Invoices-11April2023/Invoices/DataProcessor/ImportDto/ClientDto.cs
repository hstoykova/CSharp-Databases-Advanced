using Invoices.Data.Models;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics.Metrics;
using System.Xml.Serialization;

namespace Invoices.DataProcessor.ImportDto
{
    [XmlType("Client")]
    public class ClientDto
    {
        [Required]
        [MinLength(10)]
        [MaxLength(25)]
        [XmlElement("Name")]
        public string Name { get; set; }

        [Required]
        [MinLength(10)]
        [MaxLength(15)]
        [XmlElement("NumberVat")]
        public string NumberVat { get; set; }

        [XmlArray("Addresses")]
        public List<AddressDto> Addresses { get; set; } = new List<AddressDto>();

    }

    [XmlType("Address")]
    public class AddressDto
    {
        [Required]
        [MinLength(10)]
        [MaxLength(20)]
        [XmlElement("StreetName")]
        public string StreetName { get; set; }

        [Required]
        [XmlElement("StreetNumber")]
        public int StreetNumber { get; set; }

        [Required]
        [XmlElement("PostCode")]
        public string PostCode { get; set; }

        [Required]
        [MinLength(5)]
        [MaxLength(15)]
        [XmlElement("City")]
        public string City { get; set; }

        [Required]
        [MinLength(5)]
        [MaxLength(15)]
        [XmlElement("Country")]
        public string Country { get; set; }
    }
}
