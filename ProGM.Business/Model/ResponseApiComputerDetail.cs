using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProGM.Business.Model
{
    public class ResponseApiComputerDetail
    {
        public Computedetail[] computeDetail { get; set; }
    }

    public class Computedetail
    {
        public string strId { get; set; }
        public string strGroupId { get; set; }
        public string strName { get; set; }
        public string strLocalIp { get; set; }
        public string strMacAddress { get; set; }
        public string strComment { get; set; }
        public string iActive { get; set; }
        public string strCreatedDate { get; set; }
        public string strCreatedByAccountId { get; set; }
        public string strModifiedDate { get; set; }
        public string strModifiedByAccountId { get; set; } 
        public string strManagerPcIP { get; set; }
        public string strManagerPcMac { get; set; }
        public decimal iPrice { get; set; }
    }

}
