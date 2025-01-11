using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameStarter : MonoBehaviour,IController
{
    // Start is called before the first frame update
    private StartArchitecture startArchitecture;
    void Awake()
    {
        startArchitecture = StartArchitecture.instance;//获取架构实例
        startArchitecture.SetGameArchitexture(new WowArchitexture());
        startArchitecture.InitAllModulesInArchitexture();
        DontDestroyOnLoad(gameObject);
    }
    private void Update()
    {
        this.GetSystem<INetSystem>().Update();
    }
    /* private void Start()
     {
         INetSystem ins = this.GetSystem<INetSystem>();
         ins.Connect("127.0.0.1", 8888);

     //    ins.Send("TriggerTest");
      //   ins.Receive();
     }
     */


}
