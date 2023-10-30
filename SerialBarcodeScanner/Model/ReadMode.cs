namespace SerialBarcodeScanner.Model
{
    /// <summary>
    /// Výčtový typ reprezentující různé režimy čtení dat.
    /// </summary>
    public enum ReadMode
    {
        /// <summary>
        /// Režim čtení, kdy se čte do oddělovače.
        /// </summary>
        WaitToDelimiter,

        /// <summary>
        /// Režim čtení, kdy se čte do zrušení čtení.
        /// </summary>
        WaitToCancelationToken,

        /// <summary>
        /// Režim čtení, kdy se čte do získání produkčního štítku.
        /// </summary>
        WaitToProductionLabel,

        /// <summary>
        /// Režim čtení, kdy se čte po dobu 200 ms.
        /// </summary>
        WaitForInterval,
    }
}
