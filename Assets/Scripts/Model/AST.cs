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
        public FullType type;
        public string name;
        public string role;

        public Component(string name,
                         string role,
                         Type type,
                         bool collection = false)
        {
            this.name = name;
            this.role = role;
            this.type = new FullType
            {
                type = type,
                collection = collection,
            };
        }
    }

    public class Line
    {
        public Component start;
        public Component command;
        public Component[] arguments;
    }
}
