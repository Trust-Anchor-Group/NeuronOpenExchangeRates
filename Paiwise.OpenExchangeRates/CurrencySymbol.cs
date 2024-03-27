using Waher.Persistence;

namespace Paiwise.OpenExchangeRates
{
	/// <summary>
	/// Currency Symbol
	/// </summary>
	public class CurrencySymbol
	{
		/// <summary>
		/// Currency Symbol
		/// </summary>
		/// <param name="Currency">Currency</param>
		/// <param name="Description">Descruption</param>
		public CurrencySymbol(CaseInsensitiveString Currency, string Description)
		{
			this.Currency = Currency;
			this.Description = Description;
		}

		/// <summary>
		/// Currency
		/// </summary>
		public CaseInsensitiveString Currency { get; }

		/// <summary>
		/// Description
		/// </summary>
		public string Description { get; }
	}
}
