using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TipPanel : BasePanel
{
    private Text tipText;
    private Text tipText2;

    private Button OKBtn;

    public override void OnInit()
    {
        tipText = DeepFindTransform.DeepFindChild(transform, "Text_Tip").GetComponent<Text>();
        tipText2 = DeepFindTransform.DeepFindChild(transform, "Text_Tip2").GetComponent<Text>();
        OKBtn = DeepFindTransform.DeepFindChild(transform, "Btn_OK").GetComponent<Button>();
        OKBtn.onClick.AddListener(OnOKBtnClick);
        base.OnInit();
    }
    /*
    param name = "objs" 1.提示内容（string） 2.是否显示button，默认为true   

    */
    public override void OnShow(params object[] objs)
    {
        base.OnShow(objs);
        if(objs.Length == 1)
        {
            tipText.gameObject.SetActive(true);
            tipText2.gameObject.SetActive(false);
            tipText.text = (string)objs[0];
        }else if(objs.Length>1)
        {
            tipText.gameObject.SetActive(false);
            tipText2.gameObject.SetActive(true);
            tipText2.text = (string)objs[0];
            OKBtn.gameObject.SetActive((bool)objs[1]);
        }
    }
    public override void OnClose()
    {
        base.OnClose();
        OKBtn.gameObject.SetActive(true);
    }
    private void OnOKBtnClick()
    {
        base.OnClose();
    }
}
