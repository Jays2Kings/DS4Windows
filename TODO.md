# TODO

* ~~Integrate HidGuardian. Whitelist cleanup, remove old exclusive mode
workaround, write template AffectedDevices file~~
* Perform some final cleanup and release version 1.5. Unfortunately,
it won't happen in 2017. Maybe version 1.5 will be ready by January 2018.
* ~~Experiment with ViGEm driver. This would be easier to test on my old
C++ test application before attempting to work it into DS4Windows.~~
* Attempt to work out BT disconnect issues by looking at older versions
* Attempt to remove reliance on the main thread when disconnecting a device.
Currently used to delay hotplug routine
* Attempt to remove more unused components
* ~~Tweak layout of some forms~~
* ~~Look into updating HidLibrary~~
* ~~Trim code execution for touchpad data. Remove unneeded new calls for Touch
instances. Make permanent instances and reuse those instances.~~
* ~~Tweak SixAxis code to attempt to improve steering wheel performance
for racing games. It is already a better experience than what the Steam
Controller offers though.~~
* Possibly lower Enhanced Precision sensitivity by a notch
* ~~Look into updating Task Scheduler to latest version~~
* ~~Look into trackball emulation methods and attempt to implement it~~
* Look into distributing profile properties around various objects
rather than using a lot of getters to obtain properties each poll.
It will complicate the architecture a little bit but hopefully
any speed difference will make up for it.
* Remove old welcome dialog and make new driver installer executable.
Use newer standards (WPF) and bundle app with DS4Windows

