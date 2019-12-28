using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProGM.Business.Model
{
    public class Messeage
    {
        public string idUserSend { set; get; }
        public string userSend { set; get; }
        public string idUserReceive { set; get; }
        public string userReceive { set; get; }
        public string content { set; get; }
        public string mac { set; get; }
    }
}
