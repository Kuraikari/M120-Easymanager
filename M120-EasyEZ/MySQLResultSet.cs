using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace M120_EasyEZ
{
    public class MySQLResultSet
    {
        public int id;
        public List<string> col;


        public MySQLResultSet()
        {
            this.col = new List<string>();
        }

        public MySQLResultSet(List<string> col)
        {
            this.col = col;
        }

        public MySQLResultSet(int id, List<string> col)
        {
            this.id = id;
            this.col = col;
        }

    }
}
