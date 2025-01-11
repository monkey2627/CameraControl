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
    /*�����Ҫ��д����������ʹ��override�ؼ���������override��ʾ���ǣ�������д��
     * 
     * Ҳ����˵�����б�virtual�ؼ������ε�ʵ��������Ϊ�鷽������Ҫ������������д��
     * ��������д����Ҫʹ��override�ؼ������Σ�����������ķ���ǩ���ͷ���ֵ���ͱ���Ҫ�ڻ������ͬ����ʱ��˵����д��
     ע�⣬�،�֮��Ͳ����{�ø���ˣ�Ҫ�{�õ�ԒҪ��base.OnInit();
     */
    public override void OnInit()
    {
       
        idInput = DeepFindTransform.DeepFindChild(transform,"IF_UserID").GetComponent<InputField>();
        pwInput = DeepFindTransform.DeepFindChild(transform, "IF_PassWord").GetComponent<InputField>();
        loginBtn = DeepFindTransform.DeepFindChild(transform, "Btn_Login").GetComponent<Button>();
        regBtn = DeepFindTransform.DeepFindChild(transform, "Btn_Regist").GetComponent<Button>();
        exitBtn = DeepFindTransform.DeepFindChild(transform, "Btn_Exit").GetComponent<Button>();

        loginBtn.onClick.AddListener(OnLoginClick);//��ӷ�����������ʱ���������
        regBtn.onClick.AddListener(OnRegClick);
        exitBtn.onClick.AddListener(ExitGame);

       
        //���ӷ�����
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
            uiSystem.OpenPanel<TipPanel>("�Ñ������ܴa���ܠ���");
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
            uiSystem.OpenPanel<TipPanel>("��½��gong", false);
            uiSystem.OpenPanel<MaskPanel>();
        }
        else
        {
            uiSystem.OpenPanel<TipPanel>("��½ʧ��", false);
        }
    }



}
