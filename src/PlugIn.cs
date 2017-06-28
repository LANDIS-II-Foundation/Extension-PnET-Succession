//  Copyright 2005-2010 Portland State University, University of Wisconsin
//  Authors:  Arjan de Bruijn

using Landis.Core;
using Landis.SpatialModeling;
using Landis.Library.Succession;
using Landis.Library.InitialCommunities;
using System.Collections.Generic;
using Edu.Wisc.Forest.Flel.Util;
using System;
using Landis.Library;
using System.Linq;
using Landis.Library.Parameters.Species;
 

namespace Landis.Extension.Succession.BiomassPnET
{
    public class PlugIn  : Landis.Library.Succession.ExtensionBase 
    {
        public static SpeciesPnET SpeciesPnET;
        //public static float Latitude;// Now an ecoregion parameter
        public static ISiteVar<Landis.Library.Biomass.Pool> WoodyDebris;
        public static ISiteVar<Landis.Library.Biomass.Pool> Litter;
        public static DateTime Date;
        public static ICore ModelCore;
        private static ISiteVar<SiteCohorts> sitecohorts;
        private static DateTime StartDate;
        private static Dictionary<ActiveSite, string> SiteOutputNames;
        public static ushort IMAX;
        //public static float PrecipEvents;// Now an ecoregion parameter
        
        
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



            //---------------AgeOnlyDisturbancesParameterFile
            Parameter<string> AgeOnlyDisturbancesParameterFile;
            if (TryGetParameter(Names.AgeOnlyDisturbances, out AgeOnlyDisturbancesParameterFile))
            {
                Allocation.Initialize(AgeOnlyDisturbancesParameterFile.Value,  parameters);
                Cohort.AgeOnlyDeathEvent += Events.CohortDied;
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
        public static float FTimeStep;

        public override void Initialize()
        {
            PlugIn.ModelCore.UI.WriteLine("Initializing " + Names.ExtensionName + " version " + typeof(PlugIn).Assembly.GetName().Version);

            Cohort.DeathEvent += DeathEvent;

            Litter = PlugIn.ModelCore.Landscape.NewSiteVar<Landis.Library.Biomass.Pool>();
            WoodyDebris = PlugIn.ModelCore.Landscape.NewSiteVar<Landis.Library.Biomass.Pool>();
            sitecohorts = PlugIn.ModelCore.Landscape.NewSiteVar<SiteCohorts>();
            Edu.Wisc.Forest.Flel.Util.Directory.EnsureExists("output");

            Timestep = ((Parameter<int>)GetParameter(Names.Timestep)).Value;

            FTimeStep = 1.0F / Timestep;

            //Latitude = ((Parameter<float>)PlugIn.GetParameter(Names.Latitude, 0, 90)).Value; // Now an ecoregion parameter

            
            ObservedClimate.Initialize();

            SpeciesPnET = new SpeciesPnET();
             
            EcoregionPnET.Initialize();
            Hydrology.Initialize();
            SiteCohorts.Initialize();
            
            EstablishmentProbability.Initialize(Timestep);
            
            IMAX = ((Parameter<ushort>)GetParameter(Names.IMAX)).Value;
            //PrecipEvents = ((Parameter<float>)GetParameter(Names.PrecipEvents)).Value;// Now an ecoregion parameter
          

            // Initialize Reproduction routines:
            Reproduction.SufficientResources = SufficientResources;
            Reproduction.Establish = Establish;
            Reproduction.AddNewCohort = AddNewCohort;
            Reproduction.MaturePresent = MaturePresent;
            Reproduction.PlantingEstablish = PlantingEstablish;
            Reproduction.EstablishmentProbability = EstabProbability;
            Reproduction.MatureBiomass = ComputeMatureBiomass;
            Reproduction.ActiveBiomass = ComputeActiveBiomass;

            
            SeedingAlgorithms SeedAlgorithm = (SeedingAlgorithms)Enum.Parse(typeof(SeedingAlgorithms), parameters["SeedingAlgorithm"].Value);
            
            base.Initialize(ModelCore, SeedAlgorithm);
             
             
            StartDate = new DateTime(((Parameter<int>)GetParameter(Names.StartYear)).Value, 1, 15);

            PlugIn.ModelCore.UI.WriteLine("Spinning up biomass");

            string InitialCommunitiesTXTFile = GetParameter(Names.InitialCommunities).Value;
            string InitialCommunitiesMapFile = GetParameter(Names.InitialCommunitiesMap).Value;
            InitializeSites(InitialCommunitiesTXTFile, InitialCommunitiesMapFile, ModelCore);
             
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
              
            foreach (ActiveSite site in PlugIn.ModelCore.Landscape)
            {
                AgeCohortSiteVar[site] = sitecohorts[site];
            }

            ModelCore.RegisterSiteVar(AgeCohortSiteVar, "Succession.AgeCohorts");

            ISiteVar<ISiteCohorts> PnETCohorts = PlugIn.ModelCore.Landscape.NewSiteVar<ISiteCohorts>();

            foreach (ActiveSite site in PlugIn.ModelCore.Landscape)
            {
                PnETCohorts[site] = sitecohorts[site];
            }

            ModelCore.RegisterSiteVar(PnETCohorts, "Succession.CohortsPnET");
            
            
             
        }
  
         
        public void AddNewCohort(ISpecies species, ActiveSite site,double propBiomass)
        {
            ISpeciesPNET spc = PlugIn.SpeciesPnET[species];
            Cohort cohort = new Cohort(spc, (ushort)Date.Year, (SiteOutputNames.ContainsKey(site)) ? SiteOutputNames[site] : null, propBiomass);
            
            sitecohorts[site].AddNewCohort(cohort);

        }
        public bool MaturePresent(ISpecies species, ActiveSite site)
        {
            bool IsMaturePresent = sitecohorts[site].IsMaturePresent(species);
            return IsMaturePresent;
        }
        protected override void InitializeSite(ActiveSite site,
                                               ICommunity initialCommunity)
        {
            if (m == null)
            {
                m = new MyClock(PlugIn.ModelCore.Landscape.ActiveSiteCount);
            }

            m.Next();
            m.WriteUpdate();

             // Create new sitecohorts
            sitecohorts[site] = new SiteCohorts(StartDate,site,initialCommunity, SiteOutputNames.ContainsKey(site)? SiteOutputNames[site] :null);

           
           
        }
        protected override void AgeCohorts(ActiveSite site,
                                            ushort years,
                                            int? successionTimestep
                                            )
        {
            DateTime date = new DateTime(PlugIn.StartDate.Year + PlugIn.ModelCore.CurrentTime - Timestep, 1, 15);

            DateTime EndDate = date.AddYears(years);

            IEcoregionPnET ecoregion_pnet = EcoregionPnET.GetPnETEcoregion(PlugIn.ModelCore.Ecoregion[site]);

            List<IEcoregionPnETVariables> climate_vars = EcoregionPnET.GetData(ecoregion_pnet, date, EndDate);

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

        public double EstabProbability(ISpecies species, ActiveSite site)
        {
            if (PlugIn.ModelCore.CurrentTime <= 0)
                return 1.0;
            else
            {
                ISpeciesPNET spc = PlugIn.SpeciesPnET[species];
                return sitecohorts[site].EstablishmentProbability.Probability[species];
            }
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
        public double ComputeMatureBiomass(ISpecies species,ActiveSite site)
        {
            if (PlugIn.ModelCore.CurrentTime <= 0)
                return 0.0;
            else
            {
                ISiteCohorts mySiteCohorts = sitecohorts[site];
                if (mySiteCohorts.CohortCountPerSpecies[species] > 0)
                {
                    double matureBiomass = mySiteCohorts.MatureBiomassPerSpecies[species];
                    return matureBiomass;
                }
                else
                    return 0.0;
            }
            
        }
        //---------------------------------------------------------------------
        public double ComputeActiveBiomass(ISpecies species, ActiveSite site)
        {
            if (PlugIn.ModelCore.CurrentTime <= 0)
                return 0.0;
            else
            {
                ISiteCohorts mySiteCohorts = sitecohorts[site];
                if (mySiteCohorts.CohortCountPerSpecies[species] > 0)
                {
                    double activeBiomass = mySiteCohorts.ActiveBiomassPerSpecies[species];
                    return activeBiomass;
                }
                else
                    return 0.0;
            }

        }
    }
}


