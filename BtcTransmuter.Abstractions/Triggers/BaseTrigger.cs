using BtcTransmuter.Data.Models;

namespace BtcTransmuter.Abstractions.Triggers
{
    public abstract class BaseTrigger<TData> : ITrigger
    {
        public virtual string Id => GetType().FullName;
        public string DataJson { get; set; }

        public virtual TData Data
        {
            get => this.Get<TData>();
            set => this.Set(value);
        }
    }
}