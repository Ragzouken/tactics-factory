using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Assertions;
using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;

namespace AST
{
    public enum Type
    {
        Error,
        Boolean,
        Number,
        Object,
        Position,
        Action,
    }

    public struct FullType
    {
        public static FullType ERROR     = new FullType { type = Type.Error };

        public static FullType NUMBER    = new FullType { type = Type.Number };
        public static FullType NUMBERS   = new FullType { type = Type.Number, collection = true };

        public static FullType POSITION  = new FullType { type = Type.Position };
        public static FullType POSITIONS = new FullType { type = Type.Position, collection = true };

        public static FullType OBJECT    = new FullType { type = Type.Object };
        public static FullType OBJECTS   = new FullType { type = Type.Object, collection = true };

        public static FullType BOOLEAN   = new FullType { type = Type.Boolean };
        public static FullType BOOLEANS  = new FullType { type = Type.Boolean, collection = true };

        public Type type;
        public bool collection;

        public override string ToString()
        {
            return type.ToString() + (collection ? "s" : "");
        }

        public override int GetHashCode()
        {
            return collection ? ~(int)type : (int)type;
        }

        public bool Equals(FullType other)
        {
            return other.type == type && other.collection == collection;
        }

        public override bool Equals(object obj)
        {
            return obj != null
                && obj.GetType() == typeof(FullType)
                && Equals((FullType) obj);
        }

        public static bool operator ==(FullType a, FullType b)
        {
            return a.Equals(b);
        }

        public static bool operator !=(FullType a, FullType b)
        {
            return !a.Equals(b);
        }
    }

    public class Component
    {
        public string comment;
        public Reference reference;
        public Line line;
        public int index;

        public Component(Reference reference, Line line, int index)
        {
            this.reference = reference;
            this.line = line;
            this.index = index;
        }

        public Component(string comment, Line line)
        {
            this.comment = comment;
            this.line = line;
            this.index = -1;
        }
    }

    public class Reference
    {
        public string name;
        public FullType type;
        public bool literal;

        public Reference(string name, FullType type, bool literal=false)
        {
            this.name = name;
            this.type = type;
            this.literal = literal;
        }

        public override string ToString()
        {
            return type + " " + name;
        }

        public Value asliteral
        {
            get
            {
                if (type.collection)
                {
                    return new Value { type = type, value = new Value[0] };
                }
                else if (type == FullType.NUMBER)
                {
                    return new Value { type = type, value = int.Parse(name) };
                }
                else if (type == FullType.BOOLEAN)
                {
                    return new Value { type = type, value = bool.Parse(name) };
                }

                return new Value { type = type, value = "SORRY NOT IMPLEMENTED" };
            }
        }
    }

    public class Line
    {
        public Function function;
        public Reference[] inputs;

        public bool @return;

        public Line(Function function, params Reference[] inputs)
        {
            this.function = function;
            this.inputs = inputs;
        }

        public Line(Function function, bool @return, params Reference[] inputs)
        {
            this.function = function;
            this.inputs = inputs;
            this.@return = @return;
        }

        public override string ToString()
        {
            string call = "";

            if (function != null)
            {
                call = string.Format("{0}({1})", function.name, string.Join(", ", inputs.Skip(1).Select(input => input.name).ToArray()));
            }

            if (@return)
            {
                string condition = call == "" ? "" : string.Format("if ({0}) ", call);

                return string.Format("{1}return {0}", inputs[0].name, condition);
            }

            return string.Format("{0} = {1}", inputs[0].name, call);
        }

        public IEnumerable<Component> components
        {
            get
            {
                string first = (function != null && function.comments[0] != "") 
                             ? " " + function.comments[0] 
                             : "";
 
                if (assignment)
                {
                    bool boolean = function.signature[0].type == FullType.BOOLEAN;

                    yield return new Component("set", this);
                    yield return new Component(inputs[0], this, 0);
                    yield return new Component("to" + (boolean ? " whether" : "") + first, this);
                }
                else if (@return)
                {
                    yield return new Component("return", this);
                    yield return new Component(inputs[0], this, 0);

                    if (function == null) yield break;

                    yield return new Component("if" + first, this);
                }

                for (int i = 1; i < function.signature.Length; ++i)
                {
                    yield return new Component(inputs[i], this, i);
                    if (function.comments[i] != "") yield return new Component(function.comments[i], this);
                }
            }
        }

        public bool assignment
        {
            get
            {
                return !@return && !function.action;
            }
        }
    }

    public class Function
    {
        public string name;
        public Reference[] signature;
        public string[] comments;
        public List<Line> body;
        public bool uncacheable;
        public Func<Value[], Value> builtin;

        public IEnumerable<Component> components
        {
            get
            {
                yield return new Component(comments[0], null);

                for (int i = 1; i < signature.Length; ++i)
                {
                    yield return new Component(signature[i], null, i);
                    yield return new Component(comments[i], null);
                }
            }
        }

        public IEnumerable<Reference> GetLocalsFor(Line call)
        {
            int index = body.IndexOf(call);

            for (int i = 0; i < index; ++i)
            {
                var line = body[i];

                if (line.assignment) yield return line.inputs[0];
            }
        }

        public bool action
        {
            get
            {
                return false;
            }
        }
    }

    public struct Value
    {
        public FullType type;
        public object value;

        public override string ToString()
        {
            return string.Format("{0} {1}", type, value);
        }

        public override int GetHashCode()
        {
            return type.GetHashCode() ^ value.GetHashCode();
        }

        public bool Equals(Value other)
        {
            return other.type.Equals(type)
                && other.value.Equals(value);
        }

        public override bool Equals(object obj)
        {
            return obj != null
                && obj.GetType() == typeof(Value)
                && Equals((Value) obj);
        }

        public Value[] AsSequence()
        {
            Assert.IsTrue(type.collection, "This type is not a sequence!");

            return (Value[]) value;
        }

        public bool boolean
        {
            get
            {
                return (bool) value;
            }
        }

        public int integer
        {
            get
            {
                return (int) value;
            }
        }

        public float number
        {
            get
            {
                return (float) value; 
            }
        }

        public Value[] sequence
        {
            get
            {
                return (Value[]) value;
            }
        }
    }

    public struct Invocation
    {
        public readonly Function function;
        public readonly Value[] arguments;

        public Invocation(Function function, Value[] arguments)
        {
            this.function = function;
            this.arguments = arguments;
        }

        public bool valid
        {
            get
            {
                return arguments.Length == function.signature.Length - 1;
            }
        }

        public bool error
        {
            get
            {
                return arguments.Any(arg => arg.type == FullType.ERROR);
            }
        }

        public override string ToString()
        {
            return string.Format("{0}({1})", 
                                 function.name,
                                 string.Join(", ", arguments.Select(arg => arg.value.ToString()).ToArray()));
        }

        public override int GetHashCode()
        {
            int hash = function.GetHashCode();

            for (int i = 0; i < arguments.Length; ++i)
            {
                hash ^= arguments[i].GetHashCode();
            }

            return hash;
        }

        public bool Equals(Invocation other)
        {
            if (other.function != function
             || other.arguments == null
             || other.arguments.Length != arguments.Length)
            {
                return false;
            }

            for (int i = 0; i < arguments.Length; ++i)
            {
                if (!arguments[i].Equals(other.arguments[i])) return false;
            }

            return true;
        }

        public override bool Equals(object obj)
        {
            return obj != null
                && obj.GetType() == typeof(Invocation)
                && Equals((Invocation) obj);
        }
    }

    public class Context
    {
        private Dictionary<Invocation, Value> cache
            = new Dictionary<Invocation, Value>();

        public Value Evaluate(Invocation invocation, int depth=0)
        {
            if (depth > 10) return new Value { type = FullType.ERROR, value = "TOO DEEP" };

            Value result = default(Value);
            
            if (!invocation.function.uncacheable
             && cache.TryGetValue(invocation, out result))
            {
                return result;
            }

            Assert.IsTrue(invocation.valid, "Invocation {0} is invalid!");
            //Debug.LogFormat("invoking: {0}", invocation);

            if (invocation.function.builtin != null)
            {
                result = invocation.function.builtin(invocation.arguments);
            }
            else
            {
                var locals = new Dictionary<Reference, Value>();
                var function = invocation.function;

                for (int i = 1; i < function.signature.Length; ++i)
                {
                    Reference parameter = function.signature[i];

                    locals[parameter] = invocation.arguments[i - 1];
                }

                for (int i = 0; i < function.body.Count; ++i)
                {
                    Line line = function.body[i];
                    Reference destination = line.inputs[0];

                    var arguments = new Value[line.inputs.Length - 1];

                    for (int r = 1; r < line.inputs.Length; ++r)
                    {
                        Reference input = line.inputs[r];

                        arguments[r - 1] = input.literal ? input.asliteral 
                                                         : locals[input];
                    }

                    if (line.@return && line.function == null)
                    {
                        var ret = line.inputs[0];
                        var val = ret.literal ? ret.asliteral : locals[ret];
                        //Debug.LogFormat("return {0}", val);
                        result = val;
                        break;
                    }

                    var expression = new Invocation(line.function, arguments);
                    var value = Evaluate(expression, depth+1);

                    if (line.@return)
                    {
                        if (value.boolean)
                        {
                            var ret = line.inputs[0];
                            var val = ret.literal ? ret.asliteral : locals[ret];
                            //Debug.LogFormat("return {0}", val);
                            result = val;
                            break;
                        }
                        else
                        {
                            //Debug.LogFormat("skip return");
                            continue;
                        }
                    }
                    else
                    {
                        //Debug.LogFormat("{0} = {1}", destination, value);
                    }

                    locals[destination] = value;
                    result = value;
                }
            }

            if (!invocation.function.uncacheable)
            {
                cache[invocation] = result;
            }

            return result;
        }
    }
}
