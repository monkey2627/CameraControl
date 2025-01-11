using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//表示该对象需要初始化
public interface INeedInit 
{
    // Start is called before the first frame update
    public void Init();
}
