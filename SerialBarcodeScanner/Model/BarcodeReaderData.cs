using Logger.Model;

namespace SerialBarcodeScanner.Model
{
    /// <summary>
    /// Reprezentuje data spojená s čtečkou čárových kódů a jejich stav.
    /// </summary>
    public class BarcodeReaderData
    {
        /// <summary>
        /// Získá nebo nastaví chybovou zprávu.
        /// </summary>
        public string Error { get; set; }

        /// <summary>
        /// Získá nebo nastaví načtený čárový kód.
        /// </summary>
        public string Barcode { get; set; }

        /// <summary>
        /// Získá nebo nastaví načtený čárový kód bez ořezání mezer.
        /// </summary>
        public string BarcodeNoTrim { get; set; }

        /// <summary>
        /// Získá nebo nastaví informace o sériovém portu.
        /// </summary>
        public string SerialInfo { get; set; }

        /// <summary>
        /// Získá nebo nastaví stav zpracování čárového kódu.
        /// </summary>
        public Enums.StavBCS BcrProcessState { get; set; }
    }
}
