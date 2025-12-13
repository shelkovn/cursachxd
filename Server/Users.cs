using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static micpix.Server.Resources;

namespace micpix.Server
{
    public class Users
    {
        public int Id { get; set; }
        required public string Username { get; set; }
        public DateTime RegistrationDate { get; set; }

        // Navigation property
        public virtual ICollection<Resources> Resources { get; set; } = new List<Resources>();
    }
}
