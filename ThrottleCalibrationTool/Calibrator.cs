using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ThrottleCalibrationTool
{
    class Calibrator
    {
        List<double> eng_l_data;
        List<double> eng_r_data;
        Dictionary<int, double> crossCalibData;
        double crossCalibResolution = 0.01;

        bool calibrating = false;


        //linear callibration data
        double add_l = 0, mult_l = 1;
        double add_r = 0, mult_r = 1;

        public Calibrator()
        {
            eng_l_data = new List<double>();
            eng_r_data = new List<double>();
            crossCalibData = new Dictionary<int, double>();
        }

        public void startCalibration()
        {
            eng_l_data.Clear();
            eng_r_data.Clear();
            crossCalibData.Clear();

            calibrating = true;
        }

        public void stopCalibration(bool _singleCalibrate, bool _crossCalibrate)
        {
            if (!calibrating) return;
            calibrating = false;

            //resetting values
            add_l = 0;
            mult_l = 1;
            add_r = 0;
            mult_r = 1;

            //callibrate throttle lever
            double add_l_ = 0;
            double mult_l_ = 1;
            double min_l = 0, max_l = 0;
            double add_r_ = 0;
            double mult_r_ = 1;
            double min_r = 0, max_r = 0;

            getMinMax(out min_l, out max_l, out min_r, out max_r);

            //left throttle
            add_l_ = -min_l;
            if ((max_l - min_l) != 0)
                mult_l_ = 1/(max_l - min_l);

            //right throttle
            add_r_ = -min_r;
            if ((max_r - min_r) != 0)
                mult_r_ = 1 / (max_r - min_r);

            if (_singleCalibrate)
                singleCalibrate(add_l_, mult_l_, add_r_, mult_r_);
            if (_crossCalibrate)
                crossCalibrate();
        }

        public void putData(double leftEngine, double rightEngine)
        {
            if (calibrating)
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
        private void singleCalibrate(double add_l_, double mult_l_, double add_r_, double mult_r_)
        {
            for (int i = 0; i < eng_l_data.Count; i++)
            {
                eng_l_data[i] += add_l_;
                eng_l_data[i] *= mult_l_;

                eng_r_data[i] += add_r_;
                eng_r_data[i] *= mult_r_;
            }

            add_l = add_l_;
            mult_l = mult_l_;
            add_r = add_r_;
            mult_r = mult_r_;
        }


        private void crossCalibrate()
        {
            for (int i=0; i <= (1/crossCalibResolution); i++) {
                double e = i * crossCalibResolution;
                int index = getNearestIndex(e);

                if (index > -1) {
                    /*double eng1_ist = eng_l_data[index];
                    double eng1_ist2 = eng1_ist;

                    double eng2_ist = eng_r_data[index];
                    double eng2_ist2 = eng2_ist;

                    //lineare interpolöation eng1
                    if (e > eng1_ist) {
                        if (eng_l_data.Contains(index + 1)) {
                            eng1_ist2 = eng_l_data[index+1];
                            eng2_ist2 = eng_r_data[index + 1];
                        }
                    } else {
                        if (eng_l_data.Contains(index - 1))
                        {
                            eng1_ist2 = eng_l_data[index-1];
                            eng2_ist2 = eng_r_data[index - 1];
                        }
                    }
                    
                    double eng1_real = eng1_ist;
                    double eng2_real = eng2_ist;
                    if (Math.Abs(eng1_ist2 - eng1_ist) > 0.000001)
                    {
                        eng1_real = eng1_ist + (eng1_ist2 - eng1_ist) * Math.Abs(e - eng1_ist) / Math.Abs(eng1_ist2 - eng1_ist);
                        eng2_real = eng2_ist + (eng2_ist2 - eng2_ist) * Math.Abs(e - eng1_ist) / Math.Abs(eng1_ist2 - eng1_ist);
                    }*/

                    double diff = eng_l_data[index] - eng_r_data[index];
                    //double diff = eng1_real - eng2_ist;

                    crossCalibData.Add(i, diff);
                } else {
                    crossCalibData.Add(i, 0);
                }
            }
        }


        public double[] getCalibratedData(double leftEngine, double rightEngine)
        {
            //single callibration
            double eng1 = (leftEngine + add_l) * mult_l;
            double eng2 = (rightEngine + add_r) * mult_r;

            //cross calibration
            int cckey = getNextCrossCalibKey(eng1);
            if (crossCalibData.ContainsKey(cckey)) {
                double ccCalibVal = crossCalibData[cckey];

                //linear interpolation
                double ccCalibVal2 = ccCalibVal;
                double interp_dist = Math.Abs(cckey * crossCalibResolution - eng1);
                if (cckey * crossCalibResolution > eng1) {
                    if (crossCalibData.ContainsKey(cckey + 1))
                    {
                        ccCalibVal2 = crossCalibData[cckey + 1];
                    }
                } else {
                    if (crossCalibData.ContainsKey(cckey - 1))
                    {
                        ccCalibVal2 = crossCalibData[cckey - 1];
                    }
                }

                eng1 -= ccCalibVal + (ccCalibVal2 - ccCalibVal) * (interp_dist / crossCalibResolution);
            }

            return new double[] { eng1, eng2 };
        }

        private int getNearestIndex(double value) {
            double distance = 99999999.0;
            int saveIndex = -1;

            for (int i =0; i < eng_l_data.Count; i++) {
                if (Math.Abs(eng_l_data[i]- value) < distance) {
                    saveIndex = i;
                    distance = Math.Abs(eng_l_data[i] - value);
                }
            }
            return saveIndex;
        }

        private int getNextCrossCalibKey(double value) {
            double multip = 1 / crossCalibResolution;

            int value1 = (int)Math.Round(value * multip);
            int value2 = (int)Math.Round((value+ crossCalibResolution) * multip);

            if (Math.Abs(value2 - value* multip) < Math.Abs(value1 - value* multip)) {
                return value2;
            } else {
                return value1;
            }
        }
    }
}
