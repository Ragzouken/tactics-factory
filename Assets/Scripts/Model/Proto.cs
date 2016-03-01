using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Assertions;
using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;

public class Proto
{
    public Game game;
    public List<DAction> actions;
}

public class DAction
{
    public string name;
    public Func<DObject, bool> valid;
    public Func<DObject, bool> able;
    //public Func<DObject, IEnumerable<object>> selections;
    public Action<DObject> perform;
}
