
using Landis.Core;
 
namespace Landis.Extension.Succession.BiomassPnET 
{
    /// <summary>
    /// The information for a tree species (its index and parameters).
    /// </summary>
    public class EcoregionPnET
        : EcoregionParameters, IEcoregionPNET
    {
        private Landis.Core.IEcoregion ecoregion;

        public string Description
        {
            get
            {
                return ecoregion.Description;
            }
        }
        public bool Active
        {
            get
            {
                return ecoregion.Active;
            }
        }
        public ushort MapCode
        {
            get
            {
                return ecoregion.MapCode;
            }
        }
        public int Index
        {
            get
            {
                return ecoregion.Index;
            }
        }
      
         
        public string Name 
        {
            get
            {
                return ecoregion.Name;
            }
        }
        
         
        

        //---------------------------------------------------------------------

        public EcoregionPnET(Landis.Core.IEcoregion ecoregion , IEcoregionParameters parameters)
            : base(parameters)
        {
            this.ecoregion = ecoregion;
        }
    }
}
 