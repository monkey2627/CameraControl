using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public class ByteObject
{
    private const int DEFAULTSIZE = 1024;
    //起始大小
    private int initSize;
    //缓冲区
    public byte[] bytes = new byte[1024];
    //缓冲区容量
    private int capacity = 0;
    //当前缓冲区的读写位置,写是指数据的尽头,读是我现在解析到哪了
    public int readIndex;
    public int writeIndex;
    //剩余空间
    public int remainLength { get { return capacity - writeIndex; } }
    //发送缓冲区剩余未发送的长度
    public int dataLength{ get { return writeIndex - readIndex; } }
    public ByteObject(byte[] defaultBytes)
    {
        bytes = defaultBytes;
        capacity = initSize =  writeIndex = defaultBytes.Length;
        readIndex = 0;
       
    }
    public ByteObject(int size = DEFAULTSIZE){

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

