using System.ComponentModel;
using System.Configuration;
using System.Data;
using System.Windows;
using Bredinin.AlloyEditor.Common.Desktop;

namespace Bredinin.AlloyEditor.Desktop.App
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : PrismApplication
    {
        protected override void RegisterTypes(IContainerRegistry containerRegistry)
        {
            containerRegistry.AddApiServices("localhost:53253");
        }

        protected override Window CreateShell()
        {
            return Container.Resolve<MainWindow>();
        }
    }

}
