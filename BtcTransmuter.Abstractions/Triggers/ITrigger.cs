namespace BtcTransmuter.Abstractions
{
    public interface ITrigger<TData>
    {
        string Id { get; }
        string Name { get; }

        string ParametersJson { get; set; }

        TData Data { get; set; }
    }
}