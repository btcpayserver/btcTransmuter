namespace BtcTransmuter.Abstractions.Models
{
    public class OrderBy<T>
    {
        public T Field { get; set; }
        public OrderDirection Direction { get; set; }
    }
}