using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Library.Database
{
    class DBSql
    {
        /// <summary>
        /// Return the ConnectionString
        /// </summary>
        /// <returns></returns>
        public static string ConnectionString()
        {
            return "server=127.0.0.1;user id=root;database=musikdatenbank";
        }

        /// <summary>
        /// Return the Last inserted ID
        /// </summary>
        /// <returns></returns>
        public static string LastInsertId()
        {
            return "Select LAST_INSERT_ID()";
        }
    }
}
