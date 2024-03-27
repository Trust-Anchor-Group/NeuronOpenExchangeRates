using System;

namespace Paiwise.OpenExchangeRates
{
	/// <summary>
	/// Exceptions based on errors reported by openexchangerates.org
	/// </summary>
	public class OpenExchangeRatesException : Exception
	{
		private readonly int code;

		/// <summary>
		/// Exceptions based on errors reported by openexchangerates.org
		/// </summary>
		/// <param name="Code">Error code</param>
		/// <param name="Message">Error message</param>
		public OpenExchangeRatesException(int Code, string Message)
			: base(Message)
		{
			this.code = Code;
		}

		/// <summary>
		/// Error code
		/// </summary>
		public int Code => this.code;
	}
}
