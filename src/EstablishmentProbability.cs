using Landis.SpatialModeling;
using Landis.Core;
using System.Linq;
using System;
using System.Collections.Generic;

namespace Landis.Extension.Succession.BiomassPnET
{
    public class EstablishmentProbability  : IEstablishmentProbability
    {
        private LocalOutput establishment_siteoutput;
        private List<ISpeciesPNET> _hasEstablished;
        private Dictionary<ISpeciesPNET, float> _pest;
        private Dictionary<ISpeciesPNET, float> _fwater;
        private Dictionary<ISpeciesPNET, float> _frad;

        private static int Timestep;

       
        public bool HasEstablished(ISpeciesPNET species)
        {
            return _hasEstablished.Contains(species);
        }
       
        public Landis.Library.Parameters.Species.AuxParm<byte> Probability
        {
            get
            {
                Landis.Library.Parameters.Species.AuxParm<byte> probability = new Library.Parameters.Species.AuxParm<byte>(PlugIn.ModelCore.Species);
                foreach (ISpecies spc in PlugIn.ModelCore.Species)
                {
                    ISpeciesPNET speciespnet = SpeciesPnET.AllSpecies[spc];
                    probability[spc] = (byte)(100F * _pest[speciespnet]);
                }
                return probability;
            }
        }
        
        public string Header
        {
            get
            {
                return "Year" + "," + "Species" + "," + "Pest" + "," + "FWater" +"," + "FRad" +"," + "Est";
            }
        }
        
       
        public static void Initialize(int timestep)
        {
            Timestep = timestep;

             
        }
     
        public void Calculate_Establishment(EcoregionPnETVariables pnetvars, IEcoregionPnET ecoregion, float PAR, float water)
        {
            foreach (ISpeciesPNET spc in SpeciesPnET.AllSpecies.Values)
            {
                if (pnetvars[spc.Name].LeafOn)
                {
                    float frad = (float)Math.Pow(Cohort.CumputeFrad(PAR, spc.HalfSat), spc.EstRad);

                    float PressureHead = Hydrology.Pressureheadtable[ecoregion, (int)water];

                    float fwater = (float)Math.Pow(Cohort.CumputeFWater(spc.H2, spc.H3, spc.H4, PressureHead), spc.EstMoist);

                    float pest = 1 - (float)Math.Pow(1.0 - (frad * fwater), Timestep);

                    if (pest > _pest[spc])
                    {
                        _pest[spc] = pest;
                        _fwater[spc] = fwater;
                        _frad[spc] = frad;

                        if (pest > (float)PlugIn.ContinuousUniformRandom())
                        {
                            if (HasEstablished(spc) == false)
                            {
                                _hasEstablished.Add(spc);
                            }
                        
                        }
                        
                    }
                    if (establishment_siteoutput != null)
                    {

                        establishment_siteoutput.Add(pnetvars.Date.Year.ToString() + "," + spc.Name + "," + pest + "," + fwater + "," + frad + "," + HasEstablished(spc));

                        // TODO: win time by reducing calls to write
                        establishment_siteoutput.Write();
                    }
                }
            }
        }
        public void ResetPerTimeStep()
        {
         
            _pest = new Dictionary<ISpeciesPNET, float>();
            _fwater = new Dictionary<ISpeciesPNET, float>();
            _frad = new Dictionary<ISpeciesPNET, float>();
            _hasEstablished = new List<ISpeciesPNET>();

            foreach (ISpeciesPNET spc in SpeciesPnET.AllSpecies.Values)
            {
                _pest.Add(spc, 0);
                _fwater.Add(spc, 0);
                _frad.Add(spc, 0);
            }
        }
        public EstablishmentProbability(string SiteOutputName, string FileName)
        {
            ResetPerTimeStep();
             
            if(SiteOutputName!=null && FileName!=null)
            {
                establishment_siteoutput = new LocalOutput(SiteOutputName, "Establishment.csv", Header );
            }
            
        }
        
    }
}
