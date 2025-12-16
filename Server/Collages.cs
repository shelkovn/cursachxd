using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace micpix.Server
{
    public class Collages
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public int AuthorId { get; set; }
        public int Width { get; set; }
        public int Height { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }

        // Navigation properties
        public virtual Users Author { get; set; }
        public virtual ICollection<Layers> Layers { get; set; }
        public virtual ICollection<ResultGIFs> ResultGIFs { get; set; }

    }

}
