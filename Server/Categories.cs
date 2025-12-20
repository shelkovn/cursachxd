using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace micpix.Server
{
    public class Categories
    {
        public int Id { get; set; }
        public int? ParentId { get; set; } = null;
        required public string Name { get; set; }

        // Navigation properties
        public virtual Categories? Parent { get; set; }

        public virtual ICollection<Categories> Children { get; set; } = new List<Categories>();
    }
}
