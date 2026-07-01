using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Zitn_exe_App.Models
{
    public class UserModel
    {
        public int Id { get; set; }
        public string Username { get; set; }
        public string PasswordHash { get; set; }
        public bool IsLocked { get; set; }
        public int FailedCount { get; set; }
    }
}
