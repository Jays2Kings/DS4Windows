namespace DS4Windows
{
    public class HidDeviceAttributes
    {
        private int vendorId;
        private int productId;
        private int version;
        private string vendorHexId;
        private string productHexId;

        internal HidDeviceAttributes(NativeMethods.HIDD_ATTRIBUTES attributes)
        {
            vendorId = attributes.VendorID;
            productId = attributes.ProductID;
            version = attributes.VersionNumber;

            vendorHexId = "0x" + attributes.VendorID.ToString("X4");
            productHexId = "0x" + attributes.ProductID.ToString("X4");
        }

        public int VendorId { get => vendorId; private set => vendorId = value; }
        public int ProductId { get => productId; private set => productId = value; }
        public int Version { get => version; private set => version = value; }
        public string VendorHexId { get => vendorHexId; set => vendorHexId = value; }
        public string ProductHexId { get => productHexId; set => productHexId = value; }
    }
}
