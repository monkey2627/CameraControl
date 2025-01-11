using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Reflection;
using System;
/*  
 单例模板 
 用来创建并管理、实例化 单例
 */

public class Singleton<T> where T : class,ISingleton
{
    /*
     public class Singleton<T> 是定义了一个泛型类
    在这个类中能用T 来创建变量等
     where T : class,ISingleton 指T必须是class，并且T要继承接口ISingleton
     */
    private static T mInstance;
    public static T instance
    {
        get//get是属性的访问器，用于返回属性的值 ，此处若没有则创建，若有则返回mInstance
        {
            if(mInstance == null)
            {
               var ctors =  typeof(T).GetConstructors(BindingFlags.Instance|BindingFlags.NonPublic);
               var ctor = Array.Find(ctors, c => c.GetParameters().Length == 0);
                if (ctor == null)
                {
                    throw new Exception("没有找到非公共构造函数"+typeof(T));
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
