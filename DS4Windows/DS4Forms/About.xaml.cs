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

        private void BitchuteSocialBtn_Click(object sender, RoutedEventArgs e)
        {
            Util.StartProcessHelper("https://www.bitchute.com/channel/uE2CbiV96u1k/");
        }

        private void BittubeSocialBtn_Click(object sender, RoutedEventArgs e)
        {
            Util.StartProcessHelper("https://bittube.tv/profile/ds4windows");
        }

        private void TwitterSocialBtn_Click(object sender, RoutedEventArgs e)
        {
            Util.StartProcessHelper("https://twitter.com/ds4windows");
        }

        private void MastodonSocialBtn_Click(object sender, RoutedEventArgs e)
        {
            Util.StartProcessHelper("https://fosstodon.org/@ds4windows");
        }

        private void MindsSocialBtn_Click(object sender, RoutedEventArgs e)
        {
            Util.StartProcessHelper("https://www.minds.com/ds4windows/");
        }

        private void DiscordSocialBtn_Click(object sender, RoutedEventArgs e)
        {
            Util.StartProcessHelper("https://discord.gg/zrpPgyN");
        }
    }
}
