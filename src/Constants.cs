﻿using System;

namespace Landis.Extension.Succession.Density
{
    public class Constants 
    {
        public static float MC = 12;                        // Molecular weight of C
        public static float MCO2 = 44;                      // Molecular weight of CO2
        public static int SecondsPerHour = 60 * 60;         // Seconds per hour
        public static int billion = 1000000000;             // Bilion 
        public static  float MCO2_MC = MCO2 / MC;           // Molecular weight of CO2 relative to C 
            
        public enum Months
        {
            January = 1,
            February,
            March,
            April,
            May,
            June,
            July,
            August,
            September,
            October,
            November,
            December
        }

        // For Permafrost
        public static float cs = 1942;                  //heat capacity solid	kJ/m3/K (Farouki 1986 in vanLier and Durigon 2013)
        public static float cw = 4186;                  //heat capacity water	kJ/m3/K (vanLier and Durigon 2013)
        public static float lambda_a = 2.25F;           //thermal conductivity air	kJ/m/d/K (vanLier and Durigon 2013)
        public static float lambda_w = 51.51F;          //thermal conductivity water    kJ/m/d/K (vanLier and Durigon 2013)
        public static float lambda_clay = 80F;          //thermal conductivity clay    kJ/m/d/K (Michot et al. 2008 in vanLier and Durigon 2013)
        public static float lambda_0 = 360F;            //thermal conductivity sand-silt	kJ/m/d/K (Gemant 1950 in vanLier and Durigon 2013)
        public static float gs = 0.125F;                //(Farouki 1986 in vanLier and Durigon 2013)
        public static float omega = (float) System.Math.PI * 2 / 12;   // angular velocity of earth (monthly rotation) radians/month
        public static float tau = 12F;                  //length of temp record     months
    }

/*    public class biomassUtil
    {
        private float[] biomassData;

        private int biomassNum;

        private float biomassThreshold;

        public biomassUtil()
        {


        }
        public float GetBiomassData(int i, int j)
        {
            if (i > biomassNum || j < 1 || j > 2)
                throw new Exception("index error at GetBiomass");

            return biomassData[(i - 1) * 2 + j - 1];
        }

        public void SetBiomassData(int i, int j, float value)
        {
            if (i > biomassNum || j < 1 || j > 2)
                throw new Exception("index error at SetBiomass");

            biomassData[(i - 1) * 2 + j - 1] = value;
        }


        public void SetBiomassNum(int num)
        {
            biomassNum = num;
            biomassData = null;
            biomassData = new float[num * 2];
        }

        public float BiomassThreshold
        {
            get { return biomassThreshold; }
            set { biomassThreshold = value; }
        }

    }
*/}
