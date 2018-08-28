using UnityEngine;
using UnityEngine.UI;

public class LevelComponent : MonoBehaviour {

    [HideInInspector]
    public string strLevelName; //which level is this level? Q level? W level? etc.
    public Text labelText;
    public Text levelText;
    public Button levelUpBtn;
    public Button levelDownBtn;

    private int intLevel;
    private int leastLevel;
    private int mostLevel;

    public void Initialize(string lvlName, int initialLevel, int leastLevel, int mostLevel)
    {
        strLevelName = lvlName;
        labelText.text = strLevelName;

        levelUpBtn.onClick.RemoveAllListeners();
        levelUpBtn.onClick.AddListener(() => { ChangeLevel(1); });
        levelDownBtn.onClick.RemoveAllListeners();
        levelDownBtn.onClick.AddListener(() => { ChangeLevel(-1); });
        intLevel = initialLevel;
        levelText.text = intLevel.ToString();
        this.leastLevel = leastLevel;
        this.mostLevel = mostLevel;
        int.TryParse(levelText.text.ToString(), out intLevel);
    }
    
    private void ChangeLevel(int intChange)
    {
        if (intLevel + intChange < leastLevel || intLevel + intChange > mostLevel)
        {
            //Debug.Log(intLevel.ToString() + leastLevel.ToString() + mostLevel.ToString());
            return;
        }
        else
        {
            intLevel += intChange;
            levelText.text = intLevel.ToString();
        }
    }

    public int GetLevel()
    {
        return intLevel;
    }


}
