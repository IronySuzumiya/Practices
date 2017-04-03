using FittingStraightLine.Properties;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace FittingStraightLine
{
    public partial class MainForm : Form
    {
        List<Bitmap> images = new List<Bitmap>();

        public MainForm()
        {
            InitializeComponent();
        }

        private void pnlImage_Paint(object sender, PaintEventArgs e)
        {
            e.Graphics.ScaleTransform(2.2f, 5.9f);
            e.Graphics.DrawImage(images[cmbBitmaps.SelectedIndex], 0, 0);
        }

        private void MainForm_Load(object sender, EventArgs e)
        {
            var fs = typeof(Resources).GetProperties();
            foreach (var f in fs)
            {
                if(f.PropertyType == typeof(Bitmap))
                {
                    cmbBitmaps.Items.Add(f.Name);
                    var bitmap = f.GetValue(null) as Bitmap;
                    ImageProcesser.Process(bitmap);
                    images.Add(bitmap);
                }
            }
            cmbBitmaps.SelectedIndex = 0;
        }

        private void cmbBitmaps_SelectedIndexChanged(object sender, EventArgs e)
        {
            pnlImage.Refresh();
        }
    }
}
