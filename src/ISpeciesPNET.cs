 
namespace Landis.Extension.Succession.BiomassPnET 
{
    /// <summary>
    /// The information for a tree species (its index and parameters).
    /// </summary>
    public interface ISpeciesPNET : Landis.Core.ISpecies
    {
        // Carbon fraction in biomass 
        float CFracBiomass { get; }

        // Fraction of non-soluble carbon to active biomass
        float DNSC { get; }

        // Fraction biomass below-ground
        float FracBelowG { get; }

        // Fraction foliage to active biomass
        float FracFol { get; }

        // Fraction active biomass to total biomass 
        float FrActWd { get; }

        // Water stress parameter for excess water
        float H2 { get; }

        // Water stress parameter for water shortage: pressurehead above which growth declines
        float H3 { get; }

        // Water stress parameter: pressurehead above growth halts (= wilting point)
        float H4 { get; }

        // Initial NSC for new cohort
        float InitialNSC { get; }

        // Half saturation value for radiation (W/m2)
        float HalfSat { get; }

        // Radiation extinction rate through the canopy (LAI-1)
        float K { get; }

        // Decompostiaion constant of woody litter (yr-1)
        float KWdLit { get; }

        //Growth reduction parameter with age
        float PsnAgeRed { get; }

        // Reduction of specific leaf weight throught the canopy (g/m2/g)
        float SLWDel { get; }

        // Max specific leaf weight (g/m2)
        float SLWmax { get; }

        // Foliage turnover (g/g/y)
        float TOfol { get; }

        // Root turnover (g/g/y)
        float TOroot { get; }

        // Wood turnover (g/g/y)
        float TOwood { get; }

        // Establishemnt reduction factor as it depends on suboptimal radiation
        float EstRad { get; }

        // Establishemnt reduction factor as it depends on sub- or supraoptimal moisture
        float EstMoist { get; }

        // Lignin concentration in foliage
        float FolLignin { get; }

        // Prevent establishment 
        bool PreventEstablishment { get; }

        // Optimal temperature for photosynthesis
        float PsnTOpt { get; }

        // Temperature response factor for respiration
        float Q10 { get; }

        // Base foliar respiration (g respired / g photosynthesis)
        float BFolResp { get; }

        //Minimum temperatyre for photosynthesis
        float PsnTMin { get; }

        // Foliar nitrogen (gN/gC)
        float FolN { get; }

        // Vapor pressure deficit response parameter 
        float DVPD1 { get; }

        // Vapor pressure deficit response parameter 
        float DVPD2 { get; }

        // Water use efficiency constant
        float WUEcnst { get; }

        // Reference photosynthesis (g)
        float AmaxA { get; }

        // Response parameter for photosynthesis to N
        float AmaxB { get; }
        
        // Referece maintenance respiration 
        float MaintResp { get; }
         
        // Cold tolerance
        float ColdTol { get; }
    }
}
