using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Net.NetworkInformation;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace Examples.AutoUpdate;

public abstract class ViewModelBase : INotifyPropertyChanged
{
    public event PropertyChangedEventHandler? PropertyChanged;

    protected void RaisePropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new(propertyName));
    }

    protected bool SetProperty<T>(ref T property, T value, [CallerMemberName] string? propertyName = null)
    {
        if (EqualityComparer<T>.Default.Equals(property, value))
        {
            return false;
        }

        property = value;
        RaisePropertyChanged(propertyName);
        return true;
    }

    protected bool SetProperty<T>(ref T property, T value, Action onPropertyChanged, [CallerMemberName] string? propertyName = null)
    {
        if (EqualityComparer<T>.Default.Equals(property, value))
        {
            return false;
        }

        property = value;
        RaisePropertyChanged(propertyName);
        onPropertyChanged();
        return true;
    }
}

public class MainWindowViewModel : ViewModelBase
{
    private bool _isNetworkAvailable = NetworkInterface.GetIsNetworkAvailable();
    private Version? _currentVersion = Assembly.GetExecutingAssembly().GetName().Version;
    private Version? _latestVersion;
    private string? _currentState;

    public MainWindowViewModel()
    {
        NetworkChange.NetworkAvailabilityChanged += (s, e) => IsNetworkAvailable = e.IsAvailable;
        CheckLatestVersionCommand = new(CheckUpdate);
    }

    public Command CheckLatestVersionCommand { get; set; }

    public bool IsNetworkAvailable
    {
        get => _isNetworkAvailable;
        set => SetProperty(ref _isNetworkAvailable, value);
    }

    public Version? CurrentVersion
    {
        get => _currentVersion;
        set => SetProperty(ref _currentVersion, value, () => RaisePropertyChanged(nameof(IsLatestVersion)));
    }

    public Version? LatestVersion
    {
        get => _latestVersion;
        set => SetProperty(ref _latestVersion, value, () => RaisePropertyChanged(nameof(IsLatestVersion)));
    }

    public bool IsLatestVersion => _latestVersion is not null || (_currentVersion?.Equals(_latestVersion) ?? false);

    public string? CurrentState
    {
        get => _currentState;
        set => SetProperty(ref _currentState, value);
    }

    private async void CheckUpdate()
    {
        var version = await GetLatestVersion();
        LatestVersion = version;
        if (version is null)
        {
            CurrentState = "No Release";
        }
    }

    private static async ValueTask<Version?> GetLatestVersion()
    {
        var github = new Octokit.GitHubClient(new Octokit.ProductHeaderValue("khi0526"));
        var releases = await github.Repository.Release.GetAll("khi0526", "Examples.AutoUpdate");
        if (!releases.Any())
        {
            return null;
        }

        return new Version(releases[0].TagName);
    }
}