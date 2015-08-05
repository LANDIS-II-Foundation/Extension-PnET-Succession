using Landis.Core;
using System;
namespace Landis.Extension.Succession.BiomassPnET
{
    public interface ICohort
    {
        ISpecies Species { get; }
        int YearOfBirth { get; }
        ushort Age  { get; }
        int  CanopyLayer { get; }
        float AboveCanopyRadiation { get; set; }
        float[] CumulativeLAI(int IMAX, float SLWmax, float SLWDel, float Fol);
        float MaintenanceRespiration{ get; }
            
        float ReleasedNSC { get; }                    
        float Folalloc    { get; }  
        float WoodAlloc    { get; }   
        float RootAlloc    { get; }   
        float WUE    { get; }
        float Transpiration { get; set; }   
        int MaxBiomass    { get; }   
        float Fol    { get; }   
        float Root    { get; }
        
        float Wood    { get; }   
        
        float Fage    { get; }   
        
        float FRad0 { get; set; }
        float FWater0 { get; set; }
        float LAI { get; set; }

        float NSC { get; set; }
        float NetPsn { get; set; }
        float GrossPsn { get; set; }
        float FolResp { get; set; }

        void CalculateReleasedNSC(float IMAX, float FRad0);
        void AllocateNSC(float fRad);
        float CalculateRadiationStress(float Radiation);
        
       
        void SubtractMaintResp(DateTime date);
         float SubtractRootSenescence();
         float FoliageSenescence(DateTime date);
         float SubtractWoodSenescence();
         void CalculateWaterUseEfficiency();
      
    }
}
