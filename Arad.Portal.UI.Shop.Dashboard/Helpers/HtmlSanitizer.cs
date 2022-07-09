using System.Collections.Generic;
using System.IO;
using System.Xml;
using HtmlAgilityPack;

namespace Arad.Portal.UI.Shop.Dashboard.Helpers
{
    public class HtmlSanitizer
    {

        public  HashSet<string> BlackList = new HashSet<string>() 
        {
                { "script" },
                { "iframe" },
                { "form" },
                { "object" },
                { "embed" },
                //{ "link" },
                { "head" },
                { "meta" }
        };

        /// <summary>
        /// check for validation of html string if it does not contains valnerable tags
        /// </summary>
        /// <param name="html"></param>
        /// <returns></returns>
        public static bool SanitizeHtml(string html, params string[] blackList)
        {
            var sanitizer = new HtmlSanitizer();
            if (blackList != null && blackList.Length > 0)
            {
                sanitizer.BlackList.Clear();
                foreach (string item in blackList)
                    sanitizer.BlackList.Add(item);
            }
            return sanitizer.Sanitize(html);
        }

        /// <summary>
        /// Cleans up an HTML string by removing elements
        /// on the blacklist and all elements that start
        /// with onXXX .
        /// </summary>
        /// <param name="html"></param>
        /// <returns></returns>
        public bool Sanitize(string html)
        {
            var doc = new HtmlDocument();

            doc.LoadHtml(html);
            var isValid = SanitizeHtmlNode(doc.DocumentNode);

            
            //string output = null;
            // Use an XmlTextWriter to create self-closing tags
            //using (StringWriter sw = new StringWriter())
            //{
            //    XmlWriter writer = new XmlTextWriter(sw);
            //    doc.DocumentNode.WriteTo(writer);
            //    output = sw.ToString();

            //    // strip off XML doc header
            //    if (!string.IsNullOrEmpty(output))
            //    {
            //        int at = output.IndexOf("?>");
            //        output = output.Substring(at + 2);
            //    }

            //    writer.Close();
            //}
            //doc = null;

            return isValid;
        }

        private bool SanitizeHtmlNode(HtmlNode node)
        {
            bool isValid = true;
            if (node.NodeType == HtmlNodeType.Element)
            {
                if (BlackList.Contains(node.Name))
                {
                    node.Remove();
                    isValid = false;
                    return isValid;
                
                }

                // remove CSS Expressions and embedded script links
                //if (node.Name == "style")
                //{
                //    var val = node.InnerHtml;
                //    if (string.IsNullOrEmpty(node.InnerText))
                //    {
                //        if (HasExpressionLinks(val) || HasScriptLinks(val))
                //        {
                //            node.ParentNode.RemoveChild(node);
                //            isValid = false;
                //            return isValid;
                //        } 
                //    }
                //}

                // remove script attributes
                if (node.HasAttributes)
                {
                    for (int i = node.Attributes.Count - 1; i >= 0; i--)
                    {
                        HtmlAttribute currentAttribute = node.Attributes[i];

                        var attr = currentAttribute.Name.ToLower();
                        var val = currentAttribute.Value.ToLower();

                        if (attr.StartsWith("on") && attr != "onload" && attr == "onclick" && !val.StartsWith("window.open"))
                        {
                            node.Attributes.Remove(currentAttribute);
                            isValid = false;
                            return isValid;

                        }


                        // Remove CSS Expressions
                        else if (/*attr == "style" &&*/
                                 val != null &&
                                 HasExpressionLinks(val) || HasScriptLinks(val))
                        {
                            node.Attributes.Remove(currentAttribute);
                            isValid = false;
                            return isValid;
                        }

                        // remove script links from all attributes
                        else if (
                            //(attr == "href" || attr== "src" || attr == "dynsrc" || attr == "lowsrc") &&
                                 val != null &&
                                 HasScriptLinks(val) )
                        {
                            node.Attributes.Remove(currentAttribute);
                            isValid = false;
                            return isValid;
                        }
                            
                    }
                }
            }

            // Look through child nodes recursively
            if (node.HasChildNodes)
            {
                for (int i = node.ChildNodes.Count - 1; i >= 0; i--)
                {
                   isValid = SanitizeHtmlNode(node.ChildNodes[i]);
                   if(!isValid)
                    {
                        return isValid;
                    }
                }
            }
            return isValid;
        }

        private bool HasScriptLinks(string value)
        {
            return value.Contains("javascript:") || value.Contains("vbscript:");
        }

        private bool HasExpressionLinks(string value)
        {
            return value.Contains("expression");
        }
    }
}