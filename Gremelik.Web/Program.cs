using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Gremelik.Web;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri("https://5267-firebase-gremelik-1768666520774.cluster-j6d3cbsvdbe5uxnhqrfzzeyj7i.cloudworkstations.dev/") });

await builder.Build().RunAsync();
