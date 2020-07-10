using Landis.Core;
using System.Collections.Generic;
using Landis.Utilities;
using System;


namespace Landis.Library.DensityCohorts
{
    public class BiomassParam { }

    public class biomassUtil
    {       
        private float[] biomassData;

        private int biomassNum;

        private float biomassThreshold;

        public biomassUtil()
        {


        }
        public float GetBiomassData(int i, int j)
        {
            if (i > biomassNum || j < 1 || j > 2)
                throw new Exception("index error at GetBiomass");

            return biomassData[(i - 1) * 2 + j - 1];
        }

        public void SetBiomassData(int i, int j, float value)
        {
            if (i > biomassNum || j < 1 || j > 2)
                throw new Exception("index error at SetBiomass");

            biomassData[(i - 1) * 2 + j - 1] = value;
        }


        public void SetBiomassNum(int num)
        {
            biomassNum = num;
            biomassData = null;
            biomassData = new float[num * 2];
        }

        public float BiomassThreshold
        {
            get { return biomassThreshold; }
            set { biomassThreshold = value; }
        }

    }

    class BiomassParamParser : Landis.TextParser<BiomassParam>
    {
        
        public override string LandisDataValue
        {
            get { return "BiomassCoefficients"; }
        }

        //read biomass coefficients from a file into matrix, (float) BioMassData(int ID,2). No Return Value
        //Read a ID first from the file, and ID is the size of BioMassData;
        //Read the two variable in to BioMassData(v1,v2)
        protected override BiomassParam Parse()
        {
            ReadLandisDataVar();

            InputVar<int> speciesnum = new InputVar<int>("Number_of_species_class");
            ReadVar(speciesnum);
            SpeciesParameters.biomass_util.SetBiomassNum(speciesnum.Value.Actual);

            InputVar<float> biomassThreshold = new InputVar<float>("minimum_DBH_for_calculating_biomass");
            ReadVar(biomassThreshold);
            SpeciesParameters.biomass_util.BiomassThreshold = biomassThreshold.Value.Actual;

            InputVar<float> float_val = new InputVar<float>("V0 or V1 value for each species");

            for (int i = 1; i <= speciesnum.Value.Actual; i++)
            {
                if (AtEndOfInput)
                    throw NewParseException("Expected a line here");

                Landis.Utilities.StringReader currentLine = new Landis.Utilities.StringReader(CurrentLine);

                ReadValue(float_val, currentLine);

                SpeciesParameters.biomass_util.SetBiomassData(i, 1, float_val.Value.Actual);


                ReadValue(float_val, currentLine);

                SpeciesParameters.biomass_util.SetBiomassData(i, 2, float_val.Value.Actual);

                //CheckNoDataAfter("the Ecoregion " + float_val + " column",
                //                 currentLine);

                GetNextLine();
            }

            return null;
        }
    }

}