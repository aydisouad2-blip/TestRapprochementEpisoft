using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TestRapprochementEpisoft.Core
{
    public enum TransactionSource
    {
        Bank = 0,
        Accounting = 1
    }

    public sealed class Transaction
    {
        public Transaction(int id, DateTime date, string description, decimal amount, TransactionSource source)
        {
            Id = id;
            Date = date;
            Description = description ?? string.Empty;
            Amount = amount;
            Source = source;
        }

        public int Id { get; }
        public DateTime Date { get; }
        public string Description { get; }
        public decimal Amount { get; }
        public TransactionSource Source { get; }

        public override string ToString()
            => $"{Source}#{Id} {Date:yyyy-MM-dd} {Amount.ToString(System.Globalization.CultureInfo.InvariantCulture)} \"{Description}\"";
    }
}