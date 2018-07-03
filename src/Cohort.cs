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
        private float adjHalfSat;
        private float adjFolN;

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

        // O3Effect by sublayer
        //public float[] O3Effect = null;

        // Reduction factor for ozone 
        public float[] FOzone = null;

        // Interception (mm/mo)
        public float[] Interception = null;

        // Adjustment folN based on fRad
        public float[] AdjFolN = null;

        // Modifier of CiCa ratio based on fWater and Ozone
        public float[] CiModifier = null;

        // Adjustment to Amax based on CO2
        public float[] DelAmax = null;


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
            //O3Effect = new float[PlugIn.IMAX];
            FOzone = new float[PlugIn.IMAX];
            MaintenanceRespiration = new float[PlugIn.IMAX];
            Interception = new float[PlugIn.IMAX];
            AdjFolN = new float[PlugIn.IMAX];
            CiModifier = new float[PlugIn.IMAX];
            DelAmax = new float[PlugIn.IMAX];
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
            AdjFolN = null;
            CiModifier = null;
            DelAmax = null;
        }
        //public void NullO3SubLayers()
        //{
        //    // Reset values for subcanopy layers
        //    O3Effect = null;
        //}
      
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

        public bool CalculatePhotosynthesis(float PrecInByCanopyLayer,int precipCount, float LeakagePerCohort, IHydrology hydrology, ref float SubCanopyPar, float o3_cum, float o3_month, int subCanopyIndex, int layerCount, ref float O3Effect, float frostFreeSoilDepth)
         {            

            
            bool success = true;
            float lastO3Effect = O3Effect;
            O3Effect = 0;

            float frostFreeProp = Math.Min(1.0F,frostFreeSoilDepth / ecoregion.RootingDepth);

            // Leaf area index for the subcanopy layer by index. Function of specific leaf weight SLWMAX and the depth of the canopy
            // Depth of the canopy is expressed by the mass of foliage above this subcanopy layer (i.e. slwdel * index/imax *fol)
            LAI[index] = (1 / (float)PlugIn.IMAX) * fol / (species.SLWmax - species.SLWDel * index * (1 / (float)PlugIn.IMAX) * fol);
            
            // Precipitation interception has a max in the upper canopy and decreases exponentially through the canopy
            //Interception[index] = PrecInByCanopyLayer * (float)(1 - Math.Exp(-1 * ecoregion.PrecIntConst * LAI[index]));
            //if (Interception[index] > PrecInByCanopyLayer) throw new System.Exception("Error adding water, PrecInByCanopyLayer = " + PrecInByCanopyLayer + " Interception[index] = " + Interception[index]);
            
            // If more than one precip event assigned to layer, repeat precip, runoff, leakage for all events prior to respiration
            for (int p = 1; p <= precipCount; p++)
            {
                // Incoming precipitation
                //float waterIn = PrecInByCanopyLayer  - Interception[index]; //mm   
                float precipIn = PrecInByCanopyLayer; //mm 

                // Instantaneous runoff (excess of porosity)
                float runoff = Math.Min(precipIn, Math.Max(hydrology.Water - (ecoregion.Porosity * frostFreeProp), 0));
                Hydrology.RunOff += runoff;

                float waterIn = precipIn - runoff;

                // Add incoming precipitation to soil moisture
                success = hydrology.AddWater(waterIn);
                if (success == false) throw new System.Exception("Error adding water, waterIn = " + waterIn + " water = " + hydrology.Water);
                               
                // Leakage only occurs following precipitation events
                if (waterIn > 0)
                {
                    float leakageFrostReduction = 1.0F;
                    if(frostFreeSoilDepth < ecoregion.RootingDepth + PlugIn.LeakageFrostDepth)
                    {
                        if(frostFreeSoilDepth < ecoregion.RootingDepth)
                        {
                            leakageFrostReduction = 0.0F;
                        }
                        else
                        {
                            leakageFrostReduction = 1.0F / PlugIn.LeakageFrostDepth * (frostFreeSoilDepth - ecoregion.RootingDepth);
                        }
                    }
                    float leakage = Math.Max((float)LeakagePerCohort * leakageFrostReduction * (hydrology.Water - (ecoregion.FieldCap * frostFreeProp)), 0);
                    Hydrology.Leakage += leakage;

                    // Remove fast leakage
                    success = hydrology.AddWater(-1 * leakage);
                    if (success == false) throw new System.Exception("Error adding water, Hydrology.Leakage = " + Hydrology.Leakage + " water = " + hydrology.Water);
                }
            }
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
                    //Check if nscfrac is below threshold to determine if cohort is alive
                    if (!this.IsAlive)
                    {
                        nsc = 0.0F;  // if cohort is dead, nsc goes to zero and becomes functionally dead even though not removed until end of timestep
                    }
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
            // Leaf area index for the subcanopy layer by index. Function of specific leaf weight SLWMAX and the depth of the canopy
            // Depth of the canopy is expressed by the mass of foliage above this subcanopy layer (i.e. slwdel * index/imax *fol)
            LAI[index] = (1 / (float)PlugIn.IMAX) * fol / (species.SLWmax - species.SLWDel * index * (1 / (float)PlugIn.IMAX) * fol);
            
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

            // Adjust HalfSat for CO2 effect
            float halfSatIntercept = species.HalfSat - 350 * species.CO2HalfSatEff;
            adjHalfSat = species.CO2HalfSatEff * ecoregion.Variables.CO2 + halfSatIntercept;
            // Reduction factor for radiation on photosynthesis
            FRad[index] = ComputeFrad(SubCanopyPar, adjHalfSat);
            
            

            // Below-canopy PAR if updated after each subcanopy layer
            SubCanopyPar *= (float)Math.Exp(-species.K * LAI[index]);

            // Get pressure head given ecoregion and soil water content (latter in hydrology)
            float PressureHead = hydrology.GetPressureHead(ecoregion);

            // Reduction water for sub or supra optimal soil water content
            float fWaterOzone = 1.0f;  //fWater for ozone functions; ignores H2 parameter because only impacts when drought-stressed
            if (PlugIn.ModelCore.CurrentTime > 0)
            {
                FWater[index] = ComputeFWater(species.H2, species.H3, species.H4, PressureHead);
                fWaterOzone = ComputeFWater(0, species.H3, species.H4, PressureHead);
            }
            else // Ignore H2 parameter during spinup
            {
                FWater[index] = ComputeFWater(0, species.H3, species.H4, PressureHead);
                fWaterOzone = FWater[index];
            }
            
      

            // FoliarN adjusted based on canopy position (FRad)
            float folN_slope = species.FolNSlope; //Slope for linear FolN relationship
            float folN_int = species.FolNInt; //Intercept for linear FolN relationship
            adjFolN = (FRad[index] * folN_slope + folN_int) * species.FolN; // Linear reduction (with intercept) in FolN with canopy depth (FRad)
            AdjFolN[index] = adjFolN;  // Stored for output


            float ciModifier = fWaterOzone; // if no ozone, ciModifier defaults to fWater
            if (o3_cum > 0)
            {
                // Regression coefs estimated from New 3 algorithm for Ozone drought.xlsx
                // https://usfs.box.com/s/eksrr4d7fli8kr9r4knfr7byfy9r5z0i
                // Uses data provided by Yasutomo Hoshika and Elena Paoletti
                float ciMod_tol = (float)(fWaterOzone + (-0.021 * fWaterOzone + 0.0087) * o3_cum);
                ciMod_tol = Math.Min(ciMod_tol, 1.0f);
                float ciMod_int = (float)(fWaterOzone + (-0.0148 * fWaterOzone + 0.0062) * o3_cum);
                ciMod_int = Math.Min(ciMod_int, 1.0f);
                float ciMod_sens = (float)(fWaterOzone + (-0.0176 * fWaterOzone + 0.0118) * o3_cum);
                ciMod_sens = Math.Min(ciMod_sens, 1.0f);                              
                if ((species.O3StomataSens == "Sensitive") || (species.O3StomataSens == "Sens"))
                    ciModifier = ciMod_sens;
                else if ((species.O3StomataSens == "Tolerant") || (species.O3StomataSens == "Tol"))
                    ciModifier = ciMod_tol;
                else if ((species.O3StomataSens == "Intermediate") || (species.O3StomataSens == "Int"))
                    ciModifier = ciMod_int;
                else
                {
                    throw new System.Exception("Ozone data provided, but species O3StomataSens is not set to Sensitive, Tolerant or Intermediate");
                }
            }

            CiModifier[index] = ciModifier;  // Stored for output

            // If trees are physiologically active
            if (leaf_on)
            {
                // CO2 ratio internal to the leaf versus external
                float cicaRatio = (-0.075f * adjFolN) + 0.875f;  
                float modCiCaRatio = cicaRatio * ciModifier;
                // Reference co2 ratio
                float ci350 = 350 * modCiCaRatio;
                // Elevated leaf internal co2 concentration
                float ciElev = ecoregion.Variables.CO2 * modCiCaRatio;
                float Ca_Ci = ecoregion.Variables.CO2 - ciElev;

                // Franks method
                // (Franks,2013, New Phytologist, 197:1077-1094)
                float Gamma = 40; // 40; Gamma is the CO2 compensation point (the point at which photorespiration balances exactly with photosynthesis.  Assumed to be 40 based on leaf temp is assumed to be 25 C

                // Modified Gamma based on air temp
                // Tested here but removed for release v3.0
                // Bernacchi et al. 2002. Plant Physiology 130, 1992-1998
                // Gamma* = e^(13.49-24.46/RTk) [R is universal gas constant = 0.008314 kJ/J/mole, Tk is absolute temperature]
                //float Gamma_T = (float) Math.Exp(13.49 - 24.46 / (0.008314 * (ecoregion.Variables.Tday + 273)));

                float Ca0 = 350;  // 350
                float Ca0_adj = Ca0 * cicaRatio;  // Calculated internal concentration given external 350


                // Franks method (Franks,2013, New Phytologist, 197:1077-1094)
                float delamax = (ecoregion.Variables.CO2 - Gamma) / (ecoregion.Variables.CO2 + 2 * Gamma) * (Ca0 + 2 * Gamma) / (Ca0 - Gamma);
                if (delamax < 0)
                {
                    delamax = 0;
                }

                // Franks method (Franks,2013, New Phytologist, 197:1077-1094)
                // Adj Ca0
                float delamax_adj = (ecoregion.Variables.CO2 - Gamma) / (ecoregion.Variables.CO2 + 2 * Gamma) * (Ca0_adj + 2 * Gamma) / (Ca0_adj - Gamma);
                if (delamax_adj < 0)
                {
                    delamax_adj = 0;
                }
                
                // Modified Franks method - by M. Kubiske
                // substitute ciElev for CO2
                float delamaxCi = (ciElev - Gamma) / (ciElev + 2 * Gamma) * (Ca0 + 2 * Gamma) / (Ca0 - Gamma);
                if (delamaxCi < 0)
                {
                    delamaxCi = 0;
                }
                
                // Modified Franks method - by M. Kubiske
                // substitute ciElev for CO2
                // adjusted Ca0
                float delamaxCi_adj = (ciElev - Gamma) / (ciElev + 2 * Gamma) * (Ca0_adj + 2 * Gamma) / (Ca0_adj - Gamma);
                if (delamaxCi_adj < 0)
                {
                    delamaxCi_adj = 0;
                }

                // Choose between delamax methods here:
                DelAmax[index] = delamax;  // Franks
                //DelAmax[index] = delamax_adj;  // Franks with adjusted Ca0
                //DelAmax[index] = delamaxCi;  // Modified Franks
                //DelAmax[index] = delamaxCi_adj;  // Modified Franks with adjusted Ca0

                // M. Kubiske method for wue calculation:  Improved methods for calculating WUE and Transpiration in PnET.
                float V = (float)(8314.47 * (ecoregion.Variables.Tmin + 273) / 101.3);
                float JCO2 = (float)(0.139 * ((ecoregion.Variables.CO2 - ciElev) / V) * 0.00001);
                float JH2O = ecoregion.Variables[species.Name].JH2O * ciModifier;
                float wue = (JCO2 / JH2O) * (44 / 18);  //44=mol wt CO2; 18=mol wt H2O; constant =2.44444444444444

                float Amax = (float)(delamaxCi * (species.AmaxA + ecoregion.Variables[species.Name].AmaxB_CO2 * adjFolN)); 

                //Reference net Psn (lab conditions) in gC/g Fol/month
                float RefNetPsn = ecoregion.Variables.DaySpan * (Amax * ecoregion.Variables[species.Name].DVPD * ecoregion.Variables.Daylength * Constants.MC) / Constants.billion;

                // PSN (gC/g Fol/month) reference net psn in a given temperature
                float FTempPSNRefNetPsn = ecoregion.Variables[species.Name].FTempPSN * RefNetPsn;

                // Compute net psn from stress factors and reference net psn (gC/g Fol/month)
                // FTempPSNRefNetPsn units are gC/g Fol/mo
                float nonOzoneNetPsn = (1 / (float)PlugIn.IMAX) * FWater[index] * FRad[index] * Fage * FTempPSNRefNetPsn * fol;  // gC/m2 ground/mo
               
                // Convert Psn gC/m2 ground/mo to umolCO2/m2 fol/s
                // netPsn_ground = LayerNestPsn*1000000umol*(1mol/12gC) * (1/(60s*60min*14hr*30day))
                float netPsn_ground = nonOzoneNetPsn * 1000000F * (1F / 12F) * (1F / (ecoregion.Variables.Daylength * ecoregion.Variables.DaySpan));
                float netPsn_leaf_s = 0;
                if (netPsn_ground > 0 && LAI[index] > 0)
                {
                    // nesPsn_leaf_s = NetPsn_ground*(1/LAI){m2 fol/m2 ground}
                    netPsn_leaf_s = netPsn_ground * (1F / LAI[index]);
                    if(float.IsInfinity(netPsn_leaf_s))
                    {
                        netPsn_leaf_s = 0;
                    }
                }

                //Calculate water vapor conductance (gwv) from Psn and Ci; Kubiske Conductance_5.xlsx
                //gwv_mol = NetPsn_leaf_s /(Ca-Ci) {umol/mol} * 1.6(molH20/molCO2)*1000 {mmol/mol}
                float gwv_mol = (float)(netPsn_leaf_s / (Ca_Ci) * 1.6 * 1000);
                //gwv = gwv_mol / (444.5 - 1.3667*Tc)*10    {denominator is from Koerner et al. 1979 (Sheet 3),  Tc = temp in degrees C, * 10 converts from cm to mm.  
                float gwv = (float) (gwv_mol / (444.5 - 1.3667 * ecoregion.Variables.Tave) * 10);

                // Calculate gwv from Psn using Ollinger equation
                // g = -0.3133+0.8126*NetPsn_leaf_s
                //float g = (float) (-0.3133 + 0.8126 * netPsn_leaf_s);

                // Reduction factor for ozone on photosynthesis
                if (o3_month > 0)
                {
                    float o3Coeff = species.O3GrowthSens;
                    O3Effect = ComputeO3Effect_PnET(o3_month, delamaxCi, netPsn_leaf_s, subCanopyIndex, layerCount, fol, lastO3Effect, gwv, LAI[index], o3Coeff);
                }
                else
                { O3Effect = 0; }
                FOzone[index] = 1 - O3Effect;
               

                //Apply reduction factor for Ozone
                NetPsn[index] = nonOzoneNetPsn * FOzone[index];

                // Net foliage respiration depends on reference psn (AMAX)
                //float FTempRespDayRefResp = ecoregion.Variables[species.Name].FTempRespDay * ecoregion.Variables.DaySpan * ecoregion.Variables.Daylength * Constants.MC / Constants.billion * ecoregion.Variables[species.Name].Amax;
                //Subistitute 24 hours in place of DayLength because foliar respiration does occur at night.  FTempRespDay uses Tave temps reflecting both day and night temperatures.
                float FTempRespDayRefResp = ecoregion.Variables[species.Name].FTempRespDay * ecoregion.Variables.DaySpan * (Constants.SecondsPerHour * 24) * Constants.MC / Constants.billion * Amax;
                
                // Actal foliage respiration (growth respiration) 
                FolResp[index] = FWater[index] * FTempRespDayRefResp * fol / (float)PlugIn.IMAX;
                
                // Gross psn depends on net psn and foliage respiration
                GrossPsn[index] = NetPsn[index] + FolResp[index];

                // M. Kubiske equation for transpiration: Improved methods for calculating WUE and Transpiration in PnET.
                // JH2O has been modified by CiModifier to reduce water use efficiency
                Transpiration[index] = (float)(0.01227 * (GrossPsn[index] / (JCO2 / JH2O)));
 
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
                FOzone[index] = 1;

            }
           
            if (index < PlugIn.IMAX - 1) index++;
            return success;
        }

        // Based on Michaelis-Menten saturation curve
        // https://en.wikibooks.org/wiki/Structural_Biochemistry/Enzyme/Michaelis_and_Menten_Equation
        public static float ComputeFrad(float Radiation, float HalfSat)
        {
            // Derived from Michaelis-Menton equation
            // https://en.wikibooks.org/wiki/Structural_Biochemistry/Enzyme/Michaelis_and_Menten_Equation

            return Radiation / (Radiation + HalfSat);
        }
        public static float ComputeFWater(float H2, float H3, float H4, float pressurehead)
        {
            // Compute water stress
            if (pressurehead < 0 || pressurehead > H4) return 0;
            else if (pressurehead > H3) return 1 - ((pressurehead - H3) / (H4 - H3));
            else if (pressurehead < H2) return pressurehead / H2;
            else return 1;
        }

        public static float ComputeO3Effect_PnET(float o3, float delAmax, float netPsn_leaf_s, int Layer, int nLayers, float FolMass, float lastO3Effect, float gwv, float layerLAI, float o3Coeff)
        {
            float currentO3Effect = 1.0F;
            float droughtO3Frac = 1.0F; // Not using droughtO3Frac from PnET code per M. Kubiske and A. Chappelka
            //float kO3Eff = 0.0026F;  // Generic coefficient from Ollinger
            float kO3Eff = 0.0026F * o3Coeff;  // Scaled by species using input parameters
            

            float O3Prof = (float)(0.6163 + (0.00105 * FolMass));
            float RelLayer = (float)Layer / (float)nLayers;
            float relO3 = Math.Min(1,1 - (RelLayer * O3Prof) * (RelLayer * O3Prof) * (RelLayer * O3Prof));
            // Kubiske method (using gwv in place of conductance
            currentO3Effect = (float)Math.Min(1, (lastO3Effect * droughtO3Frac) + (kO3Eff * gwv * o3 * relO3));

            // Ollinger method
            // Calculations for gsSlope and gsInt could be moved back to EcoregionPnETVariables since they only depend on delamax
            //float gsSlope=(float)((-1.1309*delAmax)+1.9762);
            //float gsInt = (float)((0.4656 * delAmax) - 0.9701);
            //float conductance = Math.Max(0, (gsInt + (gsSlope * netPsn_leaf_s)) * (1 - lastO3Effect));
            //float currentO3Effect_conductance = (float)Math.Min(1, (lastO3Effect * droughtO3Frac) + (kO3Eff * conductance * o3 * relO3));

            // Tested here but removed for release v3.0
            //string OzoneConductance = ((Parameter<string>)PlugIn.GetParameter(Names.OzoneConductance)).Value;
            //if (OzoneConductance == "Kubiske")
            //    return currentO3Effect;
            //else if (OzoneConductance == "Ollinger")
            //    return currentO3Effect_conductance;
            //else
            //{
            //    System.Console.WriteLine("OzoneConductance is not Kubiske or Ollinger.  Using Kubiske by default");
            //    return currentO3Effect;
            //}

            return currentO3Effect;
            
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
                       SumLAI + "," +
                       GrossPsn.Sum() + "," +
                       FolResp.Sum() + "," +
                       MaintenanceRespiration.Sum() + "," +
                       netPsnSum + "," +                  // Sum over canopy layers
                       transpirationSum + "," +
                       WUE.ToString() + "," +
                       fol + "," +
                       Root + "," +
                       Wood + "," +
                       NSC + "," +
                       NSCfrac + "," +
                       FWater.Average() + "," +
                       FRad.Average() + "," +
                       FOzone.Average() + "," +
                       DelAmax.Average() + "," +
                       monthdata[Species.Name].FTempPSN + "," +
                       monthdata[Species.Name].FTempRespWeightedDayAndNight + "," +
                       Fage + "," +
                       leaf_on + "," +
                       FActiveBiom + "," +
                       AdjFolN.Average() + "," +
                       CiModifier.Average() + ","+
                       adjHalfSat + ",";
             
            cohortoutput.Add(s);

       
        }

        public string OutputHeader
        {
            get
            { 
                // Cohort output file header
                string hdr = OutputHeaders.Time + "," +
                            OutputHeaders.Age + "," +
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
                            OutputHeaders.FOzone + "," +
                            OutputHeaders.DelAMax + "," +
                            OutputHeaders.fTemp_psn + "," +
                            OutputHeaders.fTemp_resp + "," +
                            OutputHeaders.fage + "," +
                            OutputHeaders.LeafOn + "," +
                            OutputHeaders.FActiveBiom + "," +
                            OutputHeaders.AdjFolN + "," +
                            OutputHeaders.CiModifier + ","+
                            OutputHeaders.AdjHalfSat + ",";

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
            // If cohort is dead, then all foliage is lost
            if (NSCfrac <= 0.01F)
                Litter = fol;
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
        public void ReduceBiomass(object sitecohorts, double fraction, ExtensionType disturbanceType)
        {
            Allocation.Allocate(sitecohorts, this, disturbanceType, fraction);

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
            //if (AgeOnlyDeathEvent != null)
            //{
            //    AgeOnlyDeathEvent(sender, new Landis.Library.BiomassCohorts.DeathEventArgs(cohort, site, disturbanceType));
            //}
            if (DeathEvent != null)
            {
                DeathEvent(sender, new Landis.Library.BiomassCohorts.DeathEventArgs(cohort, site, disturbanceType));
            }
           
        }

        
    } 
}
