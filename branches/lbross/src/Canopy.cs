using System;
using System.Collections.Generic;
using Landis.SpatialModeling;
using System.Linq;
using Landis.Core;

namespace Landis.Extension.Succession.BiomassPnET
{
    public class Canopy
    {
        public static byte MaxCanopyLayers { get; private set; }
        public static ushort MaxDevLyrAv { get; private set; }
        public static double MaxLayerStDev{get; private set;}
        

        public List<SubCanopyLayer>[] canopy;
        Landis.SpatialModeling.ActiveSite site;
        SiteCohorts sitecohorts;
        
        public int Nlayers
        { 
            get
            {
                return canopy.Count();
            }
        }
        public static void Initialize()
        {
            MaxDevLyrAv = ((Parameter<ushort>)PlugIn.GetParameter(Names.MaxDevLyrAv, 0, ushort.MaxValue)).Value;
            MaxCanopyLayers = ((Parameter<byte>)PlugIn.GetParameter(Names.MaxCanopyLayers, 0, 20)).Value;
        }

        IEcoregion ecoregion;
        public Canopy(Landis.SpatialModeling.ActiveSite site, SiteCohorts sitecohorts)
        {
            ecoregion = PlugIn.ModelCore.Ecoregion[site];
            this.site = site;
            this.sitecohorts = sitecohorts;

            SetCanopy();
        }
        
        void SetCanopy( )
        {
            foreach (List<Cohort> species_cohort in sitecohorts.cohorts.Values)
            {
                foreach (Cohort cohort in species_cohort)
                {
                    cohort.AddSubLayers(PlugIn.IMAX);
                }
            }

            List<SubCanopyLayer> subcanopylayers = GetSubcanopyLayers();

            // Determine best canopy settings
            int[] BestBin = GetCanopyLayers(subcanopylayers);

            // Set the Canopy
            SetCanopy(BestBin, subcanopylayers);

        }
        private int[] GetCanopyLayers(List<SubCanopyLayer> subcanopylayers)
        { 
            int[] BestBin = null;
            int nlayers = 0;

            MaxLayerStDev = float.MaxValue;

            int[] Bin = null;

            while (MaxLayerStDev >= MaxDevLyrAv && nlayers < MaxCanopyLayers)
            {
                nlayers++;

                Bin=  GetFirstBinPositions(nlayers, subcanopylayers.Count);

                // Kies alle mogelijke combinaties van nlayer-1 punten in the BiomRankedCohorts array
                while (Bin != null)
                {
                    // Define canopy layers given the boundary points in index_in
                    SetCanopy(Bin, subcanopylayers);

                    // Calculate total within-canopy layer variance
                    float CurrentMaxLayerStDev = GetWithinLayerMaxBiomassMaxDev(canopy);// GetWithinLayerMaxBiomassStDev();
                    if (CurrentMaxLayerStDev < MaxLayerStDev)
                    {
                        MaxLayerStDev = CurrentMaxLayerStDev;

                        BestBin = new List<int>(Bin).ToArray();
                 
                    }
                    Bin = GetNextBinPositions(Bin, subcanopylayers.Count);
                }
            }

            return BestBin;
        }




        private List<SubCanopyLayer> GetSubcanopyLayers()
        {
            List<SubCanopyLayer> subcanopylayers = new List<SubCanopyLayer>();

            foreach (List<Cohort> species_cohort in sitecohorts.cohorts.Values)
            {
                foreach (Cohort cohort in species_cohort)
                {
                    for (int i = 0; i < PlugIn.IMAX; i++)
                    {
                        ushort CumCohortBiomass = (ushort)((i + 1) / (float)PlugIn.IMAX * cohort.MaxBiomass);

                        SubCanopyLayer sl = cohort[i];
                        sl.CumCohortBiomass = CumCohortBiomass;

                        bool Inserted = false;
                        for (int c = 0; c < subcanopylayers.Count; c++)
                        {
                            if (subcanopylayers[c].CumCohortBiomass > sl.CumCohortBiomass)
                            {
                                subcanopylayers.Insert(c, sl);
                                Inserted = true;
                                break;
                            }
                        }
                        if (Inserted == false) subcanopylayers.Add(sl);

                    }
                }

            }
            return subcanopylayers;
        }
        
        private static int[] GetNextBinPositions(int[] index_in, int numcohorts)
        {
            for (int index = index_in.Length - 1; index >= 0; index--)
            {
                int maxvalue = numcohorts - index_in.Length + index - 1;
                if (index_in[index] < maxvalue)
                {
                    index_in[index]++;

                    return index_in;
                }
                else if (index_in[index] == maxvalue)
                {
                    if (index == 0) return null;

                    index_in[index - 1]++;

                    for (int i = index; i < index_in.Length; i++)
                    {
                        index_in[i] = index_in[index - 1] + i;
                    }

                }
            }
            return null;
        }
        private static float GetWithinLayerMaxBiomassMaxDev(List<SubCanopyLayer>[] proposed_canopy)
        {
            float LayerMaxDev = 0;

            for (int c = 0; c < proposed_canopy.Length; c++)
            {
                if (proposed_canopy[c].Count > 1)
                {
                    List<double> cumbiom = new List<double>();
                    foreach (SubCanopyLayer s in proposed_canopy[c])
                    {
                        cumbiom.Add(s.CumCohortBiomass);
                    }

                    double average_biomass = cumbiom.Average();
                    cumbiom = null;
                    foreach (SubCanopyLayer b in proposed_canopy[c])
                    {
                        float Deviation = (float)Math.Abs(b.CumCohortBiomass - average_biomass);
                        LayerMaxDev = (float)Math.Max(LayerMaxDev, Deviation);
                    }

                }

            }
            return LayerMaxDev;
        }
        private void SetCanopy(int[] index_in, List<SubCanopyLayer> subcanopylayers)
        {
            canopy = new List<SubCanopyLayer>[index_in.Length + 1];
            for (int ly = 0; ly < canopy.Length; ly++) canopy[ly] = new List<SubCanopyLayer>();

            for (int cohortcnt = 0; cohortcnt < subcanopylayers.Count; cohortcnt++)
            {
                bool cohortadded = false;
                for (int ly = 0; ly < index_in.Length; ly++)
                {
                    if (cohortcnt <= index_in[ly])
                    {
                        subcanopylayers[cohortcnt].CanopyLayer = (byte)(ly + 1);
                        canopy[ly].Add(subcanopylayers[cohortcnt]);
                        cohortadded = true;
                        break;
                    }
                }
                if (cohortadded == false)
                {
                    canopy[canopy.Length - 1].Add(subcanopylayers[cohortcnt]);
                    subcanopylayers[cohortcnt].CanopyLayer = (byte)canopy.Length;
                }
            }
             
        }
        private int[] GetFirstBinPositions(int nlayers, int ncohorts)
        {
            int[] Bin = new int[nlayers - 1];
            
            for (int ly = 0; ly < Bin.Length; ly++)
            {
                Bin[ly] = ly;
            }
            return Bin;
        }

        public static List<T> Shuffle<T>(List<T> array)
        {
            int n = array.Count;
            while (n > 1)
            {
                int k = PlugIn.DiscreteUniformRandom(0, n);
                n--;
                
                T temp = array[n];
                array[n] = array[k];
                array[k] = temp;
            }
            return array;
        }
        public void CalculatePhotosynthesis(EcoregionDateData data, Hydrology hydrology, ref ushort Water, ref float Radiation)
                                            
        {
            for (int CanopyLayer = canopy.Length-1 ; CanopyLayer >=0 ; CanopyLayer--)
            {
                canopy[CanopyLayer] = Shuffle<SubCanopyLayer>(canopy[CanopyLayer]);

                foreach (SubCanopyLayer subcanopylayer in canopy[CanopyLayer])
                {

                    ushort PressureHead = (ushort)Hydrology.PressureHead(ecoregion, Water);

                    subcanopylayer.cohort[subcanopylayer.LayerIndex].ComputePhotosynthesis(ref Radiation, PressureHead, data, ref Water);

                     

                    
                }
            }
           
        }
        
         
    }
}
