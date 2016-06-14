using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using FSInterface;

namespace ThrottleCalibrationTest
{
    public partial class Form1 : Form
    {
        FSIClient fsi;
        public Form1()
        {
            fsi = new FSIClient("Throttle test");
            fsi.OnVarReceiveEvent += fsiOnVarReceive;
            fsi.ProcessWrites();

            InitializeComponent();
        }

        private void trackBar1_ValueChanged(object sender, EventArgs e)
        {
            double value = ((double)trackBar1.Value) / 10000.0;
            double eng1 = (Math.Pow(value,2) + 5.1) * 10123;
            double eng2 = (Math.Pow(value,5) - 123.1) * 12345;

            label1.Text = value.ToString();


            fsi.SLI_AC_STBY_BUS_PHASE_1_VOLTAGE = (float)eng1;
            fsi.SLI_AC_STBY_BUS_PHASE_2_VOLTAGE = (float)eng2;
            fsi.ProcessWrites();
        }


        private void fsiOnVarReceive(FSIID id)
        {

        }
    }
}
