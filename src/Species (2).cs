using Landis.Core;
using Landis.SpatialModeling;
using Landis.Library.InitialCommunities;
using System.Collections.Generic;
using Edu.Wisc.Forest.Flel.Util;
using System;
using System.Linq;
using System.Reflection;

namespace Landis.Extension.Succession.BiomassPnET
{
    public class Species : Landis.Core.ISpecies, IEnumerable<Species>
    {
        public IEnumerable<Species> GetEnumerator()
        { 
        
        }


        float _folLignin;
        public float FolLignin
        {
            get {
                return _folLignin;
            }
        }
        float _cFracBiomass;
        public float CFracBiomass
        {
            get {
                return _cFracBiomass;
            }
        }

        float _sLWDel;
        public float SLWDel
        {
            get {
                return _sLWDel;
            }
        }

        float _sLWmax;
        public float SLWmax
        {
            get {
                return _sLWmax;
            }
        }

        float _fracFol;
        public float FracFol
        {
            get {
                return _fracFol;
            }
        }
        float _dNSC;
        public float DNSC
        {
            get
            {
                return _dNSC;
            }
        }
        float _fracBelowG;
        public float FracBelowG
        {
            get
            {
                return _fracBelowG;
            }
        }
        float _tOfol;
        public float TOfol
        {
            get
            {
                return _tOfol;
            }
        }
        float _tOroot;
        public float TOroot
        {
            get
            {
                return _tOroot;
            }
        }
        float _tOwood;
        public float TOwood
        {
            get
            {
                return _tOwood;
            }
        }
        float _bFolResp;
        public float BFolResp
        {
            get
            {
                return _bFolResp;
            }
        }
        float _psnAgeRed;
        public float PsnAgeRed
        {
            get
            {
                return _psnAgeRed;
            }
        }
        float _kWdLit;
        public float KWdLit
        {
            get
            {
                return _kWdLit;
            }
        }
        float _frActWd;
        public float FrActWd
        {
            get
            {
                return _frActWd;
            }
        }
        float _initialNSC;
        public float InitialNSC
        {
            get
            {
                return _initialNSC;
            }
        }
        bool _preventEstablishment;
        public bool PreventEstablishment
        {
            get
            {
                return _preventEstablishment;
            }
        }
        float _halfSat;
        public float HalfSat
        {
            get
            {
                return _halfSat;
            }
        }
        float _psnTMin;
        public float PsnTMin
        {
            get
            {
                return _psnTMin;
            }
        }
        float _estRad;
        public float EstRad
        {
            get
            {
                return _estRad;
            }
        }
        float _estMoist;
        public float EstMoist
        {
            get
            {
                return _estMoist;
            }
        }
        float _k;
        public float K
        {
            get
            {
                return _k;
            }
        }
        float _h2;
        public float H2
        {
            get
            {
                return _h2;
            }
        }
        float _h3;
        public float H3
        {
            get
            {
                return _h3;
            }
        }
        ushort _h4;
        public ushort H4
        {
            get
            {
                return _h4;
            }
        }
        float _maintResp;
        public float MaintResp
        {
            get
            {
                return _maintResp;
            }
        }
        float _psnTOpt;
        public float PsnTOpt
        {
            get
            {
                return _psnTOpt;
            }
        }
        float _dVPD1;
        public float DVPD1
        {
            get
            {
                return _dVPD1;
            }
        }
        float _dVPD2;
        public float DVPD2
        {
            get
            {
                return _dVPD2;
            }
        }
        float _wUEcnst;
        public float WUEcnst
        {
            get
            {
                return _wUEcnst;
            }
        }
        float _amaxA;
        public float AmaxA
        {
            get
            {
                return _amaxA;
            }
        }
        float _amaxB;
        public float AmaxB
        {
            get
            {
                return _amaxB;
            }
        }
        float _q10;
        public float Q10
        {
            get
            {
                return _q10;
            }
        }
        float _folN;
        public float FolN
        {
            get
            {
                return _folN;
            }
        }
        int _effectiveSeedDist;
        public int EffectiveSeedDist
        {
            get
            {
                return _effectiveSeedDist;
            }
        }
        int _index;
        public int Index
        {
            get
            {
                return _index;
            }
        }
       
        byte _fireTolerance;
        public byte FireTolerance
        {
            get
            {
                return _fireTolerance;
            }
        }
        int _longevity;
        public int Longevity
        {
            get
            {
                return _longevity;
            }
        }
        int _maturity;
        public int Maturity
        {
            get
            {
                return _maturity;
            }
        }
        int _maxSeedDist;
        public int MaxSeedDist
        {
            get
            {
                return _maxSeedDist;
            }
        }
        int _maxSproutAge;
        public int MaxSproutAge
        {
            get
            {
                return _maxSproutAge;
            }
        }
        int _minSproutAge;
        public int MinSproutAge
        {
            get
            {
                return _minSproutAge;
            }
        }
        string _name;
        public string Name
        {
            get
            {
                return _name;
            }
        }
        PostFireRegeneration _postFireRegeneration;
        public PostFireRegeneration PostFireRegeneration
        {
            get
            {
                return _postFireRegeneration;
            }
        }
        byte _shadeTolerance;
        public byte ShadeTolerance
        {
            get
            {
                return _shadeTolerance;
            }
        }
        float _vegReprodProb;
        public float VegReprodProb
        {
            get
            {
                return _vegReprodProb;
            }
        }
        
        public static List<string> ParameterNames
        {
            get
            {
                return typeof(Species).GetFields(BindingFlags.Public).Select(x => x.Name).ToList();
            }
        }
        

        public Species(ISpecies species)
        {
            _cFracBiomass = ((Landis.Library.Parameters.Species.AuxParm<float>)(Parameter<float>)PlugIn.GetParameter("CFracBiomass", 0, 1))[species]; 
            _sLWDel = ((Landis.Library.Parameters.Species.AuxParm<float>)(Parameter<float>)PlugIn.GetParameter("SLWDel", 0, float.MaxValue))[species];
            _sLWmax = ((Landis.Library.Parameters.Species.AuxParm<float>)(Parameter<float>)PlugIn.GetParameter("SLWmax", 0, float.MaxValue))[species];
            _fracFol = ((Landis.Library.Parameters.Species.AuxParm<float>)(Parameter<float>)PlugIn.GetParameter("FracFol", 0, 1))[species];
            _dNSC = ((Landis.Library.Parameters.Species.AuxParm<float>)(Parameter<float>)PlugIn.GetParameter("DNSC", 0, 1))[species];
            _fracBelowG = ((Landis.Library.Parameters.Species.AuxParm<float>)(Parameter<float>)PlugIn.GetParameter("FracBelowG", 0, 1))[species];
            _tOroot = ((Landis.Library.Parameters.Species.AuxParm<float>)(Parameter<float>)PlugIn.GetParameter("TOroot", 0, 1))[species];
            _tOwood = ((Landis.Library.Parameters.Species.AuxParm<float>)(Parameter<float>)PlugIn.GetParameter("TOwood", 0, 1))[species];
            _tOfol = ((Landis.Library.Parameters.Species.AuxParm<float>)(Parameter<float>)PlugIn.GetParameter("TOfol", 0, 1))[species];
            _psnAgeRed = ((Landis.Library.Parameters.Species.AuxParm<float>)(Parameter<float>)PlugIn.GetParameter("PsnAgeRed", 0, float.MaxValue))[species];
            _bFolResp = ((Landis.Library.Parameters.Species.AuxParm<float>)(Parameter<float>)PlugIn.GetParameter("BFolResp", 0, float.MaxValue))[species];
            _kWdLit = ((Landis.Library.Parameters.Species.AuxParm<float>)(Parameter<float>)PlugIn.GetParameter("KWdLit", 0, 1))[species];
            _frActWd = ((Landis.Library.Parameters.Species.AuxParm<float>)(Parameter<float>)PlugIn.GetParameter("FrActWd", 0, float.MaxValue))[species];
            _initialNSC = ((Landis.Library.Parameters.Species.AuxParm<float>)(Parameter<float>)PlugIn.GetParameter("InitialNSC", 0, float.MaxValue))[species];
            _preventEstablishment = ((Landis.Library.Parameters.Species.AuxParm<bool>)(Parameter<bool>)PlugIn.GetParameter("PreventEstablishment"))[species];
            _estRad = ((Landis.Library.Parameters.Species.AuxParm<float>)(Parameter<float>)PlugIn.GetParameter("EstRad", 0, float.MaxValue))[species];
            _estMoist = ((Landis.Library.Parameters.Species.AuxParm<float>)(Parameter<float>)PlugIn.GetParameter("EstMoist", 0, float.MaxValue))[species];
            _psnTMin = ((Landis.Library.Parameters.Species.AuxParm<float>)(Parameter<float>)PlugIn.GetParameter("PsnTMin", -10, 10))[species];
            _halfSat = ((Landis.Library.Parameters.Species.AuxParm<float>)(Parameter<float>)PlugIn.GetParameter("HalfSat", 0, float.MaxValue))[species];
            _folLignin = ((Landis.Library.Parameters.Species.AuxParm<float>)(Parameter<float>)PlugIn.GetParameter("FolLignin", 0, float.MaxValue))[species];
            _k = ((Landis.Library.Parameters.Species.AuxParm<float>)(Parameter<float>)PlugIn.GetParameter("K", 0, float.MaxValue))[species];
            _h2 = ((Landis.Library.Parameters.Species.AuxParm<ushort>)(Parameter<ushort>)PlugIn.GetParameter("H2", 0, int.MaxValue))[species];
            _h3 = ((Landis.Library.Parameters.Species.AuxParm<ushort>)(Parameter<ushort>)PlugIn.GetParameter("H3", 0, int.MaxValue))[species];
            _h4 = ((Landis.Library.Parameters.Species.AuxParm<ushort>)(Parameter<ushort>)PlugIn.GetParameter("H4", 0, int.MaxValue))[species];
            _folN = ((Landis.Library.Parameters.Species.AuxParm<float>)(Parameter<float>)PlugIn.GetParameter("FolN", 0, 10))[species];

            _maintResp = ((Landis.Library.Parameters.Species.AuxParm<float>)(Parameter<float>)PlugIn.GetParameter("MaintResp", 0, 1))[species];
            _psnTOpt = ((Landis.Library.Parameters.Species.AuxParm<float>)(Parameter<float>)PlugIn.GetParameter("PsnTOpt", 0, 30))[species];
            _dVPD1 = ((Landis.Library.Parameters.Species.AuxParm<float>)(Parameter<float>)PlugIn.GetParameter("DVPD1", 0, 10))[species];
            _dVPD2 = ((Landis.Library.Parameters.Species.AuxParm<float>)(Parameter<float>)PlugIn.GetParameter("DVPD2", 0, 10))[species];
            _wUEcnst = ((Landis.Library.Parameters.Species.AuxParm<float>)(Parameter<float>)PlugIn.GetParameter("WUEcnst", 0, 200))[species];
            _amaxA = ((Landis.Library.Parameters.Species.AuxParm<float>)(Parameter<float>)PlugIn.GetParameter("AmaxA", -200, 200))[species];
            _amaxB = ((Landis.Library.Parameters.Species.AuxParm<float>)(Parameter<float>)PlugIn.GetParameter("AmaxB", 0, float.MaxValue))[species];
            _q10 = ((Landis.Library.Parameters.Species.AuxParm<float>)(Parameter<float>)PlugIn.GetParameter("Q10", 0, 10))[species];
        }


        /*
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
        private static Landis.Library.Parameters.Species.AuxParm<bool>  preventEstablishment;
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

        public static float FolLignin(this ISpecies s)
        {
            return folLignin[s]; 
        }
        public static bool PreventEstablishment(this ISpecies s)
        {
            return preventEstablishment[s]; 
        }
        public static float HalfSat(this ISpecies s)
        {
            return halfSat[s]; 
        }
        public static ushort H2(this ISpecies s)
        {
            return h2[s];
        }
        public static ushort H3(this ISpecies s)
        {
            return h3[s];
        }
        public static ushort H4(this ISpecies s)
        {
            return h4[s];
        }
        public static float PsnTMin(this ISpecies s)
        {
            return psnTMin[s];
        }
        public static float PsnTOpt(this ISpecies s)
        {
            return psnTOpt[s];
        }
        public static float DVPD1(this ISpecies s)
        {
            return dVPD1[s];
        }
        public static float FolN(this ISpecies s)
        {
            return folN[s];
        }
        public static float WUEcnst(this ISpecies s)
        {
            return wUEcnst[s];
        }
        public static float TOfol(this ISpecies s)
        {
            return tOfol[s];
        }
        public static float TOroot(this ISpecies s)
        {
            return tOroot[s];
        }
        public static float TOwood(this ISpecies s)
        {
            return tOwood[s];
        }
        public static float SLWmax(this ISpecies s)
        {
            return sLWmax[s];
        }
        public static float SLWDel(this ISpecies s)
        {
            return sLWDel[s];
        }
        public static float K(this ISpecies s)
        {
            return k[s];
        }
        public static float FracFol(this ISpecies s)
        {
            return fracFol[s];
        }
        public static float CFracBiomass(this ISpecies s)
        {
            return cFracBiomass[s];
        }
        public static float DNSC(this ISpecies s)
        {
            return dNSC[s];
        }
        public static float KWdLit(this ISpecies s)
        {
            return kWdLit[s];
        }
              
        public static float PsnAgeRed(this ISpecies s)
        {
            return psnAgeRed[s];
        }
        public static float FrActWd(this ISpecies s)
        {
            return frActWd[s];
        }
        public static float DVPD2(this ISpecies s)
        {
            return dVPD2[s];
        }
        public static float InitialNSC(this ISpecies s)
        {
            return initialNSC[s];
        }
        public static float Q10(this ISpecies s)
        {
            return q10[s];
        }
        public static float AmaxA(this ISpecies s)
        {
            return amaxA[s];
        }
        public static float AmaxB(this ISpecies s)
        {
            return amaxB[s];
        }
        public static float MaintResp(this ISpecies s)
        {
            return maintResp[s];
        }
        public static float BFolResp(this ISpecies s)
        {
            return bFolResp[s];
        }
        public static float FracBelowG(this ISpecies s)
        {
            return fracBelowG[s];
        }
        public static float EstRad(this ISpecies s)
        {
            return estRad[s];
        }
        public static float EstMoist(this ISpecies s)
        {
            return estMoist[s];
        }
        public static List<string> ParameterNames
        {
            get
            {
                return typeof(Species).GetFields(BindingFlags.Static | BindingFlags.NonPublic).Select(x => x.Name).ToList();
            }
        }

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
         */
    }
}
