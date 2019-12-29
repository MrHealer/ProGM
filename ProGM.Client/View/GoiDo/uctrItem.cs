using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using ProGM.Client.Model;
using ProGM.Business.Model;
using System.Net;

namespace ProGM.Client.View.GoiDo
{
    public partial class uctrItem : UserControl
    {
        private Food food;
        FoodItemCallback foodItemCallback;
        public uctrItem(Food food)
        {
            InitializeComponent();
            this.food = food;
            ptImage.Image = food.ImageThumbnail;
            lbName.Text = food.strName;
            String price = food.iPrice.ToString();
            if (price.Length > 3)
            {
                price = price.Insert(price.Length - 3, ".");
            }
            lbPrice.Text = price;
            pictureEdit1.Parent = ptImage;
            pictureEdit1.BackColor = Color.Transparent;
           
        }
        public void SetCallback(FoodItemCallback foodItemCallback)
        {
            this.foodItemCallback = foodItemCallback;
        }


        public Productlist Food { get => food; }

        private void btnBuyNow_Click(object sender, EventArgs e)
        {
            foodItemCallback.buyNow(food);
        }

        private void btnAddToCart_Click(object sender, EventArgs e)
        {
            foodItemCallback.addToCart(food);
        }
        
        public void updateImage(Image image)
        {
            ptImage.Image = image;
        }
    }
}
