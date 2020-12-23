using Landis.Core;
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

        public Dictionary<ISpeciesPNET,float> Calculate_Establishment_Month(IEcoregionPnETVariables pnetvars, IEcoregionPnET ecoregion, float PAR, IHydrology hydrology,float minHalfSat, float maxHalfSat, bool invertPest)
        {
            Dictionary<ISpeciesPNET, float> estabDict = new Dictionary<ISpeciesPNET, float>();
            //_fwater = new Dictionary<ISpeciesPNET, float>();
            //_pest = new Dictionary<ISpeciesPNET, float>();
            //_frad = new Dictionary<ISpeciesPNET, float>();
            float halfSatRange = maxHalfSat - minHalfSat;

            foreach (ISpeciesPNET spc in PlugIn.SpeciesPnET.AllSpecies)
            {
                if (pnetvars.Tmin > spc.PsnTMin && pnetvars.Tmax < spc.PsnTMax)
                {
                    // Adjust HalfSat for CO2 effect
                    float halfSatIntercept = spc.HalfSat - 350 * spc.CO2HalfSatEff;
                    float adjHalfSat = spc.CO2HalfSatEff * pnetvars.CO2 + halfSatIntercept;
                    float frad = (float)(Math.Min(1.0,(Math.Pow(Cohort.ComputeFrad(PAR, adjHalfSat),2) * (1/(Math.Pow(spc.EstRad,2))))));
                    float adjFrad = frad;
                    // Optional adjustment to invert Pest based on relative halfSat
                    if (invertPest && halfSatRange > 0)
                    {
                        float frad_adj_int = (spc.HalfSat - minHalfSat) / halfSatRange;
                        float frad_slope = (frad_adj_int * 2) - 1;
                        adjFrad = 1 - frad_adj_int + frad * frad_slope;
                    }

                    
                    float PressureHead = hydrology.GetPressureHead(ecoregion);

                    float fwater = (float)(Math.Min(1.0,(Math.Pow(Cohort.ComputeFWater(spc.H1,spc.H2, spc.H3, spc.H4, PressureHead), 2) * (1/(Math.Pow(spc.EstMoist,2))))));

                    //float pest = 1 - (float)Math.Pow(1.0 - (frad * fwater * spc.MaxPest), Timestep);
                    float pest = (float) Math.Min(1.0,adjFrad * fwater);
                    estabDict[spc] = pest;
                    _pest[spc] = pest;
                    _fwater[spc] = fwater;
                    _frad[spc] = adjFrad;
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
