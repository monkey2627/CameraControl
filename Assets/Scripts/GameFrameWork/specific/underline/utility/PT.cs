using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Text;
using System;
using System.Linq;
//Э���װ�����

public class PT 
{
    public static byte[] EncodeBody(PTBase ptBase)
    {
       string str =  JsonUtility.ToJson(ptBase);
        return Encoding.UTF8.GetBytes(str);
    }
    public static PTBase DecodeBody(string ptName,byte[] bytes,int startIndex,int count)
    {
        string str = Encoding.UTF8.GetString(bytes,startIndex,count);
        PTBase ptBase = (PTBase)JsonUtility.FromJson(str, Type.GetType(ptName));
        return ptBase;
    }
    //���ֳ���+��������
    public static byte[] EncodeName(PTBase ptBase)
    {
        byte[] nameBytes = Encoding.UTF8.GetBytes(ptBase.ptName);
        Int16 length = (Int16) nameBytes.Length;
        Debug.Log(length);
        byte[] lenthBytes = BitConverter.GetBytes(length);
        if (!BitConverter.IsLittleEndian)
        {
            lenthBytes.Reverse();
        }
        byte[] sendBytes = lenthBytes.Concat(nameBytes).ToArray();
        return sendBytes;
    }
    public static string DecodeName(byte[] bytes, int startIndex,out int count)
    {
        count = 0;
        if (bytes.Length < 2+startIndex)
        {
            return "";
        }
        //����С�˴�ģ����Ǽ��㷽ʽ
        Int16 length = (Int16)(bytes[startIndex] |bytes[startIndex + 1] << 8);
        Debug.Log("Э�������ȣ�"+length);
        count = length + 2;
        if(2 + length + startIndex > bytes.Length)
        {
            Debug.Log("����Э��ʧ��");
            return "";
        } 
       
        return Encoding.UTF8.GetString(bytes,startIndex+2,length);
    }
}
