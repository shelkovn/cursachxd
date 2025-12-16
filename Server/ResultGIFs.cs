using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace micpix.Server
{
    public class ResultGIFs
    {
        public int Id { get; set; }
        public int CollageId { get; set; }
        public string FilePath { get; set; }
        public int FrameCount { get; set; }
        public DateTime CreatedAt { get; set; }

        // Navigation property
        public virtual Collages Collage { get; set; }
    }
}
