using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DebugScript
{
    public class DebugScript : MonoBehaviour
    {
        public void Start()
        {
            Parent parent = new Parent();
            Child child = new Child();
            List<Parent> peoples = new List<Parent>();
            peoples.Add(new Parent { Hour = 1 });
            peoples.Add(new Parent { Hour = 2 });
            peoples.Add(new Child { Hour = 1, Min = 5 });
            peoples.Add(new Child { Hour = 2, Min = 10 });
            List<Parent> Childs = peoples.FindAll(x => x is Child);
            foreach(Child c in Childs)
            {
                c.Display();
            }
            foreach (Child p in peoples)//Cannot cast
            {
                p.Display();
            }
        }

        public void Display(Parent parent)
        {
            parent.Display();
        }
    }

    public class Parent
    {
        public int Hour = 10;
        public void Initialize()
        {

        }

        public virtual void Display()
        {
            Debug.Log("Time is " + Hour);
        }
    }

    public class Child : Parent
    {
        public int Min = 9;

        public override void Display()
        {
            Debug.Log("Time is " + Hour + " " + Min);
        }
    }

}