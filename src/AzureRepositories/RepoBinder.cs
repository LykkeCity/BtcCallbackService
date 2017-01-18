using System;
using Autofac;
using AzureStorage.Queue;
using AzureStorage.Tables;
using AzureStorage.Tables.Templates.Index;
using Common.Log;
using Core.Repositories;
using Core.Settings;

namespace AzureRepositories
{
    public static class RepoBinder
    {
        public static void BindAzure(this ContainerBuilder ioc, BaseSettings settings, ILog log)
        {
            ioc.RegisterInstance(
                    new BitCoinTransactionsRepository(
                        new AzureTableStorage<BitCoinTransactionEntity>(settings.Db.BitCoinQueueConnectionString, "BitCoinTransactions", log)))
                .As<IBitCoinTransactionsRepository>();

            ioc.RegisterInstance(
                    new CashOperationsRepository(
                        new AzureTableStorage<CashInOutOperationEntity>(settings.Db.ClientPersonalInfoConnString, "OperationsCash", log),
                        new AzureTableStorage<AzureIndex>(settings.Db.ClientPersonalInfoConnString, "OperationsCash", log)))
                .As<ICashOperationsRepository>();

            ioc.RegisterInstance(
                    new ClientTradesRepository(
                        new AzureTableStorage<ClientTradeEntity>(settings.Db.HTradesConnString, "Trades", log),
                        new AzureTableStorage<AzureIndex>(settings.Db.HTradesConnString, "Trades", log)))
                .As<IClientTradesRepository>();

            ioc.RegisterInstance(
                    new TransferEventsRepository(
                        new AzureTableStorage<TransferEventEntity>(settings.Db.ClientPersonalInfoConnString, "Transfers", log),
                        new AzureTableStorage<AzureIndex>(settings.Db.ClientPersonalInfoConnString, "Transfers", log)))
                .As<ITransferEventsRepository>();

            ioc.RegisterInstance(
                new TransactionQueueSender(
                    new AzureQueueExt(settings.Db.BitCoinQueueConnectionString, "outdata"), log))
                    .As<ITransactionQueueSender>();

            ioc.RegisterInstance(
                    new ProcessedTransactionsRepository(
                        new AzureTableStorage<PreProcessedTransaction>(settings.Db.BitCoinQueueConnectionString,
                            "CallbackProcessed", log),
                        new AzureTableStorage<PostProcessedTransaction>(settings.Db.BitCoinQueueConnectionString,
                            "CallbackProcessed", log)))
                .As<IProcessedTransactionsRepository>();
        }
    }
}
