 
namespace Landis.Extension.Succession.BiomassPnET 
{
    /// <summary>
    /// The information for a tree species (its index and parameters).
    /// </summary>
    public interface ISpeciesPNET : Landis.Core.ISpecies 
    {
        float CFracBiomass { get; }
        float DNSC { get; }
        float FracBelowG { get; }
        float FracFol { get; }
        float FrActWd { get; }
        ushort H2 { get; }
        ushort H3 { get; }
        ushort H4 { get; }
        float InitialNSC { get; }
        float HalfSat { get; }
        float K { get; }
        float KWdLit { get; }
        float PsnAgeRed { get; }
        float SLWDel { get; }
        float SLWmax { get; }
        float TOfol { get; }
        float TOroot { get; }
        float TOwood { get; }
        float EstRad { get; }
        float EstMoist { get; }
        float FolLignin { get; }
        bool PreventEstablishment { get; }
        float PsnTOpt { get; }
        float Q10 { get; }
        float BFolResp { get; }
        
        float PsnTMin { get; }
        float DVPD1 { get; }
        float FolN { get; }
        float DVPD2 { get; }
        float WUEcnst { get; }
        
        float AmaxA { get; }
        float AmaxB { get; }
         
        float MaintResp { get; }

           
           
         
    }
}
