using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProGM.Business.Model
{
    public class CategoryListResponse
    {
        public Productcategorylist[] productCategoryList { get; set; }
    }

    public class Productcategorylist
    {
        public string strId { get; set; }
        public string strCompanyId { get; set; }
        public string strParentId { get; set; }
        public string strName { get; set; }
        public string strThumbnail { get; set; }
        public string strDesc { get; set; }
        public string iActive { get; set; }
        public string strCreatedDate { get; set; }
        public string strCreatedByAccountId { get; set; }
        public string strModifiedDate { get; set; }
        public string strModifiedByAccountId { get; set; }
    }

}
