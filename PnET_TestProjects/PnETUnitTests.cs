using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Landis.Extension.Succession.BiomassPnET;

namespace PnET_TestProjects
{
    [TestClass]
    public class UnitTest1
    {
        [TestMethod]
        public void CompareWythersWithEarlier()
        {
            string ClimateFileName = @"C:\Users\debruijna\Desktop\PnET-Succession\EricCalibration\Oconto_weather_Temp0_Prec0_PAR0.txt";
            
            IObservedClimate observed = new ObservedClimate(ClimateFileName);

            //PnETSpeciesParameters	FolN	SLWmax	SLWDel	TOfol	AmaxA	AmaxB	HalfSat	H3	H4	PsnAgeRed	PsnTMin	PsnTOpt	k	
            //tsugcana	            1.2	    190	    0	    0.3333	5.3 	21.5	230	    114	235	5	        1	    19	    0.5	

            //PnETSpeciesParameters   WUEcnst	MaintResp	DNSC	FracBelowG	EstMoist	EstRad	FracFol	FrActWd	
            //tsugcana                6	        0.0005	    0.05	0.33	    1	        1	    0.13	0.00005	<<tsugcan

            /*
            LandisData PnETGenericParameters
            PnETGenericParameters 		Value
            BFolResp			0.1
            TOroot				0.02
            TOwood				0.01
            MaxCanopyLayers			2
            IMAX				70
            >>DNSC				0.05 <<target NSCfraction
            >>MaintResp			0.0025
            PreventEstablishment		true
            wythers				true
            */
            Landis.Core.PostFireRegeneration postfireregen = Landis.Core.PostFireRegeneration.Resprout;
            float wuecnst = 6; 
            float dnsc =0.05F;
            float cfracbiomass =0.45F;
            float kwdlit = 0.1F;
            float fracbelowg =0.33F;
            float fracfol =0.13F;
            float fractWd = 0.00005F;
            float psnagered =5;
            ushort h2 =0;
            ushort h3 =114;
            ushort h4 =235;
            float slwdel =0;
            float slwmax =190;
            float tofol =0.3333F;
            float toroot =0.02F;
            float halfsat =230F;
            float initialnsc =7;
            float k =0.5F;
            float towood =0.01F;
            float estrad =1;
            float estmoist =1;
            float follignin =0.2F;
            bool preventestablishment =false;
            float psntopt =19F;
            float q10 =2F;
            float psntmin =1F;
            float dvpd1 =0.05F;
            float dvpd2 =2F;
            float foln =1.2F;
            float amaxa =5.3F; 	
            float amaxb =21.5F;
            float maintresp =0.0005F;
            float bfolresp =0.1F;
            int Index =0;
            string name = "tsugcana";
            int maxSproutAge =0;
            int minSproutAge =0;
            int maxSeedDist =100;
            int effectiveSeedDist =30;
            float vegReprodProb = 0;
            byte fireTolerance =3;
            byte shadeTolerance =5;
            int maturity =60;
            int longevity =450;

            ISpeciesPNET species = new SpeciesPnET(Landis.Core.PostFireRegeneration.Resprout, 
             wuecnst,dnsc,cfracbiomass,kwdlit,fracbelowg,fracfol,fractWd,psnagered,h2,h3,h4,slwdel,slwmax,tofol
            ,toroot,halfsat,initialnsc,k,towood,estrad,estmoist,follignin,preventestablishment,psntopt,q10,psntmin,dvpd1
            ,dvpd2,foln,amaxa,amaxb,maintresp,bfolresp,Index,name,maxSproutAge,minSproutAge,maxSeedDist,effectiveSeedDist,vegReprodProb
            ,fireTolerance,shadeTolerance,maturity,longevity);

            DateTime date = new DateTime(1910, 1,1);
            string fn = @"C:\Users\debruijna\Desktop\FTempRespDayRefResp.txt";
            System.IO.StreamWriter sw = new System.IO.StreamWriter(fn);
            sw.WriteLine("date" + "\t" + "FTempRespDayRefRespWythers" + "\t" + "FTempRespDayRefRespNoWythers");
            while (date < new DateTime(1950, 1, 1))
            {
                IObservedClimate climate =  ObservedClimate.GetData(observed, date);

                float FTempRespDayRefRespWythers = new EcoregionPnETVariables(climate, date, true, new System.Collections.Generic.List<ISpeciesPNET>() { species })[species.Name].FTempRespDayRefResp;

                float FTempRespDayRefRespNoWythers = new EcoregionPnETVariables(climate, date, false, new System.Collections.Generic.List<ISpeciesPNET>() { species })[species.Name].FTempRespDayRefResp;


                sw.WriteLine(date + "\t" + FTempRespDayRefRespWythers + "\t" + FTempRespDayRefRespNoWythers);

                date = date.AddMonths(1);
            }
            sw.Close();
           
            
            

        }
    }
}
