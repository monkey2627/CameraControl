using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PTRegister : PTBase
{
    public PTRegister()
    {
        ptName = "PTRegister";
    }
    public string id = "";
    public string pw = "";
    //服务器返回
    public int result = 0;
}

public class PTLogin : PTBase
{
    public PTLogin()
    {
        ptName = "PTLogin";
    }
    public string id = "";
    public string pw = "";
    //服务器返回
    public int result = 0;
}