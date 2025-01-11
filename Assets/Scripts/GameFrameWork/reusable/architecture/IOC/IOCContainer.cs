using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
// IOC Container,save all layers and shili of each layer
public class IOCContainer 
{
    private Dictionary<Type,object> instancesDict = new Dictionary<Type, object>();
    //register,用于注入
    public void Register<T>(T instance)
    {
        var key = typeof(T);
        if(instancesDict.ContainsKey(key))
        {
            instancesDict[key] = instance;
        }else
        {
            instancesDict.Add(key,instance);
        }
    }
    //get instance
    public T Get<T>() where T:class{
        var key = typeof(T);
        object obj = null;
        if(instancesDict.TryGetValue(key,out obj)){
            return obj as T;
        }else{
            Debug.Log("想要获取的为空");
        }
        return null;
    }
    //init all instance in IOC container
    public void InitAllModules(){
        foreach(var item in instancesDict){

            ((INeedInit)item.Value).Init();

        }
    }

}
