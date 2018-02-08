# TODO

* ~~Integrate HidGuardian. Whitelist cleanup, remove old exclusive mode
workaround, write template AffectedDevices file~~
* Perform some final cleanup and release version 1.5
* ~~Experiment with ViGEm driver. This would be easier to test on my old
C++ test application before attempting to work it into DS4Windows.~~
* Attempt to work out BT disconnect issues by looking at older versions
* Attempt to remove reliance on the main thread when disconnecting a device.
Currently used to delay hotplug routine
* Possibly lower Enhanced Precision sensitivity by a notch
* Look into distributing profile properties around various objects
rather than using a lot of getters to obtain properties each poll.
It will complicate the architecture a little bit but hopefully
any speed difference will make up for it.
* Remove old welcome dialog and make new driver installer executable.
Use newer standards (WPF) and bundle app with DS4Windows

