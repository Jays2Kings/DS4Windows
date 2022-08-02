using DS4Windows;
using System.Windows;

namespace DS4WinWPF.DS4Forms
{
    /// <summary>
    /// Interaction logic for About.xaml
    /// </summary>
    public partial class About : Window
    {
        public About()
        {
            InitializeComponent();

            string version = Global.exeversion;
            headerLb.Content += version + ")";
        }

        private void ChangeLogLink_Click(object sender, RoutedEventArgs e)
        {
            Util.StartProcessHelper("https://docs.google.com/document/d/1CovpH08fbPSXrC6TmEprzgPwCe0tTjQ_HTFfDotpmxk/edit?usp=sharing");
        }

        private void PaypalLink_Click(object sender, RoutedEventArgs e)
        {
            Util.StartProcessHelper("https://paypal.me/ryochan7");
        }

        private void PatreonLink_Click(object sender, RoutedEventArgs e)
        {
            Util.StartProcessHelper("https://patreon.com/user?u=501036");
        }

        private void SubscribeStartLink_Click(object sender, RoutedEventArgs e)
        {
            Util.StartProcessHelper("https://subscribestar.com/ryochan7");
        }

        private void SiteLink_Click(object sender, RoutedEventArgs e)
        {
            Util.StartProcessHelper("https://ryochan7.github.io/ds4windows-site/");
        }

        private void SourceLink_Click(object sender, RoutedEventArgs e)
        {
            Util.StartProcessHelper("https://github.com/Ryochan7/DS4Windows");
        }

        private void Jays2KingsLink_Click(object sender, RoutedEventArgs e)
        {
            Util.StartProcessHelper("https://github.com/Jays2Kings/");
        }

        private void InhexSTERLink_Click(object sender, RoutedEventArgs e)
        {
            Util.StartProcessHelper("https://code.google.com/p/ds4-tool/");
        }

        private void ElectrobrainsLink_Click(object sender, RoutedEventArgs e)
        {
            Util.StartProcessHelper("https://code.google.com/r/brianfundakowskifeldman-ds4windows/");
        }

        private void YoutubeSocialBtn_Click(object sender, RoutedEventArgs e)
        {
            Util.StartProcessHelper("https://www.youtube.com/channel/UCIoUA_XLlCSZbvZGeg3Byeg");
        }

        private void BittubeSocialBtn_Click(object sender, RoutedEventArgs e)
        {
            Util.StartProcessHelper("https://bittube.tv/profile/ds4windows");
        }

        private void TwitterSocialBtn_Click(object sender, RoutedEventArgs e)
        {
            Util.StartProcessHelper("https://twitter.com/ds4windows");
        }

        private void GithubSocialBtn_Click(object sender, RoutedEventArgs e)
        {
            Util.StartProcessHelper("https://github.com/Ryochan7/DS4Windows");
        }

        private void ViGEmBusLink_Click(object sender, RoutedEventArgs e)
        {
            Util.StartProcessHelper("https://vigem.org/");
        }

        private void HidHideLink_Click(object sender, RoutedEventArgs e)
        {
            Util.StartProcessHelper("https://vigem.org/projects/HidHide/");
        }

        private void Crc32Link_Click(object sender, RoutedEventArgs e)
        {
            Util.StartProcessHelper("https://github.com/dariogriffo/Crc32");
        }

        private void OneEuroLink_Click(object sender, RoutedEventArgs e)
        {
            Util.StartProcessHelper("http://cristal.univ-lille.fr/~casiez/1euro/");
        }

        private void FakerInputLink_Click(object sender, RoutedEventArgs e)
        {
            Util.StartProcessHelper("https://github.com/Ryochan7/FakerInput/");
        }

        private void HNotifyIconLink_Click(object sender, RoutedEventArgs e)
        {
            Util.StartProcessHelper("https://github.com/HavenDV/H.NotifyIcon/");
        }
    }

    public class AboutImgPathLocations
    {
        private string gitHubImg =
            $"{Global.RESOURCES_PREFIX}/social/GitHub-Mark-64px.png";
        public string GitHubImg { get => gitHubImg; }

        public AboutImgPathLocations()
        {
            App current = App.Current as App;
            if (current != null)
            {
                PopulateFromAppResources(current);
            }
        }

        private void PopulateFromAppResources(App currentApp)
        {
            gitHubImg = $"{Global.RESOURCES_PREFIX}/social/{currentApp.FindResource("GitHubImg")}";
        }
    }
}
