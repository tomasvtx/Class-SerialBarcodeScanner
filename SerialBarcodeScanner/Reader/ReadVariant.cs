using SerialBarcodeScanner.Model;
using System;
using System.Diagnostics;
using System.IO.Ports;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;

namespace SerialBarcodeScanner.Reader
{
    internal class ReadVariant
    {
        /// <summary>
        /// Čte data ze sériového portu a ukládá je do výstupního řetězce s možností nastavení maximální délky a sledování časového limitu.
        /// </summary>
        /// <param name="serialPort">Instance sériového portu pro čtení dat.</param>
        /// <param name="maxReadLength">Maximální délka dat k přečtení.</param>
        /// <param name="Read">Výstupní řetězec obsahující přečtená data.</param>
        /// <returns>True, pokud byla operace čtení úspěšná; jinak False.</returns>
        protected static bool ReadData(IBarcodeReader barcodeReader, int maxReadLength, out string Data)
        {
            // Vytvoření nového StringBuilderu pro ukládání přečtených dat.
            StringBuilder sb = new StringBuilder();

            try
            {
                // Inicializace stopky pro sledování času čtení.
                Stopwatch stopwatch = new Stopwatch();
                stopwatch.Start();

                // Pokud je délka přečtených dat menší než zadaná maximální délka a nebyl dosažen časový limit (1 sekunda), pokračuj v čtení.
                while (sb.Length < maxReadLength && stopwatch.Elapsed < TimeSpan.FromSeconds(1))
                {
                    Thread.Sleep(TimeSpan.FromMilliseconds(1));
                    sb?.Append(barcodeReader?.SerialPortConf?.SerialPort?.ReadExisting());

                    if (stopwatch?.Elapsed > TimeSpan.FromSeconds(1))
                    {
                        // Časový limit (1 sekunda) byl překročen, ukončení čekání.
                        break;
                    }


                    if (barcodeReader?.CancellationBarcodeReading.IsCancellationRequested != false)
                    {
                        // Pokud byla přijata žádost o zrušení operace, ukonči čtení.
                        break;
                    }
                }
                Data = sb?.ToString();

                return true;
            }
            catch (Exception ex)
            {
                // Pokud došlo k výjimce, přidej chybovou zprávu k datům.
                sb?.Append(ex?.Message);
                Data = sb?.ToString();

                // Návrat s neúspěšným výsledkem.
                return false;
            }
        }

        /// <summary>
        /// Čte data ze sériového portu a pokračuje ve čtení, dokud není přijata žádost o zrušení operace.
        /// </summary>
        /// <param name="serialPort">Instance sériového portu pro čtení dat.</param>
        /// <param name="cancellationToken">Token pro zrušení operace.</param>
        /// <param name="Read">Načtená data ze sériového portu.</param>
        /// <returns>True, pokud byla operace úspěšná, jinak False.</returns>
        protected static bool ReadUntilCancellation(IBarcodeReader barcodeReader, out string Data)
        {
            // Vytvoření nového StringBuilderu pro ukládání přečtených dat.
            StringBuilder sb = new StringBuilder();

            try
            {
                // Neustále čti data ze sériového portu, dokud nezpůsobí žádost o zrušení operace.
                while (true)
                {
                    Thread.Sleep(TimeSpan.FromMilliseconds(1));
                    sb?.Append(barcodeReader?.SerialPortConf?.SerialPort?.ReadExisting());

                    if (barcodeReader?.CancellationBarcodeReading.IsCancellationRequested != false)
                    {
                        // Pokud byla přijata žádost o zrušení operace, ukonči čtení.
                        break;
                    }
                }
                Data = sb?.ToString();

                return true;
            }
            catch (Exception ex)
            {
                // Pokud došlo k výjimce, přidej chybovou zprávu k datům.
                sb?.Append(ex.Message);
                Data = sb?.ToString();

                // Návrat s neúspěšným výsledkem.
                return false;
            }
        }

        /// <summary>
        /// Čte data ze sériového portu po dobu zadaného časového intervalu.
        /// </summary>
        /// <param name="serialPort">Instance sériového portu pro čtení dat.</param>
        /// <param name="duration">Časový interval pro čtení dat.</param>
        /// <param name="Read">Načtená data ze sériového portu.</param>
        /// <returns>True, pokud byla operace úspěšná, jinak False.</returns>
        protected static bool ReadForTime(IBarcodeReader barcodeReader, out string Data)
        {
            // Vytvoření nového StringBuilderu pro ukládání přečtených dat
            StringBuilder sb = new StringBuilder();

            try
            {
                // Vytvoření a spuštění stopky pro měření času trvání operace.
                Stopwatch stopwatch = new Stopwatch();
                stopwatch?.Start();

                // Postupné čtení dat ze sériového portu po dobu trvání definované stopkou.
                while (stopwatch?.Elapsed < barcodeReader?.BarcodeReadingInterval)
                {
                    // Malá prodleva pro efektivní čtení dat.
                    Thread.Sleep(TimeSpan.FromMilliseconds(1));
                    sb?.Append(barcodeReader?.SerialPortConf?.SerialPort?.ReadExisting());

                    if (barcodeReader?.CancellationBarcodeReading.IsCancellationRequested != false)
                    {
                        // Pokud byla přijata žádost o zrušení operace, ukonči čtení.
                        break;
                    }
                }
                Data = sb?.ToString();

                // Úspěšné dokončení operace.
                return true;
            }
            catch (Exception ex)
            {
                // Pokud došlo k výjimce, přidej chybovou zprávu k datům.
                sb?.Append(ex.Message);
                Data = sb?.ToString();

                // Návrat s neúspěšným výsledkem.
                return false;
            }
        }

        /// <summary>
        /// Čte data ze sériového portu až do dosažení zadaného oddělovače (delimiteru).
        /// </summary>
        /// <param name="serialPort">Instance SerialPort pro komunikaci se sériovým portem.</param>
        /// <param name="delimiter">Oddělovač, na který se má čekat při čtení.</param>
        /// <param name="Read">Výstupní řetězec obsahující načtená data bez oddělovače.</param>
        /// <returns>True, pokud byla operace čtení úspěšná; jinak false.</returns>
        protected static bool ReadUntilDelimiter(IBarcodeReader barcodeReader, out string Data)
        {
            StringBuilder sb = new StringBuilder();

            // Nastaví oddělovač pro SerialPort na zadanou hodnotu.
            barcodeReader.SerialPortConf.SerialPort.NewLine = Regex.Unescape(barcodeReader?.Delimiter ?? @"\r");

            try
            {
                // Čte data ze sériového portu do vnitřního StringBuilderu, dokud nedojde k zjištění oddělovače.
                while (!sb.ToString().Contains(Regex.Unescape(barcodeReader?.Delimiter ?? @"\r")))
                {
                    sb?.Append(Convert.ToChar(barcodeReader?.SerialPortConf?.SerialPort?.ReadChar()));

                    if (barcodeReader?.CancellationBarcodeReading.IsCancellationRequested != false)
                    {
                        // Pokud byla přijata žádost o zrušení operace, ukonči čtení.
                        break;
                    }
                }
                Data = sb?.ToString();

                return true;
            }
            catch (Exception ex)
            {
                // Pokud došlo k výjimce, přidej chybovou zprávu k datům.
                sb?.Append(ex.Message);
                Data = sb?.ToString();

                // Návrat s neúspěšným výsledkem.
                return false;
            }
        }
    }
}
