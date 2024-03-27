using System;
using Waher.Persistence;

namespace Paiwise.OpenExchangeRates
{
	/// <summary>
	/// Conversion Rate
	/// </summary>
	public class ExchangeRate : ICurrencyConverterQuote
	{
		/// <summary>
		/// Conversion Rate
		/// </summary>
		/// <param name="From">Conversion from</param>
		/// <param name="To">Conversion to</param>
		/// <param name="Rate">Conversion rate</param>
		/// <param name="Timestamp">Timestamp of quote</param>
		/// <param name="Source">Source of quote</param>
		public ExchangeRate(CaseInsensitiveString From, CaseInsensitiveString To, decimal Rate, DateTime Timestamp, string Source)
		{
			this.FromCurrency = From;
			this.ToCurrency = To;
			this.Rate = Rate;
			this.Timestamp = Timestamp;
			this.Source = Source;
		}

		/// <summary>
		/// Conversion from this currency.
		/// </summary>
		public CaseInsensitiveString FromCurrency { get; }

		/// <summary>
		/// Conversion to this currency.
		/// </summary>
		public CaseInsensitiveString ToCurrency { get; }

		/// <summary>
		/// Exchange rate.
		/// </summary>
		public decimal Rate { get; }

		/// <summary>
		/// Timestamp of quote.
		/// </summary>
		public DateTime Timestamp { get; }

		/// <summary>
		/// Source of quote.
		/// </summary>
		public string Source { get; }
	}
}
