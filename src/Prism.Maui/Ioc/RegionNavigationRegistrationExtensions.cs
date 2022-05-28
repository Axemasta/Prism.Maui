﻿using Microsoft.Maui.Controls.Compatibility;
using Prism.Regions;
using Prism.Regions.Adapters;
using Prism.Regions.Behaviors;
using Prism.Regions.Navigation;

namespace Prism.Ioc;

public static class RegionNavigationRegistrationExtensions
{
    private static bool s_IsRegistered;

    /// <summary>
    /// Registers a <see cref="View"/> for region navigation.
    /// </summary>
    /// <typeparam name="TView">The Type of <see cref="View"/> to register</typeparam>
    /// <param name="containerRegistry"><see cref="IContainerRegistry"/> used to register type for Navigation.</param>
    /// <param name="name">The unique name to register with the View</param>
    public static IContainerRegistry RegisterForRegionNavigation<TView>(this IContainerRegistry containerRegistry, string name = null)
        where TView : View => 
        containerRegistry.RegisterForNavigationWithViewModel(typeof(TView), null, name);

    /// <summary>
    /// Registers a <see cref="View"/> for region navigation.
    /// </summary>
    /// <typeparam name="TView">The Type of <see cref="View" />to register</typeparam>
    /// <typeparam name="TViewModel">The ViewModel to use as the BindingContext for the View</typeparam>
    /// <param name="name">The unique name to register with the View</param>
    /// <param name="containerRegistry"></param>
    public static IContainerRegistry RegisterForRegionNavigation<TView, TViewModel>(this IContainerRegistry containerRegistry, string name = null)
        where TView : View
        where TViewModel : class => 
        containerRegistry.RegisterForNavigationWithViewModel(typeof(TView), typeof(TViewModel), name);

    private static IContainerRegistry RegisterForNavigationWithViewModel(this IContainerRegistry containerRegistry, Type viewType, Type viewModelType, string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            name = viewType.Name;

        if (viewModelType is not null)
            containerRegistry.Register(viewModelType);
        
        containerRegistry.Register(viewType);

        RegionNavigationRegistry.Register(viewType, viewModelType, name);

        return containerRegistry;
    }

    /// <summary>
    /// Registers a <see cref="View"/> for region navigation.
    /// </summary>
    /// <typeparam name="TView">The Type of <see cref="View"/> to register</typeparam>
    /// <param name="services"><see cref="IServiceCollection"/> used to register type for Navigation.</param>
    /// <param name="name">The unique name to register with the View</param>
    public static IServiceCollection RegisterForRegionNavigation<TView>(this IServiceCollection services, string name = null)
        where TView : View =>
        services.RegisterForNavigationWithViewModel(typeof(TView), null, name);

    /// <summary>
    /// Registers a <see cref="View"/> for region navigation.
    /// </summary>
    /// <typeparam name="TView">The Type of <see cref="View" />to register</typeparam>
    /// <typeparam name="TViewModel">The ViewModel to use as the BindingContext for the View</typeparam>
    /// <param name="name">The unique name to register with the View</param>
    /// <param name="services"></param>
    public static IServiceCollection RegisterForRegionNavigation<TView, TViewModel>(this IServiceCollection services, string name = null)
        where TView : View
        where TViewModel : class =>
        services.RegisterForNavigationWithViewModel(typeof(TView), typeof(TViewModel), name);

    private static IServiceCollection RegisterForNavigationWithViewModel(this IServiceCollection services, Type viewType, Type viewModelType, string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            name = viewType.Name;

        if (viewModelType is not null)
            services.AddTransient(viewModelType);

        services.AddTransient(viewType);

        RegionNavigationRegistry.Register(viewType, viewModelType, name);

        return services;
    }

    public static IContainerRegistry RegisterRegionServices(IContainerRegistry containerRegistry, Action<RegionAdapterMappings> configureAdapters = null, Action<IRegionBehaviorFactory> configureBehaviors = null)
    {
        if (s_IsRegistered)
            return containerRegistry;

        s_IsRegistered = true;
        containerRegistry.RegisterSingleton<RegionAdapterMappings>(p =>
        {
            var regionAdapterMappings = new RegionAdapterMappings();
            configureAdapters?.Invoke(regionAdapterMappings);

            regionAdapterMappings.RegisterDefaultMapping<CarouselView, CarouselViewRegionAdapter>();
            // TODO: CollectionView is buggy with only last View showing despite multiple Active Views
            // BUG: iOS Crash with CollectionView https://github.com/xamarin/Xamarin.Forms/issues/9970
            //regionAdapterMappings.RegisterDefaultMapping<CollectionView, CollectionViewRegionAdapter>();
            regionAdapterMappings.RegisterDefaultMapping<Layout<View>, LayoutViewRegionAdapter>();
            regionAdapterMappings.RegisterDefaultMapping<ScrollView, ScrollViewRegionAdapter>();
            regionAdapterMappings.RegisterDefaultMapping<ContentView, ContentViewRegionAdapter>();
            return regionAdapterMappings;
        });

        containerRegistry.RegisterSingleton<IRegionManager, RegionManager>();
        containerRegistry.RegisterSingleton<IRegionNavigationContentLoader, RegionNavigationContentLoader>();
        containerRegistry.RegisterSingleton<IRegionViewRegistry, RegionViewRegistry>();
        containerRegistry.Register<RegionBehaviorFactory>();
        containerRegistry.RegisterSingleton<IRegionBehaviorFactory>(p =>
        {
            var regionBehaviors = p.Resolve<RegionBehaviorFactory>();
            regionBehaviors.AddIfMissing<BindRegionContextToVisualElementBehavior>(BindRegionContextToVisualElementBehavior.BehaviorKey);
            regionBehaviors.AddIfMissing<RegionActiveAwareBehavior>(RegionActiveAwareBehavior.BehaviorKey);
            regionBehaviors.AddIfMissing<SyncRegionContextWithHostBehavior>(SyncRegionContextWithHostBehavior.BehaviorKey);
            regionBehaviors.AddIfMissing<RegionManagerRegistrationBehavior>(RegionManagerRegistrationBehavior.BehaviorKey);
            regionBehaviors.AddIfMissing<RegionMemberLifetimeBehavior>(RegionMemberLifetimeBehavior.BehaviorKey);
            regionBehaviors.AddIfMissing<ClearChildViewsRegionBehavior>(ClearChildViewsRegionBehavior.BehaviorKey);
            regionBehaviors.AddIfMissing<AutoPopulateRegionBehavior>(AutoPopulateRegionBehavior.BehaviorKey);
            regionBehaviors.AddIfMissing<DestructibleRegionBehavior>(DestructibleRegionBehavior.BehaviorKey);
            configureBehaviors?.Invoke(regionBehaviors);
            return regionBehaviors;
        });
        containerRegistry.Register<IRegionNavigationJournalEntry, RegionNavigationJournalEntry>();
        containerRegistry.Register<IRegionNavigationJournal, RegionNavigationJournal>();
        containerRegistry.Register<IRegionNavigationService, RegionNavigationService>();
        //containerRegistry.RegisterManySingleton<RegionResolverOverrides>(typeof(IResolverOverridesHelper), typeof(IActiveRegionHelper));
        return containerRegistry.RegisterSingleton<IRegionManager, RegionManager>();
    }

    private static IServiceCollection RegisterServices(IServiceCollection services, Action<RegionAdapterMappings> configureAdapters = null, Action<IRegionBehaviorFactory> configureBehaviors = null)
    {
        if (s_IsRegistered)
            return services;

        s_IsRegistered = true;

        services.AddSingleton<RegionAdapterMappings>(p =>
        {
            var regionAdapterMappings = new RegionAdapterMappings();
            configureAdapters?.Invoke(regionAdapterMappings);

            regionAdapterMappings.RegisterDefaultMapping<CarouselView, CarouselViewRegionAdapter>();
            // TODO: CollectionView is buggy with only last View showing despite multiple Active Views
            // BUG: iOS Crash with CollectionView https://github.com/xamarin/Xamarin.Forms/issues/9970
            //regionAdapterMappings.RegisterDefaultMapping<CollectionView, CollectionViewRegionAdapter>();
            regionAdapterMappings.RegisterDefaultMapping<Layout<View>, LayoutViewRegionAdapter>();
            regionAdapterMappings.RegisterDefaultMapping<ScrollView, ScrollViewRegionAdapter>();
            regionAdapterMappings.RegisterDefaultMapping<ContentView, ContentViewRegionAdapter>();
            return regionAdapterMappings;
        });

        services.AddSingleton<IRegionManager, RegionManager>();
        services.AddSingleton<IRegionNavigationContentLoader, RegionNavigationContentLoader>();
        services.AddSingleton<IRegionViewRegistry, RegionViewRegistry>();
        services.AddTransient<RegionBehaviorFactory>();
        services.AddSingleton<IRegionBehaviorFactory>(p =>
        {
            var regionBehaviors = p.GetService<RegionBehaviorFactory>();
            regionBehaviors.AddIfMissing<BindRegionContextToVisualElementBehavior>(BindRegionContextToVisualElementBehavior.BehaviorKey);
            regionBehaviors.AddIfMissing<RegionActiveAwareBehavior>(RegionActiveAwareBehavior.BehaviorKey);
            regionBehaviors.AddIfMissing<SyncRegionContextWithHostBehavior>(SyncRegionContextWithHostBehavior.BehaviorKey);
            regionBehaviors.AddIfMissing<RegionManagerRegistrationBehavior>(RegionManagerRegistrationBehavior.BehaviorKey);
            regionBehaviors.AddIfMissing<RegionMemberLifetimeBehavior>(RegionMemberLifetimeBehavior.BehaviorKey);
            regionBehaviors.AddIfMissing<ClearChildViewsRegionBehavior>(ClearChildViewsRegionBehavior.BehaviorKey);
            regionBehaviors.AddIfMissing<AutoPopulateRegionBehavior>(AutoPopulateRegionBehavior.BehaviorKey);
            regionBehaviors.AddIfMissing<DestructibleRegionBehavior>(DestructibleRegionBehavior.BehaviorKey);
            configureBehaviors?.Invoke(regionBehaviors);
            return regionBehaviors;
        });
        services.AddTransient<IRegionNavigationJournalEntry, RegionNavigationJournalEntry>();
        services.AddTransient<IRegionNavigationJournal, RegionNavigationJournal>();
        services.AddTransient<IRegionNavigationService, RegionNavigationService>();
        //services.RegisterManySingleton<RegionResolverOverrides>(typeof(IResolverOverridesHelper), typeof(IActiveRegionHelper));
        return services.AddSingleton<IRegionManager, RegionManager>();
    }
}
