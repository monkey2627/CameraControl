using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IUISystem :ISystem
{
    // public void Init();
    public void OpenPanel<T>(params object[] objs) where T:BasePanel;
    public void ClosePanel(string panelType);
    //關閉其他面板，打開游戲面板
    public void UnLoadLoginPanelsAndLoadGamePanels();
    //旋转玩家角色模型
    public void RotateModel(float angle);
    public PlayerData ChoicePlayerData { get ; set ; }
}
