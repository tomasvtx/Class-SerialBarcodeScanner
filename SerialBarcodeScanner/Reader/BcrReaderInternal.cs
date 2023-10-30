using AppConfigure;
using Logger;
using Logger.Model;
using MyApplication;
using SerialBarcodeScanner.Model;
using System;
using System.IO.Ports;
using System.Reflection;
using System.Security.Cryptography;
using System.Text.RegularExpressions;
using System.Threading;
using static AppConfigure.BaseModel;
using static AppConfigure.BaseModelProgr;

namespace SerialBarcodeScanner.Reader
{
    internal class BcrReaderInternal : ReadVariant
    {
        /// <summary>
        /// Výchozí instance třídy BarcodeReaderData s inicializovanými hodnotami.
        /// </summary>
        private static BarcodeReaderData Default = new BarcodeReaderData
        {
            Error = string.Empty,
            SerialInfo = string.Empty,
            Barcode = string.Empty,
            BarcodeNoTrim = string.Empty,
            BcrProcessState = Enums.StavBCS.InitBcs
        };

        /// <summary>
        /// Interní metoda pro čekání na data z portu s různými režimy čtení.
        /// </summary>
        /// <param name="serialPort">Instance SerialPort pro čtení dat.</param>
        /// <param name="Port">Název portu.</param>
        /// <param name="readMode">Režim čtení (WaitToProductionLabel, WaitToDelimiter nebo WaitToCancelationToken).</param>
        /// <param name="ReadLength">Očekávaná délka načtených dat.</param>
        /// <param name="cancellationToken">Token pro zrušení čekání.</param>
        /// <returns>Stav čtečky čárových kódů s načtenými daty.</returns>
        internal static BarcodeReaderData WaitDataInternal(IMyApp iMyApp, IBarcodeReader barcodeReader, int ReadLength)
        {
            GetSerialPortConf(iMyApp, barcodeReader);

            Thread.Sleep(TimeSpan.FromMilliseconds(barcodeReader?.SerialPortConf?.SerialPortConf?.BcsDelay ?? 1000));
            BarcodeReaderData _Default = Default;

            try
            {
                // Inicializace portu, pokud je null.
                if (barcodeReader?.SerialPortConf?.SerialPort == null)
                {
                    barcodeReader.SerialPortConf.SerialPort = new SerialPort
                    {
                        PortName = barcodeReader?.SerialPortConf?.SerialPortConf?.PortName ?? "COM1",
                        BaudRate = 9600,
                        Parity = Parity.None,
                        StopBits = StopBits.One,
                        DataBits = 8,
                        Handshake = Handshake.None
                    };
                }

                try
                {
                    if (barcodeReader?.SerialPortConf?.SerialPort?.IsOpen == false)
                    {
                        barcodeReader?.SerialPortConf?.SerialPort?.Open();
                    }
                    barcodeReader?.SerialPortConf?.SerialPort?.DiscardInBuffer();


                    // Zaznamenává do logu informaci o čekání na data v sériovém portu.
                    Reader.Loger.Log(MethodBase.GetCurrentMethod(), iMyApp, Enums.StavBCS.BcRead, $"{barcodeReader?.SerialPortConf?.SerialPortConf} - Čekám na data\n");

                    // Čekání na data.
                    WaitForData(iMyApp, barcodeReader);

                    // Volba režimu čtení.
                    bool Readed = false;
                    string Read = string.Empty;

                    switch (barcodeReader?.ReadMode)
                    {
                        case ReadMode.WaitToProductionLabel:
                            // Čte data dokud nenarazí na výrobní etiketu.
                            Readed = ReadVariant.ReadData(barcodeReader, ReadLength, out Read);
                            break;

                        case ReadMode.WaitToDelimiter:
                            // Čte data dokud nenarazí na oddělovač.
                            Readed = ReadUntilDelimiter(barcodeReader, out Read);
                            break;

                        case ReadMode.WaitToCancelationToken:
                            // Čte data dokud nepřijde žádost o zrušení operace.
                            Readed = ReadUntilCancellation(barcodeReader, out Read);
                            break;

                        case ReadMode.WaitForInterval:
                            // Čte data po dobu 200 ms.
                            Readed = ReadForTime(barcodeReader, out Read);
                            break;

                        default:
                            break;
                    }

                    // Nastavuje hodnoty výchozího objektu _Default na základě čtených dat.
                    _Default.BarcodeNoTrim = Read;
                    _Default.Barcode = Read?.Trim();

                    // Získává informace o sériovém portu a přiřazuje je _Default.SerialInfo (volitelně).
                    _Default.SerialInfo = Loger.SerialInfo(barcodeReader?.SerialPortConf?.SerialPort, _Default?.Barcode);

                    // Pokud čtení selže, vyvolá výjimku s chybovou zprávou.
                    if (!Readed)
                    {
                        throw new Exception(Read);
                    }

                    // Nastavuje stav procesu čtení na "Data BCR načtena".
                    _Default.BcrProcessState = Enums.StavBCS.BcRead;

                    // Zaznamenává informaci o úspěšném načtení dat BCR do logu.
                    Reader.Loger.Log(MethodBase.GetCurrentMethod(), iMyApp, Enums.StavBCS.BarcoreReaded, $"Data BCR načtena: {_Default?.BarcodeNoTrim}\n");

                    // Vrací upravený objekt _Default.
                    return _Default;

                }
                catch (Exception ex)
                {
                    Loger.HandleException(ex, MethodBase.GetCurrentMethod(), iMyApp, _Default, Enums.StavBCS.BarcodeError, "Data BCR problem", barcodeReader?.SerialPortConf?.SerialPort);
                    return _Default;
                }
            }
            catch (Exception ex)
            {
                Loger.HandleException(ex, MethodBase.GetCurrentMethod(), iMyApp, _Default, Enums.StavBCS.BcrError, "BarcodeReader problem", barcodeReader?.SerialPortConf?.SerialPort);

                return _Default;
            }
        }

        /// <summary>
        /// Čeká na data v sériovém portu a zaznamenává tuto událost do logu.
        /// </summary>
        /// <param name="serialPort">Instance sériového portu.</param>
        /// <param name="methodBase">Metoda nebo operace, kde je prováděno čekání na data.</param>
        /// <param name="barcodeReader">Instance čtečky čárových kódů spojená s operací čekání na data.</param>
        /// <param name="cancellationToken">Token pro zrušení operace.</param>
        private static void WaitForData(IMyApp iMyApp, IBarcodeReader barcodeReader)
        {
            try
            {
                // Čeká na data v sériovém portu.
                while (barcodeReader?.SerialPortConf?.SerialPort?.BytesToRead == 0)
                {
                    Thread.Sleep(TimeSpan.FromMilliseconds(1));
                }

                // Zaznamenává do logu informaci o zjištěných datech od čtečky čárových kódů.
                Reader.Loger.Log(MethodBase.GetCurrentMethod(), iMyApp, Enums.StavBCS.BarcoreReaded, $"Data BCR zjištěna: {barcodeReader?.SerialPortConf?.SerialPort?.BytesToRead} bytes\n");
            }
            catch (Exception f)
            {
                // Předává výjimku k vyšší úrovni zpracování.
                throw f;
            }
        }

        /// <summary>
        /// Načte konfiguraci sériového portu pro čtečku čárových kódů a nastaví ji v čtečce, pokud není již nastavena.
        /// </summary>
        /// <param name="ID">Identifikátor konfigurace sériového portu.</param>
        /// <param name="myApp">Instance aplikace, která obsahuje konfiguraci.</param>
        /// <param name="barcodeReader">Čtečka čárových kódů, do které bude nastavena konfigurace.</param>
        public static void GetSerialPortConf(IMyApp myApp, IBarcodeReader barcodeReader)
        {
            if (barcodeReader?.SerialPortConf != null)
            {
                return;
            }

            SerialPortConfProg Def = new SerialPortConfProg
            {
                SerialPort = null,
                SerialPortConf = new BaseModel.SerialPortConf
                {
                    BcsDelay = 900,
                    ClearBuffer = true,
                    Description = "ERR",
                    PortName = "COM1"
                }
            };

            barcodeReader.SerialPortConf = myApp?.AppConfig?.SerialPortConfig?.TryGetValue(barcodeReader?.SerialPortConfID, out var serialPortValue) == true
                ? serialPortValue
                : Def;
        }

    }
}
