using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TravelAgency.Data.Models
{
    public class Customer
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [MaxLength(60)]
        public string FullName { get; set; } 

        [Required]
        [MaxLength(50)]
        public string Email { get; set; } 

        [Required]
        [MaxLength(13)]
        public string PhoneNumber { get; set; } 

        public virtual ICollection<Booking> Bookings { get; set; } = new List<Booking>();
    }
}
