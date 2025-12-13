using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace micpix.Server
{
    public class Resources
    {
        public int Id { get; set; }
        public required string ImagePath { get; set; }
        public required string Title { get; set; }
        public DateTime UploadDate { get; set; }

        // Foreign key property
        public int AuthorId { get; set; }

        // Navigation property
        public virtual Users Author { get; set; }

    }
}
