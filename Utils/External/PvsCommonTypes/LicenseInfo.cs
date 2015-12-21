using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProgramVerificationSystems.PVSStudio.CommonTypes
{
    public class LicenseReloadedEventArgs : EventArgs
    {
        public LicenseInfo.RegistrationStates NewRegistrationState
        {
            get;
            private set;
        }

        public LicenseReloadedEventArgs(LicenseInfo.RegistrationStates newRegistrationState)
        {
            NewRegistrationState = newRegistrationState;
        }
    }

    public class LicenseInfo
    {
        public enum RegistrationStates
        {
            Trial,
            Invalid,
            Valid,
            Timeout
        }

        public delegate void LicenseReloadedEventHandler(object sender, LicenseReloadedEventArgs e);


        public RegistrationStates RegistrationState = RegistrationStates.Trial;
        public String licenseType = String.Empty;
        public String date = String.Empty;

        private String registrationState = String.Empty;

        public event LicenseReloadedEventHandler LicenseReloaded;

        // Reload() вызывается вручную когда введена новая регистрационная информация 
        // и требуется обновить значения полей (дата, тип лицензии, состояние);
        // Также Reload() вызывается при создании объекта (при старте плагина).
        public void Reload()
        {
            GetLicenseInfo(out registrationState, out licenseType, out date);
            if (registrationState == "trial")
                RegistrationState = RegistrationStates.Trial;
            else if (registrationState == "invalid")
                RegistrationState = RegistrationStates.Invalid;
            else if (registrationState == "valid")
                RegistrationState = RegistrationStates.Valid;
            else if (registrationState == "timeout")
                RegistrationState = RegistrationStates.Timeout;
            else
                RegistrationState = RegistrationStates.Trial;

            OnLicenseReloaded();
        }

        private void OnLicenseReloaded()
        {
            LicenseReloadedEventHandler handler = LicenseReloaded;
            if (handler != null)
                handler(this, new LicenseReloadedEventArgs(RegistrationState));
        }

        private bool GetLicenseInfo(out String registrationState,
                                    out String licenseType,
                                    out String date)
        {
            System.Diagnostics.Process launcher = new System.Diagnostics.Process();
            launcher.StartInfo.FileName = EnvironmentUtils.GetPVSStudioExePath();
            launcher.StartInfo.Arguments = "--checkreg=yes";
            launcher.StartInfo.UseShellExecute = false;
            launcher.StartInfo.RedirectStandardError = true;
            launcher.StartInfo.RedirectStandardOutput = true;
            launcher.StartInfo.CreateNoWindow = true;
            launcher.EnableRaisingEvents = true;
            launcher.Start();
            String strStandardOutput = launcher.StandardOutput.ReadToEnd();
            String strStandardError = launcher.StandardError.ReadToEnd();
            launcher.WaitForExit();

            char sep = '\n';
            String[] stdError = strStandardError.Split(sep);
            String[] stdOut = strStandardOutput.Split(sep);
            if (stdOut.Length == 4)
            {
                registrationState = stdOut[0].TrimEnd();
                licenseType = stdOut[1].TrimEnd();
                date = stdOut[2].TrimEnd();
                return true;
            }

            registrationState = licenseType = date = String.Empty;
            return false;
        }
    }
}
