using Edu.Wisc.Forest.Flel.Util;
using Landis.Core;
using System.Collections.Generic;
using System.Reflection;
using System.Linq;
namespace Landis.Extension.Succession.BiomassPnET
{
    public class SpeciesParameters : ISpeciesParameters
    {
        private static Landis.Library.Parameters.Species.AuxParm<float> folLignin;
        private static Landis.Library.Parameters.Species.AuxParm<float> cFracBiomass;
        private static Landis.Library.Parameters.Species.AuxParm<float> sLWDel;
        private static Landis.Library.Parameters.Species.AuxParm<float> sLWmax;
        private static Landis.Library.Parameters.Species.AuxParm<float> fracFol;
        private static Landis.Library.Parameters.Species.AuxParm<float> dNSC;
        private static Landis.Library.Parameters.Species.AuxParm<float> fracBelowG;
        private static Landis.Library.Parameters.Species.AuxParm<float> tOfol;
        private static Landis.Library.Parameters.Species.AuxParm<float> tOroot;
        private static Landis.Library.Parameters.Species.AuxParm<float> tOwood;
        private static Landis.Library.Parameters.Species.AuxParm<float> bFolResp;
        private static Landis.Library.Parameters.Species.AuxParm<float> psnAgeRed;
        private static Landis.Library.Parameters.Species.AuxParm<float> kWdLit;
        private static Landis.Library.Parameters.Species.AuxParm<float> frActWd;
        private static Landis.Library.Parameters.Species.AuxParm<float> initialNSC;
        private static Landis.Library.Parameters.Species.AuxParm<bool> preventEstablishment;
        private static Landis.Library.Parameters.Species.AuxParm<float> halfSat;
        private static Landis.Library.Parameters.Species.AuxParm<float> psnTMin;
        private static Landis.Library.Parameters.Species.AuxParm<float> estRad;
        private static Landis.Library.Parameters.Species.AuxParm<float> estMoist;
        private static Landis.Library.Parameters.Species.AuxParm<float> k;
        private static Landis.Library.Parameters.Species.AuxParm<ushort> h2;
        private static Landis.Library.Parameters.Species.AuxParm<ushort> h3;
        private static Landis.Library.Parameters.Species.AuxParm<ushort> h4;
        private static Landis.Library.Parameters.Species.AuxParm<float> maintResp;
        private static Landis.Library.Parameters.Species.AuxParm<float> psnTOpt;
        private static Landis.Library.Parameters.Species.AuxParm<float> dVPD1;
        private static Landis.Library.Parameters.Species.AuxParm<float> dVPD2;
        private static Landis.Library.Parameters.Species.AuxParm<float> wUEcnst;
        private static Landis.Library.Parameters.Species.AuxParm<float> amaxA;
        private static Landis.Library.Parameters.Species.AuxParm<float> amaxB;
        private static Landis.Library.Parameters.Species.AuxParm<float> q10;
        private static Landis.Library.Parameters.Species.AuxParm<float> folN;

        public static void Initialize()
        {
            cFracBiomass = (Landis.Library.Parameters.Species.AuxParm<float>)(Parameter<float>)PlugIn.GetParameter("CFracBiomass", 0, 1);
            sLWDel = (Landis.Library.Parameters.Species.AuxParm<float>)(Parameter<float>)PlugIn.GetParameter("SLWDel", 0, float.MaxValue);
            sLWmax = (Landis.Library.Parameters.Species.AuxParm<float>)(Parameter<float>)PlugIn.GetParameter("SLWmax", 0, float.MaxValue);
            fracFol = (Landis.Library.Parameters.Species.AuxParm<float>)(Parameter<float>)PlugIn.GetParameter("FracFol", 0, 1);
            dNSC = (Landis.Library.Parameters.Species.AuxParm<float>)(Parameter<float>)PlugIn.GetParameter("DNSC", 0, 1);
            fracBelowG = (Landis.Library.Parameters.Species.AuxParm<float>)(Parameter<float>)PlugIn.GetParameter("FracBelowG", 0, 1);
            tOroot = (Landis.Library.Parameters.Species.AuxParm<float>)(Parameter<float>)PlugIn.GetParameter("TOroot", 0, 1);
            tOwood = (Landis.Library.Parameters.Species.AuxParm<float>)(Parameter<float>)PlugIn.GetParameter("TOwood", 0, 1);
            tOfol = (Landis.Library.Parameters.Species.AuxParm<float>)(Parameter<float>)PlugIn.GetParameter("TOfol", 0, 1);
            psnAgeRed = (Landis.Library.Parameters.Species.AuxParm<float>)(Parameter<float>)PlugIn.GetParameter("PsnAgeRed", 0, float.MaxValue);
            bFolResp = (Landis.Library.Parameters.Species.AuxParm<float>)(Parameter<float>)PlugIn.GetParameter("BFolResp", 0, float.MaxValue);
            kWdLit = (Landis.Library.Parameters.Species.AuxParm<float>)(Parameter<float>)PlugIn.GetParameter("KWdLit", 0, 1);
            frActWd = (Landis.Library.Parameters.Species.AuxParm<float>)(Parameter<float>)PlugIn.GetParameter("FrActWd", 0, float.MaxValue);
            initialNSC = (Landis.Library.Parameters.Species.AuxParm<float>)(Parameter<float>)PlugIn.GetParameter("InitialNSC", 0, float.MaxValue);
            preventEstablishment = (Landis.Library.Parameters.Species.AuxParm<bool>)(Parameter<bool>)PlugIn.GetParameter("PreventEstablishment");
            estRad = (Landis.Library.Parameters.Species.AuxParm<float>)(Parameter<float>)PlugIn.GetParameter("EstRad", 0, float.MaxValue);
            estMoist = (Landis.Library.Parameters.Species.AuxParm<float>)(Parameter<float>)PlugIn.GetParameter("EstMoist", 0, float.MaxValue);
            psnTMin = (Landis.Library.Parameters.Species.AuxParm<float>)(Parameter<float>)PlugIn.GetParameter("PsnTMin", -10, 10);
            halfSat = (Landis.Library.Parameters.Species.AuxParm<float>)(Parameter<float>)PlugIn.GetParameter("HalfSat", 0, float.MaxValue);
            folLignin = (Landis.Library.Parameters.Species.AuxParm<float>)(Parameter<float>)PlugIn.GetParameter("FolLignin", 0, float.MaxValue);
            k = (Landis.Library.Parameters.Species.AuxParm<float>)(Parameter<float>)PlugIn.GetParameter("K", 0, float.MaxValue);
            h2 = (Landis.Library.Parameters.Species.AuxParm<ushort>)(Parameter<ushort>)PlugIn.GetParameter("H2", 0, int.MaxValue);
            h3 = (Landis.Library.Parameters.Species.AuxParm<ushort>)(Parameter<ushort>)PlugIn.GetParameter("H3", 0, int.MaxValue);
            h4 = (Landis.Library.Parameters.Species.AuxParm<ushort>)(Parameter<ushort>)PlugIn.GetParameter("H4", 0, int.MaxValue);
            folN = (Landis.Library.Parameters.Species.AuxParm<float>)(Parameter<float>)PlugIn.GetParameter("FolN", 0, 10);

            maintResp = (Landis.Library.Parameters.Species.AuxParm<float>)(Parameter<float>)PlugIn.GetParameter("MaintResp", 0, 1);
            psnTOpt = (Landis.Library.Parameters.Species.AuxParm<float>)(Parameter<float>)PlugIn.GetParameter("PsnTOpt", 0, 30);
            dVPD1 = (Landis.Library.Parameters.Species.AuxParm<float>)(Parameter<float>)PlugIn.GetParameter("DVPD1", 0, 10);
            dVPD2 = (Landis.Library.Parameters.Species.AuxParm<float>)(Parameter<float>)PlugIn.GetParameter("DVPD2", 0, 10);
            wUEcnst = (Landis.Library.Parameters.Species.AuxParm<float>)(Parameter<float>)PlugIn.GetParameter("WUEcnst", 0, 200);
            amaxA = (Landis.Library.Parameters.Species.AuxParm<float>)(Parameter<float>)PlugIn.GetParameter("AmaxA", -200, 200);
            amaxB = (Landis.Library.Parameters.Species.AuxParm<float>)(Parameter<float>)PlugIn.GetParameter("AmaxB", 0, float.MaxValue);
            q10 = (Landis.Library.Parameters.Species.AuxParm<float>)(Parameter<float>)PlugIn.GetParameter("Q10", 0, 10);
        }

        private float _q10;
        private float _wuecnst;
        private float _psntmin;
        private float _fracfol;
        private float _maintresp;
        private float _foln;
        private float _dvpd1;
        private float _dvpd2;
        private float _bfolresp;
        private float _amaxb;
        private float _amaxa;
        private float _psntopt;
        private float _toroot;
        private float _towood;
        private float _tofol;
        private float _initialnsc;
        private float _dnsc;
        private float _fractwd;
        private float _slwmax;
        private float _slwdel;
        private float _kwdlit;
        private float _cfracbiomass;
        private float _psnagered;
        private ushort _h3;
        private ushort _h4;
        private ushort _h2;
        private float _k;
        private float _halfsat;
        private bool _preventestablishment;
        private float _estrad;
        private float _estmoist;
        private float _fracbelowg;
        private float _follignin;

        public float FolLignin
        {
            get {
                return _follignin;
            }
        }
        public float FracBelowG
        {
            get {
                return _fracbelowg;
            }
        }
        public float EstMoist
        {
            get {
                return _estmoist;
            }
        }
        public float EstRad
        {
            get {
                return _estrad;
            }
        }
        public float HalfSat
        {
            get {
                return _halfsat;
            }
        }
        public float K
        {
            get {
                return _k;
            }
        }
        public ushort H2
        {
            get {
                return _h2;
            }
        }
        public ushort H4
        {
            get {
                return _h4;
            }
        }
        public ushort H3
        {
            get {
                return _h3;
            }
        }
        public float PsnAgeRed
        {
            get {
                return _psnagered;
            }
        }
        public float CFracBiomass
        {
            get {
                return _cfracbiomass;
            }
        }
        public float KWdLit
        {
            get {
                return _kwdlit;
            }
        }
        public float DNSC
        {
            get {
                return _dnsc;
            }
        }

        public float SLWmax
        {
            get {
                return _slwmax;
            }
        }
        public float SLWDel
        {
            get{
                return _slwdel;
            }
        } 
        public float FrActWd
        {
            get
            {
                return _fractwd;
            }
        }
        public float InitialNSC
        {
            get
            {
                return _initialnsc;
            }
        }
         
        public float TOroot
        {
            get
            {
                return _toroot;
            }
        }
        public float TOwood
        {
            get
            {
                return _towood;
            }
        }
        public float TOfol
        {
            get
            {
                return _tofol;
            }
        }
        public float AmaxB
        {
            get
            {
                return _amaxb;
            }
        }
        public float AmaxA
        {
            get
            {
                return _amaxa;
            }
        }

        public float BFolResp
        {
            get
            {
                return _bfolresp;
            }
        }
        public float DVPD2
        {
            get
            {
                return _dvpd2;
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
        public float MaintResp
        {
            get
            {
                return _maintresp;
            }
        }

        public float FracFol
        {
            get
            {
                return _fracfol;
            }
        }
        public float PsnTMin
        {
            get
            {
                return _psntmin;
            }
        }
        public bool PreventEstablishment
        {
            get {
                return _preventestablishment;
            }
        }
        public float Q10
        {
            get
            {
                return _q10;
            }
        }
        public float WUEcnst
        {
            get
            {
                return _wuecnst;
            }
        }
        public float PsnTOpt
        {
            get
            {
                return _psntopt;
            }
        }
        public static List<string> ParameterNames
        {
            get
            {
                System.Type type = typeof(SpeciesParameters); // Get type pointer
                List<string> names = type.GetProperties().Select(x => x.Name).ToList(); // Obtain all fields


                return names;
            }
        }

        public SpeciesParameters(ISpecies species)
        {
            this._psntopt = psnTOpt[species];
            this._follignin = folLignin[species];
            this._cfracbiomass = cFracBiomass[species];
            this._slwdel = sLWDel[species];
            this._slwmax  = sLWmax[species];
            this._fracfol = fracFol[species];
            this._dnsc  =dNSC[species]; 
            this._fracbelowg =  fracBelowG[species];
            this._tofol = tOfol[species];
            this._toroot = tOroot[species];
            this._towood = tOwood[species];
            this._bfolresp = bFolResp[species];
            this._psnagered = psnAgeRed[species];
            this._kwdlit = kWdLit[species];
            this._fractwd = frActWd[species];
            this._initialnsc = initialNSC[species];
            this._preventestablishment = preventEstablishment[species];
            this._halfsat = halfSat[species];
            this._psntmin = psnTMin[species];
            this._estrad = estRad[species];
            this._estmoist = estMoist[species];
            this._k =  k[species];
            this._h2 = h2[species];
            this._h3 = h3[species];
            this._h4 = h4[species];
            this._maintresp = maintResp[species];
            this._psntopt = psnTOpt[species];
            this._dvpd1 = dVPD1[species];
            this._dvpd2 = dVPD2[species];
            this._wuecnst = wUEcnst[species];
            this._amaxa = amaxA[species];
            this._amaxb = amaxB[species];
            this._q10 = q10[species];
            this._foln = folN[species];
        }
        public SpeciesParameters(ISpeciesParameters parameters)
        {
            this._psntopt = parameters.PsnTOpt;
            this._follignin = parameters.FolLignin;
            this._cfracbiomass = parameters.CFracBiomass;
            this._slwdel = parameters.SLWDel;
            this._slwmax = parameters.SLWmax;
            this._fracfol = parameters.FracFol;
            this._dnsc = parameters.DNSC;
            this._fracbelowg = parameters.FracBelowG;
            this._tofol = parameters.TOfol;
            this._toroot = parameters.TOroot;
            this._towood = parameters.TOwood;
            this._bfolresp = parameters.BFolResp;
            this._psnagered = parameters.PsnAgeRed;
            this._kwdlit = parameters.KWdLit;
            this._fractwd = parameters.FrActWd;
            this._initialnsc = parameters.InitialNSC;
            this._preventestablishment = parameters.PreventEstablishment;
            this._halfsat = parameters.HalfSat;
            this._psntmin = parameters.PsnTMin;
            this._estrad = parameters.EstRad;
            this._estmoist = parameters.EstMoist;
            this._k = parameters.K;
            this._h2 = parameters.H2;
            this._h3 = parameters.H3;
            this._h4 = parameters.H4;
            this._maintresp = parameters.MaintResp;
            this._psntopt = parameters.PsnTOpt;
            this._dvpd1 = parameters.DVPD1;
            this._dvpd2 = parameters.DVPD2;
            this._wuecnst = parameters.WUEcnst;
            this._amaxa = parameters.AmaxA;
            this._amaxb = parameters.AmaxB;
            this._q10 = parameters.Q10;
            this._foln = parameters.FolN;
        }
    }

}

/*
namespace Landis.Extension.Succession.BiomassPnET 
{
    /// <summary>
    /// The parameters for a single tree species.
    /// </summary>
    public class Parameters
        : ISpeciesParameters
    {
        
        private string name;
        private int longevity;
        private int maturity;
        private byte shadeTolerance;
        private byte fireTolerance;
        private int effectiveSeedDist;
        private int maxSeedDist;
        private float vegReprodProb;
        private int minSproutAge;
        private int maxSproutAge;
        private PostFireRegeneration postFireRegen;

        //---------------------------------------------------------------------

        public string Name
        {
            get {
                return name;
            }
        }

        //---------------------------------------------------------------------

        public int Longevity
        {
            get {
                return longevity;
            }
        }

        //---------------------------------------------------------------------

        public int Maturity
        {
            get {
                return maturity;
            }
        }

        //---------------------------------------------------------------------

        public byte ShadeTolerance
        {
            get {
                return shadeTolerance;
            }
        }

        //---------------------------------------------------------------------

        public byte FireTolerance
        {
            get {
                return fireTolerance;
            }
        }

        //---------------------------------------------------------------------

        public int EffectiveSeedDist
        {
            get {
                return effectiveSeedDist;
            }
        }

        //---------------------------------------------------------------------

        public int MaxSeedDist
        {
            get {
                return maxSeedDist;
            }
        }

        //---------------------------------------------------------------------

        public float VegReprodProb
        {
            get {
                return vegReprodProb;
            }
        }

        //---------------------------------------------------------------------

        public int MinSproutAge
        {
            get {
                return minSproutAge;
            }
        }

        //---------------------------------------------------------------------

        public int MaxSproutAge
        {
            get {
                return maxSproutAge;
            }
        }

        //---------------------------------------------------------------------

        public PostFireRegeneration PostFireRegeneration
        {
            get {
                return postFireRegen;
            }
        }

        //---------------------------------------------------------------------

        public Parameters(string name,
                          int longevity,
                          int maturity,
                          byte shadeTolerance,
                          byte fireTolerance,
                          int effectiveSeedDist,
                          int maxSeedDist,
                          float vegReprodProb,
                          int minSproutAge,
                          int maxSproutAge,
                          PostFireRegeneration postFireRegen)
        {
            this.name              = name;
            this.longevity         = longevity;
            this.maturity          = maturity;
            this.shadeTolerance    = shadeTolerance;
            this.fireTolerance     = fireTolerance;
            this.effectiveSeedDist = effectiveSeedDist;
            this.maxSeedDist       = maxSeedDist;
            this.vegReprodProb     = vegReprodProb;
            this.minSproutAge      = minSproutAge;
            this.maxSproutAge      = maxSproutAge;
            this.postFireRegen     = postFireRegen;
        }

        //---------------------------------------------------------------------

        public Parameters(ISpeciesParameters parameters)
        {
            name              = parameters.Name;
            longevity         = parameters.Longevity;
            maturity          = parameters.Maturity;
            shadeTolerance    = parameters.ShadeTolerance;
            fireTolerance     = parameters.FireTolerance;
            effectiveSeedDist = parameters.EffectiveSeedDist;
            maxSeedDist       = parameters.MaxSeedDist;
            vegReprodProb     = parameters.VegReprodProb;
            minSproutAge      = parameters.MinSproutAge;
            maxSproutAge      = parameters.MaxSproutAge;
            postFireRegen     = parameters.PostFireRegeneration;
        } 
    }
        
}
*/