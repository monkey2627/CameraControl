using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ChoicePanel : BasePanel
{
    private Button rotateLeftBtn;
    private Button enterGameBtn;
    private Button rotateRightBtn;

    private Button createCharacterBtn;
    private Button deleteCharacterBtn;
    private Button exitBtn;
    private ToggleGroup toggleGroup; 
    private Text playerIDText;
    private GameObject leafBG;
    private GameObject character;
    // Start is called before the first frame update
    public override void OnInit()
    {
        enterGameBtn = DeepFindTransform.DeepFindChild(transform,"Btn_StartGame").GetComponent<Button>();
        rotateLeftBtn = DeepFindTransform.DeepFindChild(transform, "Btn_RotateLeft").GetComponent<Button>();
        rotateRightBtn = DeepFindTransform.DeepFindChild(transform, "Btn_RotateRight").GetComponent<Button>();
        createCharacterBtn = DeepFindTransform.DeepFindChild(transform, "Btn_CreateCharacter").GetComponent<Button>();
        deleteCharacterBtn = DeepFindTransform.DeepFindChild(transform, "Btn_DeleteCharacter").GetComponent<Button>(); 
        exitBtn = DeepFindTransform.DeepFindChild(transform,"Btn_Exit").GetComponent<Button>();
        toggleGroup = DeepFindTransform.DeepFindChild(transform, "CharacterInfoContent").GetComponent<ToggleGroup>(); 
        playerIDText = DeepFindTransform.DeepFindChild(transform, "Text_PlayerID").GetComponent<Text>();
        rotateLeftBtn.onClick.AddListener(OnLeftRotateCharacterClick);
        rotateRightBtn.onClick.AddListener(OnRightRotateCharacterClick);
        enterGameBtn.onClick.AddListener(OnEnterGameClick);
        createCharacterBtn.onClick.AddListener(OnOpenCreateCharacterPanel);
        deleteCharacterBtn.onClick.AddListener(OnDeleteCharacterClick);
        exitBtn.onClick.AddListener(OnReTurnToLoginPanelClick);
                       base.OnInit();
    }
    public override void OnShow(params object[] objs)
    {
        base.OnShow(objs);
        LoadOrDestroyChoicePanelGameObjects(true);
    }
    private void OnEnterGameClick()
    {
        base.OnClose();
        uiSystem.OpenPanel<LoadPanel>();
    }
    private void OnLeftRotateCharacterClick()
    {
        uiSystem.RotateModel(-45);
    }
    private void OnRightRotateCharacterClick()
    {
        uiSystem.RotateModel(45);
    }
    private void OnOpenCreateCharacterPanel()
    {
        base.OnClose();
        uiSystem.OpenPanel<CharacterCreatePanel>();
    }
    private void OnDeleteCharacterClick()
    {
        uiSystem.OpenPanel<TipPanel>("你是否要删除该角色？");
    }
    public override void OnClose()
    {
        LoadOrDestroyChoicePanelGameObjects(false);
        base.OnClose();
    }
    private void OnReTurnToLoginPanelClick()
    {
        base.OnClose();
        uiSystem.OpenPanel<LoginPanel>();
    }
    public void LoadOrDestroyChoicePanelGameObjects(bool ifLoad = false)
    {
        if (ifLoad)
        {
            leafBG = GameObject.Instantiate(GameResSystem.GetRes<GameObject>("Prefabs/Effect/LeavesPS"));
            character = GameObject.Instantiate(GameResSystem.GetRes<GameObject>("Prefabs/Character/ChoiceModel/BloodDelf"));
        }
        else
        {
            GameObject.Destroy(leafBG);
            GameObject.Destroy(character);
        }
    }
}
