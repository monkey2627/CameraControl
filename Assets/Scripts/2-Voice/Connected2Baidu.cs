using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Baidu.Aip.Speech;
using System.Net.Http;
using UnityEngine.UI;
using System;
using TMPro;

/// <summary>
/// �ٶ�����ʶ�����Ľ�����
/// </summary>
public class Connected2Baidu : MonoBehaviour
{
    public static Connected2Baidu instance; 

   // private string accessToken; // ����AIP��Ҫ�õ�Token

    #region �ٶ���������Ӧ��
    private string APP_ID ="";
    private string API_KEY = "D0DNagS6l1nE7vh7gqDLrlLk";
    private string SECRET_KEY = "4UAtaDadyqaJ4bgG6o6sWB4sjt22Bnrw";
    private string authHost = "https://aip.baidubce.com/oauth/2.0/token";
    private Asr aipClient;  // �ٶ�����ʶ��SDK
    #endregion

    private void Awake()
    {
        instance = this;
    }

    void Start()
    {
        aipClient = new Asr(APP_ID,API_KEY, SECRET_KEY);   // ����SDK��ʵ��
        aipClient.Timeout = 6000;   // ��ʱʱ��Ϊ6000����
        // ���浱ǰӦ�õ�Token
       // accessToken = GetAccessToken();

    }

    public void Recognition(byte[] byteData)
    {

        //�����Ǳ����Ǳ�ʶ��
        var result = aipClient.Recognize(byteData, "pcm", 16000);
        var speaking = result.GetValue("result");
        string usefulText = speaking.First.ToString();
        Debug.Log(usefulText);
        
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
        Debug.Log(result);
        return result;
    }
}

