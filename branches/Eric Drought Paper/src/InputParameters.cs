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
        private string climatefilename;
         
        private int latitude;
        private Landis.Library.Biomass.Species.AuxParm<float> foliageturnover;
        private Landis.Library.Biomass.Species.AuxParm<float> folreten;
        private Landis.Library.Biomass.Species.AuxParm<float> woodturnover;
        private Landis.Library.Biomass.Species.AuxParm<float> rootturnover;
        private Landis.Library.Biomass.Species.AuxParm<float> gddfolstart;
        private Landis.Library.Biomass.Species.AuxParm<float> gddfolend;
        private Landis.Library.Biomass.Species.AuxParm<float> mortCurveShapeParm;
        private Landis.Library.Biomass.Species.AuxParm<float> growthCurveShapeParm;
        private Landis.Library.Biomass.Species.AuxParm<float> slwmax;
        private Landis.Library.Biomass.Species.AuxParm<float> slwdel;
        private Landis.Library.Biomass.Species.AuxParm<int> senescStart;
        private Landis.Library.Biomass.Species.AuxParm<float> wueconst;
        private Landis.Library.Biomass.Species.AuxParm<float> dnsc;
        private Landis.Library.Biomass.Species.AuxParm<float> maintresp;
        private Landis.Library.Biomass.Species.AuxParm<float> rootstemratio;
        private Landis.Library.Biomass.Species.AuxParm<float> respq10;
        private Landis.Library.Biomass.Species.AuxParm<float> psntmin;
        private Landis.Library.Biomass.Species.AuxParm<float> halfsat;
        private Landis.Library.Biomass.Species.AuxParm<float> psntopt;
        private Landis.Library.Biomass.Species.AuxParm<float> folncon;
        private Landis.Library.Biomass.Species.AuxParm<float> basefolrespfrac;
        private Landis.Library.Biomass.Species.AuxParm<float> growthmoisturesensitivity;
        private Landis.Library.Biomass.Species.AuxParm<float> wiltingpoint;
        private Landis.Library.Biomass.Species.AuxParm<float> dvpd2;
        private Landis.Library.Biomass.Species.AuxParm<float> dvpd1;
        private Landis.Library.Biomass.Species.AuxParm<float> amaxa;
        private Landis.Library.Biomass.Species.AuxParm<float> amaxb;
        private Landis.Library.Biomass.Species.AuxParm<float> psnagered;
        private Landis.Library.Biomass.Species.AuxParm<float> k;
        private Landis.Library.Biomass.Species.AuxParm<float[]> estmoisturesensitivity;
        private Landis.Library.Biomass.Species.AuxParm<float[]> estradsensitivity;

        private Landis.Library.Biomass.Ecoregions.AuxParm<int> aet;
        private Landis.Library.Biomass.Ecoregions.AuxParm<float> whc;
        private Landis.Library.Biomass.Ecoregions.AuxParm<float> evaporationfraction;
        private Landis.Library.Biomass.Ecoregions.AuxParm<float> leakagefraction;

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
 
        public string climateFileName
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

        public Landis.Library.Biomass.Ecoregions.AuxParm<float> LeakageFraction
        {
            get
            {
                return leakagefraction;
            }
        }
        public Landis.Library.Biomass.Ecoregions.AuxParm<float> EvaporationFraction
        {
            get
            {
                return evaporationfraction;
            }
        }
        public Landis.Library.Biomass.Ecoregions.AuxParm<float> WHC
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

         

        public Landis.Library.Biomass.Species.AuxParm<float> RootTurnover
        {
            get {
                return rootturnover;
            }
        }
        public Landis.Library.Biomass.Species.AuxParm<float> WoodTurnover
        {
            get {
                return woodturnover;
            }
        }

        public Landis.Library.Biomass.Species.AuxParm<float> FolReten
        {
            get {
                return folreten;
            }
        }
        public Landis.Library.Biomass.Species.AuxParm<float> FoliageTurnover
        {
            get {
                return foliageturnover;
            }
        }
        public Landis.Library.Biomass.Species.AuxParm<float> GDDFolStart
        {
            get {
                return gddfolstart;
            }
        }
        public Landis.Library.Biomass.Species.AuxParm<float> GDDFolEnd
        {
            get {
                return gddfolend;
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
        
        public Landis.Library.Biomass.Species.AuxParm<int> SenescStart
        {
            get
            {
                return senescStart;
            }
        }
        
        public Landis.Library.Biomass.Species.AuxParm<float> FolNCon
        {
            get
            {
                return folncon;
            }
        }


        public Landis.Library.Biomass.Species.AuxParm<float> WiltingPoint
        {
            get
            {
                return wiltingpoint;
            }
        }
        
        public Landis.Library.Biomass.Species.AuxParm<float> GrowthMoistureSensitivity
        {
            get
            {
                return growthmoisturesensitivity;
            }
        }

        
        public Landis.Library.Biomass.Species.AuxParm<float> BaseFolRespFrac
        {
            get
            {
                return basefolrespfrac;
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

        public Landis.Library.Biomass.Species.AuxParm<float> RootStemRatio
        {
            get
            {
                return rootstemratio;
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
        public Landis.Library.Biomass.Species.AuxParm<float> RespQ10
        {
            get
            {
                return respq10;
            }
        }
        public Landis.Library.Biomass.Species.AuxParm<float> PsnTMin
        {
            get
            {
                return psntmin;
            }
        }

        public Landis.Library.Biomass.Species.AuxParm<float[]> EstMoistureSensitivity
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
        public Landis.Library.Biomass.Species.AuxParm<float[]> EstRadSensitivity
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
        
        public Landis.Library.Biomass.Species.AuxParm<float> WUEConst
        {
            get
            {
                return wueconst;
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

            slwdel = new Landis.Library.Biomass.Species.AuxParm<float>(PlugIn.ModelCore.Species);
            rootturnover = new Landis.Library.Biomass.Species.AuxParm<float>(PlugIn.ModelCore.Species);
            woodturnover = new Landis.Library.Biomass.Species.AuxParm<float>(PlugIn.ModelCore.Species);
            folreten = new Landis.Library.Biomass.Species.AuxParm<float>(PlugIn.ModelCore.Species);
            foliageturnover = new Landis.Library.Biomass.Species.AuxParm<float>(PlugIn.ModelCore.Species);
            gddfolend = new Landis.Library.Biomass.Species.AuxParm<float>(PlugIn.ModelCore.Species);
            gddfolstart = new Landis.Library.Biomass.Species.AuxParm<float>(PlugIn.ModelCore.Species);
            mortCurveShapeParm = new Landis.Library.Biomass.Species.AuxParm<float>(PlugIn.ModelCore.Species);
            growthCurveShapeParm = new Landis.Library.Biomass.Species.AuxParm<float>(PlugIn.ModelCore.Species);
            slwmax = new Landis.Library.Biomass.Species.AuxParm<float>(PlugIn.ModelCore.Species);
            senescStart = new Landis.Library.Biomass.Species.AuxParm<int>(PlugIn.ModelCore.Species);
            wueconst = new Landis.Library.Biomass.Species.AuxParm<float>(PlugIn.ModelCore.Species);
            respq10 = new Landis.Library.Biomass.Species.AuxParm<float>(PlugIn.ModelCore.Species);
            psntmin = new Landis.Library.Biomass.Species.AuxParm<float>(PlugIn.ModelCore.Species);
            halfsat = new Landis.Library.Biomass.Species.AuxParm<float>(PlugIn.ModelCore.Species);
            estradsensitivity = new Landis.Library.Biomass.Species.AuxParm<float[] >(PlugIn.ModelCore.Species);
            estmoisturesensitivity = new Landis.Library.Biomass.Species.AuxParm<float[]>(PlugIn.ModelCore.Species);
            psntopt = new Landis.Library.Biomass.Species.AuxParm<float>(PlugIn.ModelCore.Species);
            folncon = new Landis.Library.Biomass.Species.AuxParm<float>(PlugIn.ModelCore.Species);
            basefolrespfrac = new Landis.Library.Biomass.Species.AuxParm<float>(PlugIn.ModelCore.Species);
            growthmoisturesensitivity = new Landis.Library.Biomass.Species.AuxParm<float>(PlugIn.ModelCore.Species);
            dvpd2 = new Landis.Library.Biomass.Species.AuxParm<float>(PlugIn.ModelCore.Species);
            dvpd1 = new Landis.Library.Biomass.Species.AuxParm<float>(PlugIn.ModelCore.Species);
            amaxa = new Landis.Library.Biomass.Species.AuxParm<float>(PlugIn.ModelCore.Species);
            amaxb = new Landis.Library.Biomass.Species.AuxParm<float>(PlugIn.ModelCore.Species);
            psnagered = new Landis.Library.Biomass.Species.AuxParm<float>(PlugIn.ModelCore.Species);
            k = new Landis.Library.Biomass.Species.AuxParm<float>(PlugIn.ModelCore.Species);
            dnsc = new Landis.Library.Biomass.Species.AuxParm<float>(PlugIn.ModelCore.Species);
            maintresp = new Landis.Library.Biomass.Species.AuxParm<float>(PlugIn.ModelCore.Species);
            wiltingpoint = new Landis.Library.Biomass.Species.AuxParm<float>(PlugIn.ModelCore.Species);
            rootstemratio = new Landis.Library.Biomass.Species.AuxParm<float>(PlugIn.ModelCore.Species);
            whc = new Landis.Library.Biomass.Ecoregions.AuxParm<float>(PlugIn.ModelCore.Ecoregions);
            aet = new Landis.Library.Biomass.Ecoregions.AuxParm<int>(PlugIn.ModelCore.Ecoregions);
            evaporationfraction = new Landis.Library.Biomass.Ecoregions.AuxParm<float>(PlugIn.ModelCore.Ecoregions);
            leakagefraction = new Landis.Library.Biomass.Ecoregions.AuxParm<float>(PlugIn.ModelCore.Ecoregions);
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
