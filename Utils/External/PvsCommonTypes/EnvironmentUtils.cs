using Microsoft.Win32;
using System;
using System.IO;
using System.Threading;
using System.Text.RegularExpressions;
using System.Security.Cryptography;
using System.Text;

namespace ProgramVerificationSystems.PVSStudio
{
    public delegate void Action();
}

namespace ProgramVerificationSystems.PVSStudio.CommonTypes
{
    public class DevenvRegPaths
    {
        public const string VisualStudio2005 = @"HKEY_LOCAL_MACHINE\Software\Wow6432Node\Microsoft\VisualStudio\8.0";
        public const string VisualStudio2008 = @"HKEY_LOCAL_MACHINE\Software\Wow6432Node\Microsoft\VisualStudio\9.0";
        public const string VisualStudio2010 = @"HKEY_LOCAL_MACHINE\Software\Wow6432Node\Microsoft\VisualStudio\10.0";
    }

    class EnvironmentUtils
    {
        public const String fileNameSettings = "Settings.xml";
        public const String MSVCRegistryPath = "HKEY_LOCAL_MACHINE\\Software\\Microsoft\\VisualStudio\\{0}\\Setup\\VC";
        public const String MSVCRegistryKey = "ProductDir";
        public static String[] SupportedMSVC = { "8.0", "9.0", "10.0", "11.0", "12.0", "14.0" };

        public static void ExecuteSynchronousOperationThroughMutex(Action action, String mutexName)
        {
            using (Mutex mutex = new Mutex(false, mutexName))
            {
                bool isReleased = false;
                while (!isReleased)
                {
                    try
                    {
                        if (mutex.WaitOne())
                        {
                            isReleased = true;
                            //Do sync work
                            action();
                            //end sync work
                        }
                    }
                    catch (AbandonedMutexException)
                    {
                        //Mutex was abandoned, owner process had not exited correctly, retrying...
                        isReleased = false;
                    }
                    finally
                    {
                        // Whether or not the exception was thrown, the current
                        // thread owns the mutexes, and must release them.
                        mutex.ReleaseMutex();
                    }
                }
            }
        }

        public static String GetAppName()
        {
            String appName = "PVS-Studio";
            return appName;
        }

        public static String GetCurrentUserPVSDataFolder()
        {
            String appDataDir = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            String fullDirectoryName = appDataDir + Path.DirectorySeparatorChar + GetAppName();
            return fullDirectoryName;
        }

        public static String GetPVSStudioExePath()
        {
            String arch = Environment.GetEnvironmentVariable("PROCESSOR_ARCHITECTURE",
                EnvironmentVariableTarget.Machine).ToLower();
            String archPath = "x86";
            if (arch == "x86")
            {
                // use default
            }
            else if (arch == "amd64")
            {
                archPath = "x64";
            }
            else
            {
                // use default
            }

            String program = GetModuleDirectory() + Path.DirectorySeparatorChar +
                archPath + Path.DirectorySeparatorChar + "PVS-Studio.exe";
            return program;
        }

        const String PVSRegistryKeyName = @"\ProgramVerificationSystems\PVS-Studio";

        /// <summary>
        /// Получение пути к ключу реестра PVS-Studio в HKLM. Запись в HKLM требует элевации прав.
        /// </summary>
        /// <returns>Путь к ключу PVS-Studio в HKLM</returns>
        public static String GetMachineLevelRegistryPath()
        {
            if (IntPtr.Size == 8)
                //мы - 64 битный процесс. Инсталятор же всегда работает в 32-битном режиме.
                return @"HKEY_LOCAL_MACHINE\SOFTWARE\Wow6432Node" + PVSRegistryKeyName;
            else
                return @"HKEY_LOCAL_MACHINE\SOFTWARE" + PVSRegistryKeyName;
        }

        /// <summary>
        /// Получение пути к ключу реестра PVS-Studio в HKCU.
        /// </summary>
        /// <returns>Путь к ключу PVS-Studio в HKCU</returns>
        public static String GetUserLevelRegistryPath()
        {
            return @"HKEY_CURRENT_USER\SOFTWARE" + PVSRegistryKeyName;
        }

        private static object _moduleDirectoryLocker = new object();

        private static String _modulePath = String.Empty;
        public static String GetModuleDirectory()
        {
            lock (_moduleDirectoryLocker)
            {
                String DefaultPVSDir = Environment.ExpandEnvironmentVariables("%ProgramFiles(x86)%") + Path.DirectorySeparatorChar + "PVS-Studio";
                try
                {
                    if (!String.IsNullOrEmpty(_modulePath))
                        return _modulePath;


                    String PVSDir = (String)Registry.GetValue(GetMachineLevelRegistryPath(), "InstallDir", DefaultPVSDir);
                    if (String.IsNullOrEmpty(PVSDir))
                    {
                        _modulePath = DefaultPVSDir;
                        return DefaultPVSDir;
                    }
                    else
                    {
                        _modulePath = PVSDir;
                        return PVSDir;
                    }
                }
                catch (Exception)
                {
                    // ProcessInternalError commented for stoping static-fication of all code.
                    //ProcessInternalError(ex);
                    _modulePath = DefaultPVSDir;
                    return DefaultPVSDir;
                }
            }
        }

        public static string ConvertPathMasksToRegexMask(string fileMask)
        {
            string pattern = string.Empty;
            pattern += "^";
            foreach (char symbol in fileMask)
                switch (symbol)
                {
                    case '\\': pattern += @"\\"; break;
                    case '.': pattern += @"\."; break;
                    case '?': pattern += @"."; break;
                    case '*': pattern += @".*"; break;
                    default: pattern += symbol; break;
                }
            pattern += "$";
            return pattern;
        }
    }

    public static class StringExtensions
    {
        public static String GetMD5(this String input)
        {
            byte[] encodedPassword = new UTF8Encoding().GetBytes(input);

            // need MD5 to calculate the hash
            byte[] hash = ((HashAlgorithm)CryptoConfig.CreateFromName("MD5")).ComputeHash(encodedPassword);

            // string representation (similar to UNIX format)
            return BitConverter.ToString(hash)
                               // without dashes
                               .Replace("-", string.Empty)
                               // make lowercase
                               .ToLower();
        }
    }
}