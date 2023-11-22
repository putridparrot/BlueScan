using Acr.UserDialogs;
using BlueScan.Services;
using BlueScan.Views;
using Bluetooth.Service;
using Prism;
using Prism.DryIoc;
using Prism.Ioc;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

[assembly: XamlCompilation(XamlCompilationOptions.Compile)]
namespace BlueScan
{
    public partial class App : PrismApplication
    {
        public App() :
            this(null) { }

        public App(IPlatformInitializer initializer) :
            base(initializer) { }

        protected override void RegisterTypes(IContainerRegistry containerRegistry)
        {
            containerRegistry.RegisterForNavigation<NavigationPage>();
            containerRegistry.RegisterForNavigation<MainPage>();
            containerRegistry.RegisterForNavigation<OptionsPage>();
            containerRegistry.RegisterForNavigation<AboutPage>();
            containerRegistry.RegisterForNavigation<MorePage>();
            containerRegistry.RegisterForNavigation<HistoryPage>();
            containerRegistry.RegisterForNavigation<ServicesPage>();

            containerRegistry.RegisterSingleton<IBluetoothScanner, BluetoothScanner>();
            containerRegistry.RegisterSingleton<IOptions, Options>();
            containerRegistry.RegisterSingleton<IPersist, Persist>();
            containerRegistry.RegisterInstance(typeof(IUserDialogs), UserDialogs.Instance);
        }

        protected override void OnStart()
        {
            // Handle when your app starts
            base.OnStart();

            var persist = Container.Resolve<IPersist>();
            // fire and forget as we don't want to block OnStart
            persist?.SynchronizeAsync();
        }

        protected override void OnSleep()
        {
            // Handle when your app sleeps
            base.OnSleep();
        }

        protected override async void OnInitialized()
        {
            InitializeComponent();

            await NavigationService.NavigateAsync("NavigationPage/MainPage");
        }

        protected override void OnResume()
        {
            // Handle when your app resumes
            base.OnResume();
        }
    }
}
