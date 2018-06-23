# DS4Windows

Like those other ds4tools, but sexier.

DS4Windows is an extract anywhere program that allows you to get the best DualShock 4 experience on your PC. By emulating a Xbox 360 controller, many more games are accessible.

You can find the latest and older versions [here](https://github.com/Ryochan7/DS4Windows/releases).

UdpServer builds for using Gyro motion controls in Cemu.

http://ryochan7.xyz/ds4windows/test/DS4Windows_1.4.120_UdpServer_x64.zip
http://ryochan7.xyz/ds4windows/test/DS4Windows_1.4.120_ViGEm_UdpServer_x64.zip

ViGEm build.

http://ryochan7.xyz/ds4windows/test/DS4Windows_1.4.120_ViGEm_x64.zip

This project is a fork of the work of Jays2Kings. You can find the old project
website at [ds4windows.com](http://ds4windows.com).

## Requirements

- Windows 7 or newer
- [Microsoft .NET 4.5.2 or higher (needed to unzip the driver and for macros to work properly)](http://www.microsoft.com/en-us/download/details.aspx?id=42642)
- SCP Virtual Bus Driver (Downloaded & Installed with DS4Windows)
- Microsoft 360 Driver (link inside DS4Windows, already installed on Windows 7 SP1 and higher or if you've used a 360 controller before)
- Sony DualShock 4 (This should be obvious)
- Micro USB cable
- (Optional)Bluetooth 2.1+, via adapter or built in pc [(My recommendation)](https://www.newegg.com/Product/Product.aspx?Item=N82E16833166126) (Toshiba's bluetooth adapters currently do not work)

## Device Detection Issue

If your DS4 is not detected by DS4Windows and the lightbar continues to
flash yellow, there is a chance that Exclusive Mode has permanently
disabled your DS4 in Windows. The easiest way to test if this has happened is
for you to plug in the controller into a different USB port and see if it
works then. Although this problem mainly affected versions of
DS4Windows prior to 1.4.109 when using some applications, other mapping
programs can cause the same problem to occur.


If you suspect that your DS4 has been disabled, open the Device Manager
(Control Panel\Hardware and Sound\Device Manager) and look for devices listed
under the path "Human Interface Devices\HID-compliant game controller".

![Disabled Device Example](https://raw.githubusercontent.com/Ryochan7/DS4Windows/jay/disabled_device_example_small.png)

If the icon shown for a device has a down arrow icon then you should
check the device's device instance path and see if the device is a
DualShock 4 device. Right click the device item and select "Enable device"
from the menu. That will re-enable the device and be seen by applications
again.

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

Bitcoin: 1DnMJwjdd7JRfHJap2mmTmADYm38SzR2z9  
Dogecoin: D9fhbXp9bCHEhuS8vX1BmVu6t7Y2nVNUCK  
Litecoin: La5mniW7SFMH2RhqDgUty3RwkBSYbjbnJ6  
Monero: 49RvRMiMewaeez1Y2auxHmfMaAUYfhUpBem4ohzRJd9b5acPcxzh1icjnhZfjnYd1S7NQ57reQ7cP1swGre3rpfzUgJhEB7  
PayPal: https://paypal.me/ryochan7

