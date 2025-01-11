using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StartArchitecture : Singleton<StartArchitecture>,ISingleton
{
    // Start is called before the first frame update
    private IArchitexture gameArchitexture;
    public void Init()
    {

    }
    private StartArchitecture()//与类同名，是其构造函数
    {
        Init();
    }
    //设置不同项目的架构
    public void SetGameArchitexture(IArchitexture architexture)
    {
        gameArchitexture = architexture;
    }
    public IArchitexture GetArchitexture()
    {
        return gameArchitexture;
    }
    //初始化框架中的所有模块
    public void InitAllModulesInArchitexture()
    {
        gameArchitexture.InitAllModules();
    }
}
