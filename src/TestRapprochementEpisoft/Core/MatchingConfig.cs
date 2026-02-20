using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;

namespace TestRapprochementEpisoft.Core
{

    /// // Bonus : seuils configurables via un  fichier texte config.txt


    public sealed class MatchingConfig
    {
        // Defaults per statement
        public int Rule2DateToleranceDays { get; private set; } = 1;
        public decimal Rule3AmountTolerance { get; private set; } = 5.00m;
        public int Rule4DateToleranceDays { get; private set; } = 2;
        public decimal Rule4AmountTolerance { get; private set; } = 5.00m;

        public static MatchingConfig Load(string explicitPath)
        {
            var cfg = new MatchingConfig();
            string path = explicitPath;

            if (string.IsNullOrWhiteSpace(path))
            {

                string candidate = Path.Combine(Environment.CurrentDirectory, "config.txt");
                if (File.Exists(candidate))
                    path = candidate;
            }

            if (string.IsNullOrWhiteSpace(path) || !File.Exists(path))
                return cfg;

            try
            {
                foreach (var raw in File.ReadAllLines(path))
                {
                    var line = raw?.Trim();
                    if (string.IsNullOrWhiteSpace(line) || line.StartsWith("#", StringComparison.Ordinal))
                        continue;

                    int eq = line.IndexOf('=');
                    if (eq <= 0) continue;

                    string key = line.Substring(0, eq).Trim();
                    string val = line.Substring(eq + 1).Trim();

                    if (key.Equals("Rule2DateToleranceDays", StringComparison.OrdinalIgnoreCase)
                        && int.TryParse(val, NumberStyles.Integer, CultureInfo.InvariantCulture, out var d2))
                        cfg.Rule2DateToleranceDays = Math.Max(0, d2);

                    if (key.Equals("Rule3AmountTolerance", StringComparison.OrdinalIgnoreCase)
                        && decimal.TryParse(val, NumberStyles.Number, CultureInfo.InvariantCulture, out var a3))
                        cfg.Rule3AmountTolerance = Math.Abs(a3);

                    if (key.Equals("Rule4DateToleranceDays", StringComparison.OrdinalIgnoreCase)
                        && int.TryParse(val, NumberStyles.Integer, CultureInfo.InvariantCulture, out var d4))
                        cfg.Rule4DateToleranceDays = Math.Max(0, d4);

                    if (key.Equals("Rule4AmountTolerance", StringComparison.OrdinalIgnoreCase)
                        && decimal.TryParse(val, NumberStyles.Number, CultureInfo.InvariantCulture, out var a4))
                        cfg.Rule4AmountTolerance = Math.Abs(a4);
                }
            }
            catch
            {
                // // Si la configuration ne peut pas être analysée, conserver les valeurs par défauttr.
            }

            return cfg;
        }
    }
}
