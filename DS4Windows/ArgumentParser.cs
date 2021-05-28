
using System.Collections.Generic;
using DS4Windows.DS4Control;

namespace DS4WinWPF
{
    public class ArgumentParser
    {
        private bool mini;
        private bool stop;
        private bool driverinstall;
        private bool reenableDevice;
        private string deviceInstanceId;
        private bool runtask;
        private bool command;
        private string commandArgs;
        private string virtualkbmHandler = VirtualKBMFactory.DEFAULT_IDENTIFIER;

        private Dictionary<string, string> errors =
            new Dictionary<string, string>();

        public bool Mini { get => mini; }
        public bool Stop { get => stop; }
        public bool Driverinstall { get => driverinstall; }
        public bool ReenableDevice { get => reenableDevice; }
        public bool Runtask { get => runtask; }
        public bool Command { get => command; }
        public string DeviceInstanceId { get => deviceInstanceId; }
        public string CommandArgs { get => commandArgs; }
        public string VirtualkbmHandler { get => virtualkbmHandler; }
        public Dictionary<string, string> Errors { get => errors; }

        public bool HasErrors => errors.Count > 0;

        public void Parse(string[] args)
        {
            errors.Clear();
            //foreach (string arg in args)
            for (int i = 0; i < args.Length; i++)
            {
                string arg = args[i];
                switch(arg)
                {
                    case "driverinstall":
                    case "-driverinstall":
                        driverinstall = true;
                        break;

                    case "re-enabledevice":
                    case "-re-enabledevice":
                        reenableDevice = true;
                        if (i + 1 < args.Length)
                        {
                            deviceInstanceId = args[++i];
                        }

                        break;

                    case "runtask":
                    case "-runtask":
                        runtask = true;
                        break;

                    case "-stop":
                        stop = true;
                        break;

                    case "-m":
                        mini = true;
                        break;

                    case "command":
                    case "-command":
                        command = true;
                        if (i + 1 < args.Length)
                        {
                            i++;
                            string temp = args[i];
                            if (temp.Length > 0 && temp.Length <= 256)
                            {
                                commandArgs = temp;
                            }
                            else
                            {
                                command = false;
                                errors["Command"] = "Command length is invalid";
                            }
                        }
                        else
                        {
                            errors["Command"] = "Command string not given";
                        }
                        break;
                    case "-virtualkbm":
                        if (i + 1 < args.Length)
                        {
                            i++;
                            string temp = args[i];
                            bool valid = VirtualKBMFactory.IsValidHandler(temp);
                            if (valid)
                            {
                                virtualkbmHandler = temp;
                            }
                        }

                        break;

                    default: break;
                }
            }
        }
    }
}
