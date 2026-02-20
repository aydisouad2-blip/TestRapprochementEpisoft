using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;



namespace TestRapprochementEpisoft.Core
{
    public static class OutputWriters
    {
        public static void WriteMatchesCsv(string path, IReadOnlyList<LigneMatch> matches)
    {
        using (var sw = new StreamWriter(path, false, System.Text.Encoding.UTF8))
        {
            sw.WriteLine("BankId,AccountingId,Score,RuleApplied");
            foreach (var m in matches.OrderBy(x => x.BankId))
            {
                sw.WriteLine($"{m.BankId},{m.AccountingId},{m.Score},{EscapeCsv(m.RuleApplied)}");
            }
        }
    }

    public static void WriteReport(
        string path,
        IReadOnlyList<Transaction> bank,
        IReadOnlyList<Transaction> accounting,
        RapprocheResult result,
        IReadOnlyList<string> parsingErrors)
    {
        var matchedBankIds = new HashSet<int>(result.Matches.Select(m => m.BankId));
        var usedAccountingIds = new HashSet<int>(result.UsedAccountingIds);

        int weak = result.Matches.Count(m => m.Score < 85);

        var unmatchedBank = bank.Where(b => !matchedBankIds.Contains(b.Id)).Select(b => b.Id).OrderBy(x => x).ToList();
        var unmatchedAcc = accounting.Where(a => !usedAccountingIds.Contains(a.Id)).Select(a => a.Id).OrderBy(x => x).ToList();

        using (var sw = new StreamWriter(path, false, System.Text.Encoding.UTF8))
        {
            sw.WriteLine($"Nb total banque: {bank.Count}");
            sw.WriteLine($"Nb total compta: {accounting.Count}");
            sw.WriteLine($"Nb matchés: {result.Matches.Count}");
            sw.WriteLine($"Nb non matchés banque: {unmatchedBank.Count}");
            sw.WriteLine($"Nb non matchés compta: {unmatchedAcc.Count}");
            sw.WriteLine($"Nb de matchs faibles (score < 85): {weak}");
            sw.WriteLine();

            if (parsingErrors != null && parsingErrors.Count > 0)
            {
                sw.WriteLine("Erreurs de parsing:");
                foreach (var e in parsingErrors)
                    sw.WriteLine(" - " + e);
                sw.WriteLine();
            }

            sw.WriteLine("Cas ambigus (BankId -> Accounting candidates):");
            if (result.Ambiguities.Count == 0)
            {
                sw.WriteLine("(aucun)");
            }
            else
            {
                foreach (var a in result.Ambiguities.OrderBy(x => x.BankId))
                    sw.WriteLine($"- {a.BankId}: {string.Join(", ", a.CandidateAccountingIds)}");
            }

            sw.WriteLine();
            sw.WriteLine("Transactions banque non matchées:");
            sw.WriteLine(unmatchedBank.Count == 0 ? "(aucune)" : string.Join(", ", unmatchedBank));

            sw.WriteLine();
            sw.WriteLine("Transactions compta non matchées:");
            sw.WriteLine(unmatchedAcc.Count == 0 ? "(aucune)" : string.Join(", ", unmatchedAcc));
        }
    }

    private static string EscapeCsv(string s)
    {
        if (s == null) return string.Empty;
        if (s.Contains(",") || s.Contains("\"") || s.Contains("\n") || s.Contains("\r"))
            return "\"" + s.Replace("\"", "\"\"") + "\"";
        return s;
    }
}
}
