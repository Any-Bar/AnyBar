using Flow.Bar.Models.Enums;
using Flow.Bar.Views.SettingPages;
using iNKORE.UI.WPF.Modern.Controls;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Flow.Bar.Services;

public class PageService
{
    private readonly Dictionary<SettingPageTag, Type> _pages = [];

    public PageService()
    {
        Configure<SettingsPaneGeneral>(SettingPageTag.General);
        Configure<SettingsPaneAppBar>(SettingPageTag.AppBar);
        Configure<SettingsPaneAbout>(SettingPageTag.About);
        Configure<SettingsPaneAppBarSetting>(SettingPageTag.AppBarSetting);
    }

    public Type GetPageType(SettingPageTag tag)
    {
        Type? page;
        lock (_pages)
        {
            if (!_pages.TryGetValue(tag, out page))
            {
                throw new ArgumentException($"Page not found: {tag}. Did you forget to call PageService.Configure?");
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
                throw new ArgumentException($"Page not found: {pageType}. Did you forget to call PageService.Configure?");
            }

            return _pages.FirstOrDefault(p => p.Value == pageType).Key;
        }
    }

    private void Configure<V>(SettingPageTag tag) where V : Page
    {
        lock (_pages)
        {
            if (_pages.ContainsKey(tag))
            {
                throw new ArgumentException($"The tag {tag} is already configured in PageService!");
            }

            var view = typeof(V);
            if (_pages.ContainsValue(view))
            {
                throw new ArgumentException($"This type is already configured with tag {_pages.First(p => p.Value == view).Key}!");
            }

            _pages.Add(tag, view);
        }
    }
}
