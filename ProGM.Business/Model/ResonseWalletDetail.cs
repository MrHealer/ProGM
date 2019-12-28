using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProGM.Business.Model
{
    public class ResonseWalletDetail
    {
        public Walletdetail[] walletDetail { get; set; }
    }

    public class Walletdetail
    {
        public string strId { get; set; }
        public string dBalance { get; set; }
    }

}
