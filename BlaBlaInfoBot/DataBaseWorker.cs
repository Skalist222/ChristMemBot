using System.Data;
using System.Data.OleDb;
using static System.Runtime.InteropServices.JavaScript.JSType;
using System.Xml.Linq;
using static Constantdata.ConstData;
using System.Diagnostics;
using UserWorker;
using Telegram.Bot.Types;
using BlaBlaInfoBot;
using MemWorkerSpace;

namespace DataBaseWorker
{
    public class Base
    {
        private static string conStr = "Provider=Microsoft.ACE.OLEDB.12.0;Data Source=" + PathDB;
        private static OleDbConnection con = new OleDbConnection(conStr);
        public static DataTable? Select(string select)
        {
            try
            {
                con.Open();
                OleDbDataAdapter adapter = new OleDbDataAdapter(select, con);
                DataTable table = new DataTable();
                adapter.Fill(table);
                con.Close();
                return table;
            }
            catch (Exception e)
            {
                Debug.WriteLine("SELECT ERROR!!!!!! DATABASE");
                Debug.WriteLine(e.Message);
                try { con.Close(); }catch { }
                return null;
            }

        }
        public static bool Execute(string select)
        {
            try 
            {
                con.Open();
                OleDbCommand com = new OleDbCommand(select, con); 
                com.ExecuteNonQuery();
                con.Close();
                return true;
            }
            catch (Exception e)
            {
                Debug.WriteLine("EXECUTE ERROR!!!!!! DATABASE");
                Debug.WriteLine(e.Message);
                try { con.Close(); } catch { }
                return false;
            }
        }
        //Memases
        public static string? GetRandomIdMem()
        {
            DataTable t = Select($"Select IdPhoto From Memases");
            if (t.Rows.Count > 0)
            {
                Random r = new Random();
                int selectedId = r.Next(0, t.Rows.Count);
                return t.Rows[selectedId][0].ToString();
            }
            else return null;
            

        }
        public static bool CreateNewMem(string idMem)
        {
            return Execute($"Insert Into Memases(idPhoto) values('{idMem}')");
        }
        //Users
        public static DataTable? SelectAllUsers() 
        {
            return Select("Select * From users");
        }
        public static bool HaveUser(long id)
        {
            try
            {
                DataTable t = Select("Select * from users where id='" + id+"'");
                if (t is null) return false;
                return t.Rows.Count == 1;
            }
            catch (Exception e)
            {
                Debug.WriteLine(e.Message);
                return false;
            }
        }
        public static bool CreateUser(UserI user)
        {
            try
            {
                string lastStih = user.LastStih is not null ? user.LastStih.AddressText : "-";
                string add = user.AddedMem ? 1+"" : 0+"";
                string commandS = $"Insert Into users values('{user.Id}','{user.Name}','{user.LastName}','{user.Firstname}',{user.AddedMem},'{lastStih}')";
                return Execute(commandS);
            }
            catch (Exception e)
            {
                Debug.WriteLine(e.Message);
                return false;
            }
        }
        public static bool RedactLastStih(long idUser,string newStih)
        {
            try
            {
                string commandS = "Update users Set lastStih = '"+newStih+"' where id ='"+ idUser + "'";
                return Execute(commandS);
            }
            catch (Exception e)
            {
                Debug.WriteLine(e.Message);
                return false;
            }
        }
        public static bool RedactLastGold(long idUser, string idLastGold)
        {
            try
            {
                string commandS = "Update users Set lastAddGold = " + idLastGold + " where id ='" + idUser + "'";
                return Execute(commandS);
            }
            catch (Exception e)
            {
                Debug.WriteLine(e.Message);
                return false;
            }
        }
        //GoldStih
        public static DataTable GetAllGoldverses()
        {
            DataTable t = Select("Select * From Golds");
            return t;
        }
        public static bool CreateGold(Verse s)
        {
            try
            {
                string commandS = $"Insert into Golds (address,textI) values('{s.AddressText}','{s.Text}');";
                return Execute(commandS);
            }
            catch (Exception e)
            {
                Debug.WriteLine(e.Message);
                return false;
            }
        }
        //Commands
        public static TextCommand GetCommand(string idCommand)
        {
            DataTable t = Select($"Select * From Commands Where id ={idCommand}");
            if (t is not null && t.Rows.Count > 0)
            {
                TextCommand tCommand = new TextCommand(Convert.ToInt32(idCommand), t.Rows[0][1].ToString());
                return tCommand;
            }
            else return null;
            

        }
        public static string GetStringsResponce(string command,char split=' ')
        {
            DataTable t = Select($"Select id From Commands where textCommand = '{command}'");
            try
            {
                string idCommand = t.Rows[0][0].ToString();
                t = Select($"Select word From Responce_word where idCommand = {idCommand}");
                string splittedString = "";
                foreach (DataRow r in t.Rows)
                {
                    splittedString += r[0].ToString() + split;
                }
                splittedString = splittedString.Substring(0, splittedString.Length - 1);
                return splittedString;
            }
            catch
            {
                return null;
            }
        }
        public static string GetStringsAnsveres(string command, char split = ' ')
        {
            DataTable t = Select($"Select id From Commands where textCommand = '{command}'");
            try
            {
                string idCommand = t.Rows[0][0].ToString();
                t = Select($"Select word From Ansvere_word where idCommand = {idCommand}");
                string splittedString = "";
                foreach (DataRow r in t.Rows)
                {
                    splittedString += r[0].ToString() + split;
                }
                splittedString = splittedString.Substring(0, splittedString.Length - 1);
                return splittedString;
            }
            catch
            {
                return null;
            }
        }
        public static string? GetRandomAnswere(string command)
        {
            DataTable t = Select($"Select id From Commands where textCommand = '{command}'");
            try
            {
                string idCommand = t.Rows[0][0].ToString();
                t = Select($"Select word From Ansvere_word where idCommand = {idCommand}");
               
                Random r = new Random();
                int num = r.Next(0,t.Rows.Count);
              
                return t.Rows[num][0].ToString();
            }
            catch(Exception e)
            {
                return null;
            }
        }
    }
}
