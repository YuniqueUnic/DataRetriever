using System;
using System.Windows.Input;

namespace VSTSDataProvider.ViewModels.ViewModelBase;

class RelayCommand : ICommand
{
    private Action action;

    public RelayCommand(Action action)
    {
        this.action = action;
    }

    public event EventHandler CanExecuteChanged = (sender , e) => { };

    public bool CanExecute(object parameter)
    {
        return true;
    }

    public void Execute(object parameter)
    {
        action();
    }
}