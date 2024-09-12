using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Web.Data.Entities
{
    public class Role
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public List<User> Users { get; set; }
    }
}
