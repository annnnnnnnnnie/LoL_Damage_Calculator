using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class AnnieInfo : HeroInfo
{
    public override string heroName
    {
        get
        {
            return "Annie";
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




