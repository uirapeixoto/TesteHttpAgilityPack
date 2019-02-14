using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace ProjetoTest.Domain.Model
{
    [DebuggerDisplay("{Model}, {Price}")]
    public class CarModel
    {
        public string Model { get; set; }
        public string Price { get; set; }
        public string Link { get; set; }
        public string ImageUrl { get; set; }
    }
}
