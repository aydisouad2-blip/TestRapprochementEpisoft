using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TestRapprochementEpisoft.Core
{
    public sealed class Rapprocher
    {
        private readonly MatchingConfig _config;

        public Rapprocher(MatchingConfig config)
        {
            _config = config ?? new MatchingConfig();
        }

        public RapprocheResult Match(IReadOnlyList<Transaction> bank, IReadOnlyList<Transaction> accounting)
        {
            bank = bank ?? Array.Empty<Transaction>();
            accounting = accounting ?? Array.Empty<Transaction>();

            // Determinism: always iterate in stable ID order
            var bankOrdered = bank.OrderBy(t => t.Id).ToList();
            var accountingOrdered = accounting.OrderBy(t => t.Id).ToList();

            var used = new HashSet<int>();
            var matches = new List<LigneMatch>(bankOrdered.Count);
            var ambiguities = new List<Ambiguity>();

            foreach (var b in bankOrdered)
            {
                var candidates = new List<candidat>();

                foreach (var a in accountingOrdered)
                {
                    if (used.Contains(a.Id))
                        continue;

                    if (TryEvaluateRules(b, a, out var score, out var rule, out var dateDiffDays, out var amountDiff))
                    {
                        candidates.Add(new candidat(a.Id, score, rule, dateDiffDays, amountDiff));
                    }
                }

                if (candidates.Count == 0)
                    continue;

                candidates.Sort(CandidatComparer.Instance);
                candidat best = candidates[0];

                // Ambigu si plusieurs candidats sont encore à égalité après comparaison du score / de l'écart de date / de l'écart de montant
                var tied = candidates
                    .Where(c => c.Score == best.Score && c.DateDiffDays == best.DateDiffDays && c.AmountDiff == best.AmountDiff)
                    .OrderBy(c => c.AccountingId)
                    .ToList();

                bool isAmbiguous = tied.Count > 1;
                if (isAmbiguous)
                {
                    ambiguities.Add(new Ambiguity(b.Id, tied.Select(t => t.AccountingId).ToList()));
                }

                used.Add(best.AccountingId);
                matches.Add(new LigneMatch(b.Id, best.AccountingId, best.Score, best.RuleApplied, isAmbiguous));
            }

            return new RapprocheResult(matches, ambiguities, used);
        }

        private bool TryEvaluateRules(Transaction b, Transaction a, out int score, out string ruleApplied, out int dateDiffDays, out decimal amountDiff)
        {
            dateDiffDays = Math.Abs((b.Date - a.Date).Days);
            amountDiff = Math.Abs(b.Amount - a.Amount);

            // Rule 1 — exact
            if (dateDiffDays == 0 && amountDiff == 0m)
            {
                score = 100;
                ruleApplied = "R1_EXACT";
                return true;
            }

            // Rule 2 — meme amount, date tolerance
            if (amountDiff == 0m && dateDiffDays <= _config.Rule2DateToleranceDays)
            {
                score = 85;
                ruleApplied = "R2_AMOUNT_DATE_1D";
                return true;
            }

            // Rule 3 — meme date, amount tolerance
            if (dateDiffDays == 0 && amountDiff <= _config.Rule3AmountTolerance)
            {
                score = 70;
                ruleApplied = "R3_DATE_AMOUNT_5";
                return true;
            }

            // Rule 4 — date + amount tolerance
            if (dateDiffDays <= _config.Rule4DateToleranceDays && amountDiff <= _config.Rule4AmountTolerance)
            {
                score = 55;
                ruleApplied = "R4_DATE_2D_AMOUNT_5";
                return true;
            }

            score = 0;
            ruleApplied = string.Empty;
            return false;
        }

        private sealed class candidat
        {
            public candidat(int accountingId, int score, string ruleApplied, int dateDiffDays, decimal amountDiff)
            {
                AccountingId = accountingId;
                Score = score;
                RuleApplied = ruleApplied;
                DateDiffDays = dateDiffDays;
                AmountDiff = amountDiff;
            }

            public int AccountingId { get; }
            public int Score { get; }
            public string RuleApplied { get; }
            public int DateDiffDays { get; }
            public decimal AmountDiff { get; }
        }

        private sealed class CandidatComparer : IComparer<candidat>
        {
            public static CandidatComparer Instance { get; } = new CandidatComparer();

            public int Compare(candidat x, candidat y)
            {
                if (ReferenceEquals(x, y)) return 0;
                if (x is null) return 1;
                if (y is null) return -1;

                // Higher score first
                int c = y.Score.CompareTo(x.Score);
                if (c != 0) return c;

                // Smaller date diff first
                c = x.DateDiffDays.CompareTo(y.DateDiffDays);
                if (c != 0) return c;

                // Smaller amount diff first
                c = x.AmountDiff.CompareTo(y.AmountDiff);
                if (c != 0) return c;

                // Stable order: smallest AccountingId first
                return x.AccountingId.CompareTo(y.AccountingId);
            }
        }
    }
}