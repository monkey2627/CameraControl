using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public struct SendPTCommand : ICommand
{
    public void Execute(object dataObj)
    {
        this.GetSystem<INetSystem>().Send( (PTBase) dataObj);
    }

}