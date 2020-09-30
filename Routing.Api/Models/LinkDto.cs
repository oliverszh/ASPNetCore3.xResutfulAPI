using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;

namespace Routing.Api.Models
{
    public class LinkDto
    {
        public string Href { get; set; }
        public string Rel { get; set; }
        public string Method { get; set; }
        public LinkDto(string href,string rel,string method)
        {
            Href = href;
            Rel = rel;
            Method = method;
        }
    }
}
