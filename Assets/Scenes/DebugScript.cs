using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DebugScript
{
    public class DebugScript : MonoBehaviour
    {
        GrandPa grandPa = new GrandPa();
        Papa papa = new Papa();

        // Use this for initialization
        void Start()
        {
            Son son = new Son();
            foreach(string st in son.str)
            {
                Debug.Log("");
            }
            son.str.Add("");
            foreach (string st in son.str)
            {
                Debug.Log((1.5f-0.5f)% 1f);
            }
        }

        // Update is called once per frame
        void Update()
        {
            if (Input.GetMouseButtonDown(1))
            {
                grandPa.StartCounting();
            }
        }
    }
    public class GrandPa
    {
        Papa papa;
        float fTime;
        public void Initialize(Papa papa)
        {
            this.papa = papa;
        }

        public void StartCounting()
        {
            for(int i = 0; i < 100; i++)
            {
                fTime += 0.1f;
            }
            papa.Start();
        }

        public void Update(int number)
        {
            Debug.Log(number);
        }
    }
    public class Papa : GrandPa
    {
        public GrandPa grandPa;
        private float fTime;
        private int i;
        public void Start()
        {
            fTime = 0f;
            i = 0;
            DoTdamage();
            Debug.Log("Started");
        }

        IEnumerator DoTdamage()
        {
            grandPa.Update(i);
            Debug.Log("Started");
            i += 1;
            yield return Count();
        }

        IEnumerator Count()
        {
            for(int j =0; j<10; j++)
            {
                i += 1;
                yield return null;
            }
        }
    }
    public class Son : Papa
    {
        public List<string> str;
        public Son()
        {
            str = new List<string>();
        }


    }

}