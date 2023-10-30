using MyApplication;
using SerialBarcodeScanner.Model;
using System;
using System.IO.Ports;
using System.Linq;
using System.Reflection;
using System.Threading;

namespace SerialBarcodeScanner.Reader
{
    public static class BcrReader
    {
        /// <summary>
        /// Čeká na data ze sériového portu s různými typy štítků.
        /// </summary>
        /// <param name="barcodeReader">Instance čtečky čárových kódů.</param>
        /// <param name="serialPort">Instance SerialPort pro čtení dat.</param>
        /// <param name="methodBase">Metoda, ve které se tento kód provádí.</param>
        /// <param name="cancellationToken">Token pro zrušení čekání.</param>
        /// <returns>Stav čtečky čárových kódů s načtenými daty.</returns>
        public static BarcodeReaderData WaitData(this IBarcodeReader barcodeReader, IMyApp iMyApp)
        {
            int readLength = CalculateReadLength(barcodeReader);
            return BcrReaderInternal.WaitDataInternal(iMyApp, barcodeReader, readLength);
        }

        /// <summary>
        /// Vypočítá očekávanou délku načítaných dat na základě konfigurace čtečky.
        /// </summary>
        /// <param name="barcodeReader">Instance čtečky čárových kódů.</param>
        /// <returns>Očekávaná délka načtených dat.</returns>
        private static int CalculateReadLength(IBarcodeReader barcodeReader)
        {
            if (barcodeReader?.LabelTypes == null || !barcodeReader.LabelTypes.Any())
            {
                // Pokud není konfigurován žádný typ štítku, očekáváme délku 1.
                return 1;
            }

            // Vytvoření seznamu délek načítaných dat z různých typů štítků.
            var labelLengths = barcodeReader?.LabelTypes?.Select(GetLabelLength);

            // Vracíme maximální délku ze všech typů štítků.
            return labelLengths.Max();
        }

        /// <summary>
        /// Získá délku štítku na základě typu štítku.
        /// </summary>
        /// <param name="labelType">Typ štítku.</param>
        /// <returns>Délka štítku.</returns>
        private static int GetLabelLength(Type labelType)
        {
            var label = (DaikinLabel.Label)Activator.CreateInstance(labelType);
            return label.Delka;
        }
    }
}
