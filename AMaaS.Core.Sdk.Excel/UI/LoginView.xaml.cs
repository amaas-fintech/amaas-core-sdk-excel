using Autofac;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace AMaaS.Core.Sdk.Excel.UI
{
    /// <summary>
    /// Interaction logic for LoginView.xaml
    /// </summary>
    public partial class LoginView : UserControl, ILoginView
    {
        public Window ParentWindow { get; set; }
        public IUserViewModel ViewModel
        {
            get { return (IUserViewModel)DataContext; }
            set { DataContext = value; }
        }

        public LoginView()
        {
            ViewModel = AddinContext.Container.Resolve<IUserViewModel>();
            InitializeComponent();
        }

        //TODO: Move this event handlers to ViewModel as Commands
        private void OnCancelClicked(object sender, RoutedEventArgs e)
        {
            ParentWindow?.Close();
        }

        private void OnLoginClicked(object sender, RoutedEventArgs e)
        {
            _errorText.Visibility      = Visibility.Hidden;

            ViewModel?.LoginAsync(() => ParentWindow?.Close(),
            (error) =>
            {
                _errorText.Visibility = Visibility.Visible;
                _errorText.Text       = error;
            });   
        }
    }
}
