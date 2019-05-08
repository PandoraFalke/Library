using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
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
            foreach (PropertyInfo pi in pia)
            {
                ///Sofern der PropertyType nicht der Typ des Objektes ist
                ///Zum Beispiel bei einer Variable vom Typ des Objektes (Ticket in Ticket oder so)
                if (pi.PropertyType != o.GetType())
                    where.Append($" {pi.Name} = {ObjectToString(pi.GetValue(o))} AND");
            }
            ///Löscht die letzten 4 Zeichen des String
            ///in diesem Fall' AND'
            where.Remove(where.Length - 4, 4);
            return where.ToString();
        }

        /// <summary>
        /// Erstellt einen Delete-Befehl für das angegebene Objekt
        /// Hierbei ist wichtig, dass der Typ des Objektes ebenso heißt wie der Tabellenname
        /// </summary>
        /// <param name="obj">Objekt welches gelöscht werden soll</param>
        /// <returns></returns>
        public string CreateDelete(object obj)
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
            return delete.ToString();

        }

        /// <summary>
        /// Erstellt einen Insert-SQL-Befehl, welcher sich nach den Properties des ObjectTypes richtet
        /// </summary>
        /// <param name="obj">Das Object welches in die Datenbank übernommen werden soll</param>
        /// <returns></returns>
        public string CreateInsert(object obj)
        {
            ///Solange das Object nicht leer ist
            if (obj != null)
            {
                ///Typ des Objectes ermitteln
                Type t = obj.GetType();
                ///Property Array erstellen
                PropertyInfo[] pia = t.GetProperties();
                ///StringBuilder für den SQL-Befehl erstellen
                StringBuilder insert = new StringBuilder();
                insert.Append($"INSERT INTO {t.Name} (");
                ///Für jede PropertyInfo in dem zuvor erstellten Array
                foreach (PropertyInfo pi in pia)
                {
                    ///Sofern die Property nicht vom gleichen Typ ist, wie das Object
                    if (pi.PropertyType != t)
                    {
                        ///Aussage hinzufügen
                        insert.Append($"{pi.Name}, ");
                    }
                }
                ///Löscht die letzten 2 Zeichen des Strings: ", "
                insert.Remove(insert.Length - 2, 2);
                insert.Append($") VALUES (");
                ///Für jede PropertyInfo in dem zuvor erstellten Array
                foreach (PropertyInfo pi in pia)
                {
                    ///Sofern die Property nicht vom gleichen Typ ist, wie das Object
                    if (pi.PropertyType != t)
                    {
                        ///Aussage hinzufügen
                        insert.Append($"{ObjectToString(pi.GetValue(obj))}, ");
                    }
                }
                insert.Remove(insert.Length - 2, 2);
                insert.Append(")");
                return insert.ToString();
            }
            ///Exception sofern das übergebene Object 'null'/leer ist
            else
            {
                throw new Exception("Object == null");
            }
        }

        /// <summary>
        /// Dient zum erhalt der Values des Objektes als String
        /// Je nach Type der Propertie wird ein anderer String zurück gegeben
        /// </summary>
        /// <param name="obj">Das Objekt welches ausgelesen werden soll</param>
        /// <returns></returns>
        public static string ObjectToString(object obj)
        {
            ///Sollte das Object/die Value von DateTime sein
            if (obj is DateTime date)
                return $"'{date.ToString("yyyy-MM-dd")}'";
            ///Sollte das Object/die Value eine Enum sein
            else if (obj.GetType().IsEnum)
                return $"{(int)obj}";
            ///In allen anderen Fällen
            else
                return $"'{obj}'";
        }

    }
}
