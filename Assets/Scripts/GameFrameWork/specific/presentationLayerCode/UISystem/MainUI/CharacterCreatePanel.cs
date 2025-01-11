using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CharacterCreatePanel : BasePanel

{
    private Button rotateLeftBtn;
    private Button agreeBtn;
    private Button rotateRightBtn;

    private Text nameIF;
    private Button exitBtn;
    // Start is called before the first frame update
    public override void OnInit()
    {
        base.OnInit();
        rotateLeftBtn = DeepFindTransform.DeepFindChild(transform, "Btn_RotateLeft").GetComponent<Button>();
        rotateRightBtn = DeepFindTransform.DeepFindChild(transform, "Btn_RotateRight").GetComponent<Button>();
        agreeBtn = DeepFindTransform.DeepFindChild(transform, "Btn_Agree").GetComponent<Button>();
        nameIF = DeepFindTransform.DeepFindChild(transform, "IF_Name").GetComponent<Text>();
        exitBtn = DeepFindTransform.DeepFindChild(transform, "Btn_Exit").GetComponent<Button>();
        rotateLeftBtn.onClick.AddListener(OnLeftRotateCharacterClick);
        rotateRightBtn.onClick.AddListener(OnRightRotateCharacterClick);
        agreeBtn.onClick.AddListener(OnAgreeCreateClick);
        exitBtn.onClick.AddListener(OnReturnToChoicePanel);
    }
    private void OnLeftRotateCharacterClick()
    {
        uiSystem.RotateModel(-45);
    }
    private void OnRightRotateCharacterClick()
    {
        uiSystem.RotateModel(45);
    }
    private void OnReturnToChoicePanel()
    {
        OnClose();
        uiSystem.OpenPanel<ChoicePanel>();
    }
    private void OnAgreeCreateClick()
    {
        if(nameIF.text == null || nameIF.text.Contains(""))
        {
            uiSystem.OpenPanel<TipPanel>("名字不能为空且不能包含空格");
        }
        OnClose();
        uiSystem.OpenPanel<TipPanel>("创建成功");
        uiSystem.OpenPanel<ChoicePanel>();
    }
}
