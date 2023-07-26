using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Microsoft.AspNetCore.Components.WebAssembly.Authentication;
using Microsoft.AspNetCore.Components.Authorization;
using MudBlazor.Services;
using Chat.Client;
using Chat.Client.State;
using Chat.Client.Security;


var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

builder.Services.AddSingleton<StateContainer>();
builder.Services.AddMudServices();

if (builder.Configuration.GetValue<string>("Mock") == (true).ToString())
{
    builder.Services.AddOptions();
    builder.Services.AddAuthorizationCore();
    builder.Services.AddScoped<IAccessTokenProvider, MockAccessTokenProvider>();
    builder.Services.AddScoped<AuthenticationStateProvider, MockAuthenticationStateProvider>();
    builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });
    builder.Services.AddGraphQLClient()
       .ConfigureHttpClient(client =>
       {
           client.BaseAddress = new Uri(builder.HostEnvironment.BaseAddress + "graphql");
       });
}
else
{
    builder.Services.AddHttpClient("Chat.Client.ServerAPI", client => client.BaseAddress = new Uri(builder.HostEnvironment.BaseAddress))
    .AddHttpMessageHandler<BaseAddressAuthorizationMessageHandler>();
    builder.Services.AddScoped(sp => sp.GetRequiredService<IHttpClientFactory>().CreateClient("Chat.Client.ServerAPI"));
    builder.Services.AddGraphQLClient()
       .ConfigureHttpClient(client =>
       {
           client.BaseAddress = new Uri(builder.HostEnvironment.BaseAddress + "graphql");
       }, builder => builder.AddHttpMessageHandler<BaseAddressAuthorizationMessageHandler>());
    builder.Services.AddMsalAuthentication(options =>
    {
        builder.Configuration.Bind("AzureAdB2C", options.ProviderOptions.Authentication);
        options.ProviderOptions.DefaultAccessTokenScopes.Add(builder.Configuration.GetValue<string>("Scope_Uri")!);
        options.ProviderOptions.LoginMode = "redirect";
    });
}

await builder.Build().RunAsync();