using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace FittingStraightLine
{
    public partial class MainForm : Form
    {
        Bitmap bitmap = new Bitmap(Properties.Resources.十字);

        public MainForm()
        {
            InitializeComponent();
        }

        private void pnlImage_Paint(object sender, PaintEventArgs e)
        {
            e.Graphics.ScaleTransform(2.2f, 5.9f);
            e.Graphics.DrawImage(bitmap, 0, 0);
        }

        unsafe private void MainForm_Load(object sender, EventArgs e)
        {
            var data = bitmap.LockBits(new Rectangle(0, 0, bitmap.Width, bitmap.Height)
                , ImageLockMode.ReadWrite, PixelFormat.Format24bppRgb);
            for(int j = 0; j < data.Height; ++j)
            {
                for(int i = 0; i < data.Width; ++i)
                {
                    var color = (byte*)data.Scan0 + j * data.Stride + i * 3;
                    // do some thing
                }
            }
            bitmap.UnlockBits(data);
        }
    }
}
