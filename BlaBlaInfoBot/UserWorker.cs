using BlaBlaInfoBot;
using System.Xml;
using System.Xml.Linq;
using DataBaseWorker;

using static Constantdata.ConstData;
using System.Data;
using BotWorkerSpace;

namespace UserWorker
{
    public class UserI
    {
        long id;
        string name;
        string firstname;
        string lastName;
        Verse lastStih = null;
        Verse lastAddGold = null;
        int countExpletive = 0;
        public Verse LastStih { get { return lastStih; }
            set {
                lastStih = value;
                Base.RedactLastStih(Id,value.AddressText);
            }
        }
        public Verse LastAddGold { get { return lastAddGold; }
            set 
            {
                lastAddGold = value;
                Base.RedactLastGold(id,value.AddressText);
            }
        }
        public bool AddedMem { get; set; }
        public long Id { get { return id; } }
        public string Name { get { return name; } }
        public string Firstname { get { return firstname; } }
        public string LastName { get { return lastName; } }
        public bool Expletive()
        {
            if (countExpletive == 10) return true;
            else { countExpletive++; return false; }
        }
        public UserI(long id, string name, string firstname, string lastName, BibleWorker bw, string addmem="False", string lastStihAddress="-")
        {
            this.id = id;
            this.name = name;
            this.firstname = firstname;
            this.lastName = lastName;
            AddedMem = addmem.ToLower()=="true";
            if (lastStihAddress != "-") LastStih = bw.GetVerse(lastStihAddress);
        }
        public UserI(DataRow row,BibleWorker bw)
        {
            this.id = Int64.Parse(row["id"].ToString());
            this.name = row["nick"].ToString();
            this.firstname = row["firstName"].ToString();
            this.lastName = row["lastName"].ToString();
            string lastStihAddress = row["lastStih"].ToString();
            this.AddedMem = row["memAdded"].ToString() == "True";
            
            if(lastStihAddress!="-") LastStih = bw.GetVerse(lastStihAddress);
        }

    }
    public class UserList : List<UserI>
    {
        public UserList(DataTable table, BibleWorker bw)
        {
            foreach (DataRow r in table.Rows)
            {
                base.Add(new UserI(r, bw));
            }
        }
        public UserList(BibleWorker bw) : this(Base.SelectAllUsers(), bw) { }
        public void Add(UserI user) 
        {
            base.Add(user);
            Base.CreateUser(user);
        }
        public UserI? this[long id]
        {
            get {
                foreach (UserI u in this)
                {
                    if (u.Id == id) 
                        return u;
                }
                return null;
            }
        }
    }
}


