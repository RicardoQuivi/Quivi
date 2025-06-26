namespace Quivi.Printer.Service
{
    public static class DeviceIdManager
    {
        private static readonly string FilePath = Path.Combine(AppContext.BaseDirectory, "device.id");

        public static string GetOrCreateDeviceId()
        {
            if (File.Exists(FilePath))
                return File.ReadAllText(FilePath).Trim();

            var id = Guid.NewGuid().ToString();
            File.WriteAllText(FilePath, id);
            return id;
        }
    }
}