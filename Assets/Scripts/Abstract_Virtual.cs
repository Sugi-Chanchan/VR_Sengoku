using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Abstract_Virtual : MonoBehaviour
{
    public static List<int> list=new List<int>(); 
        public Vector3[] vec = new Vector3[5];
    // Start is called before the first frame update
    void Start()
    {
        //OverrideAbstract o = new OverrideAbstract();
        //o.t = transform;

        //AbstractBase a = o.ReturnInstance();

        //a.Method();

        CollisionManager.OverrapCheck(vec[0], vec[1], vec[2], vec[3], vec[4]);

    }

    private void Update()
    {
        
       // print(list.Count);
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
