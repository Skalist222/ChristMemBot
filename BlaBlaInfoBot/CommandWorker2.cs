using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CommandWorker2
{
    abstract class Responce
    {
        string Text { get; set; }
        
    }
    interface IAsverer
    {
        Responce GetAnsver(string responce);

    }
}
