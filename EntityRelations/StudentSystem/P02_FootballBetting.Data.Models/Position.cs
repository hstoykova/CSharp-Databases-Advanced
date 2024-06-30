using P02_FootballBetting.Common;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace P02_FootballBetting.Data.Models
{
    public class Position
    {
        public Position()
        {
            Players = new HashSet<Player>();
        }
        public int PositionId { get; set; }

        [MaxLength(ValidationConstraints.PositionNameLength)]
        public string Name { get; set; }

        public virtual ICollection<Player> Players { get; set; }
    }
}
