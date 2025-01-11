using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Reflection;
using System;
/*  
 ����ģ�� 
 ��������������ʵ���� ����
 */

public class Singleton<T> where T : class,ISingleton
{
    /*
     public class Singleton<T> �Ƕ�����һ��������
    �������������T ������������
     where T : class,ISingleton ָT������class������TҪ�̳нӿ�ISingleton
     */
    private static T mInstance;
    public static T instance
    {
        get//get�����Եķ����������ڷ������Ե�ֵ ���˴���û���򴴽��������򷵻�mInstance
        {
            if(mInstance == null)
            {
               var ctors =  typeof(T).GetConstructors(BindingFlags.Instance|BindingFlags.NonPublic);
               var ctor = Array.Find(ctors, c => c.GetParameters().Length == 0);
                if (ctor == null)
                {
                    throw new Exception("û���ҵ��ǹ������캯��"+typeof(T));
                }
                mInstance  = ctor.Invoke(null) as T;
            }
            return mInstance;
        }


    }
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
