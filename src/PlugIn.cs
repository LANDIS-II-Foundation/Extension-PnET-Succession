//  Authors:    Arjan de Bruijn
//              Brian R. Miranda

// John McNabb: (02.04.2019)
//
//  Summary of changes to allow the climate library to be used with PnET-Succession:
//   (1) Added ClimateRegionData class based on that of NECN to hold the climate library data. This is Initialized by a call
//       to InitialClimateLibrary() in Plugin.Initialize().
//   (2) Modified EcoregionPnET to add GetClimateRegionData() which grabs climate data from ClimateRegionData.  This uses an intermediate
//       MonthlyClimateRecord instance which is similar to ObservedClimate.
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
using Landis.Library.InitialCommunities;
using Landis.Library.PnETCohorts;
using Landis.Library.Succession;
using Landis.SpatialModeling;
using Landis.Library.Climate;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Diagnostics;


namespace Landis.Extension.Succession.BiomassPnET
{
    public class PlugIn  : Landis.Library.Succession.ExtensionBase 
    {
        public static SpeciesPnET SpeciesPnET;
        public static DateTime Date;
        public static ICore ModelCore;
        private static DateTime StartDate;
        private static Dictionary<ActiveSite, string> SiteOutputNames;
        public static float FTimeStep;
        public static bool UsingClimateLibrary;
        private Dictionary<ActiveSite, ICommunity> sitesAndCommunities;
        public static string InitialCommunitiesSpinup;
        public static int CohortBinSize;
        public static int ParallelThreads;
        private static readonly object threadLock = new object();
        private Dictionary<ActiveSite, uint> allKeys;
        public static float MinFolRatioFactor;

        MyClock m = null;
        //---------------------------------------------------------------------
        public void DeathEvent(object sender, Landis.Library.PnETCohorts.DeathEventArgs eventArgs)
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
        //---------------------------------------------------------------------
        string PnETDefaultsFolder
        {
            get
            {
                return System.IO.Path.Combine(System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location), "Defaults");
            }
        }
        //---------------------------------------------------------------------
        public PlugIn()
            : base(Names.ExtensionName)
        {
            LocalOutput.PNEToutputsites = Names.PNEToutputsites;

            // The number of thread workers to use in succession routines that have been optimized. Should
            // more or less match the number of cores in the computer thats running LANDIS-II's processor
            //this.ThreadCount = 3;
            //this.ThreadCount = 1;

            allKeys = new Dictionary<ActiveSite, uint>();
            sitesAndCommunities = new Dictionary<ActiveSite, ICommunity>();
        }
        //---------------------------------------------------------------------
        public override void LoadParameters(string InputParameterFile, ICore mCore)
        {
            ModelCore = mCore;

            Names.parameters.Add(Names.ExtensionName, new Parameter<string>(Names.ExtensionName, InputParameterFile));

            //-------------PnET-Succession input files
            Dictionary<string, Parameter<string>> InputParameters = Names.LoadTable(Names.ExtensionName, Names.AllNames, null, true);
            InputParameters.ToList().ForEach(x => Names.parameters.Add(x.Key, x.Value));

            //-------------Read Species parameters input file
            List<string> SpeciesNames = PlugIn.ModelCore.Species.ToList().Select(x => x.Name).ToList();
            List<string> SpeciesPars = SpeciesPnET.ParameterNames;
            SpeciesPars.Add(Names.PnETSpeciesParameters);
            Dictionary<string, Parameter<string>> speciesparameters = Names.LoadTable(Names.PnETSpeciesParameters, SpeciesNames, SpeciesPars);
            foreach (string key in speciesparameters.Keys)
            {
                if (Names.parameters.ContainsKey(key)) throw new System.Exception("Parameter " + key + " was provided twice");
            }
            speciesparameters.ToList().ForEach(x => Names.parameters.Add(x.Key, x.Value));

            //-------------Ecoregion parameters
            List<string> EcoregionNames = PlugIn.ModelCore.Ecoregions.ToList().Select(x => x.Name).ToList();
            List<string> EcoregionParameters = EcoregionData.ParameterNames;
            Dictionary<string, Parameter<string>> ecoregionparameters = Names.LoadTable(Names.EcoregionParameters, EcoregionNames, EcoregionParameters);
            foreach (string key in ecoregionparameters.Keys)
            {
                if (Names.parameters.ContainsKey(key)) throw new System.Exception("Parameter "+ key +" was provided twice");
            }
            ecoregionparameters.ToList().ForEach(x => Names.parameters.Add(x.Key, x.Value));

            //-------------DisturbanceReductionsParameterFile
            Parameter<string> DisturbanceReductionsParameterFile;
            if (Names.TryGetParameter(Names.DisturbanceReductions, out DisturbanceReductionsParameterFile))
            {
                Allocation.Initialize(DisturbanceReductionsParameterFile.Value, Names.parameters);
                Cohort.AgeOnlyDeathEvent += DisturbanceReductions.Events.CohortDied;
            }
             
            //---------------SaxtonAndRawlsParameterFile
            if (Names.parameters.ContainsKey(PressureHeadSaxton_Rawls.SaxtonAndRawlsParameters) == false)
            {
                Parameter<string> SaxtonAndRawlsParameterFile = new Parameter<string>(PressureHeadSaxton_Rawls.SaxtonAndRawlsParameters, (string)PnETDefaultsFolder + "\\SaxtonAndRawlsParameters.txt");
                Names.parameters.Add(PressureHeadSaxton_Rawls.SaxtonAndRawlsParameters, SaxtonAndRawlsParameterFile);
            }
            Dictionary<string, Parameter<string>> SaxtonAndRawlsParameters = Names.LoadTable(PressureHeadSaxton_Rawls.SaxtonAndRawlsParameters, null, PressureHeadSaxton_Rawls.ParameterNames);
            foreach (string key in SaxtonAndRawlsParameters.Keys)
            {
                if (Names.parameters.ContainsKey(key)) throw new System.Exception("Parameter " + key + " was provided twice");
            }
            SaxtonAndRawlsParameters.ToList().ForEach(x => Names.parameters.Add(x.Key, x.Value));

            //--------------PnETGenericParameterFile
            //----------See if user supplied overwriting default parameters
            List<string> RowLabels = new List<string>(Names.AllNames);
            RowLabels.AddRange(SpeciesPnET.ParameterNames); 

            if (Names.parameters.ContainsKey(Names.PnETGenericParameters))
            {
                Dictionary<string, Parameter<string>> genericparameters = Names.LoadTable(Names.PnETGenericParameters,  RowLabels, null, true);
                foreach (KeyValuePair<string, Parameter<string>> par in genericparameters)
                {
                    if (Names.parameters.ContainsKey(par.Key)) throw new System.Exception("Parameter " + par.Key + " was provided twice");
                    Names.parameters.Add(par.Key, par.Value);
                }
            }

            //----------Load in default parameters to fill the gaps
            Parameter<string> PnETGenericDefaultParameterFile = new Parameter<string>(Names.PnETGenericDefaultParameters, (string)PnETDefaultsFolder + "\\PnETGenericDefaultParameters.txt");
            Names.parameters.Add(Names.PnETGenericDefaultParameters, PnETGenericDefaultParameterFile);
            Dictionary<string, Parameter<string>> genericdefaultparameters = Names.LoadTable(Names.PnETGenericDefaultParameters, RowLabels, null, true);

            foreach (KeyValuePair<string, Parameter<string>> par in genericdefaultparameters)
            {
                if (Names.parameters.ContainsKey(par.Key) == false)
                {
                    Names.parameters.Add(par.Key, par.Value);
                }
            }

            SiteOutputNames = new Dictionary<ActiveSite, string>();
            Parameter<string> OutputSitesFile;
            if (Names.TryGetParameter(LocalOutput.PNEToutputsites, out OutputSitesFile))
            {
                Dictionary<string, Parameter<string>> outputfiles = Names.LoadTable(LocalOutput.PNEToutputsites, null, AssignOutputFiles.ParameterNames.AllNames, true);
                AssignOutputFiles.MapCells(outputfiles, ref SiteOutputNames);
            }
        }
        //---------------------------------------------------------------------
        public override void Initialize()
        {
            PlugIn.ModelCore.UI.WriteLine("Initializing " + Names.ExtensionName + " version " + typeof(PlugIn).Assembly.GetName().Version);
            Cohort.DeathEvent += DeathEvent;
            Globals.InitializeCore(ModelCore, ((Parameter<ushort>)Names.GetParameter(Names.IMAX)).Value);
            EcoregionData.Initialize();
            SiteVars.Initialize();

            Landis.Utilities.Directory.EnsureExists("output");

            Timestep = ((Parameter<int>)Names.GetParameter(Names.Timestep)).Value;
            Parameter<string> CohortBinSizeParm = null;
            if (Names.TryGetParameter(Names.CohortBinSize, out CohortBinSizeParm))
            {
                if (Int32.TryParse(CohortBinSizeParm.Value, out CohortBinSize))
                {
                    if(CohortBinSize < Timestep)
                    {
                        throw new System.Exception("CohortBinSize cannot be smaller than Timestep.");
                    }
                    else
                        PlugIn.ModelCore.UI.WriteLine("  Succession timestep = " + Timestep + "; CohortBinSize = " + CohortBinSize + ".");
                }
                else
                {
                    throw new System.Exception("CohortBinSize is not an integer value.");
                }
            }
            else
                CohortBinSize = Timestep;

            string Parallel = ((Parameter<string>)Names.GetParameter(Names.Parallel)).Value;
            if (Parallel == "false")
            {
                ParallelThreads = 1;
                PlugIn.ModelCore.UI.WriteLine("  MaxParallelThreads = " + ParallelThreads.ToString() + ".");
            }
            else if (Parallel == "true")
            {
                ParallelThreads = -1;
                PlugIn.ModelCore.UI.WriteLine("  MaxParallelThreads determined by system.");
            }
            else
            {
                if (Int32.TryParse(Parallel, out ParallelThreads))
                {
                    if (ParallelThreads < 1)
                    {
                        throw new System.Exception("Parallel cannot be < 1.");
                    }
                    else
                    {
                        PlugIn.ModelCore.UI.WriteLine("  MaxParallelThreads = " + ParallelThreads.ToString() + ".");
                    }
                }else
                {
                    throw new System.Exception("Parallel must be 'true', 'false' or an integer >= 1.");
                }
            }
            this.ThreadCount = ParallelThreads;

            FTimeStep = 1.0F / Timestep;
            if(!Names.TryGetParameter(Names.ClimateConfigFile, out var climateLibraryFileName))
            {
                PlugIn.ModelCore.UI.WriteLine($"  No ClimateConfigFile provided. Using climate files in ecoregion parameters: {Names.parameters["EcoregionParameters"].Value}.");
                ObservedClimate.Initialize();
            }
            SpeciesPnET = new SpeciesPnET();
            Landis.Library.PnETCohorts.SpeciesParameters.LoadParameters(SpeciesPnET);

            Hydrology.Initialize();
            SiteCohorts.Initialize();
            string PARunits = ((Parameter<string>)Names.GetParameter(Names.PARunits)).Value;
            if (PARunits != "umol" && PARunits != "W/m2")
            {
                throw new System.Exception("PARunits are not 'umol' or 'W/m2'.");
            }
            string ETMethod = ((Parameter<string>)Names.GetParameter(Names.ETMethod)).Value;
            if (ETMethod != "Original" && ETMethod != "Radiation" && ETMethod != "WATER" && ETMethod != "WEPP")
            {
                throw new System.Exception("ETMethod is not 'Original' or 'Radiation' or 'WATER' or 'WEPP'.");
            }
            StartDate = new DateTime(((Parameter<int>)Names.GetParameter(Names.StartYear)).Value, 1, 15);
            InitializeClimateLibrary(StartDate.Year); // John McNabb: initialize climate library after EcoregionPnET has been initialized
            //EstablishmentProbability.Initialize(Timestep);  // Not used

            // Initialize Reproduction routines:
            Reproduction.SufficientResources = SufficientResources;
            Reproduction.Establish = Establish;
            Reproduction.AddNewCohort = AddNewCohort;
            Reproduction.MaturePresent = MaturePresent;
            Reproduction.PlantingEstablish = PlantingEstablish;
            SeedingAlgorithms SeedAlgorithm = (SeedingAlgorithms)Enum.Parse(typeof(SeedingAlgorithms), Names.parameters["SeedingAlgorithm"].Value);
            base.Initialize(ModelCore, SeedAlgorithm);
             
            

            PlugIn.ModelCore.UI.WriteLine("Spinning up biomass or reading from maps...");

            string InitialCommunitiesTXTFile = Names.GetParameter(Names.InitialCommunities).Value;
            string InitialCommunitiesMapFile = Names.GetParameter(Names.InitialCommunitiesMap).Value;
            InitialCommunitiesSpinup = Names.GetParameter(Names.InitialCommunitiesSpinup).Value;
            MinFolRatioFactor = ((Parameter<float>)Names.GetParameter(Names.MinFolRatioFactor,0,float.MaxValue)).Value;
            Parameter<string> LitterMapFile;
            bool litterMapFile = Names.TryGetParameter(Names.LitterMap, out LitterMapFile);
            Parameter<string> WoodyDebrisMapFile;
            bool woodyDebrisMapFile = Names.TryGetParameter(Names.WoodyDebrisMap, out WoodyDebrisMapFile);
            InitializeSites(InitialCommunitiesTXTFile, InitialCommunitiesMapFile, ModelCore);
            if(litterMapFile)
                MapReader.ReadLitterFromMap(LitterMapFile.Value);
            if(woodyDebrisMapFile)
                MapReader.ReadWoodyDebrisFromMap(WoodyDebrisMapFile.Value);

            // Convert PnET cohorts to biomasscohorts
            ISiteVar<Landis.Library.BiomassCohorts.ISiteCohorts> biomassCohorts = PlugIn.ModelCore.Landscape.NewSiteVar<Landis.Library.BiomassCohorts.ISiteCohorts>();
            
            foreach (ActiveSite site in PlugIn.ModelCore.Landscape)
            {
                biomassCohorts[site] = SiteVars.SiteCohorts[site];
                
                if (SiteVars.SiteCohorts[site] != null && biomassCohorts[site] == null)
                {
                    throw new System.Exception("Cannot convert PnET SiteCohorts to biomass site cohorts");
                }
            }
            ModelCore.RegisterSiteVar(biomassCohorts, "Succession.BiomassCohorts");

            ISiteVar<Landis.Library.AgeOnlyCohorts.ISiteCohorts> AgeCohortSiteVar = PlugIn.ModelCore.Landscape.NewSiteVar<Landis.Library.AgeOnlyCohorts.ISiteCohorts>();
            ISiteVar<ISiteCohorts> PnETCohorts = PlugIn.ModelCore.Landscape.NewSiteVar<ISiteCohorts>();

            foreach (ActiveSite site in PlugIn.ModelCore.Landscape)
            {
                AgeCohortSiteVar[site] = SiteVars.SiteCohorts[site];
                PnETCohorts[site] = SiteVars.SiteCohorts[site];
                SiteVars.FineFuels[site] = SiteVars.Litter[site].Mass;
                IEcoregionPnET ecoregion = EcoregionData.GetPnETEcoregion(PlugIn.ModelCore.Ecoregion[site]);
                IHydrology hydrology = new Hydrology(ecoregion.FieldCap);
                SiteVars.PressureHead[site] = hydrology.PressureHeadTable.CalculateWaterPressure(hydrology.Water, ecoregion.SoilType);
                if (UsingClimateLibrary)
                {
                    SiteVars.ExtremeMinTemp[site] = ((float)Enumerable.Min(Climate.Future_MonthlyData[Climate.Future_MonthlyData.Keys.Min()][ecoregion.Index].MonthlyTemp) - (float)(3.0 * ecoregion.WinterSTD));  
                }
                else
                {
                    SiteVars.ExtremeMinTemp[site] = 999;
                }
            }

            ModelCore.RegisterSiteVar(AgeCohortSiteVar, "Succession.AgeCohorts");
            ModelCore.RegisterSiteVar(PnETCohorts, "Succession.CohortsPnET");
        }
        //---------------------------------------------------------------------
        /// <summary>This must be called after EcoregionPnET.Initialize() has been called</summary>
        private void InitializeClimateLibrary(int startYear = 0)
        {
            // John McNabb: initialize ClimateRegionData after initializing EcoregionPnet
            Parameter<string> climateLibraryFileName;
            UsingClimateLibrary = Names.TryGetParameter(Names.ClimateConfigFile, out climateLibraryFileName);
            if (UsingClimateLibrary)
            {
                PlugIn.ModelCore.UI.WriteLine($"Using climate library: {climateLibraryFileName.Value}.");
                Climate.Initialize(climateLibraryFileName.Value, false, ModelCore, startYear);
                ClimateRegionData.Initialize();
            }
            //else
            //{  
            //    PlugIn.ModelCore.UI.WriteLine($"Using climate files in ecoregion parameters: {Names.parameters["EcoregionParameters"].Value}.");
            //}

            string PARunits = ((Parameter<string>)Names.GetParameter(Names.PARunits)).Value;

            if (PARunits == "umol")
            {
                PlugIn.ModelCore.UI.WriteLine("Using PAR units of umol/m2/s.");
            }
            else if(PARunits == "W/m2")
            {
                PlugIn.ModelCore.UI.WriteLine("Using PAR units of W/m2.");
            }else
            {
                throw new ApplicationException(string.Format("PARunits units are not 'umol' or 'W/m2'"));
            }
        }
        //---------------------------------------------------------------------
        public void AddNewCohort(ISpecies species, ActiveSite site, string reproductionType, double propBiomass = 1.0)
        {
            ISpeciesPnET spc = PlugIn.SpeciesPnET[species];
            bool addCohort = true;
            if (SiteVars.SiteCohorts[site].cohorts.ContainsKey(species))
            {
                // This should deliver only one KeyValuePair
                KeyValuePair<ISpecies, List<Cohort>> i = new List<KeyValuePair<ISpecies, List<Cohort>>>(SiteVars.SiteCohorts[site].cohorts.Where(o => o.Key == species))[0];
                List<Cohort> Cohorts = new List<Cohort>(i.Value.Where(o => o.Age < CohortBinSize));
                if (Cohorts.Count() > 0)
                {
                    addCohort = false;
                }
            }
            bool addSiteOutput = false;
            addSiteOutput = (SiteOutputNames.ContainsKey(site) && addCohort);
            Cohort cohort = new Cohort(species, spc, (ushort)Date.Year, (addSiteOutput) ? SiteOutputNames[site] : null, propBiomass, false);
            if (((Parameter<bool>)Names.GetParameter(Names.CohortStacking)).Value)
            {
                cohort.CanopyGrowingSpace = 1.0f;
                cohort.CanopyLayerProp = 1.0f;
            }
            
            addCohort = SiteVars.SiteCohorts[site].AddNewCohort(cohort);

            if (addCohort)
            {
                if (reproductionType == "plant")
                {
                    if (!SiteVars.SiteCohorts[site].SpeciesEstablishedByPlant.Contains(species))
                        SiteVars.SiteCohorts[site].SpeciesEstablishedByPlant.Add(species);
                }
                else if (reproductionType == "serotiny")
                {
                    if (!SiteVars.SiteCohorts[site].SpeciesEstablishedBySerotiny.Contains(species))
                        SiteVars.SiteCohorts[site].SpeciesEstablishedBySerotiny.Add(species);
                }
                else if (reproductionType == "resprout")
                {
                    if (!SiteVars.SiteCohorts[site].SpeciesEstablishedByResprout.Contains(species))
                        SiteVars.SiteCohorts[site].SpeciesEstablishedByResprout.Add(species);
                }
                else if (reproductionType == "seed")
                {
                    if (!SiteVars.SiteCohorts[site].SpeciesEstablishedBySeed.Contains(species))
                        SiteVars.SiteCohorts[site].SpeciesEstablishedBySeed.Add(species);
                }

                // Recalculate BiomassLayerProp for layer 0 after adding new cohort?? Should only apply to biomass
            }
        }
        //---------------------------------------------------------------------
        public bool MaturePresent(ISpecies species, ActiveSite site)
        {
            bool IsMaturePresent = SiteVars.SiteCohorts[site].IsMaturePresent(species);
            return IsMaturePresent;
        }
        //---------------------------------------------------------------------
        protected override void InitializeSite(ActiveSite site)//,ICommunity initialCommunity)
        {
            lock (threadLock)
            {
                if (m == null)
                {
                    m = new MyClock(PlugIn.ModelCore.Landscape.ActiveSiteCount);
                }

                m.Next();
                m.WriteUpdate();
            }

            uint key = 0;
            allKeys.TryGetValue(site, out key);

            ICommunity initialCommunity = null;

            if (!sitesAndCommunities.TryGetValue(site, out initialCommunity))
            {
                throw new ApplicationException(string.Format("Unable to retrieve initialCommunity for site: {0}", site.Location.Row + "," + site.Location.Column));
            }

            if (!SiteCohorts.InitialSitesContainsKey(key))
            {
                // Create new sitecohorts from scratch
                SiteVars.SiteCohorts[site] = new SiteCohorts(StartDate, site, initialCommunity, UsingClimateLibrary, PlugIn.InitialCommunitiesSpinup, MinFolRatioFactor, SiteOutputNames.ContainsKey(site) ? SiteOutputNames[site] : null);
            }
            else
            {
                // Create new sitecohorts using initialcommunities data
                SiteVars.SiteCohorts[site] = new SiteCohorts(StartDate, site, initialCommunity, SiteOutputNames.ContainsKey(site) ? SiteOutputNames[site] : null);
            }
        }
        //---------------------------------------------------------------------
        public override void InitializeSites(string initialCommunitiesText, string initialCommunitiesMap, ICore modelCore)
        {
            ModelCore.UI.WriteLine("   Loading initial communities from file \"{0}\" ...", initialCommunitiesText);
            Landis.Library.InitialCommunities.DatasetParser parser = new Landis.Library.InitialCommunities.DatasetParser(CohortBinSize, ModelCore.Species);

            //Landis.Library.InitialCommunities.DatasetParser parser = new Landis.Library.InitialCommunities.DatasetParser(Timestep, ModelCore.Species);
            Landis.Library.InitialCommunities.IDataset communities = Landis.Data.Load<Landis.Library.InitialCommunities.IDataset>(initialCommunitiesText, parser);

            List<ActiveSite> processFirst = new List<ActiveSite>();
            List<ActiveSite> processSecond = new List<ActiveSite>();

            ModelCore.UI.WriteLine("   Reading initial communities map \"{0}\" ...", initialCommunitiesMap);
            ProcessInitialCommunitiesMap(initialCommunitiesMap, communities, ref processFirst, ref processSecond);

            if (this.ThreadCount != 1)
            {
                // Handle creation of initial community sites first
                Parallel.ForEach(processFirst, new ParallelOptions { MaxDegreeOfParallelism = this.ThreadCount }, site =>
                {
                    InitializeSite(site);
                });

                Parallel.ForEach(processSecond, new ParallelOptions { MaxDegreeOfParallelism = this.ThreadCount }, site =>
                {
                    InitializeSite(site);
                });
            }
            else
            {
                // First, process sites so that the initial communities are set up
                foreach (ActiveSite site in processFirst)
                {
                    InitializeSite(site);
                }

                foreach (ActiveSite site in processSecond)
                {
                    InitializeSite((ActiveSite)site);
                }
            }
        }
        //---------------------------------------------------------------------
        protected override void AgeCohorts(ActiveSite site,
                                            ushort years,
                                            int? successionTimestep)                                            
        {
            // Date starts at 1/15/Year
            DateTime date = new DateTime(PlugIn.StartDate.Year + PlugIn.ModelCore.CurrentTime - Timestep, 1, 15);

            DateTime EndDate = date.AddYears(years);

            IEcoregionPnET ecoregion_pnet = EcoregionData.GetPnETEcoregion(PlugIn.ModelCore.Ecoregion[site]);

            List<IEcoregionPnETVariables> climate_vars = UsingClimateLibrary ? EcoregionData.GetClimateRegionData(ecoregion_pnet, date, EndDate, Climate.Phase.Future_Climate) : EcoregionData.GetData(ecoregion_pnet, date, EndDate);

            SiteVars.SiteCohorts[site].Grow(climate_vars);
            SiteVars.SiteCohorts[site].DisturbanceTypesReduced.Clear();

            Date = EndDate;
        }
        //---------------------------------------------------------------------
        // Required function - not used within PnET-Succession
        public override byte ComputeShade(ActiveSite site)
        {
            return 0;
        }
        //---------------------------------------------------------------------
        public override void Run()
        {
            base.Run();
        }
        //---------------------------------------------------------------------
        // Does not seem to be used
        /*public void AddLittersAndCheckResprouting(object sender, Landis.Library.AgeOnlyCohorts.DeathEventArgs eventArgs)
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
        }*/
        //---------------------------------------------------------------------
        // This is a Delegate method to base succession.
        // Not used within PnET-Succession
        public bool SufficientResources(ISpecies species, ActiveSite site)
        {
            return true;
        }
        //---------------------------------------------------------------------
        /// <summary>
        /// Determines if a species can establish on a site.
        /// This is a Delegate method to base succession.
        /// </summary>
        public bool Establish(ISpecies species, ActiveSite site)
        {
            ISpeciesPnET spc = PlugIn.SpeciesPnET[species];

            bool Establish = SiteVars.SiteCohorts[site].EstablishmentProbability.HasEstablished(spc);
            return Establish;
        }
        //---------------------------------------------------------------------
        /// <summary>
        /// Determines if a species can be planted on a site (all conditions are satisfied).
        /// This is a Delegate method to base succession.
        /// </summary>
        public bool PlantingEstablish(ISpecies species, ActiveSite site)
        {
            return true;
        }
        //---------------------------------------------------------------------

        //---------------------------------------------------------------------
        /// <summary>
        /// Reads the initial communities map, finds all unique site keys, and sets aside sites to process first and second
        /// </summary>
        private void ProcessInitialCommunitiesMap(string initialCommunitiesMap, 
            Landis.Library.InitialCommunities.IDataset communities, ref List<ActiveSite> processFirst,
            ref List<ActiveSite> processSecond)
        {
            IInputRaster<uintPixel> map = ModelCore.OpenRaster<uintPixel>(initialCommunitiesMap);
            Dictionary<uint, ActiveSite> uniqueKeys = new Dictionary<uint, ActiveSite>();

            using (map)
            {
                uintPixel pixel = map.BufferPixel;
                foreach (Site site in ModelCore.Landscape.AllSites)
                {
                    map.ReadBufferPixel();
                    uint mapCode = pixel.MapCode.Value;
                    if (!site.IsActive)
                        continue;

                    ActiveSite activeSite = (ActiveSite)site;
                    var initialCommunity = communities.Find(mapCode);
                    if (initialCommunity == null)
                    {
                        throw new ApplicationException(string.Format("Unknown map code for initial community: {0}", mapCode));
                    }

                    sitesAndCommunities.Add(activeSite, initialCommunity);
                    uint key = SiteCohorts.ComputeKey((ushort)initialCommunity.MapCode, Globals.ModelCore.Ecoregion[site].MapCode);

                    if (!uniqueKeys.ContainsKey(key))
                    {
                        uniqueKeys.Add(key, activeSite);
                        processFirst.Add(activeSite);
                    }
                    else
                    {
                        processSecond.Add(activeSite);
                    }

                    if (!allKeys.ContainsKey(activeSite))
                    {
                        allKeys.Add(activeSite, key);
                    }
                }
            }
        }
        //---------------------------------------------------------------------
    }
}


