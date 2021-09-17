using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Landis.Utilities;

using System.Diagnostics;
using System.IO;


namespace Landis.Extension.Succession.Density
{
    using SortedSITE = List<site>;
    public enum enum1 { TPA, BA, Bio, Car, IV, Seeds, RDensity, DBH };


    public class sites
    {
    	private float[] SeedRain;
        private float[] GrowthRates;
        private float[] MortalityRates;

        private List<float[]> GrowthRates_file    = new List<float[]>();
        private List<float[]> MortalityRates_file = new List<float[]>();
        private List<float[]> Volume_file         = new List<float[]>();

        private float[] Volume;

        private List<float> AreaList = new List<float>();

        private List<int> SpecIndexArray = new List<int>();
        private List<int> AgeIndexArray  = new List<int>();

        private site[] map;
        private site[] map70;        
        
        //private landunit[] map_landtype;

        private float[] biomassData;

        private int biomassNum;

        private float biomassThreshold;


        private uint rows, columns;                //Number of rows and columns contained in the whole map.

        private uint[] header = new uint[32];			 //Upperleft coordinates of sites

        //private static uint UINT_MAX = 0xffffffffU;
        private static uint UINT_MAX = uint.MaxValue;

        private static int NumTypes70Output = 8;


        private int SeedRainFlag;
        private int GrowthFlag;
        private int MortalityFlag;
        private int VolumeFlag;

        private int cellSize;
        private int MaxDistofAllSpec;
        private static uint specNum;

        private int timeStep;

        private int pro0or401=1;
        private int maxShadeTolerance;

        //Change bt YYF 2018/11
        public int[] flag_cut_GROUP_CUT      = new int[200];
        public int[] flag_plant_GROUP_CUT    = new int[200];
        public int[] num_TreePlant_GROUP_CUT = new int[200];
        public int[] flag_cut_GROUP_CUT_copy      = new int[200];
        public int[] flag_plant_GROUP_CUT_copy    = new int[200];
        public int[] num_TreePlant_GROUP_CUT_copy = new int[200];


        private int[] AgeDistStat_Year;
        private int[] AgeDistStat_AgeRange;


        private int[] OutputGeneralFlagArray;
        private int[] OutputAgerangeFlagArray;
        private int[] SpeciesAgerangeArray;

        //=============================================================================================
        //=============================================================================================

        

        public SortedSITE SortedIndex;

        public int FlagAgeRangeOutput { get; set; }
        public int Flag_AgeDistStat   { get; set; }

        public float XLLCorner { get; set; }
        public float YLLCorner { get; set; }


        public int MaxDistance { set { MaxDistofAllSpec = value; } }
        public uint SpecNum    { get { return specNum;   } }
        public int Pro0or401   { get { return pro0or401; } }
        //Changed By YYF 2018.11
        public int TimeStepHarvest ;
        //Changed By YYF 2019.4
        public int TimeStepFire;
        
        public int CellSize 
        {              
            get { return cellSize;  }
            set { cellSize = value; }
        }

        public int SuccessionTimeStep
        {
            get { return timeStep;  }
            set { timeStep = value; }
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

        public int MaxShadeTolerance
        {
            set { maxShadeTolerance = value; }
        }



        //The first parameter is the number of species in the model.  
        //The other parameters dimension the number of rows and columns on the map.
        public sites(uint species_num, uint row_num, uint column_num)
        {
            species junk = new species(species_num);

            pro0or401 = 1;
            
            map70 = new site[row_num * column_num];
            
            rows 	=    row_num;
            columns = column_num;
        }


        //Constructor.  Initial values must be set later by the dim method.  This is
        //useful so that a SITES object may be declared statically but set up dynamically.

        //Constructor.  The maximum number of species will be set to species.
        //The map dimensions will can be set by a call to SITES::dim after construction
        public sites()
        {
        	rows 	= 0;
            columns = 0;
            
            FlagAgeRangeOutput = 0;

            map70 					= null;
            //map_landtype 			= null;
            SeedRain 				= null;
            GrowthRates 			= null;
            MortalityRates 			= null;
            Volume 					= null;            
            
            OutputGeneralFlagArray  = null;
            OutputAgerangeFlagArray = null;
            SpeciesAgerangeArray 	= null;
            AgeDistStat_AgeRange 	= null;
            AgeDistStat_Year 		= null;
        }


        ~sites()
        {        	
            map70 					= null;
            //map_landtype 			= null;
            SeedRain 				= null;
            GrowthRates 			= null;
            MortalityRates 			= null;
            GrowthRates_file 		= null;
            MortalityRates_file 	= null;
            Volume_file 			= null;
            Volume 					= null;
            OutputGeneralFlagArray 	= null;
            OutputAgerangeFlagArray = null;
            SpeciesAgerangeArray 	= null;
            AgeDistStat_AgeRange 	= null;
            AgeDistStat_Year 		= null;
            
            OutputGeneralFlagArray  = null;
            OutputAgerangeFlagArray = null;
            SpeciesAgerangeArray 	= null;
            AgeDistStat_AgeRange 	= null;
            AgeDistStat_Year 		= null;
        }

        /// <summary>
        /// Add by YYF 2018/11
        /// </summary>
        public double[] BiomassHarvestCost;
        public double[] CarbonHarvestCost;
        public int Harvestflag;

        public double stocking_x_value;
        public double stocking_y_value;
        public double stocking_z_value;

        public void Harvest70outputdim()
        {
            BiomassHarvestCost = new double[rows * columns];
            CarbonHarvestCost = new double[rows * columns];
            Harvestflag = 1;
            Array.Clear(BiomassHarvestCost, 0, (int)rows * (int)columns);
            Array.Clear(CarbonHarvestCost, 0, (int)rows * (int)columns);
        }
        

        //This will dimension the size of the map to i rows and j columns, and it
        //will initialize the SPECIES class to species..
        //This will set the number of species, rows and columns in the map.
        public void dim(uint species_num, uint row_num, uint col_num)
        {           
            site temp = new site();
            temp.Num_Species = species_num;

            specNum = species_num;

            pro0or401 = 1;

            uint total_size = row_num * col_num;
            map70 		 = new site    [total_size];
            //map_landtype = new landunit[total_size];
            
            //Console.WriteLine("dim row = {0}, col = {1}", row_num, col_num);
            for (int i = 0; i < total_size; i++)
            {
                //Console.WriteLine("initilize map_landtype index {0}", i);
                //map_landtype[i] = new landunit();
            }                
            

            SeedRain = new float[species_num * (MaxDistofAllSpec / cellSize + 2)];

            long arr_size1 = species_num * (320 / timeStep + 1);
            GrowthRates    = new float[arr_size1];
            MortalityRates = new float[arr_size1];
            Volume 		   = new float[arr_size1];
            
            rows 	= row_num;
            columns = col_num;

            OutputGeneralFlagArray  = new int[(species_num + 1) * NumTypes70Output];
            OutputAgerangeFlagArray = new int[(species_num + 1) * NumTypes70Output];

            long arr_size2 = species_num * 500 / timeStep;
            SpeciesAgerangeArray    = new int[arr_size2];
            AgeDistStat_Year 		= new int[arr_size2];
            AgeDistStat_AgeRange 	= new int[arr_size2];
            
        }



        //This will referrence a site map by row i and column j.
        public site this[uint i, uint j]
        {
            get
            {                
                Debug.Assert(i > 0 && i <= rows);
                Debug.Assert(j > 0 && j <= columns);

                return map70[(i - 1) * columns + j - 1];
            }
            set
            {
                Debug.Assert(i > 0 && i <= rows);
                Debug.Assert(j > 0 && j <= columns);
 
                map70[(i - 1) * columns + j - 1] = value;
            }
        }


        //Read in all site data.
        //This will read in a set of sites from infile.
        /*public void read(StreamReader infile)
        {            
            for (int i = 0; i < rows * columns; ++i)
            {
                if (system1.LDeof(infile)) 
                	throw new Exception("SITES::read(FILE *)->Premature end of file.");

                map70[i].read(infile);
            }
        }*/


        //Write out all site data.
        //This will write out a set of sites to outfile.
        public void write(StreamWriter outfile)
        {            
            for (int i = 0; i < rows * columns; ++i)
            {
                map70[i].write(outfile);
                outfile.Write('\n');
            }
        }


        //This will dump a set of sites to the CRT screen.
        public void dump()
        {            
            for (uint i = rows; i >= 1; --i)
                for (uint j = 1; j <= columns; ++j)
                {
                    Console.WriteLine("site({1},{2}) =", i, j);
                    //this[i, j].dump();//cnx
                }
        }


        //This will return the number of rows in the sites structure.
        public uint numRows
        {
            get { return rows; }
        }


        //This will return the number of columns in the sites structure.
        public uint numColumns
        {            
            get { return columns; }
        }

        //This will return the total number of sites.
        public uint number()
        {            
            return rows * columns;
        }


        //This will return the number of sites with active land units on the map.
        //public int numActive()
        //{            
        //    return map_landtype.Count(x => x.active());
        //}


        //add by YYF 2018/11
        //There is a return at the begining
        public void AftStChg(int i, int j)
        //After Site Change
        //This function does combination and delete of the seprated site made by BefStChg(int i, int j)
        //insert this site to the sorted vector
        {
            return;
            //SITE_insert(0, sitetouse, i, j);
            //return;
        }

        public void BefStChg(int i, int j)
        //Before Site Change
        //This function back up a site and following changes are based on this seprated site
        //sort vector is not touched here
        {
            return;
            //SITE temp;
            //temp = locateSitePt(i, j);
            //*sitetouse = temp;
            //if (temp.numofsites == 1)
            //{
            //    int pos;
            //    int ifexist = 0;
            //    SITE_LocateinSortIndex(sitetouse, pos, ifexist);
            //    if (ifexist != 0)
            //    {
            //        List<SITE>.Enumerator temp_sitePtr;
            //        temp_sitePtr = SortedIndex.begin();
            //        SortedIndex.erase(temp_sitePtr + pos);
            //        temp = null;
            //    }
            //    else
            //    {
            //        Console.Write("num of vectors {0:D}\n", SortedIndex.size());
            //        Console.Write("ERROR ERROR ERROR ERROR!!~~~{0:D}\n", pos);
            //        temp.dump();
            //        SortedIndex.at(pos).dump();
            //        SortedIndex.at(pos - 1).dump();
            //        SortedIndex.at(pos - 2).dump();
            //        SortedIndex.at(0).dump();
            //        SortedIndex.at(1).dump();
            //    }
            //}
            //else if (temp.numofsites <= 0)
            //{
            //    Console.Write("NO NO NO NO NO\n");
            //}
            //else
            //{
            //    temp.numofsites--;
            //}
            ////sitetouse->numofsites=1;
            //fillinSitePt(i, j, sitetouse);
            //return;
        }

        public void Harvest70outputIncreaseBiomassvalue(int i, int j, double value)
        {
            int x;
#if BOUNDSCHECK  
		if (i <= 0 || i> rows || j <= 0 || j> columns)  
		{  
			 string err = new string(new char[80]);   
			 err = string.Format("SITES::operator() (int,int)-> ({0:D}, {1:D}) are illegal map					  coordinates", i, j);   
			 throw new Expection(err);    
		}   
#endif

            x = (i - 1) * (int)columns;
            x = x + j - 1;
            BiomassHarvestCost[x] += value; // Add by Qia Oct 07 2008
        }

        public void Harvest70outputIncreaseCarbonvalue(int i, int j, double value)
        {
            int x;
#if BOUNDSCHECK 
			if (i <= 0 || i> rows || j <= 0 || j> columns) 
			{    
				 string err = new string(new char[80]);   
				 err = string.Format("SITES::operator() (int,int)-> ({0:D}, {1:D}) are illegal map						  coordinates", i, j);   
				 throw new Expection(err);    
			}
#endif

            x = (i - 1) * (int)columns;
            x = x + j - 1;
            CarbonHarvestCost[x] += value; // Add by Qia Oct 07 2008
        }

        //sets the sites header info.
        //this might be a problem
        public void setHeader(uint[] dest)
        {
            for (int i = 0; i < 32; ++i)
            {
                header[i] = dest[i];
            }


            byte[] bytes = BitConverter.GetBytes(header[28]);
            XLLCorner = System.BitConverter.ToSingle(bytes, 0);

            bytes = BitConverter.GetBytes(header[29]);
            YLLCorner = System.BitConverter.ToSingle(bytes, 0);

            //xLLCorner = header[28];
            //yLLCorner = header[29];
        }

        public uint[] Header
        {
            get { return header; }
        }


        public float Header_ele_to_float(int index)
        {
            byte[] bytes = BitConverter.GetBytes(header[index]);
            float ret_val = System.BitConverter.ToSingle(bytes, 0);

            return ret_val;
        }



        public bool inBounds(int r, int c)
        {
            return (r >= 1 && r <= rows && c >= 1 && c <= columns);
        }


        // Compare two sites to see the relation between them
        // return 0:equal; return 1: site1 is bigger; return 2: site2 is bigger; -1: error
        public int SITE_compare(uint site1_x, uint site1_y, uint site2_x, uint site2_y)
        {            
            uint x = (site1_x - 1) * columns + site1_y - 1;
            uint y = (site2_x - 1) * columns + site2_y - 1;

            site site1 = map[x];
            site site2 = map[y];
            
            return SITE_compare(site1, site2);
        }


        public int SITE_compare(site site1, site site2)
        {
            specie specie1 = site1.first();
            specie specie2 = site2.first();

            int num = (int)specie1.getAgeVectorNum();
            
            while (specie1 != null && specie2 != null)
            {
                if (specie1.VegPropagules > specie2.VegPropagules) return 1;
                if (specie1.VegPropagules < specie2.VegPropagules) return 2;

                if (specie1.DisPropagules > specie2.DisPropagules) return 1;
                if (specie1.DisPropagules < specie2.DisPropagules) return 2;
                
                for (int i = 0; i < num; ++i)
                {
                    if (specie1.getAgeVector(i) > specie2.getAgeVector(i))
                        return 1;
                    
                    if (specie1.getAgeVector(i) < specie2.getAgeVector(i))
                        return 2;
                }
                
                specie1 = site1.next();
                specie2 = site2.next();
            }

            return 0;
        }



        public void fillinSitePt(uint i, uint j, site local_site)
        {
            map[(i - 1) * columns + j - 1] = local_site;
        }


        //find the landtype
        public site locateSitePt(uint i, uint j)
        {            
            return map[(i - 1) * columns + j - 1];
        }


        //public void fillinLanduPt(uint i, uint j, landunit landUnitPt)
        //{
        //    map_landtype[(i - 1) * columns + j - 1] = landUnitPt;
        //}

        public landunit locateLanduPt(uint i, uint j)
        {
            //Console.WriteLine("map_landtype len = {0}, index = {1}", map_landtype.Length, (i - 1) * columns + j - 1);
            //return map_landtype[(i - 1) * columns + j - 1];

            //landispro and landis2 look at the map in different ways. landispro looks at the map from the lower left 
            //corner, while landis 2 does that from the upper left corner. therefore we need to make the following change:
            int landispro_i = (int)(rows - i + 1);
            if (PlugIn.ModelCore.Landscape[landispro_i, (int)j].DataIndex == 0)
                return PlugIn.gl_landUnits["empty"];
            else
                return PlugIn.gl_landUnits[PlugIn.ModelCore.Ecoregion[PlugIn.ModelCore.Landscape.GetSite(landispro_i, (int)j)].Name];
        }

        public int convertLP_Row(int i)
        {
            //Convert LANDIS-II row coordinate to LANDIS PRO row coordinate 
            //corner, while landis 2 does that from the upper left corner. therefore we need to make the following change:
            return (int)(rows - i + 1);
        }


        //When a site disappears, delete it
        public int site_delete(int pos_sortIndex, site site, uint i, uint j)
        {            
            uint x = (i - 1) * columns + j - 1;

            if (site != SortedIndex[pos_sortIndex])
                return 0;
            
            if (site != map[x])
                return 0;
            
            site = null;
            
            SortedIndex.RemoveAt(pos_sortIndex);
            
            return 1;
        }



        //when there is a new site during succession or whatever, we need to 
        //check if the new site already exists, if yes combine with existing one
        //if not insert to the position according to sort
        public void SITE_insert(int pos_sortIndex, site site_in, uint i, uint j)
        {           
            uint x = (i - 1) * columns + j - 1;

            int ifexist = 0;
            int pos = 0;
            
            SITE_LocateinSortIndex(site_in, ref pos, ref ifexist);
            
            if (ifexist != 0)
            {
                map[x] = SortedIndex[pos];
                map[x].NumSites++;
            }
            else
            {
                site temp = site_in;//cnx deep copy

                temp.NumSites = 1;

                map[x] = temp;
                
                SortedIndex.Insert(pos, temp);
            }
        }



        //Find if a new site exists in sorted list
		//If a new site exists, find its location and set *ifexist as 1
        //if this no site matches this one, find location before which new site pointer should be inserted
        public int SITE_LocateinSortIndex(site site, ref int pos, ref int ifexist)
        {           
            Debug.Assert(SortedIndex.Count <= int.MaxValue && SortedIndex.Count >= int.MinValue);
            
            if (SortedIndex.Count == 0)
            {
                Console.WriteLine("No site at all wrong wrong wrong");
                return -1;
            }

            int begin = 0;
            int end = SortedIndex.Count - 1;
            int mid = (begin + end) / 2;
            
            site temp = SortedIndex[mid];
            
            int temp_flag;
            
            ifexist = 0;
            
            while (begin < end)
            {
                temp_flag = SITE_compare(site, temp);

                if (temp_flag == 0)
                {
                    ifexist = 1;
                    pos = mid;
                    return 1;
                }
                else if (temp_flag == 1)
                {
                    begin = mid + 1;
                    mid = (begin + end) / 2;
                }
                else if (temp_flag == 2)
                {
                    end = mid - 1;
                    mid = (begin + end) / 2;
                }
                else 
                	return -1;
                
                temp = SortedIndex[mid];
            }//end while

            temp_flag = SITE_compare(site, temp);
            
            if (temp_flag == 0)
            {
                ifexist = 1;
                pos = mid;
                return 1;
            }
            else if (temp_flag == 1 || temp_flag == 2)
            {
                ifexist = 0;
                pos = mid + (2 - temp_flag);
                return 0;
            }
            else 
            	return -1;
        }



        // sort the pointers to sites use babble algorithm to sort the initial site list array
        public void SITE_sort()
        {            
            for (int i = SortedIndex.Count - 1; i > 0; --i)
            {
            	for (int j = 0; j <= i - 1; ++j)
                {
                    site site1 = SortedIndex[j];
                    site site2 = SortedIndex[j + 1];

                    if (SITE_compare(site1, site2) == 1)
                    {
                        site temp = SortedIndex[j];
                        SortedIndex[j] = SortedIndex[j + 1];
                        SortedIndex[j + 1] = temp;
                    }
                }
            }
        }





        //According to shade-rule and RD-rule determine species succession on sites, No Return Value
        public void SiteDynamics(int RDflag, uint Row, uint Col)
        {            
            site siteptr = this[Row, Col];

            landunit l = locateLanduPt(Row, Col);

            if (0 == RDflag || 1 == RDflag || 2 == RDflag || 3 == RDflag)
            {
                GetSeedNumberOnSite(Row, Col);
                
                SeedGermination(siteptr, l, RDflag);
                
                GetRDofSite(Row, Col);

                if (3 == RDflag)
                    NaturalMortality(siteptr, Row, Col, 0);//kill all ages of trees
                else
                    NaturalMortality(siteptr, Row, Col, 1);//kill the youngest of trees

                GetRDofSite(Row, Col);

            }
            else
            {
                Debug.Assert(RDflag == 4); //otherwise, Site Dynamics Parameter Error.

                double thres_RD4 = timeStep / 100000.0;
                double tmp = system1.drand();
                
                if (tmp > thres_RD4)
                {
                    NaturalMortality(siteptr, Row, Col, 0);//kill all ages of trees

                    GetRDofSite(Row, Col);                    

                    if (siteptr.RD > l.MaxRD)
                    {
                        Selfthinning(siteptr, l, Row, Col);

                        GetRDofSite(Row, Col);
                    }
                }
                else
                {
                    double targetRD = l.MaxRDArray(0);

                    NaturalMortality_killbytargetRD(siteptr, Row, Col, targetRD);
                    
                    GetRDofSite(Row, Col);
                }

            }



            if (siteptr.RD <= 0.0)
            {
                //Console.WriteLine("\nrow = {0}, col = {1}, RDflag = {2}", Row, Col, RDflag);
                SeedGermination(siteptr, l, RDflag);

                GetRDofSite(Row, Col);

                NaturalMortality(siteptr, Row, Col, 1);//kill the youngest of trees

                GetRDofSite(Row, Col);
            }

            //Console.ReadLine();
        }



        //Bubble sort function 
        public void ListbubbleSort()
        {
            for (int i = AreaList.Count - 1; i > 0; --i)
            {
            	for (int j = 1; j <= i; ++j)
                {
                	if (AreaList[j - 1] > AreaList[j])
                    {
                        {
                            float tmp = AreaList[j - 1];
                            AreaList[j - 1] = AreaList[j];
                            AreaList[j] = tmp;
                        }
                        {
                            int tmp = SpecIndexArray[j - 1];
                            SpecIndexArray[j - 1] = SpecIndexArray[j];
                            SpecIndexArray[j] = tmp;
                        }
                        {
                            int tmp = AgeIndexArray[j - 1];
                            AgeIndexArray[j - 1] = AgeIndexArray[j];
                            AgeIndexArray[j] = tmp;
                        }
                    }
                }   
            } 
        }



        //this function is rather strange. it seems some "if" is missing, only {} is left
        //sometimes, it is "<", sometimes, it is "<="
        public void Selfthinning(site siteptr, landunit l, uint row, uint col)
        {
            int[] quaterPercent = new int[specNum * 5];

            double Area_tobeThin;

            float TargetRD = siteptr.RD - l.MaxRD;

            if (TargetRD <= 0)
            {
                quaterPercent = null;
                return;
            }
            else
            {
                Area_tobeThin = TargetRD * cellSize * cellSize;
            }

            float[] thinning_percentatge = new float[5];
            thinning_percentatge[0] = 0.95f;
            thinning_percentatge[1] = 0.90f;
            thinning_percentatge[2] = 0.85f;
            thinning_percentatge[3] = 0.80f;
            thinning_percentatge[4] = 0.75f;


            for (int i = 1; i <= siteptr.Num_Species; i++)
            {
                int tmp_index = (i - 1) * 4;
                int longevity_d_timestep = siteptr.specAtt(i).Longevity / timeStep;

                quaterPercent[tmp_index + 0] = 1;
                quaterPercent[tmp_index + 1] = longevity_d_timestep / 4;
                quaterPercent[tmp_index + 2] = longevity_d_timestep / 2;
                quaterPercent[tmp_index + 3] = longevity_d_timestep / 4 * 3;
                quaterPercent[tmp_index + 4] = longevity_d_timestep;
            }



            for (int j = 0; j < 4; j++)
            {
                AreaList.Clear();

                SpecIndexArray.Clear();

                AgeIndexArray.Clear();

                if (j == 0)
                {
                    float subArea_tobeThin = 0;

                    for (int spec_i = 1; spec_i <= siteptr.Num_Species; spec_i++)
                    {
                        int local_id = (spec_i - 1) * 4 + j;

                        int age_beg = quaterPercent[local_id];
                        int age_end = quaterPercent[local_id + 1];

                        for (int age_i = age_beg; age_i < age_end; age_i++)
                        {
                            double term1 = Math.Pow((GetGrowthRates(spec_i, age_i, l.LtID) / 25.4), 1.605);
                            float  term2 = siteptr.specAtt(spec_i).MaxAreaOfSTDTree;
                            float  term3 = thinning_percentatge[siteptr.specAtt(spec_i).Shade_Tolerance - 1];
                            uint   term4 = siteptr.SpecieIndex(spec_i).getTreeNum(age_i, spec_i);


                            double temp = term1 * term2;
                            subArea_tobeThin += (float)(temp * term3 * term4);
                            

                            AreaList.Add((float)temp);

                            SpecIndexArray.Add(spec_i);

                            AgeIndexArray.Add(age_i);

                        }

                    }


                    ListbubbleSort();


                    if (subArea_tobeThin >= Area_tobeThin)
                    {
                        for (int i = 0; i < AreaList.Count; i++)
                        {
                            specie local_specie = siteptr.SpecieIndex(SpecIndexArray[i]);

                            uint local_tree_num = local_specie.getTreeNum(AgeIndexArray[i], SpecIndexArray[i]);
                            
                            float local_thin_percent = thinning_percentatge[siteptr.specAtt(SpecIndexArray[i]).Shade_Tolerance - 1];
                            

                            if (local_tree_num > 0)
                            {
                                float tempAreaInvolveTreeNum = AreaList[i] * local_tree_num * local_thin_percent;

                                if (tempAreaInvolveTreeNum > Area_tobeThin)
                                {
                                    double tmp_val = Area_tobeThin / AreaList[i];

                                    Debug.Assert(tmp_val <= uint.MaxValue && tmp_val >= int.MinValue);
                                    
                                    int treesToremove = (int)tmp_val;

                                    int treesLeft = (int)local_tree_num - treesToremove;

                                    local_specie.setTreeNum(AgeIndexArray[i], SpecIndexArray[i], Math.Max(0, treesLeft));

                                    //if (treesToremove > 0)
                                    //{
                                    //    Console.WriteLine("Remain - {0}, Initial - {1}", Math.Max(0, treesLeft), local_tree_num);
                                    //}


                                    Area_tobeThin -= tempAreaInvolveTreeNum;

                                    quaterPercent = null;

                                    return;

                                }
                                else
                                {
                                    float float_treesLeft = local_tree_num * (1 - local_thin_percent);

                                    Debug.Assert(float_treesLeft <= int.MaxValue && float_treesLeft >= 0);
                                    
                                    int treesLeft = (int)float_treesLeft;

                                    local_specie.setTreeNum(AgeIndexArray[i], SpecIndexArray[i], Math.Max(0, treesLeft));

                                    //if (treesLeft != local_tree_num)
                                    //{
                                    //    Console.WriteLine("Remain - {0}, Initial - {1}", Math.Max(0, treesLeft), local_tree_num);
                                    //}

                                    Area_tobeThin -= tempAreaInvolveTreeNum;

                                }

                            }

                        }

                    }

                    else
                    {

                        for (int i = 0; i < AreaList.Count; i++)
                        {
                            specie local_specie = siteptr.SpecieIndex(SpecIndexArray[i]);

                            uint local_tree_num = local_specie.getTreeNum(AgeIndexArray[i], SpecIndexArray[i]);

                            float local_thin_percent = thinning_percentatge[siteptr.specAtt(SpecIndexArray[i]).Shade_Tolerance - 1];
                            

                            if (local_tree_num > 0)
                            {
                                float tempAreaInvolveTreeNum = AreaList[i] * local_tree_num * local_thin_percent;

                                float float_treesLeft = local_tree_num * (1 - local_thin_percent);
                                
                                Debug.Assert(float_treesLeft <= int.MaxValue && float_treesLeft >= 0);
                                
                                int treesLeft = (int)float_treesLeft;

                                local_specie.setTreeNum(AgeIndexArray[i], SpecIndexArray[i], Math.Max(0, treesLeft));

                                //if (treesLeft != local_tree_num)
                                //{
                                //    Console.WriteLine("Remain - {0}, Initial - {1}", Math.Max(0, treesLeft), local_tree_num);
                                //}

                                Area_tobeThin -= tempAreaInvolveTreeNum;

                            }

                        }

                    }

                }

                AreaList.Clear();

                SpecIndexArray.Clear();

                AgeIndexArray.Clear();


                if (j == 1)
                {
                    float subArea_tobeThin = 0;

                    for (int spec_i = 1; spec_i <= siteptr.Num_Species; spec_i++)
                    {
                        int local_id = (spec_i - 1) * 4 + j;
                        
                        int age_beg = quaterPercent[local_id    ];
                        int age_end = quaterPercent[local_id + 1];

                        for (int age_i = age_beg; age_i < age_end; age_i++)
                        {
                            double term1 = Math.Pow((GetGrowthRates(spec_i, age_i, l.LtID) / 25.4), 1.605);
                            float  term2 = siteptr.specAtt(spec_i).MaxAreaOfSTDTree;
                            float  term3 = thinning_percentatge[siteptr.specAtt(spec_i).Shade_Tolerance - 1];
                            uint   term4 = siteptr.SpecieIndex(spec_i).getTreeNum(age_i, spec_i);


                            double temp = term1 * term2;
                            subArea_tobeThin += (float)(temp * term3 * term4);
                            
                            AreaList.Add((float)temp);

                            SpecIndexArray.Add(spec_i);

                            AgeIndexArray.Add(age_i);

                        }

                    }


                    ListbubbleSort();


                    if (subArea_tobeThin >= Area_tobeThin)
                    {
                        for (int i = 0; i < AreaList.Count; i++)
                        {
                            specie local_specie = siteptr.SpecieIndex(SpecIndexArray[i]);

                            uint local_tree_num = local_specie.getTreeNum(AgeIndexArray[i], SpecIndexArray[i]);
                            
                            float local_thin_percent = thinning_percentatge[siteptr.specAtt(SpecIndexArray[i]).Shade_Tolerance - 1];


                            if (local_tree_num > 0)
                            {
                                float tempAreaInvolveTreeNum = AreaList[i] * local_tree_num * local_thin_percent / 2;

                                if (tempAreaInvolveTreeNum > Area_tobeThin)
                                {
                                    double tmp_val = Area_tobeThin / AreaList[i];

                                    Debug.Assert(tmp_val <= int.MaxValue && tmp_val >= int.MinValue);

                                    int treesToremove = (int)tmp_val;

                                    int treesLeft = (int)local_tree_num - treesToremove;

                                    local_specie.setTreeNum(AgeIndexArray[i], SpecIndexArray[i], Math.Max(0, treesLeft));

                                    //if (treesToremove > 0)
                                    //{
                                    //    Console.WriteLine("Remain - {0}, Initial - {1}", Math.Max(0, treesLeft), local_tree_num);
                                    //}

                                    Area_tobeThin -= tempAreaInvolveTreeNum;

                                    quaterPercent = null;

                                    return;

                                }

                                else
                                {                                    
                                    float float_treeleft = local_tree_num * (1 - local_thin_percent / 2);
                                    
                                    Debug.Assert(float_treeleft <= int.MaxValue && float_treeleft >= int.MinValue);
                                    
                                    int treesLeft = (int)float_treeleft;

                                    local_specie.setTreeNum(AgeIndexArray[i], SpecIndexArray[i], Math.Max(0, treesLeft));

                                    //if (treesLeft != local_tree_num)
                                    //{
                                    //    Console.WriteLine("Remain - {0}, Initial - {1}", Math.Max(0, treesLeft), local_tree_num);
                                    //}

                                    Area_tobeThin -= tempAreaInvolveTreeNum;

                                }

                            }

                        }

                    }
                    else
                    {
                        for (int i = 0; i < AreaList.Count; i++)
                        {
                            specie local_specie = siteptr.SpecieIndex(SpecIndexArray[i]);

                            uint local_tree_num = local_specie.getTreeNum(AgeIndexArray[i], SpecIndexArray[i]);
                            
                            float local_thin_percent = thinning_percentatge[siteptr.specAtt(SpecIndexArray[i]).Shade_Tolerance - 1];


                            if (local_tree_num > 0)
                            {
                                float tempAreaInvolveTreeNum = AreaList[i] * local_tree_num * local_thin_percent / 2;
                                {
                                    float float_treeleft = local_tree_num * (1 - local_thin_percent / 2);

                                    Debug.Assert(float_treeleft <= int.MaxValue && float_treeleft >= int.MinValue);
                                    
                                    int treesLeft = (int)float_treeleft;

                                    local_specie.setTreeNum(AgeIndexArray[i], SpecIndexArray[i], Math.Max(0, treesLeft));

                                    //if (treesLeft != local_tree_num)
                                    //{
                                    //    Console.WriteLine("Remain - {0}, Initial - {1}", Math.Max(0, treesLeft), local_tree_num);
                                    //}

                                    Area_tobeThin -= tempAreaInvolveTreeNum;

                                }

                            }

                        }

                    }

                }


                      AreaList.Clear();
                SpecIndexArray.Clear();
                 AgeIndexArray.Clear();



                if (j == 2)
                {
                    float subArea_tobeThin = 0;

                    for (int spec_i = 1; spec_i <= siteptr.Num_Species; spec_i++)
                    {
                        int local_id = (spec_i - 1) * 4 + j;
                        
                        int age_beg = quaterPercent[local_id    ];
                        int age_end = quaterPercent[local_id + 1];

                        for (int age_i = age_beg; age_i < age_end; age_i++)
                        {
                            double term1 = Math.Pow((GetGrowthRates(spec_i, age_i, l.LtID) / 25.4), 1.605);
                            float  term2 = siteptr.specAtt(spec_i).MaxAreaOfSTDTree;
                            float  term3 = thinning_percentatge[siteptr.specAtt(spec_i).Shade_Tolerance - 1];
                            uint   term4 = siteptr.SpecieIndex(spec_i).getTreeNum(age_i, spec_i);


                            double temp = term1 * term2;
                            subArea_tobeThin += (float)(temp * term3 * term4);


                            AreaList.Add((float)temp);

                            SpecIndexArray.Add(spec_i);

                            AgeIndexArray.Add(age_i);

                        }

                    }


                    ListbubbleSort();


                    if (subArea_tobeThin >= Area_tobeThin)
                    {
                        for (int i = 0; i < AreaList.Count; i++)
                        {
                            specie local_specie = siteptr.SpecieIndex(SpecIndexArray[i]);

                            uint local_tree_num = local_specie.getTreeNum(AgeIndexArray[i], SpecIndexArray[i]);
                            
                            float local_thin_percent = thinning_percentatge[siteptr.specAtt(SpecIndexArray[i]).Shade_Tolerance - 1];


                            if (local_tree_num > 0)
                            {
                                float tempAreaInvolveTreeNum = AreaList[i] * local_tree_num * local_thin_percent / 4;


                                if (tempAreaInvolveTreeNum > Area_tobeThin)
                                {
                                    double tmp_val = Area_tobeThin / (AreaList[i]);

                                    Debug.Assert(tmp_val <= int.MaxValue && tmp_val >= int.MinValue);

                                    int treesToremove = (int)tmp_val;

                                    int treesLeft = (int)local_tree_num - treesToremove;

                                    local_specie.setTreeNum(AgeIndexArray[i], SpecIndexArray[i], Math.Max(0, treesLeft));

                                    //if (treesToremove > 0)
                                    //{
                                    //    Console.WriteLine("Remain - {0}, Initial - {1}", Math.Max(0, treesLeft), local_tree_num);
                                    //}

                                    Area_tobeThin -= tempAreaInvolveTreeNum;

                                    quaterPercent = null;

                                    return;

                                }
                                else
                                {                                 
                                    float float_treeleft = local_tree_num * (1 - local_thin_percent / 4);
                                    
                                    Debug.Assert(float_treeleft <= int.MaxValue || float_treeleft >= int.MinValue);
                                    
                                    int treesLeft = (int)float_treeleft;

                                    local_specie.setTreeNum(AgeIndexArray[i], SpecIndexArray[i], Math.Max(0, treesLeft));

                                    //if (treesLeft != local_tree_num)
                                    //{
                                    //    Console.WriteLine("Remain - {0}, Initial - {1}", Math.Max(0, treesLeft), local_tree_num);
                                    //}

                                    Area_tobeThin -= tempAreaInvolveTreeNum;

                                }

                            }

                        }

                    }

                    else
                    {
                        for (int i = 0; i < AreaList.Count; i++)
                        {
                            specie local_specie = siteptr.SpecieIndex(SpecIndexArray[i]);

                            uint local_tree_num = local_specie.getTreeNum(AgeIndexArray[i], SpecIndexArray[i]);
                            
                            float local_thin_percent = thinning_percentatge[siteptr.specAtt(SpecIndexArray[i]).Shade_Tolerance - 1];


                            if (local_tree_num > 0)
                            {
                                float tempAreaInvolveTreeNum = AreaList[i] * local_tree_num * local_thin_percent / 4;
                                {
                                    float float_treeleft = local_tree_num * (1 - local_thin_percent / 4);
                                    
                                    Debug.Assert(float_treeleft <= int.MaxValue && float_treeleft >= int.MinValue);
                                    
                                    int treesLeft = (int)float_treeleft;

                                    local_specie.setTreeNum(AgeIndexArray[i], SpecIndexArray[i], Math.Max(0, treesLeft));

                                    //if (treesLeft != local_tree_num)
                                    //{
                                    //    Console.WriteLine("Remain - {0}, Initial - {1}", Math.Max(0, treesLeft), local_tree_num);
                                    //}

                                    Area_tobeThin -= tempAreaInvolveTreeNum;

                                }

                            }

                        }

                    }

                }


                      AreaList.Clear();
                SpecIndexArray.Clear();
                 AgeIndexArray.Clear();



                if (j == 3)
                {

                    float subArea_tobeThin = 0;

                    for (int spec_i = 1; spec_i <= siteptr.Num_Species; spec_i++)
                    {
                        int local_id = (spec_i - 1) * 4 + j;
                        
                        int age_beg = quaterPercent[local_id    ];
                        int age_end = quaterPercent[local_id + 1];


                        for (int age_i = age_beg; age_i <= age_end; age_i++)
                        {
                            double term1 = Math.Pow((GetGrowthRates(spec_i, age_i, l.LtID) / 25.4), 1.605);
                            float  term2 = siteptr.specAtt(spec_i).MaxAreaOfSTDTree;
                            float  term3 = thinning_percentatge[siteptr.specAtt(spec_i).Shade_Tolerance - 1];
                            uint   term4 = siteptr.SpecieIndex(spec_i).getTreeNum(age_i, spec_i);


                            double temp = term1 * term2;
                            subArea_tobeThin += (float)(temp * term3 * term4);


                            AreaList.Add((float)temp);

                            SpecIndexArray.Add(spec_i);

                            AgeIndexArray.Add(age_i);
                        }

                    }


                    ListbubbleSort();


                    if (subArea_tobeThin >= Area_tobeThin)
                    {
                        for (int i = 0; i < AreaList.Count; i++)
                        {
                            specie local_specie = siteptr.SpecieIndex(SpecIndexArray[i]);

                            uint local_tree_num = local_specie.getTreeNum(AgeIndexArray[i], SpecIndexArray[i]);
                            
                            float local_thin_percent = thinning_percentatge[siteptr.specAtt(SpecIndexArray[i]).Shade_Tolerance - 1];


                            if (local_tree_num > 0)
                            {
                                float tempAreaInvolveTreeNum = AreaList[i] * local_tree_num * local_thin_percent / 8;

                                if (tempAreaInvolveTreeNum > Area_tobeThin)
                                {
                                    double tmp_val = Area_tobeThin / (AreaList[i]);

                                    Debug.Assert(tmp_val <= int.MaxValue && tmp_val >= int.MinValue);
                                    
                                    int treesToremove = (int)tmp_val;

                                    int treesLeft = (int)local_tree_num - treesToremove;

                                    local_specie.setTreeNum(AgeIndexArray[i], SpecIndexArray[i], Math.Max(0, treesLeft));

                                    //if (treesToremove > 0)
                                    //{
                                    //    Console.WriteLine("Remain - {0}, Initial - {1}", Math.Max(0, treesLeft), local_tree_num);
                                    //}

                                    Area_tobeThin -= tempAreaInvolveTreeNum;

                                    quaterPercent = null;

                                    return;

                                }
                                else
                                {
                                    float float_treeleft = local_tree_num * (1 - local_thin_percent / 8);
                                    
                                    Debug.Assert(float_treeleft <= int.MaxValue && float_treeleft >= int.MinValue);
                                    
                                    int treesLeft = (int)float_treeleft;

                                    local_specie.setTreeNum(AgeIndexArray[i], SpecIndexArray[i], Math.Max(0, treesLeft));

                                    //if (treesLeft != local_tree_num)
                                    //{
                                    //    Console.WriteLine("Remain - {0}, Initial - {1}", Math.Max(0, treesLeft), local_tree_num);
                                    //}

                                    Area_tobeThin -= tempAreaInvolveTreeNum;

                                }

                            }

                        }

                    }

                    else
                    {

                        for (int i = 0; i < AreaList.Count; i++)
                        {
                            specie local_specie = siteptr.SpecieIndex(SpecIndexArray[i]);

                            uint local_tree_num = local_specie.getTreeNum(AgeIndexArray[i], SpecIndexArray[i]);
                            
                            float local_thin_percent = thinning_percentatge[siteptr.specAtt(SpecIndexArray[i]).Shade_Tolerance - 1];


                            if (local_tree_num > 0)
                            {
                                float tempAreaInvolveTreeNum = AreaList[i] * local_tree_num * local_thin_percent / 8;
                                {
                                    float float_treeleft = local_tree_num * (1 - local_thin_percent / 8);
                                    
                                    Debug.Assert(float_treeleft <= int.MaxValue && float_treeleft >= 0);
                                    
                                    int treesLeft = (int)float_treeleft;

                                    local_specie.setTreeNum(AgeIndexArray[i], SpecIndexArray[i], Math.Max(0, treesLeft));   

                                    Area_tobeThin -= tempAreaInvolveTreeNum;

                                }

                            }

                        }

                    }

                }

            }


            quaterPercent = null;

        }







        public void SeedGermination(site siteptr, landunit l, int RDFlag)
        {            
            double RDTotal;
            double seedlingTemp;

            long temp;
            int cellsize_square = cellSize * cellSize;

            uint site_num = siteptr.Num_Species;

            double[] IndexRD     = new double[site_num];
            int[]    SppPresence = new int   [site_num];
            long[]   Seedling    = new long  [site_num];

            float[] SppRD    = new float[6];
            int[]   SppShade = new int  [6];
            
            
            if (RDFlag == 0 || RDFlag == 1)
            {
                RDTotal = 0.0;
                //Console.WriteLine("site_num = {0}", site_num);
                for (int i = 1; i <= site_num; i++)
                {
                    double local_term1 = Math.Pow((GetGrowthRates(i, 1, l.LtID) / 25.4), 1.605);
                    float  local_term2 = siteptr.specAtt(i).MaxAreaOfSTDTree;
                    uint   local_term3 = siteptr.SpecieIndex(i).AvailableSeed;

                    IndexRD[i - 1] = local_term1 * local_term2 * local_term3 / cellsize_square;

                    //Console.WriteLine("{0:N6}, {1:N6}, {2}", local_term1, local_term2, local_term3);
                    //if (local_term3 != 0)
                    //Console.WriteLine("{0}, {1}", i, local_term3);

                    RDTotal += IndexRD[i - 1];

                }                

                if (RDTotal >= 0.0)
                {
                    //Console.WriteLine("RDFlag == 1");

                    if (RDTotal <= l.MaxRD - siteptr.RD)
                    {
                        for (int i = 1; i <= site_num; i++)
                        {
                            speciesattr local_speciesattr = siteptr.specAtt(i);
                            specie      local_specie      = siteptr.SpecieIndex(i);

                            if (local_speciesattr.Shade_Tolerance <= 4 || (local_speciesattr.Shade_Tolerance == 5 && siteptr.MaxAge >= l.MinShade))
                            {
                                temp = local_specie.TreesFromVeg * local_specie.VegPropagules;

                                local_specie.TreesFromVeg = 0;

                                float float_val = local_specie.AvailableSeed * l.probRepro(i);

                                Debug.Assert(float_val <= int.MaxValue && float_val >= int.MinValue);

                                Seedling[i - 1] = (long)float_val + temp;

                                //if (Seedling[i - 1] != 0)
                                //Console.WriteLine("first {0}, {1:N6}, {2}", local_specie.AvailableSeed, l.probRepro(i), temp);
                            }

                        }

                    }

                    else
                    {

                        for (int i = 1; i <= site_num; i++)
                        {
                            speciesattr local_speciesattr = siteptr.specAtt(i);
                            specie      local_specie      = siteptr.SpecieIndex(i);

                            if (local_speciesattr.Shade_Tolerance <= 4 || (local_speciesattr.Shade_Tolerance == 5 && siteptr.MaxAge >= l.MinShade))
                            {
                                temp = local_specie.TreesFromVeg * local_specie.VegPropagules;

                                local_specie.TreesFromVeg = 0;

                                seedlingTemp = IndexRD[i - 1] * (l.MaxRD - siteptr.RD) / RDTotal * cellsize_square / (Math.Pow((GetGrowthRates(i, 1, l.LtID) / 25.4), 1.605) * local_speciesattr.MaxAreaOfSTDTree);

                                double double_val = seedlingTemp * l.probRepro(i);

                                Debug.Assert(double_val <= int.MaxValue && double_val >= int.MinValue);

                                Seedling[i - 1] = (long)double_val + temp;
                            }

                        }

                    }

                }

                else
                {

                    IndexRD     = null;
                    Seedling    = null;
                    SppRD       = null;
                    SppShade    = null;
                    SppPresence = null;

                    return;

                }

            }

            else if (RDFlag == 2)

            {

                RDTotal = 0.0;

                for (int i = 1; i <= site_num; i++)
                {
                    //changed by wenjuan//if(siteptr.specAtt(i).Shade_Tolerance==siteptr.HighestShadeTolerance&&siteptr.MaxAge>=l.minShade)
                    if (siteptr.specAtt(i).Shade_Tolerance >= siteptr.HighestShadeTolerance)
                    {
                        double local_term1 = Math.Pow((GetGrowthRates(i, 1, l.LtID) / 25.4), 1.605);
                        float  local_term2 = siteptr.specAtt(i).MaxAreaOfSTDTree;
                        uint   local_term3 = siteptr.SpecieIndex(i).AvailableSeed;


                        IndexRD[i - 1] = local_term1 * local_term2 * local_term3 / cellsize_square;

                        RDTotal += IndexRD[i - 1];

                    }
                    else
                    {
                        Seedling[i - 1] = 0;
                    }

                }

                if (RDTotal >= 0.0)
                {
                    if (RDTotal <= l.MaxRD - siteptr.RD)
                    {
                        for (int i = 1; i <= site_num; i++)
                        {
                            speciesattr local_speciesattr = siteptr.specAtt(i);
                            specie      local_specie      = siteptr.SpecieIndex(i);

                            if (local_speciesattr.Shade_Tolerance >= siteptr.HighestShadeTolerance)
                            {
                                temp = local_specie.TreesFromVeg * local_specie.VegPropagules;

                                local_specie.TreesFromVeg = 0;

                                float float_val = local_specie.AvailableSeed * l.probRepro(i);
                                
                                Debug.Assert(float_val <= int.MaxValue && float_val >= int.MinValue);
                                
                                Seedling[i - 1] = (long)float_val + temp;
                            }

                        }

                    }
                    else
                    {
                        for (int i = 1; i <= site_num; i++)
                        {
                            speciesattr local_speciesattr = siteptr.specAtt(i);
                            specie      local_specie      = siteptr.SpecieIndex(i);

                            if (local_speciesattr.Shade_Tolerance >= siteptr.HighestShadeTolerance)
                            {
                                temp = local_specie.TreesFromVeg * local_specie.VegPropagules;

                                local_specie.TreesFromVeg = 0;

                                seedlingTemp = IndexRD[i - 1] * (l.MaxRD - siteptr.RD) / RDTotal * cellsize_square / (Math.Pow((GetGrowthRates(i, 1, l.LtID) / 25.4), 1.605) * local_speciesattr.MaxAreaOfSTDTree);

                                double double_val = seedlingTemp * l.probRepro(i);
                                if (!(double_val <= int.MaxValue && double_val >= int.MinValue))
                                    Console.WriteLine("{0} {1} {2} {3}", seedlingTemp, l.probRepro(i), RDTotal==0.0, RDTotal>0.0);
                                Debug.Assert(double_val <= int.MaxValue && double_val >= int.MinValue);
                                Seedling[i - 1] = (long)double_val + temp;
                            }

                        }

                    }

                }

                else
                {

                    IndexRD     = null;
                    Seedling    = null;
                    SppRD       = null;
                    SppShade    = null;
                    SppPresence = null;

                    return;

                }
            }
            else if (RDFlag == 3)
            {
                RDTotal = 0.0;

                for (int i = 1; i <= site_num; i++)
                {
                    //changed by wenjuan//if(siteptr.specAtt(i).Shade_Tolerance==siteptr.HighestShadeTolerance&&siteptr.MaxAge>=l.minShade)
                    if (siteptr.specAtt(i).Shade_Tolerance == maxShadeTolerance && siteptr.MaxAge >= l.MinShade)
                    {
                        double local_term1 = Math.Pow((GetGrowthRates(i, 1, l.LtID) / 25.4), 1.605);
                        float  local_term2 = siteptr.specAtt(i).MaxAreaOfSTDTree;
                        uint   local_term3 = siteptr.SpecieIndex(i).AvailableSeed;

                        IndexRD[i - 1] = local_term1 * local_term2 * local_term3 / cellsize_square;

                        RDTotal += IndexRD[i - 1];
                    }
                    else
                    {
                        Seedling[i - 1] = 0;
                    }

                }

                if (RDTotal >= 0.0)
                {
                    if (RDTotal <= l.MaxRD - siteptr.RD)
                    {
                        for (int i = 1; i <= site_num; i++)
                        {
                            speciesattr local_speciesattr = siteptr.specAtt(i);
                            specie      local_specie      = siteptr.SpecieIndex(i);

                            if (local_speciesattr.Shade_Tolerance == maxShadeTolerance && siteptr.MaxAge >= l.MinShade)
                            {
                                temp = local_specie.TreesFromVeg * local_specie.VegPropagules;

                                local_specie.TreesFromVeg = 0;

                                float float_val = local_specie.AvailableSeed * l.probRepro(i);
                                
                                Debug.Assert(float_val <= int.MaxValue && float_val >= int.MinValue);
                                
                                Seedling[i - 1] = (long)float_val + temp;
                            }

                        }

                    }
                    else
                    {
                        for (int i = 1; i <= site_num; i++)
                        {
                            speciesattr local_speciesattr = siteptr.specAtt(i);
                            specie      local_specie      = siteptr.SpecieIndex(i);

                            if (local_speciesattr.Shade_Tolerance == maxShadeTolerance && siteptr.MaxAge >= l.MinShade)
                            {
                                temp = local_specie.TreesFromVeg * local_specie.VegPropagules;

                                local_specie.TreesFromVeg = 0;

                                seedlingTemp = IndexRD[i - 1] * (l.MaxRD - siteptr.RD) / RDTotal * cellsize_square / (Math.Pow((GetGrowthRates(i, 1, l.LtID) / 25.4), 1.605) * local_speciesattr.MaxAreaOfSTDTree);

                                double double_val = seedlingTemp * l.probRepro(i);

                                Debug.Assert(double_val <= int.MaxValue && double_val >= int.MinValue);

                                Seedling[i - 1] = (long)double_val + temp;
                            }

                        }

                    }

                }

                else
                {
                    IndexRD     = null;
                    Seedling    = null;
                    SppRD       = null;
                    SppShade    = null;
                    SppPresence = null;

                    return;
                }

            }
            else
            {
                for (int i = 1; i <= site_num; i++)
                {
                    Seedling[i - 1] = 0;
                }

            }


            //Console.WriteLine("c# seedling");

            for (int i = 1; i <= site_num; i++)
            {
                if (Seedling[i - 1] > 0)
                {
                    siteptr.SpecieIndex(i).setTreeNum(1, i, (int)Seedling[i - 1]);
                    //Console.Write("{0} ", (int)Seedling[i - 1]);
                }
                else
                    siteptr.SpecieIndex(i).setTreeNum(1, i, 0);
            }

            //Console.WriteLine("RDFlag = {0}", RDFlag);

            IndexRD     = null;
            Seedling    = null;
            SppRD       = null;
            SppShade    = null;
            SppPresence = null;

        }






        //calculate the RD for a given site, No Return Value
        public void GetRDofSite(uint Row, uint Col)
        {        
            site siteptr = this[Row, Col];

            siteptr.HighestShadeTolerance = 0;

            uint numSpec = siteptr.Num_Species;

            siteptr.RD = 0;

            landunit l = locateLanduPt(Row, Col);

            int cellsize_square = cellSize * cellSize;


            for (int i = 1; i <= numSpec; i++)
            {
                speciesattr local_speciesattr = siteptr.specAtt(i);

                //Console.WriteLine("Species {0} - MaxAreaOfSTDTree {1}", i, local_speciesattr.MaxAreaOfSTDTree);

                int site_spec_i_type = local_speciesattr.SpType;

                if (site_spec_i_type >= 0)
                {
                    specie local_specie = siteptr.SpecieIndex(i);
                    
                    int max_count = local_speciesattr.Longevity / timeStep;

                    for (int j = 1; j <= max_count; j++)
                    {
                        double tmp_term1 = Math.Pow((GetGrowthRates(i, j, l.LtID) / 25.4), 1.605);
                        float  tmp_term2 = local_speciesattr.MaxAreaOfSTDTree;
                        uint   tmp_term3 = local_specie.getTreeNum(j, i);

                        double tmp = tmp_term1 * tmp_term2 * tmp_term3 / cellsize_square;
                        siteptr.RD += (float)tmp;

                        if (tmp_term3 > 0)
                        {
                            int specAtt_i_shadetolerance = local_speciesattr.Shade_Tolerance;

                            if (siteptr.HighestShadeTolerance < specAtt_i_shadetolerance)
                                siteptr.HighestShadeTolerance = specAtt_i_shadetolerance;
                        }

                    }

                }//end if
 
            }//end for

        }





        //calculate the MaxAge for a given site
        public void MaxAgeofSite(site siteptr)
        {            
            siteptr.MaxAge = 0;

            uint numSpec = siteptr.Num_Species;

            for (int i = 1; i <= numSpec; i++)
            {
            	int max_count = siteptr.specAtt(i).Longevity / timeStep;

            	specie local_specie = siteptr.SpecieIndex(i);

                for (int j = 1; j <= max_count; j++)
                {
                    if (local_specie.getTreeNum(j, i) > 0)
                    {
                        if (siteptr.MaxAge < j)
                            siteptr.MaxAge = j;
                    }

                }

            }

        }




        public void GetMatureTree()
        {
            for (uint i = 1; i <= rows; i++)
            {
                for (uint j = 1; j <= columns; j++)
                {
                    site siteptr = this[i, j];

                    uint siteptr_num = siteptr.Num_Species;

                    for (int k = 1; k <= siteptr_num; k++)
                    {
                        int m_begin = siteptr.specAtt(k).Maturity / timeStep;
                        int m_limit = siteptr.specAtt(k).Longevity / timeStep;

                        specie local_specie = siteptr.SpecieIndex(k);

                        uint treenum = 0;

                        for (int m = m_begin; m <= m_limit; m++)
                        {
                            treenum += local_specie.getTreeNum(m, k);
                        }

                        local_specie.MatureTree = treenum;
                    }
                }
            }

        }





        //According to total seed of each species, calculate the available seed number would disperse into other sites
        public void GetSeedNumberOnSite(uint Row, uint Col)
        {          
            site siteptr = this[Row, Col];
            
            landunit l = locateLanduPt(Row, Col);

            
            uint const_cellsqr = (uint)(cellSize * cellSize * 1000);

            uint local_site_num = siteptr.Num_Species;

            for (int k = 1; k <= local_site_num; k++)
            {
                speciesattr local_speciesattr = siteptr.specAtt(k);
                specie      local_specie      = siteptr.SpecieIndex(k);
                
                local_specie.AvailableSeed = 0;

                int totalseed_m_timestep = local_speciesattr.TotalSeed * timeStep;

                
                if (local_speciesattr.SpType < 0)
                {
                    local_specie.AvailableSeed = (uint)totalseed_m_timestep;
                }
                else
                {
                    double double_val;

                    if (locateLanduPt(Row, Col).probRepro(k) > 0 && local_speciesattr.Max_seeding_Dis < 0)
                    {
                        int i_age_begin = local_speciesattr.Maturity / timeStep;
                    	int i_age_end   = local_speciesattr.Longevity / timeStep;

                        for (int i_age = i_age_begin; i_age <= i_age_end; i_age++)
                        {
                            uint local_treenum = local_specie.getTreeNum(i_age, k);

                            if (local_treenum > 0)
                            {
                                double loc_term = Math.Pow(GetGrowthRates(k, i_age, l.LtID) / 25.4, 1.605);

                                 //wenjuan changed on mar 30 2011
                                double_val = loc_term * local_treenum * totalseed_m_timestep;

                                Debug.Assert(double_val <= UINT_MAX && double_val > 0);

                                local_specie.AvailableSeed += (uint)double_val;
                                
                            }

                        }

                    }



                    if (locateLanduPt(Row, Col).probRepro(k) > 0 && local_speciesattr.Max_seeding_Dis > 0)
                    {
                        int maxD_d_cellsize = local_speciesattr.Max_seeding_Dis / cellSize;

                        for (int i = (int)Row - maxD_d_cellsize; i <= Row + maxD_d_cellsize; i++)
                        {
                            for (int j = (int)Col - maxD_d_cellsize; j <= Col + maxD_d_cellsize; j++)
                            {
                                if (i >= 1 && i <= rows && j >= 1 && j <= columns && locateLanduPt((uint)i, (uint)j) != null && locateLanduPt((uint)i, (uint)j).Active)
                                {
                                    int TempDist = (int)Math.Max(Math.Abs(i - Row), Math.Abs(j - Col));

                                    site local_site = this[(uint)i, (uint)j];

                                    //double_val = GetSeedRain(k, TempDist) * local_site.SpecieIndex(k).MatureTree * local_site.specAtt(k).TotalSeed * timeStep;
                                    float seed_rain = GetSeedRain(k, TempDist);
                                    uint mature_tree = local_site.SpecieIndex(k).MatureTree;
                                    int local_tseed = local_site.specAtt(k).TotalSeed;

                                    double_val = seed_rain * mature_tree * local_tseed * timeStep;

                                    Debug.Assert(double_val <= UINT_MAX && double_val >= 0);

                                    local_specie.AvailableSeed += (uint)double_val;
                                    
                                }

                            }

                            if (local_specie.AvailableSeed > const_cellsqr)
                            {
                                local_specie.AvailableSeed = const_cellsqr;

                                break;
                            }

                        }

                        float float_rand = system1.frand1();

                        double_val = local_specie.AvailableSeed * (0.95 + float_rand * 0.1);

                        Debug.Assert(double_val <= UINT_MAX && double_val >= 0);

                        local_specie.AvailableSeed = (uint)double_val;
                        
                    }

                }//end else

            }// end for

        }



        public void NaturalMortality_killbytargetRD(site siteptr, uint Row, uint Col, double targetRD)
        {
            for (int i = 1; i <= siteptr.Num_Species; i++)
            {
                if (siteptr.specAtt(i).SpType >= 0)
                {
                    specie local_specie = siteptr.SpecieIndex(i);

                    for (int j = siteptr.specAtt(i).Longevity / timeStep; j >= 1; j--)
                    {
                        if (local_specie.getTreeNum(j, i) > 0)
                        {
                            local_specie.setTreeNum(j, i, 0);

                            GetRDofSite(Row, Col);

                            if (siteptr.RD <= targetRD)                            
                                return;                            
                        }
                    }
                }
            }
        }



        public void NaturalMortality(site siteptr, uint Row, uint Col, int StartAge)
        {
            landunit l = locateLanduPt(Row, Col);

            int cellsize_square = cellSize * cellSize;

            double DQ_const = 3.1415926 / (4 * 0.0002471 * cellsize_square * 30.48 * 30.48);

            uint site_species_num = siteptr.Num_Species; //number of species

            //Console.WriteLine("StartAge = {0}", StartAge);
            //kill all tree, else kill youngest tree
            if (StartAge == 0)
            {
                //Console.WriteLine("MortalityFlag = {0}", MortalityFlag);

                if (MortalityFlag == 0)
                {
                    double tmpDQ = 0;

                    for (int i = 1; i <= site_species_num; i++)
                    {
                        int site_spec_i_type = siteptr.specAtt(i).SpType;
                        int max_count = siteptr.specAtt(i).Longevity / timeStep;

                        if (site_spec_i_type >= 0)
                        {
                            specie local_specie = siteptr.SpecieIndex(i);

                            for (int j = 1; j <= max_count; j++)
                            {
                                float growthrate = GetGrowthRates(i, j, l.LtID);

                                uint spec_ij_treenum = local_specie.getTreeNum(j, i);

                                //Wenjuan Suggested on Nov 16 2010
                                tmpDQ += growthrate * growthrate * DQ_const * spec_ij_treenum;

                                //if (spec_ij_treenum != 0)
                                //    Console.Write("{0}, {1:N2} ", spec_ij_treenum, growthrate);
                            }
                        }
                    }

                    //Console.Write("{0:N2} ", d_tmpDQ);
                    //Console.ReadLine();                    


                    for (int i = 1; i <= site_species_num; i++)
                    {
                        int site_spec_i_type = siteptr.specAtt(i).SpType;
                        int max_count = siteptr.specAtt(i).Longevity / timeStep;

                        specie local_specie = siteptr.SpecieIndex(i);

                        if (site_spec_i_type >= 0)
                        {
                            for (int j = 1; j <= max_count; j++)
                            {
                                float growthrate = GetGrowthRates(i, j, l.LtID);
                                uint spec_ij_treenum = local_specie.getTreeNum(j, i);

                                double TmpMortality = timeStep / 10 / (1.0 + Math.Exp(3.25309 - 0.00072647 * tmpDQ + 0.01668809 * growthrate / 2.54));
                                TmpMortality = (1.0f < TmpMortality ? 1.0f : TmpMortality);

                                double DeadTree = spec_ij_treenum * TmpMortality;

                                Debug.Assert(DeadTree <= UINT_MAX && DeadTree >= 0);

                                uint DeadTreeInt = (uint)DeadTree;

                                //if (DeadTree - DeadTreeInt >= 0.0001)
                                if (DeadTree > DeadTreeInt)
                                {
                                    float rand = system1.frand1();

                                    if (rand < 0.1)
                                        DeadTreeInt++;
                                }

                                local_specie.setTreeNum(j, i, (int)Math.Max(0, spec_ij_treenum - DeadTreeInt));

                                tmpDQ -= (float)(growthrate * growthrate * DQ_const * DeadTree);
                            }

                        }
                        else
                        {
                            for (int j = 1; j <= max_count; j++)
                                local_specie.setTreeNum(j, i, cellsize_square);
                        }

                    }

                }
                else
                {

                    for (int i = 1; i <= site_species_num; i++)
                    {
                        int site_spec_i_type = siteptr.specAtt(i).SpType;
                        int max_count = siteptr.specAtt(i).Longevity / timeStep;

                        specie local_specie = siteptr.SpecieIndex(i);

                        if (site_spec_i_type >= 0)
                        {
                            for (int j = 1; j <= max_count; j++)
                            {
                                uint spec_ij_treenum = local_specie.getTreeNum(j, i);

                                double DeadTree = spec_ij_treenum * GetMortalityRates(i, j, l.LtID);

                                Debug.Assert(DeadTree <= UINT_MAX && DeadTree >= 0);

                                uint DeadTreeInt = (uint)DeadTree;

                                //if (DeadTree - DeadTreeInt >= 0.0001)
                                if (DeadTree > DeadTreeInt)
                                {
                                    float rand = system1.frand1();

                                    if (rand < 0.1)
                                        DeadTreeInt++;
                                }


                                local_specie.setTreeNum(j, i, (int)Math.Max(0, spec_ij_treenum - DeadTreeInt));
                            }

                        }
                        else
                        {
                            for (int j = 1; j <= max_count; j++)
                                local_specie.setTreeNum(j, i, cellsize_square);
                        }

                    }

                }

            }
            else //kill youngest tree
            {
                if (MortalityFlag == 0)
                {
                    double tmpDQ = 0;

                    for (int i = 1; i <= site_species_num; i++)
                    {
                        int site_spec_i_type = siteptr.specAtt(i).SpType;
                        int max_count = siteptr.specAtt(i).Longevity / timeStep;

                        if (site_spec_i_type >= 0)
                        {
                            specie local_specie = siteptr.SpecieIndex(i);

                            for (int j = 1; j <= max_count; j++)
                            {
                                float growthrate = GetGrowthRates(i, j, l.LtID);

                                uint spec_ij_treenum = local_specie.getTreeNum(j, i);

                                //Wenjuan Suggested on Nov 16 2010
                                tmpDQ += growthrate * growthrate * DQ_const * spec_ij_treenum;
                            }
                        }
                    }


                    for (int i = 1; i <= site_species_num; i++)
                    {
                        int site_spec_i_type = siteptr.specAtt(i).SpType;
                        int max_count = siteptr.specAtt(i).Longevity / timeStep;

                        specie local_specie = siteptr.SpecieIndex(i);

                        if (site_spec_i_type >= 0)
                        {
                            for (int j = 1; j <= StartAge; j++)
                            {
                                float growthrate = GetGrowthRates(i, j, l.LtID);

                                double TmpMortality = timeStep / 10 / (1.0 + Math.Exp(3.25309 - 0.00072647 * tmpDQ + 0.01668809 * growthrate / 2.54));
                                TmpMortality = (1.0f < TmpMortality ? 1.0f : TmpMortality);

                                uint spec_ij_treenum = local_specie.getTreeNum(j, i);

                                double DeadTree = spec_ij_treenum * TmpMortality;

                                Debug.Assert(DeadTree <= UINT_MAX && DeadTree >= 0);

                                uint DeadTreeInt = (uint)DeadTree;

                                //if (DeadTree - DeadTreeInt >= 0.0001)
                                if (DeadTree > DeadTreeInt)
                                {
                                    float rand = system1.frand1();

                                    if (rand < 0.1)
                                        DeadTreeInt++;
                                }

                                local_specie.setTreeNum(j, i, (int)Math.Max(0, spec_ij_treenum - DeadTreeInt));

                                tmpDQ -= (float)(growthrate * growthrate * DQ_const * DeadTree);
                            }

                        }
                        else
                        {
                            for (int j = 1; j <= StartAge; j++)
                                local_specie.setTreeNum(j, i, cellsize_square);
                        }

                    }


                }
                else
                {

                    for (int i = 1; i <= site_species_num; i++)
                    {
                        int site_spec_i_type = siteptr.specAtt(i).SpType;
                        int max_count = siteptr.specAtt(i).Longevity / timeStep;

                        specie local_specie = siteptr.SpecieIndex(i);

                        if (site_spec_i_type >= 0)
                        {
                            for (int j = 1; j <= StartAge; j++)
                            {
                                uint spec_ij_treenum = local_specie.getTreeNum(j, i);

                                double DeadTree = spec_ij_treenum * GetMortalityRates(i, j, l.LtID);

                                Debug.Assert(DeadTree <= UINT_MAX && DeadTree >= 0);

                                uint DeadTreeInt = (uint)DeadTree;

                                //if (DeadTree - DeadTreeInt >= 0.0001)
                                if (DeadTree > DeadTreeInt)
                                {
                                    float rand = system1.frand1();

                                    if (rand < 0.1)
                                        DeadTreeInt++;
                                }

                                local_specie.setTreeNum(j, i, (int)Math.Max(0, spec_ij_treenum - DeadTreeInt));

                            }

                        }
                        else
                        {
                            for (int j = 1; j <= StartAge; j++)
                                local_specie.setTreeNum(j, i, cellsize_square);
                        }

                    }

                }

            }

        }





        public void SetOutputGeneralFlagArray(uint i, int j, int value)
        {
            if (i > (specNum + 1) || j > NumTypes70Output)
                throw new Exception("Out bound in Flag Array\n");
            
            OutputGeneralFlagArray[i * NumTypes70Output + j] = value;
        }


        public int GetOutputGeneralFlagArray(uint i, int j)
        {
            if (i > (specNum + 1) || j > NumTypes70Output)
                throw new Exception("Out bound in Flag Array\n");
            else
                return OutputGeneralFlagArray[i * NumTypes70Output + j];
        }



        public void SetOutputAgerangeFlagArray(uint i, int j, int value)
        {
            if (i > (specNum + 1) || j > NumTypes70Output)
                throw new Exception("Out bound in Flag Array\n");

            OutputAgerangeFlagArray[i * NumTypes70Output + j] = value;
        }

        public int GetOutputAgerangeFlagArray(uint i, int j)
        {
            if (i > (specNum + 1) || j > NumTypes70Output)
                throw new Exception("Out bound in Flag Array\n");

            return OutputAgerangeFlagArray[i * NumTypes70Output + j];
        }



        public void SetSpeciesAgerangeArray(int specindex, int count, int value1, int value2)
        {
            if (specindex > specNum || count > 40)
                throw new Exception("Out bound in species age range\n");


            int local_index = specindex * 500 / timeStep + count * 2;

            SpeciesAgerangeArray[local_index - 1] = value1;
            SpeciesAgerangeArray[local_index    ] = value2;
        }



        public void GetSpeciesAgerangeArray(int specindex, int count, ref int value1, ref int value2)
        {
            if (specindex > specNum || count > 40)
                throw new Exception("Out bound in species age range\n");

            int local_index = specindex * 500 / timeStep + count * 2;

            value1 = SpeciesAgerangeArray[local_index - 1];
            value2 = SpeciesAgerangeArray[local_index];
        }



        public int GetAgerangeCount(int specindex)
        {
            return SpeciesAgerangeArray[specindex * 500 / timeStep];
        }

        public void SetAgerangeCount(int specindex, int count)
        {
            SpeciesAgerangeArray[specindex * 500 / timeStep] = count;
        }

        public void SetAgeDistStat_YearVal(int specindex, int count, int value1)

        {
            if (specindex > specNum || count > 40)
                throw new Exception("Out bound in species age range\n");

            AgeDistStat_Year[specindex * 500 / timeStep + count] = value1;
        }



        public void GetAgeDistStat_YearVal(int specindex, int count, ref int value1)
        {
            if (specindex > specNum || count > 40)
                throw new Exception("Out bound in species age range\n");

            value1 = AgeDistStat_Year[specindex * 500 / timeStep + count];
        }



        public int GetAgeDistStat_YearCount(int specindex)
        {
            return AgeDistStat_Year[specindex * 500 / timeStep];
        }



        public void SetAgeDistStat_YearValCount(int specindex, int count)
        {
            AgeDistStat_Year[specindex * 500 / timeStep] = count;
        }



        public void SetAgeDistStat_AgeRangeVal(int specindex, int count, int value1, int value2)
        {
            if (specindex > specNum || count > 40)
                throw new Exception("Out bound in species age range\n");


            int local_index = specindex * 500 / timeStep + count * 2;

            AgeDistStat_AgeRange[local_index - 1] = value1;
            AgeDistStat_AgeRange[local_index    ] = value2;
        }


        public void GetAgeDistStat_AgeRangeVal(int specindex, int count, ref int value1, ref int value2)
        {
            if (specindex > specNum || count > 40)
                throw new Exception("Out bound in species age range\n");

            int local_index = specindex * 500 / timeStep + count * 2;

            value1 = AgeDistStat_AgeRange[local_index - 1];
            value2 = AgeDistStat_AgeRange[local_index];
        }



        public int GetAgeDistStat_AgeRangeCount(int specindex)
        {
            return AgeDistStat_AgeRange[specindex * 500 / timeStep];
        }



        public void SetAgeDistStat_AgeRangeCount(int specindex, int count)
        {
            AgeDistStat_AgeRange[specindex * 500 / timeStep] = count;
        }

        



        public void Read70OutputOption(string FileName)
        {
            OutputOptionParser OutputOption_parser = new OutputOptionParser();
            Landis.Data.Load<OutputOption>(FileName, OutputOption_parser);
        }





        public float GetSeedRain(int spec, int Distance)
        {
            int local_int = MaxDistofAllSpec / cellSize + 2;

            int id = (spec - 1) * local_int + Distance;

            if (id >= specNum * local_int)
                return 0;
            else
                return SeedRain[id];
        }




        public float GetGrowthRates(int spec, int year, int landtype_index)
        {
            int local_const = 320 / timeStep + 1;
            int index = (spec - 1) * local_const + year - 1;

            Debug.Assert(spec >= 1 && spec <= specNum);
            Debug.Assert(spec * year < specNum * local_const);
            
            
            if (GrowthRates_file.Count == 0)
                return GrowthRates[index];
            else
            {
                Debug.Assert(landtype_index < GrowthRates_file.Count);

                return GrowthRates_file[landtype_index][index];
            }
        }



        public float GetMortalityRates(int spec, int year, int landtype_index)
        {
            int local_const = 320 / timeStep + 1;
            int index = (spec - 1) * local_const + year - 1;

            if (spec < 1 && spec > specNum && spec * year >= specNum * local_const)
                throw new Exception("Out bound in GetMortalityRates\n");

            if (MortalityRates_file.Count == 0)
                return MortalityRates[index];
            else
            {
                if (landtype_index < MortalityRates_file.Count)
                    return MortalityRates_file[landtype_index][index];
                else
                    throw new Exception("Out bound in MortalityRates\n");
            }
        }



        public float GetVolume(int spec, int year, int landtype_index)
        {
            int local_const = 320 / timeStep + 1;
            int index = (spec - 1) * local_const + year - 1;


            if (spec < 1 && spec > specNum && spec * year >= specNum * local_const)
                throw new Exception("Out bound in GetVolume\n");


            if (Volume_file.Count == 0)
                return Volume[index];
            else if (landtype_index < Volume_file.Count)
                return Volume_file[landtype_index][index];
            else 
                throw new Exception("Out bound in Volume\n");
        }



        public void SetSeedRain(int spec, int Distance, float value)
        {
            int local_int = MaxDistofAllSpec / cellSize + 2;

            int id = (spec - 1) * local_int + Distance;


            if (id >= specNum * local_int)
                throw new Exception("Out bound in SetSeedRain\n");

            SeedRain[id] = value;
        }




        public void SetGrowthRates(int flag, int spec, int year, float value, int index_landtype)
        {
            int local_const = 320 / timeStep + 1;
            int index = (spec - 1) * local_const + year - 1;


            if (spec < 1 && spec > specNum && spec * year >= specNum * local_const)
                throw new Exception("Out bound in SetGrowthRates\n");

            if (flag == 0)
            {
                GrowthRates[index] = value;
            }
            else
            {
                if (index_landtype == GrowthRates_file.Count)
                {
                    float[] temp = new float[specNum * local_const];

                    GrowthRates_file.Add(temp);
                }

                GrowthRates_file[index_landtype][index] = value;
            }
        }




        public void SetMortalityRates(int flag, int spec, int year, float value, int index_landtype)
        {
            int local_const = 320 / timeStep + 1;
            int index = (spec - 1) * local_const + year - 1;


            if (spec < 1 && spec > specNum && spec * year >= specNum * local_const)
                throw new Exception("Out bound in SetMortalityRates\n");

            if (flag == 0)
            {
                MortalityRates[index] = value;
            }
            else
            {
                if (index_landtype == MortalityRates_file.Count)
                {
                    float[] temp = new float[specNum * local_const];
                    MortalityRates_file.Add(temp);
                }

                MortalityRates_file[index_landtype][index] = value;
            }
        }




        public void SetVolume(int growthrate_flag, int spec, int year, float value, int index_landtype)
        {
            int local_const = 320 / timeStep + 1;
            int index = (spec - 1) * local_const + year - 1;


            if (spec < 1 && spec > specNum && spec * year >= specNum * local_const)
                throw new Exception("Out bound in Volume\n");
                

            if (growthrate_flag == 0)
            {
                Volume[index] = value;
            }
            else
            {
                if (index_landtype == Volume_file.Count)
                {
                    float[] temp = new float[specNum * local_const];
                    Volume_file.Add(temp);
                }

                Volume_file[(index_landtype)][index] = value;
            }
        }




        //Get seed dispersal probability on a site and save in a matrix consists of distance and probability
        public void GetSeedDispersalProbability(string fileSeedDispersal, int seedflag)        
        {
            float temp;

            site sitetmp = this[1, 1];

            SeedRainFlag = seedflag;


            if (SeedRainFlag == 0)
            {
                for (int i = 1; i <= specNum; i++)  //Wen's code
                {
                    int local_maxD_d_cellsize = sitetmp.specAtt(i).Max_seeding_Dis / cellSize;

                    float[] Probability = new float[local_maxD_d_cellsize + 1];

                    float probSum = 0.0f;

                    double tmp_inverse = -1.0 / local_maxD_d_cellsize;

                    for (int j = 0; j <= local_maxD_d_cellsize; j++)
                    {
                        //Probability[j] = (float)Math.Exp(-1.0 * j / local_maxD_d_cellsize);
                        Probability[j] = (float)Math.Exp(tmp_inverse * j);

                        probSum += Probability[j];
                    }

                    for (int j = 0; j <= local_maxD_d_cellsize; j++)
                    {
                        if (j == 0)
                        {
                            temp = Probability[j] / probSum;
                        }
                        else
                        {
                            temp = Probability[j] / probSum / (8 * j);
                        }

                        SetSeedRain(i, j, temp);

                    }

                    Probability = null;

                }



            }

            else

            {//Read From File
                StreamReader fp = new StreamReader(new FileStream(fileSeedDispersal, FileMode.Open));

                for (int i = 1; i <= specNum; i++)
                {
                    speciesattr local_speciesattr = sitetmp.specAtt(i);
                    int local_speciesattr_maxD = local_speciesattr.Max_seeding_Dis;


                    if (local_speciesattr_maxD < 0)
                    {
                        temp = system1.read_float(fp);
                        temp = Math.Abs(temp);

                        SetSeedRain(i, 0, temp);
                    }
                    else
                    {
                        int local_specatt_i_totalseed = local_speciesattr.TotalSeed;

                        if (local_speciesattr_maxD < cellSize)
                        {
                            for (int j = 0; j <= 1; j++)
                            {
                                // temp = system1.read_float(fp);
                                // temp = temp * local_specatt_i_totalseed;
                                temp = system1.read_float(fp) * local_specatt_i_totalseed;                                

                                SetSeedRain(i, j, temp);

                            }

                        }
                        else
                        {
                            for (int j = 0; j <= local_speciesattr_maxD / cellSize; j++)
                            {
                                // temp = system1.read_float(fp);
                                // temp = temp * local_specatt_i_totalseed;
                            	temp = system1.read_float(fp) * local_specatt_i_totalseed;                                

                                SetSeedRain(i, j, temp);

                            }

                        }

                    }

                }

                fp.Close();
            }

        }




        //Reads or calculates species' growth rates at corresponding ages,  no return value
        //Save the DBH and Age in a two dimension matrix, GrowthRates
        public void GetSpeciesGrowthRates(string fileGrowthRates, int growthrateflag)
        {
            site sitetmp = this[1, 1];

            GrowthFlag = growthrateflag;


            if (GrowthFlag == 0)
            {
                for (int i = 1; i <= specNum; i++)
                {
                    speciesattr local_speciesattr = sitetmp.specAtt(i);
                    int local_speciesattr_maxDQ = local_speciesattr.MaxDQ;

                    int local_time_loop = local_speciesattr.Longevity / timeStep;

                    if (local_speciesattr.SpType == 0)
                    {
                    	double local_const = -0.088 * 100 * timeStep / local_speciesattr.Longevity;

                        for (int j = 1; j <= local_time_loop + 1; j++)
                        {
                            float temp = (float)Math.Exp(-11.37 * Math.Exp(local_const * j)) * local_speciesattr_maxDQ;

                            SetGrowthRates(GrowthFlag, i, j, temp, 0);
                        }

                    }
                    else if (local_speciesattr.SpType == 1)
                    {
                    	double local_const = -0.12 * 100 * timeStep / local_speciesattr.Longevity;

                        for (int j = 1; j <= local_time_loop + 1; j++)
                        {
                            float temp = (float)Math.Exp(-11.7 * Math.Exp(local_const * j)) * local_speciesattr_maxDQ;

                            SetGrowthRates(GrowthFlag, i, j, temp, 0);
                        }

                    }
                    else
                    {
                        for (int j = 1; j <= local_time_loop + 1; j++)
                        {
                            SetGrowthRates(GrowthFlag, i, j, local_speciesattr_maxDQ, 0);
                        }

                    }

                }

            }

            else

            {   
                GrowthRateParamParser growthrate_parser = new GrowthRateParamParser(GrowthFlag);
                Landis.Data.Load<GrowthRateParam>(fileGrowthRates, growthrate_parser);

            }



        }



        //Reads or calculates species' mortality rates at corresponding ages, No Return Value
        //Save the DBH and Age in a two dimension matrix, MortalityRates
        public void GetSpeciesMortalityRates(string fileMortalityRates, int mortalityrateflag)
        {
            site sitetmp = this[1, 1];

            MortalityFlag = mortalityrateflag;

            if (MortalityFlag != 0)
            {
                //read data from file
                StreamReader fp = new StreamReader(new FileStream(fileMortalityRates, FileMode.Open));

                //specific MortalityRates for different landtypes go here
                int numLU = 0;

                while (!system1.LDeof(fp))
                {
                    for (int i = 1; i <= specNum; i++)
                    {
                        int local_time_loop = sitetmp.specAtt(i).Longevity / timeStep;

                        for (int j = 1; j <= local_time_loop; j++)
                        {
                            float temp = system1.read_float(fp);

                            SetMortalityRates(MortalityFlag, i, j, temp, numLU);
                        }
                    }

                    numLU++;
                }

                fp.Close();
            }

        }







        public void GetVolumeRead(string fileVolumeFlag, int VolumeFlag_Flag)
        {
            site sitetmp = this[1, 1];

            float[] VolumeTemp = new float[specNum * 6];

            VolumeFlag = VolumeFlag_Flag;


            if (VolumeFlag == 0)
            {
                for (int i = 1; i <= specNum; i++)
                {
                    for (int j = 1; j <= sitetmp.specAtt(i).Longevity / timeStep; j++)
                    {
                        double local_pow = Math.Pow(0.3048, 3.0);

                        if (GrowthFlag == 0)
                        {
                            float temp = (float)((-61.9 + 6.83 * GetGrowthRates(i, j, 0) / 2.54) * local_pow);

                            SetVolume(GrowthFlag, i, j, temp, 0);
                        }
                        else
                        {
                            for (int i_growth = 0; i_growth < GrowthRates_file.Count; i_growth++)
                            {
                                float temp = (float)((-61.9 + 6.83 * GetGrowthRates(i, j, i_growth) / 2.54) * local_pow);

                                SetVolume(GrowthFlag, i, j, temp, i_growth);
                            }
                        }

                    }

                }

            }
            else
            {//read data from file

                StreamReader fp = new StreamReader(new FileStream(fileVolumeFlag, FileMode.Open));
                
                float temp = system1.read_int(fp);

                Debug.Assert(temp <= int.MaxValue && temp >= int.MinValue);
                
                VolumeFlag = (int)temp;
                
                string FilenameHeights = system1.read_string(fp);
                
                StreamReader fpTreeheight = new StreamReader(new FileStream(FilenameHeights, FileMode.Open));


                if (VolumeFlag == 1)
                {
                    for (int i = 1; i <= specNum; i++)
                    {
                        int loc_id = (i - 1) * 6;

                        for (int j = 1; j <= 6; j++)
                        {
                            VolumeTemp[loc_id + j - 1] = system1.read_float(fp);
                        }

                    }

                    for (int i = 1; i <= specNum; i++)
                    {
                        for (int j = 1; j <= sitetmp.specAtt(i).Longevity / timeStep; j++)
                        {
                            float TreeHeight = system1.read_float(fpTreeheight);

                            int loc_id = (i - 1) * 6;

                            float vol_tmp0 = VolumeTemp[loc_id];
                            float vol_tmp1 = VolumeTemp[loc_id + 1];
                            float vol_tmp2 = VolumeTemp[loc_id + 2];
                            float vol_tmp3 = VolumeTemp[loc_id + 3];
                            float vol_tmp4 = VolumeTemp[loc_id + 4];
                            float vol_tmp5 = VolumeTemp[loc_id + 5];


                            if (GrowthFlag == 0)
                            {
                                float local_growthrate = GetGrowthRates(i, j, 0);

                                //temp = vol_tmp0 + vol_tmp1 * (float)Math.Pow(local_growthrate, vol_tmp2) + vol_tmp3 * (float)Math.Pow(local_growthrate, vol_tmp4) * (float)Math.Pow(TreeHeight, vol_tmp5);
                                temp = (float)(vol_tmp0 + vol_tmp1 * Math.Pow(local_growthrate, vol_tmp2) + vol_tmp3 * Math.Pow(local_growthrate, vol_tmp4) * Math.Pow(TreeHeight, vol_tmp5));

                                SetVolume(GrowthFlag, i, j, temp, 0);
                            }
                            else
                            {
                                for (int i_growth = 0; i_growth < GrowthRates_file.Count; i_growth++)
                                {
                                    float local_growthrate = GetGrowthRates(i, j, i_growth);

                                    //temp = vol_tmp0 + vol_tmp1 * (float)Math.Pow(local_growthrate, vol_tmp2) + vol_tmp3 * (float)Math.Pow(local_growthrate, vol_tmp4) * (float)Math.Pow(TreeHeight, vol_tmp5);
                                    temp = (float)(vol_tmp0 + vol_tmp1 * Math.Pow(local_growthrate, vol_tmp2) + vol_tmp3 * Math.Pow(local_growthrate, vol_tmp4) * Math.Pow(TreeHeight, vol_tmp5));
                                    
                                    SetVolume(GrowthFlag, i, j, temp, i_growth);
                                }
                            }
                        }

                    }

                }

                else
                {
                    throw new Exception("compute volume wrong");
                }

                fpTreeheight.Close();
                fp.Close();
            }

            VolumeTemp = null;
        }



    }



    public class BiomassParam {}

    class BiomassParamParser : Landis.TextParser<BiomassParam>
    {
        public override string LandisDataValue
        {
            get { return "BiomassCoefficients"; }
        }

        //read biomass coefficients from a file into matrix, (float) BioMassData(int ID,2). No Return Value
        //Read a ID first from the file, and ID is the size of BioMassData;
        //Read the two variable in to BioMassData(v1,v2)
        protected override BiomassParam Parse()
        {
            ReadLandisDataVar();

            InputVar<int> speciesnum = new InputVar<int>("Number_of_species_class");
            ReadVar(speciesnum);
            PlugIn.gl_sites.SetBiomassNum(speciesnum.Value.Actual);

            InputVar<float> biomassThreshold = new InputVar<float>("minimum_DBH_for_calculating_biomass");
            ReadVar(biomassThreshold);
            PlugIn.gl_sites.BiomassThreshold = biomassThreshold.Value.Actual;

            InputVar<float> float_val = new InputVar<float>("V0 or V1 value for each species");

            for (int i = 1; i <= speciesnum.Value.Actual; i++)
            {
                if (AtEndOfInput)
                    throw NewParseException("Expected a line here");

                Landis.Utilities.StringReader currentLine = new Landis.Utilities.StringReader(CurrentLine);

                ReadValue(float_val, currentLine);

                PlugIn.gl_sites.SetBiomassData(i, 1, float_val.Value.Actual);


                ReadValue(float_val, currentLine);

                PlugIn.gl_sites.SetBiomassData(i, 2, float_val.Value.Actual);

                //CheckNoDataAfter("the Ecoregion " + float_val + " column",
                //                 currentLine);

                GetNextLine();
            }

            return null;
        }
    }



    public class GrowthRateParam { }

    class GrowthRateParamParser : Landis.TextParser<GrowthRateParam>
    {
        private int growthFlag;

        public override string LandisDataValue
        {
            get { return "LandtypeGrowthRate"; }
        }

        public GrowthRateParamParser(int GrowthFlag_in)
        { 
            growthFlag = GrowthFlag_in; 
        }

        
        //read biomass coefficients from a file into matrix, (float) BioMassData(int ID,2). No Return Value
        //Read a ID first from the file, and ID is the size of BioMassData;
        //Read the two variable in to BioMassData(v1,v2)
        protected override GrowthRateParam Parse()
        {
            ReadLandisDataVar();

            InputVar<float> float_val = new InputVar<float>("growth rate value");

            int numLU = 0;

            site sitetmp = PlugIn.gl_sites[1, 1];
            int timeStep = PlugIn.gl_param.SuccessionTimestep;

            Landis.Utilities.StringReader currentLine = new Landis.Utilities.StringReader(CurrentLine);

            uint species_num = PlugIn.gl_sites.SpecNum;
            while (!AtEndOfInput)
            {
                for (int i = 1; i <= species_num; i++)
                {
                    int local_time_loop = sitetmp.specAtt(i).Longevity / timeStep;
                    for (int j = 1; j <= local_time_loop; j++)
                    {
                        ReadValue(float_val, currentLine);
                        PlugIn.gl_sites.SetGrowthRates(growthFlag, i, j, float_val.Value.Actual, numLU);
                    }
                    CheckNoDataAfter("the Ecoregion " + float_val + " column",
                                 currentLine);

                    GetNextLine();
                    currentLine = new Landis.Utilities.StringReader(CurrentLine);
                }

                numLU++;
                
            }

            return null;
        }
    }



    












    public class OutputOption { }



    class OutputOptionParser : Landis.TextParser<OutputOption>
    {
        public override string LandisDataValue
        {
            get { return "Output option"; }
        }


        protected override OutputOption Parse()
        {
            Console.WriteLine("reading Landis 70 output customization file.");

            ReadLandisDataVar();

            uint specNum = PlugIn.gl_sites.SpecNum;

            Landis.Utilities.StringReader currentLine = new Landis.Utilities.StringReader(CurrentLine);




            ////general every species////            
            InputVar<string> temp = new InputVar<string>("string value");

            for (uint i = 0; i < specNum; i++)
            {                
                ReadValue(temp, currentLine);
                ///////////////////
                ReadValue(temp, currentLine);
                
                if (temp.Value.Actual == "Y")
                {
                    PlugIn.gl_sites.SetOutputGeneralFlagArray(i, (int)enum1.TPA, 1);
                }
                else
                {
                    PlugIn.gl_sites.SetOutputGeneralFlagArray(i, (int)enum1.TPA, 0);
                }



                ReadValue(temp, currentLine);

                if (temp.Value.Actual == "Y")
                {
                    PlugIn.gl_sites.SetOutputGeneralFlagArray(i, (int)enum1.BA, 1);
                }
                else
                {
                    PlugIn.gl_sites.SetOutputGeneralFlagArray(i, (int)enum1.BA, 0);
                }



                ReadValue(temp, currentLine);

                if (temp.Value.Actual == "Y")
                {
                    PlugIn.gl_sites.SetOutputGeneralFlagArray(i, (int)enum1.Bio, 1);
                }
                else
                {
                    PlugIn.gl_sites.SetOutputGeneralFlagArray(i, (int)enum1.Bio, 0);
                }


                ReadValue(temp, currentLine);

                if (temp.Value.Actual == "Y")
                {
                    PlugIn.gl_sites.SetOutputGeneralFlagArray(i, (int)enum1.IV, 1);
                }
                else
                {
                    PlugIn.gl_sites.SetOutputGeneralFlagArray(i, (int)enum1.IV, 0);
                }


                ReadValue(temp, currentLine);

                if (temp.Value.Actual == "Y")
                {
                    PlugIn.gl_sites.SetOutputGeneralFlagArray(i, (int)enum1.Seeds, 1);
                }
                else
                {
                    PlugIn.gl_sites.SetOutputGeneralFlagArray(i, (int)enum1.Seeds, 0);
                }


                ReadValue(temp, currentLine);

                if (temp.Value.Actual == "Y")
                {
                    PlugIn.gl_sites.SetOutputGeneralFlagArray(i, (int)enum1.RDensity, 1);
                }
                else
                {
                    PlugIn.gl_sites.SetOutputGeneralFlagArray(i, (int)enum1.RDensity, 0);
                }


                CheckNoDataAfter(temp + " column", currentLine);

                GetNextLine();
                currentLine = new Landis.Utilities.StringReader(CurrentLine);
            }


            ///////below is for general total//////////

            ReadValue(temp, currentLine);
            ReadValue(temp, currentLine);


            if (temp.Value.Actual == "Y")
            {
                PlugIn.gl_sites.SetOutputGeneralFlagArray(specNum, (int)enum1.TPA, 1);
            }
            else
            {
                PlugIn.gl_sites.SetOutputGeneralFlagArray(specNum, (int)enum1.TPA, 0);
            }



            ReadValue(temp, currentLine);

            if (temp.Value.Actual == "Y")
            {
                PlugIn.gl_sites.SetOutputGeneralFlagArray(specNum, (int)enum1.BA, 1);
            }
            else
            {
                PlugIn.gl_sites.SetOutputGeneralFlagArray(specNum, (int)enum1.BA, 0);
            }



            ReadValue(temp, currentLine);

            if (temp.Value.Actual == "Y")
            {
                PlugIn.gl_sites.SetOutputGeneralFlagArray(specNum, (int)enum1.Bio, 1);
            }
            else
            {
                PlugIn.gl_sites.SetOutputGeneralFlagArray(specNum, (int)enum1.Bio, 0);
            }



            ReadValue(temp, currentLine);

            if (temp.Value.Actual == "Y")
            {
                PlugIn.gl_sites.SetOutputGeneralFlagArray(specNum, (int)enum1.Car, 1);
            }
            else
            {
                PlugIn.gl_sites.SetOutputGeneralFlagArray(specNum, (int)enum1.Car, 0);
            }


            ReadValue(temp, currentLine);

            if (temp.Value.Actual == "Y")
            {
                PlugIn.gl_sites.SetOutputGeneralFlagArray(specNum, (int)enum1.RDensity, 1);
            }
            else
            {
                PlugIn.gl_sites.SetOutputGeneralFlagArray(specNum, (int)enum1.RDensity, 0);
            }

            CheckNoDataAfter(temp + " column", currentLine);
            GetNextLine();
            currentLine = new Landis.Utilities.StringReader(CurrentLine);
            /////////////////////

            ReadValue(temp, currentLine);
            
            CheckNoDataAfter(temp + " column", currentLine);
            GetNextLine();
            currentLine = new Landis.Utilities.StringReader(CurrentLine);
            if (temp.Value.Actual == "Y")
            {
                PlugIn.gl_sites.FlagAgeRangeOutput = 1;
            }
            else
            {
                PlugIn.gl_sites.FlagAgeRangeOutput = 0;
            }

            /////below is age range for every species///////

            for (uint i = 0; i < specNum; i++)
            {

                ReadValue(temp, currentLine);

                //////////////////////

                ReadValue(temp, currentLine);

                if (temp.Value.Actual == "Y")
                {
                    PlugIn.gl_sites.SetOutputAgerangeFlagArray(i, (int)enum1.TPA, 1);
                }
                else
                {
                    PlugIn.gl_sites.SetOutputAgerangeFlagArray(i, (int)enum1.TPA, 0);
                }


                ReadValue(temp, currentLine);

                if (temp.Value.Actual == "Y")
                {
                    PlugIn.gl_sites.SetOutputAgerangeFlagArray(i, (int)enum1.BA, 1);
                }
                else
                {
                    PlugIn.gl_sites.SetOutputAgerangeFlagArray(i, (int)enum1.BA, 0);
                }


                ReadValue(temp, currentLine);

                if (temp.Value.Actual == "Y")
                {
                    PlugIn.gl_sites.SetOutputAgerangeFlagArray(i, (int)enum1.Bio, 1);
                }
                else
                {
                    PlugIn.gl_sites.SetOutputAgerangeFlagArray(i, (int)enum1.Bio, 0);
                }


                ReadValue(temp, currentLine);

                if (temp.Value.Actual == "Y")
                {
                    PlugIn.gl_sites.SetOutputAgerangeFlagArray(i, (int)enum1.IV, 1);
                }
                else
                {
                    PlugIn.gl_sites.SetOutputAgerangeFlagArray(i, (int)enum1.IV, 0);
                }

                CheckNoDataAfter(temp + " column", currentLine);
                GetNextLine();
                currentLine = new Landis.Utilities.StringReader(CurrentLine);
            }

            ///////below is for age range total//////////

            ReadValue(temp, currentLine);
            ReadValue(temp, currentLine);

            if (temp.Value.Actual == "Y")
            {
                PlugIn.gl_sites.SetOutputAgerangeFlagArray(specNum, (int)enum1.TPA, 1);
            }
            else
            {
                PlugIn.gl_sites.SetOutputAgerangeFlagArray(specNum, (int)enum1.TPA, 0);
            }


            ReadValue(temp, currentLine);

            if (temp.Value.Actual == "Y")
            {
                PlugIn.gl_sites.SetOutputAgerangeFlagArray(specNum, (int)enum1.BA, 1);
            }
            else
            {
                PlugIn.gl_sites.SetOutputAgerangeFlagArray(specNum, (int)enum1.BA, 0);
            }


            ReadValue(temp, currentLine);

            if (temp.Value.Actual == "Y")
            {
                PlugIn.gl_sites.SetOutputAgerangeFlagArray(specNum, (int)enum1.Bio, 1);
            }
            else
            {
                PlugIn.gl_sites.SetOutputAgerangeFlagArray(specNum, (int)enum1.Bio, 0);
            }


            ReadValue(temp, currentLine);

            if (temp.Value.Actual == "Y")
            {
                PlugIn.gl_sites.SetOutputAgerangeFlagArray(specNum, (int)enum1.Car, 1);
            }
            else
            {
                PlugIn.gl_sites.SetOutputAgerangeFlagArray(specNum, (int)enum1.Car, 0);
            }


            ReadValue(temp, currentLine);

            if (temp.Value.Actual == "Y")
            {
                PlugIn.gl_sites.SetOutputAgerangeFlagArray(specNum, (int)enum1.RDensity, 1);
            }
            else
            {
                PlugIn.gl_sites.SetOutputAgerangeFlagArray(specNum, (int)enum1.RDensity, 0);
            }
            CheckNoDataAfter(temp + " column", currentLine);
            GetNextLine();
            currentLine = new Landis.Utilities.StringReader(CurrentLine);
            ////////////////////////

            for (int i = 0; i < specNum; i++)
            {
                ReadValue(temp, currentLine);

                InputVar<int> count = new InputVar<int>("count");

                ReadValue(count, currentLine);
                PlugIn.gl_sites.SetAgerangeCount(i, count.Value.Actual);


                //InputVar<int> value1 = new InputVar<int>("age cohorts for each species to output: begining age");
                //InputVar<int> value2 = new InputVar<int>("age cohorts for each species to output: end age");
                InputVar<string> charc = new InputVar<string>("character");

                for (int j = 1; j <= count.Value.Actual; j++)
                {
                    //ReadValue(value1, currentLine);
                    ReadValue( charc, currentLine);
                    //ReadValue(value2, currentLine);

                    //int value1 = system1.read_int(fp);
                    //char ch = (char)fp.Read();//cnx big problem
                    //int value2 = system1.read_int(fp);
                    string[] strc = charc.Value.Actual.Split('-');
                    //Console.WriteLine("{0} {1}", strc[0], strc[1]);
                    PlugIn.gl_sites.SetSpeciesAgerangeArray(i, j, int.Parse(strc[0]), int.Parse(strc[1]));                    
                }

                CheckNoDataAfter(temp + " column", currentLine);
                GetNextLine();
                currentLine = new Landis.Utilities.StringReader(CurrentLine);
            }



            ///////////////////////

/*            ReadValue(temp, currentLine);

            if (temp.Value.Actual != "N/A")
            {
                age_TPA_distParser age_TPA_dist_parser = new age_TPA_distParser();
                Landis.Data.Load<age_TPA_dist>(temp.Value.Actual, age_TPA_dist_parser);

            }
            else
            {
                PlugIn.gl_sites.Flag_AgeDistStat = 0;
            } */          


            return null;
        }



    }



    public class age_TPA_dist { }

    class age_TPA_distParser : Landis.TextParser<age_TPA_dist>
    {
        public override string LandisDataValue
        {
            get { return "age TPA distribution"; }
        }

        protected override age_TPA_dist Parse()
        {
            ReadLandisDataVar();

            uint specNum = PlugIn.gl_sites.SpecNum;

            InputVar<int> int_val = new InputVar<int>("int value");
            InputVar<string> temp = new InputVar<string>("string value");

            PlugIn.gl_sites.Flag_AgeDistStat = 1;
            
            Landis.Utilities.StringReader currentLine = new Landis.Utilities.StringReader(CurrentLine);

            for (int i = 0; i < specNum; i++)
            {
                ReadValue(temp, currentLine);
                
                InputVar<int> count = new InputVar<int>("count");
                ReadValue(count, currentLine);

                PlugIn.gl_sites.SetAgeDistStat_YearValCount(i, count.Value.Actual);


                InputVar<int> value1 = new InputVar<int>("int value");

                for (int j = 1; j <= count.Value.Actual; j++)
                {
                    ReadValue(value1, currentLine);
                    PlugIn.gl_sites.SetAgeDistStat_YearVal(i, j, value1.Value.Actual);
                }

            }


            for (int i = 0; i < specNum; i++)
            {
                ReadValue(temp, currentLine);

                InputVar<int> count = new InputVar<int>("count");
                ReadValue(count, currentLine);

                PlugIn.gl_sites.SetAgeDistStat_AgeRangeCount(i, count.Value.Actual);

                InputVar<int> value1 = new InputVar<int>("age cohorts for each species to output: begining age");
                InputVar<int> value2 = new InputVar<int>("age cohorts for each species to output: end age");
                InputVar<char> charc = new InputVar<char>("character");

                for (int j = 1; j <= count.Value.Actual; j++)
                {
                    ReadValue(value1, currentLine);
                    ReadValue(charc, currentLine);
                    ReadValue(value2, currentLine);

                    //int value1 = system1.read_int(fp);
                    //char ch = (char)fp.Read();//cnx big problem
                    //int value2 = system1.read_int(fp);

                    PlugIn.gl_sites.SetAgeDistStat_AgeRangeVal(i, j, value1.Value.Actual, value2.Value.Actual);

                }

            }

            return null;
        }
    }

}
