//Code based on Marlon Grech's post here: http://marlongrech.wordpress.com/2008/11/26/avoiding-commandbinding-in-the-xaml-code-behind-files/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace MetroPad.Input
{
    public class CommandBase : ICommand
    {
        public Predicate<object> CanExecuteDelegate { get; set; }

        public Action<object> ExecuteDelegate { get; set; }
        
        public bool CanExecute(object parameter)
        {
            if (CanExecuteDelegate == null)
            {
                return true;
            }

            return CanExecuteDelegate(parameter);
        }

        public event EventHandler CanExecuteChanged;

        /// <summary>
        /// Raises the CanExecuteChanged event
        /// </summary>
        public void OnCanExecuteChanged()
        {
            if (CanExecuteChanged != null)
            {
                CanExecuteChanged(this, EventArgs.Empty);
            }
        }

        public void Execute(object parameter)
        {
            if (ExecuteDelegate == null)
            {
                return;
            }
            
            ExecuteDelegate(parameter);
        }
    }
}
