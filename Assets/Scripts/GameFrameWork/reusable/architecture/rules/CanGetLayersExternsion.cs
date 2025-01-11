using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//静态类
/*
 静态构造函数：静态类可以有一个静态构造函数，它会在类型首次被引用时自动调用，用于初始化静态成员。

不能被继承：静态类不能被继承，它们是密封的（sealed）。

不能实例化：你不能创建静态类的实例。你只能直接通过类名来访问它的静态成员。

所有成员都是静态的：静态类中的所有成员（方法、属性、字段、事件等）都必须是静态的。

不能包含虚成员：静态类不能包含虚成员（virtual members），因为它们不能被重写。

静态类是密封的：静态类自动成为密封类，这意味着你不能从静态类派生新类。

使用场景：静态类通常用于工具类，比如数学函数库、常量集合、帮助方法等。
 */
public static class CanGetLayersExternsion 
    //让某些对象可以访问到其他层的拓展方法
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
