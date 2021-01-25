using System;

namespace Liber.Onlinebok
{
    public class LiberOnlinebokDocumentAttachment : ILiberOnlinebokUuid
    {
        public string Type { get; set; }
        public Guid Uuid { get; set; }
        public string SubType { get; set; }
        public string Provider { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public int ContentItemOrderingIndexRef { get; set; }
        public LiberOnlinebokEbookAttachmentBehavior Behavior { get; set; }
        public Uri IconUrl { get; set; }
        public LiberOnlinebokEbookAttachmentMedium[] Media { get; set; }
    }

    public class LiberOnlinebokEbookAttachmentBehavior
    {
        public string Type { get; set; }
        public object Properties { get; set; }
    }

    public class LiberOnlinebokEbookAttachmentMedium
    {
        public Uri Url { get; set; }
        public string MimeType { get; set; }
        public int DurationMillis { get; set; }
    }
}