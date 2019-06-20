using System;
using System.Collections.Generic;
using System.Linq;
using MySql.Data.MySqlClient;

namespace M120_EasyEZ
{
    public class ConnectionBuilder
    {
        private String ConnectionString { get; set; }
        private String Server { get; set; }
        private String UserID { get; set; }
        private int Port { get; set; }
        private String Password { get; set; }
        private String Database { get; set; }
        private MySqlConnection Connection { get; set; }
        public MySQLResultSet Result { get; set; }
        public Dictionary<string, int> LastEntry { get; set; }

        //MySqlTrans
        private MySqlTransaction tr;

        public ConnectionBuilder ()
        {
            this.Connection = new MySqlConnection();
            this.Result = new MySQLResultSet();
            this.LastEntry = new Dictionary<string, int>();
        }

        //Builder Functions

        public ConnectionBuilder SetServer(string s) {
            this.Server = s;
            return this;
        }
        public ConnectionBuilder SetUserID(string s) 
        {
            this.UserID = s;
            return this;
        }
        public ConnectionBuilder SetPort(int s) {
            this.Port = s;
            return this;
        }
        public ConnectionBuilder SetPassword(string s) 
        {
            this.Password = s;
            return this;
        }
        public ConnectionBuilder SetDatabase(string s) 
        {
            this.Database = s;
            return this;
        }
        public ConnectionBuilder BuildConnectionString() {
            Object[] args = { this.Server, this.Port, this.Database, this.UserID, this.Password };
            this.ConnectionString = String.Format("Data Source='{0}';Port={1};Database='{2}';UID='{3}';PWD='{4}';", args);
            return this;
        }
        public ConnectionBuilder Connect() 
        {
            this.Connection.ConnectionString = this.ConnectionString;
            try
            {
                this.Connection.Open();
                Console.WriteLine("Yes!!!");
                this.Connection.Close();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                throw;
            }     

            return this;
        }
        public ConnectionBuilder UpdateEntry(string table, Dictionary<string, object> row)
        {
            this.Connection.Open();

            try
            {
                tr = this.Connection.BeginTransaction();
                MySqlCommand cmd = new MySqlCommand();
                cmd.Transaction = tr;

                string cmdText = String.Format("UPDATE {0} SET ", table);

                for (int i = 0; i < row.Count() - 1; i++)
                {
                    if(row.Count() - 1 > i )
                        cmdText += String.Format("{0}='{1}' ", new object[]{ row.Keys.ToArray()[i], row.Values});
                    else
                        cmdText += string.Format("WHERE {0} LIKE '{1}'", row.Keys.Select(x => x.StartsWith("id_")), row.Values);
                }
               
                cmd.CommandText = cmdText;
                
                cmd.ExecuteNonQuery();
                tr.Commit();
            }
            catch (MySqlException ex)
            {
                try
                {
                    tr.Rollback();
                }
                catch (MySqlException ex1)
                {
                    Console.WriteLine(ex1.ToString());
                }

                Console.WriteLine(ex.ToString());
            }

            return this;
        }
        public ConnectionBuilder CreateEntry(string table, Dictionary<string, object> row)
        {
           
            this.Connection.ConnectionString = this.ConnectionString;
            this.Connection.Open();
            tr = this.Connection.BeginTransaction();    

            // Console.WriteLine(row.Keys.Count);

            string cmdText = String.Format("INSERT INTO {0} ( ", table);

            for (int i = 0; i < row.Count(); i++)
            {
                if (row.Count() - 1 != i)
                    cmdText += String.Format("{0}, ", new object[] { row.Keys.ToArray()[i]});
                else
                    cmdText += String.Format("{0} ) ", new object[] { row.Keys.ToArray()[i] });
            }

            cmdText += String.Format(" VALUES ( ");

            for (int i = 0; i < row.Count(); i++)
            {
                if (row.Count() - 1 != i)
                {
                    if (row.Values.ToArray()[i].GetType() == typeof(String))
                    {
                        cmdText += String.Format("'{0}', ", new object[] { row.Values.ToArray()[i] });
                    }
                    else
                    {
                        cmdText += String.Format("{0}, ", new object[] { row.Values.ToArray()[i] });
                    }
                }
                else
                {
                    cmdText += String.Format("'{0}' ) ", new object[] { row.Values.ToArray()[i] });
                }
            }

            this.SelectEntry(table, new string[] { "id_"+table }, false, null, null, true);
            if (Result.col.Count > 1) LastEntry.Add(table, int.Parse(this.Result.col[1]));

            try
            {
                MySqlCommand cmd = this.Connection.CreateCommand();
                cmd.Transaction = tr;
                cmd.CommandText = cmdText;

                cmd.ExecuteNonQuery();
                tr.Commit();
            }
            catch (MySqlException ex)
            {
                try
                {
                    tr.Rollback();
                }
                catch (MySqlException ex1)
                {
                    Console.WriteLine(ex1.ToString());
                }

                Console.WriteLine(ex.ToString());
            }

            this.Connection.Close();
            return this;
        }
        public ConnectionBuilder SelectEntry(string table, string[] columns, bool hasWhere = false, string[] where = null, string[] value = null, bool isSubselect = false)
        {

            where = where ?? new string[0];
            value = value ?? new string[0];

            if (!isSubselect)
            {
                this.Connection.ConnectionString = this.ConnectionString;
                this.Connection.Open();
            }
            
            string s = "";
            columns.ToList().ForEach(x => s += (columns.Last() != x) ? x + "," : x);
            string cmdText = String.Format("SELECT {0} FROM {1} ", s, table);

            if (!hasWhere)
                cmdText += String.Format("");
            else
            {
                cmdText += String.Format("WHERE ");
                for (int i = 0; i < where.Count(); i++)
                {
                    if (!(where.ToList().IndexOf(where.ToList().Last()) == i))
                    {
                        int possibleResult;
                        cmdText += String.Format("{0} = {1} AND", where[i], (int.TryParse(s, out possibleResult)) ? value[i] : "'" + value[i] + "'");
                    }
                    else
                    {
                        int possibleResult;
                        cmdText += String.Format("{0} = {1}", where[i], (int.TryParse(s, out possibleResult)) ? value[i] : "'" + value[i] + "'");
                    }
                }
            }

            Console.WriteLine(cmdText);

            try
            {
                MySqlCommand cmd = Connection.CreateCommand();
                cmd.CommandText = cmdText;                

                MySqlDataReader reader = cmd.ExecuteReader();

                Result = new MySQLResultSet();

                while (reader.Read())
                {
                    for(int i= 0; i < reader.FieldCount; i++)
                        Result.col.Add(reader.GetValue(i).ToString());
                }

                reader.Close();

            }
            catch (MySqlException ex)
            {
                Console.WriteLine(ex.ToString());
            }


            if (!isSubselect)
            {
                this.Connection.Close();
            }
            return this;
        }
    }
}
