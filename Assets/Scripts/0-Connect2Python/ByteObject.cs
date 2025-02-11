using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public class DataObject
{
    //缓冲区默认大小
    private const int DEFAULTSIZE = 1024;
    //起始大小
    private int initSize;
    //缓冲区
    public byte[] bytes = new byte[1024];
    //缓冲区容量
    private int capacity = 0;
    //这里的读写指的是针对缓冲区的操作，不是真正意义上的读写
    //当前缓冲区的读写位置,写是指数据的尽头,读是我现在解析到哪了
    public int readIndex;
    //dataobject接收字节流数据接收到哪了
    public int writeIndex;
    //剩余空间
    public int remainLength { get { return capacity - writeIndex; } }
    //发送缓冲区剩余未发送的长度
    public int dataLength{ get { return writeIndex - readIndex; } }
    public DataObject(byte[] defaultBytes)
    {
        bytes = defaultBytes;
        capacity = initSize =  writeIndex = defaultBytes.Length;
        readIndex = 0;
       
    }
    public DataObject(int size = DEFAULTSIZE){

        bytes = new byte[size];
        capacity = initSize = size;
        readIndex = writeIndex = 0;
    }
    //扩容缓冲区大小
    public void Resize(int size)
    {
        if(size < dataLength)
        {
            return;
        }
        if(size < initSize)
        {
            return;
        }
        int useCapicity = 1;
        while(useCapicity < size)
        {
            useCapicity *= 2;
        }
        capacity = useCapicity;
        byte[] newBytes = new byte[capacity];
        Array.Copy(bytes, readIndex, newBytes, 0, writeIndex - readIndex);
        bytes = newBytes;
        writeIndex = dataLength;
        readIndex = 0;
    }
    //检查并移动数据
    public void CheckAndMoveBytes()
    {
        if( dataLength < 8)
        {
            MoveBytes();
        }
    }
    //具体的移动方法
    public void MoveBytes()
    { 
        Array.Copy(bytes, readIndex, bytes,0,dataLength);
       
        writeIndex = dataLength;
        readIndex = 0;
    }

}

