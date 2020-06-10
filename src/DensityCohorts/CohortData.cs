namespace Landis.Library.DensityCohorts
{
    /// <summary>
    /// Data for an individual cohort that is not shared with other cohorts.
    /// </summary>
    public struct CohortData
    {
        /// <summary>
        /// The cohort's age (years).
        /// </summary>
        public ushort Age;

        //---------------------------------------------------------------------

        /// <summary>
        /// The cohort's biomass (g/m2).
        /// </summary>
        public int Biomass;

        //---------------------------------------------------------------------
        /// <summary>
        /// The cohort's diameter (cm).
        /// </summary>
        public float Diameter;
        
        //---------------------------------------------------------------------

        /// <summary>
        /// The number of individual trees in the cohort.
        /// </summary>
        public int Treenumber;

        //---------------------------------------------------------------------


        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        /// <param name="age">
        /// The cohort's age.
        /// </param>
        /// <param name="treenumber">
        /// The number of trees in the cohort.
        /// </param>
        public CohortData(ushort age,
                          int treenumber)
        {
            this.Age = age;
            this.Treenumber = treenumber;
            this.Biomass = 0;
            this.Diameter = 0;
        }
    }
}
