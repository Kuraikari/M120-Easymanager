using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace M120_EasyEZ
{
    class Profile
    {
        public int Id { get; set; }
        public string Profilename { get; set; }
        public int UserId { get; set; }

        public Profile(int id, string profilename, int userId)
        {
            this.Id = id;
            this.Profilename = profilename;
            this.UserId = userId;
        }

    }
}
