using Landis.Core;
using System.Collections.Generic;
using System.Linq;

namespace Landis.Extension.Succession.BiomassPnET
{
    /// <summary>
    /// The information for a tree species (its index and parameters).
    /// </summary>
    public class SpeciesPnET : ISpeciesPNET
    {
        static List<Tuple<ISpecies, ISpeciesPNET>> SpeciesCombinations;

         
         public List<ISpeciesPNET> AllSpecies 
         { 
             get
             { 
                 return SpeciesCombinations.Select(combination => combination.Item2).ToList(); 
             } 
         } 
 
 
         public ISpeciesPNET this[ISpecies species] 
         { 
             get
             { 
                 return SpeciesCombinations.Where(spc => spc.Item1 == species).First().Item2; 
             } 
         } 
         public ISpecies this[ISpeciesPNET species] 
         { 
             get
             { 
                 return SpeciesCombinations.Where(spc => spc.Item2 == species).First().Item1; 
             } 
         } 


        #region private variables
        private float _co2HalfSatEff;
        private float _cfracbiomass;
        private float _kwdlit;
        private float _dnsc;
        private float _fracbelowg;
        private float _fracfol;
        private float _fractWd;
        private float _psnagered;
        private float _h1;
        private float _h2;
        private float _h3;
        private float _h4;
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
        private float _maxPest;
        private float _follignin;
        private bool _preventestablishment;
        private float _psntopt;
        private float _q10;
        private float _psntmin;
        private float _psntmax;
        private float _dvpd1;
        private float _foln;
        private float _dvpd2;
        private float _amaxa;
        private float _amaxb;
        private float _co2AMaxBEff;
        private float _maintresp;
        private float _bfolresp;
        private string _ozoneSens;
        private float _coldTol;
        private int _initBiomass;
        private string name;
        private int index;
        
        private int  maxSproutAge;
        private int minSproutAge;
        private Landis.Core.PostFireRegeneration postfireregeneration;
        private int maxSeedDist;
        private int effectiveSeedDist;
        private float  vegReprodProb;
        private byte fireTolerance;
        private byte shadeTolerance;
        int maturity;
        int longevity;
        private float _folNShape;
        private float _maxFolN;
        private float _fracFolShape;
        private float _maxFracFol;
        private float _o3Coeff;
        private float _leafOnMinT;
        # endregion


        #region private static species variables
        private static Landis.Library.Parameters.Species.AuxParm<float> co2HalfSatEff;
        //private static Landis.Library.Parameters.Species.AuxParm<float> wuecnst;
        private static Landis.Library.Parameters.Species.AuxParm<float> dnsc;
        private static Landis.Library.Parameters.Species.AuxParm<float> cfracbiomass;
        private static Landis.Library.Parameters.Species.AuxParm<float> kwdlit;
        private static Landis.Library.Parameters.Species.AuxParm<float> fracbelowg;
        private static Landis.Library.Parameters.Species.AuxParm<float> fracfol;
        private static Landis.Library.Parameters.Species.AuxParm<float> fractWd;
        private static Landis.Library.Parameters.Species.AuxParm<float> psnagered;
        private static Landis.Library.Parameters.Species.AuxParm<float> h1;
        private static Landis.Library.Parameters.Species.AuxParm<float> h2;
        private static Landis.Library.Parameters.Species.AuxParm<float> h3;
        private static Landis.Library.Parameters.Species.AuxParm<float> h4;
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
        private static Landis.Library.Parameters.Species.AuxParm<float> maxPest;
        private static Landis.Library.Parameters.Species.AuxParm<float> follignin;
        private static Landis.Library.Parameters.Species.AuxParm<bool> preventestablishment;
        private static Landis.Library.Parameters.Species.AuxParm<float> psntopt;


        private static Landis.Library.Parameters.Species.AuxParm<float> q10;
        private static Landis.Library.Parameters.Species.AuxParm<float> psntmin;
        private static Landis.Library.Parameters.Species.AuxParm<float> psntmax;
        private static Landis.Library.Parameters.Species.AuxParm<float> dvpd1;
        private static Landis.Library.Parameters.Species.AuxParm<float> dvpd2;
        private static Landis.Library.Parameters.Species.AuxParm<float> foln;
        private static Landis.Library.Parameters.Species.AuxParm<float> amaxa;
        private static Landis.Library.Parameters.Species.AuxParm<float> amaxb;
        private static Landis.Library.Parameters.Species.AuxParm<float> co2AMaxBEff;
        
        private static Landis.Library.Parameters.Species.AuxParm<float> maintresp;
        private static Landis.Library.Parameters.Species.AuxParm<float> bfolresp;
        private static Landis.Library.Parameters.Species.AuxParm<float> coldTol;


        private static Landis.Library.Parameters.Species.AuxParm<string> ozoneSens;

        private static Landis.Library.Parameters.Species.AuxParm<float> folNShape;
        private static Landis.Library.Parameters.Species.AuxParm<float> maxFolN;
        private static Landis.Library.Parameters.Species.AuxParm<float> fracFolShape;
        private static Landis.Library.Parameters.Species.AuxParm<float> maxFracFol;
        private static Landis.Library.Parameters.Species.AuxParm<float> o3Coeff;
        private static Landis.Library.Parameters.Species.AuxParm<float> leafOnMinT;
        #endregion

        public SpeciesPnET()
        {
            #region initialization of private static species variables
            co2HalfSatEff = ((Landis.Library.Parameters.Species.AuxParm<float>)(Parameter<float>)PlugIn.GetParameter("CO2HalfSatEff"));
            //wuecnst = ((Landis.Library.Parameters.Species.AuxParm<float>)(Parameter<float>)PlugIn.GetParameter("WUEcnst"));
            dnsc =  ((Landis.Library.Parameters.Species.AuxParm<float>)(Parameter<float>)PlugIn.GetParameter("DNSC"));
            cfracbiomass=  ((Landis.Library.Parameters.Species.AuxParm<float>)(Parameter<float>)PlugIn.GetParameter("CFracBiomass"));
            kwdlit = ((Landis.Library.Parameters.Species.AuxParm<float>)(Parameter<float>)PlugIn.GetParameter("kwdlit"));
            fracbelowg = ((Landis.Library.Parameters.Species.AuxParm<float>)(Parameter<float>)PlugIn.GetParameter("fracbelowg"));
            fracfol = ((Landis.Library.Parameters.Species.AuxParm<float>)(Parameter<float>)PlugIn.GetParameter("fracfol"));
            fractWd = ((Landis.Library.Parameters.Species.AuxParm<float>)(Parameter<float>)PlugIn.GetParameter("fractWd"));
            psnagered = ((Landis.Library.Parameters.Species.AuxParm<float>)(Parameter<float>)PlugIn.GetParameter("psnagered"));
            h1 = ((Landis.Library.Parameters.Species.AuxParm<float>)(Parameter<float>)PlugIn.GetParameter("h1"));
            h2 = ((Landis.Library.Parameters.Species.AuxParm<float>)(Parameter<float>)PlugIn.GetParameter("h2"));
            h3 = ((Landis.Library.Parameters.Species.AuxParm<float>)(Parameter<float>)PlugIn.GetParameter("h3"));
            h4 = ((Landis.Library.Parameters.Species.AuxParm<float>)(Parameter<float>)PlugIn.GetParameter("h4"));
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
            maxPest = ((Landis.Library.Parameters.Species.AuxParm<float>)(Parameter<float>)PlugIn.GetParameter("MaxPest"));
            follignin = ((Landis.Library.Parameters.Species.AuxParm<float>)(Parameter<float>)PlugIn.GetParameter("follignin"));
            preventestablishment = ((Landis.Library.Parameters.Species.AuxParm<bool>)(Parameter<bool>)PlugIn.GetParameter("preventestablishment"));
            psntopt = ((Landis.Library.Parameters.Species.AuxParm<float>)(Parameter<float>)PlugIn.GetParameter("psntopt"));
            q10 = ((Landis.Library.Parameters.Species.AuxParm<float>)(Parameter<float>)PlugIn.GetParameter("q10"));
            psntmin = ((Landis.Library.Parameters.Species.AuxParm<float>)(Parameter<float>)PlugIn.GetParameter("psntmin"));
            psntmax = ((Landis.Library.Parameters.Species.AuxParm<float>)(Parameter<float>)PlugIn.GetParameter("psntmax"));
            dvpd1 = ((Landis.Library.Parameters.Species.AuxParm<float>)(Parameter<float>)PlugIn.GetParameter("dvpd1"));
            dvpd2 = ((Landis.Library.Parameters.Species.AuxParm<float>)(Parameter<float>)PlugIn.GetParameter("dvpd2"));
            foln = ((Landis.Library.Parameters.Species.AuxParm<float>)(Parameter<float>)PlugIn.GetParameter("foln"));
            amaxa = ((Landis.Library.Parameters.Species.AuxParm<float>)(Parameter<float>)PlugIn.GetParameter("amaxa"));
            amaxb = ((Landis.Library.Parameters.Species.AuxParm<float>)(Parameter<float>)PlugIn.GetParameter("amaxb"));
            co2AMaxBEff = ((Landis.Library.Parameters.Species.AuxParm<float>)(Parameter<float>)PlugIn.GetParameter("CO2AMaxBEff"));
            maintresp = ((Landis.Library.Parameters.Species.AuxParm<float>)(Parameter<float>)PlugIn.GetParameter("maintresp"));
            bfolresp = ((Landis.Library.Parameters.Species.AuxParm<float>)(Parameter<float>)PlugIn.GetParameter("bfolresp"));
            ozoneSens = ((Landis.Library.Parameters.Species.AuxParm<string>)(Parameter<string>)PlugIn.GetParameter("O3StomataSens"));
            folNShape = ((Landis.Library.Parameters.Species.AuxParm<float>)(Parameter<float>)PlugIn.GetParameter("FolNShape"));
            maxFolN = ((Landis.Library.Parameters.Species.AuxParm<float>)(Parameter<float>)PlugIn.GetParameter("MaxFolN"));
            if (maxFolN[this] == -9999F)
                maxFolN = foln;
            fracFolShape = ((Landis.Library.Parameters.Species.AuxParm<float>)(Parameter<float>)PlugIn.GetParameter("FracFolShape"));
            maxFracFol = ((Landis.Library.Parameters.Species.AuxParm<float>)(Parameter<float>)PlugIn.GetParameter("MaxFracFol"));
            if (maxFracFol[this] == -9999F)
                maxFracFol = fracfol;
            o3Coeff = ((Landis.Library.Parameters.Species.AuxParm<float>)(Parameter<float>)PlugIn.GetParameter("O3GrowthSens"));            
            coldTol = ((Landis.Library.Parameters.Species.AuxParm<float>)(Parameter<float>)PlugIn.GetParameter("ColdTol"));
            leafOnMinT = ((Landis.Library.Parameters.Species.AuxParm<float>)(Parameter<float>)PlugIn.GetParameter("LeafOnMinT")); //Optional
            // If LeafOnMinT is not provided, then set to PsnMinT
            if (leafOnMinT[this] == -9999F)
                leafOnMinT = psntmin;
            #endregion

            SpeciesCombinations = new List<Tuple<ISpecies, ISpeciesPNET>>();
             
            foreach (ISpecies spc in PlugIn.ModelCore.Species)
            {
                SpeciesPnET species = new SpeciesPnET(spc);

                SpeciesCombinations.Add(new Tuple<ISpecies, ISpeciesPNET>(spc, species));
            }


        }


        SpeciesPnET(PostFireRegeneration postFireGeneration,
            //float wuecnst, 
            float dnsc,
            float cfracbiomass,
            float kwdlit,
            float fracbelowg,
            float fracfol,
            float fractWd,
            float psnagered,
            float h1,
            float h2,
            float h3,
            float h4,
            float slwdel,
            float slwmax,
            float tofol,
            float toroot,
            float halfsat,
            float initialnsc,
            float k,
            float towood,
            float estrad,
            float estmoist,
            float maxPest,
            float follignin,
            bool preventestablishment,
            float psntopt,
            float q10,
            float psntmin,
            float psntmax,
            float dvpd1,
            float dvpd2,
            float foln,
            float amaxa,
            float amaxb,
            float co2AMaxBEff,
            float maintresp,
            float bfolresp,
            float coldTol,
            string ozoneSens,
            int Index,
            string name,
            int maxSproutAge,
            int minSproutAge,
            int maxSeedDist,
            int effectiveSeedDist,
            float vegReprodProb,
            byte fireTolerance,
            byte shadeTolerance,
            int maturity,
            int longevity,
            float folNShape,
            float maxFolN,
            float fracFolShape,
            float maxFracFol,
            float o3Coeff,
            float leafOnMinT
            )
        {
            this.postfireregeneration = postFireGeneration;
            //this._wuecnst = wuecnst;
            this._dnsc = dnsc;
            this._cfracbiomass = cfracbiomass;
            this._kwdlit = kwdlit;
            this._fracbelowg = fracbelowg;
            this._fracfol = fracfol;
            this._fractWd = fractWd;
            this._psnagered = psnagered;
            this._h1 = h1;
            this._h2 = h2;
            this._h3 = h3;
            this._h4 = h4;
            this._slwdel = slwdel;
            this._slwmax = slwmax;
            this._tofol = tofol;
            this._toroot = toroot;
            this._halfsat = halfsat;
            this._initialnsc = initialnsc;
            this._k = k;
            this._towood = towood;
            this._estrad = estrad;
            this._estmoist = estmoist;
            this._maxPest = maxPest;
            this._follignin = follignin;
            this._preventestablishment = preventestablishment;
            this._psntopt = psntopt;
            this._q10 = q10;
            this._psntmin = psntmin;
            this._psntmax = psntmax;
            this._dvpd1 = dvpd1;
            this._foln = foln;
            this._dvpd2 = dvpd2;
            this._amaxa = amaxa;
            this._amaxb = amaxb;
            this._co2AMaxBEff = co2AMaxBEff;
            this._maintresp = maintresp;
            this._bfolresp = bfolresp;
            this._coldTol = coldTol;
            this._ozoneSens = ozoneSens;
            this.index = Index;
            this.name = name;
            this.maxSproutAge = maxSproutAge;
            this.minSproutAge = minSproutAge;
            this.postfireregeneration = postFireGeneration;
            this.maxSeedDist = maxSeedDist;
            this.effectiveSeedDist = effectiveSeedDist;
            this.vegReprodProb = vegReprodProb;
            this.fireTolerance = fireTolerance;
            this.shadeTolerance = shadeTolerance;
            this.maturity = maturity;
            this.longevity = longevity;
            this._folNShape = folNShape;
            this._maxFolN = maxFolN;
            this._fracFolShape = fracFolShape;
            this._maxFracFol = maxFracFol;
            this._o3Coeff = o3Coeff;
            this._leafOnMinT = leafOnMinT;
            //  initBiomass = initBiomass - Senescence
            this._initBiomass = (int)((uint)(1F / dnsc * (ushort)initialnsc) - ((uint)(fracbelowg * (uint)(1F / dnsc * (ushort)initialnsc))*toroot) - ((uint)((1 - fracbelowg) * (uint)(1F / dnsc * (ushort)initialnsc)) * towood));
            //senescence = ((Root * species.TOroot) + Wood * species.TOwood);
        }
       
        private SpeciesPnET(ISpecies species)
        {
            //_wuecnst = wuecnst[species];
            _initBiomass = (int)((uint)(1F / dnsc[species] * (ushort)initialnsc[species]) - ((uint)(fracbelowg[species] * (uint)(1F / dnsc[species] * (ushort)initialnsc[species])) * toroot[species]) - ((uint)((1 - fracbelowg[species]) * (uint)(1F / dnsc[species] * (ushort)initialnsc[species])) * towood[species]));
            _dnsc = dnsc[species];
            _cfracbiomass = cfracbiomass[species];
            _kwdlit = kwdlit[species];
            _fracbelowg = fracbelowg[species];
            _fracfol = fracfol[species];
            _fractWd = fractWd[species];
            _psnagered = psnagered[species];
            _h1 = h1[species];
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
            _maxPest = maxPest[species];
            _follignin = follignin[species];
            _preventestablishment = preventestablishment[species];
            _psntopt = psntopt[species];
            _q10 = q10[species]; 
            _psntmin = psntmin[species];
            _psntmax = psntmax[species];
            _dvpd1 = dvpd1[species];
            _foln = foln[species];
            _dvpd2 = dvpd2[species];
            _amaxa = amaxa[species];
            _amaxb = amaxb[species];
            _co2AMaxBEff = co2AMaxBEff[species];
            _maintresp = maintresp[species];
            _bfolresp = bfolresp[species];
            _coldTol = coldTol[species];
            _co2HalfSatEff = co2HalfSatEff[species];
            _ozoneSens = ozoneSens[species];
            index = species.Index;
            name = species.Name;

            maxSproutAge = species.MaxSproutAge;
            minSproutAge = species.MinSproutAge;
            postfireregeneration = species.PostFireRegeneration;
            maxSeedDist = species.MaxSeedDist;
            effectiveSeedDist = species.EffectiveSeedDist;
            vegReprodProb = species.VegReprodProb;
            fireTolerance = species.FireTolerance;
            shadeTolerance = species.ShadeTolerance;
            maturity = species.Maturity;
            longevity = species.Longevity;

            _folNShape = folNShape[species];
            _maxFolN = maxFolN[species];
            _fracFolShape = fracFolShape[species];
            _maxFracFol = maxFracFol[species];
            _o3Coeff = o3Coeff[species];
            _leafOnMinT = leafOnMinT[species];
          
        }
        

        #region Accessors

        public int Index
        {
            get
            {
                return index;
            }
        }
        public float BFolResp
        {
            get
            {
                return _bfolresp;
            }
        }
        public float ColdTol
        {
            get
            {
                return _coldTol;
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
        public float CO2AMaxBEff
        {
            get
            {
                return _co2AMaxBEff;
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
        public float PsnTMax
        {
            get
            {
                if (_psntmax == -9999F)
                    return _psntopt + (_psntopt - _psntmin);
                else 
                    return _psntmax;
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
            get 
            { 
                return _preventestablishment; 
            }
        }
        public float FolLignin
        {
            get 
            { 
                return _follignin; 
            }
        }
        public float EstMoist
        {
            get 
            { 
                return _estmoist; 
            }
        }
        public float MaxPest
        {
            get
            {
                return _maxPest;
            }
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
        public float H4
        {
            get
            {
                return _h4;
            }
        }
        public float H3
        {
            get
            {
                return _h3;
            }
        }
        public float H2
        {
            get
            {
                return _h2;
            }
        }
        public float H1
        {
            get
            {
                return _h1;
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
        public int InitBiomass
        {
            get
            {
                return _initBiomass;
            }
        }
        public float CFracBiomass
        {
            get
            {
                return _cfracbiomass;
            }
        }
        public string Name
        {
            get
            {
                return name;
            }
        }

     
        public int MaxSproutAge
        {
            get
            {
                return maxSproutAge;
            }
        }
        public int MinSproutAge
        {
            get
            {
                return  minSproutAge;
            }
        }

        public float CO2HalfSatEff
        {
            get
            {
                return _co2HalfSatEff;
            }
        }
        public Landis.Core.PostFireRegeneration PostFireRegeneration
        {
            get
            {
                return postfireregeneration;

            }
        }
        
        public int MaxSeedDist
        {
            get
            {
                return maxSeedDist;
            }
        }
        public int EffectiveSeedDist
        {
            get
            {
                return effectiveSeedDist;
            }
        }
       
        public float VegReprodProb
        {
            get
            {
                return vegReprodProb;
            }
        }
        public byte FireTolerance
        {
            get
            {
                return fireTolerance;
            }
        }
        public byte ShadeTolerance
        {
            get
            {
                return shadeTolerance;
            }
        }
        public int Maturity
        {
            get
            {
                return maturity;
            }
        }
        public int Longevity
        {
            get
            {
                return longevity;
            }
        }
        public string O3StomataSens
        {
            get
            {
                return _ozoneSens;
            }
        }

        public float FolNShape
        {
            get
            {
                return _folNShape;
            }
        }
        public float MaxFolN
        {
            get
            {
                return _maxFolN;
            }
        }
        public float FracFolShape
        {
            get
            {
                return _fracFolShape;
            }
        }
        public float MaxFracFol
        {
            get
            {
                return _maxFracFol;
            }
        }
        public float O3GrowthSens
        {
            get
            {
                return _o3Coeff;
            }
        }
        public float LeafOnMinT
        {
            get
            {
                return _leafOnMinT;
            }
        }
        #endregion

        public static List<string> ParameterNames
        {
            get
            {
                System.Type type = typeof(SpeciesPnET); // Get type pointer
                List<string> names = type.GetProperties().Select(x => x.Name).ToList(); // Obtain all fields


                return names;
            }
        }
       
    }
}
