﻿using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace TradeSharp.Client.BL
{
    public class FingerPrint
    {
        private static FingerPrint instance;
        public static FingerPrint Instance
        {
            get { return instance ?? (instance = new FingerPrint()); }
        }

        public readonly long fingerPrintLong;
        public readonly string fingerPrintString;
        private string fingerPrintHash = string.Empty;
        public string FingerPrintHash
        {
            get
            {
                return !string.IsNullOrEmpty(fingerPrintHash)
                           ? fingerPrintHash
                           : fingerPrintHash = GetHash(fingerPrintString);
            }
        }

        private FingerPrint()
        {
            fingerPrintString = "CPU >> " + CpuId() + "\nBIOS >> " +
                              BiosId() + /*"\nBASE >> " + BaseId() +
                              +"\nDISK >> "+ diskId() + "\nVIDEO >> " + 
                              /*VideoId() + */"\nMAC >> " + MacId();
            fingerPrintLong = fingerPrintString.GetHashCode();
        }
        
        private static string GetHash(string s)
        {
            MD5 sec = new MD5CryptoServiceProvider();
            var enc = new ASCIIEncoding();
            byte[] bt = enc.GetBytes(s);
            return GetHexString(sec.ComputeHash(bt));
        }
        
        private static string GetHexString(byte[] bt)
        {
            var s = string.Empty;
            for (int i = 0; i < bt.Length; i++)
            {
                byte b = bt[i];
                int n, n1, n2;
                n = b;
                n1 = n & 15;
                n2 = (n >> 4) & 15;
                if (n2 > 9)
                    s += ((char)(n2 - 10 + 'A')).ToString();
                else
                    s += n2.ToString();
                if (n1 > 9)
                    s += ((char)(n1 - 10 + 'A')).ToString();
                else
                    s += n1.ToString();
                if ((i + 1) != bt.Length && (i + 1) % 2 == 0) s += "-";
            }
            return s;
        }
        
        #region Original Device ID Getting Code
        
        //Return a hardware identifier
        private static string Identifier(string wmiClass, string wmiProperty, string wmiMustBeTrue)
        {
            string result = "";
            var mc = new System.Management.ManagementClass(wmiClass);
            System.Management.ManagementObjectCollection moc = mc.GetInstances();
            foreach (System.Management.ManagementObject mo in moc)
            {
                if (mo[wmiMustBeTrue].ToString() == "True")
                {
                    //Only get the first one
                    if (result == "")
                    {
                        try
                        {
                            result = mo[wmiProperty].ToString();
                            break;
                        }
                        catch
                        {
                        }
                    }
                }
            }
            return result;
        }
        
        //Return a hardware identifier
        private static string Identifier(string wmiClass, string wmiProperty)
        {
            var result = "";
            var mc = new System.Management.ManagementClass(wmiClass);
            System.Management.ManagementObjectCollection moc = mc.GetInstances();
            foreach (System.Management.ManagementObject mo in moc)
            {
                //Only get the first one
                if (result == "")
                {
                    try
                    {
                        result = mo[wmiProperty].ToString();
                        break;
                    }
                    catch
                    {
                    }
                }
            }
            return result;
        }

        private static string CpuId()
        {
            //Uses first CPU identifier available in order of preference
            //Don't get all identifiers, as it is very time consuming
            string retVal = Identifier("Win32_Processor", "UniqueId");
            if (retVal == "") //If no UniqueID, use ProcessorID
            {
                retVal = Identifier("Win32_Processor", "ProcessorId");
                if (retVal == "") //If no ProcessorId, use Name
                {
                    retVal = Identifier("Win32_Processor", "Name");
                    if (retVal == "") //If no Name, use Manufacturer
                    {
                        retVal = Identifier("Win32_Processor", "Manufacturer");
                    }
                    //Add clock speed for extra security
                    retVal += Identifier("Win32_Processor", "MaxClockSpeed");
                }
            }
            return retVal;
        }

        //BIOS Identifier
        private static string BiosId()
        {
            return Identifier("Win32_BIOS", "Manufacturer")
            + Identifier("Win32_BIOS", "SMBIOSBIOSVersion")
            + Identifier("Win32_BIOS", "IdentificationCode")
            + Identifier("Win32_BIOS", "SerialNumber")
            + Identifier("Win32_BIOS", "ReleaseDate")
            + Identifier("Win32_BIOS", "Version");
        }

        //Main physical hard drive ID
        private static string DiskId()
        {
            return Identifier("Win32_DiskDrive", "Model")
            + Identifier("Win32_DiskDrive", "Manufacturer")
            + Identifier("Win32_DiskDrive", "Signature")
            + Identifier("Win32_DiskDrive", "TotalHeads");
        }

        //Motherboard ID
        private static string BaseId()
        {
            return Identifier("Win32_BaseBoard", "Model")
            + Identifier("Win32_BaseBoard", "Manufacturer")
            + Identifier("Win32_BaseBoard", "Name")
            + Identifier("Win32_BaseBoard", "SerialNumber");
        }

        //Primary video controller ID
        private static string VideoId()
        {
            return Identifier("Win32_VideoController", "DriverVersion")
            + Identifier("Win32_VideoController", "Name");
        }

        //First enabled network card ID
        private static string MacId()
        {
            return Identifier("Win32_NetworkAdapterConfiguration",
                "MACAddress", "IPEnabled");
        }
        #endregion
    }

    public class ExecutableVersion
    {
        private static ExecutableVersion instance;
        public static ExecutableVersion Instance
        {
            get { return instance ?? (instance = new ExecutableVersion()); }
        }

        public readonly long executableVersion;

        private ExecutableVersion()
        {
            var sm = System.Reflection.Assembly.GetEntryAssembly();
            executableVersion = File.GetLastWriteTimeUtc(sm.Location).Ticks;
        }
    }
}