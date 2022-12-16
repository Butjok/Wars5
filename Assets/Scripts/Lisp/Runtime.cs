using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Lisp
{
	public class RuntimeError : Exception
	{
		public Value expr;
		public RuntimeError(Value expr, string message)
			: base(message)
		{
			this.expr = expr;
		}
	}

	public class Environment
	{
		public Dictionary<string, Value> dict = new Dictionary<string, Value>();

		public Environment parent;
		public Environment(Environment parent = null)
		{
			this.parent = parent;
		}
		public Environment Find(string name)
		{
			for (var current = this; current != null; current = current.parent)
			{
				Value val;
				if (current.dict.TryGetValue(name, out val))
				{
					return current;
				}
			}
			return null;
		}
	}

	abstract public class Value
	{
		abstract public bool isTrue { get; }
		abstract public Value Eval(Environment env);

		static protected void Check(bool condition, string message, Value obj)
		{
			if (!condition)
			{
				throw new RuntimeError(obj, message);
			}
		}
		protected void Check(bool condition, string message)
		{
			Check(condition, message, this);
		}
	}

	public class Bool : Value
	{
		public bool val;
		public Bool(bool val)
		{
			this.val = val;
		}

		static public implicit operator bool(Bool val) => val.val;
		static public implicit operator Bool(bool val) => new Bool(val);

		public override bool isTrue => this;

		public override Value Eval(Environment env)
			=> this;
	}

	public class Int : Value
	{
		public int val;
		public Int(int val)
		{
			this.val = val;
		}
		static public implicit operator int(Int val) => val.val;
		static public implicit operator Int(int val) => new Int(val);

		static public implicit operator bool(Int val) => val.val!=0;
		public override bool isTrue => this;

		public override Value Eval(Environment env)
			=> this;
	}

	public class Float : Value
	{
		public float val;
		public Float(float val)
		{
			this.val = val;
		}
		static public implicit operator float(Float val) => val.val;
		static public implicit operator Float(float val) => new Float(val);

		static public implicit operator bool(Float val)
		{
			Check(false, "cannot implicitly cast to bool", val);
			return false;
		}
		public override bool isTrue => this;

		public override Value Eval(Environment env)
			=> this;
	}

	public class Str : Value
	{
		public string val;
		public Str(string val)
		{
			this.val = val;
		}
		static public implicit operator string(Str val) => val.val;
		static public implicit operator Str(string val) => new Str(val);

		static public implicit operator bool(Str val) => val.val != "";
		public override bool isTrue => this;

		public override Value Eval(Environment env)
			=> this;
	}

	public class Symbol : Value
	{
		public string name;
		public Symbol(string name)
		{
			this.name = name;
		}

		static public implicit operator bool(Symbol val) => true;
		public override bool isTrue => this;

		public override Value Eval(Environment env)
		{
			if (name == "nil")
			{
				return null;
			}
			if (name == "true")
			{
				return new Bool(true);
			}
			if (name == "false")
			{
				return new Bool(false);
			}
			var symbolEnv = env.Find(name);
			Check(symbolEnv != null, $"undefined variable `{name}'");
			return symbolEnv.dict[name];
		}
	}

	public class Keyword : Symbol
	{
		public Keyword(string name)
			: base(name)
		{
		}
		public override Value Eval(Environment env)
			=> this;
	}

	public class Dict : Value
	{
		public Dictionary<Keyword, Value> val;
		public Dict(Dictionary<Keyword, Value> val = null)
		{
			this.val = val ?? new Dictionary<Keyword, Value>();
		}

		static public implicit operator bool(Dict val) => val.val.Count > 0;
		public override bool isTrue => this;

		public override Value Eval(Environment env)
			=> this;
	}

	public class Closure : Value
	{
		public string[] argnames;
		public Vector body;
		public Environment env;
		public Closure(string[] argnames, Vector body, Environment env)
		{
			this.argnames = argnames;
			this.body = body;
			this.env = env;
		}

		static public implicit operator bool(Closure val) => true;
		public override bool isTrue => this;

		public override Value Eval(Environment env)
			=> this;

		public Value Call(params Value[] args)
		{
			Check(
				argnames.Length == args.Length,
				$"function must take exactly {argnames.Length} arguments, {args.Length} given");

			var subenv = new Environment(env);
			for (var i = 0; i < args.Length; i++)
			{
				subenv.dict[argnames[i]] = args[i];
			}

			return body.Eval(subenv);
		}
	}

	public class HostFunction : Value
	{
		public Func<Value[], Value> function;
		public HostFunction(Func<Value[], Value> function)
		{
			this.function = function;
		}

		static public implicit operator bool(HostFunction val) => true;
		public override bool isTrue => this;

		public override Value Eval(Environment env)
			=> this;
		public Value Call(params Value[] args)
			=> function(args);
	}

	public class HostObject : Value
	{
		public object val;
		public HostObject(object val = null)
		{
			this.val = val;
		}

		static public implicit operator bool(HostObject val) => val.val!=null;
		public override bool isTrue => this;

		public override Value Eval(Environment env)
			=> this;
	}

	public class Vector : Value
	{
		public List<Value> items = new List<Value>();
		public Vector(params Value[] items)
		{
			this.items.AddRange(items);
		}

		static public implicit operator bool(Vector val) => val.items.Count>0;
		public override bool isTrue => this;

		public override Value Eval(Environment env)
		{
			if (items.Count == 0)
			{
				return this;
			}

			var first = items[0];
			var symbol = first as Symbol;

			if (symbol != null)
			{
				if (symbol.name == "quote")
				{
					return EvalQuote(env);
				}
				if (symbol.name == "if")
				{
					return EvalIf(env);
				}
				if (symbol.name == "do")
				{
					return EvalDo(env);
				}
				if (symbol.name == "def")
				{
					return EvalDef(env);
				}
				if (symbol.name == "set!")
				{
					return EvalSet(env);
				}
				if (symbol.name == ".")
				{
					return EvalMethodCall(env);
				}
			}

			return EvalCall(env);
		}
		public Value EvalQuote(Environment env)
		{
			Check(items.Count == 2, "must take exactly one argument");
			return items[1];
		}
		public Value EvalIf(Environment env)
		{
			Check(items.Count == 3 || items.Count == 4, "must take 2 or 3 arguments");
			var test = items[1];
			var conseq = items[2];
			var alt = items.Count == 4 ? items[3] : null;

			return test.Eval(env).isTrue ? conseq.Eval(env) : alt.Eval(env);
		}
		public Value EvalDo(Environment env)
		{
			Value result = null;
			foreach (var exp in items.Skip(1))
			{
				result = exp.Eval(env);
			}
			return result;
		}
		public Value EvalDef(Environment env)
		{
			Check(items.Count == 3, "must take exactly 2 arguments");
			var symbol = items[1];
			var val = items[2];
			Check(symbol is Symbol, "cannot define non-symbol");
			var name = ((Symbol)symbol).name;
			var symbolEnv = env.Find(name);
			Check(symbolEnv != env, $"cannot redefine variable `{name}'");
			return env.dict[name] = val.Eval(env);
		}
		public Value EvalSet(Environment env)
		{
			Check(items.Count == 3, "must take exactly 2 arguments");
			var symbol = items[1];
			var val = items[2];
			Check(symbol is Symbol, "cannot set non-symbol");
			var name = ((Symbol)symbol).name;
			var symbolEnv = env.Find(name);
			Check(symbolEnv != null, $"cannot set undefined variable `{name}'");
			return symbolEnv.dict[name] = val.Eval(env);
		}
		public Value EvalCall(Environment env)
		{
			var values = items.Select(it => it.Eval(env)).ToArray();
			var first = values[0];
			var args = values.Skip(1).ToArray();

			var closure = first as Closure;
			var hostFunction = first as HostFunction;

			Check(closure != null || hostFunction != null, $"cannot call on {first.GetType().Name}");
			return closure != null ? closure.Call(args) : hostFunction.Call(args);
		}
		public Value EvalMethodCall(Environment env)
		{
			var values = items.Skip(1).Select(it => it.Eval(env)).ToArray();
			var _obj = values[0];
			var _methodName = values[1];
			var args = values.Skip(2).ToArray();

			Check(_obj is HostObject, "cannot call method on non-host object");
			Check(_methodName is Keyword, "method name must be a keyword");

			var obj = (HostObject)_obj;
			var methodName = (Keyword)_methodName;

			Check(obj.val != null, "cannot call method on null host-object");
			var method = obj.val.GetType().GetMethod(methodName.name, BindingFlags.Public | BindingFlags.Instance);
			Check(method != null, $"method `{methodName.name}' is not found");

			var result = method.Invoke(obj.val, args.Select(arg => (object)arg).ToArray());
			Check(result is Value, "method call must return Wisp value");

			return (Value)result;
		}
	}
}