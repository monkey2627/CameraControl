using System.Collections;
using System.Collections.Generic;
using UnityEngine;
//面板基類
public class BasePanel : MonoBehaviour,IBasePanel
{
    protected IUISystem uiSystem;
    public virtual void OnClose()
    {
        gameObject.SetActive(false);
    }

    public virtual void  OnInit()
    {
        uiSystem = this.GetSystem<IUISystem>();
        gameObject.SetActive(false);
    }
    //params	使用 params 关键字可以指定采用数目可变的参数的方法参数。 参数类型必须是一维数组，調用的時候用逗號隔開輸入就歐克
    public virtual void OnShow(params object[] objs)
    {
       gameObject.SetActive(true);
    }

   
}
