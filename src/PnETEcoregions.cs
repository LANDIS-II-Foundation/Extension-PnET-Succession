using Landis.Core;
using Landis.SpatialModeling;
using Landis.Library.InitialCommunities;
//using Landis.Library.BiomassCohortsPnET;
using System.Collections.Generic;
using Landis.Utilities;
using System;
using System.Linq;

namespace Landis.Extension.Succession.BiomassPnET
{
    public class PnETEcoregion : IEcoregion
    {
        IEcoregion ecoregion;

        public string Description
        {
            get
            {
                return ecoregion.Description;
            }
        }
        public ushort MapCode
        {
            get
            {
                return ecoregion.MapCode;
            }
        }
        public bool Active
        {
            get
            {
                return ecoregion.Active;
            }
        }
        public IEcoregion IEcoregion
        {
            get
            {
                return ecoregion;
            }
        }

        public string Name
        {
            get
            {
                return ecoregion.Name;
            }
        }
        public int Index
        {
            get
            {
                return ecoregion.Index;
            }
        }
    
    }

    public class PnETEcoregions  
    {
        static ICore ModelCore;
        ISiteVar<IEcoregion> ecoregions = ModelCore.Landscape.NewSiteVar<IEcoregion>();

        IEcoregion ecoregion;

        public readonly float AET;
        public readonly float LeakageFrac;
        public readonly float RunoffCapture;
        public readonly float PrecLossFrac;
        public readonly float RootingDepth;
        public readonly string SoilType;
        public readonly string ClimateFileName;

        public IEcoregion this[ActiveSite site]
        {
            get
            {
                return ecoregions[site];
            }
        }

        
        
        
        
        public static List<string> ParameterNames
        {
            get
            {
                return typeof(PnETEcoregion).GetFields().Select(x => x.Name).ToList();
            }
        }


        public static void Initialize( ICore mCore)
        {
            ModelCore = mCore;
        }

        public PnETEcoregions(IEcoregion ecoregion, float AET, float LeakageFrac, float RunoffCapture, float PrecLossFrac, float RootingDepth, string SoilType, string ClimateFileName)
        {
            this.ecoregion = ecoregion;
            this.AET = AET;
            this.LeakageFrac = LeakageFrac;
            this.RunoffCapture = RunoffCapture;
            this.PrecLossFrac=PrecLossFrac;
            this.RootingDepth=RootingDepth;
            this.SoilType=SoilType;
            this.ClimateFileName = ClimateFileName;

        }
    }
}
