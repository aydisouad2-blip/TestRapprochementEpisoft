using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;

namespace TestRapprochementEpisoft.Core
{
    /// /// Lecteur CSV simple avec support des champs entre guillemets.

    public sealed class CsvTransactionParsing
    {
        private readonly List<string> errors = new List<string>();

        public IReadOnlyList<string> Errors => errors;

        public IReadOnlyList<Transaction> Read(string path, TransactionSource source)
        {
            errors.Clear();
            var results = new List<Transaction>();

            string[] lines;
            try
            {
                lines = File.ReadAllLines(path);
            }
            catch (Exception ex)
            {
                errors.Add($"{path}: Impossible de lire le fichier ({ex.Message})");
                return results;
            }

            if (lines.Length == 0)
            {
                errors.Add($"{path}: fichier vide");
                return results;
            }

            var header = SplitCsvLine(lines[0]);
            if (header.Count < 3 || header[0] != "Date" || header[1] != "Description" || header[2] != "Amount")
            {
                errors.Add($"{path}: header manquant ou invalide (attendu : Date,Description,Amount)");

            }

            int id = 1;
            for (int i = 1; i < lines.Length; i++)
            {
                string raw = lines[i];
                if (string.IsNullOrWhiteSpace(raw))
                    continue;

                var cols = SplitCsvLine(raw);
                if (cols.Count < 3)
                {
                    errors.Add($"{path}:{i + 1}: Ligne invalide (colonnes manquantes): \"{raw}\"");
                    continue;
                }

                string dateStr = cols[0];
                string description = cols[1];
                string amountStr = cols[2];

                if (!DateTime.TryParseExact(dateStr, "yyyy-MM-dd", CultureInfo.InvariantCulture,
                        DateTimeStyles.AssumeLocal, out var date))
                {
                    errors.Add($"{path}:{i + 1}: date invalide: \"{dateStr}\"");
                    continue;
                }

                if (!decimal.TryParse(amountStr, NumberStyles.Number, CultureInfo.InvariantCulture, out var amount))
                {
                    errors.Add($"{path}:{i + 1}: amount invalide: \"{amountStr}\"");
                    continue;
                }

                results.Add(new Transaction(id++, date.Date, description, amount, source));
            }

            return results;
        }

        internal static List<string> SplitCsvLine(string line)
        {
            var fields = new List<string>(3);
            if (line == null) return fields;

            int i = 0;
            while (i < line.Length)
            {

                if (line[i] == ',')
                {
                    fields.Add(string.Empty);
                    i++;
                    continue;
                }

                if (line[i] == '"')
                {
                    i++;
                    var start = i;
                    var sb = new System.Text.StringBuilder();

                    while (i < line.Length)
                    {
                        if (line[i] == '"')
                        {

                            if (i + 1 < line.Length && line[i + 1] == '"')
                            {
                                sb.Append(line, start, i - start);
                                sb.Append('"');
                                i += 2;
                                start = i;
                                continue;
                            }


                            sb.Append(line, start, i - start);
                            i++;
                            break;
                        }

                        i++;
                    }


                    while (i < line.Length && line[i] != ',')
                        i++;

                    if (i < line.Length && line[i] == ',')
                        i++;

                    fields.Add(sb.ToString());
                }
                else
                {
                    int start = i;
                    while (i < line.Length && line[i] != ',')
                        i++;

                    string value = line.Substring(start, i - start).Trim();
                    if (i < line.Length && line[i] == ',')
                        i++;

                    fields.Add(value);
                }
            }

            if (line.EndsWith(",", StringComparison.Ordinal))
                fields.Add(string.Empty);

            return fields;
        }
    }
}