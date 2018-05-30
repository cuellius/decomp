using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;

namespace Decomp.Windows.HtmlConverter
{
	internal class HtmlParser
	{
		private HtmlParser(string inputString)
		{
			_document = new XmlDocument();
			_openedElements = new Stack<XmlElement>();
			_pendingInlineElements = new Stack<XmlElement>();
			_htmlLexicalAnalyzer = new HtmlLexicalAnalyzer(inputString);
			_htmlLexicalAnalyzer.GetNextContentToken();
		}

		internal static XmlElement ParseHtml(string htmlString)
		{
			var htmlParser = new HtmlParser(htmlString);
			var htmlRootElement = htmlParser.ParseHtmlContent();
			return htmlRootElement;
		}

		internal const string HtmlHeader = "Version:1.0\r\nStartHTML:{0:D10}\r\nEndHTML:{1:D10}\r\nStartFragment:{2:D10}\r\nEndFragment:{3:D10}\r\nStartSelection:{4:D10}\r\nEndSelection:{5:D10}\r\n";
		internal const string HtmlStartFragmentComment = "<!--StartFragment-->";
		internal const string HtmlEndFragmentComment = "<!--EndFragment-->";

		internal static string ExtractHtmlFromClipboardData(string htmlDataString)
		{
			var startHtmlIndex = htmlDataString.IndexOf("StartHTML:", StringComparison.Ordinal);
			if (startHtmlIndex < 0) return "ERROR: Urecognized html header";
			startHtmlIndex = Int32.Parse(htmlDataString.Substring(startHtmlIndex + "StartHTML:".Length, "0123456789".Length));
			if (startHtmlIndex < 0 || startHtmlIndex > htmlDataString.Length) return "ERROR: Urecognized html header";

			var endHtmlIndex = htmlDataString.IndexOf("EndHTML:", StringComparison.Ordinal);
			if (endHtmlIndex < 0) return "ERROR: Urecognized html header";
			endHtmlIndex = Int32.Parse(htmlDataString.Substring(endHtmlIndex + "EndHTML:".Length, "0123456789".Length));
			if (endHtmlIndex > htmlDataString.Length) endHtmlIndex = htmlDataString.Length;

			return htmlDataString.Substring(startHtmlIndex, endHtmlIndex - startHtmlIndex);
		}

		internal static string AddHtmlClipboardHeader(string htmlString)
		{
			var stringBuilder = new StringBuilder();

			var startHtml = HtmlHeader.Length + 6 * ("0123456789".Length - "{0:D10}".Length);
			var endHtml = startHtml + htmlString.Length;
			var startFragment = htmlString.IndexOf(HtmlStartFragmentComment, 0, StringComparison.Ordinal);
			if (startFragment >= 0)
				startFragment = startHtml + startFragment + HtmlStartFragmentComment.Length;
			else
				startFragment = startHtml;
			var endFragment = htmlString.IndexOf(HtmlEndFragmentComment, 0, StringComparison.Ordinal);
			if (endFragment >= 0)
				endFragment = startHtml + endFragment;
			else
				endFragment = endHtml;

			stringBuilder.AppendFormat(HtmlHeader, startHtml, endHtml, startFragment, endFragment, startFragment, endFragment);

			stringBuilder.Append(htmlString);

			return stringBuilder.ToString();
		}

		private XmlElement ParseHtmlContent()
		{
			var htmlRootElement = _document.CreateElement("html", XhtmlNamespace);
			OpenStructuringElement(htmlRootElement);

			while (_htmlLexicalAnalyzer.NextTokenType != HtmlTokenType.Eof)
			{
			    switch (_htmlLexicalAnalyzer.NextTokenType)
			    {
			        case HtmlTokenType.OpeningTagStart:
			            _htmlLexicalAnalyzer.GetNextTagToken();
			            if (_htmlLexicalAnalyzer.NextTokenType == HtmlTokenType.Name)
			            {
			                var htmlElementName = _htmlLexicalAnalyzer.NextToken.ToLower();
			                _htmlLexicalAnalyzer.GetNextTagToken();

			                var htmlElement = _document.CreateElement(htmlElementName, XhtmlNamespace);

			                ParseAttributes(htmlElement);

			                if (_htmlLexicalAnalyzer.NextTokenType == HtmlTokenType.EmptyTagEnd || HtmlSchema.IsEmptyElement(htmlElementName))
			                    AddEmptyElement(htmlElement);
			                else if (HtmlSchema.IsInlineElement(htmlElementName))
			                    OpenInlineElement(htmlElement);
			                else if (HtmlSchema.IsBlockElement(htmlElementName) || HtmlSchema.IsKnownOpenableElement(htmlElementName))
			                    OpenStructuringElement(htmlElement);
			            }
			            break;
			        case HtmlTokenType.ClosingTagStart:
			            _htmlLexicalAnalyzer.GetNextTagToken();
			            if (_htmlLexicalAnalyzer.NextTokenType == HtmlTokenType.Name)
			            {
			                var htmlElementName = _htmlLexicalAnalyzer.NextToken.ToLower();

			                _htmlLexicalAnalyzer.GetNextTagToken();

			                CloseElement(htmlElementName);
			            }
			            break;
			        case HtmlTokenType.Text:
			            AddTextContent(_htmlLexicalAnalyzer.NextToken);
			            break;
			        case HtmlTokenType.Comment:
			            AddComment(_htmlLexicalAnalyzer.NextToken);
			            break;
			    }

			    _htmlLexicalAnalyzer.GetNextContentToken();
			}

		    if (htmlRootElement.FirstChild is XmlElement child && htmlRootElement.FirstChild == htmlRootElement.LastChild && htmlRootElement.FirstChild.LocalName.ToLower() == "html") htmlRootElement = child;

			return htmlRootElement;
		}

		private XmlElement CreateElementCopy(XmlElement htmlElement)
		{
			var htmlElementCopy = _document.CreateElement(htmlElement.LocalName, XhtmlNamespace);
			for (var i = 0; i < htmlElement.Attributes.Count; i++)
			{
				var attribute = htmlElement.Attributes[i];
				htmlElementCopy.SetAttribute(attribute.Name, attribute.Value);
			}
			return htmlElementCopy;
		}

		private void AddEmptyElement(XmlNode htmlEmptyElement)
		{
			var htmlParent = _openedElements.Peek();
			htmlParent.AppendChild(htmlEmptyElement);
		}

		private void OpenInlineElement(XmlElement htmlInlineElement)
		{
			_pendingInlineElements.Push(htmlInlineElement);
		}

		private void OpenStructuringElement(XmlElement htmlElement)
		{
			if (HtmlSchema.IsBlockElement(htmlElement.LocalName))
			{
				while (_openedElements.Count > 0 && HtmlSchema.IsInlineElement(_openedElements.Peek().LocalName))
				{
					var htmlInlineElement = _openedElements.Pop();
					_pendingInlineElements.Push(CreateElementCopy(htmlInlineElement));
				}
			}

			if (_openedElements.Count > 0)
			{
				var htmlParent = _openedElements.Peek();

				if (HtmlSchema.ClosesOnNextElementStart(htmlParent.LocalName, htmlElement.LocalName))
				{
					_openedElements.Pop();
					htmlParent = _openedElements.Count > 0 ? _openedElements.Peek() : null;
				}

			    htmlParent?.AppendChild(htmlElement);
			}

			_openedElements.Push(htmlElement);
		}

		private bool IsElementOpened(string htmlElementName)
		{
		    return _openedElements.Any(openedElement => openedElement.LocalName == htmlElementName);
		}

		private void CloseElement(string htmlElementName)
		{
			if (_pendingInlineElements.Count > 0 && _pendingInlineElements.Peek().LocalName == htmlElementName)
			{
				var htmlInlineElement = _pendingInlineElements.Pop();
				var htmlParent = _openedElements.Peek();
				htmlParent.AppendChild(htmlInlineElement);
				return;
			}
		    if (!IsElementOpened(htmlElementName)) return;
		    while (_openedElements.Count > 1)
		    {
		        var htmlOpenedElement = _openedElements.Pop(); 
		        if (htmlOpenedElement.LocalName == htmlElementName) return; 
		        if (HtmlSchema.IsInlineElement(htmlOpenedElement.LocalName)) _pendingInlineElements.Push(CreateElementCopy(htmlOpenedElement));
		    }
		}

		private void AddTextContent(string textContent)
		{
			OpenPendingInlineElements();

			var htmlParent = _openedElements.Peek();
			var textNode = _document.CreateTextNode(textContent);
			htmlParent.AppendChild(textNode);
		}

		private void AddComment(string comment)
		{
			OpenPendingInlineElements();

			var htmlParent = _openedElements.Peek();
			var xmlComment = _document.CreateComment(comment);
			htmlParent.AppendChild(xmlComment);
		}

		private void OpenPendingInlineElements()
		{
		    if (_pendingInlineElements.Count <= 0) return;
		    var htmlInlineElement = _pendingInlineElements.Pop();

		    OpenPendingInlineElements();

		    var htmlParent = _openedElements.Peek();
		    htmlParent.AppendChild(htmlInlineElement);
		    _openedElements.Push(htmlInlineElement);
		}

		private void ParseAttributes(XmlElement xmlElement)
		{
			while (_htmlLexicalAnalyzer.NextTokenType != HtmlTokenType.Eof && _htmlLexicalAnalyzer.NextTokenType != HtmlTokenType.TagEnd &&
					_htmlLexicalAnalyzer.NextTokenType != HtmlTokenType.EmptyTagEnd)
			{
				if (_htmlLexicalAnalyzer.NextTokenType == HtmlTokenType.Name)
				{
					var attributeName = _htmlLexicalAnalyzer.NextToken;
					_htmlLexicalAnalyzer.GetNextEqualSignToken();

					_htmlLexicalAnalyzer.GetNextAtomToken();

					var attributeValue = _htmlLexicalAnalyzer.NextToken;
					xmlElement.SetAttribute(attributeName, attributeValue);
				}
				_htmlLexicalAnalyzer.GetNextTagToken();
			}
		}

	    internal const string XhtmlNamespace = "http://www.w3.org/1999/xhtml";

        private readonly HtmlLexicalAnalyzer _htmlLexicalAnalyzer;

		private readonly XmlDocument _document;

	    private readonly Stack<XmlElement> _openedElements;
	    private readonly Stack<XmlElement> _pendingInlineElements;

	}
}
