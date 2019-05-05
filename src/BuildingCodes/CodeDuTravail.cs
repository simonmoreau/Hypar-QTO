using Elements;
using Elements.Geometry;
using System;
using System.Linq;
using System.Collections.Generic;

namespace Bim42HyparQto.BuildingCode
{
    /// <summary>
    /// A helper class to manage building codes
    /// </summary>
  	public static class CodeDuTravail
    {
        /// <summary>
        /// Article R4216-5 Get the width of the evacuation
        /// </summary>
        public static double ConvertUpToMeter(int nombreUp)
        {
            double meter = 0.6 * nombreUp;
            if (nombreUp == 1) { meter = 0.9; }
            if (nombreUp == 2) { meter = 1.4; }
            return meter;
        }

        /// <summary>
        /// Article R4216-8 Get the number of UP for a givn number of occupants
        /// </summary>
        public static int GetUPNumber(int effectif)
        {
            if (effectif < 20) { return 1; }
            else if (20 <= effectif && effectif <= 50) { return 2; } //it is actually more complex than that, I am being consevative here
            else if (50 < effectif && effectif <= 100) { return 2; }
            else if (100 < effectif && effectif <= 200) { return 3; }
            else if (200 < effectif && effectif <= 300) { return 4; }
            else if (300 < effectif && effectif <= 400) { return 5; }
            else if (400 < effectif && effectif <= 500) { return 6; }
            else if (effectif > 500)
            {
                return (int)Math.Ceiling((double)effectif / 100);
            }
            else
            {
                return 0;
            }
        }

                /// <summary>
        /// Article R4216-8 Get the number of evacuation paths for a given number of occupants
        /// </summary>
        public static int GetDegagementNumber(int effectif)
        {
            if (effectif < 20) { return 1; }
            else if (20 <= effectif && effectif <= 50) { return 2; } //it is actually more complex than that, I am being consevative here
            else if (50 < effectif && effectif <= 100) { return 2; }
            else if (100 < effectif && effectif <= 200) { return 2; }
            else if (200 < effectif && effectif <= 300) { return 2; }
            else if (300 < effectif && effectif <= 400) { return 2; }
            else if (400 < effectif && effectif <= 500) { return 2; }
            else if (effectif > 500)
            {
                return 2 + (int)Math.Ceiling((double)(effectif-500) / 500);
            }
            else{
                return 0;
            }
        }
    }
}