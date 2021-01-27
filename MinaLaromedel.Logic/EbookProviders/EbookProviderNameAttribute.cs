using System;

namespace MinaLaromedel.EbookProviders
{
    internal class EbookProviderNameAttribute : Attribute
    {
        public EbookProviderNameAttribute(string name)
        {
            Name = name;
        }

        public string Name { get; }
    }
}