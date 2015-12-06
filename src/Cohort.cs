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

        public static SubtractTranspiration subtract_transpiration;
        public static AddWoodyDebris addwoodydebris;
        public static AddLitter addlitter;
        
        
        private float biomassmax;
        private float biomass; // root + wood
        private float fol;
        private float nsc;
        private ushort age;
        
         
        public ushort index;
        
        private ISpeciesPNET species;
        private LocalOutput cohortoutput;

        public float[] LAI = null;
        public float[] GrossPsn = null;
        public float[] FolResp = null;
        public float[] NetPsn = null;
        public float[] Transpiration = null;
        public float[] FRad = null;
        public float[] FWater = null;
        public float[] MaintenanceRespiration = null;
        public float[] Interception = null;

        public void InitializeSubLayers()
        {
            index = 0;
            LAI = new float[PlugIn.IMAX];
            GrossPsn = new float[PlugIn.IMAX];
            FolResp = new float[PlugIn.IMAX];
            NetPsn = new float[PlugIn.IMAX];
            Transpiration = new float[PlugIn.IMAX];
            FRad = new float[PlugIn.IMAX];
            FWater = new float[PlugIn.IMAX];
            MaintenanceRespiration = new float[PlugIn.IMAX];
            //ConductanceCO2 = new float[PlugIn.IMAX];
            Interception = new float[PlugIn.IMAX];
        }
        public void NullSubLayers()
        {
            LAI = null;
            GrossPsn = null;
            FolResp = null;
            NetPsn = null;
            Transpiration = null;
            FRad = null;
            FWater = null;
            MaintenanceRespiration = null;
            //ConductanceCO2 = null;
            Interception = null;
        }
      
        public ushort Age
        {
            get
            {
                return age;
            }
        }
        public float NSC
        {
            get
            {
                return nsc;
            }
        }
        public float Fol
        {
            get
            {
                return fol;
            }
        }
        public int Biomass
        {
            get
            {
                return (int)biomass;
            }
        }
        public uint Wood
        {
            get
            {
                return (uint)((1 - species.FracBelowG) * biomass);
            }
        }
        public uint Root
        {
            get
            {
                return (uint)(species.FracBelowG * biomass);
            }
        }
        public void ReduceBiomass(double fraction)
        {
            biomass *= (float)(1.0 - fraction);
            fol *= (float)(1.0 - fraction);
        }
        public float BiomassMax
        {
            get
            {
                return biomassmax;
            }
        }
     
 
        

        public void Accumulate(Cohort c)
        {
            biomass += c.biomass;
            biomassmax = Math.Max(biomassmax, biomass);
            fol += c.Fol;
        }
       
        public string outputfilename
        {
            get
            {
                return cohortoutput.FileName;
            }
        }
        public float NSCfrac
        {
            get
            {
                return nsc / (FActiveBiom * (biomass + fol));
            }
        }
        public ISpeciesPNET SpeciesPNET
        {
            get
            {
                return species;
            }
        }
        public Landis.Core.ISpecies Species
        {
            get
            {
                return species;
            }
        }

        public Cohort(ISpeciesPNET species, ushort year_of_birth, string SiteName)
        {
            this.species = species;
            age = 0; 
           
            this.nsc = (ushort)species.InitialNSC;
           
            this.biomass = (uint)(1F / species.DNSC * (ushort)species.InitialNSC);
            biomassmax = biomass;

            // First declare default variables
            
            // Then overwrite them if you need stuff for outputs6+
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
        public static void SetSiteAccessFunctions(SiteCohorts sitecohorts)
        {
             Cohort.subtract_transpiration = sitecohorts.SubtractTranspiration;
             Cohort.addlitter = sitecohorts.AddLitter;
             Cohort.addwoodydebris = sitecohorts.AddWoodyDebris;
             Cohort.ecoregion = sitecohorts.Ecoregion;
        }
        bool leaf_on = true;

        public static IEcoregionPnET ecoregion;

        float Fage
        {
            get
            {
                return Math.Max(0, 1 - (float)Math.Pow((age / (float)species.Longevity), species.PsnAgeRed));
            }
        }

        public void CalculatePhotosynthesis(float PrecInByCanopyLayer, float LeakagePerCohort, IHydrology hydrology, ref float SubCanopyPar)
        {
            
            bool success = true;

            LAI[index] = PlugIn.fIMAX * fol / (species.SLWmax - species.SLWDel * index * PlugIn.fIMAX * fol);

            Interception[index] = PrecInByCanopyLayer * (float)(1 - Math.Exp(-1 * ecoregion.PrecIntConst * LAI[index]));

            float waterIn = PrecInByCanopyLayer  - Interception[index]; //mm   

            success = hydrology.AddWater(waterIn);
            if (success == false) throw new System.Exception("Error adding water, waterIn = " + waterIn + " water = " + hydrology.Water);
           

            // Instantaneous runoff (excess of porosity)
            float runoff = Math.Max(hydrology.Water - ecoregion.Porosity, 0);
            success = hydrology.AddWater(-1 * runoff);
            if (success == false) throw new System.Exception("Error adding water, runoff = " + runoff + " water = " + hydrology.Water);

            // Fast Leakage 
            Hydrology.Leakage = Math.Max(LeakagePerCohort * (hydrology.Water - ecoregion.FieldCap), 0);
            success = hydrology.AddWater(-1 * Hydrology.Leakage);
            if (success == false) throw new System.Exception("Error adding water, Hydrology.Leakage = " + Hydrology.Leakage + " water = " + hydrology.Water);

            MaintenanceRespiration[index] = PlugIn.fIMAX * (float)Math.Min(NSC, ecoregion.Variables[Species.Name].MaintRespFTempResp * biomass);//gC //IMAXinverse
            nsc -= MaintenanceRespiration[index];


            if (index == PlugIn.IMAX - 1)
            {
                
                if (ecoregion.Variables.Month == (int)Constants.Months.January)
                {

                    addwoodydebris(Senescence(), species.KWdLit);

                    float Allocation = Math.Max(nsc - (species.DNSC * FActiveBiom * biomass), 0);
                    biomass += Allocation;
                    biomassmax = Math.Max(biomassmax, biomass);
                    nsc -= Allocation;

                    age++;
                }
            }
             
            leaf_on = GetLeafOn(ecoregion.Variables);
             
            if (leaf_on)
            {
                float IdealFol = (species.FracFol * FActiveBiom * biomass);

                if (IdealFol > fol)
                {
                    float Folalloc = Math.Max(0, Math.Min(nsc, species.CFracBiomass * (IdealFol - fol))); // gC/mo

                    fol += Folalloc / species.CFracBiomass;// gDW
                    nsc -= Folalloc;
                }
            }
            
            FRad[index] = CumputeFrad(SubCanopyPar, species.HalfSat);

            SubCanopyPar *= (float)Math.Exp(-species.K * LAI[index]);

            float PressureHead = hydrology.GetPressureHead(ecoregion);
            //Pressureheadtable[(IEcoregion)ecoregion, (ushort)Water];

            FWater[index] = CumputeFWater(species.H2, species.H3, species.H4, PressureHead);

            // g/mo
            if (leaf_on)
            {
                NetPsn[index] = PlugIn.fIMAX * FWater[index] * FRad[index] * Fage * ecoregion.Variables[species.Name].FTempPSNRefNetPsn * fol;

                float FTempRespDayRefResp = ecoregion.Variables.DaySpan * ecoregion.Variables.Daylength * Constants.MC / Constants.billion * ecoregion.Variables[species.Name].Amax;

                FolResp[index] = FWater[index] * ecoregion.Variables[species.Name].FTempRespDay * fol *  PlugIn.fIMAX;

                GrossPsn[index] = NetPsn[index] + FolResp[index];

                if (NetPsn[index] < 0) throw new System.Exception("NetPsn = " + NetPsn[index]);
                if (FolResp[index] < 0) throw new System.Exception("FolResp = " + FolResp[index]);

                Transpiration[index] = GrossPsn[index] * Constants.MCO2_MC / ecoregion.Variables[Species.Name].WUE_CO2_corr;
                 
                subtract_transpiration(Transpiration[index], SpeciesPNET);

                nsc += NetPsn[index];
            }
            else
            {
                NetPsn[index] = 0;
                FolResp[index] = 0;
                GrossPsn[index] = 0;
                Transpiration[index] = 0;

            }

            if (index < PlugIn.IMAX - 1) index++;
            return;
        }

        private bool GetLeafOn(EcoregionPnETVariables monthdata)
        {
            bool Leaf_on = monthdata[species.Name].LeafOn;

            if (leaf_on == true && Leaf_on == false)
            {
                leaf_on = false;
                addlitter(FoliageSenescence(), SpeciesPNET);
            }
            return Leaf_on;
        }
       
        public static float CumputeFrad(float Radiation, float HalfSat)
        {
            return Radiation / (Radiation + HalfSat);
        }
        public static float CumputeFWater(float H2, float H3, float H4, float pressurehead)
        {
            if (pressurehead < 0 || pressurehead > H4) return 0;
            else if (pressurehead > H3) return 1 - ((pressurehead - H3) / (H4 - H3));
            else if (pressurehead < H2) return pressurehead / H2;
            else return 1;
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
        public void UpdateCohortData(EcoregionPnETVariables monthdata )
        {
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
                       fol + "," + 
                       Root + "," + 
                       Wood + "," + 
                       NSC + "," +
                       NSCfrac + "," +
                       FWater.Average() + "," +
                       FRad.Average() + "," +
                       monthdata[Species.Name].FTempPSN + "," +
                       monthdata[Species.Name].FTempRespWeightedDayAndNight + "," +
                       Fage + "," +
                       monthdata[Species.Name].LeafOn + "," +
                       FActiveBiom;
             
            cohortoutput.Add(s);

       
        }

        public string OutputHeader
        {
            get
            {
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
