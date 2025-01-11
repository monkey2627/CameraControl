using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Baidu.Aip.Speech;
using System.Net.Http;
using UnityEngine.UI;
using System;

/// <summary>
/// �ٶ�����ʶ�����Ľ�����
/// </summary>
public class Connected2Baidu : MonoBehaviour
{
   // private ListenButton listenBtn; // �̳���д��Button��
    private AudioSource recordSource;
    private AudioClip recordClip;

    #region UI���ؼ�
    //public Image tImage;
    public Text recognizeText;
    public Color tokenGotColor;
    #endregion

    private string accessToken; // ����AIP��Ҫ�õ�Token

    #region �ٶ���������Ӧ��
    private string APP_ID ="";
    private string API_KEY = "IBCbYFAbZBGkfbqpf6UBO1svY6WB6ISQ";
    private string SECRET_KEY = "XcPMlGudUm8o558YCT6Rjz1pGRSqcHW7";
    private string authHost = "https://aip.baidubce.com/oauth/2.0/token";
    private Asr aipClient;  // �ٶ�����ʶ��SDK
    #endregion
    //����¼�����������ݵ���
    private Voice voice;
    

    void Start()
    {
        aipClient = new Asr(APP_ID,API_KEY, SECRET_KEY);   // ����SDK��ʵ��
        aipClient.Timeout = 6000;   // ��ʱʱ��Ϊ6000����
        
        // ���浱ǰӦ�õ�Token
        accessToken = GetAccessToken();
        //�õ�����¼������
        voice = gameObject.GetComponent<Voice>();
    }

    /// <summary>
    /// �������˵����ʼ¼��
    /// </summary>
    public void StartRecord()
    {
        if (Microphone.devices.Length > 0)
        {
            string device = Microphone.devices[0];
            AudioClip clip = Microphone.Start(device, true, 60, 16000);
            recordSource.clip = clip;
            recordClip = clip;
        }
        else
        {
            //SetRecognizeText(TipsReference.CANT_FIND_MICROPHONE);
            //listenBtn.ReleaseClickEvent(TipsReference.RECORD_TYPE.NoMicroPhone);
        }
    }

    /// <summary>
    /// �ɿ�����˵��ֹͣ¼��������ʶ��
    /// </summary>
    public void StopRecord()
    {
        Microphone.End(Microphone.devices[0]);
        StartCoroutine(Recognition(recordClip));
    }

    public void SetRecognizeText(string result)
    {
        recognizeText.text = result;
    }

    IEnumerator Recognition(AudioClip clip2Send)
    {
        float[] sample = new float[recordClip.samples];
        recordClip.GetData(sample, 0);
        short[] intData = new short[sample.Length];
        byte[] byteData = new byte[intData.Length * 2];

        for (int i = 0; i < sample.Length; i++)
        {
            intData[i] = (short)(sample[i] * short.MaxValue);
        }

        Buffer.BlockCopy(intData, 0, byteData, 0, byteData.Length);
        //�����Ǳ����Ǳ�ʶ��
        var result = aipClient.Recognize(byteData, "pcm", 16000);
        var speaking = result.GetValue("result");

        if (speaking == null)
        {
         //   SetRecognizeText(TipsReference.NOTHING_RECORD);
            StopAllCoroutines();
            yield return null;
        }

        string usefulText = speaking.First.ToString();
        SetRecognizeText(usefulText);

        yield return 0;
    }

    private string GetAccessToken()
    {
        HttpClient client = new HttpClient();
        List<KeyValuePair<string, string>> paraList = new List<KeyValuePair<string, string>>();
        paraList.Add(new KeyValuePair<string, string>("grant_type", "client_credentials"));
        paraList.Add(new KeyValuePair<string, string>("client_id", API_KEY));
        paraList.Add(new KeyValuePair<string, string>("client_secret", SECRET_KEY));

        HttpResponseMessage response = client.PostAsync(authHost, new FormUrlEncodedContent(paraList)).Result;
        string result = response.Content.ReadAsStringAsync().Result;
            
        return result;
    }
    public void DisplayClip()
    {
        recordSource.Play();
    }
}

