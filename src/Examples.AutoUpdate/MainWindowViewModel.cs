using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Net.NetworkInformation;
using System.Reflection;
using System.Runtime.CompilerServices;

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

    public MainWindowViewModel()
    {
        NetworkChange.NetworkAvailabilityChanged += (s, e) => IsNetworkAvailable = e.IsAvailable;
    }

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

    public bool IsLatestVersion => _latestVersion is null || (_currentVersion?.Equals(_latestVersion) ?? false);

    private void CheckUpdate()
    {

    }
}