﻿using Prism.Common;
using Prism.Navigation.Builder;

namespace Prism.Navigation;

public static class NavigationBuilderExtensions
{
    public static INavigationBuilder CreateBuilder(this INavigationService navigationService) =>
           new NavigationBuilder(navigationService);

    internal static string GetNavigationKey<TViewModel>(object builder)
    {
        var vmType = typeof(TViewModel);
        if (vmType.IsAssignableFrom(typeof(VisualElement)))
            throw new NavigationException(NavigationException.MvvmPatternBreak, typeof(TViewModel).Name);

        if (builder is not IRegistryAware registryAware)
            throw new Exception("The builder does not implement IRegistryAware");

        return registryAware.Registry.GetViewModelNavigationKey(vmType);
    }

    public static INavigationBuilder UseAbsoluteNavigation(this INavigationBuilder builder) =>
        builder.UseAbsoluteNavigation(true);

    public static INavigationBuilder AddNavigationSegment(this INavigationBuilder builder, string segmentName, bool? useModalNavigation = null) =>
        builder.AddNavigationSegment(segmentName, o =>
        {
            if (useModalNavigation.HasValue)
                o.UseModalNavigation(useModalNavigation.Value);
        });

    public static ICreateTabBuilder AddNavigationSegment<TViewModel>(this ICreateTabBuilder builder)  =>
        builder.AddNavigationSegment<TViewModel>(b => { });

    public static ICreateTabBuilder AddNavigationSegment<TViewModel>(this ICreateTabBuilder builder, Action<ISegmentBuilder> configureSegment) =>
        builder.AddNavigationSegment(GetNavigationKey<TViewModel>(builder), configureSegment);

    public static INavigationBuilder AddNavigationSegment<TViewModel>(this INavigationBuilder builder) =>
        builder.AddNavigationSegment<TViewModel>(b => { });

    public static INavigationBuilder AddNavigationSegment<TViewModel>(this INavigationBuilder builder, Action<ISegmentBuilder> configureSegment) =>
        builder.AddNavigationSegment(GetNavigationKey<TViewModel>(builder), configureSegment);

    public static INavigationBuilder AddNavigationSegment<TViewModel>(this INavigationBuilder builder, bool useModalNavigation) =>
        builder.AddNavigationSegment<TViewModel>(b => b.UseModalNavigation(useModalNavigation));

    // Will check for the Navigation key of a registered NavigationPage
    public static INavigationBuilder AddNavigationPage(this INavigationBuilder builder) =>
        builder.AddNavigationPage(b => { });

    public static INavigationBuilder AddNavigationPage(this INavigationBuilder builder, Action<ISegmentBuilder> configureSegment)
    {
        if (builder is not IRegistryAware registryAware)
            throw new Exception("The builder does not implement IRegistryAware");

        var registrations = registryAware.Registry.ViewsOfType(typeof(NavigationPage));
        if (!registrations.Any())
            throw new NavigationException(NavigationException.NoPageIsRegistered, nameof(NavigationPage));

        var registration = registrations.Last();
        return builder.AddNavigationSegment(registration.Name, configureSegment);
    }

    public static ICreateTabBuilder AddNavigationPage(this ICreateTabBuilder builder) =>
        builder.AddNavigationPage(b => { });

    public static ICreateTabBuilder AddNavigationPage(this ICreateTabBuilder builder, Action<ISegmentBuilder> configureSegment)
    {
        if (builder is not IRegistryAware registryAware)
            throw new Exception("The builder does not implement IRegistryAware");

        var registrations = registryAware.Registry.ViewsOfType(typeof(NavigationPage));
        if (!registrations.Any())
            throw new NavigationException(NavigationException.NoPageIsRegistered, nameof(NavigationPage));

        var registration = registrations.Last();
        return builder.AddNavigationSegment(registration.Name, configureSegment);
    }

    public static INavigationBuilder AddNavigationPage(this INavigationBuilder builder, bool useModalNavigation) =>
        builder.AddNavigationPage(o => o.UseModalNavigation(useModalNavigation));

    //public static INavigationBuilder AddNavigationSegment(this INavigationBuilder builder, string segmentName, params string[] createTabs)
    //{
    //    return builder;
    //}

    //public static INavigationBuilder AddNavigationSegment(this INavigationBuilder builder, string segmentName, bool useModalNavigation, params string[] createTabs)
    //{
    //    return builder;
    //}

    //public static INavigationBuilder AddNavigationSegment(this INavigationBuilder builder, string segmentName, string selectTab, bool? useModalNavigation, params string[] createTabs)
    //{
    //    return builder;
    //}

    public static async void Navigate(this INavigationBuilder builder)
    {
        await builder.NavigateAsync();
    }

    public static async void Navigate(this INavigationBuilder builder, Action<Exception> onError)
    {
        await builder.NavigateAsync(onError);
    }

    public static async void Navigate(this INavigationBuilder builder, Action onSuccess)
    {
        await builder.NavigateAsync(onSuccess, _ => { });
    }

    public static async void Navigate(this INavigationBuilder builder, Action onSuccess, Action<Exception> onError)
    {
        await builder.NavigateAsync(onSuccess, onError);
    }

    public static ISegmentBuilder UseModalNavigation(this ISegmentBuilder builder) =>
        builder.UseModalNavigation(true);

    public static ICreateTabBuilder AddNavigationSegment(this ICreateTabBuilder builder, string segmentNameOrUri) =>
            builder.AddNavigationSegment(segmentNameOrUri, null);

    public static ITabbedSegmentBuilder CreateTab(this ITabbedSegmentBuilder builder, string segmentName, Action<ISegmentBuilder> configureSegment) =>
        builder.CreateTab(o => o.AddNavigationSegment(segmentName, configureSegment));

    public static ITabbedSegmentBuilder CreateTab(this ITabbedSegmentBuilder builder, string segmentNameOrUri) =>
        builder.CreateTab(o => o.AddNavigationSegment(segmentNameOrUri));

    public static ITabbedSegmentBuilder CreateTab<TViewModel>(this ITabbedSegmentBuilder builder)
    {
        var navigationKey = GetNavigationKey<TViewModel>(builder);
        return builder.CreateTab(navigationKey);
    }

    public static ITabbedSegmentBuilder SelectTab<TViewModel>(this ITabbedSegmentBuilder builder)
    {
        var navigationKey = GetNavigationKey<TViewModel>(builder);
        return builder.SelectedTab(navigationKey);
    }

    public static ITabbedNavigationBuilder SelectTab(this ITabbedNavigationBuilder builder, params string[] navigationSegments) =>
        builder.SelectTab(string.Join("|", navigationSegments));
}
