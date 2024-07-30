using Cadastre.Data.Enumerations;
using Cadastre.Data.Models;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace Cadastre.DataProcessor.ImportDtos
{
    public class CitizensDto
    {
        [Required]
        [MinLength(2)]
        [MaxLength(30)]
        public string FirstName { get; set; }

        [Required]
        [MinLength(2)]
        [MaxLength(30)]
        public string LastName { get; set; }

        [Required]
        public string BirthDate { get; set; }

        [Required]
        public string MaritalStatus { get; set; } = null!; //possible mistake - can be string

        public int[] Properties { get; set; }

    }
}
