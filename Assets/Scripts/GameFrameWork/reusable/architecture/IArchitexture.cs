using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

//架构的抽象接口
public interface IArchitexture
{
    // Start is called before the first frame update
    void RegistSystem<U>(U Instance) where U : ISystem;
    void RegistModel<U>(U Instance) where U : IModel;
    void RegistUtility<U>(U Instance) where U : IUtility;
    U GetSystem<U>() where U : class,ISystem;
    U GetModel<U>() where U : class,IModel;
    U GetUtility<U>() where U : class, IUtility;
    void RegistEvent<U>(Action<object> onEvent) where U : new();

    public void UnRegistEvent<U>(Action<object> onEvent) where U : new();
    public void SendEvent<U>(object dataObj) where U : new();
    public void SendCommand<U>(object dataObj) where U : ICommand, new();

    void InitAllModules();
}
