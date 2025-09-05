using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Library_Management_System.Models
{
    internal class User
    {
        public string Username { get; set; }
        public string Password { get; set; }  // Ideally, hashed in real apps public string Username { get; set; }
    }
}
