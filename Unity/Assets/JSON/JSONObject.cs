#define PRETTY      //Comment out when you no longer need to read JSON to disable pretty Print system-wide
//Using doubles will cause errors in VectorTemplates.cs; Unity speaks floats
#define USEFLOAT    //Use floats for numbers instead of doubles	(enable if you're getting too many significant digits in string output)
using System.Diagnostics;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System;
using System.Globalization;

public class JSONObject
{
	const int MAX_DEPTH = 100;
	const string INFINITY = "\"INFINITY\"";
	const string NEGINFINITY = "\"NEGINFINITY\"";
	const string NaN = "\"NaN\"";
	public static readonly char[] WHITESPACE = { ' ', '\r', '\n', '\t', '\uFEFF', '\u0009' };
	public enum Type
	{
		NULL,
		STRING,
		NUMBER,
		OBJECT,
		ARRAY,
		BOOL,
		BAKED
	}

	public bool isContainer { get { return (type == Type.ARRAY || type == Type.OBJECT); } }
	public Type type = Type.NULL;
	public int Count
	{
		get
		{
			if (type == Type.ARRAY)
				return list.Count;
			else if (type == Type.OBJECT)
				return dictionary.Count;
			else
				return -1;
		}
	}
	public List<JSONObject> list;
	public Dictionary<string, JSONObject> dictionary;
	public string str;
#if USEFLOAT
	public float n;
	public float f => n;
#else
	public double n;
	public float f => (float)n;
#endif
	public bool useInt;
	public long i;
	public bool b;

	public JSONObject(Type t)
	{
		type = t;
		switch (t)
		{
			case Type.ARRAY:
				list = new List<JSONObject>();
				break;
			case Type.OBJECT:
				dictionary = new Dictionary<string, JSONObject>();
				break;
		}
	}
	protected JSONObject() { }
	//Convenience function for creating a JSONObject containing a string.  This is not part of the constructor so that malformed JSON data doesn't just turn into a string object
	public static JSONObject StringObject(string val) { return CreateStringObject(val); }
	public void Absorb(JSONObject obj)
	{
		if (obj.list != null && obj.list.Count > 0)
		{
			list = list ?? new List<JSONObject>();
			list.AddRange(obj.list);
		}
		if (obj.dictionary != null && obj.dictionary.Count > 0)
		{
			dictionary = dictionary ?? new Dictionary<string, JSONObject>();
			foreach (var pair in obj.dictionary)
				dictionary[pair.Key] = pair.Value;
		}
		str = obj.str;
		n = obj.n;
		useInt = obj.useInt;
		i = obj.i;
		b = obj.b;
		type = obj.type;
	}
	public static JSONObject Create()
	{
		return new JSONObject();
	}
	public static JSONObject Create(bool val)
	{
		JSONObject obj = Create();
		obj.type = Type.BOOL;
		obj.b = val;
		return obj;
	}
	public static JSONObject Create(float val)
	{
		JSONObject obj = Create();
		obj.type = Type.NUMBER;
		obj.n = val;
		return obj;
	}
	public static JSONObject Create(int val)
	{
		JSONObject obj = Create();
		obj.type = Type.NUMBER;
		obj.n = val;
		obj.useInt = true;
		obj.i = val;
		return obj;
	}
	public static JSONObject Create(long val)
	{
		JSONObject obj = Create();
		obj.type = Type.NUMBER;
		obj.n = val;
		obj.useInt = true;
		obj.i = val;
		return obj;
	}
	public static JSONObject CreateStringObject(string val)
	{
		JSONObject obj = Create();
		obj.type = Type.STRING;
		obj.str = val;
		return obj;
	}
	public static JSONObject CreateBakedObject(string val)
	{
		JSONObject bakedObject = Create();
		bakedObject.type = Type.BAKED;
		bakedObject.str = val;
		return bakedObject;
	}
	/// <summary>
	/// Create a JSONObject by parsing string data
	/// </summary>
	/// <param name="val">The string to be parsed</param>
	/// <param name="maxDepth">The maximum depth for the parser to search.  Set this to to 1 for the first level,
	/// 2 for the first 2 levels, etc.  It defaults to -2 because -1 is the depth value that is parsed (see below)</param>
	/// <param name="storeExcessLevels">Whether to store levels beyond maxDepth in baked JSONObjects</param>
	/// <param name="strict">Whether to be strict in the parsing. For example, non-strict parsing will successfully
	/// parse "a string" into a string-type </param>
	/// <returns></returns>
	public static JSONObject Create(string val, int maxDepth = -2, bool storeExcessLevels = false, bool strict = false)
	{
		if (string.IsNullOrWhiteSpace(val))
			return new JSONObject(Type.NULL);
		else
		{
			ArraySegment<char> stringSegment = new ArraySegment<char>(val.ToCharArray(), 0, val.Length);
			stringSegment = GetSubStringSegment(stringSegment, 0, stringSegment.Count);
			JSONObject obj = Create();
			obj.Parse(stringSegment, maxDepth, storeExcessLevels, strict);
			return obj;
		}
	}
	public static JSONObject Create(Dictionary<string, string> dic)
	{
		JSONObject obj = Create();
		obj.type = Type.OBJECT;
		obj.dictionary = new Dictionary<string, JSONObject>();
		foreach (KeyValuePair<string, string> kvp in dic)
		{
			obj.dictionary.Add(kvp.Key, CreateStringObject(kvp.Value));
		}
		return obj;
	}
	public static JSONObject Create(IEnumerable<JSONObject> objs)
	{
		JSONObject obj = Create();
		obj.type = Type.ARRAY;
		obj.list = new List<JSONObject>(objs);
		return obj;
	}
	#region PARSE
	private static bool IsCharListEqual(IList<char> charList, string str, bool ignoreCase)
	{
		if (charList.Count != str.Length)
			return false;
		for (int i = 0; i < charList.Count; i++)
		{
			char c = str[i];
			if (charList[i] != c)
			{
				// small case to big case
				if (ignoreCase && c >= 97 && c <= 122)
				{
					c = (char)((int)c - 32);
					if (charList[i] != c)
						return false;
				}
				else
					return false;
			}
		}
		return true;
	}
	private static bool IsCharListEqualWhiteSpace(IList<char> charList, int start, int count)
	{
		if (count < 1)
			return true;
		int end = start + count;
		for (int i = end - 1; i >= start; i--)
		{
			char c = charList[i];
			if (c == ' ' || c == '\r' || c == '\n' || c == '\t' || c == '\uFEFF' || c == '\u0009')
				continue;
			else
				return false;
		}
		return true;
	}
	private static ArraySegment<char> GetSubStringSegment(ArraySegment<char> stringSegment, int start, int count)
	{
		int reallyStart = start;	
		int reallyEnd = start + count;
		char[] array = stringSegment.Array;
		char c = array[reallyStart];
		while (c == ' ' || c == '\r' || c == '\n' || c == '\t' || c == '\uFEFF' || c == '\u0009')
		{
			reallyStart++;
			c = array[reallyStart];
		}
		c = array[reallyEnd - 1];
		while (c == ' ' || c == '\r' || c == '\n' || c == '\t' || c == '\uFEFF' || c == '\u0009')
		{
			reallyEnd--;
			c = array[reallyEnd - 1];
		}
		return new ArraySegment<char>(stringSegment.Array, reallyStart, reallyEnd - reallyStart);
	}
	void Parse(ArraySegment<char> stringSegment, int maxDepth, bool storeExcessLevels, bool strict)
	{
		IList<char> charList = stringSegment;
		if (charList.Count > 0)
		{
			if (strict)
			{
				if (charList[0] != '[' && charList[0] != '{')
				{
					type = Type.NULL;
					Debug.WriteLine("Improper (strict) JSON formatting.  First character must be [ or {");
					return;
				}
			}
			if (IsCharListEqual(charList, "true", true))
			{
				type = Type.BOOL;
				b = true;
			}
			else if (IsCharListEqual(charList, "false", true))
			{
				type = Type.BOOL;
				b = false;
			}
			else if (IsCharListEqual(charList, "null", true))
			{
				type = Type.NULL;
			}
#if USEFLOAT
			else if (IsCharListEqual(charList, INFINITY, false))
			{
				type = Type.NUMBER;
				n = float.PositiveInfinity;
			}
			else if (IsCharListEqual(charList, NEGINFINITY, false))
			{
				type = Type.NUMBER;
				n = float.NegativeInfinity;
			}
			else if (IsCharListEqual(charList, NaN, false))
			{
				type = Type.NUMBER;
				n = float.NaN;
			}
#else
			else if (str == INFINITY)
			{
				type = Type.NUMBER;
				n = double.PositiveInfinity;
			}
			else if (str == NEGINFINITY)
			{
				type = Type.NUMBER;
				n = double.NegativeInfinity;
			}
			else if (str == NaN)
			{
				type = Type.NUMBER;
				n = double.NaN;
			}
#endif
			else if (charList[0] == '"')
			{
				type = Type.STRING;
				this.str = new string(stringSegment.Array, stringSegment.Offset + 1, stringSegment.Count - 2);
			}
			else
			{
				int tokenTmp = 1;
				/*
					* Checking for the following formatting (www.json.org)
					* object - {"field1":value,"field2":value}
					* array - [value,value,value]
					* value - string	- "string"
					*		 - number	- 0.0
					*		 - bool		- true -or- false
					*		 - null		- null
					*/
				int offset = 0;
				switch (charList[offset])
				{
					case '{':
						type = Type.OBJECT;
						dictionary = new Dictionary<string, JSONObject>();
						break;
					case '[':
						type = Type.ARRAY;
						list = new List<JSONObject>();
						break;
					default:
						try
						{
							string content = new string(stringSegment.Array, stringSegment.Offset, stringSegment.Count);
							if (content.Contains("."))
							{
#if USEFLOAT
								n = float.Parse(content, CultureInfo.InvariantCulture);
#else
								n = double.Parse(content, CultureInfo.InvariantCulture);
#endif
							}
							else
							{
								i = long.Parse(content, CultureInfo.InvariantCulture);
								useInt = true;
								n = i;
							}
							type = Type.NUMBER;
						}
						catch (System.FormatException)
						{
							type = Type.NULL;
							Debug.WriteLine("improper JSON formatting:" + new string(stringSegment.Array, stringSegment.Offset, stringSegment.Count));
						}
						return;
				}
				string propName = string.Empty;
				bool openQuote = false;
				bool inProp = false;
				int depth = 0;
				int maxOffset = charList.Count;
				while (++offset < maxOffset)
				{
					char c = charList[offset];
					if (c == ' ' || c == '\r' || c == '\n' || c == '\t' || c == '\uFEFF' || c == '\u0009')
						continue;
					if (c == '\\')
					{
						offset += 1;
						continue;
					}
					if (c == '"')
					{
						if (openQuote)
						{
							if (!inProp && depth == 0 && type == Type.OBJECT)
								propName = new string(stringSegment.Array, stringSegment.Offset + tokenTmp + 1, offset - tokenTmp - 1);
							openQuote = false;
						}
						else
						{
							if (depth == 0 && type == Type.OBJECT)
								tokenTmp = offset;
							openQuote = true;
						}
					}
					if (openQuote)
					{
						int shift = 1;
						while (charList[offset + shift] != '"' && charList[offset + shift] != '\\')
							shift++;
						offset += shift - 1;
						continue;
					}
					if (c == ':' && depth == 0 && type == Type.OBJECT)
					{
						tokenTmp = offset + 1;
						inProp = true;
					}
				AnalysisDepth:
					if (c == '[' || c == '{')
						depth++;
					else if (c == ']' || c == '}')
						depth--;
					if (depth > 0 && offset + 1 < maxOffset)
					{
						c = charList[offset + 1];
						if (c != '"')
						{
							offset++;
							goto AnalysisDepth;
						}
					}
					//if  (encounter a ',' at top level)  || a closing ]/}
					if (c == ',' && depth == 0 || depth < 0)
					{
						inProp = false;
						if (!IsCharListEqualWhiteSpace(charList, tokenTmp, offset - tokenTmp))
						{
							if (maxDepth != -1)                                                         //maxDepth of -1 is the end of the line
							{
								ArraySegment<char> inner = GetSubStringSegment(stringSegment, stringSegment.Offset + tokenTmp, offset - tokenTmp);
								JSONObject obj = Create();
								obj.Parse(inner, (maxDepth < -1) ? -2 : maxDepth - 1, false, false);
								if (type == Type.OBJECT)
									dictionary[propName] = obj;
								else if (type == Type.ARRAY)
									list.Add(obj);
							}
							else if (storeExcessLevels)
							{
								string inner = new string(stringSegment.Array, stringSegment.Offset + tokenTmp, offset - tokenTmp).Trim(WHITESPACE);
								JSONObject obj = CreateBakedObject(inner);
								if (type == Type.OBJECT)
									dictionary[propName] = obj;
								else if (type == Type.ARRAY)
									list.Add(obj);
							}
						}
						tokenTmp = offset + 1;
					}
				}
			}
		}
		else
			type = Type.NULL;  //If the string is missing, this is a null
	}
	#endregion
	public bool IsNumber { get { return type == Type.NUMBER; } }
	public bool IsNull { get { return type == Type.NULL; } }
	public bool IsString { get { return type == Type.STRING; } }
	public bool IsBool { get { return type == Type.BOOL; } }
	public bool IsArray { get { return type == Type.ARRAY; } }
	public bool IsObject { get { return type == Type.OBJECT || type == Type.BAKED; } }
	public void Add(bool val)
	{
		Add(Create(val));
	}
	public void Add(float val)
	{
		Add(Create(val));
	}
	public void Add(int val)
	{
		Add(Create(val));
	}
	public void Add(string str)
	{
		Add(CreateStringObject(str));
	}
	public void Add(JSONObject obj)
	{
		if (obj != null)
		{
			//Don't do anything if the object is null
			if (type != Type.ARRAY)
			{
				//Congratulations, son, you're an ARRAY now
				type = Type.ARRAY;
				if (list == null)
					list = new List<JSONObject>();
			}
			list.Add(obj);
		}
	}
	public void AddField(string name, bool val)
	{
		AddField(name, Create(val));
	}
	public void AddField(string name, float val)
	{
		AddField(name, Create(val));
	}
	public void AddField(string name, int val)
	{
		AddField(name, Create(val));
	}
	public void AddField(string name, long val)
	{
		AddField(name, Create(val));
	}
	public void AddField(string name, string val)
	{
		AddField(name, CreateStringObject(val));
	}
	public void AddField(string name, JSONObject obj)
	{
		if (obj != null)
		{
			//Don't do anything if the object is null
			if (type != Type.OBJECT)
			{
				dictionary = dictionary ?? new Dictionary<string, JSONObject>();
				if (type == Type.ARRAY && list != null)
				{
					for (int i = 0; i < list.Count; i++)
						dictionary[i.ToString()] = list[i];
					list.Clear();
				}
				//Congratulations, son, you're an OBJECT now
				type = Type.OBJECT;
			}
			dictionary[name] = obj;
		}
	}
	public void SetField(string name, string val) { SetField(name, CreateStringObject(val)); }
	public void SetField(string name, bool val) { SetField(name, Create(val)); }
	public void SetField(string name, float val) { SetField(name, Create(val)); }
	public void SetField(string name, int val) { SetField(name, Create(val)); }
	public void SetField(string name, JSONObject obj)
	{
		if (type == Type.OBJECT)
			dictionary[name] = obj;
		else
			AddField(name, obj);
	}
	public void RemoveField(string name)
	{
		dictionary.Remove(name);
	}
	public bool TryGetField(out bool value, string name)
	{
		if (type == Type.OBJECT &&
			dictionary.TryGetValue(name, out JSONObject field))
		{
			value = field.b;
			return true;
		}
		else
		{
			value = default;
			return false;
		}
	}
#if USEFLOAT
	public bool TryGetField(out float value, string name)
	{
#else
	public bool TryGetField(out double value, string name)
	{
#endif
		if (type == Type.OBJECT &&
			dictionary.TryGetValue(name, out JSONObject field))
		{
			value = field.f;
			return true;
		}
		else
		{
			value = default;
			return false;
		}
	}
	public bool TryGetField(out int value, string name)
	{
		if (type == Type.OBJECT &&
			dictionary.TryGetValue(name, out JSONObject field))
		{
			value = (int)field.i;
			return true;
		}
		else
		{
			value = default;
			return false;
		}
	}
	public bool TryGetField(out long value, string name)
	{
		if (type == Type.OBJECT &&
			dictionary.TryGetValue(name, out JSONObject field))
		{
			value = field.i;
			return true;
		}
		else
		{
			value = default;
			return false;
		}
	}
	public bool TryGetField(out uint value, string name)
	{
		if (type == Type.OBJECT &&
			dictionary.TryGetValue(name, out JSONObject field))
		{
			value = (uint)field.i;
			return true;
		}
		else
		{
			value = default;
			return false;
		}
	}
	public bool TryGetField(out string value, string name)
	{
		if (type == Type.OBJECT &&
			dictionary.TryGetValue(name, out JSONObject field))
		{
			value = field.str;
			return true;
		}
		else
		{
			value = default;
			return false;
		}
	}
	public JSONObject GetField(string name)
	{
		if (IsObject && dictionary.TryGetValue(name, out JSONObject field))
			return field;
		return null;
	}
	public bool HasFields(IEnumerable<string> names)
	{
		if (!IsObject)
			return false;
		foreach (string name in names)
		{
			if (!dictionary.ContainsKey(name))
				return false;
		}
		return true;
	}
	public bool HasField(string name)
	{
		if (!IsObject)
			return false;
		return dictionary.ContainsKey(name);
	}
	public void Clear()
	{
		type = Type.NULL;
		if (list != null)
			list.Clear();
		if (dictionary != null)
			dictionary.Clear();
		str = string.Empty;
		n = 0;
		b = false;
	}
	/// <summary>
	/// Copy a JSONObject. This could probably work better
	/// </summary>
	/// <returns></returns>
	public JSONObject DeepCopy() => Create(Print());

	private static readonly object LockObject = new object();
	private static StringBuilder m_CachedStringBuilder;
	private static StringBuilder CachedStringBuilder
	{
		get
		{
			lock (LockObject)
			{
				if (m_CachedStringBuilder != null)
				{
					var temp = m_CachedStringBuilder;
					m_CachedStringBuilder = null;
					temp.Length = 0;
					return temp;
				}
				else
					return new StringBuilder(2048);
			}
		}
		set
		{
			lock (LockObject)
			{
				m_CachedStringBuilder = value;
				if (value != null)
					value.Length = 0;
			}
		}
	}

	/// <summary>
	/// 烘焙当前<see cref="JSONObject"/>的数据到字符串缓存
	/// </summary>
	/// <param name="deleteChilds">如果烘焙完成且选择移除子节点，将丢弃子节点引用</param>
	public void Bake(bool deleteChilds)
	{
		if (type != Type.BAKED)
		{
			str = Print();
            if (deleteChilds)
            {
				if (type == Type.ARRAY)
					list = null;
				else if (type == Type.OBJECT)
				{
					dictionary = null;
					list = null;
				}
            }
			type = Type.BAKED;
		}
	}
	public string Print(bool pretty = false)
	{
		StringBuilder builder = CachedStringBuilder;
		Stringify(0, builder, pretty);
		string content = builder.ToString();
		CachedStringBuilder = builder;
		return content;
	}
	#region STRINGIFY
	//TODO: Refactor Stringify functions to share core logic
	/*
		* I know, I know, this is really bad form.  It turns out that there is a
		* significant amount of garbage created when calling as a coroutine, so this
		* method is duplicated.  Hopefully there won't be too many future changes, but
		* I would still like a more elegant way to optionaly yield
		*/
	void Stringify(int depth, StringBuilder builder, bool pretty = false)
	{   //Convert the JSONObject into a string
		if (depth++ > MAX_DEPTH)
		{
			Debug.WriteLine("reached max depth!");
			return;
		}
		switch (type)
		{
			case Type.BAKED:
				builder.Append(str);
				break;
			case Type.STRING:
				builder.AppendFormat("\"{0}\"", str);
				break;
			case Type.NUMBER:
				if (useInt)
				{
					builder.Append(i.ToString());
				}
				else
				{
#if USEFLOAT
					if (float.IsInfinity(n))
						builder.Append(INFINITY);
					else if (float.IsNegativeInfinity(n))
						builder.Append(NEGINFINITY);
					else if (float.IsNaN(n))
						builder.Append(NaN);
#else
					if(double.IsInfinity(n))
						builder.Append(INFINITY);
					else if(double.IsNegativeInfinity(n))
						builder.Append(NEGINFINITY);
					else if(double.IsNaN(n))
						builder.Append(NaN);
#endif
					else
						builder.Append(n.ToString());
				}
				break;
			case Type.OBJECT:
				builder.Append("{");
				if (dictionary.Count > 0)
				{
#if (PRETTY)        //for a bit more readability, comment the define above to disable system-wide
					if (pretty)
						builder.Append("\n");
#endif
					foreach (var pair in dictionary)
					{
						string key = pair.Key;
						JSONObject obj = pair.Value;
						if (obj != null)
						{
#if (PRETTY)
							if (pretty)
								for (int j = 0; j < depth; j++)
									builder.Append("\t"); //for a bit more readability
#endif
							builder.AppendFormat("\"{0}\":", key);
							obj.Stringify(depth, builder, pretty);
							builder.Append(",");
#if (PRETTY)
							if (pretty)
								builder.Append("\n");
#endif
						}
					}
#if (PRETTY)
					if (pretty)
						builder.Length -= 2;
					else
#endif
						builder.Length--;
				}
#if (PRETTY)
				if (pretty && dictionary.Count > 0)
				{
					builder.Append("\n");
					for (int j = 0; j < depth - 1; j++)
						builder.Append("\t"); //for a bit more readability
				}
#endif
				builder.Append("}");
				break;
			case Type.ARRAY:
				builder.Append("[");
				if (list.Count > 0)
				{
#if (PRETTY)
					if (pretty)
						builder.Append("\n"); //for a bit more readability
#endif
					for (int i = 0; i < list.Count; i++)
					{
						if (list[i] != null)
						{
#if (PRETTY)
							if (pretty)
								for (int j = 0; j < depth; j++)
									builder.Append("\t"); //for a bit more readability
#endif
							list[i].Stringify(depth, builder, pretty);
							builder.Append(",");
#if (PRETTY)
							if (pretty)
								builder.Append("\n"); //for a bit more readability
#endif
						}
					}
#if (PRETTY)
					if (pretty)
						builder.Length -= 2;
					else
#endif
						builder.Length--;
				}
#if (PRETTY)
				if (pretty && list.Count > 0)
				{
					builder.Append("\n");
					for (int j = 0; j < depth - 1; j++)
						builder.Append("\t"); //for a bit more readability
				}
#endif
				builder.Append("]");
				break;
			case Type.BOOL:
				if (b)
					builder.Append("true");
				else
					builder.Append("false");
				break;
			case Type.NULL:
				builder.Append("null");
				break;
		}
	}
	#endregion
	public JSONObject this[int index]
	{
		get
		{
			if (type == Type.ARRAY && list.Count > index)
				return list[index];
			return null;
		}
		set
		{
			if (type == Type.ARRAY && list.Count > index)
				list[index] = value;
		}
	}
	public JSONObject this[string index]
	{
		get
		{
			return GetField(index);
		}
		set
		{
			SetField(index, value);
		}
	}
	public override string ToString()
	{
		return Print();
	}
	public string ToString(bool pretty)
	{
		return Print(pretty);
	}
}