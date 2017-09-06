using System;
using System.Globalization;
using System.IO;
using System.Text;
using System.Xml;

namespace Decomp.Windows.HtmlConverter
{
	internal static class HtmlFromXamlConverter
	{
		internal static string ConvertXamlToHtml(string xamlString)
		{
		    var xamlReader = new XmlTextReader(new StringReader(xamlString));

			var htmlStringBuilder = new StringBuilder(100);
			var htmlWriter = new XmlTextWriter(new StringWriter(htmlStringBuilder));

			if (!WriteFlowDocument(xamlReader, htmlWriter)) return "";

			var htmlString = htmlStringBuilder.ToString();

			return htmlString;
		}

		private static bool WriteFlowDocument(XmlTextReader xamlReader, XmlTextWriter htmlWriter)
		{
			if (!ReadNextToken(xamlReader)) return false;

			if (xamlReader.NodeType != XmlNodeType.Element || xamlReader.Name != "FlowDocument") return false;

			var inlineStyle = new StringBuilder();

			htmlWriter.WriteStartElement("HTML");
			htmlWriter.WriteStartElement("BODY");

			WriteFormattingProperties(xamlReader, htmlWriter, inlineStyle);

			WriteElementContent(xamlReader, htmlWriter, inlineStyle);

			htmlWriter.WriteEndElement();
			htmlWriter.WriteEndElement();

			return true;
		}

		private static void WriteFormattingProperties(XmlReader xamlReader, XmlWriter htmlWriter, StringBuilder inlineStyle)
		{
			inlineStyle.Remove(0, inlineStyle.Length);

			if (!xamlReader.HasAttributes) return;

			var borderSet = false;

			while (xamlReader.MoveToNextAttribute())
			{
				string css = null;

				switch (xamlReader.Name)
				{
					case "Background":
						css = "background-color:" + ParseXamlColor(xamlReader.Value) + ";";
						break;
					case "FontFamily":
						css = "font-family:" + xamlReader.Value + ";";
						break;
					case "FontStyle":
						css = "font-style:" + xamlReader.Value.ToLower() + ";";
						break;
					case "FontWeight":
						css = "font-weight:" + xamlReader.Value.ToLower() + ";";
						break;
					case "FontStretch":
						break;
					case "FontSize":
						css = "font-size:" + xamlReader.Value + ";";
						break;
					case "Foreground":
						css = "color:" + ParseXamlColor(xamlReader.Value) + ";";
						break;
					case "TextDecorations":
						css = "text-decoration:underline;";
						break;
					case "TextEffects":
						break;
					case "Emphasis":
						break;
					case "StandardLigatures":
						break;
					case "Variants":
						break;
					case "Capitals":
						break;
					case "Fraction":
						break;

					case "Padding":
						css = "padding:" + ParseXamlThickness(xamlReader.Value) + ";";
						break;
					case "Margin":
						css = "margin:" + ParseXamlThickness(xamlReader.Value) + ";";
						break;
					case "BorderThickness":
						css = "border-width:" + ParseXamlThickness(xamlReader.Value) + ";";
						borderSet = true;
						break;
					case "BorderBrush":
						css = "border-color:" + ParseXamlColor(xamlReader.Value) + ";";
						borderSet = true;
						break;
					case "LineHeight":
						break;
					case "TextIndent":
						css = "text-indent:" + xamlReader.Value + ";";
						break;
					case "TextAlignment":
						css = "text-align:" + xamlReader.Value + ";";
						break;
					case "IsKeptTogether":
						break;
					case "IsKeptWithNext":
						break;
					case "ColumnBreakBefore":
						break;
					case "PageBreakBefore":
						break;
					case "FlowDirection":
						break;

					case "Width":
						css = "width:" + xamlReader.Value + ";";
						break;
					case "ColumnSpan":
						htmlWriter.WriteAttributeString("COLSPAN", xamlReader.Value);
						break;
					case "RowSpan":
						htmlWriter.WriteAttributeString("ROWSPAN", xamlReader.Value);
						break;
				}

				if (css != null) inlineStyle.Append(css);
			}

			if (borderSet) inlineStyle.Append("border-style:solid;mso-element:para-border-div;");

			xamlReader.MoveToElement();
		}

		private static string ParseXamlColor(string color)
		{
			if (color.StartsWith("#")) color = "#" + color.Substring(3);
			return color;
		}

		private static string ParseXamlThickness(string thickness)
		{
			var values = thickness.Split(',');

			for (var i = 0; i < values.Length; i++)
			{
				double value;
				if (double.TryParse(values[i], out value))
					values[i] = Math.Ceiling(value).ToString(CultureInfo.GetCultureInfo("en-US"));
				else
					values[i] = "1";
			}

			string cssThickness;
			switch (values.Length)
			{
				case 1:
					cssThickness = thickness;
					break;
				case 2:
					cssThickness = values[1] + " " + values[0];
					break;
				case 4:
					cssThickness = values[1] + " " + values[2] + " " + values[3] + " " + values[0];
					break;
				default:
					cssThickness = values[0];
					break;
			}

			return cssThickness;
		}

		private static void WriteElementContent(XmlTextReader xamlReader, XmlTextWriter htmlWriter, StringBuilder inlineStyle)
		{
			var elementContentStarted = false;

			if (xamlReader.IsEmptyElement)
			{
			    if (htmlWriter == null || inlineStyle.Length <= 0) return;
			    htmlWriter.WriteAttributeString("STYLE", inlineStyle.ToString());
			    inlineStyle.Remove(0, inlineStyle.Length);
			}
			else
			{
				while (ReadNextToken(xamlReader) && xamlReader.NodeType != XmlNodeType.EndElement)
				{
					switch (xamlReader.NodeType)
					{
						case XmlNodeType.Element:
							if (xamlReader.Name.Contains("."))
								AddComplexProperty(xamlReader, inlineStyle);
							else
							{
								if (htmlWriter != null && !elementContentStarted && inlineStyle.Length > 0)
								{
									htmlWriter.WriteAttributeString("STYLE", inlineStyle.ToString());
									inlineStyle.Remove(0, inlineStyle.Length);
								}
								elementContentStarted = true;
								WriteElement(xamlReader, htmlWriter, inlineStyle);
							}
							break;
						case XmlNodeType.Comment:
							if (htmlWriter != null)
							{
								if (!elementContentStarted && inlineStyle.Length > 0) htmlWriter.WriteAttributeString("STYLE", inlineStyle.ToString());
								htmlWriter.WriteComment(xamlReader.Value);
							}
							elementContentStarted = true;
							break;
						case XmlNodeType.CDATA:
						case XmlNodeType.Text:
						case XmlNodeType.SignificantWhitespace:
							if (htmlWriter != null)
							{
								if (!elementContentStarted && inlineStyle.Length > 0) htmlWriter.WriteAttributeString("STYLE", inlineStyle.ToString());
								htmlWriter.WriteString(xamlReader.Value);
							}
							elementContentStarted = true;
							break;
					}
				}
			}
		}

		private static void AddComplexProperty(XmlTextReader xamlReader, StringBuilder inlineStyle)
		{
			if (inlineStyle != null && xamlReader.Name.EndsWith(".TextDecorations")) inlineStyle.Append("text-decoration:underline;");
            WriteElementContent(xamlReader, null, null);
		}

		private static void WriteElement(XmlTextReader xamlReader, XmlTextWriter htmlWriter, StringBuilder inlineStyle)
		{
			if (htmlWriter == null)
				WriteElementContent(xamlReader, null, null);
			else
			{
				string htmlElementName;

				switch (xamlReader.Name)
				{
					case "Run":
					case "Span":
						htmlElementName = "SPAN";
						break;
					case "InlineUIContainer":
						htmlElementName = "SPAN";
						break;
					case "Bold":
						htmlElementName = "B";
						break;
					case "Italic":
						htmlElementName = "I";
						break;
					case "Paragraph":
						htmlElementName = "P";
						break;
					case "BlockUIContainer":
						htmlElementName = "DIV";
						break;
					case "Section":
						htmlElementName = "DIV";
						break;
					case "Table":
						htmlElementName = "TABLE";
						break;
					case "TableColumn":
						htmlElementName = "COL";
						break;
					case "TableRowGroup":
						htmlElementName = "TBODY";
						break;
					case "TableRow":
						htmlElementName = "TR";
						break;
					case "TableCell":
						htmlElementName = "TD";
						break;
					case "List":
						var marker = xamlReader.GetAttribute("MarkerStyle");
						if (marker == null || marker == "None" || marker == "Disc" || marker == "Circle" || marker == "Square" || marker == "Box")
							htmlElementName = "UL";
						else
							htmlElementName = "OL";
						break;
					case "ListItem":
						htmlElementName = "LI";
						break;
					default:
						htmlElementName = null;
						break;
				}

				if (htmlElementName != null)
				{
					htmlWriter.WriteStartElement(htmlElementName);

					WriteFormattingProperties(xamlReader, htmlWriter, inlineStyle);

					WriteElementContent(xamlReader, htmlWriter, inlineStyle);

					htmlWriter.WriteEndElement();
				}
				else
					WriteElementContent(xamlReader, null, null);
			}
		}

		private static bool ReadNextToken(XmlReader xamlReader)
		{
			while (xamlReader.Read())
			{
				switch (xamlReader.NodeType)
				{
					case XmlNodeType.Element:
					case XmlNodeType.EndElement:
					case XmlNodeType.None:
					case XmlNodeType.CDATA:
					case XmlNodeType.Text:
					case XmlNodeType.SignificantWhitespace:
						return true;

					case XmlNodeType.Whitespace:
						if (xamlReader.XmlSpace == XmlSpace.Preserve) return true;
						break;

					case XmlNodeType.EndEntity:
					case XmlNodeType.EntityReference:
						break;

					case XmlNodeType.Comment:
						return true;
				}
			}
			return false;
		}

	}
}
