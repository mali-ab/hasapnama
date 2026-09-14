using Microsoft.Extensions.Logging;
using HasapNama.App.Services;

namespace HasapNama.App;

public static class MauiProgram
{
	public static MauiApp CreateMauiApp()
	{
		var builder = MauiApp.CreateBuilder();
		builder
			.UseMauiApp<App>()
			.ConfigureFonts(fonts =>
			{
				fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
				fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
			});

		builder.Services.AddMauiBlazorWebView();

		// Shared server connection
		builder.Services.AddSingleton<HttpClient>(_ => new HttpClient
		{
			Timeout = TimeSpan.FromSeconds(30)
		});
		builder.Services.AddSingleton<ApiClient>();
		builder.Services.AddSingleton<AuthService>();

#if DEBUG
		builder.Services.AddBlazorWebViewDeveloperTools();
		builder.Logging.AddDebug();
#endif

		return builder.Build();
	}
}