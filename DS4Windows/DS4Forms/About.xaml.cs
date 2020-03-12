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
            Util.StartProcessInExplorer("https://docs.google.com/document/d/1CovpH08fbPSXrC6TmEprzgPwCe0tTjQ_HTFfDotpmxk/edit?usp=sharing");
        }

        private void PaypalLink_Click(object sender, RoutedEventArgs e)
        {
            Util.StartProcessInExplorer("https://paypal.me/ryochan7");
        }

        private void PatreonLink_Click(object sender, RoutedEventArgs e)
        {
            Util.StartProcessInExplorer("https://patreon.com/user?u=501036");
        }

        private void SubscribeStartLink_Click(object sender, RoutedEventArgs e)
        {
            Util.StartProcessInExplorer("https://subscribestar.com/ryochan7");
        }

        private void SiteLink_Click(object sender, RoutedEventArgs e)
        {
            Util.StartProcessInExplorer("https://ryochan7.github.io/ds4windows-site/");
        }

        private void SourceLink_Click(object sender, RoutedEventArgs e)
        {
            Util.StartProcessInExplorer("https://github.com/Ryochan7/DS4Windows");
        }

        private void Jays2KingsLink_Click(object sender, RoutedEventArgs e)
        {
            Util.StartProcessInExplorer("https://github.com/Jays2Kings/");
        }

        private void InhexSTERLink_Click(object sender, RoutedEventArgs e)
        {
            Util.StartProcessInExplorer("https://code.google.com/p/ds4-tool/");
        }

        private void ElectrobrainsLink_Click(object sender, RoutedEventArgs e)
        {
            Util.StartProcessInExplorer("https://code.google.com/r/brianfundakowskifeldman-ds4windows/");
        }
    }
}
