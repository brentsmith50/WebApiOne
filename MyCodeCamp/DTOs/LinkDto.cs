using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MyCodeCamp.DTOs
{
    public class LinkDto
    {
        public string Href { get; set; }
        public string Rel { get; set; }
        public string Verb { get; set; } = "GET";
    }
}
