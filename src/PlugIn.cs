<<<<<<< HEAD
﻿
#define LANDISPRO_ONLY_SUCCESSION


using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using System.IO;
//using OSGeo.GDAL;
using OSGeo.OSR;
using Landis.Core;
using Landis.SpatialModeling;


namespace Landis.Extension.Succession.Density
{
    public class PlugIn : SuccessionMain
    {
        public static readonly string ExtensionName = "Density Succession";


        private static ICore modelCore;

        //public static int gl_currentDecade;
        //private static UInt16 currentHarvestEventId = 0;
        //private static map16 gl_visitMap = new map16();
        private static string[] ageMaps = new string[defines.MAX_RECLASS];

        
        private string fpLogFileSEC_name = null;

        private static List<string> SEC_landtypefiles = new List<string>();
        private static List<string> SEC_gisfiles = new List<string>();

        //change by YYF 2018/11
        //public static int[] freq = new int[6];
        public static int[] freq = new int[6] { 1, 1, 1, 1, 1, 1 };
        public static uint numSpecies;
        public static uint snr, snc;
        public static pdp pPDP = new pdp();
        public static string fpforTimeBU_name = null;
        public static double[] wAdfGeoTransform = new double[6];

        //change by YYF 2019/4
        public static int envOn;

        private static uint specAtNum;

        private static int numbOfIter;

        public static InputParameters   gl_param     = new InputParameters();
        public static speciesattrs gl_spe_Attrs = new speciesattrs(defines.MAX_SPECIES);
        public static landunits    gl_landUnits = new landunits(defines.MAX_LANDUNITS);
        public static sites            gl_sites = new sites();
        //=========================================================================

        public static uint NumSpecies { set { numSpecies = value; } }
        public PlugIn() : base(ExtensionName) { }
        //public PlugIn(string name, ExtensionType ty) : base(name, ty) { }



        //this destructor is used because in the original landispro program, there are still some operations 
        //after the main simulation loops
        ~PlugIn()
        {
            In_Output.AgeDistOutputFromBufferToFile();
            
            Console.WriteLine("Ending Landispro succession.");
        }

        public static ICore ModelCore
        {
            get { return modelCore; }
            private set { modelCore = value; }
        }

        public Land_type_Attributes gl_land_attrs = null;

        public override void LoadParameters(string dataFile, ICore mCore)
        {
            modelCore = mCore;
            //Console.WriteLine("Run: From {0} to {1}, current {2}", modelCore.StartTime, modelCore.EndTime, modelCore.CurrentTime);
            //Console.WriteLine("\n\nhere datefile = {0}\n\n", dataFile);
           
            InputParametersParser parser = new InputParametersParser();
            gl_param = Landis.Data.Load<InputParameters>(dataFile, parser);
            //Init_Output.GDLLMode = gl_param.read(dataFile);
            //PlugIn.gl_spe_Attrs.read(saFile);
            PlugIn.gl_spe_Attrs.read(PlugIn.gl_param.ExtraSpecAtrFile);

            //PlugIn.gl_landUnits.attach(PlugIn.gl_spe_Attrs);

            //PlugIn.gl_landUnits.read(PlugIn.gl_param.LandUnitFile);

            //PlugIn.gl_sites.BiomassRead(PlugIn.gl_param.Biomassfile);

            BiomassParamParser bioparser = new BiomassParamParser();
            Landis.Data.Load<BiomassParam>(PlugIn.gl_param.Biomassfile, bioparser);

            numSpecies = gl_spe_Attrs.NumAttrs;


            species.attach(PlugIn.gl_spe_Attrs);
            gl_landUnits.attach(PlugIn.gl_spe_Attrs);
            Land_type_AttributesParser parser2 = new Land_type_AttributesParser();
            gl_land_attrs = Landis.Data.Load<Land_type_Attributes>(gl_param.LandUnitFile, parser2);
            gl_landUnits.Copy(gl_land_attrs);
            Establishment_probability_AttributesParser parser3 = new Establishment_probability_AttributesParser();
            //var establish =
            Landis.Data.Load<Establishment_probability_Attributes>(gl_param.VarianceSECFile, parser3);

        }


        public override void Initialize()
        {
            DateTime now = DateTime.Now;
            //Gdal.AllRegister();
            Console.WriteLine("Start Landis Pro at {0}", now);

            Timestep = gl_param.SuccessionTimestep;
            gl_sites.SuccessionTimeStep = gl_param.SuccessionTimestep;
            In_Output.Init_IO();


            envOn = 0;
            int reclYear = 0;


            Console.Write("Beginning Landis 7.0 Pro Run\n Initializing...\n");

            if (envOn > 0)
                Console.Write("Environment will be updated every {0} iterarion\n", envOn);
            
            Density.Timestep.timestep = (uint)gl_param.SuccessionTimestep;

            numbOfIter = gl_param.Num_Iteration;

            //gl_sites.Stocking_x_value = gl_param.Stocking_x_value;
            //gl_sites.Stocking_y_value = gl_param.Stocking_y_value;
            //gl_sites.Stocking_z_value = gl_param.Stocking_z_value;
            
            gl_sites.CellSize = gl_param.CellSize;

#if !LANDISPRO_ONLY_SUCCESSION
                gl_sites.TimeStep_BDA     = gl_param.Timestep_BDA;
                gl_sites.TimeStep_Fire    = gl_param.Timestep_Fire;
                gl_sites.TimeStep_Fuel    = gl_param.Timestep_Fuel;
                gl_sites.TimeStep_Harvest = gl_param.Timestep_Harvest;  //Harvest Module
                gl_sites.TimeStep_Wind    = gl_param.Timestep_Wind;
#endif


            for (int x = 0; x < 5; x++)
                freq[x] = 1;

#if !LANDISPRO_ONLY_SUCCESSION
            if ((Init_Output.GDLLMode & defines.G_HARVEST) != 0)    //Harvest Module
                freq[5] = 1;

            if ((Init_Output.GDLLMode & defines.G_BDA) != 0)
                Console.Write("BDA ");

            if ((Init_Output.GDLLMode & defines.G_WIND) != 0)
                Console.Write("Wind ");

            if ((Init_Output.GDLLMode & defines.G_HARVEST) != 0)
                Console.Write("Harvest ");

            if ((Init_Output.GDLLMode & defines.G_FUEL) != 0)
                Console.Write("Fuel ");

            if ((Init_Output.GDLLMode & defines.G_FUELMANAGEMENT) != 0)
                Console.Write("Fuel management ");

            if ((Init_Output.GDLLMode & defines.G_FIRE) != 0)
                Console.Write("Fire ");

            if (Init_Output.GDLLMode != 0)
                Console.Write("are(is) on\n");
#endif

            //In_Output.getInput(freq, ageMaps, pPDP, BDANo, wAdfGeoTransform);
            SiteVars.Initialize();
            In_Output.getInput(freq, pPDP);

#if !LANDISPRO_ONLY_SUCCESSION
            if ((gDLLMode & DEFINES.G_HARVEST) != 0)
            {
                Console.WriteLine("Harvest Dll loaded in...");
                GlobalFunctions.HarvestPass(sites, speciesAttrs);
                sites.Harvest70outputdim();
            }
#endif

            Console.Write("Finish getting input\n");



            In_Output.OutputScenario();
            
            In_Output.initiateOutput_landis70Pro();



            snr = gl_sites.numRows;
            snc = gl_sites.numColumns;

            specAtNum = gl_spe_Attrs.NumAttrs;


            gl_sites.GetSeedDispersalProbability(null, gl_param.SeedRainFlag);

            gl_sites.GetSpeciesGrowthRates(gl_param.GrowthFlagFile, gl_param.GrowthFlag);

            gl_sites.GetSpeciesMortalityRates(gl_param.MortalityFile, gl_param.MortalityFlag);

            gl_sites.GetVolumeRead(gl_param.VolumeFile, gl_param.VolumeFlag);

            initiateRDofSite_Landis70();


            if (reclYear != 0)
            {
                int local_num = reclYear / gl_sites.SuccessionTimeStep;

                //Jacob reclass3.reclassify(reclYear, ageMaps);

                //Jacob In_Output.putOutput(local_num, local_num, freq);

                In_Output.putOutput_Landis70Pro(local_num, local_num, freq);

                In_Output.putOutput_AgeDistStat(local_num);
                
                Console.Write("Ending Landispro Succession.\n");
            }
            else
            {
                //Jacob In_Output.putOutput(0, 0, freq);

                In_Output.putOutput_Landis70Pro(0, 0, freq);

                In_Output.putOutput_AgeDistStat(0);
            }


            if (gl_param.RandSeed == 0)  //random
            {
                DateTime startTime = new DateTime(1970, 1, 1);
                gl_param.RandSeed = (int)Convert.ToUInt32(Math.Abs((DateTime.Now - startTime).TotalSeconds));
            }

            system1.fseed(gl_param.RandSeed);


            Console.WriteLine("gl_param.RandSeed = {0}", gl_param.RandSeed);


            if (envOn > gl_param.Num_Iteration)
                throw new Exception("Invalid year of interpretation for updating environment");

            fpforTimeBU_name  = gl_param.OutputDir + "/Running_Time_Stat.txt";
            fpLogFileSEC_name = gl_param.OutputDir + "/SECLog.txt";

            var now2 = DateTime.Now;

            Console.WriteLine("\nFinish the initilization at {0}", now2);

            var ltimeDiff = now2 - now;

            Console.Write("it took {0} seconds\n", ltimeDiff);
            gl_landUnits.initiateVariableVector(gl_param.Num_Iteration, gl_param.SuccessionTimestep, specAtNum, gl_param.FlagforSECFile);
            using (StreamWriter fpforTimeBU = new StreamWriter(fpforTimeBU_name))
            {
                fpforTimeBU.Write("Initilization took: {0} seconds\n", ltimeDiff);
            }
        }









        public static void succession_Landis70(pdp ppdp,int itr)
        {
            gl_sites.GetMatureTree();

            //Jacob ----- Test
            foreach (Site site in modelCore.Landscape.ActiveSites)
            {
                uint tempRow = (uint)gl_sites.convertLP_Row(site.Location.Row);
                uint tempCol = (uint)site.Location.Column;

                string tempName = site.Location.ToString();
                int mapCD = PlugIn.ModelCore.Ecoregion[site].MapCode;
                string erName = PlugIn.ModelCore.Ecoregion[site].Name;

                float local_RD = gl_sites[tempRow, tempCol].RD;

                landunit l = PlugIn.gl_landUnits[PlugIn.ModelCore.Ecoregion[PlugIn.ModelCore.Landscape.GetSite(site.Location.Row, site.Location.Column)].Name];

                site local_site = gl_sites[tempRow, tempCol];

                for (int k = 1; k <= specAtNum; ++k)
                {
                    local_site.SpecieIndex(k).GrowTree();
                }

            }


/*          Jacob ----- Old site iterator      
 *                //increase ages
                for (uint i = 1; i <= snr; ++i)
            {
                for (uint j = 1; j <= snc; ++j)
                {
                    ppdp.addedto_sTSLMortality(i, j, (short)gl_sites.SuccessionTimeStep);

                    //define land unit
                    landunit l = gl_sites.locateLanduPt(i, j);
                    //if (l != null && l.active())
                    if (l != null)
                    {
                        site local_site = gl_sites[i, j];

                        for (int k = 1; k <= specAtNum; ++k)
                        {
                            local_site.SpecieIndex(k).GrowTree();
                        }
                        
                    }

                }
                //Console.ReadLine();
            }*/

            //seed dispersal
            initiateRDofSite_Landis70();
            Console.WriteLine("Seed Dispersal:");

            //Jacob ----- Test
            foreach (Site site in modelCore.Landscape.ActiveSites)
            {
                uint tempRow = (uint)gl_sites.convertLP_Row(site.Location.Row);
                uint tempCol = (uint)site.Location.Column;

                string tempName = site.Location.ToString();
                int mapCD = PlugIn.ModelCore.Ecoregion[site].MapCode;
                string erName = PlugIn.ModelCore.Ecoregion[site].Name;

                float local_RD = gl_sites[tempRow, tempCol].RD;

                landunit l = PlugIn.gl_landUnits[PlugIn.ModelCore.Ecoregion[PlugIn.ModelCore.Landscape.GetSite(site.Location.Row, site.Location.Column)].Name];

                if (local_RD < l.MaxRDArray(0))

                    gl_sites.SiteDynamics(0, tempRow, tempCol);

                else if (local_RD >= l.MaxRDArray(0) && local_RD < l.MaxRDArray(1))

                    gl_sites.SiteDynamics(1, tempRow, tempCol);

                else if (local_RD >= l.MaxRDArray(1) && local_RD <= l.MaxRDArray(2))

                    gl_sites.SiteDynamics(2, tempRow, tempCol);

                else if (local_RD > l.MaxRDArray(2) && local_RD <= l.MaxRDArray(3))

                    gl_sites.SiteDynamics(3, tempRow, tempCol);

                else
                {
                    Debug.Assert(local_RD > l.MaxRDArray(3));
                    gl_sites.SiteDynamics(4, tempRow, tempCol);
                }
            }

/*          Jacob ----- Old site iterator  
             *            for (uint i = 1; i <= snr; ++i)
                        {
                            //Console.WriteLine("\n{0}%\n", 100 * i / snr);

                            for (uint j = 1; j <= snc; ++j)
                            {
                                //Console.WriteLine("i = {0}, j = {1}", i, j);
                                landunit l = gl_sites.locateLanduPt(i, j);
                                KillTrees(i, j);
                                if (l != null && l.active())
                                //if (l != null)
                                {
                                    float local_RD = gl_sites[i, j].RD;

                                    if (local_RD < l.MaxRDArray(0))

                                        gl_sites.SiteDynamics(0, i, j);

                                    else if (local_RD >= l.MaxRDArray(0) && local_RD < l.MaxRDArray(1))

                                        gl_sites.SiteDynamics(1, i, j);

                                    else if (local_RD >= l.MaxRDArray(1) && local_RD <= l.MaxRDArray(2))

                                        gl_sites.SiteDynamics(2, i, j);

                                    else if (local_RD > l.MaxRDArray(2) && local_RD <= l.MaxRDArray(3))

                                        gl_sites.SiteDynamics(3, i, j);

                                    else
                                    {
                                        Debug.Assert(local_RD > l.MaxRDArray(3));
                                        gl_sites.SiteDynamics(4, i, j);
                                    }
                                }
                            }

                        }*/
            
            Console.WriteLine("End density succession");
        }





        //initiating Landis70 RD values
        public static void initiateRDofSite_Landis70()
        {
            for (uint i = 1; i <= snr; ++i)
                for (uint j = 1; j <= snc; ++j)
                    gl_sites.GetRDofSite(i, j);
        }




        //start killing trees gradually at the 80 % longevity until they reach their longevity
        // modified version of function : void SUCCESSION::kill(SPECIE *s, SPECIESATTR *sa) 
        public static void KillTrees(uint local_r, uint local_c)
        {
            site local_site = gl_sites[local_r, local_c];

            for (int k = 1; k <= specAtNum; ++k)//sites.specNum
            {
                int longev = gl_spe_Attrs[k].Longevity;

                int numYears = longev / 5;

                float chanceMod = 0.8f / (numYears + 0.00000001f);

                float chanceDeath = 0.2f;

                int m_beg = (longev - numYears) / gl_sites.SuccessionTimeStep;
                int m_end = longev / gl_sites.SuccessionTimeStep;

                specie local_specie = local_site.SpecieIndex(k);

                for (int m = m_beg; m <= m_end; m++)
                {
                    int tmpTreeNum = (int)local_specie.getTreeNum(m, k);

                    int tmpMortality = 0;

                    if (tmpTreeNum > 0)
                    {
                        float local_threshold = chanceDeath * gl_sites.SuccessionTimeStep / 10;

                        for (int x = 1; x <= tmpTreeNum; x++)
                        {
                            if (system1.frand() < local_threshold)
                                tmpMortality++;
                        }
                        local_specie.setTreeNum(m, k, Math.Max(0, tmpTreeNum - tmpMortality));
                    }

                    chanceDeath += chanceMod;

                }
            }
        }

        /////////////////////////////////////////////////////////////////////////////

        //                      SINGULAR LANDIS ITERATION ROUTINE                  //

        /////////////////////////////////////////////////////////////////////////////



        //This processes a singular Landis iteration.  It loops through each site
        //followed by each species.  For every iteration of the loop grow and kill
        //are called.  Then seed availability is checked.  If seed is available
        //and shade conditions are correct birth is called.
        public void singularLandisIteration(int itr, pdp ppdp)
        {
            DateTime ltime, ltimeTemp;
            TimeSpan ltimeDiff;


            using (StreamWriter fpforTimeBU = File.AppendText(fpforTimeBU_name))
            {
                fpforTimeBU.WriteLine("\nProcessing succession at Year: {0}:", itr);


                if (itr % gl_sites.SuccessionTimeStep == 0)
                {
                    ltime = DateTime.Now;

                    Console.WriteLine("Start succession ... at {0}", ltime);

                    system1.fseed(gl_param.RandSeed + itr / gl_sites.SuccessionTimeStep * 6);

                    gl_landUnits.ReprodUpdate(itr / gl_sites.SuccessionTimeStep);
                    //Console.WriteLine("random number: {0}", system1.frand());
                    succession_Landis70(ppdp,itr);
                    //Console.WriteLine("random number: {0}", system1.frand());
                    ltimeTemp = DateTime.Now;

                    ltimeDiff = ltimeTemp - ltime;

                    Console.WriteLine("Finish succession at {0} sit took {1} seconds", DateTime.Now, ltimeDiff);

                    fpforTimeBU.WriteLine("Processing succession: {0} seconds", ltimeDiff);

                    fpforTimeBU.Flush();
                }
            }
            
            system1.fseed(gl_param.RandSeed);
        }



        /*
        public static void updateLandtypeMap8(BinaryReader ltMapFile)
        {
            uint[] dest = new uint[32];

            //LDfread((char*)dest, 4, 32, ltMapFile);
            for (int i = 0; i < 32; i++)
                dest[i] = ltMapFile.ReadUInt32();


            int b16or8;
            if ((dest[1] & 0xff0000) == 0x020000)
                b16or8 = 16;
            else if ((dest[1] & 0xff0000) == 0)
                b16or8 = 8;
            else
            {
                b16or8 = -1;
                throw new Exception("Error: IO: Landtype map is neither 16 bit or 8 bit.");
            }

            uint nCols = dest[4];
            uint nRows = dest[5];


            uint xDim = gl_sites.numColumns;
            uint yDim = gl_sites.numRows;

            if ((nCols != xDim) || (nRows != yDim))
                throw new Exception("landtype map and species map do not match.");



            if (b16or8 == 8)  //8 bit
            {
                for (uint i = yDim; i > 0; i--)
                {
                    for (uint j = 1; j <= xDim; j++)
                    {
                        int coverType = ltMapFile.Read();

                        if (coverType >= 0)
                            gl_sites.fillinLanduPt(i, j, gl_landUnits[coverType]);
                        else
                            throw new Exception("illegel landtype class found.");
                    }
                }
            }
            else if (b16or8 == 16)  //16 bit
            {
                for (uint i = yDim; i > 0; i--)
                {
                    for (uint j = 1; j <= xDim; j++)
                    {
                        int coverType = ltMapFile.ReadUInt16();

                        if (coverType >= 0)
                            gl_sites.fillinLanduPt(i, j, gl_landUnits[coverType]);
                        else
                            throw new Exception("illegel landtype class found.");
                    }
                }
            }

        }
        */






        //Main program.  This contains start and shut down procedures as well as the main iteration loop.
        public override void Run()
        {
            int i = modelCore.TimeSinceStart;

            int i_d_timestep = i / gl_sites.SuccessionTimeStep;
            for (int r = 0; r < gl_landUnits.Num_Landunits; ++r)
            {
                gl_landUnits[r].MinShade = Land_type_Attributes.get_min_shade(r);
                float[] rd = new float[4];
                rd[0] = Land_type_Attributes.get_gso(0, r);
                rd[1] = Land_type_Attributes.get_gso(1, r);
                rd[2] = Land_type_Attributes.get_gso(2, r);
                rd[3] = gl_land_attrs.get_maxgso(i, r);
                gl_landUnits[r].MaxRDArrayItem = rd;
                gl_landUnits[r].MaxRD = rd[3];
            }

            //Simulation loops////////////////////////////////////////////////

            if (i % gl_sites.SuccessionTimeStep == 0)
            {
                if (gl_param.FlagforSECFile == 3)
                {
                    int index = i_d_timestep - 1;

                    if (index == 0)
                    {
                        SEC_landtypefiles.Clear();
                        SEC_gisfiles.Clear();

                        if (index < gl_land_attrs.year_arr.Count)
                        {
                            Console.Write("\nEnvironment parameter Updated.\n");
                            string SECfileMapGIS = gl_land_attrs.Get_new_landtype_map(index);

                            gl_param.LandImgMapFile = SECfileMapGIS;

                            Console.WriteLine("\nEnvironment map Updated.");

                            landunit SECLog_use = gl_landUnits.first();

                            int ii_count = 0;


                            using (StreamWriter fpLogFileSEC = new StreamWriter(fpLogFileSEC_name))
                            {
                                fpLogFileSEC.Write("Year: {0}\n", i);

                                for (; ii_count < gl_landUnits.Num_Landunits; ii_count++)
                                {
                                    fpLogFileSEC.Write("Landtype{0}:\n", ii_count);


                                    for (int jj_count = 1; jj_count <= specAtNum; jj_count++)
                                    {
                                        fpLogFileSEC.Write("spec{0}: {1:N6}, ", jj_count, SECLog_use.probRepro(jj_count));
                                    }

                                    SECLog_use = gl_landUnits.next();

                                    fpLogFileSEC.Write("\n");
                                }
                            }
                        }
                    }


                    if (index > 0)
                    {
                        if (index < SEC_landtypefiles.Count)
                        {
                            gl_param.LandImgMapFile = gl_land_attrs.Get_new_landtype_map(index);
                            Console.WriteLine("\nEnvironment parameter Updated.");

                            Console.WriteLine("\nEnvironment map Updated.");

                            landunit SECLog_use = gl_landUnits.first();

                            using (StreamWriter fpLogFileSEC = new StreamWriter(fpLogFileSEC_name))
                            {
                                fpLogFileSEC.Write("Year: {0}\n", i);

                                int ii_count = 0;

                                for (; ii_count < gl_landUnits.Num_Landunits; ii_count++)
                                {
                                    fpLogFileSEC.Write("Landtype{0}:\n", ii_count);

                                    for (int jj_count = 1; jj_count <= specAtNum; jj_count++)
                                        fpLogFileSEC.Write("spec{0}: {1:N6}, ", jj_count, SECLog_use.probRepro(jj_count));

                                    SECLog_use = gl_landUnits.next();

                                    fpLogFileSEC.Write("\n");
                                }
                            }

                        }

                    }
                }

            }//end if

            Console.WriteLine("Processing succession at Year {0}", i);


            singularLandisIteration(i, pPDP);


            if (i % gl_sites.SuccessionTimeStep == 0 || i == numbOfIter * gl_sites.SuccessionTimeStep)
            {
                int[] frequency = new int[6] { 1, 1, 1, 1, 1, 1 };
                    
                if (i % (gl_sites.SuccessionTimeStep * freq[0]) == 0 && i_d_timestep <= numbOfIter)
                {
                    In_Output.putOutput_Landis70Pro(0, i_d_timestep, freq);
                }
                    
                if (i == gl_sites.SuccessionTimeStep * numbOfIter)
                {
                    In_Output.putOutput_Landis70Pro(0, numbOfIter, frequency);
                }

                In_Output.putOutput_AgeDistStat(i_d_timestep);

            }


            //}

            //Simulation loops end/////////////////////////////////////////////////


        }//end Run()

        public override void InitializeSites(string initialCommunities, string initialCommunitiesMap, ICore modelCore)
        {
            throw new NotImplementedException();
        }
    }


}
=======
//  Authors:    Arjan de Bruijn
//              Brian R. Miranda

// John McNabb: (02.04.2019)
//
//  Summary of changes to allow the climate library to be used with PnET-Succession:
//   (1) Added ClimateRegionData class based on that of NECN to hold the climate library data. This is Initialized by a call
//       to InitialClimateLibrary() in Plugin.Initialize().
//   (2) Modified EcoregionPnET to add GetClimateRegionData() which grabs climate data from ClimateRegionData.  This uses an intermediate
//       MonthlyClimateRecord instance which is similar to ObservedClimate.
//   (3) Added ClimateRegionPnETVariables class which is a copy of the EcoregionPnETVariables class which uses MonthlyClimateRecord rather than
//       ObserverdClimate. I had hoped to use the same class, but the definition of IObservedClimate prevents MonthlyClimateRecord from implementing it.
//       IMPORTANT NOTE: The climate library precipation is in cm/month, so that it is converted to mm/month in MonthlyClimateRecord.
//   (4) Modified Plugin.AgeCohorts() and SiteCohorts.SiteCohorts() to call either EcoregionPnET.GetClimateRegoinData() or EcoregionPnET.GetData()
//       depending on whether the climate library is enabled.

//   Enabling the climate library with PnET:
//   (1) Indicate the climate library configuration file in the 'PnET-succession' configuration file using the 'ClimateConfigFile' parameter, e.g.
//        ClimateConfigFile	"./climate-generator-baseline.txt"
//
//   NOTE: Use of the climate library is OPTIONAL.  If the 'ClimateConfigFile' parameter is missing (or commented-out) of the 'PnET-succession'
//   configuration file, then PnET reverts to using climate data as specified by the 'ClimateFileName' column in the 'EcoregionParameters' file
//   given in the 'PnET-succession' configuration file.
//
//   NOTE: This uses a version (v4?) of the climate library that exposes AnnualClimate_Monthly.MonthlyOzone[] and .MonthlyCO2[].

using Landis.Core;
using Landis.Library.DensityCohorts.InitialCommunities;
using Landis.Library.Succession;
using Landis.SpatialModeling;
using Landis.Library.Climate;
using System;
using System.Collections.Generic;
using System.Linq;
using Landis.Library.DensityCohorts;
using Landis.Library.Metadata;

namespace Landis.Extension.Succession.Density
{
    public class PlugIn  : Landis.Library.Succession.ExtensionBase 
    {
        //================================== Density variables ================================
        //public static ISiteVar<float> SiteRD;
        public static SpeciesDensity SpeciesDensity;
        //=====================================================================================
        public static DateTime Date;
        public static ICore ModelCore;
        private static ISiteVar<SiteCohorts> sitecohorts;
        private static DateTime StartDate;
        private static Dictionary<ActiveSite, string> SiteOutputNames;
        //public static ushort IMAX;
        //public static float FTimeStep;

        public static biomassUtil biomass_util = new biomassUtil();
        public static bool UsingClimateLibrary;
        private ICommunity initialCommunity;
        //public static int CohortBinSize;

        private static SortedDictionary<string, Parameter<string>> parameters = new SortedDictionary<string, Parameter<string>>(StringComparer.InvariantCultureIgnoreCase);
        MyClock m = null;

        public static bool TryGetParameter(string label, out Parameter<string> parameter)
        {
            parameter = null;
            if (label == null)
            {
                return false;
            }

            if (parameters.ContainsKey(label) == false) return false;

            else
            {
               parameter = parameters[label];
               return true;
            }
        }

        public static Parameter<string> GetParameter(string label)
        {
            if (parameters.ContainsKey(label) == false)
            {
                throw new System.Exception("No value provided for parameter " + label);
            }

            return parameters[label];

        }
        public static Parameter<string> GetParameter(string label, float min, float max)
        {
            if (parameters.ContainsKey(label) == false)
            {
                throw new System.Exception("No value provided for parameter " + label);
            }

            Parameter<string> p = parameters[label];

            foreach (KeyValuePair<string, string> value in p)
            {
                float f;
                if (float.TryParse(value.Value, out f) == false)
                {
                    throw new System.Exception("Unable to parse value " + value.Value + " for parameter " + label +" unexpected format.");
                }
                if (f > max || f < min)
                {
                    throw new System.Exception("Parameter value " + value.Value + " for parameter " + label + " is out of range. [" + min + "," + max + "]");
                }
            }
            return p;
            
        }
      
        /// <summary>
        /// Choose random integer between min and max (inclusive)
        /// </summary>
        /// <param name="min">Minimum integer</param>
        /// <param name="max">Maximum integer</param>
        /// <returns></returns>
        public static int DiscreteUniformRandom(int min, int max)
        {
            ModelCore.ContinuousUniformDistribution.Alpha = min;
            ModelCore.ContinuousUniformDistribution.Beta = max + 1;
            ModelCore.ContinuousUniformDistribution.NextDouble();

            //double testMin = ModelCore.ContinuousUniformDistribution.Minimum;
            //double testMax = ModelCore.ContinuousUniformDistribution.Maximum;
            
            double valueD = ModelCore.ContinuousUniformDistribution.NextDouble();
            int value = Math.Min((int)valueD,max);

            return value;
        }

        public static double ContinuousUniformRandom(double min = 0, double max = 1)
        {
            ModelCore.ContinuousUniformDistribution.Alpha = min;
            ModelCore.ContinuousUniformDistribution.Beta = max;
            ModelCore.ContinuousUniformDistribution.NextDouble();

            double value = ModelCore.ContinuousUniformDistribution.NextDouble();

            return value;
        }

        public void DeathEvent(object sender, Landis.Library.DensityCohorts.DeathEventArgs eventArgs)
        {
            ExtensionType disturbanceType = eventArgs.DisturbanceType;
            if (disturbanceType != null)
            {
                ActiveSite site = eventArgs.Site;

                 
                if (disturbanceType.IsMemberOf("disturbance:fire"))
                    Reproduction.CheckForPostFireRegen(eventArgs.Cohort, site);
                else
                    Reproduction.CheckForResprouting(eventArgs.Cohort, site);
            }
        }
       
        /*string PnETDefaultsFolder
        {
            get
            {
                return System.IO.Path.Combine(System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location), "Defaults");
            }
        }*/
        
        public PlugIn()
            : base(Names.ExtensionName)
        {
            //LocalOutput.PNEToutputsites = Names.PNEToutputsites;
        }

        public static Dictionary<string, Parameter<string>> LoadTable(string label, List<string> RowLabels, List<string> Columnheaders, bool transposed = false)
        {
            string filename = GetParameter(label).Value;
            if (System.IO.File.Exists(filename) == false) throw new System.Exception("File not found " + filename);
            ParameterTableParser parser = new ParameterTableParser(filename, label, RowLabels, Columnheaders, transposed);
            Dictionary<string, Parameter<string>> parameters = Landis.Data.Load<Dictionary<string, Parameter<string>>>(filename, parser);
            return parameters;
        }
       
        public override void LoadParameters(string InputParameterFile, ICore mCore)
        {
            ModelCore = mCore;
            EcoregionData.InitializeCore(mCore);
            parameters.Add(Names.ExtensionName, new Parameter<string>(Names.ExtensionName, InputParameterFile));

            //-------------PnET-Succession input files
            Dictionary<string, Parameter<string>> InputParameters = LoadTable(Names.ExtensionName, Names.AllNames, null, true);
            InputParameters.ToList().ForEach(x => parameters.Add(x.Key, x.Value));

            //-------------Read Species parameters input file
            List<string> SpeciesNames = PlugIn.ModelCore.Species.ToList().Select(x => x.Name).ToList();
            List<string> SpeciesPars = SpeciesDensity.ParameterNames;
            SpeciesPars.Add(Names.DensitySpeciesParameters);
            Dictionary<string, Parameter<string>> speciesparameters = LoadTable(Names.DensitySpeciesParameters, SpeciesNames, SpeciesPars);
            foreach (string key in speciesparameters.Keys)
            {
                if (parameters.ContainsKey(key)) throw new System.Exception("Parameter " + key + " was provided twice");
            }
            speciesparameters.ToList().ForEach(x => parameters.Add(x.Key, x.Value));

            //-------------Ecoregion parameters
            List<string> EcoregionNames = PlugIn.ModelCore.Ecoregions.ToList().Select(x => x.Name).ToList();
            List<string> EcoregionParameters = EcoregionData.ParameterNames;
            Dictionary<string, Parameter<string>> ecoregionparameters = LoadTable(Names.EcoregionParameters, EcoregionNames, EcoregionParameters);
            foreach (string key in ecoregionparameters.Keys)
            {
                if (parameters.ContainsKey(key)) throw new System.Exception("Parameter "+ key +" was provided twice");
            }

            ecoregionparameters.ToList().ForEach(x => parameters.Add(x.Key, x.Value));
                       
            //---------------DisturbanceReductionsParameterFile
            Parameter<string> DisturbanceReductionsParameterFile;
            if (TryGetParameter(Names.DisturbanceReductions, out DisturbanceReductionsParameterFile))
            {
                Allocation.Initialize(DisturbanceReductionsParameterFile.Value, parameters);
                Cohort.AgeOnlyDeathEvent += DisturbanceReductions.Events.CohortDied;
            }


            //----------------Read biomass estimation parameters
            
            string BiomassVariableFile = GetParameter(Names.BiomassVariables).Value;
            if (System.IO.File.Exists(BiomassVariableFile) == false) throw new System.Exception("File not found " + BiomassVariableFile);

            BiomassParamParser bioparser = new BiomassParamParser();
            Landis.Data.Load<BiomassParam>(BiomassVariableFile, bioparser);

            //----------------Read diameter growth tables

            string DiameterFile = GetParameter(Names.DiameterInputFile).Value;
            if (System.IO.File.Exists(DiameterFile) == false) throw new System.Exception("File not found " + DiameterFile);

            SiteOutputNames = new Dictionary<ActiveSite, string>();
            Parameter<string> OutputSitesFile;
            if (TryGetParameter(LocalOutput.PNEToutputsites, out OutputSitesFile))
            {
                Dictionary<string, Parameter<string>> outputfiles = LoadTable(LocalOutput.PNEToutputsites, null, AssignOutputFiles.ParameterNames.AllNames, true);
                AssignOutputFiles.MapCells(outputfiles, ref SiteOutputNames);
            }

        }

        public override void Initialize()
        {
            PlugIn.ModelCore.UI.WriteLine("Initializing " + Names.ExtensionName + " version " + typeof(PlugIn).Assembly.GetName().Version);
            Cohort.DeathEvent += DeathEvent;

            sitecohorts = PlugIn.ModelCore.Landscape.NewSiteVar<Landis.Library.DensityCohorts.SiteCohorts>();
            Landis.Utilities.Directory.EnsureExists("output");
            Landis.Library.DensityCohorts.Names.LoadParameters(parameters);
            Timestep = ((Parameter<int>)GetParameter(Names.Timestep)).Value;

            ObservedClimate.Initialize();

            SpeciesDensity = new SpeciesDensity();
            Landis.Library.DensityCohorts.SpeciesParameters.LoadParameters(SpeciesDensity);
            EcoregionData.Initialize();
            SiteVars.Initialize();
            string DynamicEcoregionFile = ((Parameter<string>)GetParameter(Names.DynamicEcoregionFile)).Value;
            DynamicEcoregions.Initialize(DynamicEcoregionFile, false);
            var TimestepData = DynamicEcoregions.EcoRegData[0];

            EcoregionData.EcoregionDynamicChange(TimestepData);

            string DynamicInputFile = ((Parameter<string>)GetParameter(Names.DynamicInputFile)).Value;
            DynamicInputs.Initialize(DynamicInputFile, false);

            string DiameterInputFile = ((Parameter<string>)GetParameter(Names.DiameterInputFile)).Value;
            DiameterInputs.Initialize(DiameterInputFile, false);

            DynamicEcoregions.ChangeDynamicParameters(0);  // Year 0

            Landis.Library.DensityCohorts.Cohorts.Initialize(Timestep);
            // This creates the cohorts - FIXME
            SiteCohorts.Initialize();

            // John McNabb: initialize climate library after EcoregionPnET has been initialized
            InitializeClimateLibrary();

            EstablishmentProbability.Initialize(Timestep);

            // Initialize Reproduction routines:
            Reproduction.SufficientResources = SufficientResources;
            Reproduction.Establish = Establish;
            Reproduction.AddNewCohort = AddNewCohort;
            Reproduction.MaturePresent = MaturePresent;
            Reproduction.PlantingEstablish = PlantingEstablish;
            Reproduction.DensitySeeds = DensitySeeds;
            Reproduction.EstablishmentProbability = EstabProbability;


            StartDate = new DateTime(((Parameter<int>)GetParameter(Names.StartYear)).Value, 1, 15);

            PlugIn.ModelCore.UI.WriteLine("Spinning up biomass or reading from maps...");

            string InitialCommunitiesTXTFile = GetParameter(Names.InitialCommunities).Value;
            string InitialCommunitiesMapFile = GetParameter(Names.InitialCommunitiesMap).Value;
            InitializeSites(InitialCommunitiesTXTFile, InitialCommunitiesMapFile, ModelCore);

            SeedingAlgorithms SeedAlgorithm = (SeedingAlgorithms)Enum.Parse(typeof(SeedingAlgorithms), parameters["SeedingAlgorithm"].Value);

            base.Initialize(ModelCore, SeedAlgorithm);


            ISiteVar<Landis.Library.DensityCohorts.ISiteCohorts> DensityCohorts = PlugIn.ModelCore.Landscape.NewSiteVar<Landis.Library.DensityCohorts.ISiteCohorts>();
            // Convert Density cohorts to biomasscohorts
            ISiteVar<Landis.Library.BiomassCohorts.ISiteCohorts> biomassCohorts = PlugIn.ModelCore.Landscape.NewSiteVar<Landis.Library.BiomassCohorts.ISiteCohorts>();
            // Convert Density cohorts to agecohorts
            ISiteVar<Landis.Library.AgeOnlyCohorts.ISiteCohorts> AgeCohortSiteVar = PlugIn.ModelCore.Landscape.NewSiteVar<Landis.Library.AgeOnlyCohorts.ISiteCohorts>();
           
            foreach (ActiveSite site in PlugIn.ModelCore.Landscape)
            {
                Cohort.SetSiteAccessFunctions(sitecohorts[site]);
                float tempRD = SiteVars.SiteRD[site];
                DensityCohorts[site] = sitecohorts[site];

                biomassCohorts[site] = sitecohorts[site];
                if (sitecohorts[site] != null && biomassCohorts[site] == null)
                {
                    throw new System.Exception("Cannot convert Density SiteCohorts to biomass site cohorts");
                }

                AgeCohortSiteVar[site] = sitecohorts[site];
                if (sitecohorts[site] != null && AgeCohortSiteVar[site] == null)
                {
                    throw new System.Exception("Cannot convert Density SiteCohorts to age-only site cohorts");
                }
            }
            ModelCore.RegisterSiteVar(DensityCohorts, "Succession.DensityCohorts");
            //ModelCore.RegisterSiteVar(biomassCohorts, "Succession.BiomassCohorts");
            ModelCore.RegisterSiteVar(AgeCohortSiteVar, "Succession.AgeCohorts");          
        }

        /// <summary>This must be called after EcoregionPnET.Initialize() has been called</summary>
        private void InitializeClimateLibrary()
        {
            // John McNabb: initialize ClimateRegionData after initializing EcoregionPnet

            Parameter<string> climateLibraryFileName;
            UsingClimateLibrary = TryGetParameter(Names.ClimateConfigFile, out climateLibraryFileName);
            if (UsingClimateLibrary)
            {
                PlugIn.ModelCore.UI.WriteLine($"Using climate library: {climateLibraryFileName.Value}.");
                Climate.Initialize(climateLibraryFileName.Value, false, ModelCore);
                ClimateRegionData.Initialize();
                
            }
            else
            {
                PlugIn.ModelCore.UI.WriteLine($"Using climate files in ecoregion parameters: {PlugIn.parameters["EcoregionParameters"].Value}.");
            }
        }

        public void AddNewCohort(ISpecies species, ActiveSite site, string reproductionType, double propBiomass = 1.0)
        {
            Cohort cohort = new Cohort(species, (ushort)Date.Year, (SiteOutputNames.ContainsKey(site)) ? SiteOutputNames[site] : null, (int)propBiomass);
            
            sitecohorts[site].AddNewCohort(cohort);

            if (reproductionType == "plant")
            {
                if (!sitecohorts[site].SpeciesEstablishedByPlant.Contains(species))
                    sitecohorts[site].SpeciesEstablishedByPlant.Add(species);
            }
            else if(reproductionType == "serotiny")
            {
                if (!sitecohorts[site].SpeciesEstablishedBySerotiny.Contains(species))
                    sitecohorts[site].SpeciesEstablishedBySerotiny.Add(species);
            }
            else if(reproductionType == "resprout")
            {
                if (!sitecohorts[site].SpeciesEstablishedByResprout.Contains(species))
                    sitecohorts[site].SpeciesEstablishedByResprout.Add(species);
            }
            else if(reproductionType == "seed")
            {
                if (!sitecohorts[site].SpeciesEstablishedBySeed.Contains(species))
                    sitecohorts[site].SpeciesEstablishedBySeed.Add(species);
            }


        }
        public bool MaturePresent(ISpecies species, ActiveSite site)
        {
            bool IsMaturePresent = sitecohorts[site].IsMaturePresent(species);
            return IsMaturePresent;
        }
        protected override void InitializeSite(ActiveSite site)//,ICommunity initialCommunity)
        {
            if (m == null)
            {
                m = new MyClock(PlugIn.ModelCore.Landscape.ActiveSiteCount);
            }

            m.Next();
            m.WriteUpdate();

             // Create new sitecohorts
            sitecohorts[site] = new SiteCohorts(StartDate,site,initialCommunity, UsingClimateLibrary, SiteOutputNames.ContainsKey(site)? SiteOutputNames[site] :null);

           
           
        }

        public override void InitializeSites(string initialCommunitiesText, string initialCommunitiesMap, ICore modelCore)
        {

            ModelCore.UI.WriteLine("   Loading initial communities from file \"{0}\" ...", initialCommunitiesText);
            Landis.Library.DensityCohorts.InitialCommunities.DatasetParser parser = new Landis.Library.DensityCohorts.InitialCommunities.DatasetParser(Timestep, ModelCore.Species);

            //Landis.Library.InitialCommunities.DatasetParser parser = new Landis.Library.InitialCommunities.DatasetParser(Timestep, ModelCore.Species);
            Landis.Library.DensityCohorts.InitialCommunities.IDataset communities = Landis.Data.Load<Landis.Library.DensityCohorts.InitialCommunities.IDataset>(initialCommunitiesText, parser);

            ModelCore.UI.WriteLine("   Reading initial communities map \"{0}\" ...", initialCommunitiesMap);
            IInputRaster<uintPixel> map;
            map = ModelCore.OpenRaster<uintPixel>(initialCommunitiesMap);
            using (map)
            {
                uintPixel pixel = map.BufferPixel;
                foreach (Site site in ModelCore.Landscape.AllSites)
                {
                    map.ReadBufferPixel();
                    uint mapCode = pixel.MapCode.Value;
                    if (!site.IsActive)
                        continue;

                    //if (!modelCore.Ecoregion[site].Active)
                    //    continue;

                    //modelCore.Log.WriteLine("ecoregion = {0}.", modelCore.Ecoregion[site]);

                    ActiveSite activeSite = (ActiveSite)site;
                    initialCommunity = communities.Find(mapCode);
                    if (initialCommunity == null)
                    {
                        throw new ApplicationException(string.Format("Unknown map code for initial community: {0}", mapCode));
                    }

                    InitializeSite(activeSite);
                }
            }
        }

        protected override void AgeCohorts(ActiveSite site,
                                            ushort years,
                                            int? successionTimestep
                                            )
        {
            DateTime date = new DateTime(PlugIn.StartDate.Year + PlugIn.ModelCore.CurrentTime - Timestep, 1, 15);

            DateTime EndDate = date.AddYears(years);

            //IEcoregionPnET ecoregion_pnet = EcoregionPnET.GetPnETEcoregion(PlugIn.ModelCore.Ecoregion[site]);

            //List<IEcoregionClimateVariables> climate_vars = UsingClimateLibrary ? EcoregionPnET.GetClimateRegionData(ecoregion_pnet, date, EndDate, Climate.Phase.Future_Climate) : EcoregionPnET.GetData(ecoregion_pnet, date, EndDate);

            DynamicEcoregions.ChangeDynamicParameters(PlugIn.ModelCore.CurrentTime);

            sitecohorts[site].Grow(site, successionTimestep.HasValue);
           
            Date = EndDate;
             
        }

        // Shade is calculated internally during the growth calculations
        public override byte ComputeShade(ActiveSite site)
        {
            return 0;
        }
        
        public override void Run()
        {
            bool isSuccessionTimestep = (ModelCore.CurrentTime % Timestep == 0);
            //FIXME --- JSF --- Better way to check dynamic parameters?
            if (isSuccessionTimestep && DynamicEcoregions.EcoRegData.ContainsKey(ModelCore.CurrentTime))
            {
                Landis.Library.DensityCohorts.IDynamicEcoregionRecord[] TimestepData = (Landis.Library.DensityCohorts.IDynamicEcoregionRecord[])DynamicEcoregions.EcoRegData[ModelCore.CurrentTime];

                EcoregionData.EcoregionDynamicChange(TimestepData);
            } 
            
            base.Run();
        }


        public void AddLittersAndCheckResprouting(object sender, Landis.Library.AgeOnlyCohorts.DeathEventArgs eventArgs)
        {
            if (eventArgs.DisturbanceType != null)
            {
                ActiveSite site = eventArgs.Site;
                Disturbed[site] = true;

                if (eventArgs.DisturbanceType.IsMemberOf("disturbance:fire"))
                    Reproduction.CheckForPostFireRegen(eventArgs.Cohort, site);
                else
                    Reproduction.CheckForResprouting(eventArgs.Cohort, site);
            }
            

        }
        
        // Resources (growing space) is calculated internally during the growth calculations
        public bool SufficientResources(ISpecies species, ActiveSite site)
        {
            return true;
        }

        public bool Establish(ISpecies species, ActiveSite site)
        {
            ISpeciesDensity spc = PlugIn.SpeciesDensity[species];
            double establishProbability = DynamicEcoregions.EstablishProbability[species, PlugIn.ModelCore.Ecoregion[site]];
            //bool Establish = sitecohorts[site].EstablishmentProbability.HasEstablished(spc);
            return ModelCore.GenerateUniform() < establishProbability;
        }

        
        //---------------------------------------------------------------------

        /// <summary>
        /// Determines if a species can establish on a site.
        /// This is a Delegate method to base succession.
        /// </summary>
        public bool PlantingEstablish(ISpecies species, ActiveSite site)
        {
            return true;
           
        }
        //---------------------------------------------------------------------
        /// <summary>
        /// Determines if a species can establish on a site.
        /// This is a Delegate method to the succession library.
        /// </summary>
        public int DensitySeeds(ISpecies species, ActiveSite site)
        {
            int availableSeed = 0;
            int totalseed_m_timestep = SpeciesDensity[species].TotalSeed * Timestep;
            if (SpeciesDensity[species].SpType < 0)
                availableSeed += totalseed_m_timestep; //site.cs Ln 1971
            else
            {
                if (SpeciesDensity[species].MaxSeedDist < 0)
                {
                    SiteCohorts mySiteCohorts = sitecohorts[site];
                    foreach (Cohort cohort in mySiteCohorts[species])
                    {
                        double loc_term = Math.Pow(cohort.Diameter / 25.4, 1.605);
                        //wenjuan changed on mar 30 2011
                        double double_val = loc_term * cohort.Treenumber * totalseed_m_timestep; //site.cs Ln 1991
                        availableSeed += (int)double_val;
                    }
                }
                else
                {
                    SiteCohorts mySiteCohorts = sitecohorts[site];
                    int matureTrees = 0;
                    List<Cohort> spCohorts = mySiteCohorts.AllCohorts;
                    if (mySiteCohorts[species] != null)
                    {
                        foreach (Cohort cohort in mySiteCohorts[species])
                        {
                            if (cohort.Age > SpeciesDensity[species].Maturity)
                            {
                                matureTrees += cohort.Treenumber;
                            }
                        }
                    }

                    int local_tseed = SpeciesDensity[species].TotalSeed;

                    double double_val = matureTrees * totalseed_m_timestep;  //modified from site.cs Ln 2024
                    availableSeed += (int)double_val;
                }
            }
            float float_rand = (float)ModelCore.ContinuousUniformDistribution.NextDouble();
            double double_value = availableSeed * (0.95 + float_rand * 0.1);  //from site.cs Ln 2045
            availableSeed = (int)double_value;

            return availableSeed;
        }
        //---------------------------------------------------------------------

        public double EstabProbability(ISpecies species, ActiveSite site)
        {
            if (PlugIn.ModelCore.CurrentTime <= 0)
                return 1.0;
            else
            {
                return DynamicEcoregions.EstablishProbability[species, sitecohorts[site].Ecoregion];
            }
        }
    }
}


>>>>>>> Density_Template
