using P02_FootballBetting.Common;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace P02_FootballBetting.Data.Models
{
    public class User
    {
        public User()
        {
            Bets = new HashSet<Bet>();
        }
        public int UserId { get; set; }

        [MaxLength(ValidationConstraints.UserUsernameLength)]
        public string Username { get; set; }

        [MaxLength(ValidationConstraints.UserPasswordLength)]
        public string Password { get; set; }

        [MaxLength(ValidationConstraints.UserEmailLength)]
        public string Email { get; set; }

        [MaxLength(ValidationConstraints.UserNameLength)]
        public string Name { get; set; }

        public decimal Balance { get; set; }

        public virtual ICollection<Bet> Bets { get; set; }
    }
}
