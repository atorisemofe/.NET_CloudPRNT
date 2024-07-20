namespace CloudPRNT_Solution.Models
{
    public class ComninedTableModel
    {
        public IEnumerable<DeviceTable> Devices { get; set; }
        public IEnumerable<LocationTable> Locations { get; set; }
    }
}