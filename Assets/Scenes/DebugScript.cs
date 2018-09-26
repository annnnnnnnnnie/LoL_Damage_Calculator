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

            Display(child);
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