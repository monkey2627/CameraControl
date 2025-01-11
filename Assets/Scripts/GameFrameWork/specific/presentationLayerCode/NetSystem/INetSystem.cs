using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface INetSystem : ISystem
{

    public void Close();
    void Connect(string ip, int port);
    public void Send(PTBase msg);
    public void Update();
    public void RegistPTListenr(string PTname, NetSystem.PTListener listener);


    public void UnregistPTListenr(string PTname, NetSystem.PTListener listener);

    public void SendPTEvent(string PTname, PTBase pt);
   
}
