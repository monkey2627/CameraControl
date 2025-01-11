using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface ICommand  : ICanGetModel,ICanGetSystem,ICanGetUtility,ICanSendCommand,ICanSendEvent
{
    public void Execute(object dataObj);
}
