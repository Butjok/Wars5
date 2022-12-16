namespace Lisp
{
	abstract public class Token
	{
	}
	public class LeftParenToken : Token
	{
	}
	public class RightParenToken : Token
	{
	}
	public class QuoteToken : Token
	{
	}
	public class IntToken : Token
	{
		public int val;
	}
	public class FloatToken : Token
	{
		public float val;
	}
	public class StrToken : Token
	{
		public string val;
	}
	public class SymbolToken : Token
	{
		public string name;
	}
	public class KeywordToken : Token
	{
		public string name;
	}
}