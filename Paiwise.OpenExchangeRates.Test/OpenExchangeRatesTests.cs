using System.Text;
using Waher.Content;
using Waher.Persistence;
using Waher.Persistence.Files;
using Waher.Persistence.Serialization;
using Waher.Runtime.Inventory;
using Waher.Runtime.Settings;
using Waher.Script;

namespace Paiwise.OpenExchangeRates.Test
{
	[TestClass]
	public class OpenExchangeRatesTests
	{
		private static FilesProvider? filesProvider = null;

		[AssemblyInitialize]
		public static async Task AssemblyInitialize(TestContext _)
		{
			Types.Initialize(
				typeof(OpenExchangeRatesTests).Assembly,
				typeof(ICurrencyConverterService).Assembly,
				typeof(OpenExchangeRatesService).Assembly,
				typeof(InternetContent).Assembly,
				typeof(Database).Assembly,
				typeof(FilesProvider).Assembly,
				typeof(ObjectSerializer).Assembly,
				typeof(RuntimeSettings).Assembly,
				typeof(Expression).Assembly);

			if (!Database.HasProvider)
			{
				filesProvider = await FilesProvider.CreateAsync("Data", "Default", 8192, 1000, 8192, Encoding.UTF8, 10000, true);
				Database.Register(filesProvider);
			}

			await RuntimeSettings.SetAsync("Paiwise.OpenExchangeRates.ApiKey", "7e4c92b8710647b0bb656d4de50f5d77");		// Enter Key here

			// Before running tests for the first time, configure the following properties:
			// NOTE: Do not check in keys.
			//
			// await RuntimeSettings.SetAsync("Paiwise.OpenExchangeRates.ApiKey", "");		// Enter Key here

			Assert.IsTrue(await Types.StartAllModules(60000));
		}

		[AssemblyCleanup]
		public static async Task AssemblyCleanup()
		{
			await Types.StopAllModules();

			filesProvider?.Dispose();
			filesProvider = null;
		}

		[TestMethod]
		public async Task Test_01_GetCurrencies()
		{
			OpenExchangeRatesService Service = Types.Instantiate<OpenExchangeRatesService>(false);
			CurrencySymbol[] Symbols = await Service.Client.GetCurrencies();

			foreach (CurrencySymbol P in Symbols)
				Console.Out.WriteLine(P.Currency + ": " + P.Description);
		}

		[TestMethod]
		public async Task Test_02_GetAlternativeCurrencies()
		{
			OpenExchangeRatesService Service = Types.Instantiate<OpenExchangeRatesService>(false);
			CurrencySymbol[] Symbols = await Service.Client.GetCurrencies(true, false);

			foreach (CurrencySymbol P in Symbols)
				Console.Out.WriteLine(P.Currency + ": " + P.Description);
		}

		[TestMethod]
		public async Task Test_03_GetInactiveCurrencies()
		{
			OpenExchangeRatesService Service = Types.Instantiate<OpenExchangeRatesService>(false);
			CurrencySymbol[] Symbols = await Service.Client.GetCurrencies(false, true);

			foreach (CurrencySymbol P in Symbols)
				Console.Out.WriteLine(P.Currency + ": " + P.Description);
		}

		[TestMethod]
		public async Task Test_04_GetAlternativeAndInactiveCurrencies()
		{
			OpenExchangeRatesService Service = Types.Instantiate<OpenExchangeRatesService>(false);
			CurrencySymbol[] Symbols = await Service.Client.GetCurrencies(true, true);

			foreach (CurrencySymbol P in Symbols)
				Console.Out.WriteLine(P.Currency + ": " + P.Description);
		}

		[TestMethod]
		public async Task Test_05_GetLatest()
		{
			OpenExchangeRatesService Service = Types.Instantiate<OpenExchangeRatesService>(false);
			ExchangeRate[] Rates = await Service.Client.GetLatest();

			foreach (ExchangeRate P in Rates)
				Console.Out.WriteLine(P.FromCurrency + " -> " + P.ToCurrency + " : " + P.Rate.ToString());
		}

		[TestMethod]
		[Ignore]
		public async Task Test_06_GetLatest_Base()
		{
			OpenExchangeRatesService Service = Types.Instantiate<OpenExchangeRatesService>(false);
			ExchangeRate[] Rates = await Service.Client.GetLatest("EUR", false);

			foreach (ExchangeRate P in Rates)
				Console.Out.WriteLine(P.FromCurrency + " -> " + P.ToCurrency + " : " + P.Rate.ToString());
		}

		[TestMethod]
		public async Task Test_07_GetLatest_Symbols()
		{
			OpenExchangeRatesService Service = Types.Instantiate<OpenExchangeRatesService>(false);
			ExchangeRate[] Rates = await Service.Client.GetLatest(new string[] { "SEK", "CLP", "USD" });

			foreach (ExchangeRate P in Rates)
				Console.Out.WriteLine(P.FromCurrency + " -> " + P.ToCurrency + " : " + P.Rate.ToString());
		}

		[TestMethod]
		public async Task Test_08_GetLatest_Alternative()
		{
			OpenExchangeRatesService Service = Types.Instantiate<OpenExchangeRatesService>(false);
			ExchangeRate[] Rates = await Service.Client.GetLatest(true);

			foreach (ExchangeRate P in Rates)
				Console.Out.WriteLine(P.FromCurrency + " -> " + P.ToCurrency + " : " + P.Rate.ToString());
		}

		[TestMethod]
		[Ignore]
		public async Task Test_09_Convert()
		{
			OpenExchangeRatesService Service = Types.Instantiate<OpenExchangeRatesService>(false);
			decimal Rate = await Service.Client.Convert("USD", "EUR", 1);

			Console.Out.WriteLine("1 USD = " + Rate.ToString() + " EUR");
		}

		[TestMethod]
		public async Task Test_10_Quote()
		{
			OpenExchangeRatesService Service = Types.Instantiate<OpenExchangeRatesService>(false);
			ICurrencyConverterQuote Quote = await Service.GetCurrencyConversionQuote("SEK", "CLP");
		
			Console.Out.WriteLine("SEK => CLP: " + Quote.Rate.ToString());
		}

	}
}
