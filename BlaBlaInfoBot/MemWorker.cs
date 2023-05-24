namespace MemWorkerSpace
{
    internal class MemWorker
    {
        string[] list;

        public MemWorker(string path)
        {
            string[] l = Directory.GetFiles(path);
            if (l.Length > 0)
            {
                list = l;
            }
        }
        public string GetRandomPathMem()
        {
            Random r = new Random();
            int i = r.Next(0,list.Length-1);
            return list[i];
        }
        public string GetRandomVPPathMem(string validate,bool parametrNotFullText=false)
        {
            if (!parametrNotFullText)
            {
                try { validate = validate.Split('.')[1]; }
                catch { return ""; }
                
            }
            VPList validVP = new VPList();
            //foreach (VP vp in listVP)
            //{
            //    foreach (string parametr in vp.Parametrs)
            //    {
            //        if (validate.ToLower().IndexOf(validate.ToLower()) != -1) 
            //            validVP.Add(vp);
            //    }
            //}
            if (validVP.Count == 0) return "";
            Random r = new Random();
            int i = r.Next(0, validVP.Count);
            return validVP[i].PathFile;
        }
        public int Length { get { return list.Count(); } }
    }

    public class VP
    {
        string[] parametrs;
        string path;

        public string PathFile { get { return path; } }
        public string[] Parametrs { get { return parametrs; } }

        public VP(string[] parametrs, string path)
        {
            this.parametrs = parametrs;
            this.path = path;
        }
        public void WriteInFile(string path)
        {
            string line = path;
            foreach (string p in parametrs)
            {
                line += "|" + p;
            }
            File.WriteAllText(path, line+Environment.NewLine);
        }
    }
    public class VPList : List<VP> 
    {
        public VPList()
        {
            
        }
        public VPList(string[] array):base()
        {
            foreach (string s in array)
            {
                string[] infos = s.Split('|');
                string name = infos[0];
                string[] parametrs = infos.Skip(1).ToArray();
                Add(new VP(parametrs,name));
            }
        }
    }
}
