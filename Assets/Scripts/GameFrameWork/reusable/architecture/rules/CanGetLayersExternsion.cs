using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//��̬��
/*
 ��̬���캯������̬�������һ����̬���캯���������������״α�����ʱ�Զ����ã����ڳ�ʼ����̬��Ա��

���ܱ��̳У���̬�಻�ܱ��̳У��������ܷ�ģ�sealed����

����ʵ�������㲻�ܴ�����̬���ʵ������ֻ��ֱ��ͨ���������������ľ�̬��Ա��

���г�Ա���Ǿ�̬�ģ���̬���е����г�Ա�����������ԡ��ֶΡ��¼��ȣ��������Ǿ�̬�ġ�

���ܰ������Ա����̬�಻�ܰ������Ա��virtual members������Ϊ���ǲ��ܱ���д��

��̬�����ܷ�ģ���̬���Զ���Ϊ�ܷ��࣬����ζ���㲻�ܴӾ�̬���������ࡣ

ʹ�ó�������̬��ͨ�����ڹ����࣬������ѧ�����⡢�������ϡ����������ȡ�
 */
public static class CanGetLayersExternsion 
    //��ĳЩ������Է��ʵ����������չ����
{
  public static T GetSystem<T>(this ICanGetSystem self) where T : class, ISystem
    {                                      
        return StartArchitecture.instance.GetArchitexture().GetSystem<T>();
    }
    public static T GetModel<T>(this ICanGetModel self) where T : class, IModel
    {
        return StartArchitecture.instance.GetArchitexture().GetModel<T>();
    }
    public static T GetUtility<T>(this ICanGetUtility self) where T : class, IUtility
    {
        return StartArchitecture.instance.GetArchitexture().GetUtility<T>();
    }

}
