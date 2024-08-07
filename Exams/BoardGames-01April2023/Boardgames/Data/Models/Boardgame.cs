﻿using Boardgames.Data.Models.Enums;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Boardgames.Data.Models
{
    public class Boardgame
    {
        public Boardgame()
        {
            BoardgamesSellers = new List<BoardgameSeller>();
        }

        [Key]
        public int Id { get; set; }

        [Required]
        [MaxLength(20)]
        public string Name { get; set; }

        [Required]
        //[Range(1, 10.00)]
        public double Rating { get; set; }

        [Required]
        //[Range(2018, 2023)]
        public int YearPublished { get; set; }

        [Required]
        public CategoryType CategoryType { get; set; }

        [Required]
        public string Mechanics { get; set; }

        [ForeignKey(nameof(Creator))]
        public int CreatorId { get; set; }
        public virtual Creator Creator { get; set; }

        public virtual ICollection<BoardgameSeller> BoardgamesSellers { get; set; } 
    }
}
