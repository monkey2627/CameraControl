using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
//����¼�����࣬�������仯����ʼ�ͽ���¼��
public class Voice : MonoBehaviour
{
    //������Ƶѹ�� https://zhuanlan.zhihu.com/p/139347299
    public static Voice instance;
    // ��ǰʹ�õ���˷�����
    private string microphoneName;
    // ��˷��б���ַ���
    private string[] micDevicesNames;
    public float minVolumn = 0.6f;
    // �Ƿ�ʼ¼��
    private bool isRecord = false;
    //
    private AudioClip micRecord;
    private void Awake()
    {
        instance = this;        
        // ��ȡ��˷��б�
        micDevicesNames = Microphone.devices;
        if (micDevicesNames.Length <= 0){
            Debug.Log("ȱ����˷��豸");
        }
        else{ 
            // ����б��е�һ����˷磩
            microphoneName = micDevicesNames[0];  
            Debug.Log("��ǰ�û�¼������˷�����Ϊ��" + micDevicesNames[0]);
        
        }
    }
    public void StartRecord()
    {
        if (!isRecord)
        {
            micRecord = Microphone.Start(microphoneName, true, 60 * 10, 16000);//44100��Ƶ������   �̶���
            isRecord = true;
            StartCoroutine("RecordAudioIENU");
        }
        else
        {
            Debug.Log("�Ѿ�����¼��������");
        }
    }
    private float minLevel = 1;
    private bool saveRecord = false;
    private int beginPos = 0;
    private bool isRecording = false;
    private bool saved = false;
    private List<byte[]> audioList = new List<byte[]>();
    private float quiet = 0;
    private float quietSeconds(float v)
    {
        if(v > minLevel)
        {
            quiet = 0;
        }
        quiet += Time.deltaTime;
        return quiet;
    }
    void Update()
    {
        //ʵ����˷�һֱ���ţ�ֻ������û��
        //��⵽����������ֵ��������û���ڡ�¼����������¼��
        if (isRecord)
        {
            float v = GetVolume();
            if (v > minVolumn && !isRecording) {
            
                beginPos = Microphone.GetPosition(microphoneName);
                isRecording = true;
                saved = false;
                Debug.Log("��ʼ��¼"+beginPos);
            };
            quietSeconds(v);
    
            if (v < minVolumn && quiet > 2f && isRecording && !saveRecord && !saved){
                Debug.Log("������¼" + Microphone.GetPosition(microphoneName));
                saveRecord = true;
            } /**/
        }
    }
    //��Э��������¼��

  
    private IEnumerator RecordAudioIENU()
    {
        yield return new WaitForSeconds(1f);
        while (true)//һֱѭ��������unityֻ��������
        {
            if (saveRecord)
            {
                //getPosition  ��ȡ��¼�����������ڵ�λ�á�
                int now = Microphone.GetPosition(microphoneName);
                int offset = now - beginPos;
                // ¼���ĳ��ȳ�������ͨ��
                float[] soundata = new float[offset * 1];
                // ʹ�ü����е�����������飬�����ǿ�ʼ��ȡ��λ��
                micRecord.GetData(soundata, beginPos);
               
                // �������вɼ�������Ƶ��Ϣ
                byte[] outData = new byte[soundata.Length * 2];
                // short���ʹ��������ֵ 
                int rescaleFactor = 32767;
                for (int i = 0; i < soundata.Length; i++)
                {
                    // short�������ڴ����ŵĶ��������ͣ�short����ռ2�ֽ�(16λ)�ڴ�ռ䣬�洢-32768 �� 32767
                    short temshort = (short)(soundata[i] * rescaleFactor);
                    //��ͬ���������ͺ��ֽ�����֮�����ת��
                    byte[] temdata = BitConverter.GetBytes(temshort);
                    outData[i * 2] = temdata[0];
                    outData[i * 2 + 1] = temdata[1];
                }
                saveRecord = false;
                isRecording = false;
                saved = true;
                audioList.Add(outData);
                //����ÿһ�ε�¼���ڱ���
                Connected2Baidu.instance.Recognition(outData);
                SaveRecord(outData,soundata);
                //ֱ��saveRecord Ϊ��ſ�����һ��Э��
           
            }
            yield return new WaitForEndOfFrame();
        }
    }
    private int audioNum = 0;
    public void SaveRecord(byte[] data,float[] soundData)
    {
        string fileName = "D:/CameraControl/Assets/AudioClip/" + audioNum + ".wav";
        Debug.Log("save " +data.Length+" at "+fileName);
        audioNum ++;
       using (FileStream fs = CreateEmpty(fileName))
        {
            fs.Write(data, 0, data.Length);
            WriteHeader(fs, micRecord); //wav�ļ�ͷ
        } 
    }
    /// <summary>��ȡ��˷�����</summary>
    /// <returns>��˷��������ֵ</returns>
    public virtual float GetVolume()
    {
        float levelMax = 0;
        if (Microphone.IsRecording(microphoneName))
        {
            float[] samples = new float[16000];
            int startPosition = Microphone.GetPosition(microphoneName) - (16000 + 1);
            if (startPosition >= 0)
            {//����˷绹δ��ʽ����ʱ����ֵ��Ϊ��ֵ��AudioClip.GetData�����ᱨ��
                micRecord.GetData(samples, startPosition);
                for (int i = 0; i < 16000; i++)
                {
                    float wavePeak = samples[i];
                    if (levelMax < wavePeak)
                    {
                        levelMax = wavePeak;
                    }
                }
                levelMax = levelMax * 99;
               // Debug.Log("��˷�������" + levelMax);
            }
        }
        return levelMax;
    }
    /// <summary>
    /// д�ļ�ͷ
    /// </summary>
    /// <param name="stream"></param>
    /// <param name="clip"></param>
    # region ���߷���
    public static void WriteHeader(FileStream stream, AudioClip clip)
    {
        int hz = clip.frequency;
        int channels = clip.channels;
        int samples = clip.samples;

        stream.Seek(0, SeekOrigin.Begin);

        Byte[] riff = System.Text.Encoding.UTF8.GetBytes("RIFF");
        stream.Write(riff, 0, 4);

        Byte[] chunkSize = BitConverter.GetBytes(stream.Length - 8);
        stream.Write(chunkSize, 0, 4);

        Byte[] wave = System.Text.Encoding.UTF8.GetBytes("WAVE");
        stream.Write(wave, 0, 4);

        Byte[] fmt = System.Text.Encoding.UTF8.GetBytes("fmt ");
        stream.Write(fmt, 0, 4);

        Byte[] subChunk1 = BitConverter.GetBytes(16);
        stream.Write(subChunk1, 0, 4);

        UInt16 one = 1;

        Byte[] audioFormat = BitConverter.GetBytes(one);
        stream.Write(audioFormat, 0, 2);

        Byte[] numChannels = BitConverter.GetBytes(channels);
        stream.Write(numChannels, 0, 2);

        Byte[] sampleRate = BitConverter.GetBytes(hz);
        stream.Write(sampleRate, 0, 4);

        Byte[] byteRate = BitConverter.GetBytes(hz * channels * 2);
        stream.Write(byteRate, 0, 4);

        UInt16 blockAlign = (ushort)(channels * 2);
        stream.Write(BitConverter.GetBytes(blockAlign), 0, 2);

        UInt16 bps = 16;
        Byte[] bitsPerSample = BitConverter.GetBytes(bps);
        stream.Write(bitsPerSample, 0, 2);

        Byte[] datastring = System.Text.Encoding.UTF8.GetBytes("data");
        stream.Write(datastring, 0, 4);

        Byte[] subChunk2 = BitConverter.GetBytes(samples * channels * 2);
        stream.Write(subChunk2, 0, 4);
    }
    /// <summary>
    /// ����wav��ʽ�ļ�ͷ
    /// </summary>
    /// <param name="filepath"></param>
    /// <returns></returns>
    private FileStream CreateEmpty(string filepath)
    {
        FileStream fileStream = new FileStream(filepath, FileMode.Create);
        byte emptyByte = new byte();

        for (int i = 0; i < 44; i++) //Ϊwav�ļ�ͷ�����ռ�
        {
            fileStream.WriteByte(emptyByte);
        }

        return fileStream;
    }
    #endregion
}
