namespace Waehrungsumrechner.Models
{
    /// <summary>
    /// Repräsentiert eine Währung mit Code (z.B. "EUR") und Name (z.B. "Euro").
    /// </summary>
    public class Currency
    {
        // ISO-Währungscode in Großbuchstaben
        public string Code { get; set; }

        // Ausgeschriebener Name der Währung
        public string Name { get; set; }
    }

    /// <summary>
    /// Optional für direkte Mapping-Nutzung (wird aktuell nicht verwendet).
    /// </summary>
    public class RatesResponse
    {
        public Dictionary<string, decimal> Rates { get; set; }
    }
}
