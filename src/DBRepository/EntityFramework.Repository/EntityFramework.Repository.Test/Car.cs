using Repository.IEntity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EntityFramework.Repository.Test
{
    public class Car : IEntity<int>
    {
        public string CarName { get; set; }

        public int CarPrice { get; set; }

        public int ID { get; set; }
    }
}
