namespace Core.Services.Models
{
    public static class CommandTypes
    {
        public const string CashIn = "CashIn";
        public const string Swap = "Swap";
        public const string CashOut = "CashOut";
        public const string Transfer = "Transfer";
        public const string Destroy = "Destroy";
        public const string TransferAll = "TransferAll";
        public const string Issue = "Issue";

        public static string GetCommandType(this string message)
        {
            if (!string.IsNullOrEmpty(message))
            {
                var i = message.IndexOf(':');

                var cmdType = message.Substring(0, i);

                return cmdType;
            }

            return string.Empty;
        }

        public static string GetData(this string message)
        {
            var i = message.IndexOf(':');
            return message.Substring(i + 1, message.Length - i - 1);
        }
    }
}
