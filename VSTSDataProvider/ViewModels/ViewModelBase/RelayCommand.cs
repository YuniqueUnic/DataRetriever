using System;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace VSTSDataProvider.ViewModels.ViewModelBase;

public class RelayCommand : ICommand
{
    private readonly Action<object> _execute;
    private readonly Action _executeNonParam;
    private readonly Predicate<object> _canExecute;
    private Action<object , RoutedEventArgs> decreaseIndentationButton_Clicked;

    public event EventHandler CanExecuteChanged;
    //{
    //    add { CommandManager.RequerySuggested += value; }
    //    remove { CommandManager.RequerySuggested -= value; }
    //}

    public RelayCommand(Action<object> execute , Predicate<object> canExecute = null)
    {
        _execute = execute ?? throw new ArgumentNullException(nameof(execute));
        _canExecute = canExecute;
    }

    public RelayCommand(Action executeNonParam , Predicate<object> canExecute = null)
    {
        _executeNonParam = executeNonParam ?? throw new ArgumentNullException(nameof(executeNonParam));
        _canExecute = canExecute;
    }

    public RelayCommand(Action<object , RoutedEventArgs> decreaseIndentationButton_Clicked)
    {
        this.decreaseIndentationButton_Clicked = decreaseIndentationButton_Clicked;
    }

    public bool CanExecute(object parameter)
    {
        return _canExecute == null || _canExecute(parameter);
    }

    public void Execute(object parameter)
    {
        if( _execute != null )
        {
            _execute(parameter);
        }
        else if( _executeNonParam != null )
        {
            _executeNonParam();
        }
    }

    public void RaiseCanExecuteChanged( ) => OnCanExecuteChanged(); // 触发命令可执行性改变事件

    protected virtual void OnCanExecuteChanged( ) => CanExecuteChanged?.Invoke(this , EventArgs.Empty);
}

public class AsyncRelayCommand : ViewModelBase.BaseViewModel, ICommand
{
    private readonly Func<CancellationToken , Task> _execute; // 异步执行的委托
    private readonly Func<object , bool> _canExecute; // 判断命令是否可执行的委托
    private readonly Action _onCompleted; // 命令执行完成时的回调函数
    private CancellationTokenSource _cancellationTokenSource; // 取消命令执行的令牌源
    private bool _isExecuting; // 表示命令是否正在执行的标志位
    private Func<Task> refreshDataTableAsync;
    private Func<object , bool> value;
    private Func<CancellationTokenSource , Task> updateRichTextTextAsync;

    public bool IsExecuting
    {
        get => _isExecuting;
        private set
        {
            SetProperty(ref _isExecuting , value);
            OnCanExecuteChanged();
        }
    }

    public event EventHandler? CanExecuteChanged; // 命令可执行性改变时触发的事件

    public AsyncRelayCommand(Func<CancellationToken , Task> execute , Func<object , bool> canExecute = null , Action onCompleted = null)
    {
        _execute = execute ?? throw new ArgumentNullException(nameof(execute)); // 检查异步执行的委托是否为空
        _canExecute = canExecute; // 可执行性判断的委托可选
        _onCompleted = onCompleted; // 命令执行完成时的回调函数可选
        _cancellationTokenSource = new CancellationTokenSource(); // 创建取消命令执行的令牌源
    }

    public AsyncRelayCommand(Func<Task> refreshDataTableAsync , Func<object , bool> value)
    {
        this.refreshDataTableAsync = refreshDataTableAsync;
        this.value = value;
    }

    public AsyncRelayCommand(Func<CancellationTokenSource , Task> updateRichTextTextAsync)
    {
        this.updateRichTextTextAsync = updateRichTextTextAsync;
    }

    public bool CanExecute(object parameter)
    {
        return !_isExecuting && (_canExecute?.Invoke(parameter) ?? true); // 判断命令是否可执行
    }

    public async void Execute(object parameter)
    {
        if( !CanExecute(parameter) )
        {
            return;
        }

        _cancellationTokenSource = new CancellationTokenSource(); // 创建新的取消命令执行的令牌源
        var cancellationToken = _cancellationTokenSource.Token; // 获取取消命令执行的令牌

        try
        {
            IsExecuting = true; // 设置命令正在执行的标志位
            await _execute(cancellationToken); // 异步执行命令
        }
        finally
        {
            IsExecuting = false; // 设置命令执行完成的标志位
            _onCompleted?.Invoke(); // 执行命令执行完成时的回调函数
        }
    }

    public void ExecuteWithCancellation(object parameter)
    {
        if( !CanExecute(parameter) )
        {
            return;
        }

        _cancellationTokenSource?.Cancel(); // 取消之前的命令执行
        _cancellationTokenSource = new CancellationTokenSource(); // 创建新的取消命令执行的令牌源
        var cancellationToken = _cancellationTokenSource.Token; // 获取取消命令执行的令牌

        try
        {
            IsExecuting = true; // 设置命令正在执行的标志位
            _execute(cancellationToken).Wait(cancellationToken); // 同步执行命令，并等待命令执行完成或被取消
        }
        catch( OperationCanceledException )
        {
            // 忽略取消异常
        }
        finally
        {
            IsExecuting = false; // 设置命令执行完成的标志位
            _onCompleted?.Invoke(); // 执行命令执行完成时的回调函数
        }
    }

    // 触发命令可执行性改变事件 
    public void RaiseCanExecuteChanged( ) => OnCanExecuteChanged();

    // 触发命令可执行性改变事件
    protected virtual void OnCanExecuteChanged( ) => CanExecuteChanged?.Invoke(this , EventArgs.Empty);
}

//带结果的 AsyncRelayCommand
public class AsyncRelayCommand<TResult> : ViewModelBase.BaseViewModel, ICommand
{
    private readonly Func<CancellationToken , Task<TResult>> _execute;
    private readonly Func<bool> _canExecute;
    private CancellationTokenSource _cts;
    private bool _isExecuting;

    public bool IsExecuting
    {
        get { return _isExecuting; }
        private set
        {
            SetProperty(ref _isExecuting , value);
            OnCanExecuteChanged();
        }
    }

    public event EventHandler CanExecuteChanged;
    public event EventHandler<TResult> ExecutionCompleted;
    public event EventHandler<Exception> ExecutionError;

    public AsyncRelayCommand(Func<CancellationToken , Task<TResult>> execute , Func<bool> canExecute = null)
    {
        _execute = execute ?? throw new ArgumentNullException(nameof(execute));
        _canExecute = canExecute;
    }

    public bool CanExecute(object parameter)
    {
        return !_isExecuting && (_canExecute?.Invoke() ?? true);
    }

    public async void Execute(object parameter)
    {
        if( CanExecute(parameter) )
        {
            try
            {
                _isExecuting = true;
                _cts = new CancellationTokenSource();
                var result = await _execute(_cts.Token);
                OnExecutionCompleted(result);
            }
            catch( Exception ex )
            {
                OnExecutionError(ex);
            }
            finally
            {
                _isExecuting = false;
                _cts.Dispose();
            }
        }
    }

    // 触发命令可执行性改变事件
    public void RaiseCanExecuteChanged( ) => OnCanExecuteChanged();

    public void Cancel( ) => _cts?.Cancel();

    protected virtual void OnExecutionCompleted(TResult result) => ExecutionCompleted?.Invoke(this , result);

    protected virtual void OnExecutionError(Exception ex) => ExecutionError?.Invoke(this , ex);

    // 触发命令可执行性改变事件 
    protected virtual void OnCanExecuteChanged( ) => CanExecuteChanged?.Invoke(this , EventArgs.Empty);
}