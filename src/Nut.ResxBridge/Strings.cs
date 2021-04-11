using System;
using System.Globalization;
using System.Resources;

namespace Nut.ResxBridge {

    internal static class Strings {

        private static ResourceManager? resourceMan;

        internal static ResourceManager ResourceManager
        {
            get
            {
                if(resourceMan is null)
                {
                    resourceMan = new ResourceManager("Nut.ResxBridge.Strings", typeof(Strings).Assembly);
                }
                return resourceMan;
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to {0}. に類似しているローカライズされた文字列を検索します。
        /// </summary>
        internal static string XmlDocCommentSumary(string text) => string.Format(ResourceManager.GetString("XmlDocCommentSumary"), text);
    }
}
