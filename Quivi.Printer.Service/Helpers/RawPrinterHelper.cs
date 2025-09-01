using System.Management;
using System.Runtime.InteropServices;

namespace Quivi.Printer.Service.Helpers
{
    public class RawPrinterHelper
    {
        const string ESC_POS_DOCUMENT = "Esc POS Document";

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
        public class DOCINFOA
        {
            [MarshalAs(UnmanagedType.LPStr)] public string pDocName;
            [MarshalAs(UnmanagedType.LPStr)] public string pOutputFile;
            [MarshalAs(UnmanagedType.LPStr)] public string pDataType;
        }

        #region Declaration Dll

        [DllImport("winspool.Drv", EntryPoint = "OpenPrinterA", SetLastError = true, CharSet = CharSet.Ansi,
            ExactSpelling = true, CallingConvention = CallingConvention.StdCall)]
        public static extern bool OpenPrinter([MarshalAs(UnmanagedType.LPStr)] string szPrinter, out IntPtr hPrinter,
            IntPtr pd);

        [DllImport("winspool.Drv", EntryPoint = "ClosePrinter", SetLastError = true, ExactSpelling = true,
            CallingConvention = CallingConvention.StdCall)]
        public static extern bool ClosePrinter(IntPtr hPrinter);

        [DllImport("winspool.Drv", EntryPoint = "StartDocPrinterA", SetLastError = true, CharSet = CharSet.Ansi,
            ExactSpelling = true, CallingConvention = CallingConvention.StdCall)]
        public static extern bool StartDocPrinter(IntPtr hPrinter, int level,
            [In][MarshalAs(UnmanagedType.LPStruct)] DOCINFOA di);

        [DllImport("winspool.Drv", EntryPoint = "EndDocPrinter", SetLastError = true, ExactSpelling = true,
            CallingConvention = CallingConvention.StdCall)]
        public static extern bool EndDocPrinter(IntPtr hPrinter);

        [DllImport("winspool.Drv", EntryPoint = "StartPagePrinter", SetLastError = true, ExactSpelling = true,
            CallingConvention = CallingConvention.StdCall)]
        public static extern bool StartPagePrinter(IntPtr hPrinter);

        [DllImport("winspool.Drv", EntryPoint = "EndPagePrinter", SetLastError = true, ExactSpelling = true,
            CallingConvention = CallingConvention.StdCall)]
        public static extern bool EndPagePrinter(IntPtr hPrinter);

        [DllImport("winspool.Drv", EntryPoint = "WritePrinter", SetLastError = true, ExactSpelling = true,
            CallingConvention = CallingConvention.StdCall)]
        public static extern bool WritePrinter(IntPtr hPrinter, IntPtr pBytes, int dwCount, out int dwWritten);


        [DllImport("winspool.drv", SetLastError = true, CharSet = CharSet.Auto)]
        public static extern bool GetPrinter(IntPtr hPrinter, uint level, IntPtr pPrinter, uint cbBuf, ref uint pcbNeeded);
        #endregion

        #region Methods

        public static PrinterStatusResponse GetPrinterStatus(string printerName)
        {
            try
            {
                var enqueuedJobs =
                     new ManagementObjectSearcher($"SELECT * FROM Win32_PrintJob WHERE Name like '{printerName},%'")?
                     .Get()?
                     .Cast<ManagementObject>()?
                     .Any() ?? false;

                if (!enqueuedJobs)
                    SendBytesToPrinter(printerName, Array.Empty<byte>(), out string error);

                string query = $"SELECT * FROM Win32_Printer WHERE Name = '{printerName}'";
                ManagementObjectSearcher searcher = new ManagementObjectSearcher(query);

                ManagementObject? printer = searcher.Get().Cast<ManagementObject>().FirstOrDefault();

                if (printer == null)
                    return new PrinterStatusResponse
                    {
                        IsOnline = false,
                        Log = "Cant find printer",
                    };

                var worksOffline = printer["WorkOffline"]?.ToString()?.ToLower() == "true";

                var statusParsed = Enum.TryParse<PrinterStatus>(printer.GetPropertyValue("PrinterStatus").ToString(), true, out PrinterStatus printerStatus);

                var printerOnline = statusParsed && new[]
                {
                    PrinterStatus.Idle,
                    PrinterStatus.Printing,
                    PrinterStatus.WarmUp
                }
                .Contains(printerStatus);

                return new PrinterStatusResponse
                {
                    IsOnline = printerOnline,
                    IsAvailableOffline = worksOffline,
                };
            }
            catch (Exception e)
            {
                return new PrinterStatusResponse
                {
                    IsOnline = false,
                    IsAvailableOffline = false,
                    Log = $"Cant find printer: {e.Message} {e.StackTrace}",
                };
            }
        }

        // SendBytesToPrinter()
        // When the function is given a printer name and an unmanaged array
        // of bytes, the function sends those bytes to the printer queue.
        // Returns true on success, false on failure.
        public static bool SendBytesToPrinter(string szPrinterName, IntPtr pBytes, int dwCount, out string error)
        {
            error = "";
            int dwWritten = 0;
            var hPrinter = new IntPtr(0);
            var di = new DOCINFOA();
            var bSuccess = false;

            di.pDocName = ESC_POS_DOCUMENT;
            di.pDataType = "RAW";

            if (OpenPrinter(szPrinterName.Normalize(), out hPrinter, IntPtr.Zero))
            {
                if (StartDocPrinter(hPrinter, 1, di))
                {
                    if (StartPagePrinter(hPrinter))
                    {
                        bSuccess = WritePrinter(hPrinter, pBytes, dwCount, out dwWritten);
                        EndPagePrinter(hPrinter);
                    }
                    EndDocPrinter(hPrinter);
                }
                ClosePrinter(hPrinter);
            }

            if (bSuccess == false)
                error = new System.ComponentModel.Win32Exception(Marshal.GetLastWin32Error()).Message;

            return bSuccess;
        }

        public static bool SendBytesToPrinter(string szPrinterName, byte[] data, out string error)
        {
            var pUnmanagedBytes = Marshal.AllocCoTaskMem(data.Length);
            Marshal.Copy(data, 0, pUnmanagedBytes, data.Length);
            var retval = SendBytesToPrinter(szPrinterName, pUnmanagedBytes, data.Length, out error);
            Marshal.FreeCoTaskMem(pUnmanagedBytes);

            return retval;
        }
        #endregion
    }

    public class PrinterStatusResponse
    {
        public bool IsOnline { get; set; }
        public bool IsAvailableOffline { get; set; }
        public string? Log { get; set; }
    }

    public enum PrinterStatus
    {
        Other = 1,
        Unknown = 2,
        Idle = 3,
        Printing = 4,
        WarmUp = 5,
        StoppedPrinting = 6,
        Offline = 7,
        Paused = 8,
        Error = 9,
        Busy = 10,
        NotAvailable = 11,
        Waiting = 12,
        Processing = 13,
        Initialization = 14,
        WarmingUp = 15,
        TonerLow = 16,
        NoToner = 17,
        PagePunt = 18,
        UserInterventionRequired = 19,
        OutOfMemory = 20,
        DoorOpen = 21,
        ServerUnknown = 22,
        PowerSave = 23
    }
}