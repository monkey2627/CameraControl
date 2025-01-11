using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public struct ConnectCommand : ICommand
{
   public void Execute(object dataObj)
    {
        ConnectCommandSrc src = (ConnectCommandSrc) dataObj;
        this.GetSystem<INetSystem>().Connect(src.ipAddress,src.port);
    }
    
}
public struct ConnectCommandSrc
{
    public string ipAddress;
    public int port;
}