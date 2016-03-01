using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Assertions;
using System.Linq;
using System.Collections;
using System.Collections.Generic;

namespace AST
{
    public interface IExpression
    {
    }

    public class Query : IExpression
    {

    }

    public class Operation : IExpression
    {

    }

    public class Variable : IExpression
    {

    }

    public class Literal : IExpression
    {

    }

    public class Action
    {
        public IExpression valid;
        //public List<>
    }
}
