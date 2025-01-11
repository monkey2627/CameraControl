using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class RegisterPanel : BasePanel
{
    private InputField idInput;
    private InputField pwInput;
    private InputField reInput;
    private Button cancelBtn;
    private Button registBtn;
    // Start is called before the first frame update
    public override void OnInit()
    {
        
        idInput = DeepFindTransform.DeepFindChild(transform, "IF_UserID").GetComponent<InputField>();
        pwInput = DeepFindTransform.DeepFindChild(transform, "IF_Password").GetComponent<InputField>();
        reInput = DeepFindTransform.DeepFindChild(transform, "IF_ReInputPassword").GetComponent<InputField>();
        registBtn = DeepFindTransform.DeepFindChild(transform, "Btn_Regist").GetComponent<Button>();
        cancelBtn = DeepFindTransform.DeepFindChild(transform, "Btn_Cancel").GetComponent<Button>();
        registBtn.onClick.AddListener(OnRegistClick);
        cancelBtn.onClick.AddListener(() => { base.OnClose(); });
        this.SendCommand<RegistPTListenerCommand>(new PTSrc() { ptName = "PTRegister", listener = OnPTRegister });
        base.OnInit();

    }
    public override void OnClose()
    {
        reInput.text = "";
        pwInput.text = "";
        idInput.text = "";
        base.OnClose();
    }
    private void OnRegistClick()
    {
        if(idInput.text == "" || pwInput.text == "")
        {
            uiSystem.OpenPanel<TipPanel>("用户名和密码不能为空");
            return;
        }
        if(reInput.text != pwInput.text)
        {
            uiSystem.OpenPanel<TipPanel>("两次输入密码不同");
        }
        PTRegister pt = new PTRegister();
        pt.id = idInput.text;
        pt.pw = pwInput.text;
        this.SendCommand<SendPTCommand>(pt);
        OnClose();
    }
    private void OnPTRegister(PTBase PT)
    {
        PTRegister ptr = (PTRegister)PT;
        if(ptr.result == 0)
        {
            uiSystem.OpenPanel<TipPanel>("注册成功");
            OnClose();
        }
        else
        {
            uiSystem.OpenPanel<TipPanel>("注册失败");
        }
    }
   
}
