namespace Decomp.Windows.HtmlConverter
{
	internal enum HtmlTokenType
	{
		OpeningTagStart,
		ClosingTagStart,
		TagEnd,
		EmptyTagEnd,
		EqualSign,
		Name,
		Atom,
		Text,
		Comment,
		Eof
	}
}
