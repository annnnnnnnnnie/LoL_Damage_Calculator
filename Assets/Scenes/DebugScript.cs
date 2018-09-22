using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DebugScript
{
    public class User_2
    {
        public void ChangeData(DataClass_1 datas)
        {
            datas.data += 1;
            datas.data2 += 1;
            datas.data3 += 1;
        }
    }
    public class DataClass_1
    {
        public int data;
        public int data2;
        public int data3;
    }
    public class DebugScript : MonoBehaviour
    {
        private DataClass_1 datas = new DataClass_1() { data = 1, data2 = 2, data3 = 3 };
        private User_2 user_2 = new User_2();
        public void ShowData()
        {
            Debug.Log(datas.data);
            Debug.Log(datas.data2);
            Debug.Log(datas.data3);
        }
        public void Update()
        {
            if (Input.GetMouseButtonDown(0))
            {
                user_2.ChangeData(datas);
                ShowData();
                List<Item> items = new List<Item>();
                Debug.Log(items.Count);
            }
        }
    }

}