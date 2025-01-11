using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
public class LoadPanel : BasePanel
{
    private Slider processViewSli;
    public override void OnInit()
    {
        processViewSli = transform.Find("Slider").GetComponent<Slider>();
        base.OnInit();
    }
    public override void OnShow(params object[] objs)
    {
        base.OnShow(objs);
        StartCoroutine(startLoading(1));//开启携程
    }
    private IEnumerator startLoading(int count)//携程的返回值必须是
    {
        int displayProgress= 0;//当前的显示进度
        int toProgress = 0;
        AsyncOperation ao = SceneManager.LoadSceneAsync(count);//异步加载
        ao.allowSceneActivation = false;
        while(ao.progress < 0.9f)//可以获取其加载的进度
        {
            toProgress = (int)ao.progress * 100;
            while(displayProgress < toProgress)
            {
                displayProgress++;
                SetLoadingPercentValue(displayProgress);
                yield return new WaitForEndOfFrame();//下一帧继续执行
            }
        }
        toProgress = 100;
        while(displayProgress < toProgress)
        {
            displayProgress++;
            SetLoadingPercentValue(displayProgress);
            yield return new WaitForEndOfFrame();
        }
        yield return new WaitForSeconds(1);
        ao.allowSceneActivation = true;
        OnClose();
    }
    private void SetLoadingPercentValue(int value)
    {
        processViewSli.value = value / 100;
    }
}
