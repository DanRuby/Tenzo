using System;
using System.Windows.Input;

namespace tEngine.MVVM
{
    /// <summary>
    /// The CancelCommandEvent delegate.
    /// </summary>
    public delegate void CancelCommandEventHandler( object sender, CancelCommandEventArgs args );

    /// <summary>
    /// The CommandEventHandler delegate.
    /// </summary>
    public delegate void CommandEventHandler( object sender, CommandEventArgs args );

    /// <summary>
    /// CancelCommandEventArgs - just like above but allows the event to
    /// be cancelled.
    /// </summary>
    public class CancelCommandEventArgs : CommandEventArgs {
        /// <summary>
        /// Gets or sets a value indicating whether this <see cref="CancelCommandEventArgs"/> command should be cancelled.
        /// </summary>
        /// <value><c>true</c> if cancel; otherwise, <c>false</c>.</value>
        public bool Cancel { get; set; }
    }

    /// <summary>
    /// The ViewModelCommand class - an ICommand that can fire a function.
    /// </summary>
    public class Command : ICommand {
        /// <summary>
        /// The action (or parameterized action) that will be called when the command is invoked.
        /// </summary>
        protected Action mAction = null;

        protected Action<object> mParameterizedAction = null;

        /// <summary>
        /// Bool indicating whether the command can execute.
        /// </summary>
        private bool mCanExecute = false;

        /// <summary>
        /// Gets or sets a value indicating whether this instance can execute.
        /// </summary>
        /// <value>
        /// 	<c>true</c> if this instance can execute; otherwise, <c>false</c>.
        /// </value>
        public bool CanExecute {
            get { return mCanExecute; }
            set {
                if( mCanExecute != value ) {
                    mCanExecute = value;
                    EventHandler canExecuteChanged = CanExecuteChanged;
                    if( canExecuteChanged != null )
                        canExecuteChanged( this, EventArgs.Empty );
                }
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Command"/> class.
        /// </summary>
        /// <param name="action">The action.</param>
        /// <param name="canExecute">if set to <c>true</c> [can execute].</param>
        public Command( Action action, bool canExecute = true ) {
            //  Set the action.
            this.mAction = action;
            this.mCanExecute = canExecute;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Command"/> class.
        /// </summary>
        /// <param name="parameterizedAction">The parameterized action.</param>
        /// <param name="canExecute">if set to <c>true</c> [can execute].</param>
        public Command( Action<object> parameterizedAction, bool canExecute = true ) {
            //  Set the action.
            this.mParameterizedAction = parameterizedAction;
            this.mCanExecute = canExecute;
        }

        /// <summary>
        /// Occurs when can execute is changed.
        /// </summary>
        public event EventHandler CanExecuteChanged;

        /// <summary>
        /// Executes the command.
        /// </summary>
        /// <param name="param">The param.</param>
        public virtual void DoExecute( object param ) {
            //  Invoke the executing command, allowing the command to be cancelled.
            CancelCommandEventArgs args = new CancelCommandEventArgs() {Parameter = param, Cancel = false};
            InvokeExecuting( args );

            //  If the event has been cancelled, bail now.
            if( args.Cancel )
                return;

            //  Call the action or the parameterized action, whichever has been set.
            InvokeAction( param );

            //  Call the executed function.
            InvokeExecuted( new CommandEventArgs() {Parameter = param} );
        }

        /// <summary>
        /// Occurs when the command executed.
        /// </summary>
        public event CommandEventHandler Executed;

        /// <summary>
        /// Occurs when the command is about to execute.
        /// </summary>
        public event CancelCommandEventHandler Executing;

        protected void InvokeAction( object param ) {
            Action theAction = mAction;
            Action<object> theParameterizedAction = mParameterizedAction;
            if( theAction != null )
                theAction();
            else if( theParameterizedAction != null )
                theParameterizedAction( param );
        }

        protected void InvokeExecuted( CommandEventArgs args ) {
            CommandEventHandler executed = Executed;

            //  Call the executed event.
            if( executed != null )
                executed( this, args );
        }

        protected void InvokeExecuting( CancelCommandEventArgs args ) {
            CancelCommandEventHandler executing = Executing;

            //  Call the executed event.
            if( executing != null )
                executing( this, args );
        }

        #region ICommand Members

        /// <summary>
        /// Defines the method that determines whether the command can execute in its current state.
        /// </summary>
        /// <param name="parameter">Data used by the command.  If the command does not require data to be passed, this object can be set to null.</param>
        /// <returns>
        /// true if this command can be executed; otherwise, false.
        /// </returns>
        bool ICommand.CanExecute( object parameter ) {
            return mCanExecute;
        }

        /// <summary>
        /// Defines the method to be called when the command is invoked.
        /// </summary>
        /// <param name="parameter">Data used by the command.  If the command does not require data to be passed, this object can be set to null.</param>
        void ICommand.Execute( object parameter ) {
            this.DoExecute( parameter );
        }

        #endregion ICommand Members
    }

    /// <summary>
    /// CommandEventArgs - simply holds the command parameter.
    /// </summary>
    public class CommandEventArgs : EventArgs {
        /// <summary>
        /// Gets or sets the parameter.
        /// </summary>
        /// <value>The parameter.</value>
        public object Parameter { get; set; }
    }
}