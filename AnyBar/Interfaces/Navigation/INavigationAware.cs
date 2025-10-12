namespace AnyBar.Interfaces;

public interface INavigationAware
{
    void OnNavigatedTo(object? parameter);

    void OnNavigatedFrom();
}
