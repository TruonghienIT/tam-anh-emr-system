using System;
using System.Windows.Input;

namespace TamAnh_EMR_System.Commands
{
    /// <summary>
    /// Generic implementation of ICommand for MVVM pattern.
    /// Allows binding UI actions (buttons, menus) to ViewModel methods
    /// without placing logic in code-behind.
    /// 
    /// Usage:
    ///   MyCommand = new RelayCommand(ExecuteMethod, CanExecuteMethod);
    /// 
    /// The CanExecuteChanged event is automatically managed by WPF's CommandManager,
    /// which re-evaluates CanExecute whenever the UI detects input changes.
    /// </summary>
    public class RelayCommand : ICommand
    {
        // The action to execute when the command is invoked
        private readonly Action<object> _execute;

        // Optional predicate that determines if the command can execute
        private readonly Predicate<object> _canExecute;

        /// <summary>
        /// Creates a command that can always execute.
        /// </summary>
        /// <param name="execute">The action to run when command is triggered</param>
        public RelayCommand(Action<object> execute)
            : this(execute, null)
        {
        }

        /// <summary>
        /// Creates a command with an optional CanExecute check.
        /// </summary>
        /// <param name="execute">The action to run</param>
        /// <param name="canExecute">Predicate to determine if command is enabled</param>
        public RelayCommand(Action<object> execute, Predicate<object> canExecute)
        {
            _execute = execute ?? throw new ArgumentNullException(nameof(execute));
            _canExecute = canExecute;
        }

        /// <summary>
        /// Hooks into WPF CommandManager to automatically re-query CanExecute
        /// when user interacts with the UI.
        /// </summary>
        public event EventHandler CanExecuteChanged
        {
            add { CommandManager.RequerySuggested += value; }
            remove { CommandManager.RequerySuggested -= value; }
        }

        /// <summary>
        /// Returns true if the command can execute, false otherwise.
        /// If no predicate was provided, always returns true.
        /// </summary>
        public bool CanExecute(object parameter)
        {
            return _canExecute == null || _canExecute(parameter);
        }

        /// <summary>
        /// Runs the command action.
        /// </summary>
        public void Execute(object parameter)
        {
            _execute(parameter);
        }
    }
}
