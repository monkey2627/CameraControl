using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LoginPanel : BasePanel
{
    private InputField idInput;
    private InputField pwInput;
    private Button loginBtn;
    private Button regBtn;
    private Button exitBtn;
    private GameObject snowBG;
    private GameObject Frostwurmnorthrend;
    /*如果需要重写方法，必须使用override关键字声明，override表示覆盖，就是重写，
     * 
     * 也就是说基类中被virtual关键字修饰的实例方法称为虚方法，需要在派生类中重写，
     * 派生类重写就需要使用override关键字修饰，并且派生类的方法签名和返回值类型必须要于基类的相同，此时才说明重写。
     注意，重寫之後就不會調用父類了，要調用的話要用base.OnInit();
     */
    public override void OnInit()
    {
       
        idInput = DeepFindTransform.DeepFindChild(transform,"IF_UserID").GetComponent<InputField>();
        pwInput = DeepFindTransform.DeepFindChild(transform, "IF_PassWord").GetComponent<InputField>();
        loginBtn = DeepFindTransform.DeepFindChild(transform, "Btn_Login").GetComponent<Button>();
        regBtn = DeepFindTransform.DeepFindChild(transform, "Btn_Regist").GetComponent<Button>();
        exitBtn = DeepFindTransform.DeepFindChild(transform, "Btn_Exit").GetComponent<Button>();

        loginBtn.onClick.AddListener(OnLoginClick);//添加方法，当触发时会运行这个
        regBtn.onClick.AddListener(OnRegClick);
        exitBtn.onClick.AddListener(ExitGame);

       
        //连接服务器
        this.SendCommand<ConnectCommand>(new ConnectCommandSrc() { ipAddress ="127.0.0.1",port = 8888 });
        this.SendCommand<RegistPTListenerCommand>(new PTSrc() { ptName = "PTLogin", listener = OnPTLogin });
        base.OnInit();
    }
    public override void OnShow(params object[] objs)
    {
        base.OnShow(objs);
        LoadOrDestroyLoginPanelGameObjects(true);
    }
    public override void OnClose()
    {
        base.OnClose();
        LoadOrDestroyLoginPanelGameObjects(false);
        idInput.text = "";
        pwInput.text = "";
    }
    public void OnLoginClick()
    {
        if (idInput.text == "" || pwInput.text == "")
        {
            uiSystem.OpenPanel<TipPanel>("用戶名和密碼不能爲空");
            return;
        }
        PTLogin pt = new PTLogin();
        pt.id = idInput.text;
        pt.pw = pwInput.text;
        this.SendCommand<SendPTCommand>(pt);
    }
    public void OnRegClick(){
        uiSystem.OpenPanel<RegisterPanel>();
    }

    private void ExitGame()
    {
        Application.Quit();
    }
    public void LoadOrDestroyLoginPanelGameObjects(bool ifLoad=false)
    {
        if (ifLoad)
        {
            snowBG = GameObject.Instantiate(GameResSystem.GetRes<GameObject>("Prefabs/Scene/SnowBG"));
            Frostwurmnorthrend = GameObject.Instantiate(GameResSystem.GetRes<GameObject>("Prefabs/Scene/Frostwurmnorthrend"));
        }
        else
        {
            GameObject.Destroy(snowBG);
            GameObject.Destroy(Frostwurmnorthrend);
        }
    }
    private void OnPTLogin(PTBase pt)
    {
        PTLogin ptl = (PTLogin)pt;
        if(ptl.result == 0)
        {
            uiSystem.OpenPanel<TipPanel>("登陆成gong", false);
            uiSystem.OpenPanel<MaskPanel>();
        }
        else
        {
            uiSystem.OpenPanel<TipPanel>("登陆失败", false);
        }
    }



}
