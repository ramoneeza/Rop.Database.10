using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Rop.Database;
internal static class ConnStringExtractor
{
    private static readonly Regex s_rx = new(
        @"\b(?<key>data\s*source|initial\s*catalog)\s*=\s*(?:""(?<val>[^""]*)""|'(?<val>[^']*)'|(?<val>[^;]*))",
        RegexOptions.Compiled | RegexOptions.IgnoreCase | RegexOptions.CultureInvariant | RegexOptions.ExplicitCapture);

    /// <summary>
    /// Extrae DataSource e InitialCatalog como tupla. Devuelve null en cada elemento si no se encuentra.
    /// </summary>
    public static (string? DataSource, string? InitialCatalog) Extract(string? conn)
    {
        if (string.IsNullOrEmpty(conn)) return (null, null);

        string? dataSource = null;
        string? initialCatalog = null;

        foreach (Match m in s_rx.Matches(conn))
        {
            var keyRaw = m.Groups["key"].Value;
            var value = m.Groups["val"].Value.Trim();
            if (keyRaw.IndexOf("data", StringComparison.OrdinalIgnoreCase) >= 0)
            {
                dataSource = value; // conservar la primera coincidencia
            }
            if (keyRaw.IndexOf("initial", StringComparison.OrdinalIgnoreCase) >= 0)
            {
                initialCatalog = value; // conservar la primera coincidencia
            }
            if (dataSource is not null && initialCatalog is not null) break; // ya encontramos ambos
        }
        return (dataSource, initialCatalog);
    }
}
