using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SGT_BRIDGE.Models
{
    /// <summary>
    /// Klasa przeznaczona do przekazywania błędów w zamówieniach
    /// </summary>
    public class OrderError
    {
        /// <summary>
        /// Źródło zamówienia
        /// </summary>
        public string Source { get; set; }

        /// <summary>
        /// Numer zamówienia
        /// </summary>
        public string Number { get; set; }

        /// <summary>
        /// Numer zewnętrzny zamówienia
        /// </summary>
        public string External_number { get; set; }

        /// <summary>
        /// Kod błędu
        /// </summary>
        public string Error_code { get; set; }

        /// <summary>
        /// Opis błędu
        /// </summary>
        public string Error_description { get; set; }

        /// <summary>
        /// Stack Trace do debugowania
        /// </summary>
        public string Stack_trace { get; set; }

        public OrderError() { }

        public OrderError(string orderSource, string orderExternalNumber, string orderNumber, string errorCode, string errorDescription, string stacktrace)
        {
            Source = orderSource;
            External_number = orderExternalNumber;
            Number = orderNumber;
            Error_code = errorCode;
            Error_description = errorDescription;
        }

        public override string ToString()
        {
            return $"OrderError(errcode={Error_code}, errdesc={Error_description}, source={Source}, no={Number}, exNo={External_number})";
        }
    }
}
