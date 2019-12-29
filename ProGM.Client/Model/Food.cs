using ProGM.Business.Model;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProGM.Client.Model
{
    public class Food : Productlist
    {
        private Image imageThumbnail;

        public Image ImageThumbnail { get => imageThumbnail; set => imageThumbnail = value; }

        public Food(Productlist productlist)
        {
            this.iActive = productlist.iActive;
            this.iPrice = productlist.iPrice;
            this.strCategoryId = productlist.strCategoryId;
            this.strCreatedByAccountId = productlist.strCreatedByAccountId;
            this.strCreatedDate = productlist.strCreatedDate;
            this.strDesc = productlist.strDesc;
            this.strId = productlist.strId;
            this.strModifiedByAccountId = productlist.strModifiedByAccountId;
            this.strModifiedDate = productlist.strModifiedDate;
            this.strName = productlist.strName;
            this.strTag = productlist.strTag;
            this.strThumbnail = productlist.strThumbnail;
        }
    }
        
}
