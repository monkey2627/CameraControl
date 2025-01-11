using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//���Է�������
public static class CanSendCommandExtension 
{
   
    public static void SendCommand<T>(this ICanSendCommand self,object dataobj = null) where T : ICommand,new()
    {
        StartArchitecture.instance.GetArchitexture().SendCommand<T>(dataobj);
    }
}
