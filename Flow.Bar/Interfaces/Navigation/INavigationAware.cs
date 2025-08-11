namespace Flow.Bar.Interfaces.Navigation;

public interface INavigationAware
{
    void OnNavigatedTo(object? parameter);

    void OnNavigatedFrom();
}
