using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using TestRapprochementEpisoft.Core;

namespace TestRapprochementEpisoft
{
    internal static class Program
    {

        public static int Main(string[] args)
        {
            try
            {
                if (args.Length < 3)
                {
                    Console.Error.WriteLine("Usage: ReconciliationEngine <bank.csv> <accounting.csv> <outputDir> [configPath]");
                    return 2;
                }

                string bankPath = args[0];
                string accountingPath = args[1];
                string outputDir = args[2];

                string configPath = args.Length >= 4 ? args[3] : null;
                MatchingConfig config = MatchingConfig.Load(configPath);

                if (!File.Exists(bankPath))
                {
                    Console.Error.WriteLine("Bank CSV not found: " + bankPath);
                    return 2;
                }
                if (!File.Exists(accountingPath))
                {
                    Console.Error.WriteLine("Accounting CSV not found: " + accountingPath);
                    return 2;
                }

                Directory.CreateDirectory(outputDir);

                var parser = new CsvTransactionParsing();
                IReadOnlyList<Transaction> bank = parser.Read(bankPath, TransactionSource.Bank);
                IReadOnlyList<Transaction> accounting = parser.Read(accountingPath, TransactionSource.Accounting);

                if (parser.Errors.Count > 0)
                {
                    Console.Error.WriteLine("Parsing completed with errors:");
                    foreach (var err in parser.Errors)
                        Console.Error.WriteLine(" - " + err);

                }

                var reconciler = new Rapprocher(config);
                RapprocheResult result = reconciler.Match(bank, accounting);

                string matchesPath = Path.Combine(outputDir, "matches.csv");
                string reportPath = Path.Combine(outputDir, "report.txt");

                OutputWriters.WriteMatchesCsv(matchesPath, result.Matches);
                OutputWriters.WriteReport(reportPath, bank, accounting, result, parser.Errors);

                Console.WriteLine("Done.");
                Console.WriteLine("Matches: " + matchesPath);
                Console.WriteLine("Report : " + reportPath);

                return parser.Errors.Count > 0 ? 1 : 0;
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine("Fatal error: " + ex.Message);
                Console.Error.WriteLine(ex);
                return 3;
            }
        }
    }
}
