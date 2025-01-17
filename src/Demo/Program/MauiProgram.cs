﻿using Microsoft.Extensions.Logging;
using DigitalProduction.Maui.DynamicMenus;
using CommunityToolkit.Maui;

namespace MenuDemo;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        MauiAppBuilder builder = MauiApp.CreateBuilder();
        builder
            .UseMauiApp<App>()
			.UseMauiCommunityToolkit()
            .ConfigureFonts(fonts =>
            {
                fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
            });

		#if DEBUG
    		builder.Logging.AddDebug();
		#endif

		CreateService(builder.Services);
		CreateViewModels(builder.Services);

        return builder.Build();
    }

	private static void CreateService(IServiceCollection services)
	{
		services.AddSingleton<IDialogService, DialogService>();
		services.AddSingleton<IMenuService, MenuService>();
	}

    private static void CreateViewModels(IServiceCollection services)
    {
        services.AddSingleton<MainViewModel>();
    }
}