using ANB_UI.Tools;
using System.Configuration;
using System.Data;
using System.Windows;
using Application = System.Windows.Application;

namespace GAIA
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        protected override void OnExit(ExitEventArgs e)
        {
            TextLog.Instance.Dispose();
            
            base.OnExit(e);
        }
    }
}
