using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Abstract_Virtual : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        OverrideAbstract o = new OverrideAbstract();
        o.t = transform;

        AbstractBase a = o.ReturnInstance();
        
        a.Method();
        
    }

}

public abstract class AbstractBase :MonoBehaviour
{
    public abstract void Method();
    public AbstractBase ReturnInstance()
    {
        return this;
    }
}

public class OverrideAbstract : AbstractBase
{
    string s = "Override!";
    public Transform t;

    public override void Method()
    {
        Debug.Log(t.position);
    }
}
