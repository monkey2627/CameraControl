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
        StartCoroutine(startLoading(1));//����Я��
    }
    private IEnumerator startLoading(int count)//Я�̵ķ���ֵ������
    {
        int displayProgress= 0;//��ǰ����ʾ����
        int toProgress = 0;
        AsyncOperation ao = SceneManager.LoadSceneAsync(count);//�첽����
        ao.allowSceneActivation = false;
        while(ao.progress < 0.9f)//���Ի�ȡ����صĽ���
        {
            toProgress = (int)ao.progress * 100;
            while(displayProgress < toProgress)
            {
                displayProgress++;
                SetLoadingPercentValue(displayProgress);
                yield return new WaitForEndOfFrame();//��һ֡����ִ��
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
