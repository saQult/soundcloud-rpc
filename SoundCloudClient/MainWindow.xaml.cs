using Microsoft.Web.WebView2.Core;
using Microsoft.Web.WebView2.Wpf;
using Newtonsoft.Json;
using SoundCloudClient.Discord;
using System.IO;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media.Effects;
using System.Windows.Threading;


namespace SoundCloudClient
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private string[] _extensions = [];
        private Config _config;
        private DiscordRPCService _discordRPCService = new();
        private DispatcherTimer _timer;
        public MainWindow()
        {
            InitializeComponent();

            _config = Config.Load();

            Loaded += async (_, _ ) => await InitializeWebViewAsync();

            _timer = new DispatcherTimer
            {
                Interval = TimeSpan.FromSeconds(5)
            };
            _timer.Tick += async (_, _) =>
            {
                try
                {
                    await UpdatePresenceAsync();
                }
                catch { }
            };
            _timer.Start();
        }
        private async Task UpdatePresenceAsync()
        {
            if (_config.EnableDiscordRPC == false)
                return;
            var song = await GetSoundCloudSongInfoAsync(webView);
            if (song != null)
            {
                if(song.IsPaused)
                {
                    _discordRPCService.Clear();
                }
                else
                {
                    _discordRPCService.UpdatePresence(song);
                }
            }
        }
        public async Task<SoundCloudSongInfo?> GetSoundCloudSongInfoAsync(WebView2 webView)
        {
            try
            {
                var songName = await EvaluateAsync(webView, "document.querySelector('.playbackSoundBadge__titleLink span[aria-hidden]')?.textContent");
                var songUrl = await EvaluateAsync(webView, "document.querySelector('.playbackSoundBadge__titleLink')?.href");
                var artist = await EvaluateAsync(webView, "document.querySelector('.playbackSoundBadge__lightLink')?.textContent");
                var avatarStyle = await EvaluateAsync(webView, "document.querySelector('.playbackSoundBadge__avatar .image__full')?.style.backgroundImage");
                var timePassedStr = await EvaluateAsync(webView, "document.querySelector('.playbackTimeline__progressWrapper')?.getAttribute('aria-valuenow')");
                var durationText = await EvaluateAsync(webView, "document.querySelector('.playbackTimeline__duration')?.textContent");
                var controlTitle = await EvaluateAsync(webView, "document.querySelector('.playControl')?.title");

                songName = songName?.Trim() ?? "";
                songUrl = songUrl ?? "";
                artist = artist?.Trim() ?? "";
                durationText = durationText?.ToLowerInvariant() ?? "";
                timePassedStr = timePassedStr ?? "0";

                string avatarUrl = "";
                if (!string.IsNullOrEmpty(avatarStyle) && avatarStyle.Contains("url"))
                {
                    var match = Regex.Match(avatarStyle, @"url\(\""(.*?)\""\)");
                    if (match.Success)
                        avatarUrl = match.Groups[1].Value.Replace("50x50", "300x300");
                }

                int duration = ParseDuration(durationText);
                int.TryParse(timePassedStr, out int timePassed);
                bool isPaused = controlTitle?.ToLower().Contains("play current") ?? false;

                return new SoundCloudSongInfo(
                    songName,
                    avatarUrl,
                    artist,
                    songUrl,
                    timePassed,
                    duration,
                    isPaused
                );
            }
            catch
            {
                return null;
            }
        }
        private async Task<string?> EvaluateAsync(WebView2 webView, string jsExpression)
        {
            var result = await webView.ExecuteScriptAsync(jsExpression);
            if (string.IsNullOrWhiteSpace(result) || result == "null")
                return null;

            return JsonConvert.DeserializeObject<string>(result); 
        }
        private int ParseDuration(string durationText)
        {
            int minutes = 0, seconds = 0;
            var minMatch = Regex.Match(durationText, @"(\d+)\s*min");
            var secMatch = Regex.Match(durationText, @"(\d+)\s*sec");

            if (minMatch.Success)
                minutes = int.Parse(minMatch.Groups[1].Value);
            if (secMatch.Success)
                seconds = int.Parse(secMatch.Groups[1].Value);

            return minutes * 60 + seconds;
        }
        private string[] GetExtensions()
        {
            var extensionsFolder = Path.Combine(Directory.GetCurrentDirectory(), "extensions");
            return Directory.GetDirectories(extensionsFolder);
        }
        private async Task InitializeWebViewAsync()
        {
            string cachePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "WebView2Cache");
            string proxyArgument = "";
            if(String.IsNullOrEmpty(_config.ProxySettings) == false)
            {
                proxyArgument = $"--proxy-server=\"{_config.ProxySettings}\"";
            }

            var env = await CoreWebView2Environment.CreateAsync(
                browserExecutableFolder: null,
                userDataFolder: cachePath,
                options: new CoreWebView2EnvironmentOptions()
                {
                    AreBrowserExtensionsEnabled = true,
                    AdditionalBrowserArguments = proxyArgument
                }
            );
            
            await webView.EnsureCoreWebView2Async(env);

            _extensions = GetExtensions();

            foreach (var extension in _extensions)
            {
                if (extension == "ublock" && _config.EnableAdBlock == false)
                    continue;
                string extensionPath = Path.Combine(Directory.GetCurrentDirectory(), extension);
                await webView.CoreWebView2.Profile.AddBrowserExtensionAsync(extensionPath);
            }    

            webView.Source = new Uri("https://soundcloud.com/");
        }
        private async Task ReinitializeWebViewAsync()
        {
            MainGrid.Children.Remove(webView);

            webView.Dispose();

            await Task.Delay(200); // wait to clear resourses, idk how to write it properly

            webView = new WebView2
            {
                Name = "webView",
                VerticalAlignment = VerticalAlignment.Stretch
            };

            MainGrid.Children.Add(webView); 

            await InitializeWebViewAsync();
        }
        private void CheckForCombinations(object sender, KeyEventArgs e)
        {
            if(e.Key == Key.F1)
            {
                var settings = new SettingsWindow() { Owner = this };
                settings.SettingsChanged += async (config) =>
                {
                    _config = config;
                    await ReinitializeWebViewAsync();
                };
                settings.ShowDialog();
            }
            if ((Keyboard.Modifiers & ModifierKeys.Control) == ModifierKeys.Control)
            {
                if (e.Key == Key.B && webView.CanGoBack)
                {
                    webView.GoBack();
                }
                else if (e.Key == Key.F && webView.CanGoForward)
                {
                    webView.GoForward();
                }
            }
        }
    }
}