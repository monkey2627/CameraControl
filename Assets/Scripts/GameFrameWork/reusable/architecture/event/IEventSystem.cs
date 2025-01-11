using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public interface IEventSystem
{
    //ע���¼�
    public void Regist<T>(Action<object> onEvent);
    //
    public void UnRegist<T>(Action<object> onEvent);
    public void Send<T>(object obj)where T:new();
}
