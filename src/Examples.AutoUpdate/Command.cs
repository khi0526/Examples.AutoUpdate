using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;

namespace Examples.AutoUpdate;

public abstract class CommandBase : ICommand
{
    private readonly SynchronizationContext? _synchronizationContext;

    protected CommandBase()
    {
        _synchronizationContext = SynchronizationContext.Current;
    }

    public event EventHandler? CanExecuteChanged;

    bool ICommand.CanExecute(object? parameter)
    {
        return CanExecute(parameter);
    }

    void ICommand.Execute(object? parameter)
    {
        Execute(parameter);
    }

    protected virtual void OnCanExecuteChanged()
    {
        var handler = CanExecuteChanged;
        if (handler != null)
        {
            if (_synchronizationContext != null && _synchronizationContext != SynchronizationContext.Current)
                _synchronizationContext.Post((o) => handler.Invoke(this, EventArgs.Empty), null);
            else
                handler.Invoke(this, EventArgs.Empty);
        }
    }

    public void RaiseCanExecuteChanged()
    {
        OnCanExecuteChanged();
    }

    protected abstract void Execute(object? parameter);

    protected abstract bool CanExecute(object? parameter);
}

public class Command : CommandBase
{
    private readonly Action _executeMethod;
    private readonly Func<bool> _canExecuteMethod;

    public Command(Action executeMethod)
        : this(executeMethod, () => true)
    {

    }

    public Command(Action executeMethod, Func<bool> canExecuteMethod)
        : base()
    {
        if (executeMethod == null || canExecuteMethod == null)
            throw new ArgumentNullException(nameof(executeMethod));

        _executeMethod = executeMethod;
        _canExecuteMethod = canExecuteMethod;
    }

    public void Execute()
    {
        _executeMethod();
    }

    public bool CanExecute()
    {
        return _canExecuteMethod();
    }

    protected override bool CanExecute(object? parameter)
    {
        return CanExecute();
    }

    protected override void Execute(object? parameter)
    {
        Execute();
    }
}