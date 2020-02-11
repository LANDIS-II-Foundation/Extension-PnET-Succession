
namespace Landis.Extension.Succession.BiomassPnET
{
    // Mimicks the functionality of .Net 4 Tuple. 
    class Tuple<T1, T2>
    {
        public T1 Item1 { get; set; }
        public T2 Item2 { get; set; }

        public Tuple(T1 Item1, T2 Item2)
        {
            this.Item1 = Item1;
            this.Item2 = Item2;
           
        }

    }
}
