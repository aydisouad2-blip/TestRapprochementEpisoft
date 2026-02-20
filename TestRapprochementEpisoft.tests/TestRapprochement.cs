using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.IO;
using System.Linq;
using TestRapprochementEpisoft.Core;

namespace TestRapprochementEpisoft.tests
{
    [TestClass]
    public class ParsingTests
    {
        [TestMethod]
        public void Parse_ValidFile_ReadsAllTransactions()
        {
            string csv =
@"Date,Description,Amount
2023-10-01,Hello,-10.50
2023-10-02,""Quoted, desc"",42.00
";
            string path = WriteTemp(csv);

            var reader = new CsvTransactionParsing();
            var txs = reader.Read(path, TransactionSource.Bank);

            Assert.AreEqual(0, reader.Errors.Count);
            Assert.AreEqual(2, txs.Count);
            Assert.AreEqual(new DateTime(2023, 10, 1), txs[0].Date);
            Assert.AreEqual(-10.50m, txs[0].Amount);
            Assert.AreEqual("Quoted, desc", txs[1].Description);
        }

        [TestMethod]
        public void Parse_InvalidLines_AreSkippedAndReported()
        {
            string csv =
@"Date,Description,Amount
2023-10-01,Ok,10.00
NOTADATE,Bad,10.00
2023-10-03,MissingAmount
2023-10-04,BadAmount,ABC
";
            string path = WriteTemp(csv);

            var reader = new CsvTransactionParsing();
            var txs = reader.Read(path, TransactionSource.Bank);

            Assert.AreEqual(1, txs.Count); // seule la première ligne est valide
            Assert.IsTrue(reader.Errors.Count >= 3);
        }

        private static string WriteTemp(string content)
        {
            string path = Path.Combine(Path.GetTempPath(), "reco_test_" + Guid.NewGuid().ToString("N") + ".csv");
            File.WriteAllText(path, content);
            return path;
        }
    }

    [TestClass]
    public class MatchingTests
    {
        [TestMethod]
        public void Match_Rule1_Exact()
        {
            var cfg = new MatchingConfig();
            var rec = new Rapprocher(cfg);

            var bank = new[]
            {
                new Transaction(1, new DateTime(2023,10,1), "B", -10.00m, TransactionSource.Bank)
            };
            var acc = new[]
            {
                new Transaction(1, new DateTime(2023,10,1), "A", -10.00m, TransactionSource.Accounting)
            };

            var result = rec.Match(bank, acc);

            Assert.AreEqual(1, result.Matches.Count);
            Assert.AreEqual(100, result.Matches[0].Score);
            Assert.AreEqual("R1_EXACT", result.Matches[0].RuleApplied);
        }

        [TestMethod]
        public void Match_Rule2_SameAmount_DatePlusMinus1()
        {
            var cfg = new MatchingConfig();
            var rec = new Rapprocher(cfg);

            var bank = new[]
            {
                new Transaction(1, new DateTime(2023,10,2), "B", -10.00m, TransactionSource.Bank)
            };
            var acc = new[]
            {
                new Transaction(1, new DateTime(2023,10,1), "A", -10.00m, TransactionSource.Accounting)
            };

            var result = rec.Match(bank, acc);

            Assert.AreEqual(1, result.Matches.Count);
            Assert.AreEqual(85, result.Matches[0].Score);
            Assert.AreEqual("R2_AMOUNT_DATE_1D", result.Matches[0].RuleApplied);
        }

        [TestMethod]
        public void Match_Ambiguous_TwoCandidatesSameScore_StablePickAndFlag()
        {
            var cfg = new MatchingConfig();
            var rec = new Rapprocher(cfg);

            var bank = new[]
            {
                new Transaction(10, new DateTime(2023,10,2), "B", -10.00m, TransactionSource.Bank)
            };
            var acc = new[]
            {
                // Les deux correspondent à la Règle 2 avec le même score / écart de date / écart de montant → cas ambigu
                new Transaction(1, new DateTime(2023,10,1), "A1", -10.00m, TransactionSource.Accounting),
                new Transaction(2, new DateTime(2023,10,1), "A2", -10.00m, TransactionSource.Accounting),
            };

            var result = rec.Match(bank, acc);

            Assert.AreEqual(1, result.Matches.Count);
            Assert.AreEqual(1, result.Matches[0].AccountingId); // sélection stable (plus petit identifiant)
            Assert.IsTrue(result.Matches[0].IsAmbiguous);
            Assert.AreEqual(1, result.Ambiguities.Count);
            CollectionAssert.AreEqual(new[] { 1, 2 }, result.Ambiguities[0].CandidateAccountingIds.ToArray());
        }
    }
}