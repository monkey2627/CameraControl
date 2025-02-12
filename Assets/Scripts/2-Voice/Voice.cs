using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
//进行录音的类，用音量变化来开始和结束录音
public class Voice : MonoBehaviour
{
    //进行音频压缩 https://zhuanlan.zhihu.com/p/139347299
    public static Voice instance;
    // 当前使用的麦克风名字
    private string microphoneName;
    // 麦克风列表的字符串
    private string[] micDevicesNames;
    public float minVolumn = 0.6f;
    // 是否开始录音
    private bool isRecord = false;
    //
    private AudioClip micRecord;
    private void Awake()
    {
        instance = this;        
        // 获取麦克风列表
        micDevicesNames = Microphone.devices;
        if (micDevicesNames.Length <= 0){
            Debug.Log("缺少麦克风设备");
        }
        else{ 
            // 获得列表中第一个麦克风）
            microphoneName = micDevicesNames[0];  
            Debug.Log("当前用户录音的麦克风名字为：" + micDevicesNames[0]);
        
        }
    }
    public void StartRecord()
    {
        if (!isRecord)
        {
            micRecord = Microphone.Start(microphoneName, true, 60 * 10, 16000);//44100音频采样率   固定格
            isRecord = true;
            StartCoroutine("RecordAudioIENU");
        }
        else
        {
            Debug.Log("已经启动录音功能了");
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
        //实际麦克风一直开着，只是数据没存
        //检测到声音超过阈值并且现在没有在“录音”，开启录音
        if (isRecord)
        {
            float v = GetVolume();
            if (v > minVolumn && !isRecording) {
            
                beginPos = Microphone.GetPosition(microphoneName);
                isRecording = true;
                saved = false;
                Debug.Log("开始记录"+beginPos);
            };
            quietSeconds(v);
    
            if (v < minVolumn && quiet > 2f && isRecording && !saveRecord && !saved){
                Debug.Log("结束记录" + Microphone.GetPosition(microphoneName));
                saveRecord = true;
            } /**/
        }
    }
    //用协程来进行录制

  
    private IEnumerator RecordAudioIENU()
    {
        yield return new WaitForSeconds(1f);
        while (true)//一直循环监听，unity只负责传数据
        {
            if (saveRecord)
            {
                //getPosition  获取在录制样本中现在的位置。
                int now = Microphone.GetPosition(microphoneName);
                int offset = now - beginPos;
                // 录音的长度乘以它的通道
                float[] soundata = new float[offset * 1];
                // 使用剪辑中的数据填充数组，后者是开始读取的位置
                micRecord.GetData(soundata, beginPos);
               
                // 遍历所有采集到的音频信息
                byte[] outData = new byte[soundata.Length * 2];
                // short类型储存在最大值 
                int rescaleFactor = 32767;
                for (int i = 0; i < soundata.Length; i++)
                {
                    // short类型属于带符号的短整数类型，short类型占2字节(16位)内存空间，存储-32768 到 32767
                    short temshort = (short)(soundata[i] * rescaleFactor);
                    //不同的数据类型和字节数组之间进行转换
                    byte[] temdata = BitConverter.GetBytes(temshort);
                    outData[i * 2] = temdata[0];
                    outData[i * 2 + 1] = temdata[1];
                }
                saveRecord = false;
                isRecording = false;
                saved = true;
                audioList.Add(outData);
                //保存每一次的录音在本地
                Connected2Baidu.instance.Recognition(outData);
                SaveRecord(outData,soundata);
                //直到saveRecord 为真才开启下一次协程
           
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
            WriteHeader(fs, micRecord); //wav文件头
        } 
    }
    /// <summary>获取麦克风音量</summary>
    /// <returns>麦克风的音量数值</returns>
    public virtual float GetVolume()
    {
        float levelMax = 0;
        if (Microphone.IsRecording(microphoneName))
        {
            float[] samples = new float[16000];
            int startPosition = Microphone.GetPosition(microphoneName) - (16000 + 1);
            if (startPosition >= 0)
            {//当麦克风还未正式启动时，该值会为负值，AudioClip.GetData函数会报错
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
               // Debug.Log("麦克风音量：" + levelMax);
            }
        }
        return levelMax;
    }
    /// <summary>
    /// 写文件头
    /// </summary>
    /// <param name="stream"></param>
    /// <param name="clip"></param>
    # region 工具方法
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
    /// 创建wav格式文件头
    /// </summary>
    /// <param name="filepath"></param>
    /// <returns></returns>
    private FileStream CreateEmpty(string filepath)
    {
        FileStream fileStream = new FileStream(filepath, FileMode.Create);
        byte emptyByte = new byte();

        for (int i = 0; i < 44; i++) //为wav文件头留出空间
        {
            fileStream.WriteByte(emptyByte);
        }

        return fileStream;
    }
    #endregion
}
