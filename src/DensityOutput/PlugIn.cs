//  Authors:  Robert M. Scheller, James B. Domingo

using Landis.Core;
using Landis.Utilities;
using Landis.Library.Succession;
using Landis.Library.UniversalCohorts;
using Landis.Library.DensityCohorts;
using Landis.SpatialModeling;
using Landis.Library.Metadata;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Landis.Extension.Succession.Density;
using Landis.Library.Succession.DemographicSeeding;
using Landis.Library.Succession.DensitySeeding;
using Accord.Math;

namespace Landis.Extension.Output.Density
{
    public class PlugIn
        : ExtensionMain
    {
        public static readonly ExtensionType ExtType = new ExtensionType("output");
        public static readonly string ExtensionName = "Density Output";

        public static IEnumerable<ISpecies> speciesToMap;
        public static string speciesTemplateToMap;
        public static string poolsToMap;
        public static string poolsTemplateToMap;
        private IEnumerable<ISpecies> selectedSpecies;
        private string speciesMapNameTemplate;
        private IInputParameters parameters;
        private static ICore modelCore;
        private bool makeTable;
        public static MetadataTable<SummaryLog> summaryLog;

        //---------------------------------------------------------------------

        public PlugIn()
            : base(ExtensionName, ExtType)
        {
        }

        //---------------------------------------------------------------------

        public static ICore ModelCore
        {
            get
            {
                return modelCore;
            }
        }
        //---------------------------------------------------------------------

        public override void LoadParameters(string dataFile, ICore mCore)
        {
            modelCore = mCore;
            //InputParametersParser parser = new InputParametersParser();
            //parameters = Landis.Data.Load<IInputParameters>(dataFile, parser);
        }

        //---------------------------------------------------------------------

        public override void Initialize()
        {
            InputParameters parameters = new InputParameters();
            parameters.Timestep = Landis.Library.DensityCohorts.Cohorts.SuccessionTimeStep;
            parameters.MakeTable = true;
            parameters.SelectedSpecies = PlugIn.ModelCore.Species;
            parameters.SpeciesMapNames = "outputs/density/{species}-{timestep}.img";

            Timestep = parameters.Timestep;
            this.selectedSpecies = parameters.SelectedSpecies;
            speciesToMap = this.selectedSpecies;
            this.speciesMapNameTemplate = parameters.SpeciesMapNames;
            speciesTemplateToMap = this.speciesMapNameTemplate;
            this.makeTable = parameters.MakeTable;
            MetadataHandler.InitializeMetadata(parameters.Timestep, "spp-density-log.csv", makeTable);

            SiteVars.Initialize();
        }

        //---------------------------------------------------------------------

        public override void Run()
        {

            WriteMapForAllSpecies();

            if (makeTable)
                WriteLogFile();

            if (selectedSpecies != null)
            {
                WriteSpeciesMaps();
                
            }
        }

        //---------------------------------------------------------------------

        private void WriteSpeciesMaps()
        {
            foreach (ISpecies species in selectedSpecies) {
                string treepath = MakeSpeciesTreenumberMapName(species.Name);
                string basalpath = MakeSpeciesBasalMapName(species.Name);
                PlugIn.ModelCore.UI.WriteLine("   Writing {0} maps ...", species.Name);

                using (IOutputRaster<IntPixel> outputRaster = modelCore.CreateRaster<IntPixel>(treepath, modelCore.Landscape.Dimensions))
                {
                    IntPixel pixel = outputRaster.BufferPixel;
                    foreach (Site site in PlugIn.ModelCore.Landscape.AllSites)
                    {
                        if (site.IsActive)
                            pixel.MapCode.Value = ComputeSpeciesTreeNumber(site, species);
                        else
                            pixel.MapCode.Value = 0;

                        outputRaster.WriteBufferPixel();
                    }
                }

                using (IOutputRaster<IntPixel> outputRaster = modelCore.CreateRaster<IntPixel>(basalpath, modelCore.Landscape.Dimensions))
                {
                    IntPixel pixel = outputRaster.BufferPixel;
                    foreach (Site site in PlugIn.ModelCore.Landscape.AllSites)
                    {
                        if (site.IsActive)
                            pixel.MapCode.Value = (int)(Math.Round(ComputeSpeciesBasal(site, species), 2) * 100);
                        else
                            pixel.MapCode.Value = 0;

                        outputRaster.WriteBufferPixel();
                    }
                }
            }

        }

        //---------------------------------------------------------------------

        private void WriteMapForAllSpecies()
        {
            string treepath = MakeSpeciesTreenumberMapName("AllSpecies");
            string basalpath = MakeSpeciesBasalMapName("AllSpecies");
            PlugIn.ModelCore.UI.WriteLine("   Writing all species maps ...");
            using (IOutputRaster<IntPixel> outputRaster = modelCore.CreateRaster<IntPixel>(treepath, modelCore.Landscape.Dimensions))
            {
                IntPixel pixel = outputRaster.BufferPixel;
                foreach (Site site in PlugIn.ModelCore.Landscape.AllSites)
                {
                    if (site.IsActive)
                        pixel.MapCode.Value = ComputeTotalTreeNumber(site);
                    else
                        pixel.MapCode.Value = 0;

                    outputRaster.WriteBufferPixel();
                }
            }

            using (IOutputRaster<IntPixel> outputRaster = modelCore.CreateRaster<IntPixel>(basalpath, modelCore.Landscape.Dimensions))
            {
                IntPixel pixel = outputRaster.BufferPixel;
                foreach (Site site in PlugIn.ModelCore.Landscape.AllSites)
                {
                    if (site.IsActive)
                        pixel.MapCode.Value = (int)(Math.Round(ComputeTotalBasal(site), 2) * 100);
                    else
                        pixel.MapCode.Value = 0;

                    outputRaster.WriteBufferPixel();
                }
            }
        }

        //---------------------------------------------------------------------

        private string MakeSpeciesTreenumberMapName(string species)
        {
            string mapName = "outputs/density/{species}-TreeNumber-{timestep}.img";
            return SpeciesMapNames.ReplaceTemplateVars(mapName,
                                                       species,
                                                       PlugIn.ModelCore.CurrentTime);
        }


        //---------------------------------------------------------------------

        private string MakeSpeciesBasalMapName(string species)
        {
            string mapName = "outputs/density/{species}-BasalArea-{timestep}.img";
            return SpeciesMapNames.ReplaceTemplateVars(mapName,
                                                       species,
                                                       PlugIn.ModelCore.CurrentTime);
        }


        //---------------------------------------------------------------------


        private void WriteLogFile()
        {


            double[,,] allSppEcos = new double[ModelCore.Ecoregions.Count, ModelCore.Species.Count,2];

            int[] activeSiteCount = new int[ModelCore.Ecoregions.Count];

            //UI.WriteLine("Next, reset all values to zero.");

            foreach (IEcoregion ecoregion in ModelCore.Ecoregions)
            {
                foreach (ISpecies species in ModelCore.Species)
                {
                    allSppEcos[ecoregion.Index, species.Index, 0] = 0.0;
                    allSppEcos[ecoregion.Index, species.Index, 1] = 0.0;
                }

                activeSiteCount[ecoregion.Index] = 0;
            }

            //UI.WriteLine("Next, accumulate data.");


            foreach (ActiveSite site in ModelCore.Landscape)
            {
                IEcoregion ecoregion = ModelCore.Ecoregion[site];

                foreach (ISpecies species in ModelCore.Species)
                {
                    allSppEcos[ecoregion.Index, species.Index, 0] += ComputeSpeciesTreeNumber(site, species);
                    allSppEcos[ecoregion.Index, species.Index, 1] += ComputeSpeciesBasal(site, species);
                }

                activeSiteCount[ecoregion.Index]++;
            }

            foreach (IEcoregion ecoregion in ModelCore.Ecoregions)
            {
                summaryLog.Clear();
                SummaryLog sl = new SummaryLog();
                sl.Time = modelCore.CurrentTime;
                sl.EcoName = ecoregion.Name;
                sl.NumActiveSites = activeSiteCount[ecoregion.Index];
                double[] treeNumber = new double[modelCore.Species.Count];
                double[] basalArea = new double[modelCore.Species.Count];
                foreach (ISpecies species in ModelCore.Species)
                {
                    treeNumber[species.Index] = allSppEcos[ecoregion.Index, species.Index, 0] / (double)activeSiteCount[ecoregion.Index];
                    basalArea[species.Index] = allSppEcos[ecoregion.Index, species.Index, 1] / (double)activeSiteCount[ecoregion.Index];
                }
                sl.TreeNumber_ = treeNumber;
                sl.BasalArea_ = basalArea;

                summaryLog.AddObject(sl);
                summaryLog.WriteToFile();
            }
        }
        //---------------------------------------------------------------------
        private static double ComputeSpeciesBasal(Landis.SpatialModeling.Site site, Landis.Core.ISpecies species)
        {
            double total = 0;
            if (SiteVars.Cohorts != null)
            {
                ISiteVar<Landis.Library.Parameters.Species.AuxParm<double>> Basal_spc = SiteVars.Cohorts.GetIsiteVar(o => o.BasalPerSpecies);

                total = Basal_spc[site][species];
            }
            return total;
        }

        //---------------------------------------------------------------------

        private static int ComputeSpeciesTreeNumber(Landis.SpatialModeling.Site site, Landis.Core.ISpecies species)
        {
            int total = 0;
            if (SiteVars.Cohorts != null)
            {
                ISiteVar<Landis.Library.Parameters.Species.AuxParm<int>> TreeNumbers = SiteVars.Cohorts.GetIsiteVar(o => o.TreeNumberPerSpecies);

                total = TreeNumbers[site][species];
            }
            return total;
        }

        //---------------------------------------------------------------------

        private static int ComputeTotalTreeNumber(Landis.SpatialModeling.Site site)
        {
            int total = 0;
            if (SiteVars.Cohorts != null)
            {
                ISiteVar<Landis.Library.Parameters.Species.AuxParm<int>> TreeNumbers = SiteVars.Cohorts.GetIsiteVar(o => o.TreeNumberPerSpecies);

                foreach (ISpecies species in modelCore.Species)
                {
                    total += TreeNumbers[site][species];
                }
            }
            return total;
        }
        
        //---------------------------------------------------------------------

        private static double ComputeTotalBasal(Landis.SpatialModeling.Site site)
        {
            double total = 0;

            if (SiteVars.Cohorts != null)
            {
                ISiteVar<Landis.Library.Parameters.Species.AuxParm<double>> Basal = SiteVars.Cohorts.GetIsiteVar(o => o.BasalPerSpecies);

                foreach (ISpecies species in modelCore.Species)
                {
                    total += Basal[site][species];
                }
            }
            return total;
        }

        public override void AddCohortData()
        {
            // add cohort data here
        }
    }
}
