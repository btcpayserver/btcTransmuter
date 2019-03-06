using BtcTransmuter.Abstractions.Triggers;
using NBXplorer;
using NBXplorer.DerivationStrategy;
using NBXplorer.JsonConverters;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace BtcTransmuter.Extension.NBXplorer.Triggers.NBXplorerNewTransaction
{
    public class NBXplorerNewTransactionTrigger : BaseTrigger<NBXplorerNewTransactionTriggerData>
    {
        public static string Id = typeof(NBXplorerNewTransactionTrigger).FullName;
        private readonly ExplorerClient _explorerClient;
        private readonly DerivationStrategyFactory _derivationStrategyFactory;

        public NBXplorerNewTransactionTrigger(ExplorerClient explorerClient,
            DerivationStrategyFactory derivationStrategyFactory)
        {
            _explorerClient = explorerClient;
            _derivationStrategyFactory = derivationStrategyFactory;
        }

        public override NBXplorerNewTransactionTriggerData Data
        {
            get => base.Data;
            set
            {
                var jsonSerializerSettings = new JsonSerializerSettings();
                NBitcoin.JsonConverters.Serializer.RegisterFrontConverters(jsonSerializerSettings,
                    _explorerClient.Network.NBitcoinNetwork);
                jsonSerializerSettings.Converters.Insert(0,
                    new CachedSerializer(_explorerClient.Network.NBitcoinNetwork));
                jsonSerializerSettings.Converters.Insert(0, new FeeRateJsonConverter());
                jsonSerializerSettings.Converters.Insert(0,
                    new TrackedSourceJsonConverter(_explorerClient.Network.NBitcoinNetwork));
                jsonSerializerSettings.Converters.Insert(0,
                    new DerivationStrategyJsonConverter(_derivationStrategyFactory));
                var settings = JsonSerializer.CreateDefault();

                var jobj = JObject.FromObject(value, settings);
                DataJson = jobj.ToString();
            }
        }
    }
}