using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//����ħ������ļܹ�
public class WowArchitexture : Architexture<WowArchitexture>
{
    protected override void Init()
    {   
        this.RegistSystem<INetSystem>(new NetSystem());
        this.RegistSystem<IUISystem>(new UISystem());
     
    }
}
