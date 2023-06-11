using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace VSTSDataProvider.ViewModels.ViewModelBase;

public abstract class BaseViewModel : INotifyPropertyChanged, INotifyPropertyChanging
{
    public event PropertyChangingEventHandler PropertyChanging;
    public event PropertyChangedEventHandler PropertyChanged;

    protected void RaisePropertyChangingFor([CallerMemberName] String propertyName = "")
    {
        PropertyChanging?.Invoke(this , new PropertyChangingEventArgs(propertyName));
    }


    protected virtual void OnPropertyChanging(object newValue , [CallerMemberName] string propertyName = null)
    {
        PropertyChanging?.Invoke(this , new PropertyChangingEventArgs(propertyName));
    }


    protected void SetProperty<T>(ref T targetField , T newValue , [CallerMemberName] string propertyName = null)
    {
        if( !EqualityComparer<T>.Default.Equals(targetField , newValue) )
        {
            OnPropertyChanging(newValue , propertyName);
            targetField = newValue;
            OnPropertyChanged(newValue , propertyName);
        }
    }

    protected void RaisePropertyChangedFor([CallerMemberName] string propertyName = null)
    {
        PropertyChanged?.Invoke(this , new PropertyChangedEventArgs(propertyName));
    }

    protected virtual void OnPropertyChanged(object newValue , [CallerMemberName] string propertyName = null)
    {
        PropertyChanged?.Invoke(this , new PropertyChangedEventArgs(propertyName));
    }


}