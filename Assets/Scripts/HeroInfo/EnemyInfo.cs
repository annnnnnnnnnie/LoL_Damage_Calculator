using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


public class EnemyInfo : HeroInfo
{
    public override string heroName
    {
        get
        {
            return "Enemy";
        }

        set
        {

        }
    }
    public override void HandleOnClick()
    {
        runePage.gameObject.SetActive(true);
        runePage.Initialize(this);
    }
}


