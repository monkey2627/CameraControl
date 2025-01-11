using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//属于魔兽世界的架构
public class WowArchitexture : Architexture<WowArchitexture>
{
    protected override void Init()
    {   
        this.RegistSystem<INetSystem>(new NetSystem());
        this.RegistSystem<IUISystem>(new UISystem());
     
    }
}
