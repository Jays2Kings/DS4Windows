## User Guide
After completing the initial setup of the required drivers and dependencies, you are greeted with the main DS4Windows tab, controllers.

### Controllers
![Controller Tab](https://user-images.githubusercontent.com/32114370/189562818-8e2d5e0a-3c61-4eb0-a53c-caa731b120e4.png)

Here all of the conected controllers are shown. Hovering your mouse over the Controller's ID will display the input delay of the specificed controller.

![Input Delay Screenshot](https://user-images.githubusercontent.com/32114370/189563985-e18c074e-3caf-49a7-af36-ed12d77f88c6.png)

The Link Profile/ID checkbox allows you to link a specified profile to a certain Controller ID so that when you plug in the controller next time the Selected Profile will always be applied to it. This is helpful if you use more than one controller. The edit button will take you to the Profiles tab to edit the Selected Profile. You can also use the dropdown to create a new profile.

### Profiles

![Profile Tab Screenshot](https://user-images.githubusercontent.com/32114370/189564567-f73805f8-e16a-4a0a-a7ca-619509b10b56.png)

The **Profiles** tab displays all the profiles created. Profiles can be used to assign different settings for your controller for different circumstances. Along with creating new profiles, editing, renaming, deleting, and renaming, you can also import other profiles, and export your's for sharing with friends. When creating a new profile, it is recommended to use a preset option. For the output method, it must be chosen accordingly to what you want Windows to recognize the controller as.

For Example:

- You have a Pro Controller and want to use PS Remote Play with gyro? You need to choose DualShock 4 Output and adjust the profile for Gyro passthrough
- You have any of the supported controllers and want to play Celest, which only supports XInput devices? Set the Output to Xbox 360
- You have a fake DS4 controller that is not recognized as an official one, but want to play Witcher 3 with lightbar support and PS glyphs/icons? Then choose DualShock 4 Output and adjust for lightbar passthrough

![Output Controller Prompt Screenshot](https://user-images.githubusercontent.com/32114370/189565494-3bd6b11f-7298-4180-824e-7cde49daebb7.png)

On the resulting screen is where you can fully customize the new profile.

<img src="https://user-images.githubusercontent.com/32114370/189565801-485819c1-cfdc-4aca-8b28-1a91b925c5d9.png" width=75% height=75%>

The **Controls** tab is for remapping the controller button outputs. To open the page below, click on the desired button to remap on the image of the controller. Then select the desired new output for that input.

<img src="https://user-images.githubusercontent.com/32114370/189566012-a734210b-05e6-45f7-a1f8-8a1d5f71514f.png" width=75% height=75%>

The **Special Actions** tab allows you to create actions that are triggered when a button or combination of button presses occur.

The **Controller Readings** tab gives a live readout of the data from the joysticks being transmitted to DS4Windows

The **Axis Config** tab allows you to adjust the settings of the joysticks and adjust parameters such as deadzone and sensativity. 

The **Lightbar** tab allows you to change the color of the lighbar on DualShock 4 controllers.

The **Touchpad** tab allows configuration of the touchpad on DualShock 4 controllers to be output as mouse or controller movement.

The **Gyro** tab contains the gyro settings and allows you to assign specfic commands to certain tiling actions.

The **Other** tab contains the settings for which controller is being emulated, rumble percentage, and the polling rate.

### Auto Profiles
![Auto Profiles Screenshot](https://user-images.githubusercontent.com/32114370/189568215-1fa93173-7982-4c5a-9b36-1deda15ce6a3.png)

**Auto Profiles** allows you to assign certain profiles to a specified application. This allows you to use different settings, controls, and mappings for different applications.

### Output Slots
![Output Slots Screenshot](https://user-images.githubusercontent.com/32114370/189568564-b46a38b2-f492-43a4-bd20-f2171edc7b0c.png)

**Output Slots** shows which controllers that are connected are designated to the 8 slots that DS4Windows allows to be plugged in at one time. Here you can also select a controller and virtually plug and unplug it in.

### Settings
![Settings Screenshot](https://user-images.githubusercontent.com/75176311/232901882-6e271499-cdff-4f93-bfa5-921205c7fb69.PNG)

The **Settings** tab is where the settings for the DS4Windows application are. Options such as *Run at Startup*, *Start Minimized*, or *Show Notifications* live here.

**Disconenct from BT when Stopping** - Stops the bluetooth connection to the controllers when DS4Windows is quit

**Flash Lightbar at High Latency** - Flashes the DualShock 4's lightbar red when DS4Windows detects high input latency

**Quick Change** - Auto disables bluetooth when connecting a controller via USB

**Icon Choice** - Changes the Icon of the DS4Windows application

**App Theme** - Switch DS4Windows to Light or Dark mode

**External OSC Control & Monitoring** - Use Open Sound Control messages to remotely control and monitor buttons, sticks, triggers, and battery levels

**UDP Server** - Setting for connecting the motion controls of a compatible controller to another program

### Log

The **Log** tab is where you can look at all of the events that the DS4Windows application has encountered. There is also a button to export the log for debugging purposes.

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