using Landis.Core;
using Landis.SpatialModeling;
using Landis.Library.Succession;
using Landis.Library.InitialCommunities;
using System.Collections.Generic;
using Edu.Wisc.Forest.Flel.Util;
using System;
using System.Linq;
using Landis.Library.Parameters.Species;


namespace Landis.Extension.Succession.BiomassPnET
{
    public class Allocation
    {
        public struct Disturbances
        {
            public const string DisturbanceFire = "disturbance:fire";
            public const string DisturbanceWind = "disturbance:wind";
            public const string DisturbanceBDA = "disturbance:bda";
            public const string DisturbanceHarvest = "disturbance:harvest";
            public static List<string> AllNames
            {
                get
                {
                    List<string> Names = new List<string>();

                    typeof(Disturbances).GetFields().ToList().ForEach(x => Names.Add(x.GetValue(x).ToString()));

                    return Names;
                }
            }
        } 

        public struct Reductions
        {
            public const string WoodReduction = "WoodReduction";
            public const string FolReduction = "FolReduction";
            public const string RootReduction = "RootReduction";

            public static List<string> AllNames
            {
                get
                {
                    List<string> Names = new List<string>();

                    typeof(Reductions).GetFields().ToList().ForEach(x => Names.Add(x.GetValue(x).ToString()));

                    return Names;
                }
            }
        }
       
        public static void Allocate(object sitecohorts, Cohort cohort, ExtensionType disturbanceType)
        {
            if (sitecohorts == null) return;// Deaths in spinup are not added yet

            float pwoodlost = 0;
            float prootlost = 0;
            float pfollost = 0;

            Parameter<string> p;

            if (disturbanceType != null && PlugIn.TryGetParameter(disturbanceType.Name, out p))
            {
                if (p.ContainsKey(Reductions.WoodReduction))
                {
                    pwoodlost = float.Parse(p[Reductions.WoodReduction]);
                }
                if (p.ContainsKey(Reductions.RootReduction))
                {
                    prootlost = float.Parse(p[Reductions.RootReduction]);
                }
                if (p.ContainsKey(Reductions.FolReduction))
                {
                    pfollost = float.Parse(p[Reductions.FolReduction]);
                }

            }

            ((SiteCohorts)sitecohorts).AddWoodyDebris((float)((1 - pwoodlost) * cohort.Wood), cohort.Species.KWdLit());
            ((SiteCohorts)sitecohorts).AddWoodyDebris((float)((1 - prootlost) * cohort.Root), cohort.Species.KWdLit());
            ((SiteCohorts)sitecohorts).AddLitter((float)((1 - pfollost) * cohort.Fol), cohort.Species);


        }
    }
}
