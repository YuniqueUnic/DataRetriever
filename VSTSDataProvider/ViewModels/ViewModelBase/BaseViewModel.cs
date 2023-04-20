using System.ComponentModel;

namespace VSTSDataProvider.ViewModels.ViewModelBase;

public class BaseViewModel : INotifyPropertyChanged
{
    public event PropertyChangedEventHandler PropertyChanged = (sender , e) => { };
}