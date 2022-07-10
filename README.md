# DS4Windows

Like those other DS4 tools, but sexier.

DS4Windows is an extract anywhere program that allows you to get the best
DualShock 4 experience on your PC. By emulating an Xbox 360 controller, many
more games are accessible. Other input controllers are also supported including the
DualSense, Switch Pro, and JoyCon controllers (**first party hardware only**).

This project is a fork of the work of Jays2Kings. You can find the old project
website at [ds4windows.com](http://ds4windows.com).

![DS4Windows Preview](https://raw.githubusercontent.com/Ryochan7/DS4Windows/jay/ds4winwpf_screen_20200412.png)

## Downloads

- **[Main builds of DS4Windows](https://github.com/Ryochan7/DS4Windows/releases)**

## Requirements

- Windows 10 or newer (Thanks Microsoft)
- [Microsoft .NET 6.0 Runtime Desktop](https://dotnet.microsoft.com/en-us/download/dotnet/6.0/runtime)
- Visual C++ 2015-2019 Redistributable. [x64](https://aka.ms/vs/16/release/vc_redist.x64.exe) or [x86](https://aka.ms/vs/16/release/vc_redist.x86.exe)
- [ViGEmBus](https://vigem.org/) driver (DS4Windows will install it for you)
- Microsoft 360 Driver (link inside DS4Windows, already installed by Windows if
you've used a 360 controller before)
- **Sony** DualShock 4 or other supported controller
- Connection method:
  - Micro USB cable
  - [Sony Wireless Adapter](https://www.amazon.com/gp/product/B01KYVLKG2)
  - Bluetooth 4.0 (via an
[adapter like this](https://www.newegg.com/Product/Product.aspx?Item=N82E16833166126)
or built in pc). Only use of Microsoft BT stack is supported. CSR BT stack is
confirmed to not work with the DS4 even though some CSR adapters work fine
using Microsoft BT stack. Toshiba's adapters currently do not work. 
*Disabling 'Enable output data' in the controller profile settings might help with latency issues, but will disable lightbar and rumble support.*
- Disable **PlayStation Configuration Support** and
**Xbox Configuration Support** options in Steam

## Social

[Twitter @ds4windows](https://twitter.com/ds4windows)  
[YouTube](https://www.youtube.com/channel/UCIoUA_XLlCSZbvZGeg3Byeg)  
[BitChute](https://www.bitchute.com/channel/uE2CbiV96u1k/)  
[BitTube.tv](https://bittube.tv/profile/ds4windows)  
[Mastodon @ds4windows@fosstodon.org](https://fosstodon.org/@ds4windows)  
[Minds @ds4windows](https://www.minds.com/ds4windows/)

### Other

[Backloggery](https://backloggery.com/ryochan7)

## Device Detection Issue

If your DS4 is not detected by DS4Windows and the lightbar continues to
flash yellow, there is a chance that Exclusive Mode has permanently
disabled your DS4 in Windows. The easiest way to test if this has happened is
for you to plug in the controller into a different USB port and see if it
works then. Although this problem mainly affected older versions of
DS4Windows (text written after version 1.5.15) for various reasons,
other mapping programs can cause the same problem to occur.

If you suspect that your DS4 has been disabled, open the Device Manager
(Control Panel\Hardware and Sound\Device Manager) and look for devices listed
under the path "Human Interface Devices\HID-compliant game controller".

![Disabled Device Example](https://i.imgur.com/KI3QX2i.png)

If the icon shown for a device has a down arrow icon then you should
check the device's instance path and see if the device is a DualShock 4 device.
Right click the device item and select "Enable device" from the menu.
That will re-enable the device so it can be seen by applications again.

## Disable Steam Controller Mapping Support

With recent updates to the Steam client at the time writing this (2018-12-13),
Steam has enabled Xbox Configuration Support in the Steam client by default.
What this means is that Steam will automatically map a detected Xbox 360
controller to KB+M bindings initially (Desktop Mode) before launching Steam
Big Picture Mode or launching a game. This presents a problem for DS4Windows
since the created virtual Xbox 360 controller will be mapped to KB+M actions
for desktop mode and games launched outside of the Steam client. In order to
use DS4Windows properly, you have to open Steam Big Picture Mode, navigate to
Settings > Controller> Controller Settings and uncheck **Xbox Configuration
Support** along with **PlayStation Configuration Support**.

## Personal Game Testing

My PC game library is not that expansive so there are likely games
that will be tested by users that I will not have access to
play. There are likely going to be times when I cannot directly test
against a game since I will not have access to play it. Most free to play
games or games that include a playable demo should be fine for testing.
For other games, it might be better if people could test against any game
that I have in my library and try to reproduce a problem. Here are
links to my Steam and GOG profiles so that people can see what games I have.

https://steamcommunity.com/id/Ryochan7/games/?tab=all  
https://www.gog.com/u/Ryochan7/games

## Pull Requests

Pull requests for DS4Windows are welcome. Before making a pull request, please
test your changes to ensure that the changes made do not negatively affect
the performance of other parts of the application. Some consideration will
be made during code review to try to tweak the changes in order to improve
application performance. However, there is a chance that a pull request will be
rejected if no reasonable solution can be found to incorporate code changes.

## Tip Jar

If you would like to send some coin my way, here are some means by
which to do so.

**PayPal:** https://paypal.me/ryochan7  
**Patreon:** https://patreon.com/user?u=501036  
**SubscribeStar:** https://subscribestar.com/ryochan7

### Crypto

**Bitcoin:** 1DnMJwjdd7JRfHJap2mmTmADYm38SzR2z9  
**Litecoin:** La5mniW7SFMH2RhqDgUty3RwkBSYbjbnJ6  
**Monero:** 49RvRMiMewaeez1Y2auxHmfMaAUYfhUpBem4ohzRJd9b5acPcxzh1icjnhZfjnYd1S7NQ57reQ7cP1swGre3rpfzUgJhEB7

