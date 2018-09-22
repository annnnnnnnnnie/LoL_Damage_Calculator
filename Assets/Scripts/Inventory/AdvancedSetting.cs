using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class AdvancedSetting : MonoBehaviour
{
    public Button closeBtn;
    public Button okBtn;

    public Image itemIcon;
    public GameObject advancedSettingEditorPrefab;
    public GameObject holderGameObject;
    public List<AdvancedSettingEditor> editors;

    private Item item;
    private Item orignialItem;

    public void Start()
    {
        gameObject.SetActive(false);
    }

    public void Initialize(Item item)
    {
        this.item = item;
        orignialItem = item.MakeCopy();

        closeBtn.onClick.RemoveAllListeners();
        closeBtn.onClick.AddListener(() => { Close(); });

        okBtn.onClick.RemoveAllListeners();
        okBtn.onClick.AddListener(() => { OK(); });

        itemIcon.sprite = Resources.Load<Sprite>("ItemIcons/" + item.strName);
        
        foreach(KeyValuePair<string, int> kvP in item.Extras)
        {
            GameObject go = GameObjectUtility.CustomInstantiate(advancedSettingEditorPrefab, holderGameObject.transform);
            AdvancedSettingEditor editor = go.GetComponent<AdvancedSettingEditor>();
            editors.Add(editor);
            editor.Initialize(kvP.Key, item);
        }

    }

    
    private void OK()
    {
        Debug.Log("Modification confirmed");
        Close();
    }
    private void Close()
    {
        foreach(AdvancedSettingEditor editor in editors)
        {
            GameObjectUtility.CustomDestroy(editor.gameObject);
        }
        editors.Clear();
        Debug.Log("Closing Advanced Setting Tab");
        gameObject.SetActive(false);
    }
}
