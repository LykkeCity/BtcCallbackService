using System;
using System.Collections.Generic;
using System.Text;
using Autofac;
using AzureRepositories;
using AzureStorage.Blob;
using AzureStorage.Queue;
using AzureStorage.Tables;
using AzureStorage.Tables.Templates.Index;
using Common.Log;
using Core.PerformanceMonitor;
using Core.Repositories;
using Core.Services;
using Core.Settings;
using Lykke.SettingsReader;
using Services;
using Services.PerformanceMonitor;

namespace Lykke.Bitcoin.CallbackService.Modules
{
    public class JobModule : Module
    {
        private readonly IReloadingManager<DbSettings> _dbSettingsManager;
        private readonly ILog _log;

        public JobModule(IReloadingManager<DbSettings> dbSettingsManager, ILog log)
        {
            _dbSettingsManager = dbSettingsManager;
            _log = log;
        }

        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterInstance(_log)
                .As<ILog>()
                .SingleInstance();

            BindServices(builder);
            BindAzure(builder, _log);
        }

        private void BindServices(ContainerBuilder ioc)
        {
            ioc.RegisterType<PreBroadcastHandler>().As<IPreBroadcastHandler>().SingleInstance();
            ioc.RegisterType<PostBroadcastHandler>().As<IPostBroadcastHandler>().SingleInstance();
            ioc.RegisterType<BitcoinTransactionService>().As<IBitcoinTransactionService>().SingleInstance();

            ioc.RegisterType<PerformanceMonitorFactory>().As<IPerformanceMonitorFactory>().SingleInstance();
        }

        private void BindAzure(ContainerBuilder ioc, ILog log)
        {
            ioc.RegisterInstance(
                    new BitCoinTransactionsRepository(
                        AzureTableStorage<BitCoinTransactionEntity>.Create(_dbSettingsManager.ConnectionString(x => x.BitCoinQueueConnectionString), "BitCoinTransactions", log)))
                .As<IBitCoinTransactionsRepository>();

            ioc.RegisterInstance(
                    new CashOperationsRepository(
                        AzureTableStorage<CashInOutOperationEntity>.Create(_dbSettingsManager.ConnectionString(x => x.ClientPersonalInfoConnString), "OperationsCash", log),
                        AzureTableStorage<AzureIndex>.Create(_dbSettingsManager.ConnectionString(x => x.ClientPersonalInfoConnString), "OperationsCash", log)))
                .As<ICashOperationsRepository>();

            ioc.RegisterInstance(
                    new ClientTradesRepository(
                        AzureTableStorage<ClientTradeEntity>.Create(_dbSettingsManager.ConnectionString(x => x.HTradesConnString), "Trades", log)))
                .As<IClientTradesRepository>();

            ioc.RegisterInstance(
                    new TransferEventsRepository(
                        AzureTableStorage<TransferEventEntity>.Create(_dbSettingsManager.ConnectionString(x => x.ClientPersonalInfoConnString), "Transfers", log),
                        AzureTableStorage<AzureIndex>.Create(_dbSettingsManager.ConnectionString(x => x.ClientPersonalInfoConnString), "Transfers", log)))
                .As<ITransferEventsRepository>();

            ioc.RegisterInstance(
                new TransactionQueueSender(
                    AzureQueueExt.Create(_dbSettingsManager.ConnectionString(x => x.BitCoinQueueConnectionString), "outdata"), log))
                    .As<ITransactionQueueSender>();

            ioc.RegisterInstance(
                    new HashEventQueueSender(
                        AzureQueueExt.Create(_dbSettingsManager.ConnectionString(x => x.BitCoinQueueConnectionString), "hash-events"), log))
                .As<IHashEventQueueSender>();

            ioc.RegisterInstance(
                    new ProcessedTransactionsRepository(
                        AzureTableStorage<PreProcessedTransaction>.Create(_dbSettingsManager.ConnectionString(x => x.BitCoinQueueConnectionString),
                            "CallbackProcessed", log),
                        AzureTableStorage<PostProcessedTransaction>.Create(_dbSettingsManager.ConnectionString(x => x.BitCoinQueueConnectionString),
                            "CallbackProcessed", log)))
                .As<IProcessedTransactionsRepository>();

            ioc.RegisterInstance(new InternalOperationsRepository(
                    AzureTableStorage<InternalOperationEntity>.Create(_dbSettingsManager.ConnectionString(x => x.BitCoinQueueConnectionString),
                        "InternalOperations", log)))
                .As<IInternalOperationsRepository>();

            ioc.RegisterInstance(new BitcoinTransactionContextBlobStorage(AzureBlobStorage.Create(_dbSettingsManager.ConnectionString(x => x.BitCoinQueueConnectionString))))
                .As<IBitcoinTransactionContextBlobStorage>();
        }
    }
}
