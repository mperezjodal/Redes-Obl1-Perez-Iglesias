namespace FileStreamLibrary.Protocol
{
    public static class Specification
    {
        public const int FixedFileNameLength = 4;
        public const int FixedFileSizeLength = 8;
        public const int MaxPacketSize = 320000; // 32KB

        public static long GetParts(long fileSize)
        {
            var parts = fileSize / Specification.MaxPacketSize;
            return parts * Specification.MaxPacketSize == fileSize ? parts : parts + 1;
        }
    }
}