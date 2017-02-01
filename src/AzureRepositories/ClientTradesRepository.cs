using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AzureStorage;
using AzureStorage.Tables.Templates.Index;
using Common;
using Core.Repositories;
using Microsoft.WindowsAzure.Storage.Table;

namespace AzureRepositories
{

    public class ClientTradeEntity : TableEntity, IClientTrade
    {
        public string Id => RowKey;

        public DateTime DateTime { get; set; }
        public bool IsHidden { get; set; }
        public string LimitOrderId { get; set; }
        public string MarketOrderId { get; set; }
        public double Price { get; set; }
        public DateTime? DetectionTime { get; set; }
        public int Confirmations { get; set; }
        public double Amount => Volume;
        public string AssetId { get; set; }
        public string BlockChainHash { get; set; }
        public string Multisig { get; set; }
        public string TransactionId { get; set; }
        public string AddressFrom { get; set; }
        public string AddressTo { get; set; }
        public bool? IsSettled { get; set; }
        public double Volume { get; set; }
        public string ClientId { get; set; }

        public static class ByClientId
        {
            public static string GeneratePartitionKey(string clientId)
            {
                return clientId;
            }

            public static string GenerateRowKey(string tradeId)
            {
                return tradeId;
            }

            public static ClientTradeEntity Create(IClientTrade src)
            {
                var entity = CreateNew(src);
                entity.RowKey = GenerateRowKey(src.Id ?? Guid.NewGuid().ToString("N"));
                entity.PartitionKey = GeneratePartitionKey(src.ClientId);
                return entity;
            }
        }

        public static class ByMultisig
        {
            public static string GeneratePartitionKey(string multisig)
            {
                return multisig;
            }

            public static string GenerateRowKey(string tradeId)
            {
                return tradeId;
            }

            public static ClientTradeEntity Create(IClientTrade src)
            {
                var entity = CreateNew(src);
                entity.RowKey = GenerateRowKey(src.Id ?? Guid.NewGuid().ToString("N"));
                entity.PartitionKey = GeneratePartitionKey(src.Multisig);
                return entity;
            }
        }

        public static class ByDt
        {
            public static string GeneratePartitionKey()
            {
                return "dt";
            }

            public static string GenerateRowKey(string tradeId)
            {
                return tradeId;
            }

            public static string GetRowKeyPart(DateTime dt)
            {
                //ME rowkey format e.g. 20160812180446244_00130
                return $"{dt.Year}{dt.Month.ToString("00")}{dt.Day.ToString("00")}{dt.Hour.ToString("00")}{dt.Minute.ToString("00")}";
            }
        }

        public static ClientTradeEntity CreateNew(IClientTrade src)
        {
            return new ClientTradeEntity
            {
                ClientId = src.ClientId,
                AssetId = src.AssetId,
                DateTime = src.DateTime,
                LimitOrderId = src.LimitOrderId,
                MarketOrderId = src.MarketOrderId,
                Volume = src.Amount,
                BlockChainHash = src.BlockChainHash,
                Price = src.Price,
                IsHidden = src.IsHidden,
                AddressFrom = src.AddressFrom,
                AddressTo = src.AddressTo,
                Multisig = src.Multisig,
                DetectionTime = src.DetectionTime,
                Confirmations = src.Confirmations,
                IsSettled = src.IsSettled
            };
        }
    }

    public class ClientTradesRepository : IClientTradesRepository
    {
        private readonly INoSQLTableStorage<ClientTradeEntity> _tableStorage;

        public ClientTradesRepository(INoSQLTableStorage<ClientTradeEntity> tableStorage)
        {
            _tableStorage = tableStorage;
        }

        [Obsolete("Trades are created on ME side now.")]
        public async Task SaveAsync(params IClientTrade[] clientTrades)
        {
            foreach (var clientTradeBunch in clientTrades.ToPieces(10))
            {
                var clientTradeBunchArr = clientTradeBunch.ToArray();
                await
                    Task.WhenAll(
                        clientTradeBunchArr.Select(
                            clientTrade => _tableStorage.InsertOrReplaceAsync(ClientTradeEntity.ByClientId.Create(clientTrade)))
                            .Concat(clientTradeBunchArr.Select(
                            clientTrade => _tableStorage.InsertOrReplaceAsync(ClientTradeEntity.ByMultisig.Create(clientTrade))))
                       );
            }
        }

        public async Task<IEnumerable<IClientTrade>> GetAsync(string clientId)
        {
            var partitionKey = ClientTradeEntity.ByClientId.GeneratePartitionKey(clientId);
            return await _tableStorage.GetDataAsync(partitionKey);
        }

        public async Task<IEnumerable<IClientTrade>> GetAsync(DateTime @from, DateTime to)
        {
            // ToDo - Have to optimize according to the task: https://lykkex.atlassian.net/browse/LWDEV-131
            return await _tableStorage.GetDataAsync(itm => @from <= itm.DateTime && itm.DateTime < to);
        }

        public async Task<IClientTrade> GetAsync(string clientId, string recordId)
        {
            var partitionKey = ClientTradeEntity.ByClientId.GeneratePartitionKey(clientId);
            var rowKey = ClientTradeEntity.ByClientId.GenerateRowKey(recordId);

            return await _tableStorage.GetDataAsync(partitionKey, rowKey);
        }

        public async Task UpdateBlockChainHashAsync(string clientId, string recordId, string hash)
        {
            var partitionKey = ClientTradeEntity.ByClientId.GeneratePartitionKey(clientId);
            var rowKey = ClientTradeEntity.ByClientId.GenerateRowKey(recordId);

            var clientIdRecord = await _tableStorage.GetDataAsync(partitionKey, rowKey);

            var multisigPartitionKey = ClientTradeEntity.ByMultisig.GeneratePartitionKey(clientIdRecord.Multisig);
            var multisigRowKey = ClientTradeEntity.ByMultisig.GenerateRowKey(recordId);

            var dtPartitionKey = ClientTradeEntity.ByDt.GeneratePartitionKey();
            var dtRowKey = ClientTradeEntity.ByDt.GenerateRowKey(recordId);

            await _tableStorage.MergeAsync(partitionKey, rowKey, entity =>
            {
                entity.BlockChainHash = hash;
                return entity;
            });

            await _tableStorage.MergeAsync(multisigPartitionKey, multisigRowKey, entity =>
            {
                entity.BlockChainHash = hash;
                return entity;
            });

            await _tableStorage.MergeAsync(dtPartitionKey, dtRowKey, entity =>
            {
                entity.BlockChainHash = hash;
                return entity;
            });
        }

        public async Task SetDetectionTimeAndConfirmations(string clientId, string recordId, DateTime detectTime, int confirmations)
        {
            var partitionKey = ClientTradeEntity.ByClientId.GeneratePartitionKey(clientId);
            var rowKey = ClientTradeEntity.ByClientId.GenerateRowKey(recordId);

            var clientIdRecord = await _tableStorage.GetDataAsync(partitionKey, rowKey);

            var multisigPartitionKey = ClientTradeEntity.ByMultisig.GeneratePartitionKey(clientIdRecord.Multisig);
            var multisigRowKey = ClientTradeEntity.ByMultisig.GenerateRowKey(recordId);

            var dtPartitionKey = ClientTradeEntity.ByDt.GeneratePartitionKey();
            var dtRowKey = ClientTradeEntity.ByDt.GenerateRowKey(recordId);

            await _tableStorage.MergeAsync(partitionKey, rowKey, entity =>
            {
                entity.DetectionTime = detectTime;
                entity.Confirmations = confirmations;
                return entity;
            });

            await _tableStorage.MergeAsync(multisigPartitionKey, multisigRowKey, entity =>
            {
                entity.DetectionTime = detectTime;
                entity.Confirmations = confirmations;
                return entity;
            });

            await _tableStorage.MergeAsync(dtPartitionKey, dtRowKey, entity =>
            {
                entity.DetectionTime = detectTime;
                entity.Confirmations = confirmations;
                return entity;
            });
        }

        public async Task SetBtcTransactionAsync(string clientId, string recordId, string btcTransactionId)
        {
            var partitionKey = ClientTradeEntity.ByClientId.GeneratePartitionKey(clientId);
            var rowKey = ClientTradeEntity.ByClientId.GenerateRowKey(recordId);

            var clientIdRecord = await _tableStorage.GetDataAsync(partitionKey, rowKey);

            var multisigPartitionKey = ClientTradeEntity.ByMultisig.GeneratePartitionKey(clientIdRecord.Multisig);
            var multisigRowKey = ClientTradeEntity.ByMultisig.GenerateRowKey(recordId);

            var dtPartitionKey = ClientTradeEntity.ByDt.GeneratePartitionKey();
            var dtRowKey = ClientTradeEntity.ByDt.GenerateRowKey(recordId);

            await _tableStorage.MergeAsync(partitionKey, rowKey, entity =>
            {
                entity.TransactionId = btcTransactionId;
                return entity;
            });

            await _tableStorage.MergeAsync(multisigPartitionKey, multisigRowKey, entity =>
            {
                entity.TransactionId = btcTransactionId;
                return entity;
            });

            await _tableStorage.MergeAsync(dtPartitionKey, dtRowKey, entity =>
            {
                entity.TransactionId = btcTransactionId;
                return entity;
            });
        }

        public async Task SetIsSettledAsync(string clientId, string id)
        {
            var partitionKey = ClientTradeEntity.ByClientId.GeneratePartitionKey(clientId);
            var rowKey = ClientTradeEntity.ByClientId.GenerateRowKey(id);

            var clientIdRecord = await _tableStorage.GetDataAsync(partitionKey, rowKey);

            var multisigPartitionKey = ClientTradeEntity.ByMultisig.GeneratePartitionKey(clientIdRecord.Multisig);
            var multisigRowKey = ClientTradeEntity.ByMultisig.GenerateRowKey(id);

            var dtPartitionKey = ClientTradeEntity.ByDt.GeneratePartitionKey();
            var dtRowKey = ClientTradeEntity.ByDt.GenerateRowKey(id);

            await _tableStorage.MergeAsync(partitionKey, rowKey, entity =>
            {
                entity.IsSettled = true;
                return entity;
            });

            await _tableStorage.MergeAsync(multisigPartitionKey, multisigRowKey, entity =>
            {
                entity.IsSettled = true;
                return entity;
            });

            await _tableStorage.MergeAsync(dtPartitionKey, dtRowKey, entity =>
            {
                entity.IsSettled = true;
                return entity;
            });
        }

        public async Task<IEnumerable<IClientTrade>> GetByMultisigAsync(string multisig)
        {
            var partitionKey = ClientTradeEntity.ByMultisig.GeneratePartitionKey(multisig);
            return await _tableStorage.GetDataAsync(partitionKey);
        }

        public async Task<IEnumerable<IClientTrade>> GetByMultisigsAsync(string[] multisigs)
        {
            var tasks =
                multisigs.Select(x => _tableStorage.GetDataAsync(ClientTradeEntity.ByMultisig.GeneratePartitionKey(x)));

            var tradesByMultisigArr = await Task.WhenAll(tasks);

            var result = new List<IClientTrade>();

            foreach (var arr in tradesByMultisigArr)
            {
                result.AddRange(arr);
            }

            return result;
        }

        public Task ScanByDtAsync(Func<IEnumerable<IClientTrade>, Task> chunk, DateTime @from, DateTime to)
        {
            var rangeQuery = AzureStorageUtils.QueryGenerator<ClientTradeEntity>
                .RowKeyOnly.BetweenQuery(ClientTradeEntity.ByDt.GetRowKeyPart(from),
                    ClientTradeEntity.ByDt.GetRowKeyPart(to), ToIntervalOption.IncludeTo);
            return _tableStorage.ScanDataAsync(rangeQuery, chunk);
        }
    }
}