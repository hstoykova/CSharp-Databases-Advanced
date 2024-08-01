using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace Medicines.DataProcessor.ImportDtos
{
    [XmlType("Pharmacy")]
    public class PharmaciesDto
    {
        [XmlAttribute("non-stop")]
        [Required]
        public string IsNonStop { get; set; }

        [XmlElement("Name")]
        [Required]
        [MinLength(2)]
        [MaxLength(50)]
        public string Name { get; set; }

        [XmlElement("PhoneNumber")]
        [Required]
        [MinLength(14)]
        [MaxLength(14)]
        [RegularExpression(@"\([0-9]{3}\)\s[0-9]{3}-[0-9]{4}")]
        public string PhoneNumber { get; set; }

        [XmlArray("Medicines")]
        public MedicineDto[] Medicines { get; set; }
    }
}
