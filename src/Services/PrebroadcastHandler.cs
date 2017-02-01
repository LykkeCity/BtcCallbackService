using System.Threading.Tasks;
using Core.Repositories;
using Core.Services;
using Core.Services.Models;
using Services.Common;

namespace Services
{
    public class PreBroadcastHandler : CommandHandler, IPreBroadcastHandler
    {
        private readonly ICashOperationsRepository _cashOperationsRepository;
        private readonly IClientTradesRepository _clientTradesRepository;
        private readonly ITransferEventsRepository _transferEventsRepository;

        public PreBroadcastHandler(IBitCoinTransactionsRepository bitCoinTransactionsRepository,
            ICashOperationsRepository cashOperationsRepository,
            IClientTradesRepository clientTradesRepository,
            ITransferEventsRepository transferEventsRepository)
            : base(bitCoinTransactionsRepository)
        {
            _cashOperationsRepository = cashOperationsRepository;
            _clientTradesRepository = clientTradesRepository;
            _transferEventsRepository = transferEventsRepository;

            RegisterHandler(CommandTypes.Issue, HandleIssueAsync);
            RegisterHandler(CommandTypes.CashOut, HandleCashOutAsync);
            RegisterHandler(CommandTypes.Swap, HandleSwapAsync);
            RegisterHandler(CommandTypes.Transfer, HandleTransferAsync);
            RegisterHandler(CommandTypes.TransferAll, HandleTransferAsync);
        }

        private async Task HandleIssueAsync(IBitcoinTransaction tx, string hash)
        {
            var contextData = tx.GetContextData<IssueContextData>();

            await _cashOperationsRepository.UpdateBlockchainHashAsync(contextData.ClientId,
                contextData.CashOperationId, hash);
        }

        private async Task HandleCashOutAsync(IBitcoinTransaction tx, string hash)
        {
            var contextData = tx.GetContextData<CashOutContextData>();

            await _cashOperationsRepository.UpdateBlockchainHashAsync(contextData.ClientId,
                contextData.CashOperationId, hash);
        }

        private async Task HandleSwapAsync(IBitcoinTransaction tx, string hash)
        {
            var contextData = tx.GetContextData<SwapContextData>();
            foreach (var item in contextData.Trades)
            {
                await _clientTradesRepository.UpdateBlockChainHashAsync(item.ClientId, item.TradeId,
                    hash);
            }
        }

        private async Task HandleTransferAsync(IBitcoinTransaction tx, string hash)
        {
            var contextData = tx.GetContextData<TransferContextData>();

            foreach (var transfer in contextData.Transfers)
            {
                await _transferEventsRepository.UpdateBlockChainHashAsync(transfer.ClientId, transfer.OperationId, hash);
            }
        }
    }
}
