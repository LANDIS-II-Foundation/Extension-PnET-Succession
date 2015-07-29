using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Landis.SpatialModeling;
using Landis.Core;
using Landis.Library.BiomassCohortsPnET;
using Edu.Wisc.Forest.Flel.Util;
using ExtensionMethods;
  
namespace Landis.Extension.Succession.BiomassPnET
{
    public static class ClimateData
    {
        private static char delim;
        private static string[] Content;
        static int YearCol, MonthCol, TMaxCol, TMinCol, CO2Col, PAR0col, Preccol;
        static int FirstYearOnFile, LastYearOnFile;
        static int ColumnCount = -1;
        static IInputParameters parameters;
        public static DateVar<CClimDay> Data;

        private static void ReadContent()
        {
            Content = System.IO.File.ReadAllLines(parameters.climateFileName);
            for (int line = 0; line < Content.Count(); line++)
            {
                Content[line] = Content[line].Replace("\t\t", "\t");
            }
        }
       
         
        private static int GetColNr(string[] Headers, string Label)
        {
            for (int h = 0; h < Headers.Count(); h++)
            {
                if (Headers[h].Contains(Label, StringComparison.OrdinalIgnoreCase))
                {
                    return h;
                }
            }
            throw new System.Exception("Cannot find header " + Label + " in " + parameters.climateFileName);
        }
        enum FirstLastYear
        { 
            First =0,
            Last=1
        }
        static int GetYearFrom4charOr9charString(string s, FirstLastYear f)
        { 
            // Two possible input formats: 
            // A range of years: 1600-1700: Take term #p
            // Just a year: 1600: Take this year
            try
            {
                if (s.Contains('-'))
                {
                    string[] terms = s.Split('-');

                    if (f == FirstLastYear.First) return int.Parse(terms[0]);
                    else if (f == FirstLastYear.Last)return int.Parse(terms[1]);
                    else throw new System.Exception("Cannot retrieve a year from the string " + s + " check file " + parameters.climateFileName);
                }
                else
                {
                    return int.Parse(s);
                }
            }
            catch
            {
                throw new System.Exception("Cannot retrieve a year from the string " + s + " check file " + parameters.climateFileName);
            }
        }
        static int lastline = 1;

         

        static string GetLine(int Year, int Month)
        {
            try
            {
                lastline++;
                string line = Content[lastline];
                int FirstYear = GetYearFrom4charOr9charString(line.Split(delim)[0], FirstLastYear.First);
                int LastYear = GetYearFrom4charOr9charString(line.Split(delim)[0], FirstLastYear.Last);
                int month = int.Parse(line.Split(delim)[MonthCol]);
                if (month == Month && Year >= FirstYear && Year <= LastYear)
                {
                    return line;
                }

                for (lastline = 1; lastline < Content.Count(); lastline++)
                {
                    line = Content[lastline];
                    FirstYear = GetYearFrom4charOr9charString(line.Split(delim)[0], FirstLastYear.First);
                    LastYear = GetYearFrom4charOr9charString(line.Split(delim)[0], FirstLastYear.Last);
                    month = int.Parse(line.Split(delim)[MonthCol]);
   
                    if (month == Month && Year >= FirstYear && Year <= LastYear)
                    {

                            return line;
                    }
                     
                }
            }
            catch(System.Exception e)
            {
                throw e;
            }
            throw new System.Exception("Cannot retrieve climate data for (year, month) " + Year + "," + Month);
        }
        private static void SetDelimitor()
        {
            if (parameters.climateFileName.Contains(".csv", StringComparison.OrdinalIgnoreCase))
            {
                delim = ',';
            }
            else if (parameters.climateFileName.Contains(".txt", StringComparison.OrdinalIgnoreCase))
            {
                delim = '\t';
            }
            else throw new System.Exception("Cannot determine delimitor in "+ parameters.climateFileName + " use extension csv for comma-delimited, or txt for tab-delimited climate input");
        }
        private static void SetFileProps()
        {
            SetDelimitor();
            SetFirstAndLastYearOnFile();

            string[] Headers = Content[0].Split(delim);
            ColumnCount = Headers.Count();
            YearCol = GetColNr(Headers, "Year");
            MonthCol = GetColNr(Headers, "Month");
            TMaxCol = GetColNr(Headers, "TMax");
            TMinCol = GetColNr(Headers, "TMin");
            CO2Col = GetColNr(Headers, "CO2");
            PAR0col = GetColNr(Headers, "PAR");
            Preccol = GetColNr(Headers, "Prec");
        }
        private static void SetFirstAndLastYearOnFile()
        {
            try
            {
                FirstYearOnFile = GetYearFrom4charOr9charString(Content[1].Split(delim)[0], FirstLastYear.First);
                LastYearOnFile = GetYearFrom4charOr9charString(Content[Content.Count() - 1].Split(delim)[YearCol], FirstLastYear.Last);
            }
            catch
            {
                throw new System.Exception("Irregular table format in " + parameters.climateFileName);
            }
        }
        public static void InitializeClimData(IInputParameters _parameters)
        {
            parameters = _parameters;

            ReadContent();
            

            SetFileProps();

            Data = new DateVar<CClimDay>("ClimateData", FirstYearOnFile, LastYearOnFile);

            for (int Year = FirstYearOnFile; Year < LastYearOnFile; Year++)
            {
                for(int Month = 1; Month <=12; Month++)
                {
                    string line = GetLine(Year, Month);
                    string[] terms = line.Split(delim);
                    if (terms.Count() != ColumnCount)
                    {
                        throw new System.Exception("Unexpected number of columns in " + parameters.climateFileName + " (year,month) "+ Year +" "+ Month);
                    }
                    DateTime CurrentDate = new DateTime(Year, Month, 15);

                    float TMax = float.Parse(terms[TMaxCol]);
                    float TMin = float.Parse(terms[TMinCol]);
                    float CO2 = float.Parse(terms[CO2Col]);
                    float PAR0 = float.Parse(terms[PAR0col]);
                    float Prec = float.Parse(terms[Preccol]);

                    if (TMax < TMin) throw new System.Exception("Recheck climate data in " + parameters.climateFileName + " Tmax < Tmin on " + CurrentDate);
                    else if (Prec < 0) throw new System.Exception("Recheck climate data in " + parameters.climateFileName + " Prec = " + Prec + "\t" + CurrentDate);

                    Data[CurrentDate] = new CClimDay(parameters.Latitude, CurrentDate, PAR0, TMin, TMax, Prec, CO2);
                }
            } 
        }
    }
}
