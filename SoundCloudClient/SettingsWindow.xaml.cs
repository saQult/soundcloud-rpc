using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace SoundCloudClient
{
    /// <summary>
    /// Логика взаимодействия для SettingsWindow.xaml
    /// </summary>
    public partial class SettingsWindow : Window
    {
        private Config _newConfig;
        private Config _oldConfig;
        public Action<Config>? SettingsChanged;
        public SettingsWindow()
        {
            InitializeComponent();
            _newConfig = Config.Load();
            _oldConfig = Config.Load();

            proxySettingsInput.Text = _newConfig.ProxySettings;
            enableAddblockCheckbox.IsChecked = _newConfig.EnableAdBlock;
            enableDiscordRpcCheckbox.IsChecked = _newConfig.EnableDiscordRPC;
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            if(_newConfig.Equals(_oldConfig) == false)
            {
                _newConfig.Save();
                SettingsChanged?.Invoke(_newConfig);
                Close();
            }
            else
                Close();
        }

        private void DragWindow(object sender, MouseButtonEventArgs e)
        {
            try
            {
                DragMove();
            }
            catch { }
        }

        private void enableDiscordRpcCheckbox_Checked(object sender, RoutedEventArgs e) =>
            _newConfig.EnableDiscordRPC = (bool)enableDiscordRpcCheckbox.IsChecked!;

        private void enableAddblockCheckbox_Checked(object sender, RoutedEventArgs e) =>
            _newConfig.EnableAdBlock = (bool)enableAddblockCheckbox.IsChecked!;

        private void proxySettingsInput_TextChanged(object sender, TextChangedEventArgs e) =>
            _newConfig.ProxySettings = proxySettingsInput.Text;
    }
}
