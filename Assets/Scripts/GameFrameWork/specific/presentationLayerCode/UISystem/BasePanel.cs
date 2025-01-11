using System.Collections;
using System.Collections.Generic;
using UnityEngine;
//�����
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
    //params	ʹ�� params �ؼ��ֿ���ָ��������Ŀ�ɱ�Ĳ����ķ��������� �������ͱ�����һά���飬�{�õĕr���ö�̖���_ݔ��͚W��
    public virtual void OnShow(params object[] objs)
    {
       gameObject.SetActive(true);
    }

   
}
