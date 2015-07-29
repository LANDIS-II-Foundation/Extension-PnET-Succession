// uses dominance to allocate psn and subtract transpiration from soil water, average cohort vars over layer

using Landis.SpatialModeling;
using Landis.Core;
using Edu.Wisc.Forest.Flel.Util;
using System;
using System.Collections.Generic;

namespace Landis.Extension.Succession.BiomassPnET 
{
    public class Cohort : Landis.Library.AgeOnlyCohorts.ICohort, Landis.Library.BiomassCohorts.ICohort, IDisposable
    {
        public static event Landis.Library.BiomassCohorts.DeathEventHandler<Landis.Library.BiomassCohorts.DeathEventArgs> DeathEvent;
        public static event Landis.Library.BiomassCohorts.DeathEventHandler<Landis.Library.BiomassCohorts.DeathEventArgs> AgeOnlyDeathEvent;


        public delegate void SubtractTranspiration(float transpiration, ISpecies Species);
        public delegate void AddWoodyDebris(float Litter, float KWdLit);
        public delegate void AddLitter(float AddLitter, ISpecies Species);

        public static SubtractTranspiration subtract_transpiration;
        public static AddWoodyDebris addwoodydebris;
        public static AddLitter addlitter;
        
        
        private float biomassmax;
        private float biomass; // root + wood
        private float fol;
        private float nsc;
        private ushort age;
         

        
        private ISpecies species;
        private LocalOutput cohortoutput;
        private SubCohortVars canopy;
        private SubCohortVars layer;
        public CohortAuxiliaryPars auxpars;
         
        public void Dispose()
        {
            if (canopy != null) canopy.Add(layer);
            layer = null;
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
                return (uint)((1 - Species.FracBelowG()) * biomass);
            }
        }
        public uint Root
        {
            get
            {
                return (uint)(Species.FracBelowG() * biomass);
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
        public SubCohortVars Canopy
        {
            get
            {
                return canopy;
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
        public Landis.Core.ISpecies Species
        {
            get
            {
                return species;
            }
        }

        public Cohort(ISpecies species, ushort year_of_birth, string SiteName)
        {
            this.species = species;
            age = 0;

            this.nsc = (ushort)species.InitialNSC();

           
            this.biomass = (uint)(1F / species.DNSC() * (ushort)species.InitialNSC());
            biomassmax = biomass;

            // First declare default variables
            
            // Then overwrite them if you need stuff for outputs6+
            if (SiteName != null)
            {
                InitializeOutput(SiteName, year_of_birth, PlugIn.ModelCore.UI.WriteLine);
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
        }
        public void CalculatePhotosynthesis(ref float SubCanopyPar, uint PressureHead, ref float CanopyLAI)
        {
            layer = new SubCohortVars();

            if (auxpars == null)
            {
                auxpars = new CohortAuxiliaryPars();
            }

            if (auxpars.index == 0)
            {
                auxpars.Update(age, Species, biomass);
            }
             
            if (auxpars.index == PlugIn.IMAX - 1)
            {
                auxpars.index = 0;

                if (SiteCohorts.monthdata.Month == (int)Constants.Months.January)
                {
                    layer.MaintenanceRespiration = 0;// (float)Math.Min(NSC, SiteCohorts.monthdata.FTempRespMaintResp[Species] * biomass);//gC //IMAXinverse
                    nsc -= layer.MaintenanceRespiration;

                    addwoodydebris(Senescence(), Species.KWdLit());

                    float Allocation = Math.Max(nsc - (Species.DNSC() * FActiveBiom * biomass), 0);
                    biomass += Allocation;
                    biomassmax = Math.Max(biomassmax, biomass);
                    nsc -= Allocation;

                    age++;
                    

                }

                if (SiteCohorts.monthdata.Leaf_Change[Species])
                {
                    addlitter(FoliageSenescence(), Species);
                }
            }
            else auxpars.index++;

            layer.LAI = PlugIn.fIMAX * fol / (Species.SLWmax() - species.SLWDel() * auxpars.index * PlugIn.fIMAX * fol);
            CanopyLAI += layer.LAI;

            if (SiteCohorts.monthdata.Leaf_On[Species] == false)
            {
                return;
            }

            float IdealFol = (Species.FracFol() * FActiveBiom * biomass);
            
            if (IdealFol > fol)
            {
                float Folalloc = Math.Max(0, Math.Min(nsc, Species.CFracBiomass() * (IdealFol - fol))); // gC/mo
                 
                fol += (Folalloc / Species.CFracBiomass());// gDW
                nsc -= Folalloc;
            }

            if (Fol == 0)
            {
                //running_values.FWater = float.NaN;
                return;
            }



            layer.FRad = CumputeFrad(SubCanopyPar, species.HalfSat());

            SubCanopyPar += (float)Math.Exp(-species.K() * layer.LAI);

            layer.FWater = CumputeFWater(Species.H2(), Species.H3(), Species.H4(), PressureHead);

            if (layer.FWater == 0)
            {
                //running_values.FRad = 0;
                return;
            }
            
            // g/mo
            layer.NetPsn = layer.FWater * layer.FRad * auxpars.fage * SiteCohorts.monthdata.FTempPSNRefNetPsn[species] * PlugIn.fIMAX * fol;

            layer.ConductanceCO2 = (SiteCohorts.monthdata.gsInt + (SiteCohorts.monthdata.gsSlope * (layer.NetPsn * 1000000 / 12F)));// *(1 - O3Effect[Layer]);

            layer.FolResp = layer.FWater * SiteCohorts.monthdata.FTempRespDayRefResp[species] * fol * PlugIn.fIMAX;

            layer.GrossPsn = layer.NetPsn + layer.FolResp;

            layer.Transpiration = layer.GrossPsn * Constants.MCO2_MC / SiteCohorts.monthdata.WUE_CO2_corr[Species];

            subtract_transpiration(layer.Transpiration, Species);

            nsc += layer.NetPsn;

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
        public void InitializeOutput(string SiteName, ushort YearOfBirth, LocalOutput.SendMsg SendMsg)
        {
            cohortoutput = new LocalOutput(SiteName, "Cohort_" + Species.Name + "_" + YearOfBirth + ".csv", OutputHeader, SendMsg);
            canopy = new SubCohortVars();
        }
       
        public void UpdateCohortData()
        {

            string s = Math.Round(SiteCohorts.monthdata.Year, 2) + "," + 
                        Age + "," +
                      //  Layer + "," + 
                       canopy.ConductanceCO2 + "," +
                       canopy.LAI + "," +
                       canopy.GrossPsn + "," +
                       canopy.FolResp + "," +
                       canopy.MaintenanceRespiration + "," +
                       canopy.NetPsn + "," +                  // Sum over canopy layers
                       canopy.Transpiration + "," +
                       ((canopy.FolResp > 0) ? canopy.GrossPsn / canopy.FolResp : float.NaN).ToString() + "," +
                       fol + "," + 
                       Root + "," + 
                       Wood + "," + 
                       NSC + "," +
                       NSCfrac + "," +
                       canopy.FWater + "," +
                       canopy.FRad + "," +
                       SiteCohorts.monthdata.FTempPSN[Species] + "," +
                       SiteCohorts.monthdata.FTempResp[Species] + "," +
                       auxpars.fage + "," +
                       SiteCohorts.monthdata.Leaf_On[Species] + "," +
                       FActiveBiom;
             
            cohortoutput.Add(s);

            canopy.Reset();
        }

        public string OutputHeader
        {
            get
            {
                string hdr = OutputHeaders.Time + "," + 
                            OutputHeaders.Age + "," +
                            OutputHeaders.ConductanceCO2 + "," + 
                            //OutputHeaders.Layer + "," + 
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
            canopy.Reset();
        }
         
        public float FActiveBiom
        {
            get
            {
                return (float)Math.Exp(-species.FrActWd() * biomass);
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
            ushort Litter = (ushort)(species.TOfol() * fol);
            fol -= Litter;

            return Litter;

        }
        
        public float Senescence()
        {
            float senescence = ((Root * species.TOroot()) + Wood * species.TOwood());
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
