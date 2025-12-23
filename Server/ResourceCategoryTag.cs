using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace micpix.Server
{
    public class ResourceCategoryTags
    {
        public int Id { get; set; }
        required public int CategoryId { get; set; }

        required public int ResourceId { get; set; }

        // Navigation properties
        public virtual Categories Category { get; set; }

        public virtual Resources Resource { get; set; }
    }
}
