namespace Core.Services.Models
{
    public class CashOutContextData
    {
        public string ClientId { get; set; }
        public string AssetId { get; set; }
        public string Address { get; set; }
        public double Amount { get; set; }
        public string CashOperationId { get; set; }
        public AdditionalData AddData { get; set; }

        public static CashOutContextData Create(string clientId, string assetId, string address, double amount,
            string cashOpId, AdditionalData additionalData = null)
        {
            return new CashOutContextData
            {
                ClientId = clientId,
                AssetId = assetId,
                Amount = amount,
                Address = address,
                CashOperationId = cashOpId,
                AddData = additionalData
            };
        }

        #region Additional data

        public class AdditionalData
        {
            public SwiftData SwiftData { get; set; }
        }

        public class SwiftData
        {
            public string CashOutRequestId { get; set; }
        }

        #endregion

    }

    public class SwapContextData
    {
        public class OrderModel
        {
            public string ClientId { get; set; }
            public string OrderId { get; set; }
        }

        public class TradeModel
        {
            public string ClientId { get; set; }
            public string TradeId { get; set; }
        }

        public OrderModel MarketOrder { get; set; }
        public OrderModel ClientOrder { get; set; }
        public TradeModel[] Trades { get; set; }
    }

    public class TransferContextData
    {
        public class TransferModel
        {
            public string ClientId { get; set; }
            public string OperationId { get; set; }
            public AdditionalActions Actions { get; set; }

            public static TransferModel Create(string clientId, string operationId)
            {
                return new TransferModel
                {
                    ClientId = clientId,
                    OperationId = operationId
                };
            }
        }

        public TransferModel[] Transfers { get; set; }


        public static TransferContextData Create(params TransferModel[] transfers)
        {
            return new TransferContextData
            {
                Transfers = transfers
            };
        }

        #region Actions

        public class AdditionalActions
        {
            /// <summary>
            /// If set, then transfer complete email with conversion to LKK will be sent on successful resonse from queue
            /// </summary>
            public ConvertedOkEmailAction CashInConvertedOkEmail { get; set; }

            /// <summary>
            /// If set, then push notification will be sent when transfer detected and confirmed
            /// </summary>
            public PushNotification PushNotification { get; set; }

            /// <summary>
            /// If set, transfer complete email will be sent
            /// </summary>
            public EmailAction SendTransferEmail { get; set; }

            /// <summary>
            /// If set, then another transfer will be generated on successful resonse from queue
            /// </summary>
            public GenerateTransferAction GenerateTransferAction { get; set; }
        }

        public class ConvertedOkEmailAction
        {
            public ConvertedOkEmailAction(string assetFromId, double price, double amountFrom, double amountLkk)
            {
                AssetFromId = assetFromId;
                Price = price;
                AmountFrom = amountFrom;
                AmountLkk = amountLkk;
            }

            public string AssetFromId { get; set; }
            public double Price { get; set; }
            public double AmountFrom { get; set; }
            public double AmountLkk { get; set; }
        }

        public class EmailAction
        {
            public EmailAction(string assetId, double amount)
            {
                AssetId = assetId;
                Amount = amount;
            }

            public string AssetId { get; set; }
            public double Amount { get; set; }
        }

        public class PushNotification
        {
            public PushNotification(string assetId, double amount)
            {
                AssetId = assetId;
                Amount = amount;
            }

            /// <summary>
            /// Id of credited asset
            /// </summary>
            public string AssetId { get; set; }
            public double Amount { get; set; }
        }

        public class GenerateTransferAction
        {
            public string DestClientId { get; set; }
            public string SourceClientId { get; set; }
            public double Amount { get; set; }
            public string AssetId { get; set; }
            public double Price { get; set; }
            public double AmountFrom { get; set; }
            public string FromAssetId { get; set; }
        }

        #endregion
    }
}
