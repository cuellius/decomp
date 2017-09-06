using System;
using System.IO;
using System.Text;

namespace Decomp.Windows.HtmlConverter
{
	internal class HtmlLexicalAnalyzer
	{
		internal HtmlLexicalAnalyzer(string inputTextString)
		{
			_inputStringReader = new StringReader(inputTextString);
			_nextCharacterCode = 0;
			NextCharacter = ' ';
			_lookAheadCharacterCode = _inputStringReader.Read();
			_lookAheadCharacter = (char)_lookAheadCharacterCode;
			_previousCharacter = ' ';
			_ignoreNextWhitespace = true;
			_nextToken = new StringBuilder(100);
			NextTokenType = HtmlTokenType.Text;
			GetNextCharacter();
		}

		internal void GetNextContentToken()
		{
			_nextToken.Length = 0;
			if (IsAtEndOfStream)
			{
				NextTokenType = HtmlTokenType.Eof;
				return;
			}

			if (IsAtTagStart)
			{
				GetNextCharacter();

				if (NextCharacter == '/')
				{
					_nextToken.Append("</");
					NextTokenType = HtmlTokenType.ClosingTagStart;

					GetNextCharacter();
					_ignoreNextWhitespace = false;
				}
				else
				{
					NextTokenType = HtmlTokenType.OpeningTagStart;
					_nextToken.Append("<");
					_ignoreNextWhitespace = true;
				}
			}
			else if (IsAtDirectiveStart)
			{
			    GetNextCharacter();
			    switch (_lookAheadCharacter)
			    {
			        case '[':
			            ReadDynamicContent();
			            break;
			        case '-':
			            ReadComment();
			            break;
			        default:
			            ReadUnknownDirective();
			            break;
			    }
			}
			else
			{
				NextTokenType = HtmlTokenType.Text;
				while (!IsAtTagStart && !IsAtEndOfStream && !IsAtDirectiveStart)
				{
					if (NextCharacter == '<' && !IsNextCharacterEntity && _lookAheadCharacter == '?')
						SkipProcessingDirective();
					else
					{
						if (NextCharacter <= ' ')
						{
							if (!_ignoreNextWhitespace) _nextToken.Append(' ');
							_ignoreNextWhitespace = true;
						}
						else
						{
							_nextToken.Append(NextCharacter);
							_ignoreNextWhitespace = false;
						}
						GetNextCharacter();
					}
				}
			}
		}

		internal void GetNextTagToken()
		{
			_nextToken.Length = 0;
			if (IsAtEndOfStream)
			{
				NextTokenType = HtmlTokenType.Eof;
				return;
			}

			SkipWhiteSpace();

			if (NextCharacter == '>' && !IsNextCharacterEntity)
			{
				NextTokenType = HtmlTokenType.TagEnd;
				_nextToken.Append('>');
				GetNextCharacter();
			}
			else if (NextCharacter == '/' && _lookAheadCharacter == '>')
			{
				NextTokenType = HtmlTokenType.EmptyTagEnd;
				_nextToken.Append("/>");
				GetNextCharacter();
				GetNextCharacter();
				_ignoreNextWhitespace = false;
			}
			else if (IsGoodForNameStart(NextCharacter))
			{
				NextTokenType = HtmlTokenType.Name;

				while (IsGoodForName(NextCharacter) && !IsAtEndOfStream)
				{
					_nextToken.Append(NextCharacter);
					GetNextCharacter();
				}
			}
			else
			{
				NextTokenType = HtmlTokenType.Atom;
				_nextToken.Append(NextCharacter);
				GetNextCharacter();
			}
		}

		internal void GetNextEqualSignToken()
		{
			_nextToken.Length = 0;

			_nextToken.Append('=');
			NextTokenType = HtmlTokenType.EqualSign;

			SkipWhiteSpace();

			if (NextCharacter == '=') GetNextCharacter();
		}

		internal void GetNextAtomToken()
		{
			_nextToken.Length = 0;

			SkipWhiteSpace();

			NextTokenType = HtmlTokenType.Atom;

			if ((NextCharacter == '\'' || NextCharacter == '"') && !IsNextCharacterEntity)
			{
				var startingQuote = NextCharacter;
				GetNextCharacter();

				while (!(NextCharacter == startingQuote && !IsNextCharacterEntity) && !IsAtEndOfStream)
				{
					_nextToken.Append(NextCharacter);
					GetNextCharacter();
				}
				if (NextCharacter == startingQuote) GetNextCharacter();

			}
			else
			{
				while (!IsAtEndOfStream && !Char.IsWhiteSpace(NextCharacter) && NextCharacter != '>')
				{
					_nextToken.Append(NextCharacter);
					GetNextCharacter();
				}
			}
		}

		internal HtmlTokenType NextTokenType { get; private set; }

	    internal string NextToken => _nextToken.ToString();

		private void GetNextCharacter()
		{
			if (_nextCharacterCode == -1) throw new InvalidOperationException("GetNextCharacter method called at the end of a stream");

			_previousCharacter = NextCharacter;

			NextCharacter = _lookAheadCharacter;
			_nextCharacterCode = _lookAheadCharacterCode;
			IsNextCharacterEntity = false;

			ReadLookAheadCharacter();

			if (NextCharacter == '&')
			{
				if (_lookAheadCharacter == '#')
				{
				    var entityCode = 0;
					ReadLookAheadCharacter();

					for (var i = 0; i < 7 && Char.IsDigit(_lookAheadCharacter); i++)
					{
						entityCode = 10 * entityCode + (_lookAheadCharacterCode - '0');
						ReadLookAheadCharacter();
					}
					if (_lookAheadCharacter == ';')
					{
						ReadLookAheadCharacter();
						_nextCharacterCode = entityCode;

						NextCharacter = (char)_nextCharacterCode;

						IsNextCharacterEntity = true;
					}
					else
					{
						NextCharacter = _lookAheadCharacter;
						_nextCharacterCode = _lookAheadCharacterCode;
						ReadLookAheadCharacter();
						IsNextCharacterEntity = false;
					}
				}
				else if (Char.IsLetter(_lookAheadCharacter))
				{
					var entity = "";

					for (var i = 0; i < 10 && (Char.IsLetter(_lookAheadCharacter) || Char.IsDigit(_lookAheadCharacter)); i++)
					{
						entity += _lookAheadCharacter;
						ReadLookAheadCharacter();
					}
					if (_lookAheadCharacter == ';')
					{
						ReadLookAheadCharacter();

						if (HtmlSchema.IsEntity(entity))
						{
							NextCharacter = HtmlSchema.EntityCharacterValue(entity);
							_nextCharacterCode = NextCharacter;
							IsNextCharacterEntity = true;
						}
						else
						{
							NextCharacter = _lookAheadCharacter;
							_nextCharacterCode = _lookAheadCharacterCode;
							ReadLookAheadCharacter();

							IsNextCharacterEntity = false;
						}
					}
					else
					{
						NextCharacter = _lookAheadCharacter;
						ReadLookAheadCharacter();
						IsNextCharacterEntity = false;
					}
				}
			}
		}

		private void ReadLookAheadCharacter()
		{
			if (_lookAheadCharacterCode != -1)
			{
				_lookAheadCharacterCode = _inputStringReader.Read();
				_lookAheadCharacter = (char)_lookAheadCharacterCode;
			}
		}

		private void SkipWhiteSpace()
		{
			while (true)
			{
				if (NextCharacter == '<' && (_lookAheadCharacter == '?' || _lookAheadCharacter == '!'))
				{
					GetNextCharacter();

					if (_lookAheadCharacter == '[')
					{
						while (!IsAtEndOfStream && !(_previousCharacter == ']' && NextCharacter == ']' && _lookAheadCharacter == '>')) GetNextCharacter();
						if (NextCharacter == '>') GetNextCharacter();
					}
					else
					{
						while (!IsAtEndOfStream && NextCharacter != '>') GetNextCharacter();
						if (NextCharacter == '>') GetNextCharacter();
					}
				}

				if (!Char.IsWhiteSpace(NextCharacter)) break;

				GetNextCharacter();
			}
		}

		private static bool IsGoodForNameStart(char character)
		{
			return character == '_' || Char.IsLetter(character);
		}

		private static bool IsGoodForName(char character)
		{
			return
					IsGoodForNameStart(character) ||
					character == '.' ||
					character == '-' ||
					character == ':' ||
					Char.IsDigit(character);
		}

		private void ReadDynamicContent()
		{
			NextTokenType = HtmlTokenType.Text;
			_nextToken.Length = 0;

			GetNextCharacter();
			GetNextCharacter();

			while (!(NextCharacter == ']' && _lookAheadCharacter == '>') && !IsAtEndOfStream) GetNextCharacter();

		    if (IsAtEndOfStream) return;
		    GetNextCharacter();
		    GetNextCharacter();
		}

		private void ReadComment()
		{
			NextTokenType = HtmlTokenType.Comment;
			_nextToken.Length = 0;

			GetNextCharacter();
			GetNextCharacter();
			GetNextCharacter();

			while (true)
			{
				while (!IsAtEndOfStream && !(NextCharacter == '-' && _lookAheadCharacter == '-' || NextCharacter == '!' && _lookAheadCharacter == '>'))
				{
					_nextToken.Append(NextCharacter);
					GetNextCharacter();
				}

				GetNextCharacter();
				if (_previousCharacter == '-' && NextCharacter == '-' && _lookAheadCharacter == '>')
				{
					GetNextCharacter();
					break;
				}
			    if (_previousCharacter == '!' && NextCharacter == '>') break;
			    _nextToken.Append(_previousCharacter);
			}

			if (NextCharacter == '>') GetNextCharacter();
		}

		private void ReadUnknownDirective()
		{
			NextTokenType = HtmlTokenType.Text;
			_nextToken.Length = 0;

			GetNextCharacter();

			while (!(NextCharacter == '>' && !IsNextCharacterEntity) && !IsAtEndOfStream) GetNextCharacter();

			if (!IsAtEndOfStream) GetNextCharacter();
		}

		private void SkipProcessingDirective()
		{
			GetNextCharacter();
			GetNextCharacter();

			while (!((NextCharacter == '?' || NextCharacter == '/') && _lookAheadCharacter == '>') && !IsAtEndOfStream) GetNextCharacter();

		    if (IsAtEndOfStream) return;
		    GetNextCharacter(); 
		    GetNextCharacter();
		}

		private char NextCharacter { get; set; }

	    private bool IsAtEndOfStream => _nextCharacterCode == -1;

	    private bool IsAtTagStart => NextCharacter == '<' && (_lookAheadCharacter == '/' || IsGoodForNameStart(_lookAheadCharacter)) && !IsNextCharacterEntity;

	    private bool IsAtDirectiveStart => NextCharacter == '<' && _lookAheadCharacter == '!' && !IsNextCharacterEntity;

	    private bool IsNextCharacterEntity { get; set; }

		private readonly StringReader _inputStringReader;
		private int _nextCharacterCode;

	    private int _lookAheadCharacterCode;
		private char _lookAheadCharacter;
		private char _previousCharacter;
		private bool _ignoreNextWhitespace;

	    private readonly StringBuilder _nextToken;

	}
}
