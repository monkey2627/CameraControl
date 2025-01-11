using System.Collections;
using System.Collections.Generic;
using UnityEngine;
//表现层接口
public interface IController : ICanGetSystem,ICanSendEvent,ICanRegistAndUnregistEvent,ICanSendCommand
{
   
}
