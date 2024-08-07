using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BdKursach
{
    public class Service
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public decimal Cost { get; set; }

        public Service(int id, string name, decimal cost)
        {
            Id = id;
            Name = name;
            Cost = cost;
        }
    }
}
