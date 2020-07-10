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
using System.Net;
//using Landis.Library.AgeOnlyCohorts;

namespace Landis.Extension.Succession.BiomassPnET
{
    public class PlugIn  : Landis.Library.Succession.ExtensionBase 
    {
        //================================== Density variables ================================
        //public static ISiteVar<float> SiteRD;
        public static SpeciesDensity SpeciesDensity;
        //=====================================================================================
        //public static ISiteVar<Landis.Library.Biomass.Pool> WoodyDebris;
        //public static ISiteVar<Landis.Library.Biomass.Pool> Litter;
        //public static ISiteVar<Double> FineFuels;
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
            


            StartDate = new DateTime(((Parameter<int>)GetParameter(Names.StartYear)).Value, 1, 15);

            PlugIn.ModelCore.UI.WriteLine("Spinning up biomass or reading from maps...");

            string InitialCommunitiesTXTFile = GetParameter(Names.InitialCommunities).Value;
            string InitialCommunitiesMapFile = GetParameter(Names.InitialCommunitiesMap).Value;
            InitializeSites(InitialCommunitiesTXTFile, InitialCommunitiesMapFile, ModelCore);

            SeedingAlgorithms SeedAlgorithm = (SeedingAlgorithms)Enum.Parse(typeof(SeedingAlgorithms), parameters["SeedingAlgorithm"].Value);

            base.Initialize(ModelCore, SeedAlgorithm);



            // Convert Density cohorts to biomasscohorts
            ISiteVar<Landis.Library.BiomassCohorts.ISiteCohorts> biomassCohorts = PlugIn.ModelCore.Landscape.NewSiteVar<Landis.Library.BiomassCohorts.ISiteCohorts>();

            foreach (ActiveSite site in PlugIn.ModelCore.Landscape)
            {
                biomassCohorts[site] = sitecohorts[site];
                
                if (sitecohorts[site] != null && biomassCohorts[site] == null)
                {
                    throw new System.Exception("Cannot convert Density SiteCohorts to biomass site cohorts");
                }
            }
            ModelCore.RegisterSiteVar(biomassCohorts, "Succession.BiomassCohorts");

            ISiteVar<Landis.Library.AgeOnlyCohorts.ISiteCohorts> AgeCohortSiteVar = PlugIn.ModelCore.Landscape.NewSiteVar<Landis.Library.AgeOnlyCohorts.ISiteCohorts>();
            // FIXME
            ISiteVar<Landis.Library.DensityCohorts.ISiteCohorts> DensityCohorts = PlugIn.ModelCore.Landscape.NewSiteVar<Landis.Library.DensityCohorts.ISiteCohorts>();


            foreach (ActiveSite site in PlugIn.ModelCore.Landscape)
            {
                Cohort.SetSiteAccessFunctions(sitecohorts[site]);
                float tempRD = SiteVars.SiteRD[site];
                DensityCohorts[site] = sitecohorts[site];
            }

            ModelCore.RegisterSiteVar(AgeCohortSiteVar, "Succession.AgeCohorts");
            ModelCore.RegisterSiteVar(DensityCohorts, "Succession.CohortsDensity");
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
            ISpeciesDensity spc = SpeciesDensity[species];
            Cohort cohort = new Cohort(spc, (ushort)Date.Year, (SiteOutputNames.ContainsKey(site)) ? SiteOutputNames[site] : null);
            
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
        public double DensitySeeds(ISpecies species, ActiveSite site)
        {
            double availableSeed = 0;
            int totalseed_m_timestep = SpeciesDensity[species].TotalSeed * Timestep;
            if (SpeciesDensity[species].SpType < 0)
                availableSeed += (uint)totalseed_m_timestep; //site.cs Ln 1971
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
                        availableSeed += double_val;
                    }
                }
                else
                {
                    SiteCohorts mySiteCohorts = sitecohorts[site];
                    double matureTrees = 0;
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
                    availableSeed += double_val;
                }
            }
            float float_rand = (float)ModelCore.ContinuousUniformDistribution.NextDouble();
            double double_value = availableSeed * (0.95 + float_rand * 0.1);  //from site.cs Ln 2045
            availableSeed = (uint)double_value;

            return availableSeed;
        }
        //---------------------------------------------------------------------

    }
}


