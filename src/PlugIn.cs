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
using Landis.Library.InitialCommunities;
using Landis.Library.Succession;
using Landis.SpatialModeling;
using Landis.Library.Climate;
using System;
using System.Collections.Generic;
using System.Linq;


namespace Landis.Extension.Succession.BiomassPnET
{
    public class PlugIn  : Landis.Library.Succession.ExtensionBase 
    {
        public static SpeciesPnET SpeciesPnET;
        //public static float Latitude;// Now an ecoregion parameter
        public static ISiteVar<Landis.Library.Biomass.Pool> WoodyDebris;
        public static ISiteVar<Landis.Library.Biomass.Pool> Litter;
        public static ISiteVar<Double> FineFuels;
        public static ISiteVar<float> PressureHead;
        public static ISiteVar<float> ExtremeMinTemp;
        public static DateTime Date;
        public static ICore ModelCore;
        private static ISiteVar<SiteCohorts> sitecohorts;
        private static DateTime StartDate;
        private static Dictionary<ActiveSite, string> SiteOutputNames;
        public static ushort IMAX;
        public static float FTimeStep;

        public static bool UsingClimateLibrary;
        private ICommunity initialCommunity;
        public static int CohortBinSize;

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

        public void DeathEvent(object sender, Landis.Library.BiomassCohorts.DeathEventArgs eventArgs)
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
       
        string PnETDefaultsFolder
        {
            get
            {
                return System.IO.Path.Combine(System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location), "Defaults");
            }
        }
        
        public PlugIn()
            : base(Names.ExtensionName)
        {
            LocalOutput.PNEToutputsites = Names.PNEToutputsites;
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

            parameters.Add(Names.ExtensionName, new Parameter<string>(Names.ExtensionName, InputParameterFile));

            //-------------PnET-Succession input files
            Dictionary<string, Parameter<string>> InputParameters = LoadTable(Names.ExtensionName, Names.AllNames, null, true);
            InputParameters.ToList().ForEach(x => parameters.Add(x.Key, x.Value));

            //-------------Read Species parameters input file
            List<string> SpeciesNames = PlugIn.ModelCore.Species.ToList().Select(x => x.Name).ToList();
            List<string> SpeciesPars = SpeciesPnET.ParameterNames;
            SpeciesPars.Add(Names.PnETSpeciesParameters);
            Dictionary<string, Parameter<string>> speciesparameters = LoadTable(Names.PnETSpeciesParameters, SpeciesNames, SpeciesPars);
            foreach (string key in speciesparameters.Keys)
            {
                if (parameters.ContainsKey(key)) throw new System.Exception("Parameter " + key + " was provided twice");
            }
            speciesparameters.ToList().ForEach(x => parameters.Add(x.Key, x.Value));

            //-------------Ecoregion parameters
            List<string> EcoregionNames = PlugIn.ModelCore.Ecoregions.ToList().Select(x => x.Name).ToList();
            List<string> EcoregionParameters = EcoregionPnET.ParameterNames;
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

             
            //---------------SaxtonAndRawlsParameterFile
            if (parameters.ContainsKey(PressureHeadSaxton_Rawls.SaxtonAndRawlsParameters) == false)
            {
                Parameter<string> SaxtonAndRawlsParameterFile = new Parameter<string>(PressureHeadSaxton_Rawls.SaxtonAndRawlsParameters, (string)PnETDefaultsFolder + "\\SaxtonAndRawlsParameters.txt");
                parameters.Add(PressureHeadSaxton_Rawls.SaxtonAndRawlsParameters, SaxtonAndRawlsParameterFile);
            }
            Dictionary<string, Parameter<string>> SaxtonAndRawlsParameters = LoadTable(PressureHeadSaxton_Rawls.SaxtonAndRawlsParameters, null, PressureHeadSaxton_Rawls.ParameterNames);
            foreach (string key in SaxtonAndRawlsParameters.Keys)
            {
                if (parameters.ContainsKey(key)) throw new System.Exception("Parameter " + key + " was provided twice");
            }
            SaxtonAndRawlsParameters.ToList().ForEach(x => parameters.Add(x.Key, x.Value));

            //--------------PnETGenericParameterFile

            //----------See if user supplied overwriting default parameters
            List<string> RowLabels = new List<string>(Names.AllNames);
            RowLabels.AddRange(SpeciesPnET.ParameterNames); 

            if (parameters.ContainsKey(Names.PnETGenericParameters))
            {
                Dictionary<string, Parameter<string>> genericparameters = LoadTable(Names.PnETGenericParameters,  RowLabels, null, true);
                foreach (KeyValuePair<string, Parameter<string>> par in genericparameters)
                {
                    if (parameters.ContainsKey(par.Key)) throw new System.Exception("Parameter " + par.Key + " was provided twice");
                    parameters.Add(par.Key, par.Value);
                }
            }

            //----------Load in default parameters to fill the gaps
            Parameter<string> PnETGenericDefaultParameterFile = new Parameter<string>(Names.PnETGenericDefaultParameters, (string)PnETDefaultsFolder + "\\PnETGenericDefaultParameters.txt");
            parameters.Add(Names.PnETGenericDefaultParameters, PnETGenericDefaultParameterFile);
            Dictionary<string, Parameter<string>> genericdefaultparameters = LoadTable(Names.PnETGenericDefaultParameters, RowLabels, null, true);

            foreach (KeyValuePair<string, Parameter<string>> par in genericdefaultparameters)
            {
                if (parameters.ContainsKey(par.Key) == false)
                {
                    parameters.Add(par.Key, par.Value);
                }
            }

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

            Litter = PlugIn.ModelCore.Landscape.NewSiteVar<Landis.Library.Biomass.Pool>();
            WoodyDebris = PlugIn.ModelCore.Landscape.NewSiteVar<Landis.Library.Biomass.Pool>();
            sitecohorts = PlugIn.ModelCore.Landscape.NewSiteVar<SiteCohorts>();
            FineFuels = ModelCore.Landscape.NewSiteVar<Double>();
            PressureHead = ModelCore.Landscape.NewSiteVar<float>();
            ExtremeMinTemp = ModelCore.Landscape.NewSiteVar<float>();
            Landis.Utilities.Directory.EnsureExists("output");

            Timestep = ((Parameter<int>)GetParameter(Names.Timestep)).Value;
            Parameter<string> CohortBinSizeParm = null;
            if (TryGetParameter(Names.CohortBinSize, out CohortBinSizeParm))
            {
                if (Int32.TryParse(CohortBinSizeParm.Value, out CohortBinSize))
                {
                    if(CohortBinSize < Timestep)
                    {
                        throw new System.Exception("CohortBinSize cannot be smaller than Timestep.");
                    }
                    else
                        PlugIn.ModelCore.UI.WriteLine("Succession timestep = " + Timestep + "; CohortBinSize = " + CohortBinSize + ".");
                }
                else
                {
                    throw new System.Exception("CohortBinSize is not an integer value.");
                }
            }
            else
                CohortBinSize = Timestep;

        
                FTimeStep = 1.0F / Timestep;

            //Latitude = ((Parameter<float>)PlugIn.GetParameter(Names.Latitude, 0, 90)).Value; // Now an ecoregion parameter

            ObservedClimate.Initialize();

            SpeciesPnET = new SpeciesPnET();
             
            EcoregionPnET.Initialize();
            Hydrology.Initialize();
            SiteCohorts.Initialize();
 
            // John McNabb: initialize climate library after EcoregionPnET has been initialized
            InitializeClimateLibrary();

            EstablishmentProbability.Initialize(Timestep);
            
            IMAX = ((Parameter<ushort>)GetParameter(Names.IMAX)).Value;
            //LeakageFrostDepth = ((Parameter<float>)GetParameter(Names.LeakageFrostDepth)).Value; //Now an ecoregion parameter
            //PrecipEvents = ((Parameter<float>)GetParameter(Names.PrecipEvents)).Value;// Now an ecoregion parameter
          

            // Initialize Reproduction routines:
            Reproduction.SufficientResources = SufficientResources;
            Reproduction.Establish = Establish;
            Reproduction.AddNewCohort = AddNewCohort;
            Reproduction.MaturePresent = MaturePresent;
            Reproduction.PlantingEstablish = PlantingEstablish;

            
            SeedingAlgorithms SeedAlgorithm = (SeedingAlgorithms)Enum.Parse(typeof(SeedingAlgorithms), parameters["SeedingAlgorithm"].Value);
            
            base.Initialize(ModelCore, SeedAlgorithm);
             
             
            StartDate = new DateTime(((Parameter<int>)GetParameter(Names.StartYear)).Value, 1, 15);

            PlugIn.ModelCore.UI.WriteLine("Spinning up biomass or reading from maps...");

            string InitialCommunitiesTXTFile = GetParameter(Names.InitialCommunities).Value;
            string InitialCommunitiesMapFile = GetParameter(Names.InitialCommunitiesMap).Value;
            Parameter<string> LitterMapFile;
            bool litterMapFile = TryGetParameter(Names.LitterMap, out LitterMapFile);
            Parameter<string> WoodyDebrisMapFile;
            bool woodyDebrisMapFile = TryGetParameter(Names.WoodyDebrisMap, out WoodyDebrisMapFile);
            //Console.ReadLine();
            InitializeSites(InitialCommunitiesTXTFile, InitialCommunitiesMapFile, ModelCore);
            if(litterMapFile)
                MapReader.ReadLitterFromMap(LitterMapFile.Value);
            if(woodyDebrisMapFile)
                MapReader.ReadWoodyDebrisFromMap(WoodyDebrisMapFile.Value);

            // Convert PnET cohorts to biomasscohorts
            ISiteVar<Landis.Library.BiomassCohorts.ISiteCohorts> biomassCohorts = PlugIn.ModelCore.Landscape.NewSiteVar<Landis.Library.BiomassCohorts.ISiteCohorts>();
            
            
            foreach (ActiveSite site in PlugIn.ModelCore.Landscape)
            {
                biomassCohorts[site] = sitecohorts[site];
                
                if (sitecohorts[site] != null && biomassCohorts[site] == null)
                {
                    throw new System.Exception("Cannot convert PnET SiteCohorts to biomass site cohorts");
                }
            }
            ModelCore.RegisterSiteVar(biomassCohorts, "Succession.BiomassCohorts");
            ModelCore.RegisterSiteVar(WoodyDebris, "Succession.WoodyDebris");
            ModelCore.RegisterSiteVar(Litter, "Succession.Litter");
            

            ISiteVar<Landis.Library.AgeOnlyCohorts.ISiteCohorts> AgeCohortSiteVar = PlugIn.ModelCore.Landscape.NewSiteVar<Landis.Library.AgeOnlyCohorts.ISiteCohorts>();
            ISiteVar<ISiteCohorts> PnETCohorts = PlugIn.ModelCore.Landscape.NewSiteVar<ISiteCohorts>();
          

            foreach (ActiveSite site in PlugIn.ModelCore.Landscape)
            {
                AgeCohortSiteVar[site] = sitecohorts[site];
                PnETCohorts[site] = sitecohorts[site];
                FineFuels[site] = Litter[site].Mass;
                IEcoregionPnET ecoregion = EcoregionPnET.GetPnETEcoregion(PlugIn.ModelCore.Ecoregion[site]);
                IHydrology hydrology = new Hydrology(ecoregion.FieldCap);
                PressureHead[site] = hydrology.GetPressureHead(ecoregion);
                if (UsingClimateLibrary)
                {
                    ExtremeMinTemp[site] = ((float)Enumerable.Min(Climate.Future_MonthlyData[Climate.Future_MonthlyData.Keys.Min()][ecoregion.Index].MonthlyTemp) - (float)(3.0 * ecoregion.WinterSTD));  
                }
                else
                {
                    ExtremeMinTemp[site] = 999;
                }
            }

            ModelCore.RegisterSiteVar(AgeCohortSiteVar, "Succession.AgeCohorts");
            ModelCore.RegisterSiteVar(PnETCohorts, "Succession.CohortsPnET");
            ModelCore.RegisterSiteVar(FineFuels, "Succession.FineFuels");
            ModelCore.RegisterSiteVar(PressureHead, "Succession.PressureHead");
            ModelCore.RegisterSiteVar(ExtremeMinTemp, "Succession.ExtremeMinTemp");

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

        public void AddNewCohort(ISpecies species, ActiveSite site, string reproductionType)
        {
            ISpeciesPNET spc = PlugIn.SpeciesPnET[species];
            bool addCohort = true;
            if (sitecohorts[site].cohorts.ContainsKey(species))
            {
                // This should deliver only one KeyValuePair
                KeyValuePair<ISpecies, List<Cohort>> i = new List<KeyValuePair<ISpecies, List<Cohort>>>(sitecohorts[site].cohorts.Where(o => o.Key == species))[0];
                List<Cohort> Cohorts = new List<Cohort>(i.Value.Where(o => o.Age < CohortBinSize));
                if (Cohorts.Count() > 0)
                {
                    addCohort = false;
                }
            }
            bool addSiteOutput = false;
            addSiteOutput = (SiteOutputNames.ContainsKey(site) && addCohort);
            Cohort cohort = new Cohort(spc, (ushort)Date.Year, (addSiteOutput) ? SiteOutputNames[site] : null);
            
            addCohort = sitecohorts[site].AddNewCohort(cohort);

            if (addCohort)
            {
                if (reproductionType == "plant")
                {
                    if (!sitecohorts[site].SpeciesEstablishedByPlant.Contains(species))
                        sitecohorts[site].SpeciesEstablishedByPlant.Add(species);
                }
                else if (reproductionType == "serotiny")
                {
                    if (!sitecohorts[site].SpeciesEstablishedBySerotiny.Contains(species))
                        sitecohorts[site].SpeciesEstablishedBySerotiny.Add(species);
                }
                else if (reproductionType == "resprout")
                {
                    if (!sitecohorts[site].SpeciesEstablishedByResprout.Contains(species))
                        sitecohorts[site].SpeciesEstablishedByResprout.Add(species);
                }
                else if (reproductionType == "seed")
                {
                    if (!sitecohorts[site].SpeciesEstablishedBySeed.Contains(species))
                        sitecohorts[site].SpeciesEstablishedBySeed.Add(species);
                }
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
            Landis.Library.InitialCommunities.DatasetParser parser = new Landis.Library.InitialCommunities.DatasetParser(CohortBinSize, ModelCore.Species);

            //Landis.Library.InitialCommunities.DatasetParser parser = new Landis.Library.InitialCommunities.DatasetParser(Timestep, ModelCore.Species);
            Landis.Library.InitialCommunities.IDataset communities = Landis.Data.Load<Landis.Library.InitialCommunities.IDataset>(initialCommunitiesText, parser);

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

            IEcoregionPnET ecoregion_pnet = EcoregionPnET.GetPnETEcoregion(PlugIn.ModelCore.Ecoregion[site]);

            List<IEcoregionPnETVariables> climate_vars = UsingClimateLibrary ? EcoregionPnET.GetClimateRegionData(ecoregion_pnet, date, EndDate, Climate.Phase.Future_Climate) : EcoregionPnET.GetData(ecoregion_pnet, date, EndDate);

            sitecohorts[site].Grow(climate_vars);

            Date = EndDate;
             
        }
        public override byte ComputeShade(ActiveSite site)
        {


            return 0;
        }
        
        public override void Run()
        {
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
        
        public bool SufficientResources(ISpecies species, ActiveSite site)
        {
            return true;
        }

        public bool Establish(ISpecies species, ActiveSite site)
        {
            ISpeciesPNET spc = PlugIn.SpeciesPnET[species];

            bool Establish = sitecohorts[site].EstablishmentProbability.HasEstablished(spc);
            return Establish;
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

    }
}


