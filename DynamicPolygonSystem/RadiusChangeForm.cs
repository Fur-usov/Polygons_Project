using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace DynamicPolygonSystem
{
    public delegate void DelRadiusChanged(object sender, RadiusEventArgs e);

    public partial class RadiusChangeForm : Form
    {
        public event DelRadiusChanged dRC;
        public static bool _isCreated;

        public RadiusChangeForm()
        {
            InitializeComponent();

            this.trackBar1.Value = Shape._radius;
        }

        static RadiusChangeForm()
        {
            _isCreated = false;
        }

        private void RadiusChangeForm_Load(object sender, EventArgs e)
        {

        }

        private void trackBar1_Scroll(object sender, EventArgs e) => dRC(this, new RadiusEventArgs(trackBar1.Value));

        private void RadiusChangeForm_FormClosed(object sender, FormClosedEventArgs e)
        {
            RadiusChangeForm._isCreated = false;
        }
    }

    public class RadiusEventArgs : EventArgs
    {
        private int _radius;

        public RadiusEventArgs(int radius)
        {
            try
            {
                if (radius > 0)
                    _radius = radius;
                else throw new Exception("Radius can't be less or equal to zero");
            }
            catch (Exception expn)
            {
                MessageBox.Show(expn.Message);
            }
        }

        public int Radius
        {
            get => _radius;
        }
    }
}
