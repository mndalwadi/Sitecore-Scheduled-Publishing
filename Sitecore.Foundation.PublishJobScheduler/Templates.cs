using Sitecore.Configuration;
using Sitecore.Data;

namespace Sitecore.Foundation.PublishJobScheduler
{
    public class Templates
    {
        public struct PublishCronJobFolder
        {
            public static ID ID = new ID("{B38EC7C1-008C-4BDB-93E5-F8F62D3ED390}");
        }

        public struct PublishCronJob
        {
            public static ID ID = new ID("{4DF4F7A3-18EA-41F7-9D98-36C8044B815A}");

            public struct Fields
            {
                public static readonly ID Items = new ID("{225906C3-FB5F-4CAF-869D-4500106E2024}");
                public static readonly ID PublishWithSubItems = new ID("{E81F6A7C-53E0-4D3F-9496-8E6576A5A839}");
                public static readonly ID ExecuteExactlyAtDateTime = new ID("{AAA75B67-8603-4753-A452-374D1D80B5E2}");
                public static readonly ID PublishingTarget = new ID("{B58ABE34-26EC-4131-91FE-3AC27B8ADB0E}");
                public static readonly ID Language = new ID("{96150398-F323-47D7-BDE5-34FF5AB8AA01}");
                public static readonly ID Inactive = new ID("{2FB5D780-9DAF-4571-BF03-3313E6DF02B0}");
                public static readonly ID AutoRemove = new ID("{06101DC6-556E-4745-8E7C-25DE422CDCBD}");
                public static readonly ID JobDone = new ID("{DDA4984D-4CC3-46BB-9895-F2BFFF54E6B0}");
            }
        }

        public struct PublishingTarget
        {
            public static ID ID = new ID("{E130C748-C13B-40D5-B6C6-4B150DC3FAB3}");

            public struct Fields
            {
                public static readonly ID TargetDatabase = new ID("{39ECFD90-55D2-49D8-B513-99D15573DE41}");
                public static readonly ID PreviewPublishingTarget = new ID("{17394C5A-35A3-45A3-BF2C-F1AC607D1476}");
            }
        }

        public struct SystemModuleItems
        {
            public static readonly ID PublishCronJob = new ID("{AD647E58-30D4-42E8-803E-E57956ED4A40}");
        }
    }
}