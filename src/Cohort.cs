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
        private float expectedDefol; //set at first defoliation to prevent multiple defoliations annually
        private float expectedAlloc; //set at first allocation to prevent multiple reflushes

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

            private set
            {
                fol = value;       
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
            Fol += c.Fol;
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
                return nsc / (FActiveBiom * (biomass + Fol));
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
            int abovegroundBiomass = (int)((1 - species.FracBelowG) * biomass) + (int)Fol;
            defolProp = (float)Landis.Library.Biomass.CohortDefoliation.Compute(site, species, abovegroundBiomass, SiteAboveGroundBiomass);
        }

        public bool CalculatePhotosynthesis(float PrecInByCanopyLayer,int precipCount, double LeakagePerCohort, IHydrology hydrology, ref float SubCanopyPar)
        { 
            bool success = true;

            // Leaf area index for the subcanopy layer by index. Function of specific leaf weight SLWMAX and the depth of the canopy
            // Depth of the canopy is expressed by the mass of foliage above this subcanopy layer (i.e. slwdel * index/imax *fol)
            LAI[index] = (1 / (float)PlugIn.IMAX) * Fol / (species.SLWmax - species.SLWDel * index * (1 / (float)PlugIn.IMAX) * Fol);
            
            // Precipitation interception has a max in the upper canopy and decreases exponentially through the canopy
            //Interception[index] = PrecInByCanopyLayer * (float)(1 - Math.Exp(-1 * ecoregion.PrecIntConst * LAI[index]));
            //if (Interception[index] > PrecInByCanopyLayer) throw new System.Exception("Error adding water, PrecInByCanopyLayer = " + PrecInByCanopyLayer + " Interception[index] = " + Interception[index]);
            
            // If more than one precip event assigned to layer, repeat precip, runoff, leakage for all events prior to respiration
            for (int p = 1; p <= precipCount; p++)
            {
                // Incoming precipitation
                //float waterIn = PrecInByCanopyLayer  - Interception[index]; //mm   
                float waterIn = PrecInByCanopyLayer; //mm 

                // Add incoming precipitation to soil moisture
                success = hydrology.AddWater(waterIn);
                if (success == false) throw new System.Exception("Error adding water, waterIn = " + waterIn + " water = " + hydrology.Water);

                // Instantaneous runoff (excess of porosity)
                float runoff = Math.Max(hydrology.Water - ecoregion.Porosity, 0);
                Hydrology.RunOff += runoff;
                success = hydrology.AddWater(-1 * runoff);
                if (success == false) throw new System.Exception("Error adding water, Hydrology.RunOff = " + Hydrology.RunOff + " water = " + hydrology.Water);

                // Fast Leakage only occurs following precipitation events
                if (waterIn > 0)
                {
                    float leakage = Math.Max((float)LeakagePerCohort * (hydrology.Water - ecoregion.FieldCap), 0);
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

                    expectedDefol = 0;
                    expectedAlloc = 0;
                }
            }
            
            // When LeafOn becomes false for the first time in a year - Shouldn't happen January, February, March? 
            if (ecoregion.Variables.Tmin < this.SpeciesPNET.PsnTMin) 
            {
                if (leaf_on)
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

            /****************************** Adam's restructuring 3/1/2018 ***************************************/
            if (leaf_on)
            {
                float IdealFol = (species.FracFol * FActiveBiom * biomass);

                if (ecoregion.Variables.Month < (int)Constants.Months.June) //Growing season before defoliation outbreaks
                {
                    if (IdealFol > Fol)
                    {
                        // Foliage allocation depends on availability of NSC (allows deficit at this time so no min nsc)
                        // carbon fraction of biomass to convert C to DW
                        float Folalloc = Math.Max(0, Math.Min(nsc, species.CFracBiomass * (IdealFol - Fol))); // gC/mo

                        // Add foliage allocation to foliage
                        Fol += Folalloc / species.CFracBiomass;// gDW
                        // Subtract from NSC
                        nsc -= Folalloc;
                    }     
                }
                else if (ecoregion.Variables.Month == (int)Constants.Months.June) //Apply defoliation only in June
                {
                    if (Fol > expectedDefol) //Insect defoliation - expected defoliation prevents multiple rounds of defoliation within a cohort (which shares canopy variables, like foliage)
                    {
                        ReduceFoliage(defolProp);
                    }
                    else if (species.TOfol < 1)
                    {
                        ReduceFoliage(species.TOfol);           //Other defoliating processes in the community: needlefall in conifers
                    }
                    expectedDefol = Fol;                //Order is very important here so that defoliation is not iterated within a cohort
                }
                else if (ecoregion.Variables.Month > (int)Constants.Months.June) //During and after defoliation events
                {
                    if (defolProp > 0)
                    {
                        if (defolProp > 0.6 && species.TOfol == 1)  // Refoliation at >60% reduction in foliage for deciduous trees - MGM
                        {
                            // Foliage allocation depends on availability of NSC (allows deficit at this time so no min nsc)
                            // carbon fraction of biomass to convert C to DW
                            float Folalloc = Math.Max(0f, Math.Min(nsc, 0.9f * species.CFracBiomass * (IdealFol - Fol)));
                            if (Folalloc > expectedAlloc)
                            {
                                expectedAlloc = Folalloc;
                            }
                            else
                                Folalloc = 0;
                            
                            Fol += Folalloc / species.CFracBiomass;// gDW

                            // Subtract from NSC
                            nsc -= Folalloc;
   
                        }
                        else //No attempted refoliation but carbon loss after defoliation -             12% x
                        {
                            // Foliage allocation depends on availability of NSC (allows deficit at this time so no min nsc)
                            // carbon fraction of biomass to convert C to DW
                            float carbon_cost = 0.05f;
                            if (ecoregion.Variables.Month > (int)Constants.Months.June)
                            { }
                            else
                            { }
                            float Folalloc = Math.Max(0f, Math.Min(nsc, carbon_cost * species.CFracBiomass * (IdealFol))); // gC/mo 5% of IdealFol to take out NSC - estimated from Palacio et al 2012 - MGM 

                            // Subtract from NSC do not add Fol
                            nsc -= Folalloc;
                        }
                    }
                    else if (IdealFol > Fol)    //Non-defoliated trees refoliate 'normally'
                    {
                        // Foliage allocation depends on availability of NSC (allows deficit at this time so no min nsc)
                        // carbon fraction of biomass to convert C to DW
                        float Folalloc = Math.Max(0, Math.Min(nsc, species.CFracBiomass * (IdealFol - Fol))); // gC/mo

                        // Add foliage allocation to foliage
                        Fol += Folalloc / species.CFracBiomass;// gDW

                        // Subtract from NSC
                        nsc -= Folalloc;
                    }     
                }
            }
            /*^^^^^^^^^^^^^^^^^^^^^^^^^^^^ Adam's restructuring 3/1/2018 ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^*/

            /*if (leaf_on)
            {
                // Foliage linearly increases with active biomass
                float IdealFol = (species.FracFol * FActiveBiom * biomass);

                // If the tree should have more filiage than it currently has
                if (ecoregion.Variables.Month == (int)Constants.Months.June) //Insect defoliation occurs here rather than in the senescence block
                {
                    if (Fol > expectedDefol)            
                    {
                        ReduceFoliage(defolProp);
                        expectedDefol = Fol;
                    }

                    float adjDefol = species.TOfol;
                    ReduceFoliage(adjDefol);           //Other defoliating processes
                }
                else if (IdealFol > Fol && (ecoregion.Variables.Month < (int)Constants.Months.June || defolProp == 0))  //No refoliation after June unless no defoliation from insects
                {
                    // Foliage allocation depends on availability of NSC (allows deficit at this time so no min nsc)
                    // carbon fraction of biomass to convert C to DW
                    float Folalloc = Math.Max(0, Math.Min(nsc, species.CFracBiomass * (IdealFol - Fol))); // gC/mo

                    // Add foliage allocation to foliage
                    Fol += Folalloc / species.CFracBiomass;// gDW

                    // Subtract from NSC
                    nsc -= Folalloc;
                }
            }*/

            LAI[index] = (1 / (float)PlugIn.IMAX) * fol / (species.SLWmax - species.SLWDel * index * (1 / (float)PlugIn.IMAX) * Fol);

            //  Apply defoliation in month of june -- generalize to all months (Adam 1/13/2018) ecoregion.Variables.Month == (int)Constants.Months.June
            /*  Merged with ideal foliation block to achieve proper foliage balance AKC - 2/8/2018
             * if (PlugIn.ModelCore.CurrentTime > 0)
            {
                if (ecoregion.Variables.Month > (int)Constants.Months.May)
                {
                    if (DefolProp > 0)
                    {
                        //Adjust defol prop for foliage longevity - defol only affects current foliage
                        float adjDefol = DefolProp * species.TOfol;
                        ReduceFoliage(adjDefol);
                        
                        // Update LAI after defoliation
                        LAI[index] = (1 / (float)PlugIn.IMAX) * fol / (species.SLWmax - species.SLWDel * index * (1 / (float)PlugIn.IMAX) * fol);

                        leaf_on = false;
                    }
                }
            }*/

            // Reduction factor for radiation on photosynthesis
            FRad[index] = ComputeFrad(SubCanopyPar, species.HalfSat);
            
            // Below-canopy PAR if updated after each subcanopy layer
            SubCanopyPar *= (float)Math.Exp(-species.K * LAI[index]);

            // Get pressure head given ecoregion and soil water content (latter in hydrology)
            float PressureHead = hydrology.GetPressureHead(ecoregion);

            // Reduction water for sub or supra optimal soil water content
            if(PlugIn.ModelCore.CurrentTime > 0)
                FWater[index] = ComputeFWater(species.H2, species.H3, species.H4, PressureHead);
            else // Ignore H2 parameter during spinup
                FWater[index] = ComputeFWater(0, species.H3, species.H4, PressureHead);
            
            // If trees are physiologically active
            if (leaf_on)
            {

                // Compute net psn from stress factors and reference net psn
                NetPsn[index] = (1 / (float)PlugIn.IMAX) * FWater[index] * FRad[index] * Fage * ecoregion.Variables[species.Name].FTempPSNRefNetPsn * Fol;

                // Net foliage respiration depends on reference psn (AMAX)
                //float FTempRespDayRefResp = ecoregion.Variables[species.Name].FTempRespDay * ecoregion.Variables.DaySpan * ecoregion.Variables.Daylength * Constants.MC / Constants.billion * ecoregion.Variables[species.Name].Amax;
                //Subistitute 24 hours in place of DayLength because foliar respiration does occur at night.  FTempRespDay uses Tave temps reflecting both day and night temperatures.
                float FTempRespDayRefResp = ecoregion.Variables[species.Name].FTempRespDay * ecoregion.Variables.DaySpan * (Constants.SecondsPerHour * 24) * Constants.MC / Constants.billion * ecoregion.Variables[species.Name].Amax;
                
                // Actal foliage respiration (growth respiration) 
                FolResp[index] = FWater[index] * FTempRespDayRefResp * Fol / (float)PlugIn.IMAX;
                
                // Gross psn depends on net psn and foliage respiration
                GrossPsn[index] = NetPsn[index] + FolResp[index];

                // Transpiration depends on gross psn, water use efficiency (gCO2/mm water) and molecular weight (gC/gCO2)
                Transpiration[index] = Math.Min(hydrology.Water, GrossPsn[index] / ecoregion.Variables[Species.Name].WUE / ecoregion.Variables[Species.Name].DelAmax * Constants.MCO2_MC);
                 
                // Subtract transpiration from hydrology
                success = hydrology.AddWater(-1 * Transpiration[index]);
                if (success == false) throw new System.Exception("Error adding water, Transpiration = " + Transpiration[index] + " water = " + hydrology.Water);

                // Add net psn to non soluble carbons
                nsc += NetPsn[index];
                
                Console.WriteLine("Month: " + ecoregion.Variables.Month);
                Console.WriteLine(NetPsn[index]);
             
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

        // Based on Michaelis-Menten saturation curve
        // https://en.wikibooks.org/wiki/Structural_Biochemistry/Enzyme/Michaelis_and_Menten_Equation
        public static float ComputeFrad(float Radiation, float HalfSat)
        {
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
        public int ComputeNonWoodyBiomass(ActiveSite site)
        {
            return (int)(Fol);
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
                       ((Transpiration.Sum() > 0) ? NetPsn.Sum() / Transpiration.Sum() : 0).ToString() + "," +
                       Fol + "," + 
                       Root + "," + 
                       Wood + "," + 
                       NSC + "," +
                       NSCfrac + "," +
                       FWater.Average() + "," +
                       FRad.Average() + "," +
                       monthdata[Species.Name].FTempPSN + "," +
                       monthdata[Species.Name].FTempRespWeightedDayAndNight + "," +
                       Fage + "," +
                       leaf_on + "," +
                       FActiveBiom;
             
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
            float Litter = species.TOfol * Fol;
            Fol -= Litter;
            return Litter;
        }
        
        public float Senescence()
        {
            float senescence = ((Root * species.TOroot) + Wood * species.TOwood);
            biomass -= senescence;

            return senescence;
        }

        private void ReduceFoliage(double fraction)
        {
            Fol *= (float)(1.0 - fraction);
            //fol = fol * 0.9
        }
        public void ReduceBiomass(double fraction)
        {
            biomass *= (float)(1.0 - fraction);
            Fol *= (float)(1.0 - fraction);
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
