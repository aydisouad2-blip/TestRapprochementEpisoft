using System;
using System.Collections.Generic;

namespace TestRapprochementEpisoft.Core
{
    public sealed class LigneMatch
    {
        public LigneMatch(int bankId, int accountingId, int score, string ruleApplied, bool isAmbiguous)
        {
            BankId = bankId;
            AccountingId = accountingId;
            Score = score;
            RuleApplied = ruleApplied ?? string.Empty;
            IsAmbiguous = isAmbiguous;
        }

        public int BankId { get; }
        public int AccountingId { get; }
        public int Score { get; }
        public string RuleApplied { get; }
        public bool IsAmbiguous { get; }
    }

    public sealed class Ambiguity
    {
        public Ambiguity(int bankId, IReadOnlyList<int> candidateAccountingIds)
        {
            BankId = bankId;
            CandidateAccountingIds = candidateAccountingIds;
        }

        public int BankId { get; }
        public IReadOnlyList<int> CandidateAccountingIds { get; }
    }

    public sealed class RapprocheResult
    {
        public RapprocheResult(IReadOnlyList<LigneMatch> matches, IReadOnlyList<Ambiguity> ambiguities, IReadOnlyCollection<int> usedAccountingIds)
        {
            Matches = matches;
            Ambiguities = ambiguities;
            UsedAccountingIds = usedAccountingIds;
        }

        public IReadOnlyList<LigneMatch> Matches { get; }
        public IReadOnlyList<Ambiguity> Ambiguities { get; }
        public IReadOnlyCollection<int> UsedAccountingIds { get; }
    }
}
