﻿using Landis.SpatialModeling;
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
       
        public Landis.Library.Parameters.Species.AuxParm<double> Probability
        {
            get
            {
                Landis.Library.Parameters.Species.AuxParm<double> probability = new Library.Parameters.Species.AuxParm<double>(PlugIn.ModelCore.Species);
                foreach (ISpecies spc in PlugIn.ModelCore.Species)
                {
                    ISpeciesPNET speciespnet = PlugIn.SpeciesPnET[spc];
                    probability[spc] = _pest[speciespnet];
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
     
        //public void Calculate_Establishment(IEcoregionPnETVariables pnetvars, IEcoregionPnET ecoregion, float PAR, IHydrology hydrology)
        //{
        //    foreach (ISpeciesPNET spc in PlugIn.SpeciesPnET.AllSpecies)
        //    {
              

        //        if (pnetvars.Tmin > spc.PsnTMin)
        //        {
        //            float frad = (float)Math.Pow(Cohort.ComputeFrad(PAR, spc.HalfSat), spc.EstRad);

        //            float PressureHead = hydrology.GetPressureHead(ecoregion);
                        
        //            float fwater = (float)Math.Pow(Cohort.ComputeFWater(spc.H2, spc.H3, spc.H4, PressureHead), spc.EstMoist);

        //            float pest = 1 - (float)Math.Pow(1.0 - (frad * fwater), Timestep);
        //            if (!spc.PreventEstablishment)
        //            {
        //                if (pest > _pest[spc])
        //                {
        //                    _pest[spc] = pest;
        //                    _fwater[spc] = fwater;
        //                    _frad[spc] = frad;

        //                    if (pest > (float)PlugIn.ContinuousUniformRandom())
        //                    {
        //                        if (HasEstablished(spc) == false)
        //                        {
        //                            _hasEstablished.Add(spc);
        //                        }

        //                    }

        //                }
        //            }
        //            if (establishment_siteoutput != null)
        //            {

        //                establishment_siteoutput.Add(((int)pnetvars.Year).ToString() + "," + spc.Name + "," + pest + "," + fwater + "," + frad + "," + HasEstablished(spc));

        //                // TODO: win time by reducing calls to write
        //                establishment_siteoutput.Write();
        //            }
        //        }
        //    }
        //}
        public Dictionary<ISpeciesPNET,float> Calculate_Establishment_Month(IEcoregionPnETVariables pnetvars, IEcoregionPnET ecoregion, float PAR, IHydrology hydrology)
        {
            Dictionary<ISpeciesPNET, float> estabDict = new Dictionary<ISpeciesPNET, float>();

            foreach (ISpeciesPNET spc in PlugIn.SpeciesPnET.AllSpecies)
            {
                if (pnetvars.Tmin > spc.PsnTMin)
                {
                    float frad = (float)Math.Pow(Cohort.ComputeFrad(PAR, spc.HalfSat), spc.EstRad);

                    float PressureHead = hydrology.GetPressureHead(ecoregion);

                    float fwater = (float)Math.Pow(Cohort.ComputeFWater(spc.H2, spc.H3, spc.H4, PressureHead), spc.EstMoist);

                    float pest = 1 - (float)Math.Pow(1.0 - (frad * fwater), Timestep);
                    estabDict[spc] = pest;
                    if (fwater < _fwater[spc])
                    {
                        _fwater[spc] = fwater;
                    }
                    if (frad < _frad[spc])
                    {
                        _frad[spc] = frad;
                    }

                    /*if (establishment_siteoutput != null)
                    {

                        establishment_siteoutput.Add(((int)pnetvars.Year).ToString() + "," + spc.Name + "," + pest + "," + fwater + "," + frad + "," + HasEstablished(spc));

                        // TODO: win time by reducing calls to write
                        establishment_siteoutput.Write();
                    }
                     * */
                }
                
            }
            return estabDict;
        }
        public void ResetPerTimeStep()
        {
         
            _pest = new Dictionary<ISpeciesPNET, float>();
            _fwater = new Dictionary<ISpeciesPNET, float>();
            _frad = new Dictionary<ISpeciesPNET, float>();
            _hasEstablished = new List<ISpeciesPNET>();

            foreach (ISpeciesPNET spc in PlugIn.SpeciesPnET.AllSpecies)
            {
                _pest.Add(spc, 1);
                _fwater.Add(spc, 1);
                _frad.Add(spc, 1);
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

        public void EstablishmentTrue(ISpeciesPNET spc)
        {
            _hasEstablished.Add(spc);
        }
        
        public void RecordPest(int year, ISpeciesPNET spc, float annualPest, bool estab)
        {
            _pest[spc] = annualPest;
            if (estab)
            {
                if (HasEstablished(spc) == false)
                {
                    _hasEstablished.Add(spc);
                }
            }
            if (establishment_siteoutput != null)
            {
               
                establishment_siteoutput.Add(year.ToString() + "," + spc.Name + "," + annualPest + "," + _fwater[spc] + "," + _frad[spc] + "," + HasEstablished(spc));

                // TODO: win time by reducing calls to write
                establishment_siteoutput.Write();
            }
        }
    }
}
