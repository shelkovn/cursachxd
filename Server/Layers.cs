using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace micpix.Server
{
    public class Layers
    {
        public int Id { get; set; }
        public int CollageId { get; set; }
        public int LayerIndex { get; set; }
        public int ResourceId { get; set; }
        public int XOffset { get; set; }
        public int YOffset { get; set; }
        public decimal XScale { get; set; } = 100.00m;
        public decimal YScale { get; set; } = 100.00m;
        public decimal Rotation { get; set; } = 0.00m;
        public decimal Opacity { get; set; } = 1.00m;
        public DateTime CreatedAt { get; set; }

        // Navigation properties
        public virtual Collages Collage { get; set; }
        public virtual Resources Resource { get; set; }
    }
}
