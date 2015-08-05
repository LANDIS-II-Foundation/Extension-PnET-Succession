using Landis.Core;
using Landis.SpatialModeling;
using Landis.Library.InitialCommunities;
using Landis.Library.BiomassCohortsPnET;
using System.Collections.Generic;
using Edu.Wisc.Forest.Flel.Util;
using System;
using System.Linq;


namespace Landis.Extension.Succession.BiomassPnET
{
    public class EcoregionCollection : IEnumerable<Landis.Library.BiomassCohortsPnET.PnETEcoregion> 
    {
        Dictionary<IEcoregion, Landis.Library.BiomassCohortsPnET.PnETEcoregion> AllEcoregions = new Dictionary<IEcoregion, PnETEcoregion>();
 
        Landis.Library.Parameters.Ecoregions.AuxParm<float> AET;
        Landis.Library.Parameters.Ecoregions.AuxParm<float> LeakageFrac;
        Landis.Library.Parameters.Ecoregions.AuxParm<float> PrecLossFrac;
        Landis.Library.Parameters.Ecoregions.AuxParm<float> RootingDepth;
        Landis.Library.Parameters.Ecoregions.AuxParm<string> SoilType;
        
        public Landis.Library.BiomassCohortsPnET.PnETEcoregion this[IEcoregion ecoregion]
        {
            get
            {
                return AllEcoregions[ecoregion];
            }
        }
         

        public EcoregionCollection()
        {

            AET = (Landis.Library.Parameters.Ecoregions.AuxParm<float>)PlugIn.GetParameter("AET");
            LeakageFrac = (Landis.Library.Parameters.Ecoregions.AuxParm<float>)PlugIn.GetParameter("LeakageFrac");
            PrecLossFrac = (Landis.Library.Parameters.Ecoregions.AuxParm<float>)PlugIn.GetParameter("PrecLossFrac");
            RootingDepth = (Landis.Library.Parameters.Ecoregions.AuxParm<float>)PlugIn.GetParameter("RootingDepth");
            SoilType = (Landis.Library.Parameters.Ecoregions.AuxParm<string>)PlugIn.GetParameter("SoilType");


            foreach (IEcoregion ecoregion in PlugIn.ModelCore.Ecoregions)
            {
                AllEcoregions[ecoregion] = new Landis.Library.BiomassCohortsPnET.PnETEcoregion(ecoregion, AET[ecoregion], LeakageFrac[ecoregion], PrecLossFrac[ecoregion], RootingDepth[ecoregion], SoilType[ecoregion]);
            }
        }
        public IEnumerator<Landis.Library.BiomassCohortsPnET.PnETEcoregion> GetEnumerator()
        {
            return AllEcoregions.Values.GetEnumerator();
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }
         

    }
}
