using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common.Exceptions
{
    /// <summary>
    /// Exception if we cannot find a suitable Url slot for a full url
    /// </summary>
    public class ShortUrlHashSlotNotAvailableException : Exception
    {
        public ShortUrlHashSlotNotAvailableException()
            : base() { }

        public ShortUrlHashSlotNotAvailableException(string message)
            : base(message) { }

        public ShortUrlHashSlotNotAvailableException(string format, params object[] args)
            : base(string.Format(format, args)) { }

        public ShortUrlHashSlotNotAvailableException(string message, Exception innerException)
            : base(message, innerException) { }

        public ShortUrlHashSlotNotAvailableException(string format, Exception innerException, params object[] args)
            : base(string.Format(format, args), innerException) { }
    }
}
