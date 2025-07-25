﻿using Quivi.Printer.MassTransit.Configurations;

namespace Quivi.Hangfire.Settings
{
    public class PrinterConnectorSettings : IPrinterRabbitMqSettings
    {
        public required string Host { get; init; }
        public int Port { get; init; }
        public required string Username { get; init; }
        public required string Password { get; init; }
    }
}