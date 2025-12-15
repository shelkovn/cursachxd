using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace micpix.Server
{
    public class UserCredential
    {
        public int Id { get; set; }

        // Foreign key
        public int UserId { get; set; }

        required public string PasswordHash { get; set; }
        required public string Salt { get; set; }

        // Navigation property 
        public virtual Users User { get; set; }
    }
}
