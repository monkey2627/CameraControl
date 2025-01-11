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
    private StartArchitecture()//����ͬ�������乹�캯��
    {
        Init();
    }
    //���ò�ͬ��Ŀ�ļܹ�
    public void SetGameArchitexture(IArchitexture architexture)
    {
        gameArchitexture = architexture;
    }
    public IArchitexture GetArchitexture()
    {
        return gameArchitexture;
    }
    //��ʼ������е�����ģ��
    public void InitAllModulesInArchitexture()
    {
        gameArchitexture.InitAllModules();
    }
}
