using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//系统层接口
public interface ISystem :INeedInit,ICanSendEvent,ICanSendCommand,ICanRegistAndUnregistEvent
{
    // Start is called before the first frame update
}
