using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Liber.Onlinebok
{

    public class LiberOnlinebokDocument
    {
        public LiberOnlinebokDocumentStructure Structure { get; set; }
        public LiberOnlinebokDocumentContent Content { get; set; }
        public LiberOnlinebokDocumentProperties Properties { get; set; }
    }

    public class LiberOnlinebokDocumentStructure : ILiberOnlinebokUuid
    {
        public Guid Uuid { get; set; }
        public LiberOnlinebokDocumentStructureRoot Root { get; set; }
    }

    public class LiberOnlinebokDocumentStructureRoot : ILiberOnlinebokUuid
    {
        public Guid Uuid { get; set; }
        public string Type { get; set; }
        public Dictionary<string, LiberOnlinebokDocumentStructureRootChild> Children { get; set; }
        public bool ShowToc { get; set; }
    }

    public class LiberOnlinebokDocumentStructureRootChild : ILiberOnlinebokUuid
    {
        public Guid Uuid { get; set; }
        public string Type { get; set; }
        public Guid ContentId { get; set; }
        public string Label { get; set; }
        public bool ShowToc { get; set; }
    }

    public class LiberOnlinebokDocumentContent
    {
        public Dictionary<string, LiberOnlinebokDocumentContentContentItem> ContentItems { get; set; }
        public LiberOnlinebokDocumentContentAsset AssetPkg { get; set; }
        public LiberOnlinebokDocumentContentAsset CoverImage { get; set; }
        public LiberOnlinebokDocumentContentAsset CoverImageSmall { get; set; }
    }

    public class LiberOnlinebokDocumentContentContentItem : ILiberOnlinebokUuid
    {
        public Guid Uuid { get; set; }
        public string Html { get; set; }
        public string ContentLabel { get; set; }
        public int OrderingIndex { get; set; }
        public LiberOnlinebokDocumentContentAsset[] Assets { get; set; }
        public LiberOnlinebokDocumentContentContentItemSearchFields SearchFields { get; set; }
    }

    public class LiberOnlinebokDocumentContentContentItemSearchFields
    {
        public string Text { get; set; }
    }

    public class LiberOnlinebokDocumentContentAsset
    {
        /// <summary>
        /// E.g. "assets/img/layout/7.jpg" for a page.
        /// </summary>
        public Uri Uri { get; set; }
    }

    public class LiberOnlinebokDocumentProperties : ILiberOnlinebokUuid
    {
        public string Isbn { get; set; }
        public Guid Uuid { get; set; }
        public string Name { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public bool Printable { get; set; }
        public int TimeStamp { get; set; }
        public string WrappingDataType { get; set; }
        public string DocumentType { get; set; }
        public float PreferredWidth { get; set; }
        public float PreferredHeight { get; set; }
        public LiberOnlinebokDocumentPropertiesComponentBehaviors ComponentBehaviors { get; set; }
    }

    public class LiberOnlinebokDocumentPropertiesComponentBehaviors
    {
        public string[] TextField { get; set; }
        public string[] Level { get; set; }
        public string[] TextArea { get; set; }
        public string[] RadioButton { get; set; }
        public string[] Illustration { get; set; }
        public string[] Checkbox { get; set; }
        public string[] Drawbox { get; set; }
    }
}
