//  Author: Robert Scheller, Melissa Lucash

using Landis.Core;
using Landis.Library.Climate;
using System.Linq;
using Landis.Library.DensityCohorts;
using System;

namespace Landis.Extension.Succession.Density
{
    public class ClimateRegionData
    {
        public static Library.Parameters.Ecoregions.AuxParm<AnnualClimate> AnnualWeather;
        public static int MinSpinUpClimateYear { get; private set; }
        public static int MaxSpinUpClimateYear { get; private set; }
        public static int MinFutureClimateYear { get; private set; }
        public static int MaxFutureClimateYear { get; private set; }
        public static int MaxSpinUpIndex { get; private set; }
        public static int MaxFutureClimateIndex { get; private set; }

        //---------------------------------------------------------------------
        //public static void Initialize(IInputParameters parameters)
        public static void Initialize()
        {
            AnnualWeather = new Library.Parameters.Ecoregions.AuxParm<AnnualClimate>(PlugIn.ModelCore.Ecoregions);

            /*
            foreach (IEcoregion ecoregion in Globals.ModelCore.Ecoregions)
            {
                if (ecoregion.Active)
                {
                    // Latitude is contained in the PnET Ecoregion
                    Climate.Climate.GenerateEcoregionClimateData(ecoregion, 0, EcoregionData.GetPnETEcoregion(ecoregion).Latitude);
                    SetSingleAnnualClimate(ecoregion, 0, Climate.Climate.Phase.SpinUp_Climate);  // Some placeholder data to get things started.
                }
            }
            */

            Climate.GenerateEcoregionClimateData(((Parameter<float>)PlugIn.GetParameter(Names.Latitude)).Value);

            // grab the first year's spinup climate
            foreach (var ecoregion in PlugIn.ModelCore.Ecoregions.Where(x => x.Active))
            {
                AnnualWeather[ecoregion] = Climate.SpinupEcoregionYearClimate[ecoregion.Index][1];      // Climate data year index is 1-based
            }
            SetMinMaxClimateYears();
        }

        public static void SetAllEcoregions_FutureAnnualClimate(int year)
        {

            if (PlugIn.TryGetParameter(Names.ClimateConfigFile, out var climateLibraryFileName))
            {
                // grab the year's future climate
                foreach (var ecoregion in PlugIn.ModelCore.Ecoregions.Where(x => x.Active))
                {
                    AnnualWeather[ecoregion] = Climate.FutureEcoregionYearClimate[ecoregion.Index][year];      // Climate data year index is 1-based
                }
            }
            //int actualYear = Climate.Future_MonthlyData.Keys.Min() + year - 1;
            //foreach (IEcoregion ecoregion in PlugIn.ModelCore.Ecoregions)
            //{
            //    if (ecoregion.Active)
            //    {
            //        //PlugIn.ModelCore.UI.WriteLine("Retrieving {0} for year {1}.", spinupOrfuture.ToString(), actualYear);
            //        if (Climate.Future_MonthlyData.ContainsKey(actualYear))
            //        {
            //            AnnualWeather[ecoregion] = Climate.Future_MonthlyData[actualYear][ecoregion.Index];
            //        }

            //        //PlugIn.ModelCore.UI.WriteLine("Utilizing Climate Data: Simulated Year = {0}, actualClimateYearUsed = {1}.", actualYear, AnnualWeather[ecoregion].Year);
            //    }

            //}
        }

        public static void SetMinMaxClimateYears()
        {
            MinSpinUpClimateYear = Climate.SpinupCalendarYear(1);
            MaxSpinUpClimateYear = Climate.SpinupEcoregionYearClimate.First(x => x != null).Last(x => x != null).CalendarYear;

            MinFutureClimateYear = Climate.FutureCalendarYear(1);
            MaxSpinUpClimateYear = Climate.FutureEcoregionYearClimate.First(x => x != null).Last(x => x != null).CalendarYear;
        }

        public static bool IsFutureClimate(DateTime date)
        {
            if (date.Year - MinFutureClimateYear + 1 <= 0)
            {
                return false;
            }
            return true;
        }

        public static int ConvertYearToFutureClimateYear(DateTime date)
        {
            int convert = date.Year - MinFutureClimateYear + 1;

            return convert >= 1 ? convert : -1;
        }

        public static int ConvertYearToSpinUpClimateYear(DateTime date)
        {
            int convert = date.Year - MinSpinUpClimateYear + 1;

            return convert >= 1 ? convert : -1;
        }
    }
}
