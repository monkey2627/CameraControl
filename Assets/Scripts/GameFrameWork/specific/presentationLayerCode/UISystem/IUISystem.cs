using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IUISystem :ISystem
{
    // public void Init();
    public void OpenPanel<T>(params object[] objs) where T:BasePanel;
    public void ClosePanel(string panelType);
    //�P�]������壬���_�Α����
    public void UnLoadLoginPanelsAndLoadGamePanels();
    //��ת��ҽ�ɫģ��
    public void RotateModel(float angle);
    public PlayerData ChoicePlayerData { get ; set ; }
}
