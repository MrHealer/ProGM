using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProGM.Business.Model
{
    public class ProductResponse
    {
        public Productlist[] productList { get; set; }
    }

    public class Productlist
    {
        public string strId { get; set; }
        public string strCategoryId { get; set; }
        public string strTag { get; set; }
        public string strName { get; set; }
        public string strThumbnail { get; set; }
        public string strDesc { get; set; }
        public int iPrice { get; set; }
        public string iActive { get; set; }
        public string strCreatedDate { get; set; }
        public string strCreatedByAccountId { get; set; }
        public string strModifiedDate { get; set; }
        public string strModifiedByAccountId { get; set; }
    }
}
