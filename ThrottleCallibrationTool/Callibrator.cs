using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ThrottleCallibrationTool
{
    class Callibrator
    {
        List<double> eng_l_data;
        List<double> eng_r_data;
        bool callibrating = false;


        //linear callibration data
        double add_l = 0, mult_l = 1;
        double add_r = 0, mult_r = 1;

        public Callibrator()
        {
            eng_l_data = new List<double>();
            eng_r_data = new List<double>();
        }

        public void startCallibration()
        {
            eng_l_data.Clear();
            eng_r_data.Clear();

            callibrating = true;
        }

        public void stopCallibration()
        {
            callibrating = false;

            //callibrate throttle lever
            add_l = 0;
            mult_l = 1;
            double min_l = 0, max_l = 0;
            add_r = 0;
            mult_r = 1;
            double min_r = 0, max_r = 0;

            getMinMax(out min_l, out max_l, out min_r, out max_r);

            //left throttle
            add_l = -min_l;
            if ((max_l - min_l) != 0)
                mult_l = 1/(max_l - min_l);

            //right throttle
            add_r = -min_r;
            if ((max_r - min_r) != 0)
                mult_r = 1 / (max_r - min_r);


        }

        public void putData(double leftEngine, double rightEngine)
        {
            if (callibrating)
            {
                eng_l_data.Add(leftEngine);
                eng_r_data.Add(rightEngine);
            }
        }

        private void getMinMax(out double min_l, out double max_l, out double min_r, out double max_r)
        {
            min_l = 99999999;
            max_l = -99999999;

            foreach(double d in eng_l_data)
            {
                if (d > max_l)
                    max_l = d;
                if (d < min_l)
                    min_l = d;
            }

            min_r = 99999999;
            max_r = -99999999;

            foreach (double d in eng_r_data)
            {
                if (d > max_r)
                    max_r = d;
                if (d < min_r)
                    min_r = d;
            }
        }

        //callibrates both throttle levers single
        private void singleCallibrate(double add_l, double mult_l, double add_r, double mult_r)
        {
            for (int i = 0; i < eng_l_data.Count; i++)
            {
                eng_l_data[i] += add_l;
                eng_l_data[i] *= mult_l;

                eng_r_data[i] += add_r;
                eng_r_data[i] *= mult_r;
            }
        }


        private void crossCallibrate()
        {

        }


        public double[] getCallibratedData(double leftEngine, double rightEngine)
        {
            return new double[] { (leftEngine + add_l) * mult_l, (rightEngine + add_r) * mult_r };
        }
    }
}
