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
    }
}
