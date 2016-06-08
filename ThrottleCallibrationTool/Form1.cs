﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using FSInterface;

namespace ThrottleCallibrationTool
{
    public partial class Form1 : Form
    {
        private Callibrator callibrator;
        private FSIClient fsi;
        delegate void SetDoubleCallback(double eng_l, double eng_r);

        public Form1()
        {
            fsi = new FSIClient("Throttle Callibration Tool");
            fsi.OnVarReceiveEvent += fsiOnVarReceive;
            fsi.DeclareAsWanted(new FSIID[]
                {
                    FSIID.SLI_AC_STBY_BUS_PHASE_1_VOLTAGE,
                    FSIID.SLI_AC_STBY_BUS_PHASE_2_VOLTAGE
                }
            );
            fsi.ProcessWrites();

            callibrator = new Callibrator();
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            callibrator.startCallibration();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            callibrator.stopCallibration();
        }

        private void fsiOnVarReceive(FSIID id)
        {
            double eng_l = fsi.SLI_AC_STBY_BUS_PHASE_1_VOLTAGE;
            double eng_r = fsi.SLI_AC_STBY_BUS_PHASE_2_VOLTAGE;

            updateProgressBars(eng_l, eng_r);
        }


        private void updateProgressBars(double eng_l, double eng_r)
        {
            
            if (progressBar1.InvokeRequired)
            {
                SetDoubleCallback d = new SetDoubleCallback(updateProgressBars);
                Invoke(d, new object[] { eng_l, eng_r });
            }
            else
            {
                //display both engine values
                progressBar1.Value = doubleToProgress(eng_l * 1000);
                progressBar2.Value = doubleToProgress(eng_r * 1000);

                //put into callibration tool
                callibrator.putData(eng_l, eng_r);

                //get callibrated values
                double[] c = callibrator.getCallibratedData(eng_l, eng_r);
                progressBar3.Value = doubleToProgress(c[0] * 1000);
                progressBar4.Value = doubleToProgress(c[1] * 1000);
            }
        }

        private int doubleToProgress(double value)
        {
            int c = Convert.ToInt32(value);

            if (c > 1000)
            {
                c = 1000;
            }
            if (c < 0)
            {
                c = 0;
            }

            return c;
        }
    }
}
