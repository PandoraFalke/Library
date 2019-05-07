using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
namespace Library.Database
{
    class DBConnection : IDisposable
    {
        MySqlConnection _con;
        static MySqlTransaction _trans;

        public DBConnection()
        {
            OpenDB();
        }

        /// <summary>
        /// Transaktion beginnen
        /// Jede Transaktion hat eien feste Connectino, die NICHT geschlossen
        /// werden darf, bevor die Transaktion fertig ist.
        /// </summary>
        void BeginTransaciton()
        {
            MySqlConnection con = new MySqlConnection(DBSql.ConnectionString());
            con.Open();
            _trans = con.BeginTransaction();
        }

        /// <summary>
        /// Macht die letzte Aktion der Transaktion rückgängig
        /// Schließt die interen Verbinung
        /// Setzt die Transaktion auf 'null'
        /// </summary>
        void RollBack()
        {
            _trans.Rollback();
            _trans.Connection.Close();
            _trans = null;
        }

        /// <summary>
        /// Führt die Transaktion aus
        /// Schließt die interne Verbindung
        /// Setzt die Transaktion auf 'null'
        /// </summary>
        void Commit()
        {
            _trans.Commit();
            _trans.Connection.Close();
            _trans = null;
        }

        /// <summary>
        /// Öffnet die Verbindung
        /// </summary>
        void OpenDB()
        {
            _con = new MySqlConnection(DBSql.ConnectionString());
            _con.Open();
        }

        /// <summary>
        /// Schließt die Verbindung
        /// </summary>
        void CloseDB()
        {
            _con.Close();
        }

        /// <summary>
        /// Führt einen SQL Befehl aus
        /// Entweder über die Connection 
        /// oder über die Transaktion
        /// gibt die Anzahl der bearbeiteten Zeilen zurück
        /// </summary>
        /// <param name="sql">Sql Befehl welcher durchgeführt wird</param>
        /// <returns></returns>
        int ExecuteNonQuery(string sql)
        {
            int anz = 0;
            if (_trans == null)
            {
                MySqlCommand cmd = new MySqlCommand(sql, _con);
                anz = cmd.ExecuteNonQuery();
            }
            else
            {
                MySqlCommand cmd = new MySqlCommand(sql, _trans.Connection, _trans);
                anz = cmd.ExecuteNonQuery();
            }
            return anz;
        }

        /// <summary>
        /// Führt einen Reader aus und gibt diesen Zurück an den Sender
        /// </summary>
        /// <param name="sql">SQL Anweisung welche bearbeitet werden soll</param>
        /// <returns></returns>
        MySqlDataReader ExecuteReader(string sql)
        {
            MySqlCommand cmd = new MySqlCommand(sql, _con);
            return cmd.ExecuteReader();
        }

        /// <summary>
        /// Zum erhalt der letzten ID welche hinzugefügt wurde
        /// </summary>
        /// <returns></returns>
        int GetLastInsertId()
        {
            MySqlCommand cmd = new MySqlCommand(DBSql.LastInsertId(), _con);
            return Convert.ToInt32(cmd.ExecuteScalar());
        }

        /// <summary>
        /// Beended die Connection, wenn using() verwendet wird
        /// </summary>
        public void Dispose()
        {
            _con.Close();
        }
    }
}
