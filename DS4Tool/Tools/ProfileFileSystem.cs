using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DS4Windows.Tools
{
    [Flags]
    public enum ProfileDirectories
    {
        None = 0,
        ExecutionPath = 1,
        AppDataPath = 2,
        OldFilePath = 4
    }

    public class ProfileFileSystem
    {
        public string exePath { get; private set; }
        public string appDataPath { get; private set; }
        public string oldFilePath { get; private set; }

        public ProfileFileSystem(string executablePath, string appDataPath, string oldFilePath)
        {
            this.exePath = executablePath;
            this.appDataPath = appDataPath;
            this.oldFilePath = oldFilePath;
        }

        public ProfileDirectories CheckForFileInProfileDirectories(string fileName)
        {
            ProfileDirectories resultDirectories = ProfileDirectories.None;

            if (File.Exists(exePath + fileName))
            {
                resultDirectories |= ProfileDirectories.ExecutionPath;
            }

            if (File.Exists(appDataPath + fileName))
            {
                resultDirectories |= ProfileDirectories.AppDataPath;
            }

            if (File.Exists(this.oldFilePath + fileName))
            {
                resultDirectories |= ProfileDirectories.OldFilePath;
            }

            return resultDirectories;
        }

        public bool DirectoryExists(ProfileDirectories directory)
        {
            string directoryToCheck = null;

            switch(directory)
            {
                case ProfileDirectories.OldFilePath:
                    {
                        directoryToCheck = this.oldFilePath;
                        break;
                    }
                case ProfileDirectories.AppDataPath:
                    {
                        directoryToCheck = this.appDataPath;
                        break;
                    }
                case ProfileDirectories.ExecutionPath:
                    {
                        directoryToCheck = this.exePath;
                        break;
                    }
                default:
                    {
                        throw new Exception("None passed into directory check");
                    }
            }

            return Directory.Exists(directoryToCheck);
        }

        public ProfileDirectories CheckProfileDirectoriesExist()
        {
            ProfileDirectories resultDirectories = ProfileDirectories.None;

            if (Directory.Exists(exePath))
            {
                resultDirectories |= ProfileDirectories.AppDataPath;
            }

            if (Directory.Exists(appDataPath))
            {
                resultDirectories |= ProfileDirectories.ExecutionPath;
            }

            if (Directory.Exists(this.oldFilePath))
            {
                resultDirectories |= ProfileDirectories.OldFilePath;
            }

            return resultDirectories;
        }

        public bool GetProfileExistsInOldLocation(string fileName)
        {
            ProfileDirectories existingProfileLocations = this.CheckForFileInProfileDirectories(fileName);
            return (existingProfileLocations & ProfileDirectories.OldFilePath) == ProfileDirectories.OldFilePath;
        }

        public string GetCurrentSaveLocationDirectory(string fileName)
        {
            return GetDirectoryString(GetCurrentSaveLocation(fileName));
        }

        public string GetDirectoryString(ProfileDirectories directory)
        {
            switch (directory)
            {
                case ProfileDirectories.AppDataPath:
                    {
                        return this.appDataPath;
                    }
                case ProfileDirectories.ExecutionPath:
                    {
                        return this.exePath;
                    }
                case ProfileDirectories.OldFilePath:
                    {
                        return this.oldFilePath;
                    }
                default:
                    {
                        throw new Exception("There is not valid save location. Please make sure you call this function after checking for valid locations");
                    }
            }
        }

        public ProfileDirectories GetCurrentSaveLocation(string fileName)
        {
            ProfileDirectories existingProfileLocations = this.CheckForFileInProfileDirectories(fileName);

            if (existingProfileLocations.HasFlag(ProfileDirectories.ExecutionPath) &&
                existingProfileLocations.HasFlag(ProfileDirectories.AppDataPath))
            {
                return ProfileDirectories.None;
            }
            else if (existingProfileLocations.HasFlag(ProfileDirectories.ExecutionPath))
            {
                return ProfileDirectories.ExecutionPath;
            }
            else if (existingProfileLocations.HasFlag(ProfileDirectories.AppDataPath))
            {
                return ProfileDirectories.AppDataPath;
            }
            else if (!existingProfileLocations.HasFlag(ProfileDirectories.ExecutionPath)
                && !existingProfileLocations.HasFlag(ProfileDirectories.AppDataPath))
            {
                return ProfileDirectories.None;
            }

            return ProfileDirectories.None;
        }

        public bool TryCreateDictory(ProfileDirectories directory)
        {
            string location = GetDirectoryString(directory);
            try
            {
                Directory.CreateDirectory(location);
                return true;
            }
            catch 
            {
                return false;
            }
        }

        public bool MigrateToNewFileLocation(string fileName, ProfileDirectories oldLocation, ProfileDirectories newLocation )
        {
            string moveToLocation = GetDirectoryString(newLocation);
            string currentLocation = GetDirectoryString(oldLocation);

            if (!Directory.Exists(moveToLocation))
            {
                bool directoryCreationSucceeded = TryCreateDictory(newLocation);
                if (!directoryCreationSucceeded)
                {
                    return false;
                }
            }

            try
            {
                Directory.Move(currentLocation, moveToLocation);
                return true;
            }
            catch
            {
                return false;
            }

            return false;
        }

        public bool FileSystemIsValid(string fileName)
        {
            ProfileDirectories existingProfileLocations = this.CheckForFileInProfileDirectories(fileName);

            if (existingProfileLocations.HasFlag(ProfileDirectories.ExecutionPath) &&
                existingProfileLocations.HasFlag(ProfileDirectories.AppDataPath))
            {
                return false;
            }
            else if (existingProfileLocations.HasFlag(ProfileDirectories.ExecutionPath))
            {
                return true;
            }
            else if (existingProfileLocations.HasFlag(ProfileDirectories.AppDataPath))
            {
                return true;
            }
            else if (!existingProfileLocations.HasFlag(ProfileDirectories.ExecutionPath)
                && !existingProfileLocations.HasFlag(ProfileDirectories.AppDataPath))
            {
                return true;
            }

            return false;
        }

    }
}
