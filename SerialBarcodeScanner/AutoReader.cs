using SerialBarcodeScanner.Model;
using System;
using System.Reflection;
using System.Threading.Tasks;
using MyApplication;
using Logger.Model;
using netUtilities;
using SerialBarcodeScanner.Reader;
using Dialogs;
using DaikinLabel;
using System.Linq;
using System.Windows;
using DaikinLabel.Indoor;
using System.Runtime.InteropServices;
using Logger.Tasks;
using static Logger.Model.Enums;

namespace SerialBarcodeScanner
{
    public static class AutoReader
    {
        /// <summary>
        /// Asynchronně inicializuje proces čtení čárového kódu.
        /// </summary>
        /// <param name="barcodeReader">Čtečka čárového kódu.</param>
        /// <param name="myApp">Instance aplikace.</param>
        public static async Task InitializeScanningAsync(this IBarcodeReader barcodeReader, IMyApp myApp)
        {
            await Task.Run(() => InitializeScanning(barcodeReader, myApp));
        }

            /// <summary>
            /// Spustí proces čtení čárového kódu.
            /// </summary>
            public static async Task InitializeScanning(this IBarcodeReader barcodeReader, IMyApp myApp)
        {
            try
            {
                // Krok 1: Resetuje načtený čárový kód na null
                barcodeReader.ScannedLabel = null;

                // Krok 2: Čte čárový kód a ukládá ho do proměnného BCS
                BarcodeReaderData BCS = barcodeReader.WaitData(myApp);

                // Krok 3: Zavře chybovou zprávu v hlavním okně aplikace
                await myApp.Resources.AppWindow.Dispatcher.ZavritChybovouZpravu();

                // Krok 4: Vytvoří nový štítek na základě načteného kódu BCS
                if (barcodeReader.ReadMode == ReadMode.WaitToProductionLabel)
                {
                    barcodeReader.ScannedLabel = LabelFactory.ZiskejPovolenyLabel(BCS.Barcode, barcodeReader?.LabelTypes);
                }
                else
                {
                    barcodeReader.ScannedLabel = new Labell { Data = BCS.Barcode };
                }
              
                // Krok 5: Pokud nebylo zrušeno čtení (cancellation) čárového kódu, zpracuje načtený čárový kód
                if (!myApp.Resources.CancellationTokenSource.Token.IsCancellationRequested)
                {
                    await ProcessScannedBarcodeAsync(BCS, barcodeReader, myApp);
                }
            }
            catch (Exception dd)
            {
                await MethodBase.GetCurrentMethod()?.ReportInternalErrorAsync(myApp?.Resources?.AppWindow?.Dispatcher, dd, myApp);
            }
        }


        /// <summary>
        /// Zpracovává načtený čárový kód na základě stavu čtečky.
        /// </summary>
        /// <param name="barcodeReaderState">Stav načteného čárového kódu.</param>
        /// <returns>Asynchronní úkol představující zpracování čárového kódu.</returns>
        private static async Task ProcessScannedBarcodeAsync(BarcodeReaderData barcodeReaderState, IBarcodeReader barcodeReader, IMyApp myApp)
        {
            try
            {
                if (barcodeReaderState.BcrProcessState == Enums.StavBCS.BcRead)
                {
                    if (barcodeReader.ScannedLabel?.IsValid == true || barcodeReader.ReadMode != ReadMode.WaitToProductionLabel)
                    {
                        // Pokud čárový kód je platný, provede akci pro "BarcodeOk".
                        Reader.Loger.Log(MethodBase.GetCurrentMethod(), myApp, Enums.StavBCS.BarcoreReaded, $"Data BCR se shodují s referencí: {barcodeReaderState.BarcodeNoTrim}\n{barcodeReader?.ScannedLabel?.ToString()}\n");

                        await barcodeReader.HandleBarcodeOk();
                    }
                    else
                    {
                        // Pokud čárový kód není platný, provede akci pro "BarcodeError".
                        Reader.Loger.Log(MethodBase.GetCurrentMethod(), myApp, Enums.StavBCS.BarcodeError, $"Data BCR se neshodují s referencí: {barcodeReaderState?.BarcodeNoTrim}\n{barcodeReader?.ScannedLabel?.ToString()}\n");

                        await barcodeReader.HandleBarcodeError();
                    }
                }
                else
                {
                    // Pokud není stav načteného čárového kódu "BcRead", provede akci pro "BarcodeCancel".
                    Reader.Loger.Log(MethodBase.GetCurrentMethod(), myApp, Enums.StavBCS.BcrError, $"BCR je v chybovám stavu: \nEcxeption: {barcodeReaderState?.Error}\nSerialInfo: {barcodeReaderState?.SerialInfo}\nBCR data: {barcodeReaderState?.BarcodeNoTrim}\n");

                    await barcodeReader.HandleReaderError(barcodeReaderState);
                }

                await barcodeReader.PostTask();
            }
            catch (Exception dd)
            {
                await MethodBase.GetCurrentMethod()?.ReportInternalErrorAsync(myApp?.Resources?.AppWindow?.Dispatcher, dd, myApp);
            }
        }
    }
}
