using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

//事件系统的支持扩展
public static class  EventExtension
{
   public static void SendEvent<T>(this ICanSendEvent self,object dataobj=null) where T : new()
    {
        StartArchitecture.instance.GetArchitexture().SendEvent<T>(dataobj);
    }
    public static void RegistEvent<T>(this ICanRegistAndUnregistEvent self, Action<object> onEvent) where T : new()
    {
        
        StartArchitecture.instance.GetArchitexture().RegistEvent<T>(onEvent);
    }
    public static void UnRegistEvent<T>(this ICanRegistAndUnregistEvent self, Action<object> onEvent) where T : new()
    {
        StartArchitecture.instance.GetArchitexture().UnRegistEvent<T>(onEvent);
    }
}
