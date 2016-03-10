using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Assertions;
using System.Linq;
using System.Collections;
using System.Collections.Generic;

namespace AST
{
    public enum Type
    {
        Boolean,
        Number,
        Object,
        Position,
        Action,
    }

    public struct FullType
    {
        public Type type;
        public bool collection;

        public override string ToString()
        {
            return type.ToString() + (collection ? "s" : "");
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

        public Reference(string name, FullType type)
        {
            this.name = name;
            this.type = type;
        }

        public override string ToString()
        {
            return type + " " + name;
        }
    }

    public class Line
    {
        public Function function;
        public Reference[] inputs;

        public Line(Function function, params Reference[] inputs)
        {
            this.function = function;
            this.inputs = inputs;
        }

        public override string ToString()
        {
            return string.Format("{0} = {1}({2})", inputs[0].name, function.name, string.Join(", ", inputs.Skip(1).Select(input => input.name).ToArray()));
        }

        public IEnumerable<Component> components
        {
            get
            {
                yield return new Component("set", this);
                yield return new Component(inputs[0], this, 0);
                yield return new Component("to", this);

                yield return new Component(function.comments[0], this);

                for (int i = 1; i < function.signature.Length; ++i)
                {
                    yield return new Component(inputs[i], this, i);
                    yield return new Component(function.comments[i], this);
                }
            }
        }

        public bool assignment
        {
            get
            {
                return !function.action;
            }
        }
    }

    public class Function
    {
        public string name;
        public Reference[] signature;
        public string[] comments;
        public List<Line> body;

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
}
