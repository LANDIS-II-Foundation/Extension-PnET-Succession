//  Copyright 2005-2010 Portland State University, University of Wisconsin
//  Authors:  Robert M. Scheller, James B. Domingo

using Landis.Library.Succession;
using Landis.Core;
using Edu.Wisc.Forest.Flel.Util;
using Landis.SpatialModeling;
using System.Collections.Generic;
using System.Diagnostics;
 
namespace Landis.Extension.Succession.BiomassPnET
{

    /// <summary>
    /// The parameters for biomass succession.
    /// </summary>
    public class InputParameters
        : IInputParameters 
    {
        private int timestep;
        private int canopylayeragespan;
        
        private SeedingAlgorithms seedAlg;
        private Library.Biomass.Ecoregions.AuxParm<string> climatefilename;
         
        private int latitude;
        private List<int> canopylayerages ;
        private List<float[]> canopylayerbiomfractions;
        private List<int> canopylayernumbers;
        
        private Landis.Library.Biomass.Species.AuxParm<float> tofol;
        private Landis.Library.Biomass.Species.AuxParm<float> folret;
        private Landis.Library.Biomass.Species.AuxParm<float> towood;
        private Landis.Library.Biomass.Species.AuxParm<float> toroot;
        private Landis.Library.Biomass.Species.AuxParm<float> gddfolst;
        private Landis.Library.Biomass.Species.AuxParm<float> mortCurveShapeParm;
        private Landis.Library.Biomass.Species.AuxParm<float> growthCurveShapeParm;
        private Landis.Library.Biomass.Species.AuxParm<float> slwmax;
        private Landis.Library.Biomass.Species.AuxParm<float> slwdel;
        private Landis.Library.Biomass.Species.AuxParm<int> cddfolend;//HDDFolEnd;
        private Landis.Library.Biomass.Species.AuxParm<float> wuecnst;
        private Landis.Library.Biomass.Species.AuxParm<float> dnsc;
        private Landis.Library.Biomass.Species.AuxParm<float> maintresp;
        private Landis.Library.Biomass.Species.AuxParm<float> rtstratio;
        private Landis.Library.Biomass.Species.AuxParm<float> q10;
        private Landis.Library.Biomass.Species.AuxParm<float> psntmin;
        private Landis.Library.Biomass.Species.AuxParm<float> halfsat;
        private Landis.Library.Biomass.Species.AuxParm<float> psntopt;
        private Landis.Library.Biomass.Species.AuxParm<float> foln;
        private Landis.Library.Biomass.Species.AuxParm<float> bfolresp;
        private Landis.Library.Biomass.Species.AuxParm<float> follignin;
        private Landis.Library.Biomass.Species.AuxParm<float> kwdlit;
        private Landis.Library.Biomass.Species.AuxParm<float> grmstsens;
        private Landis.Library.Biomass.Species.AuxParm<float> wltpnt;
        private Landis.Library.Biomass.Species.AuxParm<float> dvpd2;
        private Landis.Library.Biomass.Species.AuxParm<float> dvpd1;
        private Landis.Library.Biomass.Species.AuxParm<float> amaxa;
        private Landis.Library.Biomass.Species.AuxParm<float> amaxb;
        private Landis.Library.Biomass.Species.AuxParm<float> psnagered;
        private Landis.Library.Biomass.Species.AuxParm<float> k;
        private Landis.Library.Biomass.Species.AuxParm<float> estmoisturesensitivity;
        private Landis.Library.Biomass.Species.AuxParm<float> estradsensitivity;

        private Landis.Library.Biomass.Ecoregions.AuxParm<int> aet;
        private Landis.Library.Biomass.Ecoregions.AuxParm<int> whc;
        private Landis.Library.Biomass.Ecoregions.AuxParm<float> preciplossfrac;
        private Landis.Library.Biomass.Ecoregions.AuxParm<int> porosity;
        private Landis.Library.Biomass.Ecoregions.AuxParm<float> leakagefrac;
        
        private  ISiteVar<bool> hassiteoutput;

        private string dynamicInputFile;
        private string ageOnlyDisturbanceParms;
        private string initCommunities;
        private string communitiesMap;

        public ISiteVar<bool> HasSiteOutput
        {
            get
            {
                return hassiteoutput;
            }
        }
        public List<int> CanopyLayerAges
        {
            get
            {
                return canopylayerages;
            }
            set
            {
                canopylayerages = value;
            }
        }

        public List<int> CanopyLayerNumbers
        {
            get
            {
                return canopylayernumbers;
            }
            set
            {
                canopylayernumbers = value;
            }
        }
        public List<float[]> CanopyLayerBiomFractions
        {
            get
            {
                return canopylayerbiomfractions;
            }
            set
            {
                canopylayerbiomfractions = value;
            }
        }
 
        public int Latitude
        {
            get
            {
                return latitude;
            }
            set
            {
                latitude = value;
            }
        }

        public Library.Biomass.Ecoregions.AuxParm<string> climateFileName
        {
            get
            {
                return climatefilename;
            }
            set
            {
                climatefilename = value;
            }
        }
         
        //---------------------------------------------------------------------
        /// <summary>
        /// Timestep (years)
        /// </summary>
        int startyear;
        public int StartYear
        {
            get
            {
                return startyear;
            }
            set 
            {
                startyear = value;
            }
        
        }
         

        public int Timestep
        {
            get {
                return timestep;
            }
            set {
                if (value < 0)
                    throw new InputValueException(value.ToString(), "Timestep must be > or = 0");
                timestep = value;
            }
        }

        //---------------------------------------------------------------------

        /// <summary>
        /// Seeding algorithm
        /// </summary>
        public SeedingAlgorithms SeedAlgorithm
        {
            get {
                return seedAlg;
            }
            set {
                seedAlg = value;
            }
        }

        public Landis.Library.Biomass.Ecoregions.AuxParm<float> LeakageFrac
        {
            get
            {
                return leakagefrac;
            }
        }
        public Landis.Library.Biomass.Ecoregions.AuxParm<int> Porosity
        {
            get
            {
                return porosity;
            }
        }
        public Landis.Library.Biomass.Ecoregions.AuxParm<float> PrecipLossFrac
        {
            get
            {
                return preciplossfrac;
            }
        }
        public Landis.Library.Biomass.Ecoregions.AuxParm<int> WHC
        {
            get
            {
                return whc;
            }
        }
        
        //---------------------------------------------------------------------

        /// <summary>
        /// Path to the file with the initial communities' definitions.
        /// </summary>
        public string InitialCommunities
        {
            get
            {
                return initCommunities;
            }

            set
            {
                if (value != null)
                {
                    ValidatePath(value);
                }
                initCommunities = value;
            }
        }

        //---------------------------------------------------------------------

        /// <summary>
        /// Path to the raster file showing where the initial communities are.
        /// </summary>
        public string InitialCommunitiesMap
        {
            get
            {
                return communitiesMap;
            }

            set
            {
                if (value != null)
                {
                    ValidatePath(value);
                }
                communitiesMap = value;
            }
        }
       
        //---------------------------------------------------------------------

         

        public Landis.Library.Biomass.Species.AuxParm<float> TOroot
        {
            get {
                return toroot;
            }
        }
        public Landis.Library.Biomass.Species.AuxParm<float> TOwood
        {
            get {
                return towood;
            }
        }

        public Landis.Library.Biomass.Species.AuxParm<float> FolRet
        {
            get {
                return folret;
            }
        }
        public Landis.Library.Biomass.Species.AuxParm<float> TOfol
        {
            get {
                return tofol;
            }
        }
        public Landis.Library.Biomass.Species.AuxParm<float> GDDFolSt
        {
            get {
                return gddfolst;
            }
        }
        
        public Landis.Library.Biomass.Species.AuxParm<float> SLWDel
        {
            get
            {
                return slwdel;
            }
        }
        
        public Landis.Library.Biomass.Species.AuxParm<float> SLWmax
        {
            get
            {
                return slwmax;
            }
        }

        public Landis.Library.Biomass.Species.AuxParm<int> CDDFolEnd
        {
            get
            {
                return cddfolend;
            }
        }
        
        public Landis.Library.Biomass.Species.AuxParm<float> FolN
        {
            get
            {
                return foln;
            }
        }


        public Landis.Library.Biomass.Species.AuxParm<float> WltPnt
        {
            get
            {
                return wltpnt;
            }
        }

        public Landis.Library.Biomass.Species.AuxParm<float> GrMstSens
        {
            get
            {
                return grmstsens;
            }
        }

        public Landis.Library.Biomass.Species.AuxParm<float> FolLignin
        {
            get
            {
                return follignin;
            }
        }
        public Landis.Library.Biomass.Species.AuxParm<float> KWdLit
        {
            get
            {
                return kwdlit;
            }
        }
        public Landis.Library.Biomass.Species.AuxParm<float> BFolResp
        {
            get
            {
                return bfolresp;
            }
        }
        public Landis.Library.Biomass.Species.AuxParm<float> DVPD2
        {
            get
            {
                return dvpd2;
            }
        }
        public Landis.Library.Biomass.Species.AuxParm<float> DVPD1
        {
            get
            {
                return dvpd1;
            }
        }
        public Landis.Library.Biomass.Species.AuxParm<float> AmaxA
        {
            get
            {
                return amaxa;
            }
        }
        public Landis.Library.Biomass.Species.AuxParm<float> AmaxB
        {
            get
            {
                return amaxb;
            }
        }
        public Landis.Library.Biomass.Species.AuxParm<float> MaintResp
        {
            get
            {
                return maintresp;
            }
        }

        public Landis.Library.Biomass.Species.AuxParm<float> RtStRatio
        {
            get
            {
                return rtstratio;
            }
        }
        
        public Landis.Library.Biomass.Species.AuxParm<float> DNSC
        {
            get
            {
                return dnsc;
            }
        }
        public Landis.Library.Biomass.Species.AuxParm<float> PsnAgeRed
        {
            get
            {
                return psnagered;
            }
        }
         
        public Landis.Library.Biomass.Species.AuxParm<float> K
        {
            get
            {
                return k;
            }
        }
        
        public Landis.Library.Biomass.Species.AuxParm<float> PsnTOpt
        {
            get
            {
                return psntopt;
            }
        }
        public Landis.Library.Biomass.Species.AuxParm<float> Q10
        {
            get
            {
                return q10;
            }
        }
        public Landis.Library.Biomass.Species.AuxParm<float> PsnTMin
        {
            get
            {
                return psntmin;
            }
        }

        public Landis.Library.Biomass.Species.AuxParm<float> EstMoist
        {
            get
            {
                return estmoisturesensitivity;
            }
            set
            {
                estmoisturesensitivity = value;
            }
        }
        public Landis.Library.Biomass.Species.AuxParm<float> EstRad
        {
            get
            {
                return estradsensitivity;
            }
        }
        public Landis.Library.Biomass.Species.AuxParm<float> HalfSat
        {
            get
            {
                return halfsat;
            }
        }
        
        public Landis.Library.Biomass.Species.AuxParm<float> WUEcnst
        {
            get
            {
                return wuecnst;
            }
        }
        
         
        				
        //---------------------------------------------------------------------

        public Landis.Library.Biomass.Ecoregions.AuxParm<int> AET
        {
            get {
                return aet;
            }
            set
            {
                aet = value;
            }
        }
        
        public int CanopyLayerAgeSpan
        {
            get
            {
                return canopylayeragespan;
            }
            set
            {
                canopylayeragespan = value;
            }
        }
        //---------------------------------------------------------------------
        /// <summary>
        /// Input file for the dynamic inputs
        /// </summary>
        public string DynamicInputFile
        {
            get
            {
                return dynamicInputFile;
            }
            set
            {
                dynamicInputFile = value;
            }
        }
        
       
        //---------------------------------------------------------------------
        /// <summary>
        /// Path to the optional file with the biomass parameters for age-only
        /// disturbances.
        /// </summary>
        public string AgeOnlyDisturbanceParms
        {
            get {
                return ageOnlyDisturbanceParms;
            }
            set {
                string path = value;
                if (path.Trim(null).Length == 0)
                    throw new InputValueException(path, "\"{0}\" is not a valid path.", path);
                ageOnlyDisturbanceParms = value;
            }
        }

         
        //---------------------------------------------------------------------

        public InputParameters()
        {
            hassiteoutput = PlugIn.ModelCore.Landscape.NewSiteVar<bool>();
            canopylayerbiomfractions = new List<float[]>();
            canopylayerages = new List<int>();
            climatefilename = new Library.Biomass.Ecoregions.AuxParm<string>(PlugIn.modelCore.Ecoregions);
            slwdel = new Landis.Library.Biomass.Species.AuxParm<float>(PlugIn.ModelCore.Species);
            toroot = new Landis.Library.Biomass.Species.AuxParm<float>(PlugIn.ModelCore.Species);
            towood = new Landis.Library.Biomass.Species.AuxParm<float>(PlugIn.ModelCore.Species);
            folret = new Landis.Library.Biomass.Species.AuxParm<float>(PlugIn.ModelCore.Species);
            tofol = new Landis.Library.Biomass.Species.AuxParm<float>(PlugIn.ModelCore.Species);
            gddfolst = new Landis.Library.Biomass.Species.AuxParm<float>(PlugIn.ModelCore.Species);
            mortCurveShapeParm = new Landis.Library.Biomass.Species.AuxParm<float>(PlugIn.ModelCore.Species);
            growthCurveShapeParm = new Landis.Library.Biomass.Species.AuxParm<float>(PlugIn.ModelCore.Species);
            slwmax = new Landis.Library.Biomass.Species.AuxParm<float>(PlugIn.ModelCore.Species);
            cddfolend = new Landis.Library.Biomass.Species.AuxParm<int>(PlugIn.ModelCore.Species);
            wuecnst = new Landis.Library.Biomass.Species.AuxParm<float>(PlugIn.ModelCore.Species);
            q10 = new Landis.Library.Biomass.Species.AuxParm<float>(PlugIn.ModelCore.Species);
            psntmin = new Landis.Library.Biomass.Species.AuxParm<float>(PlugIn.ModelCore.Species);
            halfsat = new Landis.Library.Biomass.Species.AuxParm<float>(PlugIn.ModelCore.Species);
            estradsensitivity = new Landis.Library.Biomass.Species.AuxParm<float >(PlugIn.ModelCore.Species);
            estmoisturesensitivity = new Landis.Library.Biomass.Species.AuxParm<float>(PlugIn.ModelCore.Species);
            psntopt = new Landis.Library.Biomass.Species.AuxParm<float>(PlugIn.ModelCore.Species);
            foln = new Landis.Library.Biomass.Species.AuxParm<float>(PlugIn.ModelCore.Species);
            bfolresp = new Landis.Library.Biomass.Species.AuxParm<float>(PlugIn.ModelCore.Species);
            follignin = new Landis.Library.Biomass.Species.AuxParm<float>(PlugIn.ModelCore.Species);
            kwdlit = new Landis.Library.Biomass.Species.AuxParm<float>(PlugIn.ModelCore.Species);
            grmstsens = new Landis.Library.Biomass.Species.AuxParm<float>(PlugIn.ModelCore.Species);
            dvpd2 = new Landis.Library.Biomass.Species.AuxParm<float>(PlugIn.ModelCore.Species);
            dvpd1 = new Landis.Library.Biomass.Species.AuxParm<float>(PlugIn.ModelCore.Species);
            amaxa = new Landis.Library.Biomass.Species.AuxParm<float>(PlugIn.ModelCore.Species);
            amaxb = new Landis.Library.Biomass.Species.AuxParm<float>(PlugIn.ModelCore.Species);
            psnagered = new Landis.Library.Biomass.Species.AuxParm<float>(PlugIn.ModelCore.Species);
            k = new Landis.Library.Biomass.Species.AuxParm<float>(PlugIn.ModelCore.Species);
            dnsc = new Landis.Library.Biomass.Species.AuxParm<float>(PlugIn.ModelCore.Species);
            maintresp = new Landis.Library.Biomass.Species.AuxParm<float>(PlugIn.ModelCore.Species);
            wltpnt = new Landis.Library.Biomass.Species.AuxParm<float>(PlugIn.ModelCore.Species);
            rtstratio = new Landis.Library.Biomass.Species.AuxParm<float>(PlugIn.ModelCore.Species);
            whc = new Landis.Library.Biomass.Ecoregions.AuxParm<int>(PlugIn.ModelCore.Ecoregions);
            aet = new Landis.Library.Biomass.Ecoregions.AuxParm<int>(PlugIn.ModelCore.Ecoregions);
            preciplossfrac = new Landis.Library.Biomass.Ecoregions.AuxParm<float>(PlugIn.ModelCore.Ecoregions);
            porosity  = new Landis.Library.Biomass.Ecoregions.AuxParm<int>(PlugIn.ModelCore.Ecoregions);
            leakagefrac = new Landis.Library.Biomass.Ecoregions.AuxParm<float>(PlugIn.ModelCore.Ecoregions);
            canopylayernumbers = new List<int>();
        }
        //---------------------------------------------------------------------

        private void ValidatePath(string path)
        {
            if (string.IsNullOrEmpty(path))
                throw new InputValueException();
            if (path.Trim(null).Length == 0)
                throw new InputValueException(path,
                                              "\"{0}\" is not a valid path.",
                                              path);
        }

    }
}
