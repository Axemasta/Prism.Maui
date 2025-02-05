﻿namespace PrismMauiDemo.ViewModels;

internal class SplashPageViewModel : IPageLifecycleAware
{
    private INavigationService _navigationService { get; }

    public SplashPageViewModel(INavigationService navigationService)
    {
        _navigationService = navigationService;
    }

    public void OnAppearing()
    {
        _navigationService.CreateBuilder()
            .AddNavigationSegment<RootPageViewModel>()
            .Navigate();
    }

    public void OnDisappearing()
    {

    }
}
