using AppConfigure;
using DaikinLabel;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using static AppConfigure.BaseModel;
using static AppConfigure.BaseModelProgr;

namespace SerialBarcodeScanner.Model
{
    /// <summary>
    /// Rozhraní pro čtečku čárových kódů, umožňuje asynchronní skenování a zpracování čárových kódů.
    /// </summary>
    public interface IBarcodeReader : IDisposable
    {
        CancellationToken CancellationBarcodeReading { get; set; }
        /// <summary>
        /// Získá nebo nastaví oddělovač pro čárové kódy.
        /// </summary>
        string Delimiter { get; }

        /// <summary>
        /// Získá dobu trvání čtení čárového kódu.
        /// </summary>
        TimeSpan BarcodeReadingInterval { get; }

        /// <summary>
        /// Získá kolekci typů štítků, které tato čtečka podporuje.
        /// </summary>
        ICollection<Type> LabelTypes { get; }

        /// <summary>
        /// Získá režim čtení dat.
        /// </summary>
        ReadMode ReadMode { get; }

        /// <summary>
        /// Získá nebo nastaví načtený štítek (čárový kód).
        /// </summary>
        Label ScannedLabel { get; set; }

        /// <summary>
        /// Získá nebo nastaví identifikátor konfigurace sériového portu používaného pro čtečku čárových kódů.
        /// </summary>
        string SerialPortConfID { get; }

        /// <summary>
        /// Získá nebo nastaví konfiguraci sériového portu používaného pro čtečku čárových kódů.
        /// </summary>
        SerialPortConfProg SerialPortConf { get; set; }

        /// <summary>
        /// Spustí proces skenování čárových kódů.
        /// </summary>
        /// <returns>Asynchronní úkol reprezentující proces skenování.</returns>
        Task StartScanning();

        /// <summary>
        /// Obsluhuje situaci, kdy byl čárový kód úspěšně načten.
        /// </summary>
        /// <returns>Asynchronní úkol reprezentující úspěšné načtení čárového kódu.</returns>
        Task HandleBarcodeOk();

        /// <summary>
        /// Obsluhuje situaci, kdy čárový kód není platný nebo nastala chyba při načítání.
        /// </summary>
        /// <returns>Asynchronní úkol reprezentující chybu v čárovém kódu.</returns>
        Task HandleBarcodeError();

        /// <summary>
        /// Obsluhuje situaci, kdy nastala chyba při načítání.
        /// </summary>
        /// <returns>Asynchronní úkol reprezentující chybu v čárovém kódu.</returns>
        Task HandleReaderError(BarcodeReaderData barcodeReaderData);

        /// <summary>
        /// Provádí akce po dokončení úkolu skenování.
        /// </summary>
        Task PostTask();
    }
}
