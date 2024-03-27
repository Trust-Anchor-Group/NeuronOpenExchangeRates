using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Waher.Events;
using Waher.IoTGateway;
using Waher.Persistence;
using Waher.Runtime.Inventory;
using Waher.Runtime.Settings;

namespace Paiwise.OpenExchangeRates
{
	/// <summary>
	/// Currency Converter service from openexchangerates.org.
	/// 
	/// Ref: https://docs.openexchangerates.org/reference/api-introduction
	/// </summary>
	[Singleton]
	public class OpenExchangeRatesService : ICurrencyConverterService, IConfigurableModule
	{
		#region Setup

		private string apiKey;
		private OpenExchangeRateClient client = null;
		private Dictionary<string, CurrencySymbol> currencyDictionary;
		private Dictionary<CaseInsensitiveString, decimal> ratesReference;
		private CurrencySymbol[] currencies;
		private DateTime ratesTimestamp = DateTime.MinValue;
		private int rateMaxAgeSeconds;
		private readonly object syncObject = new object();

		/// <summary>
		/// Currency Converter service from openexchangerates.org.
		/// </summary>
		public OpenExchangeRatesService()
			: this(string.Empty)
		{
		}

		/// <summary>
		/// Currency Converter service from openexchangerates.org.
		/// </summary>
		/// <param name="ApiKey">API Key</param>
		public OpenExchangeRatesService(string ApiKey)
		{
			this.apiKey = ApiKey;
		}

		/// <summary>
		/// Current API Key
		/// </summary>
		public string ApiKey => this.apiKey;

		/// <summary>
		/// Sets a new API Key
		/// </summary>
		/// <param name="ApiKey"></param>
		/// <returns></returns>
		public async Task SetApiKey(string ApiKey)
		{
			if (this.apiKey != ApiKey)
			{
				this.apiKey = ApiKey;
				await RuntimeSettings.SetAsync("Paiwise.OpenExchangeRates.ApiKey", ApiKey);

				this.client = null;

				await this.CheckCurrencies();
			}
		}

		/// <summary>
		/// Reference to the API client
		/// </summary>
		public OpenExchangeRateClient Client
		{
			get
			{
				if (this.client is null)
					this.client = new OpenExchangeRateClient(this.apiKey);

				return this.client;
			}
		}

		#endregion

		#region IModule interface

		/// <summary>
		/// Starts the module.
		/// </summary>
		public async Task Start()
		{
			this.apiKey = await RuntimeSettings.GetAsync("Paiwise.OpenExchangeRates.ApiKey", this.apiKey);
			this.rateMaxAgeSeconds = (int)await RuntimeSettings.GetAsync("Paiwise.OpenExchangeRates.MaxAgeSeconds", 3600);

			await this.CheckCurrencies();

			Gateway.OnAfterBackup += this.Gateway_OnAfterBackup;
		}

		private async void Gateway_OnAfterBackup(object sender, EventArgs e)
		{
			if (!string.IsNullOrEmpty(this.apiKey))
			{
				try
				{
					await this.LoadCurrencies();
				}
				catch (Exception ex)
				{
					Log.Critical(ex);
				}
			}
		}

		private async Task CheckCurrencies()
		{
			if (this.currencies is null && !string.IsNullOrEmpty(this.apiKey))
				await this.LoadCurrencies();
		}

		private async Task LoadCurrencies()
		{
			try
			{
				CurrencySymbol[] Currencies = await this.Client.GetCurrencies();    // Just to know what currency symbols are available.
				Dictionary<string, CurrencySymbol> Sorted = new Dictionary<string, CurrencySymbol>();

				foreach (CurrencySymbol Symbol in this.currencies)
					Sorted[Symbol.Currency] = Symbol;

				this.currencies = Currencies;
				this.currencyDictionary = Sorted;
			}
			catch (Exception ex)
			{
				Log.Critical(ex);
			}
		}

		/// <summary>
		/// Stops the module.
		/// </summary>
		public Task Stop()
		{
			this.apiKey = string.Empty;
			return Task.CompletedTask;
		}

		#endregion

		#region IConfigurableModule interface

		/// <summary>
		/// Gets an array of configurable pages for the module.
		/// </summary>
		/// <returns>Configurable pages</returns>
		public Task<IConfigurablePage[]> GetConfigurablePages()
		{
			return Task.FromResult(new IConfigurablePage[]
			{
				new ConfigurablePage("Fixer.io", "/OpenExchangeRates/Settings.md")
			});
		}

		#endregion

		#region API interface

		#endregion

		#region ICurrencyConverterService interface

		/// <summary>
		/// If the interface understands objects such as <paramref name="Object"/>.
		/// </summary>
		/// <param name="Object">Object</param>
		/// <returns>How well objects of this type are supported.</returns>
		public Grade Supports(CurrencyPair Object)
		{
			if (this.apiKey is null)
				return Grade.NotAtAll;

			try
			{
				if (!(this.currencyDictionary is null))
				{
					return this.currencyDictionary.ContainsKey(Object.From.Value.ToUpper()) &&
						this.currencyDictionary.ContainsKey(Object.To.Value.ToUpper()) ? Grade.Ok : Grade.NotAtAll;
				}

				this.CheckCurrencies().Wait();

				if (this.currencyDictionary is null)
					return Grade.NotAtAll;

				return this.currencyDictionary.ContainsKey(Object.From.Value.ToUpper()) &&
					this.currencyDictionary.ContainsKey(Object.To.Value.ToUpper()) ? Grade.Ok : Grade.NotAtAll;
			}
			catch (Exception)
			{
				return Grade.NotAtAll;
			}
		}

		/// <summary>
		/// Gets a Currency Exchange Rate from one currency to another.
		/// </summary>
		/// <param name="FromCurrency">From what currency conversion is to be made.</param>
		/// <param name="ToCurrency">To what currency conversion is to be made.</param>
		/// <returns>Exchange rate quote, if able, null if not.</returns>
		/// <exception cref="Exception">If errors occur within the service, that need to be propagated to the caller.</exception>
		public async Task<ICurrencyConverterQuote> GetCurrencyConversionQuote(CaseInsensitiveString FromCurrency, CaseInsensitiveString ToCurrency)
		{
			DateTime Now = DateTime.Now;

			if (Now.Subtract(this.ratesTimestamp).TotalSeconds >= this.rateMaxAgeSeconds)
			{
				ExchangeRate[] Rates = await this.Client.GetLatest();

				lock (this.syncObject)
				{
					Dictionary<CaseInsensitiveString, decimal> Sorted = new Dictionary<CaseInsensitiveString, decimal>();

					foreach (ExchangeRate Rate in Rates)
						Sorted[Rate.ToCurrency] = Rate.Rate;

					this.ratesReference = Sorted;
					this.ratesTimestamp = Now;
				}
			}

			lock (this.syncObject)
			{
				if (this.ratesReference.TryGetValue(FromCurrency, out decimal FromRate) &&
					this.ratesReference.TryGetValue(ToCurrency, out decimal ToRate))
				{
					return new ExchangeRate(FromCurrency, ToCurrency, ToRate / FromRate,
						this.ratesTimestamp, "openexchangerates.org");
				}
				else
					return null;
			}
		}

		#endregion

	}
}
