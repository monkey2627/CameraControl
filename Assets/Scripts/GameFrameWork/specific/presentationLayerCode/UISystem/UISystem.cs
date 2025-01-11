using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UISystem :IUISystem
{
    private Dictionary<string,BasePanel> panelDict = new Dictionary<string, BasePanel>();
    private Transform canvasTrans;
    private PlayerData choicePlayerData;
    public PlayerData ChoicePlayerData { get => choicePlayerData; set => choicePlayerData = value; }
    public void Init()
    {
        Debug.Log("UIϵͳ��ʵ����");
        canvasTrans = GameObject.Find("Canvas").transform;
        AddPanelToDict<LoginPanel>();
        AddPanelToDict<RegisterPanel>();
        AddPanelToDict<MaskPanel>();
        AddPanelToDict<ChoicePanel>();
        AddPanelToDict<TipPanel>();
        AddPanelToDict<LoadPanel>();
        AddPanelToDict<CharacterCreatePanel>();
        OpenPanel<LoginPanel>();

    }
    private void AddPanelToDict<T>()where T : BasePanel
    {
       string panelType =  typeof(T).ToString();
        if (panelDict.ContainsKey(panelType))
        {
            Debug.Log("��ǰ�ֵ����� " + panelType);
            return;
        }
        else
        {
            //��������塢��ӌ������ƽű��K������
          GameObject panelGo  = Object.Instantiate(GameResSystem.GetRes<GameObject>("Prefabs/UI/" + panelType),canvasTrans);
          panelDict.Add(panelType,panelGo.AddComponent<T>()) ;
     //       Debug.Log(panelDict[panelType].ToString());
          panelDict[panelType].OnInit();

        }


    }
    private void RemovePanelInDict(string panelType)
    {
        
        if (!panelDict.ContainsKey(panelType))
        {
            Debug.Log("��ǰ�ֵ䲻����" + panelType);
                return;
        }
            GameObject panelGo = panelDict[panelType].gameObject;
            panelDict.Remove(panelType);
            GameObject.Destroy(panelGo);
        }

    public void OpenPanel<T>(params object[] objs) where T : BasePanel
    {//Debug.Log(panelDict[panelType].ToString());

        string panelType = typeof(T).ToString();
        if (!panelDict.ContainsKey(panelType))
        {
            Debug.Log("��ǰ�ֵ䲻���� " + panelType);
            return;
        }
        else
        {

            panelDict[panelType].OnShow(objs);

        }

    }
    public void ClosePanel(string panelType)
    {
        if (!panelDict.ContainsKey(panelType))
        {
            Debug.Log("��ǰ�ֵ䲻���� " + panelType);
            return;
        }else{
            panelDict[panelType].OnClose();

        }

    }
    public void UnLoadLoginPanelsAndLoadGamePanels()
    {
        RemovePanelInDict("LoginPanel");
        RemovePanelInDict("RegisterPanel");
        RemovePanelInDict("MaskPanel");
        RemovePanelInDict("ChoicePanel");
        RemovePanelInDict("TipPanel");
        RemovePanelInDict("LoadPanel");
        RemovePanelInDict("CharacterCreatePanel");
        AddPanelToDict<GamePanel>();
        OpenPanel<GamePanel>();
    }
    public void RotateModel(float angle)
    {

    }

}

