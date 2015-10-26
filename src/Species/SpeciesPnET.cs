using System.Collections.Generic;
using System.Linq;
using Landis.Core;

namespace Landis.Extension.Succession.BiomassPnET
{
    /// <summary>
    /// The information for a tree species (its index and parameters).
    /// </summary>
    public class SpeciesPnET : ISpeciesPNET
    {
        public static Dictionary<ISpecies, ISpeciesPNET> AllSpecies;

        private ISpecies _species;
        private float _cfracbiomass;
        private float _kwdlit;
        private float _dnsc;
        private float _fracbelowg;
        private float _fracfol;
        private float _fractWd;
        private float _psnagered;
        private ushort _h2;
        private ushort _h3;
        private ushort _h4;
        private float _slwdel;
        private float _slwmax;
        private float _tofol;
        private float _toroot;
        private float _halfsat;
        private float _initialnsc;
        private float _k;
        private float _towood;
        private float _estrad;
        private float _estmoist;
        private float _follignin;
        private bool _preventestablishment;
        private float _psntopt;
        private float _q10;
        private float _psntmin;
        private float _dvpd1;
        private float _foln;
        private float _dvpd2;
        private float _amaxa;
        private float _amaxb;
        private float _wuecnst;
        private float _maintresp;
        private float _bfolresp;

        private static Landis.Library.Parameters.Species.AuxParm<float> dnsc;
        private static Landis.Library.Parameters.Species.AuxParm<float> cfracbiomass;
        private static Landis.Library.Parameters.Species.AuxParm<float> kwdlit;
        private static Landis.Library.Parameters.Species.AuxParm<float> fracbelowg;
        private static Landis.Library.Parameters.Species.AuxParm<float> fracfol;
        private static Landis.Library.Parameters.Species.AuxParm<float> fractWd;
        private static Landis.Library.Parameters.Species.AuxParm<float> psnagered;
        private static Landis.Library.Parameters.Species.AuxParm<ushort> h2;
        private static Landis.Library.Parameters.Species.AuxParm<ushort> h3;
        private static Landis.Library.Parameters.Species.AuxParm<ushort> h4;
        private static Landis.Library.Parameters.Species.AuxParm<float> slwdel;
        private static Landis.Library.Parameters.Species.AuxParm<float> slwmax;    
  
        private static Landis.Library.Parameters.Species.AuxParm<float> tofol;
        private static Landis.Library.Parameters.Species.AuxParm<float> halfsat;
        private static Landis.Library.Parameters.Species.AuxParm<float> toroot;
        private static Landis.Library.Parameters.Species.AuxParm<float> initialnsc;
        private static Landis.Library.Parameters.Species.AuxParm<float> k;
        
        private static Landis.Library.Parameters.Species.AuxParm<float> towood;
        private static Landis.Library.Parameters.Species.AuxParm<float> estrad;
        private static Landis.Library.Parameters.Species.AuxParm<float> estmoist;
        private static Landis.Library.Parameters.Species.AuxParm<float> follignin;
        private static Landis.Library.Parameters.Species.AuxParm<bool> preventestablishment;
        private static Landis.Library.Parameters.Species.AuxParm<float> psntopt;


        private static Landis.Library.Parameters.Species.AuxParm<float> q10;
        private static Landis.Library.Parameters.Species.AuxParm<float> psntmin;
        private static Landis.Library.Parameters.Species.AuxParm<float> dvpd1;
        private static Landis.Library.Parameters.Species.AuxParm<float> dvpd2;
        private static Landis.Library.Parameters.Species.AuxParm<float> foln;
        private static Landis.Library.Parameters.Species.AuxParm<float> amaxa;
        private static Landis.Library.Parameters.Species.AuxParm<float> amaxb;
        private static Landis.Library.Parameters.Species.AuxParm<float> wuecnst;
        private static Landis.Library.Parameters.Species.AuxParm<float> maintresp;
        private static Landis.Library.Parameters.Species.AuxParm<float> bfolresp;
         
        public static void Initialize()
        {
            AllSpecies = new Dictionary<ISpecies, ISpeciesPNET>();

            dnsc =  ((Landis.Library.Parameters.Species.AuxParm<float>)(Parameter<float>)PlugIn.GetParameter("DNSC"));
            cfracbiomass=  ((Landis.Library.Parameters.Species.AuxParm<float>)(Parameter<float>)PlugIn.GetParameter("CFracBiomass"));
            kwdlit = ((Landis.Library.Parameters.Species.AuxParm<float>)(Parameter<float>)PlugIn.GetParameter("kwdlit"));
            fracbelowg = ((Landis.Library.Parameters.Species.AuxParm<float>)(Parameter<float>)PlugIn.GetParameter("fracbelowg"));
            fracfol = ((Landis.Library.Parameters.Species.AuxParm<float>)(Parameter<float>)PlugIn.GetParameter("fracfol"));
            fractWd = ((Landis.Library.Parameters.Species.AuxParm<float>)(Parameter<float>)PlugIn.GetParameter("fractWd"));
            psnagered = ((Landis.Library.Parameters.Species.AuxParm<float>)(Parameter<float>)PlugIn.GetParameter("psnagered"));
            h2 = ((Landis.Library.Parameters.Species.AuxParm<ushort>)(Parameter<ushort>)PlugIn.GetParameter("h2"));
            h3 = ((Landis.Library.Parameters.Species.AuxParm<ushort>)(Parameter<ushort>)PlugIn.GetParameter("h3"));
            h4 = ((Landis.Library.Parameters.Species.AuxParm<ushort>)(Parameter<ushort>)PlugIn.GetParameter("h4"));
            slwdel = ((Landis.Library.Parameters.Species.AuxParm<float>)(Parameter<float>)PlugIn.GetParameter("slwdel"));
            slwmax = ((Landis.Library.Parameters.Species.AuxParm<float>)(Parameter<float>)PlugIn.GetParameter("slwmax"));
            tofol = ((Landis.Library.Parameters.Species.AuxParm<float>)(Parameter<float>)PlugIn.GetParameter("tofol"));
            halfsat = ((Landis.Library.Parameters.Species.AuxParm<float>)(Parameter<float>)PlugIn.GetParameter("halfsat"));
            toroot = ((Landis.Library.Parameters.Species.AuxParm<float>)(Parameter<float>)PlugIn.GetParameter("toroot"));
            initialnsc = ((Landis.Library.Parameters.Species.AuxParm<float>)(Parameter<float>)PlugIn.GetParameter("initialnsc")); ;
            k = ((Landis.Library.Parameters.Species.AuxParm<float>)(Parameter<float>)PlugIn.GetParameter("k")); ;
            towood = ((Landis.Library.Parameters.Species.AuxParm<float>)(Parameter<float>)PlugIn.GetParameter("towood")); ;
            estrad = ((Landis.Library.Parameters.Species.AuxParm<float>)(Parameter<float>)PlugIn.GetParameter("estrad")); ;
            estmoist = ((Landis.Library.Parameters.Species.AuxParm<float>)(Parameter<float>)PlugIn.GetParameter("estmoist"));
            follignin = ((Landis.Library.Parameters.Species.AuxParm<float>)(Parameter<float>)PlugIn.GetParameter("follignin"));
            preventestablishment = ((Landis.Library.Parameters.Species.AuxParm<bool>)(Parameter<bool>)PlugIn.GetParameter("preventestablishment"));
            psntopt = ((Landis.Library.Parameters.Species.AuxParm<float>)(Parameter<float>)PlugIn.GetParameter("psntopt"));
            q10 = ((Landis.Library.Parameters.Species.AuxParm<float>)(Parameter<float>)PlugIn.GetParameter("q10"));
            psntmin = ((Landis.Library.Parameters.Species.AuxParm<float>)(Parameter<float>)PlugIn.GetParameter("psntmin"));
            dvpd1 = ((Landis.Library.Parameters.Species.AuxParm<float>)(Parameter<float>)PlugIn.GetParameter("dvpd1"));
            dvpd2 = ((Landis.Library.Parameters.Species.AuxParm<float>)(Parameter<float>)PlugIn.GetParameter("dvpd2"));
            foln = ((Landis.Library.Parameters.Species.AuxParm<float>)(Parameter<float>)PlugIn.GetParameter("foln"));
            amaxa = ((Landis.Library.Parameters.Species.AuxParm<float>)(Parameter<float>)PlugIn.GetParameter("amaxa"));
            amaxb = ((Landis.Library.Parameters.Species.AuxParm<float>)(Parameter<float>)PlugIn.GetParameter("amaxb"));
            wuecnst = ((Landis.Library.Parameters.Species.AuxParm<float>)(Parameter<float>)PlugIn.GetParameter("wuecnst"));
            maintresp = ((Landis.Library.Parameters.Species.AuxParm<float>)(Parameter<float>)PlugIn.GetParameter("maintresp"));
            bfolresp = ((Landis.Library.Parameters.Species.AuxParm<float>)(Parameter<float>)PlugIn.GetParameter("bfolresp"));
                     

            foreach (ISpecies spc in PlugIn.ModelCore.Species)
            {
                AllSpecies.Add(spc, new SpeciesPnET(spc));
            }


        }
       
        public SpeciesPnET(ISpecies species)
        {

            _dnsc = dnsc[species];
            _cfracbiomass = cfracbiomass[species];
            _kwdlit = kwdlit[species];
            _fracbelowg = fracbelowg[species];
            _fracfol = fracfol[species];
            _fractWd = fractWd[species];
            _psnagered = psnagered[species];
            _h2 = h2[species];
            _h3 = h3[species];
            _h4 = h4[species];
            _slwdel = slwdel[species];
            _slwmax = slwmax[species];
            _tofol = tofol[species];
            _toroot = toroot[species];
            _halfsat = halfsat[species];
            _initialnsc = initialnsc[species];
            _k = k[species];
            _towood = towood[species];
            _estrad = estrad[species];
            _estmoist = estmoist[species];
            _follignin = follignin[species];
            _preventestablishment = preventestablishment[species];

            _psntopt = psntopt[species];
            _q10 = q10[species]; 
            _psntmin = psntmin[species];
            _dvpd1 = dvpd1[species];
            _foln = foln[species];
            _dvpd2 = dvpd2[species];
            _amaxa = amaxa[species];
            _amaxb = amaxb[species];
            _wuecnst = wuecnst[species];
            _maintresp = maintresp[species];
            _bfolresp = bfolresp[species];
            _species = species;
            
        
        }
        public int Index
        {
            get
            {
                return _species.Index;
            }
        }
        public float BFolResp
        {
            get
            {
                return _bfolresp;
            }
        }
        public float AmaxA
        {
            get
            {
                return _amaxa;
            }
        }
        public float AmaxB
        {
            get
            {
                return _amaxb;
            }
        }
        public float WUEcnst
        {
            get
            {
                return _wuecnst;
            }
        }
        public float MaintResp
        {
            get
            {
                return _maintresp;
            }
        }

        public float PsnTMin
        {
            get
            {
                return _psntmin;
            }
        }
        public float DVPD1
        {
            get
            {
                return _dvpd1;
            }
        }
        public float FolN
        {
            get
            {
                return _foln;
            }
        }
        public float DVPD2
        {
            get
            {
                return _dvpd2;
            }

        }
        public float PsnTOpt
        {
            get
            {
                return _psntopt;
            }
        }
        public float Q10
        {
            get
            {
                return _q10;
            }
        }
        public float EstRad
        {
            get
            {
                return _estrad;
            }
        }
        public bool PreventEstablishment
        {
            get { return _preventestablishment; }
        }
        public float FolLignin
        {
            get { return _follignin; }
        }
        public float EstMoist
        {
            get { return _estmoist; }
        }
        public float TOwood
        {
            get
            {
                return _towood;
            }
        }
        public float K
        {
            get
            {
                return _k;
            }
        }
        public float InitialNSC
        {
            get
            {
                return _initialnsc;
            }
        }
        public float HalfSat
        {
            get
            {
                return _halfsat;
            }
        }
        public float TOroot
        {
            get
            {
                return _toroot;
            }
        }
        public float TOfol
        {
            get
            {
                return _tofol;
            }
        }
        public float SLWDel
        {
            get
            {
                return _slwdel;
            }
        }
        public float SLWmax
        {
            get
            {
                return _slwmax;
            }
        }
        public ushort H4
        {
            get
            {
                return _h4;
            }
        }
        public ushort H3
        {
            get
            {
                return _h3;
            }
        }
        public ushort H2
        {
            get
            {
                return _h2;
            }
        }
        public float PsnAgeRed
        {
            get
            {
                return _psnagered;
            }
        }
        public float KWdLit
        {
            get
            {
                return _kwdlit;
            }
        }
        public float FrActWd
        {
            get
            {
                return _fractWd;
            }
        }
        public float FracFol
        {
            get
            {
                return _fracfol;
            }
        }
        public float FracBelowG
        {
            get
            {
                return _fracbelowg;
            }
        }
        public float DNSC
        {
            get
            {
                return _dnsc;
            }
        }

        public float CFracBiomass
        {
            get
            {
                return _cfracbiomass;
            }
        }

        
       
       

        


        
        public static List<string> ParameterNames
        {
            get
            {
                System.Type type = typeof(SpeciesPnET); // Get type pointer
                List<string> names = type.GetProperties().Select(x => x.Name).ToList(); // Obtain all fields


                return names;
            }
        }


        public string Name
        {
            get
            {
                return _species.Name;
            }
        }


        public int MaxSproutAge
        {
            get
            {
                return _species.MaxSproutAge;
            }
        }
        public int MinSproutAge
        {
            get
            {
                return _species.MinSproutAge;
            }
        }
        public Landis.Core.PostFireRegeneration PostFireRegeneration
        {
            get
            {
                return _species.PostFireRegeneration;
            }
        }
        public int MaxSeedDist
        {
            get
            {
                return _species.MaxSeedDist;
            }
        }
        public int EffectiveSeedDist
        {
            get
            {
                return _species.EffectiveSeedDist;
            }
        }
        public float VegReprodProb
        {
            get
            {
                return _species.VegReprodProb;
            }
        }
        public byte FireTolerance
        {
            get
            {
                return _species.FireTolerance;
            }
        }
        public byte ShadeTolerance
        {
            get
            {
                return _species.ShadeTolerance;
            }
        }
        public int Maturity
        {
            get
            {
                return _species.Maturity;
            }
        }
        public int Longevity
        {
            get
            {
                return _species.Longevity;
            }
        }



    }
}
