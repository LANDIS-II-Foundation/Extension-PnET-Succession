// uses dominance to allocate psn and subtract transpiration from soil water, average cohort vars over layer

using Landis.SpatialModeling;
using Landis.Core;
using Edu.Wisc.Forest.Flel.Util;
using System;
using System.Linq;

namespace Landis.Extension.Succession.BiomassPnET 
{
    public class Cohort : Landis.Library.AgeOnlyCohorts.ICohort, Landis.Library.BiomassCohorts.ICohort
    {
        public static event Landis.Library.BiomassCohorts.DeathEventHandler<Landis.Library.BiomassCohorts.DeathEventArgs> DeathEvent;
        public static event Landis.Library.BiomassCohorts.DeathEventHandler<Landis.Library.BiomassCohorts.DeathEventArgs> AgeOnlyDeathEvent;

        static ushort Rootsenescence;
        static ushort Woodsenescence;
        public float MaxBiomass;
        public ushort Age { get; set; }
        public bool IsAlive;
        System.Collections.Generic.List<SubCanopyLayer> SubCanopyLayers;
        public ISpecies species { get; private set; }
        public ushort Root;
        public ushort Wood;
        public ushort Fol;

        public float NSC;
        public float MaintenanceRespiration; 
        
        public LocalOutput cohortoutput{get; private set;}

        //public delegate void AllocateLitters(Cohort cohort, ActiveSite site, ExtensionType disturbanceType);

        //public static event AllocateLitters allocatelitters;

        public float Fage()
        {
            return Math.Max(0, 1 - (float)Math.Pow((Age / (float)species.Longevity), species.PsnAgeRed()));
        }
        public string outputfilename
        {
            get
            {
                return cohortoutput.FileName;
            }
        }
        

        public SubCanopyLayer this[int ly]
        {
            get
            {
                return SubCanopyLayers[ly];
            }
        }

        public float NSCfrac
        {
            get
            {
                return NSC / (FActiveBiom() * (Wood + Root) + Fol);
            }
        }

        public int Layer
        {
            get
            {
                return SubCanopyLayers.Max(o => o.CanopyLayer);
            }
        }

        public float Radiation
        {
            get
            {
                return (float)SubCanopyLayers.Max(o => o.Radiation);
            }

        }
        public float Fwater
        {
            get
            {
                return SubCanopyLayers.Average(o => o.Fwater);
            }
        }
        public float Frad
        {
            get
            {
                return SubCanopyLayers.Average(o => o.Frad);
            }
        }

        public float LAI
        {
            get
            {
                return SubCanopyLayers.Sum(o => o.LAI);
            }
        }



        public float FolResp
        {
            get
            {
                return SubCanopyLayers.Sum(o => o.FolResp);
            }
        }
        public float Transpiration
        {
            get
            {
                return SubCanopyLayers.Sum(o => o.Transpiration);
            }
        }
        public float Netpsn
        {
            get
            {
                return SubCanopyLayers.Sum(o => o.NetPsn);
            }
        }
        public float Grosspsn
        {
            get
            {
                return Netpsn + FolResp;
            }
        }

        public Landis.Core.ISpecies Species
        {
            get
            {
                return species;
            }
        }

        public float WaterUseEfficiency
        {
            get
            {
                if (Transpiration > 0) return Netpsn / Transpiration;
                return 0;
            }

        }
        public void ClearSubLayers()
        {
            SubCanopyLayers = null;
        }
        public void AddSubLayers(int NrOfSublayers)
        {
            SubCanopyLayers = new System.Collections.Generic.List<SubCanopyLayer>();
            for (byte i = 0; i < NrOfSublayers; i++)
            {
                SubCanopyLayers.Add(new SubCanopyLayer(this, i));
            }
        }

        public Cohort(ISpecies species, ushort year_of_birth, byte IMAX, float InitialNSC, float DNSC)
        {
            this.species = species;
            this.Age = 0;
            this.NSC = (ushort)InitialNSC;
            this.Wood = (ushort)(1F / DNSC * (ushort)InitialNSC);
            this.MaxBiomass = this.Biomass;
            this.IsAlive = true;
        }
        public Cohort(Cohort cohort, int IMAX)
        {
            SubCanopyLayers = new System.Collections.Generic.List<SubCanopyLayer>();
            this.MaxBiomass = cohort.MaxBiomass;
            this.species = cohort.species;
            this.Age = cohort.Age;
            this.Wood = cohort.Wood;
            this.NSC = cohort.NSC;
            this.Root = cohort.Root;
            this.Fol = cohort.Fol;
            this.MaxBiomass = cohort.MaxBiomass;
            this.IsAlive = true;

        }




        public int Biomass
        {
            get
            {
                return (int)(Wood + Fol);
            }
        }
        public int ComputeNonWoodyBiomass(ActiveSite site)
        {
            return (int)(Fol);
        }
        public static Percentage ComputeNonWoodyPercentage(Cohort cohort, ActiveSite site)
        {
            return new Percentage(cohort.Fol / (cohort.Wood + cohort.Fol));
        }


        public void InitializeOutput(string SiteName, ushort YearOfBirth, LocalOutput.SendMsg SendMsg)
        {
            cohortoutput = new LocalOutput(SiteName, "Cohort_" + Species.Name + "_" + YearOfBirth + ".csv", OutputHeader, SendMsg);
        }

        public void UpdateCohortData(float year, ActiveSite site, float FTempPSN, float FTempResp, bool Leaf_On)
        {
            string s = Math.Round(year, 2) + "," + Age + "," + Layer + "," + LAI + "," + Grosspsn + "," +
                       FolResp + "," + MaintenanceRespiration + "," + Netpsn + "," + WaterUseEfficiency + "," + Fol + "," + Root + "," + Wood + "," + NSC + "," +
                       NSCfrac + "," + Fwater + "," + Radiation + "," + Frad + "," + FTempPSN + "," + FTempResp + "," + Fage() + "," + Leaf_On + "," +
                       FActiveBiom();

            cohortoutput.Add(s);
        }

        public string OutputHeader
        {
            get
            {
                string hdr = OutputHeaders.Time + "," + OutputHeaders.Age + "," + OutputHeaders.Layer + "," + OutputHeaders.LAI + "," +
                OutputHeaders.GrossPsn + "," + OutputHeaders.FolResp + "," + OutputHeaders.MaintResp + "," + OutputHeaders.NetPsn + "," + OutputHeaders.WUE + ","
                + OutputHeaders.Fol + "," + OutputHeaders.Root + "," + OutputHeaders.Wood + "," +
                OutputHeaders.NSC + "," + OutputHeaders.NSCfrac + "," + OutputHeaders.fWater + "," + OutputHeaders.Radiation + "," + OutputHeaders.fRad + "," + OutputHeaders.fTemp_psn + "," +
                OutputHeaders.fTemp_resp + "," + OutputHeaders.fage + "," + OutputHeaders.LeafOn + "," + OutputHeaders.FActiveBiom + ",";

                return hdr;
            }
        }
        public void WriteCohortData()
        {
            cohortoutput.Write();

        }

        


 
        

        
        public float FActiveBiom()
        {
            return (float)Math.Exp(-species.FrActWd() * MaxBiomass);
        }


        public float FoliageSenescence(EcoregionDateData monthdata)
        {
            // If it is fall 
            ushort Litter = (ushort)(species.TOfol() * Fol);
            Fol -= Litter;

            if( IsAlive) IsAlive =NSCfrac > 0.01F;

            
            return Litter;

        }

        public float Senescence()
        {
            Rootsenescence = (ushort)(Root * species.TOroot());
            Root -= Rootsenescence;

            Woodsenescence = (ushort)(Wood * species.TOwood());
            Wood -= Woodsenescence;

            return Woodsenescence + Rootsenescence;
        }
         
        //---------------------------------------------------------------------
        /// <summary>
        /// Raises a Cohort.AgeOnlyDeathEvent.
        /// </summary>
        public static void Died(object sender,
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
            Allocation.Allocate(sender, cohort, disturbanceType);
        }
 
        
    } 
}
