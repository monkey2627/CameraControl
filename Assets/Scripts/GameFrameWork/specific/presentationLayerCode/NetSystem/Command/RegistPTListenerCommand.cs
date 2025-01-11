using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public struct RegistPTListenerCommand : ICommand
{
    public void Execute(object dataObj)
    {
        PTSrc pTSrc = (PTSrc)dataObj;
        this.GetSystem<INetSystem>().RegistPTListenr(pTSrc.ptName,pTSrc.listener);
    }
}

public struct PTSrc
{
    public string ptName;
    public NetSystem.PTListener listener;
}
