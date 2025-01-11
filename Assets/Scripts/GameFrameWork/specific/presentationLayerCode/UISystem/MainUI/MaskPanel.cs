using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public class MaskPanel : BasePanel
{
    private Image maskImg;
    // Start is called before the first frame update
     public override void OnInit()
    {
        maskImg = GetComponent<Image>();
        base.OnInit();
    }

    public override void OnShow(params object[] objs)
    {
        base.OnShow(objs);
        uiSystem.OpenPanel<TipPanel>("µÇÂ½³É¹¦",false);

    }
}
