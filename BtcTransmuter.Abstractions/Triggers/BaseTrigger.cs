using BtcTransmuter.Data.Models;
using Newtonsoft.Json.Linq;

namespace BtcTransmuter.Abstractions
{
    public abstract class BaseTrigger<TData> : ITrigger
    {
        public virtual string Id => GetType().FullName;
        public abstract string Name { get; }
        public string DataJson { get; set; }

        public virtual TData Data
        {
            get => this.Get<TData>();
            set => this.Set<TData>();
        }
    }
}