// ====================================================================
// FILE: RelayCommand.cs
// PROJECT: OstPlayer - Playnite Plugin for Game Soundtrack Management
// MODULE: Utils
// LOCATION: Utils/
// VERSION: 1.0.0
// CREATED: 2025-08-06
// UPDATED: 2025-08-06
// AUTHOR: TiggAdry
// ====================================================================
//
// PURPOSE:
// General-purpose ICommand implementation for MVVM pattern command binding.
// Provides delegate-based command execution with optional enable/disable logic
// for UI elements in WPF applications following MVVM architecture.
//
// FEATURES:
// - ICommand interface implementation for MVVM binding
// - Delegate-based command execution with parameter support
// - Optional CanExecute predicate for UI state management
// - Thread-safe command execution
// - Lightweight implementation with minimal overhead
// - Reusable across ViewModels and UI scenarios
//
// DEPENDENCIES:
// - System.Windows.Input (ICommand interface)
// - System (Action and Predicate delegates)
//
// COMMAND PATTERN:
// - Execute: Action<object> delegate for command logic
// - CanExecute: Predicate<object> for enabling/disabling UI
// - CanExecuteChanged: Event for UI state notifications
// - Parameter: object passed from UI binding
//
// MVVM INTEGRATION:
// - Binds to Button.Command, MenuItem.Command, etc.
// - Enables/disables UI elements based on CanExecute
// - Supports command parameters from XAML
// - Compatible with WPF command binding infrastructure
//
// PERFORMANCE NOTES:
// - Minimal memory allocation for command instances
// - Efficient delegate invocation
// - No overhead when CanExecute is not provided
// - Lightweight event handling (empty implementation)
//
// LIMITATIONS:
// - CanExecuteChanged event not implemented (no automatic notifications)
// - No built-in async command support
// - Basic parameter type handling (object only)
// - No command composition or chaining support
//
// FUTURE REFACTORING:
// FUTURE: Implement CanExecuteChanged event with CommandManager integration
// FUTURE: Add generic parameter support for type safety
// FUTURE: Create async command variant for long-running operations
// FUTURE: Add command composition capabilities
// FUTURE: Extract to shared UI utilities library
// FUTURE: Add command logging and debugging support
// FUTURE: Implement command history and undo capabilities
// FUTURE: Add command validation and error handling
// CONSIDER: Creating specialized command types (AsyncRelayCommand)
// CONSIDER: Adding command parameter validation
// IDEA: Command middleware for cross-cutting concerns
// IDEA: Integration with dependency injection for command factories
//
// TESTING:
// - Unit tests for command execution with various parameters
// - CanExecute predicate testing with different scenarios
// - MVVM binding integration tests
// - Thread safety tests for concurrent command execution
//
// USAGE EXAMPLES:
// var command = new RelayCommand(param => DoSomething(param), param => CanDoSomething(param));
// <Button Command="{Binding MyCommand}" CommandParameter="{Binding SelectedItem}" />
//
// COMPATIBILITY:
// - .NET Framework 4.6.2
// - C# 7.3
// - WPF command binding infrastructure
// - MVVM frameworks and patterns
//
// CHANGELOG:
// 2025-08-06 v1.0.0 - Initial implementation with MVVM command pattern support
// ====================================================================

using System;
using System.Windows.Input;

namespace OstPlayer.Utils
{
    /// <summary>
    /// RelayCommand implements ICommand, allowing you to bind UI actions to methods in MVVM.
    /// DESIGN PATTERN: Command Pattern implementation for WPF MVVM architecture
    /// THREAD SAFETY: Thread-safe execution (delegates handle their own thread safety)
    /// PERFORMANCE: Lightweight implementation with minimal memory overhead
    /// </summary>
    public class RelayCommand : ICommand
    {
        #region Private Fields - Command Delegates

        // The action to execute when the command is invoked.
        // PARAMETER: object parameter passed from XAML CommandParameter binding
        // THREAD SAFETY: Execution thread safety depends on the provided action
        private readonly Action<object> execute;

        // Predicate to determine if the command can execute (enables/disables UI elements).
        // RETURN VALUE: true = UI element enabled, false = UI element disabled
        // UI BINDING: Automatically controls Button.IsEnabled, MenuItem.IsEnabled, etc.
        private readonly Predicate<object> canExecute;

        #endregion

        #region Constructor - Command Initialization

        /// <summary>
        /// Constructor: takes the execute action and an optional canExecute predicate.
        /// REQUIRED: execute action must not be null (no validation for performance)
        /// OPTIONAL: canExecute predicate (null means command is always enabled)
        /// USAGE: new RelayCommand(param => DoWork(param), param => CanDoWork(param))
        /// </summary>
        /// <param name="execute">Action to execute when command is invoked</param>
        /// <param name="canExecute">Optional predicate to determine if command can execute</param>
        public RelayCommand(Action<object> execute, Predicate<object> canExecute = null)
        {
            this.execute = execute;
            this.canExecute = canExecute;
        }

        #endregion

        #region ICommand Implementation

        /// <summary>
        /// Determines if the command can execute with the given parameter.
        /// If canExecute is not provided, always returns true.
        /// PERFORMANCE: Called frequently by WPF binding system during UI updates
        /// UI IMPACT: Return value directly controls UI element enabled state
        /// PARAMETER: Same object that would be passed to Execute()
        /// </summary>
        /// <param name="parameter">Command parameter from UI binding</param>
        /// <returns>true if command can execute, false if UI should be disabled</returns>
        public bool CanExecute(object parameter)
        {
            // DEFAULT BEHAVIOR: If no canExecute predicate provided, command is always available
            // ALTERNATIVE: Could return false by default for more defensive programming
            // PERFORMANCE: Null check is faster than delegate invocation
            return canExecute == null || canExecute(parameter);
        }

        /// <summary>
        /// Executes the command's action with the given parameter.
        /// THREAD CONTEXT: Executes on the thread that calls it (usually UI thread)
        /// ERROR HANDLING: No exception handling - exceptions bubble to caller
        /// PARAMETER: Object from XAML CommandParameter binding or programmatic calls
        /// </summary>
        /// <param name="parameter">Command parameter from UI binding</param>
        public void Execute(object parameter)
        {
            // DIRECT EXECUTION: No null check for performance (constructor validation assumed)
            // EXCEPTION BEHAVIOR: Any exceptions bubble up to WPF command system
            // THREADING: Executes synchronously on calling thread
            execute(parameter);
        }

        /// <summary>
        /// Event required by ICommand, used to notify the UI when CanExecute changes.
        ///
        /// CRITICAL LIMITATION: Not implemented here (no add/remove logic)!
        ///
        /// IMPACT: UI elements will NOT automatically refresh their enabled state
        /// when underlying conditions change. This can lead to:
        /// - Buttons staying disabled when they should become enabled
        /// - Menu items not reflecting current application state
        /// - Poor user experience with "stuck" UI states
        ///
        /// WORKAROUNDS for automatic UI updates:
        /// 1. Manual refresh: Call CommandManager.InvalidateRequerySuggested()
        /// 2. Property change: Trigger PropertyChanged on bound properties
        /// 3. Event subscription: Manually subscribe to relevant change events
        /// 4. ViewModel refresh: Recreate command instances when state changes
        ///
        /// PROPER IMPLEMENTATION would be:
        /// add { CommandManager.RequerySuggested += value; }
        /// remove { CommandManager.RequerySuggested -= value; }
        ///
        /// WHY NOT IMPLEMENTED:
        /// - Simplicity: Reduces complexity for basic scenarios
        /// - Performance: Avoids CommandManager overhead for static commands
        /// - Control: Allows manual control over when UI updates occur
        ///
        /// WHEN TO USE THIS LIMITATION:
        /// - Commands that never change availability
        /// - Performance-critical scenarios
        /// - Custom refresh logic implementation
        ///
        /// WHEN TO AVOID:
        /// - Dynamic command availability based on application state
        /// - Complex multi-step workflows
        /// - Commands dependent on selection or data changes
        /// </summary>
        public event EventHandler CanExecuteChanged
        {
            // EMPTY IMPLEMENTATION: No actual event subscription logic
            // CONSEQUENCE: CanExecuteChanged event will never be raised
            // UI REFRESH: Manual intervention required for command state updates
            add { }
            remove { }
        }

        #endregion

        #region Usage Examples and Best Practices

        /*
        BASIC USAGE EXAMPLES:

        1. Simple Command (always enabled):
           var saveCommand = new RelayCommand(param => SaveFile());

        2. Command with Parameter:
           var deleteCommand = new RelayCommand(param => DeleteItem((string)param));

        3. Command with CanExecute:
           var submitCommand = new RelayCommand(
               param => SubmitForm(),
               param => IsFormValid()
           );

        4. XAML Binding:
           <Button Command="{Binding SaveCommand}" Content="Save" />
           <Button Command="{Binding DeleteCommand}"
                   CommandParameter="{Binding SelectedItem.Id}"
                   Content="Delete" />

        MANUAL UI REFRESH TECHNIQUES:

        1. CommandManager approach:
           CommandManager.InvalidateRequerySuggested();

        2. Property change approach:
           OnPropertyChanged(nameof(CanSave));

        3. Command recreation approach:
           SaveCommand = new RelayCommand(param => Save(), param => CanSave());

        PERFORMANCE CONSIDERATIONS:
        - CanExecute() called frequently during UI interactions
        - Keep CanExecute logic lightweight and fast
        - Avoid expensive operations in CanExecute predicate
        - Consider caching CanExecute results for complex logic

        THREAD SAFETY NOTES:
        - RelayCommand itself is thread-safe (immutable after construction)
        - Thread safety of execution depends on provided Action delegate
        - CanExecute and Execute typically called from UI thread
        - Cross-thread calls require proper synchronization in delegates
        */

        #endregion
    }
}
