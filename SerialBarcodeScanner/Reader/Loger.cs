using Logger;
using Logger.Model;
using Logger.Tasks;
using MyApplication;
using SerialBarcodeScanner.Model;
using System;
using System.IO.Ports;
using System.Reflection;
using System.Threading;

namespace SerialBarcodeScanner.Reader
{
    internal class Loger
    {
        /// <summary>
        /// Loguje informace o operaci do systému logování.
        /// </summary>
        /// <param name="methodBase">Odkaz na metodu, ze které byla volána logovací funkce.</param>
        /// <param name="barcodeReader">Instance třídy zodpovědné za čtení čárových kódů.</param>
        /// <param name="bcRead">Stav čtení čárového kódu, který má být zaznamenán.</param>
        /// <param name="v">Další informace nebo popis operace pro logování.</param>
        /// <param name="cancellationToken">Token pro zrušení operace, pokud je žádáno.</param>
        public static void Log(MethodBase methodBase,IMyApp iMyApp, Enums.StavBCS bcRead, string v)
        {
            // Vytvoření objektu záznamu loggeru s potřebnými informacemi.
            LoggerInputApp zaznamLoggeru = new LoggerInputApp
            {
                MethodBase = methodBase,
                DisplayedProcessState = GetState.Get(bcRead),
                Name = v
            };

            // Zaznamenání logovacího záznamu s použitím LoggerEnvironment.
            iMyApp?.Resources?.AppViewModel?.LoggerTitle(iMyApp, zaznamLoggeru, true, true);
        }

        /// <summary>
        /// Zpracovává výjimky tím, že zaznamená chybu, aktualizuje vlastnosti týkající se chyb v datech výchozího objektu
        /// a volitelně získává informace o sériovém portu.
        /// </summary>
        /// <param name="ex">Výjimka, která má být zpracována.</param>
        /// <param name="methodBase">Metoda nebo operace, kde došlo k výjimce.</param>
        /// <param name="barcodeReader">Instance čtečky čárových kódů spojená s výjimkou.</param>
        /// <param name="_Default">Objekt dat výchozího nastavení, do něhož budou aktualizovány informace o chybě.</param>
        /// <param name="cancellationToken">Token pro zrušení operace.</param>
        /// <param name="errorState">Stav chyby, který se má přiřadit datům výchozího nastavení.</param>
        /// <param name="errorMessage">Vlastní chybová zpráva nebo popis.</param>
        /// <param name="serialPort">Sériový port spojený s výjimkou (volitelně).</param>
        internal static void HandleException(Exception ex, MethodBase methodBase, IMyApp iMyApp, BarcodeReaderData _Default, Enums.StavBCS errorState, string errorMessage, SerialPort serialPort)
        {
            Log(methodBase, iMyApp, errorState, $"{errorMessage}: {ex?.Message}\n");

            _Default.Error = ex?.Message;
            _Default.BcrProcessState = errorState;
            _Default.SerialInfo = SerialInfo(serialPort, _Default.Barcode);
        }

        /// <summary>
        /// Pomocná třída pro získání informací o sériovém portu a QR kódu.
        /// </summary>
        /// <returns>Info o serial portu.</returns>
        internal static string SerialInfo(SerialPort serialPort, string QR)
        {
            string vypis = "Nedostupné";  // Výchozí hodnota pro výpis

            try
            {
                string qrData = QR ?? "null";  // Pokud QR data nejsou dostupná, použije se "null".
                string baudRate = serialPort?.BaudRate.ToString() ?? "null";  // Získání hodnoty Baud Rate, pokud je k dispozici, jinak "null".
                string dataBits = serialPort?.DataBits.ToString() ?? "null";  // Získání hodnoty Data Bits, pokud je k dispozici, jinak "null".
                string dtrEnable = serialPort?.DtrEnable.ToString() ?? "null";  // Získání hodnoty DTR Enable, pokud je k dispozici, jinak "null".
                string encoding = serialPort?.Encoding?.ToString() ?? "null";  // Získání informace o kódování, pokud je k dispozici, jinak "null".
                string portName = serialPort?.PortName?.ToString() ?? "null";  // Získání názvu portu, pokud je k dispozici, jinak "null".
                string readTimeout = serialPort?.ReadTimeout.ToString() ?? "null";  // Získání hodnoty Read Timeout, pokud je k dispozici, jinak "null".
                string rtsEnable = serialPort?.RtsEnable.ToString() ?? "null";  // Získání hodnoty RTS Enable, pokud je k dispozici, jinak "null".
                string stopBits = serialPort?.StopBits.ToString() ?? "null";  // Získání hodnoty Stop Bits, pokud je k dispozici, jinak "null".
                string writeTimeout = serialPort?.WriteTimeout.ToString() ?? "null";  // Získání hodnoty Write Timeout, pokud je k dispozici, jinak "null".

                return $"QR Data: {qrData}\nBaudRate: {baudRate}\nDataBits: {dataBits}\nDtrEnable: {dtrEnable}\nEncoding: {encoding}\nPortName: {portName}\nReadTimeout: {readTimeout}\nRtsEnable: {rtsEnable}\nStopBits: {stopBits}\nWriteTimeout: {writeTimeout}";
            }
            catch (Exception ex)
            {
                vypis = ex?.Message;  // Pokud došlo k chybě při získávání informací, nastaví se chybová zpráva.
            }

            return vypis;  // Vrací výsledný řetězec s informacemi o sériovém portu a QR kódu.
        }
    }
}
