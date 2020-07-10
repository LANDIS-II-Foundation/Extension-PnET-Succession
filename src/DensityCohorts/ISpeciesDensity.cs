 
namespace Landis.Library.DensityCohorts 
{
    /// <summary>
    /// The information for a tree species (its index and parameters).
    /// </summary>
    public interface ISpeciesDensity : Landis.Core.ISpecies
    {

        
        //FIXME(JSF) - Add any species parameters not defined in the standard species.txt input file

        // Species class for default diameter growth models
        int SpType { get; }

        // Species class for aboveground biomass estimation coefficients
        int BiomassClass { get; }

        // Maximum species diameter (cm). Only used if default diameter growth models are used.
        float MaxDia { get; }

        // Maximum stand density index (SDI) for species
        int MaxSDI { get; }

        // Total seeds produced by a individual tree
        int TotalSeed { get; }

        // Coefficient used to estimate carbon content within biomass
        float CarbonCoef { get; }
        
    }
}
