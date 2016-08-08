// uses dominance to allocate psn and subtract transpiration from soil water, average cohort vars over layer

using Landis.SpatialModeling;
using Landis.Core;
using Edu.Wisc.Forest.Flel.Util;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Landis.Extension.Succession.BiomassPnET 
{
    public class Cohort : Landis.Library.AgeOnlyCohorts.ICohort, Landis.Library.BiomassCohorts.ICohort 
    {
        public static event Landis.Library.BiomassCohorts.DeathEventHandler<Landis.Library.BiomassCohorts.DeathEventArgs> DeathEvent;
        public static event Landis.Library.BiomassCohorts.DeathEventHandler<Landis.Library.BiomassCohorts.DeathEventArgs> AgeOnlyDeathEvent;

        public byte Layer;

        public delegate void SubtractTranspiration(float transpiration, ISpeciesPNET Species);
        public delegate void AddWoodyDebris(float Litter, float KWdLit);
        public delegate void AddLitter(float AddLitter, ISpeciesPNET Species);

        private bool leaf_on = true;

        public static IEcoregionPnET ecoregion;

        public static AddWoodyDebris addwoodydebris;
        
        public static AddLitter addlitter;
        
        private float biomassmax;
        private float biomass; // root + wood
        private float fol;
        private float nsc;
        private ushort age;
        private float defolProp; //BRM
        private float lastWoodySenescence; // last recorded woody senescence
        private float lastFoliageSenescence; // last recorded foliage senescence

        public ushort index;
        
        private ISpeciesPNET species;
        private LocalOutput cohortoutput;

        // Leaf area index per subcanopy layer (m/m)
        public float[] LAI = null;

        // Gross photosynthesis (gC/mo)
        public float[] GrossPsn = null;

        // Foliar respiration (gC/mo)
        public float[] FolResp = null;

        // Net photosynthesis (gC/mo)
        public float[] NetPsn = null;

        // Mainenance respiration (gC/mo)
        public float[] MaintenanceRespiration = null;

        // Transpiration (mm/mo)
        public float[] Transpiration = null;
        
        // Reduction factor for suboptimal radiation on growth
        public float[] FRad = null;
        
        // Reduction factor for suboptimal or supra optimal water 
        public float[] FWater = null;

        // Reduction factor for ozone 
        public float[] FOzone = null;

        // Interception (mm/mo)
        public float[] Interception = null;


        public void InitializeSubLayers()
        {
            // Initialize subcanopy layers
            index = 0;
            LAI = new float[PlugIn.IMAX];
            GrossPsn = new float[PlugIn.IMAX];
            FolResp = new float[PlugIn.IMAX];
            NetPsn = new float[PlugIn.IMAX];
            Transpiration = new float[PlugIn.IMAX];
            FRad = new float[PlugIn.IMAX];
            FWater = new float[PlugIn.IMAX];
            FOzone = new float[PlugIn.IMAX];
            MaintenanceRespiration = new float[PlugIn.IMAX];
            Interception = new float[PlugIn.IMAX];
        }
        public void NullSubLayers()
        {
            // Reset values for subcanopy layers
            LAI = null;
            GrossPsn = null;
            FolResp = null;
            NetPsn = null;
            Transpiration = null;
            FRad = null;
            FWater = null;
            FOzone = null;
            MaintenanceRespiration = null;
            Interception = null;
        }
      
        public ushort Age
        {
            get
            {
                return age;
            }
        }
        // Non soluble carbons
        public float NSC
        {
            get
            {
                return nsc;
            }
        }
        // Foliage (g/m2)
        public float Fol
        {
            get
            {
                return fol;
            }
        }
        // Aboveground Biomass (g/m2)
        public int Biomass
        {
            get
            {
                return (int)((1 - species.FracBelowG) * biomass) + (int)fol;
            }
        }
        // Total Biomass (root + wood) (g/m2)
        public int TotalBiomass
        {
            get
            {
                return (int)biomass;
            }
        }
        // Wood (g/m2)
        public uint Wood
        {
            get
            {
                return (uint)((1 - species.FracBelowG) * biomass);
            }
        }
        // Root (g/m2)
        public uint Root
        {
            get
            {
                return (uint)(species.FracBelowG * biomass);
            }
        }
        
        // Max biomass achived in the cohorts' life time. 
        // This value remains high after the cohort has reached its 
        // peak biomass. It is used to determine canopy layers where
        // it prevents that a cohort could descent in the canopy when 
        // it declines (g/m2)
        public float BiomassMax
        {
            get
            {
                return biomassmax;
            }
        }
        // Get totals for the
        public void Accumulate(Cohort c)
        {
            biomass += c.biomass;
            biomassmax = Math.Max(biomassmax, biomass);
            fol += c.Fol;
        }

        // Add dead wood to last senescence
        public void AccumulateWoodySenescence (int senescence)
        {
            lastWoodySenescence += senescence;
        }

        // Add dead foliage to last senescence
        public void AccumulateFoliageSenescence(int senescence)
        {
            lastFoliageSenescence += senescence;
        }

        // Growth reduction factor for age
        float Fage
        {
            get
            {
                return Math.Max(0, 1 - (float)Math.Pow((age / (float)species.Longevity), species.PsnAgeRed));
            }
        }
        // NSC fraction: measure for resources
        public float NSCfrac
        {
            get
            {
                return nsc / (FActiveBiom * (biomass + fol));
            }
        }
        // Species with PnET parameter additions
        public ISpeciesPNET SpeciesPNET
        {
            get
            {
                return species;
            }
        }
        // LANDIS species (without PnET parameter additions)
        public Landis.Core.ISpecies Species
        {
            get
            {
                return PlugIn.SpeciesPnET[species];
            }
        }
        // Defoliation proportion - BRM
        public float DefolProp
        {
            get
            {
                return defolProp;
            }
        }

        // Annual Woody Senescence (g/m2)
        public int LastWoodySenescence
        {
            get
            {
                return (int)lastWoodySenescence;
            }
        }
        // Annual Foliage Senescence (g/m2)
        public int LastFoliageSenescence
        {
            get
            {
                return (int)lastFoliageSenescence;
            }
        }

        // Constructor
        public Cohort(ISpeciesPNET species, ushort year_of_birth, string SiteName)
        {
            this.species =  species;
            age = 0; 
           
            this.nsc = (ushort)species.InitialNSC;
           
            // Initialize biomass assuming fixed concentration of NSC
            this.biomass = (uint)(1F / species.DNSC * (ushort)species.InitialNSC);
            
            biomassmax = biomass;

            // Then overwrite them if you need stuff for outputs
            if (SiteName != null)
            {
                InitializeOutput(SiteName, year_of_birth);
            }
        }
        public Cohort(Cohort cohort)
        {
            this.species = cohort.species;
            this.age = cohort.age;
            this.nsc = cohort.nsc;
            this.biomass = cohort.biomass;
            biomassmax = cohort.biomassmax;
            this.fol = cohort.fol;
        }
        // Makes sure that litters are allocated to the appropriate site
        public static void SetSiteAccessFunctions(SiteCohorts sitecohorts)
        {
             Cohort.addlitter = sitecohorts.AddLitter;
             Cohort.addwoodydebris = sitecohorts.AddWoodyDebris;
             Cohort.ecoregion = sitecohorts.Ecoregion;
        }
        

        public void CalculateDefoliation(ActiveSite site, int SiteAboveGroundBiomass)
        {
            int abovegroundBiomass = (int)((1 - species.FracBelowG) * biomass) + (int)fol;
            defolProp = (float)Landis.Library.Biomass.CohortDefoliation.Compute(site, species, abovegroundBiomass, SiteAboveGroundBiomass);
        }

        public bool CalculatePhotosynthesis(float PrecInByCanopyLayer, float LeakagePerCohort, IHydrology hydrology, ref float SubCanopyPar, float o3)
        {
            
            bool success = true;


            // Leaf area index for the subcanopy layer by index. Function of specific leaf weight SLWMAX and the depth of the canopy
            // Depth of the canopy is expressed by the mass of foliage above this subcanopy layer (i.e. slwdel * index/imax *fol)
            LAI[index] = (1 / (float)PlugIn.IMAX) * fol / (species.SLWmax - species.SLWDel * index * (1 / (float)PlugIn.IMAX) * fol);
            
            // Precipitation interception has a max in the upper canopy and decreases exponentially through the canopy
            //Interception[index] = PrecInByCanopyLayer * (float)(1 - Math.Exp(-1 * ecoregion.PrecIntConst * LAI[index]));
            //if (Interception[index] > PrecInByCanopyLayer) throw new System.Exception("Error adding water, PrecInByCanopyLayer = " + PrecInByCanopyLayer + " Interception[index] = " + Interception[index]);

            // Incoming precipitation
            //float waterIn = PrecInByCanopyLayer  - Interception[index]; //mm   
            float waterIn = PrecInByCanopyLayer; //mm 

            // Add incoming precipitation to soil moisture
            success = hydrology.AddWater(waterIn);
            if (success == false) throw new System.Exception("Error adding water, waterIn = " + waterIn + " water = " + hydrology.Water);
           
            // Instantaneous runoff (excess of porosity)
            float runoff = Math.Max(hydrology.Water - ecoregion.Porosity, 0);
            success = hydrology.AddWater(-1 * runoff);
            if (success == false) throw new System.Exception("Error adding water, runoff = " + runoff + " water = " + hydrology.Water);

            // Fast Leakage 
            Hydrology.Leakage = Math.Max(LeakagePerCohort * (hydrology.Water - ecoregion.FieldCap), 0);
            
            // Remove fast leakage
            success = hydrology.AddWater(-1 * Hydrology.Leakage);
            if (success == false) throw new System.Exception("Error adding water, Hydrology.Leakage = " + Hydrology.Leakage + " water = " + hydrology.Water);

            // Maintenance respiration depends on biomass,  non soluble carbon and temperature
            MaintenanceRespiration[index] = (1 / (float)PlugIn.IMAX) * (float)Math.Min(NSC, ecoregion.Variables[Species.Name].MaintRespFTempResp * biomass);//gC //IMAXinverse
            
            // Subtract mainenance respiration (gC/mo)
            nsc -= MaintenanceRespiration[index];

            // Woody decomposition: do once per year to reduce unnescessary computation time so with the last subcanopy layer 
            if (index == PlugIn.IMAX - 1)
            {
                // In the first month
                if (ecoregion.Variables.Month == (int)Constants.Months.January)
                {
                    float woodSenescence = Senescence();
                    addwoodydebris(woodSenescence, species.KWdLit);
                    lastWoodySenescence = woodSenescence;

                    // Release of nsc, will be added to biomass components next year
                    // Assumed that NSC will have a minimum concentration, excess is allocated to biomass
                    float Allocation = Math.Max(nsc - (species.DNSC * FActiveBiom * biomass), 0);
                    biomass += Allocation;
                    biomassmax = Math.Max(biomassmax, biomass);
                    nsc -= Allocation;

                    age++;
                }
            }
            
            // When LeafOn becomes false for the first time in a year
            if(ecoregion.Variables.Tmin < this.SpeciesPNET.PsnTMin)
            {
                if (leaf_on == true)
                {
                    leaf_on = false;
                    float foliageSenescence = FoliageSenescence();
                    addlitter(foliageSenescence, SpeciesPNET);
                    lastFoliageSenescence = foliageSenescence;
                }
            }
            else  
            {
                leaf_on = true;
            }

            if (leaf_on)
            {
                // Foliage linearly increases with active biomass
                float IdealFol = (species.FracFol * FActiveBiom * biomass);

                // If the tree should have more filiage than it currently has
                if (IdealFol > fol)
                {
                    // Foliage allocation depends on availability of NSC (allows deficit at this time so no min nsc)
                    // carbon fraction of biomass to convert C to DW
                    float Folalloc = Math.Max(0, Math.Min(nsc, species.CFracBiomass * (IdealFol - fol))); // gC/mo

                    // Add foliage allocation to foliage
                    fol += Folalloc / species.CFracBiomass;// gDW

                    // Subtract from NSC
                    nsc -= Folalloc;
                }
            }

            //  Apply defoliation in month of june
            if ((PlugIn.ModelCore.CurrentTime > 0) && (ecoregion.Variables.Month == (int)Constants.Months.June))
            {
                if (DefolProp > 0)
                {
                    //Adjust defol prop for foliage longevity - defol only affects current foliage
                    float adjDefol = DefolProp * species.TOfol;
                    ReduceFoliage(adjDefol);
                    // Update LAI after defoliation
                    LAI[index] = (1 / (float)PlugIn.IMAX) * fol / (species.SLWmax - species.SLWDel * index * (1 / (float)PlugIn.IMAX) * fol);
                }
            
            }

            // Reduction factor for radiation on photosynthesis
            FRad[index] = CumputeFrad(SubCanopyPar, species.HalfSat);

            // Reduction factor for ozone on photosynthesis
            FOzone[index] = ComputeFOzone(o3, species.NoO3Effect, species.O3HaltPsn, species.PsnO3Red);

            // Below-canopy PAR if updated after each subcanopy layer
            SubCanopyPar *= (float)Math.Exp(-species.K * LAI[index]);

            // Get pressure head given ecoregion and soil water content (latter in hydrology)
            float PressureHead = hydrology.GetPressureHead(ecoregion);

            // Reduction water for sub or supra optimal soil water content
            FWater[index] = CumputeFWater(species.H2, species.H3, species.H4, PressureHead);
            
            // If trees are physiologically active
            if (leaf_on)
            {

                // Compute net psn from stress factors and reference net psn
                NetPsn[index] = (1 / (float)PlugIn.IMAX) * FWater[index] * FRad[index] * FOzone[index] * Fage * ecoregion.Variables[species.Name].FTempPSNRefNetPsn * fol;

                // Net foliage respiration depends on reference psn (AMAX)
                //float FTempRespDayRefResp = ecoregion.Variables[species.Name].FTempRespDay * ecoregion.Variables.DaySpan * ecoregion.Variables.Daylength * Constants.MC / Constants.billion * ecoregion.Variables[species.Name].Amax;
                //Subistitute 24 hours in place of DayLength because foliar respiration does occur at night.  FTempRespDay uses Tave temps reflecting both day and night temperatures.
                float FTempRespDayRefResp = ecoregion.Variables[species.Name].FTempRespDay * ecoregion.Variables.DaySpan * (Constants.SecondsPerHour * 24) * Constants.MC / Constants.billion * ecoregion.Variables[species.Name].Amax;
                
                // Actal foliage respiration (growth respiration) 
                FolResp[index] = FWater[index] * FTempRespDayRefResp * fol / (float)PlugIn.IMAX;
                
                // Gross psn depends on net psn and foliage respiration
                GrossPsn[index] = NetPsn[index] + FolResp[index];

                // Old method
                // Transpiration depends on gross psn, water use efficiency (gCO2/mm water) and molecular weight (gC/gCO2)
                //Transpiration[index] = Math.Min(hydrology.Water,   GrossPsn[index] * Constants.MCO2_MC / ecoregion.Variables[Species.Name].WUE_CO2_corr);

                // M. Kubiske equation for transpiration: Improved methods for calculating WUE and Transpiration in PnET.
                Transpiration[index] = (float)(0.01227 * (NetPsn[index] / (ecoregion.Variables[Species.Name].JCO2 / ecoregion.Variables[Species.Name].JH2O)));
                 
                // Subtract transpiration from hydrology
                success = hydrology.AddWater(-1 * Transpiration[index]);
                if (success == false) throw new System.Exception("Error adding water, Transpiration = " + Transpiration[index] + " water = " + hydrology.Water);

                // Add net psn to non soluble carbons
                nsc += NetPsn[index];
             
            }
            else
            {
                // Reset subcanopy layer values
                NetPsn[index] = 0;
                FolResp[index] = 0;
                GrossPsn[index] = 0;
                Transpiration[index] = 0;

            }
           
            if (index < PlugIn.IMAX - 1) index++;
            return success;
        }
 
        public static float CumputeFrad(float Radiation, float HalfSat)
        {
            return Radiation / (Radiation + HalfSat);
        }
        public static float CumputeFWater(float H2, float H3, float H4, float pressurehead)
        {
            // Compute water stress
            if (pressurehead < 0 || pressurehead > H4) return 0;
            else if (pressurehead > H3) return 1 - ((pressurehead - H3) / (H4 - H3));
            else if (pressurehead < H2) return pressurehead / H2;
            else return 1;
        }
        public static float ComputeFOzone(float o3, float NoO3Effect, float O3HaltPsn, float PsnO3Red)
        {
            if (o3 <= NoO3Effect)
            {
                return (float)1.0;
            }
            else
            {
                return Math.Max(0, 1 - (float)Math.Pow(((o3 - NoO3Effect) / (O3HaltPsn - NoO3Effect)), PsnO3Red));
            }
        }
        public int ComputeNonWoodyBiomass(ActiveSite site)
        {
            return (int)(fol);
        }
        public static Percentage ComputeNonWoodyPercentage(Cohort cohort, ActiveSite site)
        {
            return new Percentage(cohort.fol / (cohort.Wood + cohort.Fol));
        }
        public void InitializeOutput(string SiteName, ushort YearOfBirth)
        {
            cohortoutput = new LocalOutput(SiteName, "Cohort_" + Species.Name + "_" + YearOfBirth + ".csv", OutputHeader);
       
        }
        public float SumLAI
        {
            get {
                return LAI.Sum();
            }

        }
        public void UpdateCohortData(IEcoregionPnETVariables monthdata )
        {
            float netPsnSum = NetPsn.Sum();
            float transpirationSum = Transpiration.Sum();
            float JCO2_JH2O = 0;
            if(transpirationSum > 0)
                JCO2_JH2O = (float) (0.01227 * (netPsnSum / transpirationSum));
            float WUE = JCO2_JH2O * ((float)44 / (float)18); //44=mol wt CO2; 18=mol wt H2O; constant =2.44444444444444

            // Cohort output file
            string s = Math.Round(monthdata.Year, 2) + "," + 
                        Age + "," +
                        Layer + "," + 
                       //canopy.ConductanceCO2 + "," +
                       SumLAI + "," +
                       GrossPsn.Sum() + "," +
                       FolResp.Sum() + "," +
                       MaintenanceRespiration.Sum() + "," +
                       NetPsn.Sum() + "," +                  // Sum over canopy layers
                       Transpiration.Sum() + "," +
                       WUE.ToString() + "," +
                       fol + "," + 
                       Root + "," + 
                       Wood + "," + 
                       NSC + "," +
                       NSCfrac + "," +
                       FWater.Average() + "," +
                       FRad.Average() + "," +
                       FOzone.Average() + "," +
                       monthdata[Species.Name].DelAmax +
                       monthdata[Species.Name].FTempPSN + "," +
                       monthdata[Species.Name].FTempRespWeightedDayAndNight + "," +
                       Fage + "," +
                       leaf_on + "," +
                       FActiveBiom + "," ;
             
            cohortoutput.Add(s);

       
        }

        public string OutputHeader
        {
            get
            { 
                // Cohort output file header
                string hdr = OutputHeaders.Time + "," + 
                            OutputHeaders.Age + "," +
                            //OutputHeaders.ConductanceCO2 + "," + 
                            OutputHeaders.Layer + "," + 
                            OutputHeaders.LAI + "," +
                            OutputHeaders.GrossPsn + "," + 
                            OutputHeaders.FolResp + "," + 
                            OutputHeaders.MaintResp + "," + 
                            OutputHeaders.NetPsn + "," +
                            OutputHeaders.Transpiration + "," +
                            OutputHeaders.WUE + "," +
                            OutputHeaders.Fol + "," + 
                            OutputHeaders.Root + "," + 
                            OutputHeaders.Wood + "," +
                            OutputHeaders.NSC + "," + 
                            OutputHeaders.NSCfrac + "," + 
                            OutputHeaders.fWater + "," +  
                            OutputHeaders.fRad + "," + 
                            OutputHeaders.FOzone+ "," +
                            OutputHeaders.DelAMax + "," + 
                            OutputHeaders.fTemp_psn + "," +
                            OutputHeaders.fTemp_resp + "," + 
                            OutputHeaders.fage + "," + 
                            OutputHeaders.LeafOn + "," + 
                            OutputHeaders.FActiveBiom + ",";

                return hdr;
            }
        }
        public void WriteCohortData()
        {
            cohortoutput.Write();
         
        }
         
        public float FActiveBiom
        {
            get
            {
                return (float)Math.Exp(-species.FrActWd * biomass);
            }
        }
        public bool IsAlive
        {
            // Determine if cohort is alive. It is assumed that a cohort is dead when 
            // NSC decline below 1% of biomass
            get
            {
                return NSCfrac > 0.01F;
            }
        }
        public float FoliageSenescence()
        {
            // If it is fall 
            float Litter = species.TOfol * fol;
            fol -= Litter;

            return Litter;

        }
        
        public float Senescence()
        {
            float senescence = ((Root * species.TOroot) + Wood * species.TOwood);
            biomass -= senescence;

            return senescence;
        }

        public void ReduceFoliage(double fraction)
        {
            fol *= (float)(1.0 - fraction);
        }
        public void ReduceBiomass(double fraction)
        {
            biomass *= (float)(1.0 - fraction);
            fol *= (float)(1.0 - fraction);
        }

        //---------------------------------------------------------------------
        /// <summary>
        /// Raises a Cohort.AgeOnlyDeathEvent.
        /// </summary>
        public static void RaiseDeathEvent(object sender,
                                Cohort cohort, 
                                ActiveSite site,
                                ExtensionType disturbanceType)
        {
            if (AgeOnlyDeathEvent != null)
            {
                AgeOnlyDeathEvent(sender, new Landis.Library.BiomassCohorts.DeathEventArgs(cohort, site, disturbanceType));
            }
            if (DeathEvent != null)
            {
                DeathEvent(sender, new Landis.Library.BiomassCohorts.DeathEventArgs(cohort, site, disturbanceType));
            }
           
        }
 
        
    } 
}
