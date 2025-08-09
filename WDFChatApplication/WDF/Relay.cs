using System.Windows.Input;

namespace WDFChatApplication.WDF;
public class Relay : ICommand {
    private readonly Action<object?> execute;
    private readonly Func<object?, bool>? canExecute;

    // Event that WPF's CommandManager uses to automatically reevaluate CanExecute
    public event EventHandler? CanExecuteChanged {
        add { CommandManager.RequerySuggested += value; }
        remove { CommandManager.RequerySuggested -= value; }
    }

    // Constructor that accepts the execute action and an optional canExecute function
    public Relay(Action<object?> execute, Func<object?, bool>? canExecute = null) {
        this.execute = execute ?? throw new ArgumentNullException(nameof(execute));
        this.canExecute = canExecute;
    }

    // Determines whether the command can execute in its current state
    public bool CanExecute(object? parameter) {
        return this.canExecute == null || this.canExecute(parameter);
    }

    // Defines the method to be called when the command is invoked
    public void Execute(object? parameter) {
        this.execute(parameter);
    }
}
