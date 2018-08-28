using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


public class EnemyInfo : HeroInfo
{
    public new string heroName = "Enemy";
}


//public class EnemyInfo : MonoBehaviour {
//    private int intEnemyLevel;
//    public Text levelText;
//    public Toggle meleeRangeToggle; //true = ranged

//    public void Start()
//    {
//        int.TryParse(levelText.text.ToString(), out intEnemyLevel);
//    }

//    public void LevelUp()
//    {
//        if(!(intEnemyLevel + 1 > 18))
//        {
//            intEnemyLevel += 1;
//            levelText.text = intEnemyLevel.ToString();
//        }
//    }
//    public void LevelDown()
//    {
//        if (!(intEnemyLevel - 1 < 1))
//        {
//            intEnemyLevel -= 1;
//            levelText.text = intEnemyLevel.ToString();
//        }
//    }

//    public int GetLevel()
//    {
//        return intEnemyLevel;
//    }

//    public bool GetEnemyType()
//    {
//        return meleeRangeToggle.isOn;
//    }

//}

