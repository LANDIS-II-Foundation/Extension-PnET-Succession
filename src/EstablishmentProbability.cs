﻿using Landis.Core;
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
                    ISpeciesPNET speciespnet = PlugIn.SpeciesPnET[spc];
                    probability[spc] = (byte)(100F * _pest[speciespnet]);
                }
                return probability;
            }
        }
        public float Get_FWater(ISpeciesPNET species)
        {
            {
                return _fwater[species];
            }
        }
        public float Get_FRad(ISpeciesPNET species)
        {
            {
                return _frad[species];
            }
        }

        public string Header
        {
            get
            {
                return "Year" + "," + "Species" + "," + "Pest" + "," + "FWater_Avg" +"," + "FRad_Avg" +","+"ActiveMonths"+"," + "Est";
            }
        }
        
       
        public static void Initialize(int timestep)
        {
            Timestep = timestep;

             
        }
     
        /*public void Calculate_Establishment(IEcoregionPnETVariables pnetvars, IEcoregionPnET ecoregion, float PAR, IHydrology hydrology)
        {
            foreach (ISpeciesPNET spc in PlugIn.SpeciesPnET.AllSpecies)
            {
              

                if (pnetvars.Tmin > spc.PsnTMin)
                {
                    // Adjust HalfSat for CO2 effect
                    float halfSatIntercept = spc.HalfSat - 350 * spc.CO2HalfSatEff;
                    float adjHalfSat = spc.CO2HalfSatEff * pnetvars.CO2 + halfSatIntercept;
                    float frad = (float)Math.Pow(Cohort.ComputeFrad(PAR, adjHalfSat), spc.EstRad);


                    float PressureHead = hydrology.GetPressureHead(ecoregion);
                        
                    float fwater = (float)Math.Pow(Cohort.ComputeFWater(spc.H1,spc.H2, spc.H3, spc.H4, PressureHead), spc.EstMoist);

                    float pest = 1 - (float)Math.Pow(1.0 - (frad * fwater), Timestep);
                    if (!spc.PreventEstablishment)
                    {
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
                    }
                    if (establishment_siteoutput != null)
                    {

                        establishment_siteoutput.Add(((int)pnetvars.Year).ToString() + "," + spc.Name + "," + pest + "," + fwater + "," + frad + "," + HasEstablished(spc));

                        // TODO: win time by reducing calls to write
                        establishment_siteoutput.Write();
                    }
                }
            }
        }
        */

        public Dictionary<ISpeciesPNET,float> Calculate_Establishment_Month(IEcoregionPnETVariables pnetvars, IEcoregionPnET ecoregion, float PAR, IHydrology hydrology)
        {
            Dictionary<ISpeciesPNET, float> estabDict = new Dictionary<ISpeciesPNET, float>();
            _fwater = new Dictionary<ISpeciesPNET, float>();
            _pest = new Dictionary<ISpeciesPNET, float>();
            _frad = new Dictionary<ISpeciesPNET, float>();

            foreach (ISpeciesPNET spc in PlugIn.SpeciesPnET.AllSpecies)
            {
                if (pnetvars.Tmin > spc.PsnTMin && pnetvars.Tmax < spc.PsnTMax)
                {
                    // Adjust HalfSat for CO2 effect
                    float halfSatIntercept = spc.HalfSat - 350 * spc.CO2HalfSatEff;
                    float adjHalfSat = spc.CO2HalfSatEff * pnetvars.CO2 + halfSatIntercept;
                    float frad = (float)Math.Pow(Cohort.ComputeFrad(PAR, adjHalfSat), spc.EstRad);

                    //float frad = (float)Math.Pow(Cohort.ComputeFrad(PAR, spc.HalfSat), spc.EstRad);

                    float PressureHead = hydrology.GetPressureHead(ecoregion);

                    float fwater = (float)Math.Pow(Cohort.ComputeFWater(spc.H1,spc.H2, spc.H3, spc.H4, PressureHead), spc.EstMoist);

                    float pest = 1 - (float)Math.Pow(1.0 - (frad * fwater), Timestep);
                    estabDict[spc] = pest;
                    _pest[spc] = pest;
                    _fwater[spc] = fwater;
                    _frad[spc] = frad;
                    /*if (fwater < _fwater[spc])
                    {
                        _fwater[spc] = fwater;
                    }
                    if (frad < _frad[spc])
                    {
                        _frad[spc] = frad;
                    }
                    */
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
        
        public void RecordPest(int year, ISpeciesPNET spc, float annualPest, float annualfWater, float annualfRad, bool estab, int monthCount)
        {
            if (estab)
            {
                if (HasEstablished(spc) == false)
                {
                    _hasEstablished.Add(spc);
                }
            }
            if (establishment_siteoutput != null)
            {
                if (monthCount == 0)
                {
                    establishment_siteoutput.Add(year.ToString() + "," + spc.Name + "," + annualPest + "," + 0 + "," + 0 + ","+0+"," + HasEstablished(spc));
                }
                else
                {
                    establishment_siteoutput.Add(year.ToString() + "," + spc.Name + "," + annualPest + "," + annualfWater + "," + annualfRad + ","+monthCount+"," + HasEstablished(spc));
                }
                // TODO: win time by reducing calls to write
                establishment_siteoutput.Write();
            }
        }
    }
}
