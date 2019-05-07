using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
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
        public void BeginTransaciton()
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
        public void RollBack()
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
        public void Commit()
        {
            _trans.Commit();
            _trans.Connection.Close();
            _trans = null;
        }

        /// <summary>
        /// Öffnet die Verbindung
        /// </summary>
        public void OpenDB()
        {
            _con = new MySqlConnection(DBSql.ConnectionString());
            _con.Open();
        }

        /// <summary>
        /// Schließt die Verbindung
        /// </summary>
        public void CloseDB()
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
        public int ExecuteNonQuery(string sql)
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
        public MySqlDataReader ExecuteReader(string sql)
        {
            MySqlCommand cmd = new MySqlCommand(sql, _con);
            return cmd.ExecuteReader();
        }

        /// <summary>
        /// Zum erhalt der letzten ID welche hinzugefügt wurde
        /// </summary>
        /// <returns></returns>
        public int GetLastInsertId()
        {
            MySqlCommand cmd = new MySqlCommand(DBSql.LastInsertId(), _con);
            return Convert.ToInt32(cmd.ExecuteScalar());
        }

        /// <summary>
        /// Beended die Connection, wenn using() verwendet wird
        /// </summary>
        public void Dispose()
        {
            _con?.Close();
        }

        /// <summary>
        /// Erstellt eine Where Bedingung für einen SQL-Befehl
        ///  welcher sich an den Properties des Objektes orientiert
        ///  
        /// Hierbei ist zu beachten, dass die Spalten in der Datenbank
        /// genauso heißen müssen wie die Propertys des Objektes
        /// </summary>
        /// <param name="o">Objekt welches durchlaufen wird</param>
        /// <returns></returns>
        public static string CreateWhere(object o)
        {
            ///Erstellt ein PropertyInfo Array, welches die anzahl einer Propierties
            ///des Objektes besitzt.
            ///Um an die Properties zu gelangen, muss man sich erst den Type des Objektes holen
            PropertyInfo[] pia = o.GetType().GetProperties();

            ///Erstellt einen StringBuilder, welcher im Folgenden die Where-Bedinung erstellt
            StringBuilder where = new StringBuilder();
            where.Append(" Where");

            ///Für jede PropertyInfo innerhalb des zuvor erstellten Arrays, wird diese Schleife nun durchlaufen
            foreach(PropertyInfo pi in pia)
            {
                ///Sofern der PropertyType nicht der Typ des Objektes ist
                ///Zum Beispiel bei einer Variable vom Typ des Objektes (Ticket in Ticket oder so)
                if (pi.PropertyType != o.GetType())
                {
                    ///Locale Variable der Übersicht halber
                    object obj = pi.GetValue(o);

                    ///Wenn die Property ein DateTime ist
                    if (obj is DateTime date)
                        where.Append($" {pi.Name} = '{date.ToString("yyyy-MM-dd")}' AND");

                    ///Wenn die Property ein enum ist
                    else if(pi.PropertyType.IsEnum)
                        ///Der Cast für (int) ist nötigt um nicht die Value des Enum zu erhalten
                        where.Append($" {pi.Name} = '{(int)obj}' AND");

                    ///Für alle anderen PropertyTypen
                    else
                        where.Append($" {pi.Name} = '{obj.ToString()}' AND");
                }
            }
            ///Löscht die letzten 4 Zeichen des String
            ///in diesem Fall' AND'
            where.Remove(where.Length - 4, 4);
            return where.ToString();
        }

        /// <summary>
        /// Erstellt eine Liste um alle Datensätze auszulesen
        /// Hierbei kann sie oft verwendet werden, da kein spezieller Typ festgelegt ist
        /// </summary>
        /// <typeparam name="T">Typ/Name der Tabelle</typeparam>
        /// <param name="sql">SQL Befehl welcher definiert was wir auslesen wollen</param>
        /// <returns></returns>
        public List<T> CreateRead<T>(string sql)
        {
            ///Liste vom Typ T erstellen
            List<T> list = new List<T>();

            ///Reader ausführen
            MySqlDataReader reader = ExecuteReader(sql);

            ///Solange der Reader etwas liest
            while (reader.Read())
            {
                ///Neues Object von der Instanz T (Neues Objekt~oder so)
                ///Hierbei ist ein Standard Konstruktor nötig, da die Variablen nicht gefüllt werden wollen
                ///Wir wollen hierbei nur auf die PropertyInfo zugreifen. Uns ist egal was mit dieser Instanz passiert
                object obj = Activator.CreateInstance(typeof(T), null);

                ///Für jedes Feld im Reader (Spalte in der Datenbank)
                for(int i = 0; i<reader.FieldCount; i++)
                {
                    ///Eine string Variable vom Namen der Spalte des Reader
                    ///dient der vereinfachung des Codes
                    string name = reader.GetName(i);
                    ///Wir legen eine PropertyInfo an, welche vom Typ T die Property erhält mit dem Namen, 
                    /// welchen wir oben herausgsucht haben
                    PropertyInfo pi = typeof(T).GetProperty(name);
                    ///Füllt den Wert des Objectes mit der Value welche an dieser Stelle des Readers steht
                    pi.SetValue(obj, reader[i]);
                }
                ///Fügt der Liste das Object hinzu, wobei dieses vorher noch zum Typ T gecastet wird
                list.Add((T)obj);
            }
            reader.Close();
            return list;
        }

        /// <summary>
        /// Erstellt einen Delete-Befehl für das angegebene Objekt
        /// Hierbei ist wichtig, dass der Typ des Objektes ebenso heißt wie der Tabellenname
        /// </summary>
        /// <param name="obj">Objekt welches gelöscht werden soll</param>
        /// <returns></returns>
        public bool CreateDelete(object obj)
        {
            ///Ermittelt den Typ des Übergebenen Objektes
            Type t = obj.GetType();

            ///StringBuilder zum erhalt des SQL Befehls
            StringBuilder delete = new StringBuilder();
            ///DELETE FROM {Tabellenname} {Where...}
            delete.Append($"DELETE FROM {t.ToString()} {CreateWhere(obj)}");

            ///Sofern nur eine Zeile bearbeitet wurde
            ///ist alles gut und es gibt 'true' zurück
            ///Ansonsten gibts einen Fehler und 'false' wird zurück gegeben
            return ExecuteNonQuery(delete.ToString()) == 1;

        }

    }
}
