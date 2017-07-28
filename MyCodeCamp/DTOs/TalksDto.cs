using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace MyCodeCamp.DTOs
{
    public class TalksDto
    {
        public string Url { get; set; }
        [Required]
        public string Title { get; set; }
        [Required]
        public string Abstract { get; set; }
        [Required]
        public string Category { get; set; }
        public string Level { get; set; }
        public string Prerequisites { get; set; }
        public DateTime StartingTime { get; set; } = DateTime.Now;
        public string Room { get; set; }

        public ICollection<LinkDto> Links { get; set; }
    }
}
