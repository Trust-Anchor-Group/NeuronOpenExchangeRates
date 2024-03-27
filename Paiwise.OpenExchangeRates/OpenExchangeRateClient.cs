using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using Waher.Content;
using Waher.Persistence;
using Waher.Script;

namespace Paiwise.OpenExchangeRates
{
	public class OpenExchangeRateClient
	{
		private readonly string apiKey;

		/// <summary>
		/// Currency Converter service from openexchangerates.org.
		/// </summary>
		/// <param name="ApiKey">API Key</param>
		public OpenExchangeRateClient(string ApiKey)
		{
			this.apiKey = ApiKey;
		}

		/// <summary>
		/// Current API Key
		/// </summary>
		public string ApiKey => this.apiKey;


		#region HTTP Layer

		private async Task<T> RequestRespond<T>(string Url, string ResponseElement,
			params KeyValuePair<string, string>[] Parameters)
		{
			StringBuilder sb = new StringBuilder();

			sb.Append("https://openexchangerates.org/api/");
			sb.Append(Url);
			sb.Append("?app_id=");
			sb.Append(this.apiKey);

			foreach (KeyValuePair<string, string> P in Parameters)
			{
				sb.Append('&');
				sb.Append(HttpUtility.UrlEncode(P.Key));
				sb.Append('=');
				sb.Append(HttpUtility.UrlEncode(P.Value));
			}

			Uri Uri = new Uri(sb.ToString());

			if (!(await InternetContent.GetAsync(Uri, new KeyValuePair<string, string>[]
			{
				new KeyValuePair<string, string>("Accept", "application/json"),
			}) is Dictionary<string, object> Result))
			{
				throw new Exception("Unexpected response returned from openexchangerates.org.");
			}

			if (string.IsNullOrEmpty(ResponseElement))
			{
				if (Result is T Response)
					return Response;
			}
			else if (Result.TryGetValue(ResponseElement, out object Obj))
			{
				if (Obj is T Response2)
					return Response2;
				else if (System.Convert.ChangeType(Obj, typeof(T)) is T Response3)
					return Response3;
			}

			throw new Exception("Unexpected response returned from openexchangerates.org.");
		}

		#endregion

		#region Get Latest

		/// <summary>
		/// Gets latest exchange rates.
		/// </summary>
		/// <returns>Array of exchange rates.</returns>
		public Task<ExchangeRate[]> GetLatest()
		{
			return this.GetLatest(null, null, false);
		}

		/// <summary>
		/// Gets latest exchange rates.
		/// </summary>
		/// <param name="BaseCurrency">Base currency for rates.</param>
		/// <returns>Array of exchange rates.</returns>
		public Task<ExchangeRate[]> GetLatest(string BaseCurrency)
		{
			return this.GetLatest(BaseCurrency, null, false);
		}

		/// <summary>
		/// Gets latest exchange rates.
		/// </summary>
		/// <param name="Symbols">Symbols of interest</param>
		/// <returns>Array of exchange rates.</returns>
		public Task<ExchangeRate[]> GetLatest(string[] Symbols)
		{
			return this.GetLatest(null, Symbols, false);
		}

		/// <summary>
		/// Gets latest exchange rates.
		/// </summary>
		/// <param name="ShowAlternative">If alternative rates are to be returned.</param>
		/// <returns>Array of exchange rates.</returns>
		public Task<ExchangeRate[]> GetLatest(bool ShowAlternative)
		{
			return this.GetLatest(null, null, ShowAlternative);
		}

		/// <summary>
		/// Gets latest exchange rates.
		/// </summary>
		/// <param name="BaseCurrency">Base currency for rates.</param>
		/// <param name="Symbols">Symbols of interest</param>
		/// <returns>Array of exchange rates.</returns>
		public Task<ExchangeRate[]> GetLatest(string BaseCurrency, string[] Symbols)
		{
			return this.GetLatest(BaseCurrency, Symbols, false);
		}

		/// <summary>
		/// Gets latest exchange rates.
		/// </summary>
		/// <param name="BaseCurrency">Base currency for rates.</param>
		/// <param name="ShowAlternative">If alternative rates are to be returned.</param>
		/// <returns>Array of exchange rates.</returns>
		public Task<ExchangeRate[]> GetLatest(string BaseCurrency, bool ShowAlternative)
		{
			return this.GetLatest(BaseCurrency, null, ShowAlternative);
		}

		/// <summary>
		/// Gets latest exchange rates.
		/// </summary>
		/// <param name="BaseCurrency">Base currency for rates.</param>
		/// <param name="Symbols">Symbols of interest</param>
		/// <param name="ShowAlternative">If alternative rates are to be returned.</param>
		/// <returns>Array of exchange rates.</returns>
		public async Task<ExchangeRate[]> GetLatest(string BaseCurrency, string[] Symbols,
			bool ShowAlternative)
		{
			List<KeyValuePair<string, string>> Request = new List<KeyValuePair<string, string>>()
			{
				new KeyValuePair<string, string>("show_alternative", CommonTypes.Encode(ShowAlternative))
			};

			if (!string.IsNullOrEmpty(BaseCurrency))
				Request.Add(new KeyValuePair<string, string>("base", BaseCurrency));

			if (!(Symbols is null) && Symbols.Length > 0)
			{
				StringBuilder sb = new StringBuilder();
				bool First = true;

				foreach (string Symbol in Symbols)
				{
					if (First)
						First = false;
					else
						sb.Append(',');

					sb.Append(Symbol);
				}

				Request.Add(new KeyValuePair<string, string>("symbols", sb.ToString()));
			}

			Dictionary<string, object> Response = await this.RequestRespond<Dictionary<string, object>>("latest.json", null,
				Request.ToArray());

			if (Response.TryGetValue("base", out object Obj) && Obj is string Base &&
				Response.TryGetValue("rates", out Obj) && Obj is Dictionary<string, object> Rates)
			{
				int i = 0, c = Rates.Count;
				ExchangeRate[] Result = new ExchangeRate[c];
				Dictionary<CaseInsensitiveString, decimal> Reference = new Dictionary<CaseInsensitiveString, decimal>();
				DateTime TP = DateTime.UtcNow;

				foreach (KeyValuePair<string, object> P in Rates)
				{
					decimal d = Expression.ToDecimal(P.Value);

					Result[i++] = new ExchangeRate(Base, P.Key, d, TP, "openexchangerates.org");
					Reference[P.Key] = d;
				}

				return Result;
			}
			else
				throw new Exception("Unexpected response returned from openexchangerates.org.");
		}

		#endregion

		#region Get Currencies

		/// <summary>
		/// Gets supported currency symbols.
		/// 
		/// Note: This method will always load new symbols from the server.
		/// </summary>
		/// <returns>Supported currency symbols, together with descriptive names.</returns>
		public Task<CurrencySymbol[]> GetCurrencies()
		{
			return this.GetCurrencies(false, false);
		}

		/// <summary>
		/// Gets supported currency symbols.
		/// 
		/// Note: This method will always load new symbols from the server.
		/// </summary>
		/// <param name="ShowAlternative">If alternative currencies are to be shown.</param>
		/// <param name="ShowInactive">If inactive currencies are to be shown.</param>
		/// <returns>Supported currency symbols, together with descriptive names.</returns>
		public async Task<CurrencySymbol[]> GetCurrencies(bool ShowAlternative, bool ShowInactive)
		{
			Dictionary<string, object> Symbols = await this.RequestRespond<Dictionary<string, object>>("currencies.json", null,
				new KeyValuePair<string, string>("show_alternative", CommonTypes.Encode(ShowAlternative)),
				new KeyValuePair<string, string>("show_inactive", CommonTypes.Encode(ShowInactive)));
			int i = 0, c = Symbols.Count;
			CurrencySymbol[] Result = new CurrencySymbol[c];

			foreach (KeyValuePair<string, object> P in Symbols)
				Result[i++] = new CurrencySymbol(P.Key, P.Value?.ToString());

			return Result;
		}

		#endregion

		#region Convert

		/// <summary>
		/// Converts an amount from one currency to another.
		/// </summary>
		/// <returns>Converted currency.</returns>
		public async Task<decimal> Convert(CaseInsensitiveString From, CaseInsensitiveString To, int Amount)
		{
			StringBuilder sb = new StringBuilder();

			sb.Append("convert/");
			sb.Append(Amount.ToString());
			sb.Append('/');
			sb.Append(From.Value.ToUpper());
			sb.Append('/');
			sb.Append(To.Value.ToUpper());

			double Converted = await this.RequestRespond<double>(sb.ToString(), "result");

			return (decimal)Converted;
		}

		#endregion

	}
}
