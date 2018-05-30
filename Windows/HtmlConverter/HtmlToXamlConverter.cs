using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Documents;
using System.Xml;

namespace Decomp.Windows.HtmlConverter
{
	public static class HtmlToXamlConverter
	{
		public static string ConvertHtmlToXaml(string htmlString, bool asFlowDocument)
		{
			var htmlElement = HtmlParser.ParseHtml(htmlString);

			var rootElementName = asFlowDocument ? XamlFlowDocument : XamlSection;

			var xamlTree = new XmlDocument();
			var xamlFlowDocumentElement = xamlTree.CreateElement(null, rootElementName, XamlNamespace);

			var stylesheet = new CssStylesheet(htmlElement);
			var sourceContext = new List<XmlElement>(10);

			_inlineFragmentParentElement = null;

			AddBlock(xamlFlowDocumentElement, htmlElement, new Hashtable(), stylesheet, sourceContext);

			if (!asFlowDocument) xamlFlowDocumentElement = ExtractInlineFragment(xamlFlowDocumentElement);

			xamlFlowDocumentElement.SetAttribute("xml:space", "preserve");
			var xaml = xamlFlowDocumentElement.OuterXml;

			return xaml;
		}

		public static string GetAttribute(XmlElement element, string attributeName)
		{
			attributeName = attributeName.ToLower();

			for (var i = 0; i < element.Attributes.Count; i++)
			{
				if (element.Attributes[i].Name.ToLower() == attributeName) return element.Attributes[i].Value;
			}

			return null;
		}

		internal static string UnQuote(string value)
		{
			if (value.StartsWith("\"") && value.EndsWith("\"") || value.StartsWith("'") && value.EndsWith("'")) value = value.Substring(1, value.Length - 2).Trim();
			return value;
		}

		private static XmlNode AddBlock(XmlElement xamlParentElement, XmlNode htmlNode, IDictionary inheritedProperties, CssStylesheet stylesheet, List<XmlElement> sourceContext)
		{
		    if (htmlNode is XmlComment comment)
				DefineInlineFragmentParent(comment, null);
			else if (htmlNode is XmlText)
				htmlNode = AddImplicitParagraph(xamlParentElement, htmlNode, inheritedProperties, stylesheet, sourceContext);
			else if (htmlNode is XmlElement htmlElement)
            {
                var htmlElementName = htmlElement.LocalName;
                var htmlElementNamespace = htmlElement.NamespaceURI;

                if (htmlElementNamespace != HtmlParser.XhtmlNamespace) return htmlElement;

                sourceContext.Add(htmlElement);

                htmlElementName = htmlElementName.ToLower();

                switch (htmlElementName)
                {
                    case "html":
                    case "body":
                    case "div":
                    case "form":
                    case "pre":
                    case "blockquote":
                    case "caption":
                    case "center":
                    case "cite":
                        AddSection(xamlParentElement, htmlElement, inheritedProperties, stylesheet, sourceContext);
                        break;

                    case "p":
                    case "h1":
                    case "h2":
                    case "h3":
                    case "h4":
                    case "h5":
                    case "h6":
                    case "nsrtitle":
                    case "textarea":
                    case "dd":
                    case "dl":
                    case "dt":
                    case "tt":
                        AddParagraph(xamlParentElement, htmlElement, inheritedProperties, stylesheet, sourceContext);
                        break;

                    case "ol":
                    case "ul":
                    case "dir":
                    case "menu":
                        AddList(xamlParentElement, htmlElement, inheritedProperties, stylesheet, sourceContext);
                        break;
                    case "li":
                        htmlNode = AddOrphanListItems(xamlParentElement, htmlElement, inheritedProperties, stylesheet, sourceContext);
                        break;

                    case "img":
                        //AddImage(xamlParentElement, htmlElement, inheritedProperties, stylesheet, sourceContext);
                        break;

                    case "table":
                        AddTable(xamlParentElement, htmlElement, inheritedProperties, stylesheet, sourceContext);
                        break;

                    case "tbody":
                    case "tfoot":
                    case "thead":
                    case "tr":
                    case "td":
                    case "th":
                        goto default;

                    case "style":
                    case "meta":
                    case "head":
                    case "title":
                    case "script":
                        break;

                    default:
                        htmlNode = AddImplicitParagraph(xamlParentElement, htmlElement, inheritedProperties, stylesheet, sourceContext);
                        break;
                }

                sourceContext.RemoveAt(sourceContext.Count - 1);
            }

            return htmlNode;
		}

		private static void AddBreak(XmlNode xamlParentElement, string htmlElementName)
		{
		    if (xamlParentElement.OwnerDocument == null) return;

		    var xamlLineBreak = xamlParentElement.OwnerDocument.CreateElement(XamlLineBreak, XamlNamespace);
		    xamlParentElement.AppendChild(xamlLineBreak);
		    if (htmlElementName != "hr") return;

            if (xamlParentElement.OwnerDocument != null)
		    {
		        var xamlHorizontalLine = xamlParentElement.OwnerDocument.CreateTextNode("----------------------");
		        xamlParentElement.AppendChild(xamlHorizontalLine);
		    }
		    if (xamlParentElement.OwnerDocument != null) xamlLineBreak = xamlParentElement.OwnerDocument.CreateElement(
		        XamlLineBreak, XamlNamespace);
		    xamlParentElement.AppendChild(xamlLineBreak);
		}

		private static void AddSection(XmlElement xamlParentElement, XmlElement htmlElement, IDictionary inheritedProperties, CssStylesheet stylesheet, List<XmlElement> sourceContext)
		{
			var htmlElementContainsBlocks = false;
			for (var htmlChildNode = htmlElement.FirstChild; htmlChildNode != null; htmlChildNode = htmlChildNode.NextSibling)
			{
			    if (!(htmlChildNode is XmlElement element)) continue;
			    var htmlChildName = element.LocalName.ToLower();
			    if (!HtmlSchema.IsBlockElement(htmlChildName)) continue;
			    htmlElementContainsBlocks = true;
			    break;
			}

			if (!htmlElementContainsBlocks)
				AddParagraph(xamlParentElement, htmlElement, inheritedProperties, stylesheet, sourceContext);
			else
			{
			    var currentProperties = GetElementProperties(htmlElement, inheritedProperties, out var localProperties, stylesheet, sourceContext);

			    if (xamlParentElement.OwnerDocument == null) return;

			    var xamlElement = xamlParentElement.OwnerDocument.CreateElement(XamlSection, XamlNamespace);
			    ApplyLocalProperties(xamlElement, localProperties, true);

			    if (!xamlElement.HasAttributes) xamlElement = xamlParentElement;

			    for (var htmlChildNode = htmlElement.FirstChild; htmlChildNode != null; htmlChildNode = htmlChildNode?.NextSibling)
			        htmlChildNode = AddBlock(xamlElement, htmlChildNode, currentProperties, stylesheet, sourceContext);

			    if (xamlElement != xamlParentElement) xamlParentElement.AppendChild(xamlElement);
			}
		}

		private static void AddParagraph(XmlNode xamlParentElement, XmlElement htmlElement, IDictionary inheritedProperties, CssStylesheet stylesheet, List<XmlElement> sourceContext)
		{
		    var currentProperties = GetElementProperties(htmlElement, inheritedProperties, out var localProperties, stylesheet, sourceContext);

		    if (xamlParentElement.OwnerDocument == null) return;

		    var xamlElement = xamlParentElement.OwnerDocument.CreateElement(XamlParagraph, XamlNamespace);
		    ApplyLocalProperties(xamlElement, localProperties, true);

		    for (var htmlChildNode = htmlElement.FirstChild; htmlChildNode != null; htmlChildNode = htmlChildNode.NextSibling)
		        AddInline(xamlElement, htmlChildNode, currentProperties, stylesheet, sourceContext);

		    xamlParentElement.AppendChild(xamlElement);
		}

		private static XmlNode AddImplicitParagraph(XmlNode xamlParentElement, XmlNode htmlNode, IDictionary inheritedProperties, CssStylesheet stylesheet, List<XmlElement> sourceContext)
		{
		    if (xamlParentElement.OwnerDocument == null) return null;
		    var xamlParagraph = xamlParentElement.OwnerDocument.CreateElement(XamlParagraph, XamlNamespace);
		    XmlNode lastNodeProcessed = null;
		    while (htmlNode != null)
		    {
		        if (htmlNode is XmlComment comment)
		            DefineInlineFragmentParent(comment, null);
		        else if (htmlNode is XmlText)
		        {
		            if (htmlNode.Value.Trim().Length > 0) AddTextRun(xamlParagraph, htmlNode.Value);
		        }
		        else if (htmlNode is XmlElement)
		        {
		            var htmlChildName = ((XmlElement)htmlNode).LocalName.ToLower();
		            if (HtmlSchema.IsBlockElement(htmlChildName)) break;
		            AddInline(xamlParagraph, (XmlElement)htmlNode, inheritedProperties, stylesheet, sourceContext);
		        }

		        lastNodeProcessed = htmlNode;
		        htmlNode = htmlNode.NextSibling;
		    }

		    if (xamlParagraph.FirstChild != null) xamlParentElement.AppendChild(xamlParagraph);

		    return lastNodeProcessed;
		}

		private static void AddInline(XmlElement xamlParentElement, XmlNode htmlNode, IDictionary inheritedProperties, CssStylesheet stylesheet, List<XmlElement> sourceContext)
		{
		    if (htmlNode is XmlComment comment)
				DefineInlineFragmentParent(comment, xamlParentElement);
			else if (htmlNode is XmlText)
				AddTextRun(xamlParentElement, htmlNode.Value);
			else if (htmlNode is XmlElement htmlElement)
            {
                if (htmlElement.NamespaceURI != HtmlParser.XhtmlNamespace) return;

                var htmlElementName = htmlElement.LocalName.ToLower();

                sourceContext.Add(htmlElement);

                switch (htmlElementName)
                {
                    case "a":
                        AddHyperlink(xamlParentElement, htmlElement, inheritedProperties, stylesheet, sourceContext);
                        break;
                    case "img":
                        //AddImage(xamlParentElement, htmlElement, inheritedProperties, stylesheet, sourceContext);
                        break;
                    case "br":
                    case "hr":
                        AddBreak(xamlParentElement, htmlElementName);
                        break;
                    default:
                        if (HtmlSchema.IsInlineElement(htmlElementName) || HtmlSchema.IsBlockElement(htmlElementName)) AddSpanOrRun(xamlParentElement, htmlElement, inheritedProperties, stylesheet, sourceContext);
                        break;
                }

                sourceContext.RemoveAt(sourceContext.Count - 1);
            }
        }

		private static void AddSpanOrRun(XmlNode xamlParentElement, XmlElement htmlElement, IDictionary inheritedProperties, CssStylesheet stylesheet, List<XmlElement> sourceContext)
		{
			var elementHasChildren = false;
			for (var htmlNode = htmlElement.FirstChild; htmlNode != null; htmlNode = htmlNode.NextSibling)
			{
			    if (!(htmlNode is XmlElement node)) continue;
			    var htmlChildName = node.LocalName.ToLower();
			    if (!HtmlSchema.IsInlineElement(htmlChildName) && !HtmlSchema.IsBlockElement(htmlChildName) &&
			        htmlChildName != "img" && htmlChildName != "br" && htmlChildName != "hr") continue;
			    elementHasChildren = true;
			    break;
			}

			var xamlElementName = elementHasChildren ? XamlSpan : XamlRun;

		    var currentProperties = GetElementProperties(htmlElement, inheritedProperties, out var localProperties, stylesheet, sourceContext);

		    if (xamlParentElement.OwnerDocument == null) return;
		    var xamlElement = xamlParentElement.OwnerDocument.CreateElement(xamlElementName, XamlNamespace);
		    ApplyLocalProperties(xamlElement, localProperties, false);

		    for (var htmlChildNode = htmlElement.FirstChild; htmlChildNode != null; htmlChildNode = htmlChildNode.NextSibling)
		        AddInline(xamlElement, htmlChildNode, currentProperties, stylesheet, sourceContext);

		    xamlParentElement.AppendChild(xamlElement);
		}

		private static void AddTextRun(XmlNode xamlElement, string textData)
		{
			for (var i = 0; i < textData.Length; i++)
			{
				if (Char.IsControl(textData[i])) textData = textData.Remove(i--, 1);
			}

			textData = textData.Replace((char)160, ' ');

		    if (textData.Length <= 0) return;
		    if (xamlElement.OwnerDocument != null) xamlElement.AppendChild(xamlElement.OwnerDocument.CreateTextNode(textData));
		}

		private static void AddHyperlink(XmlNode xamlParentElement, XmlElement htmlElement, IDictionary inheritedProperties, CssStylesheet stylesheet, List<XmlElement> sourceContext)
		{
			var href = GetAttribute(htmlElement, "href");
			if (href == null)
				AddSpanOrRun(xamlParentElement, htmlElement, inheritedProperties, stylesheet, sourceContext);
			else
			{
			    var currentProperties = GetElementProperties(htmlElement, inheritedProperties, out var localProperties, stylesheet, sourceContext);

			    if (xamlParentElement.OwnerDocument == null) return;

			    var xamlElement = xamlParentElement.OwnerDocument.CreateElement(XamlHyperlink, XamlNamespace);
			    ApplyLocalProperties(xamlElement, localProperties, false);

			    var hrefParts = href.Split('#');
			    if (hrefParts.Length > 0 && hrefParts[0].Trim().Length > 0) xamlElement.SetAttribute(XamlHyperlinkNavigateUri, hrefParts[0].Trim());
			    if (hrefParts.Length == 2 && hrefParts[1].Trim().Length > 0) xamlElement.SetAttribute(XamlHyperlinkTargetName, hrefParts[1].Trim());

			    for (var htmlChildNode = htmlElement.FirstChild; htmlChildNode != null; htmlChildNode = htmlChildNode.NextSibling)
			        AddInline(xamlElement, htmlChildNode, currentProperties, stylesheet, sourceContext);

			    xamlParentElement.AppendChild(xamlElement);
			}
		}

		private static XmlElement _inlineFragmentParentElement;

		private static void DefineInlineFragmentParent(XmlNode htmlComment, XmlElement xamlParentElement)
		{
		    switch (htmlComment.Value)
		    {
		        case "StartFragment":
		            _inlineFragmentParentElement = xamlParentElement;
		            break;
		        case "EndFragment":
		            if (_inlineFragmentParentElement == null && xamlParentElement != null) _inlineFragmentParentElement = xamlParentElement;
		            break;
		    }
		}

		private static XmlElement ExtractInlineFragment(XmlElement xamlFlowDocumentElement)
		{
		    if (_inlineFragmentParentElement == null) return xamlFlowDocumentElement;

            if (_inlineFragmentParentElement.LocalName == XamlSpan)
		        xamlFlowDocumentElement = _inlineFragmentParentElement;
		    else
		    {
		        if (xamlFlowDocumentElement.OwnerDocument != null) xamlFlowDocumentElement = xamlFlowDocumentElement.OwnerDocument.CreateElement(
		            null, XamlSpan, XamlNamespace);
		        while (_inlineFragmentParentElement.FirstChild != null)
		        {
		            var copyNode = _inlineFragmentParentElement.FirstChild;
		            _inlineFragmentParentElement.RemoveChild(copyNode);
		            xamlFlowDocumentElement.AppendChild(copyNode);
		        }
		    }

		    return xamlFlowDocumentElement;
		}

		//private static void AddImage(XmlElement xamlParentElement, XmlElement htmlElement, Hashtable inheritedProperties, CssStylesheet stylesheet, List<XmlElement> sourceContext)
		//{
		//}

		private static void AddList(XmlNode xamlParentElement, XmlElement htmlListElement, IDictionary inheritedProperties, CssStylesheet stylesheet, List<XmlElement> sourceContext)
		{
			var htmlListElementName = htmlListElement.LocalName.ToLower();

		    var currentProperties = GetElementProperties(htmlListElement, inheritedProperties, out var localProperties, stylesheet, sourceContext);

		    if (xamlParentElement.OwnerDocument == null) return;
		    var xamlListElement = xamlParentElement.OwnerDocument.CreateElement(null, XamlList, XamlNamespace);

		    xamlListElement.SetAttribute(XamlListMarkerStyle,
		        htmlListElementName == "ol" ? XamlListMarkerStyleDecimal : XamlListMarkerStyleDisc);

		    ApplyLocalProperties(xamlListElement, localProperties, true);

		    for (var htmlChildNode = htmlListElement.FirstChild; htmlChildNode != null; htmlChildNode = htmlChildNode.NextSibling)
		    {
		        if (!(htmlChildNode is XmlElement node) || htmlChildNode.LocalName.ToLower() != "li") continue;
		        sourceContext.Add(node);
		        AddListItem(xamlListElement, node, currentProperties, stylesheet, sourceContext);
		        sourceContext.RemoveAt(sourceContext.Count - 1);
		    }

		    if (xamlListElement.HasChildNodes) xamlParentElement.AppendChild(xamlListElement);
		}

		private static XmlElement AddOrphanListItems(XmlNode xamlParentElement, XmlNode htmlLiElement, IDictionary inheritedProperties, CssStylesheet stylesheet, List<XmlElement> sourceContext)
		{
			XmlElement lastProcessedListItemElement = null;

			var xamlListItemElementPreviousSibling = xamlParentElement.LastChild;
			XmlElement xamlListElement;
			if (xamlListItemElementPreviousSibling != null && xamlListItemElementPreviousSibling.LocalName == XamlList)
				xamlListElement = (XmlElement)xamlListItemElementPreviousSibling;
			else
			{
                if(xamlParentElement.OwnerDocument == null) return null;
                xamlListElement = xamlParentElement.OwnerDocument.CreateElement(null, XamlList, XamlNamespace);
				xamlParentElement.AppendChild(xamlListElement);
			}

			var htmlChildNode = htmlLiElement;
			var htmlChildNodeName = htmlChildNode?.LocalName.ToLower();

			while (htmlChildNode != null && htmlChildNodeName == "li")
			{
				AddListItem(xamlListElement, (XmlElement)htmlChildNode, inheritedProperties, stylesheet, sourceContext);
				lastProcessedListItemElement = (XmlElement)htmlChildNode;
				htmlChildNode = htmlChildNode.NextSibling;
				htmlChildNodeName = htmlChildNode?.LocalName.ToLower();
			}

			return lastProcessedListItemElement;
		}

		private static void AddListItem(XmlNode xamlListElement, XmlElement htmlLiElement, IDictionary inheritedProperties, CssStylesheet stylesheet, List<XmlElement> sourceContext)
		{
            var currentProperties = GetElementProperties(htmlLiElement, inheritedProperties, out var _, stylesheet, sourceContext);

            if (xamlListElement.OwnerDocument == null) return;
		    var xamlListItemElement = xamlListElement.OwnerDocument.CreateElement(null, XamlListItem, XamlNamespace);

		    for (var htmlChildNode = htmlLiElement.FirstChild; htmlChildNode != null; htmlChildNode = htmlChildNode?.NextSibling)
		        htmlChildNode = AddBlock(xamlListItemElement, htmlChildNode, currentProperties, stylesheet, sourceContext);

		    xamlListElement.AppendChild(xamlListItemElement);
		}

		private static void AddTable(XmlElement xamlParentElement, XmlElement htmlTableElement, IDictionary inheritedProperties, CssStylesheet stylesheet, List<XmlElement> sourceContext)
		{
            var currentProperties = GetElementProperties(htmlTableElement, inheritedProperties, out var _, stylesheet, sourceContext);
            var singleCell = GetCellFromSingleCellTable(htmlTableElement);

			if (singleCell != null)
			{
				sourceContext.Add(singleCell);

				for (var htmlChildNode = singleCell.FirstChild; htmlChildNode != null; htmlChildNode = htmlChildNode?.NextSibling)
					htmlChildNode = AddBlock(xamlParentElement, htmlChildNode, currentProperties, stylesheet, sourceContext);

				sourceContext.RemoveAt(sourceContext.Count - 1);
			}
			else
			{
			    if (xamlParentElement.OwnerDocument == null) return;
			    var xamlTableElement = xamlParentElement.OwnerDocument.CreateElement(null, XamlTable, XamlNamespace);

			    var columnStarts = AnalyzeTableStructure(htmlTableElement);

			    AddColumnInformation(htmlTableElement, xamlTableElement, columnStarts, currentProperties, stylesheet, sourceContext);

			    var htmlChildNode = htmlTableElement.FirstChild;

			    while (htmlChildNode != null)
			    {
			        var htmlChildName = htmlChildNode.LocalName.ToLower();

			        if (htmlChildName == "tbody" || htmlChildName == "thead" || htmlChildName == "tfoot")
			        {
			            if (xamlTableElement.OwnerDocument != null)
			            {
			                var xamlTableBodyElement = xamlTableElement.OwnerDocument.CreateElement(null, XamlTableRowGroup, XamlNamespace);
			                xamlTableElement.AppendChild(xamlTableBodyElement);

			                sourceContext.Add((XmlElement)htmlChildNode);

                            var tbodyElementCurrentProperties = GetElementProperties((XmlElement)htmlChildNode, currentProperties, out var _, stylesheet, sourceContext);

                            AddTableRowsToTableBody(xamlTableBodyElement, htmlChildNode.FirstChild, tbodyElementCurrentProperties, columnStarts, stylesheet, sourceContext);
			                if (xamlTableBodyElement.HasChildNodes) xamlTableElement.AppendChild(xamlTableBodyElement);
			            }

			            sourceContext.RemoveAt(sourceContext.Count - 1);

			            htmlChildNode = htmlChildNode.NextSibling;
			        }
			        else if (htmlChildName == "tr")
			        {
			            if (xamlTableElement.OwnerDocument == null) continue;
			            var xamlTableBodyElement = xamlTableElement.OwnerDocument.CreateElement(null, XamlTableRowGroup, XamlNamespace);

			            htmlChildNode = AddTableRowsToTableBody(xamlTableBodyElement, htmlChildNode, currentProperties, columnStarts, stylesheet, sourceContext);
			            if (xamlTableBodyElement.HasChildNodes) xamlTableElement.AppendChild(xamlTableBodyElement);
			        }
			        else
			            htmlChildNode = htmlChildNode.NextSibling;
			    }

			    if (xamlTableElement.HasChildNodes) xamlParentElement.AppendChild(xamlTableElement);
			}
		}

		private static XmlElement GetCellFromSingleCellTable(XmlNode htmlTableElement)
		{
			XmlElement singleCell = null;

			for (var tableChild = htmlTableElement.FirstChild; tableChild != null; tableChild = tableChild.NextSibling)
			{
				var elementName = tableChild.LocalName.ToLower();
				if (elementName == "tbody" || elementName == "thead" || elementName == "tfoot")
				{
					if (singleCell != null) return null;
					for (var tbodyChild = tableChild.FirstChild; tbodyChild != null; tbodyChild = tbodyChild.NextSibling)
					{
					    if (tbodyChild.LocalName.ToLower() != "tr") continue;
					    if (singleCell != null) return null;
					    for (var trChild = tbodyChild.FirstChild; trChild != null; trChild = trChild.NextSibling)
					    {
					        var cellName = trChild.LocalName.ToLower();
					        if (cellName != "td" && cellName != "th") continue;
					        if (singleCell != null) return null;
					        singleCell = (XmlElement)trChild;
					    }
					}
				}
				else if (tableChild.LocalName.ToLower() == "tr")
				{
					if (singleCell != null) return null;
					for (var trChild = tableChild.FirstChild; trChild != null; trChild = trChild.NextSibling)
					{
						var cellName = trChild.LocalName.ToLower();
					    if (cellName != "td" && cellName != "th") continue;
					    if (singleCell != null) return null;
					    singleCell = (XmlElement)trChild;
					}
				}
			}

			return singleCell;
		}

		private static void AddColumnInformation(XmlNode htmlTableElement, XmlNode xamlTableElement, IList columnStartsAllRows, IDictionary currentProperties, CssStylesheet stylesheet, List<XmlElement> sourceContext)
		{
			if (columnStartsAllRows != null)
			{
				for (var columnIndex = 0; columnIndex < columnStartsAllRows.Count - 1; columnIndex++)
				{
				    if (xamlTableElement.OwnerDocument == null) continue;
				    var xamlColumnElement = xamlTableElement.OwnerDocument.CreateElement(null, XamlTableColumn, XamlNamespace);
				    xamlColumnElement.SetAttribute(XamlWidth, ((double)columnStartsAllRows[columnIndex + 1] - (double)columnStartsAllRows[columnIndex]).ToString(CultureInfo.GetCultureInfo(1033)));
				    xamlTableElement.AppendChild(xamlColumnElement);
				}
			}
			else
			{
				for (var htmlChildNode = htmlTableElement.FirstChild; htmlChildNode != null; htmlChildNode = htmlChildNode.NextSibling)
				{
					if (htmlChildNode.LocalName.ToLower() == "colgroup")
						AddTableColumnGroup(xamlTableElement, (XmlElement)htmlChildNode, currentProperties, stylesheet, sourceContext);
					else if (htmlChildNode.LocalName.ToLower() == "col")
						AddTableColumn(xamlTableElement);
					else if (htmlChildNode is XmlElement)
						break;
				}
			}
		}

		private static void AddTableColumnGroup(XmlNode xamlTableElement, XmlElement htmlColgroupElement, IDictionary inheritedProperties, CssStylesheet stylesheet, List<XmlElement> sourceContext)
		{
            GetElementProperties(htmlColgroupElement, inheritedProperties, out var _, stylesheet, sourceContext);

            for (var htmlNode = htmlColgroupElement.FirstChild; htmlNode != null; htmlNode = htmlNode.NextSibling)
			{
			    if (htmlNode is XmlElement && htmlNode.LocalName.ToLower() == "col") AddTableColumn(xamlTableElement);
			}
		}

		private static void AddTableColumn(XmlNode xamlTableElement)
		{
		    if (xamlTableElement.OwnerDocument == null) return;
		    var xamlTableColumnElement = xamlTableElement.OwnerDocument.CreateElement(null, XamlTableColumn, XamlNamespace);
		    xamlTableElement.AppendChild(xamlTableColumnElement);
		}

		private static XmlNode AddTableRowsToTableBody(XmlNode xamlTableBodyElement, XmlNode htmlTrStartNode, IDictionary currentProperties, ArrayList columnStarts, CssStylesheet stylesheet, List<XmlElement> sourceContext)
		{
			var htmlChildNode = htmlTrStartNode;
			ArrayList activeRowSpans = null;
			if (columnStarts != null)
			{
				activeRowSpans = new ArrayList();
				InitializeActiveRowSpans(activeRowSpans, columnStarts.Count);
			}

			while (htmlChildNode != null && htmlChildNode.LocalName.ToLower() != "tbody")
			{
			    XmlElement xamlTableRowElement;
                switch (htmlChildNode.LocalName.ToLower())
			    {
			        case "tr":
			            xamlTableRowElement = xamlTableBodyElement.OwnerDocument.CreateElement(null, XamlTableRow, XamlNamespace);

			            sourceContext.Add((XmlElement)htmlChildNode);

			            var trElementCurrentProperties = GetElementProperties((XmlElement)htmlChildNode, currentProperties, out _, stylesheet, sourceContext);

			            AddTableCellsToTableRow(xamlTableRowElement, htmlChildNode.FirstChild, trElementCurrentProperties, columnStarts, activeRowSpans, stylesheet, sourceContext);
			            if (xamlTableRowElement.HasChildNodes) xamlTableBodyElement.AppendChild(xamlTableRowElement);

			            sourceContext.RemoveAt(sourceContext.Count - 1);

			            htmlChildNode = htmlChildNode.NextSibling;

			            break;
			        case "td":
			            xamlTableRowElement = xamlTableBodyElement.OwnerDocument.CreateElement(null, XamlTableRow, XamlNamespace);

			            htmlChildNode = AddTableCellsToTableRow(xamlTableRowElement, htmlChildNode, currentProperties, columnStarts, activeRowSpans, stylesheet, sourceContext);
			            if (xamlTableRowElement.HasChildNodes) xamlTableBodyElement.AppendChild(xamlTableRowElement);
			            break;
			        default:
			            htmlChildNode = htmlChildNode.NextSibling;
			            break;
			    }
			}
			return htmlChildNode;
		}

		private static XmlNode AddTableCellsToTableRow(XmlNode xamlTableRowElement, XmlNode htmlTdStartNode, IDictionary currentProperties, IList columnStarts, IList activeRowSpans, CssStylesheet stylesheet, List<XmlElement> sourceContext)
		{
			var htmlChildNode = htmlTdStartNode;
		    var columnIndex = 0;

		    while (htmlChildNode != null && htmlChildNode.LocalName.ToLower() != "tr" && htmlChildNode.LocalName.ToLower() != "tbody" && htmlChildNode.LocalName.ToLower() != "thead" && htmlChildNode.LocalName.ToLower() != "tfoot")
			{
				if (htmlChildNode.LocalName.ToLower() == "td" || htmlChildNode.LocalName.ToLower() == "th")
				{
				    if (xamlTableRowElement.OwnerDocument != null)
				    {
				        var xamlTableCellElement = xamlTableRowElement.OwnerDocument.CreateElement(null, XamlTableCell, XamlNamespace);

				        sourceContext.Add((XmlElement)htmlChildNode);

                        var tdElementCurrentProperties = GetElementProperties((XmlElement)htmlChildNode, currentProperties, out var _, stylesheet, sourceContext);

                        ApplyPropertiesToTableCellElement((XmlElement)htmlChildNode, xamlTableCellElement);

				        if (columnStarts != null)
				        {
				            while (columnIndex < activeRowSpans.Count && (int)activeRowSpans[columnIndex] > 0)
				            {
				                activeRowSpans[columnIndex] = (int)activeRowSpans[columnIndex] - 1;
				                columnIndex++;
				            }
				            var columnWidth = GetColumnWidth((XmlElement)htmlChildNode);
				            var columnSpan = CalculateColumnSpan(columnIndex, columnWidth, columnStarts);
				            var rowSpan = GetRowSpan((XmlElement)htmlChildNode);

				            xamlTableCellElement.SetAttribute(XamlTableCellColumnSpan, columnSpan.ToString());

				            for (var spannedColumnIndex = columnIndex; spannedColumnIndex < columnIndex + columnSpan; spannedColumnIndex++)
				                activeRowSpans[spannedColumnIndex] = rowSpan - 1;

				            columnIndex = columnIndex + columnSpan;
				        }

				        AddDataToTableCell(xamlTableCellElement, htmlChildNode.FirstChild, tdElementCurrentProperties, stylesheet, sourceContext);
				        if (xamlTableCellElement.HasChildNodes) xamlTableRowElement.AppendChild(xamlTableCellElement);
				    }

				    sourceContext.RemoveAt(sourceContext.Count - 1);

					htmlChildNode = htmlChildNode.NextSibling;
				}
				else
					htmlChildNode = htmlChildNode.NextSibling;
			}
			return htmlChildNode;
		}

		private static void AddDataToTableCell(XmlElement xamlTableCellElement, XmlNode htmlDataStartNode, Hashtable currentProperties, CssStylesheet stylesheet, List<XmlElement> sourceContext)
		{
			for (var htmlChildNode = htmlDataStartNode; htmlChildNode != null; htmlChildNode = htmlChildNode?.NextSibling)
				htmlChildNode = AddBlock(xamlTableCellElement, htmlChildNode, currentProperties, stylesheet, sourceContext);
		}

		private static ArrayList AnalyzeTableStructure(XmlNode htmlTableElement)
		{
			if (!htmlTableElement.HasChildNodes) return null; 
			var columnWidthsAvailable = true;

			var columnStarts = new ArrayList();
			var activeRowSpans = new ArrayList();

			var htmlChildNode = htmlTableElement.FirstChild;
			double tableWidth = 0;

			while (htmlChildNode != null && columnWidthsAvailable)
			{
				switch (htmlChildNode.LocalName.ToLower())
				{
					case "tbody":
						var tbodyWidth = AnalyzeTbodyStructure((XmlElement)htmlChildNode, columnStarts, activeRowSpans);
						if (tbodyWidth > tableWidth)
							tableWidth = tbodyWidth;
						else if (Math.Abs(tbodyWidth) < 1e-6)
							columnWidthsAvailable = false;
						break;
					case "tr":
						var trWidth = AnalyzeTrStructure((XmlElement)htmlChildNode, columnStarts, activeRowSpans);
						if (trWidth > tableWidth)
							tableWidth = trWidth;
						else if (Math.Abs(trWidth) < 1e-6)
							columnWidthsAvailable = false;
						break;
					case "td":
						columnWidthsAvailable = false;
						break;
				}

				htmlChildNode = htmlChildNode.NextSibling;
			}

			if (columnWidthsAvailable)
				columnStarts.Add(tableWidth);
			else
				columnStarts = null;

			return columnStarts;
		}

		private static double AnalyzeTbodyStructure(XmlNode htmlTbodyElement, ArrayList columnStarts, ArrayList activeRowSpans)
		{
			double tbodyWidth = 0;
			var columnWidthsAvailable = true;

			if (!htmlTbodyElement.HasChildNodes) return tbodyWidth;

			ClearActiveRowSpans(activeRowSpans);

			var htmlChildNode = htmlTbodyElement.FirstChild;

			while (htmlChildNode != null && columnWidthsAvailable)
			{
				switch (htmlChildNode.LocalName.ToLower())
				{
					case "tr":
						var trWidth = AnalyzeTrStructure((XmlElement)htmlChildNode, columnStarts, activeRowSpans);
						if (trWidth > tbodyWidth) tbodyWidth = trWidth;
						break;
					case "td":
						columnWidthsAvailable = false;
						break;
				}
				htmlChildNode = htmlChildNode.NextSibling;
			}

			ClearActiveRowSpans(activeRowSpans);

			return columnWidthsAvailable ? tbodyWidth : 0;
		}

		private static double AnalyzeTrStructure(XmlNode htmlTrElement, ArrayList columnStarts, ArrayList activeRowSpans)
		{
			if (!htmlTrElement.HasChildNodes) return 0;

			var columnWidthsAvailable = true;

			double columnStart = 0;
			var htmlChildNode = htmlTrElement.FirstChild;
			var columnIndex = 0;

		    if (columnIndex < activeRowSpans.Count)
			{
				if (Math.Abs((double)columnStarts[columnIndex] - columnStart) < 1e-6)
				{
					while (columnIndex < activeRowSpans.Count && (int)activeRowSpans[columnIndex] > 0)
					{
						activeRowSpans[columnIndex] = (int)activeRowSpans[columnIndex] - 1;
						columnIndex++;
						columnStart = (double)columnStarts[columnIndex];
					}
				}
			}

			while (htmlChildNode != null && columnWidthsAvailable)
			{
				switch (htmlChildNode.LocalName.ToLower())
				{
					case "td":
						if (columnIndex < columnStarts.Count)
						{
							if (columnStart < (double)columnStarts[columnIndex])
							{
								columnStarts.Insert(columnIndex, columnStart);
								activeRowSpans.Insert(columnIndex, 0);
							}
						}
						else
						{
							columnStarts.Add(columnStart);
							activeRowSpans.Add(0);
						}
						var columnWidth = GetColumnWidth((XmlElement)htmlChildNode);
						if (Math.Abs(columnWidth + 1) > 1e-6)
						{
						    var rowSpan = GetRowSpan((XmlElement)htmlChildNode);

							var nextColumnIndex = GetNextColumnIndex(columnIndex, columnWidth, columnStarts, activeRowSpans);
							if (nextColumnIndex != -1)
							{
								for (var spannedColumnIndex = columnIndex; spannedColumnIndex < nextColumnIndex; spannedColumnIndex++)
									activeRowSpans[spannedColumnIndex] = rowSpan - 1;

								columnIndex = nextColumnIndex;

								columnStart = columnStart + columnWidth;

								if (columnIndex < activeRowSpans.Count)
								{
									if (Math.Abs((double)columnStarts[columnIndex] - columnStart) < 1e-6)
									{
										while (columnIndex < activeRowSpans.Count && (int)activeRowSpans[columnIndex] > 0)
										{
											activeRowSpans[columnIndex] = (int)activeRowSpans[columnIndex] - 1;
											columnIndex++;
											columnStart = (double)columnStarts[columnIndex];
										}
									}
								}
							}
							else
								columnWidthsAvailable = false;
						}
						else
							columnWidthsAvailable = false;
						break;
				}

				htmlChildNode = htmlChildNode.NextSibling;
			}
            
			return columnWidthsAvailable ? columnStart : 0;
		}

		private static int GetRowSpan(XmlElement htmlTdElement)
		{
		    int rowSpan;

			var rowSpanAsString = GetAttribute(htmlTdElement, "rowspan");
			if (rowSpanAsString != null)
			{
				if (!Int32.TryParse(rowSpanAsString, out rowSpan)) rowSpan = 1;
			}
			else
				rowSpan = 1;
			return rowSpan;
		}

		private static int GetNextColumnIndex(int columnIndex, double columnWidth, ArrayList columnStarts, ArrayList activeRowSpans)
		{
		    var columnStart = (double)columnStarts[columnIndex];
			var spannedColumnIndex = columnIndex + 1;

			while (spannedColumnIndex < columnStarts.Count && (double)columnStarts[spannedColumnIndex] < columnStart + columnWidth && spannedColumnIndex != -1)
			{
				if ((int)activeRowSpans[spannedColumnIndex] > 0)
					spannedColumnIndex = -1;
				else
					spannedColumnIndex++;
			}

			return spannedColumnIndex;
		}

		private static void ClearActiveRowSpans(IList activeRowSpans)
		{
			for (var columnIndex = 0; columnIndex < activeRowSpans.Count; columnIndex++)
				activeRowSpans[columnIndex] = 0;
		}

		private static void InitializeActiveRowSpans(IList activeRowSpans, int count)
		{
			for (var columnIndex = 0; columnIndex < count; columnIndex++) activeRowSpans.Add(0);
		}
        
		private static double GetColumnWidth(XmlElement htmlTdElement)
		{
		    var columnWidthAsString = GetAttribute(htmlTdElement, "width") ?? GetCssAttribute(GetAttribute(htmlTdElement, "style"), "width");
		    if (!TryGetLengthValue(columnWidthAsString, out var columnWidth) || Math.Abs(columnWidth) < 1e-6) columnWidth = -1;
			return columnWidth;
		}

		private static int CalculateColumnSpan(int columnIndex, double columnWidth, IList columnStarts)
		{
            var columnSpanningIndex = columnIndex;
			double columnSpanningValue = 0;

		    while (columnSpanningValue < columnWidth && columnSpanningIndex < columnStarts.Count - 1)
			{
				var subColumnWidth = (double)columnStarts[columnSpanningIndex + 1] - (double)columnStarts[columnSpanningIndex];
				columnSpanningValue += subColumnWidth;
				columnSpanningIndex++;
			}

			var columnSpan = columnSpanningIndex - columnIndex;

			return columnSpan;
		}

		private static void ApplyLocalProperties(XmlElement xamlElement, IDictionary localProperties, bool isBlock)
		{
			var marginSet = false;
			var marginTop = "0";
			var marginBottom = "0";
			var marginLeft = "0";
			var marginRight = "0";

			var paddingSet = false;
			var paddingTop = "0";
			var paddingBottom = "0";
			var paddingLeft = "0";
			var paddingRight = "0";

			string borderColor = null;

			var borderThicknessSet = false;
			var borderThicknessTop = "0";
			var borderThicknessBottom = "0";
			var borderThicknessLeft = "0";
			var borderThicknessRight = "0";

			var propertyEnumerator = localProperties.GetEnumerator();
			while (propertyEnumerator.MoveNext())
			{
				switch ((string)propertyEnumerator.Key)
				{
					case "font-family":
						xamlElement.SetAttribute(XamlFontFamily, (string)propertyEnumerator.Value);
						break;
					case "font-style":
						xamlElement.SetAttribute(XamlFontStyle, (string)propertyEnumerator.Value);
						break;
					case "font-variant":
						break;
					case "font-weight":
						xamlElement.SetAttribute(XamlFontWeight, (string)propertyEnumerator.Value);
						break;
					case "font-size":
						xamlElement.SetAttribute(XamlFontSize, (string)propertyEnumerator.Value);
						break;
					case "color":
						SetPropertyValue(xamlElement, TextElement.ForegroundProperty, (string)propertyEnumerator.Value);
						break;
					case "background-color":
						SetPropertyValue(xamlElement, TextElement.BackgroundProperty, (string)propertyEnumerator.Value);
						break;
					case "text-decoration-underline":
						if (!isBlock)
						{
							if ((string)propertyEnumerator.Value == "true") xamlElement.SetAttribute(XamlTextDecorations, XamlTextDecorationsUnderline);
						}
						break;
					case "text-decoration-none":
					case "text-decoration-overline":
					case "text-decoration-line-through":
					case "text-decoration-blink":
						if (!isBlock)
						{
						}
						break;
					case "text-transform":
						break;

					case "text-indent":
						if (isBlock) xamlElement.SetAttribute(XamlTextIndent, (string)propertyEnumerator.Value);
						break;

					case "text-align":
						if (isBlock) xamlElement.SetAttribute(XamlTextAlignment, (string)propertyEnumerator.Value);
						break;

					case "width":
					case "height":
						break;

					case "margin-top":
						marginSet = true;
						marginTop = (string)propertyEnumerator.Value;
						break;
					case "margin-right":
						marginSet = true;
						marginRight = (string)propertyEnumerator.Value;
						break;
					case "margin-bottom":
						marginSet = true;
						marginBottom = (string)propertyEnumerator.Value;
						break;
					case "margin-left":
						marginSet = true;
						marginLeft = (string)propertyEnumerator.Value;
						break;

					case "padding-top":
						paddingSet = true;
						paddingTop = (string)propertyEnumerator.Value;
						break;
					case "padding-right":
						paddingSet = true;
						paddingRight = (string)propertyEnumerator.Value;
						break;
					case "padding-bottom":
						paddingSet = true;
						paddingBottom = (string)propertyEnumerator.Value;
						break;
					case "padding-left":
						paddingSet = true;
						paddingLeft = (string)propertyEnumerator.Value;
						break;

					case "border-color-top":
						borderColor = (string)propertyEnumerator.Value;
						break;
					case "border-color-right":
						borderColor = (string)propertyEnumerator.Value;
						break;
					case "border-color-bottom":
						borderColor = (string)propertyEnumerator.Value;
						break;
					case "border-color-left":
						borderColor = (string)propertyEnumerator.Value;
						break;
					case "border-style-top":
					case "border-style-right":
					case "border-style-bottom":
					case "border-style-left":
						break;
					case "border-width-top":
						borderThicknessSet = true;
						borderThicknessTop = (string)propertyEnumerator.Value;
						break;
					case "border-width-right":
						borderThicknessSet = true;
						borderThicknessRight = (string)propertyEnumerator.Value;
						break;
					case "border-width-bottom":
						borderThicknessSet = true;
						borderThicknessBottom = (string)propertyEnumerator.Value;
						break;
					case "border-width-left":
						borderThicknessSet = true;
						borderThicknessLeft = (string)propertyEnumerator.Value;
						break;

					case "list-style-type":
						if (xamlElement.LocalName == XamlList)
						{
							string markerStyle;
							switch (((string)propertyEnumerator.Value).ToLower())
							{
								case "disc":
									markerStyle = XamlListMarkerStyleDisc;
									break;
								case "circle":
									markerStyle = XamlListMarkerStyleCircle;
									break;
								case "none":
									markerStyle = XamlListMarkerStyleNone;
									break;
								case "square":
									markerStyle = XamlListMarkerStyleSquare;
									break;
								case "box":
									markerStyle = XamlListMarkerStyleBox;
									break;
								case "lower-latin":
									markerStyle = XamlListMarkerStyleLowerLatin;
									break;
								case "upper-latin":
									markerStyle = XamlListMarkerStyleUpperLatin;
									break;
								case "lower-roman":
									markerStyle = XamlListMarkerStyleLowerRoman;
									break;
								case "upper-roman":
									markerStyle = XamlListMarkerStyleUpperRoman;
									break;
								case "decimal":
									markerStyle = XamlListMarkerStyleDecimal;
									break;
								default:
									markerStyle = XamlListMarkerStyleDisc;
									break;
							}
							xamlElement.SetAttribute(XamlListMarkerStyle, markerStyle);
						}
						break;

					case "float":
					case "clear":
						break;

					case "display":
						break;
				}
			}

		    if (!isBlock) return;

		    if (marginSet) ComposeThicknessProperty(xamlElement, XamlMargin, marginLeft, marginRight, marginTop, marginBottom);
            if (paddingSet) ComposeThicknessProperty(xamlElement, XamlPadding, paddingLeft, paddingRight, paddingTop, paddingBottom);
            if (borderColor != null) xamlElement.SetAttribute(XamlBorderBrush, borderColor);
            if (borderThicknessSet) ComposeThicknessProperty(xamlElement, XamlBorderThickness, borderThicknessLeft, borderThicknessRight, borderThicknessTop, borderThicknessBottom);
		}

		private static void ComposeThicknessProperty(XmlElement xamlElement, string propertyName, string left, string right, string top, string bottom)
		{
			string thickness;

			if (left[0] == '0' || left[0] == '-') left = "0"; if (right[0] == '0' || right[0] == '-') right = "0";
			if (top[0] == '0' || top[0] == '-') top = "0"; if (bottom[0] == '0' || bottom[0] == '-') bottom = "0";

			if (left == right && top == bottom) 
			    thickness = left == top ? left : left + "," + top; 
			else
				thickness = left + "," + top + "," + right + "," + bottom;

			xamlElement.SetAttribute(propertyName, thickness);
		}

		private static void SetPropertyValue(XmlElement xamlElement, DependencyProperty property, string stringValue)
		{
			var typeConverter = System.ComponentModel.TypeDescriptor.GetConverter(property.PropertyType);
			try
			{
				var convertedValue = typeConverter.ConvertFromInvariantString(stringValue);
				if (convertedValue != null) xamlElement.SetAttribute(property.Name, stringValue);
			}
			catch (Exception) { }
		}

		private static Hashtable GetElementProperties(XmlElement htmlElement, IDictionary inheritedProperties, out Hashtable localProperties, CssStylesheet stylesheet, List<XmlElement> sourceContext)
		{
			var currentProperties = new Hashtable();
			var propertyEnumerator = inheritedProperties.GetEnumerator();
			while (propertyEnumerator.MoveNext()) currentProperties[propertyEnumerator.Key] = propertyEnumerator.Value;

			var elementName = htmlElement.LocalName.ToLower();

		    localProperties = new Hashtable();
			switch (elementName)
			{
				case "i":
				case "italic":
				case "em":
					localProperties["font-style"] = "italic";
					break;
				case "b":
				case "bold":
				case "strong":
				case "dfn":
					localProperties["font-weight"] = "bold";
					break;
				case "u":
				case "underline":
					localProperties["text-decoration-underline"] = "true";
					break;
				case "font":
					var attributeValue = GetAttribute(htmlElement, "face");
					if (attributeValue != null) localProperties["font-family"] = attributeValue;
					attributeValue = GetAttribute(htmlElement, "size");
					if (attributeValue != null)
					{
						var fontSize = double.Parse(attributeValue) * (12.0 / 3.0);
						if (fontSize < 1.0)
							fontSize = 1.0;
						else if (fontSize > 1000.0)
							fontSize = 1000.0;
						localProperties["font-size"] = fontSize.ToString(CultureInfo.GetCultureInfo("en-US"));
					}
					attributeValue = GetAttribute(htmlElement, "color");
					if (attributeValue != null) localProperties["color"] = attributeValue;
					break;
				case "samp":
					localProperties["font-family"] = "Courier New";
					localProperties["font-size"] = XamlFontSizeXxSmall;
					localProperties["text-align"] = "Left";
					break;
				case "sub":
					break;
				case "sup":
					break;

				case "a":
					break;
				case "acronym":
					break;

				case "p":
					break;
				case "div":
					break;
				case "pre":
					localProperties["font-family"] = "Courier New";
					localProperties["font-size"] = XamlFontSizeXxSmall;
					localProperties["text-align"] = "Left";
					break;
				case "blockquote":
					localProperties["margin-left"] = "16";
					break;

				case "h1":
					localProperties["font-size"] = XamlFontSizeXxLarge;
					break;
				case "h2":
					localProperties["font-size"] = XamlFontSizeXLarge;
					break;
				case "h3":
					localProperties["font-size"] = XamlFontSizeLarge;
					break;
				case "h4":
					localProperties["font-size"] = XamlFontSizeMedium;
					break;
				case "h5":
					localProperties["font-size"] = XamlFontSizeSmall;
					break;
				case "h6":
					localProperties["font-size"] = XamlFontSizeXSmall;
					break;
				case "ul":
					localProperties["list-style-type"] = "disc";
					break;
				case "ol":
					localProperties["list-style-type"] = "decimal";
					break;

				case "table":
				case "body":
				case "html":
					break;
			}

			HtmlCssParser.GetElementPropertiesFromCssAttributes(htmlElement, elementName, stylesheet, localProperties, sourceContext);

			propertyEnumerator = localProperties.GetEnumerator();
			while (propertyEnumerator.MoveNext()) currentProperties[propertyEnumerator.Key] = propertyEnumerator.Value;

			return currentProperties;
		}

		private static string GetCssAttribute(string cssStyle, string attributeName)
		{
		    if (cssStyle == null) return null;
		    attributeName = attributeName.ToLower();
		    var styleValues = cssStyle.Split(';');
		    return (from t in styleValues select t.Split(':') into styleNameValue where styleNameValue.Length == 2 where styleNameValue[0].Trim().ToLower() == attributeName select styleNameValue[1].Trim()).FirstOrDefault();
		}

		private static bool TryGetLengthValue(string lengthAsString, out double length)
		{
			length = Double.NaN;

		    if (lengthAsString == null) return !Double.IsNaN(length);
		    lengthAsString = lengthAsString.Trim().ToLower();

		    if (lengthAsString.EndsWith("pt"))
		    {
		        lengthAsString = lengthAsString.Substring(0, lengthAsString.Length - 2);
		        if (Double.TryParse(lengthAsString, out length))
		            length = length * 96.0 / 72.0;
		        else
		            length = Double.NaN;
		    }
		    else if (lengthAsString.EndsWith("px"))
		    {
		        lengthAsString = lengthAsString.Substring(0, lengthAsString.Length - 2);
		        if (!Double.TryParse(lengthAsString, out length)) length = Double.NaN;
		    }
		    else
		    {
		        if (!Double.TryParse(lengthAsString, out length)) length = Double.NaN;
		    }

		    return !Double.IsNaN(length);
		}

		private static void ApplyPropertiesToTableCellElement(XmlElement htmlChildNode, XmlElement xamlTableCellElement)
		{
			xamlTableCellElement.SetAttribute(XamlTableCellBorderThickness, "1,1,1,1");
			xamlTableCellElement.SetAttribute(XamlTableCellBorderBrush, XamlBrushesBlack);
			var rowSpanString = GetAttribute(htmlChildNode, "rowspan");
			if (rowSpanString != null) xamlTableCellElement.SetAttribute(XamlTableCellRowSpan, rowSpanString);
		}

		public const string XamlFlowDocument = "FlowDocument";

		public const string XamlRun = "Run";
		public const string XamlSpan = "Span";
		public const string XamlHyperlink = "Hyperlink";
		public const string XamlHyperlinkNavigateUri = "NavigateUri";
		public const string XamlHyperlinkTargetName = "TargetName";

		public const string XamlSection = "Section";

		public const string XamlList = "List";

		public const string XamlListMarkerStyle = "MarkerStyle";
		public const string XamlListMarkerStyleNone = "None";
		public const string XamlListMarkerStyleDecimal = "Decimal";
		public const string XamlListMarkerStyleDisc = "Disc";
		public const string XamlListMarkerStyleCircle = "Circle";
		public const string XamlListMarkerStyleSquare = "Square";
		public const string XamlListMarkerStyleBox = "Box";
		public const string XamlListMarkerStyleLowerLatin = "LowerLatin";
		public const string XamlListMarkerStyleUpperLatin = "UpperLatin";
		public const string XamlListMarkerStyleLowerRoman = "LowerRoman";
		public const string XamlListMarkerStyleUpperRoman = "UpperRoman";

		public const string XamlListItem = "ListItem";

		public const string XamlLineBreak = "LineBreak";

		public const string XamlParagraph = "Paragraph";

		public const string XamlMargin = "Margin";
		public const string XamlPadding = "Padding";
		public const string XamlBorderBrush = "BorderBrush";
		public const string XamlBorderThickness = "BorderThickness";

		public const string XamlTable = "Table";

		public const string XamlTableColumn = "TableColumn";
		public const string XamlTableRowGroup = "TableRowGroup";
		public const string XamlTableRow = "TableRow";

		public const string XamlTableCell = "TableCell";
		public const string XamlTableCellBorderThickness = "BorderThickness";
		public const string XamlTableCellBorderBrush = "BorderBrush";

		public const string XamlTableCellColumnSpan = "ColumnSpan";
		public const string XamlTableCellRowSpan = "RowSpan";

		public const string XamlWidth = "Width";
		public const string XamlBrushesBlack = "Black";
		public const string XamlFontFamily = "FontFamily";

		public const string XamlFontSize = "FontSize";
		public const string XamlFontSizeXxLarge = "22pt";
		public const string XamlFontSizeXLarge = "20pt";
		public const string XamlFontSizeLarge = "18pt";
		public const string XamlFontSizeMedium = "16pt";
		public const string XamlFontSizeSmall = "12pt";
		public const string XamlFontSizeXSmall = "10pt";
		public const string XamlFontSizeXxSmall = "8pt";

		public const string XamlFontWeight = "FontWeight";
		public const string XamlFontWeightBold = "Bold";

		public const string XamlFontStyle = "FontStyle";

		public const string XamlForeground = "Foreground";
		public const string XamlBackground = "Background";
		public const string XamlTextDecorations = "TextDecorations";
		public const string XamlTextDecorationsUnderline = "Underline";

		public const string XamlTextIndent = "TextIndent";
		public const string XamlTextAlignment = "TextAlignment";

	    private const string XamlNamespace = "http://schemas.microsoft.com/winfx/2006/xaml/presentation";
	}
}
