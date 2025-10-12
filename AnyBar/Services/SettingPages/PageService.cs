using System;
using System.Collections.Generic;
using System.Linq;
using AnyBar.Enums;
using AnyBar.Views;
using iNKORE.UI.WPF.Modern.Controls;

namespace AnyBar.Services;

public class PageService
{
    private readonly Dictionary<SettingPageTag, Type> _pages = [];
    private readonly Dictionary<SettingPageTag, SettingPageTag> _containedPages = [];

    public PageService()
    {
        Configure<SettingsPaneGeneral>(SettingPageTag.General);
        Configure<SettingsPaneAppBar>(SettingPageTag.AppBar);
        Configure<SettingsPaneAppBarSetting>(SettingPageTag.AppBarSetting);
        Configure<SettingsPanePlugins>(SettingPageTag.Plugins);
        Configure<SettingsPaneAbout>(SettingPageTag.About);
        Configure<SettingsPaneBarElementSetting>(SettingPageTag.BarElementSetting);

        Configure(SettingPageTag.AppBarSetting, SettingPageTag.AppBar);
        Configure(SettingPageTag.BarElementSetting, SettingPageTag.AppBar);
    }

    public Type GetPageType(SettingPageTag tag)
    {
        Type? page;
        lock (_pages)
        {
            if (!_pages.TryGetValue(tag, out page))
            {
                throw new ArgumentException($"Page not found: {tag}. Did you forget to call {nameof(PageService)}.{nameof(Configure)}?");
            }
        }

        return page;
    }

    public SettingPageTag GetPageTag(Type pageType)
    {
        lock (_pages)
        {
            if (!_pages.ContainsValue(pageType))
            {
                throw new ArgumentException($"Page not found: {pageType}. Did you forget to call {nameof(PageService)}.{nameof(Configure)}?");
            }

            return _pages.FirstOrDefault(p => p.Value == pageType).Key;
        }
    }

    public SettingPageTag? GetContainedPageTag(SettingPageTag containedTag)
    {
        lock (_containedPages)
        {
            if (!_containedPages.TryGetValue(containedTag, out var tag))
            {
                return null;
            }

            return tag;
        }
    }

    private void Configure<V>(SettingPageTag tag) where V : Page
    {
        lock (_pages)
        {
            if (_pages.ContainsKey(tag))
            {
                throw new ArgumentException($"{tag} is already configured in {nameof(PageService)}");
            }

            var view = typeof(V);
            if (_pages.ContainsValue(view))
            {
                throw new ArgumentException($"This type is already configured with {_pages.First(p => p.Value == view).Key}");
            }

            _pages.Add(tag, view);
        }
    }

    private void Configure(SettingPageTag containedTag, SettingPageTag tag)
    {
        lock (_pages)
        {
            if (_containedPages.ContainsKey(containedTag))
            {
                throw new ArgumentException($"{containedTag} is already configured in {nameof(PageService)}");
            }

            _containedPages.Add(containedTag, tag);
        }
    }
}
