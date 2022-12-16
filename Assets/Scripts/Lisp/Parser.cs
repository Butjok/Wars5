using System;
using UnityEngine;

namespace Lisp
{
	static public class Parser
	{
		static public int Parse(Token[] tokens, int start, out Value result)
		{
			if (tokens.Length==0)
			{
				result = null;
				return 0;
			}
			{
				var token = tokens[start] as IntToken;
				if (token != null)
				{
					result = new Int(token.val);
					return start + 1;
				}
			}
			{
				var token = tokens[start] as FloatToken;
				if (token != null)
				{
					result = new Float(token.val);
					return start + 1;
				}
			}
			{
				var token = tokens[start] as StrToken;
				if (token != null)
				{
					result = new Str(token.val);
					Debug.Log(token.val);
					return start + 1;
				}
			}
			{
				var token = tokens[start] as SymbolToken;
				if (token != null)
				{
					result = new Symbol(token.name);
					return start + 1;
				}
			}
			{
				var token = tokens[start] as KeywordToken;
				if (token != null)
				{
					result = new Keyword(token.name);
					return start + 1;
				}
			}
			{
				var token = tokens[start] as QuoteToken;
				if (token != null && start + 1 < tokens.Length)
				{
					Value quoted;
					start = Parse(tokens, start + 1, out quoted);
					result = new Vector(new Symbol("quote"), quoted);
					return start;
				}
			}
			if (tokens[start] is LeftParenToken)
			{
				var vector = new Vector();
				start++;
				while (start < tokens.Length && !(tokens[start] is RightParenToken))
				{
					Value item;
					start = Parse(tokens, start, out item);
					vector.items.Add(item);
				}
				result = vector;
				return start + 1;
			}
			throw new Exception($"bad token {tokens[start].GetType().Name}");
		}
	}
}