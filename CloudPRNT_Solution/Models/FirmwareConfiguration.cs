using System;
using System.Collections.Generic;

public class FirmwareConfiguration
{
    public string Title { get; } = "star_configuration";
    public string Version { get; } = "1.4.0";
    public string PrintWhenCompletedType { get; } = "ascii";
    public string PrintWhenCompleted { get; } = "Completed";
    public List<FirmwareDetails> Firmwares { get; }

    public FirmwareConfiguration()
    {
        Firmwares = new List<FirmwareDetails>
        {
            new FirmwareDetails()
        };
    }
}

public class FirmwareDetails
{
    public string Action { get; } = "force";
    public string DeviceName { get; } = "mC-Label3";
    public string PathType { get; } = "url";
    
    // Define a constant path for now
    public string Path { get; } = "http://192.168.1.100:7148/mC-Label3_Ver250212a(MAIN).bin";
    
    public string PrintType { get; } = "ascii";
    public string PrintBeforeWriting { get; } = "Start Firmware Update";
    public string SelftestAfterWriting { get; } = "version";
}
