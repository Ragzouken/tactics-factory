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
    }

    public class Component
    {
        public string comment;

        public FullType type;
        public string name;

        public Component(string name,
                         Type type,
                         bool collection = false)
        {
            this.name = name;
            this.type = new FullType
            {
                type = type,
                collection = collection,
            };
        }

        public Component(string comment)
        {
            this.comment = comment;
        }
    }

    public class Line
    {
        public Component[] arguments;

        public bool assignment
        {
            get
            {
                return arguments[0].comment == "set";
            }
        }

        public Component destination
        {
            get
            {
                return arguments[1];
            }
        }
    }

    public class Function
    {
        public string name;
        public Line signature;
        public List<Line> definition;

        public IEnumerable<Component> GetLocalsFor(Line call)
        {
            int index = definition.IndexOf(call);

            for (int i = 0; i < index; ++i)
            {
                var line = definition[i];

                if (line.assignment) yield return line.destination;
            }
        }
    }
}
