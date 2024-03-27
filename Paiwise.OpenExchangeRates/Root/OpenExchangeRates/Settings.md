Title: openexchangerates.org settings
Description: Configures integration with the openexchangerates.org backend currency index.
Date: 2024-03-27
Author: Peter Waher
Master: /Master.md
Cache-Control: max-age=0, no-cache, no-store
UserVariable: User
Privilege: Admin.Payments.Paiwise.OpenExchangeRates
Login: /Login.md

========================================================================

<form action="Settings.md" method="post">
<fieldset>
<legend>openexchangerates.org settings</legend>

The following settings are required by the integration of the Neuron(R) with the <openexchangerates.org> currency index service back-end. 
By providing such an integration, services can access current currency indices on the neuron.

{{
if exists(Posted) then
(
	SetSetting("Paiwise.OpenExchangeRates.ApiKey",Posted.ApiKey);
	SetSetting("Paiwise.OpenExchangeRates.MaxAgeSeconds",Num(Posted.MaxAgeSeconds));

	Paiwise.OpenExchangeRates.OpenExchangeRatesService.InvalidateCurrent();

	SeeOther("Settings.md");
);
}}

<p>
<label for="ApiKey">API Key:</label>  
<input type="text" id="ApiKey" name="ApiKey" value='{{GetSetting("Paiwise.OpenExchangeRates.ApiKey","")}}' autofocus required title="API Key for accessing openexchangerates.org."/>
</p>

<p>
<label for="MaxAgeSeconds">Maximum Age of Exchange Rates (in seconds):</label>  
<input type="number" min="1" max="3600" id="MaxAgeSeconds" name="MaxAgeSeconds" value='{{GetSetting("Paiwise.OpenExchangeRates.MaxAgeSeconds",3600)}}' title="Maximum age (in seconds) of an exchange rate, before a new exchange rate is fetched."/>
</p>

<button type="submit" class="posButton">Apply</button>
</fieldset>
</form>