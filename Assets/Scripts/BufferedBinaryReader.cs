using System;
using System.IO;

public class BufferedBinaryReader : IDisposable
{
    public  Stream Stream { get; private set; }
    private readonly byte[] buffer;
    private readonly int bufferSize;
    private int bufferOffset;
    private int numBufferedBytes;
 
    public BufferedBinaryReader(Stream stream, int bufferSize)
    {
        this.Stream = stream;
        this.bufferSize = bufferSize;
        buffer = new byte[bufferSize];
        bufferOffset = bufferSize;
    }
 
    public int NumBytesAvailable { get { return Math.Max(0, numBufferedBytes - bufferOffset); } }
 
    public bool FillBuffer()
    {
        var numBytesUnread = bufferSize - bufferOffset;
        var numBytesToRead = bufferSize - numBytesUnread;
        bufferOffset = 0;
        numBufferedBytes = numBytesUnread;
        if (numBytesUnread > 0)
        {
            Buffer.BlockCopy(buffer, numBytesToRead, buffer, 0, numBytesUnread);
        }
        while (numBytesToRead > 0)
        {
            var numBytesRead = Stream.Read(buffer, numBytesUnread, numBytesToRead);
            if (numBytesRead == 0)
            {
                return false;
            }
            numBufferedBytes += numBytesRead;
            numBytesToRead -= numBytesRead;
            numBytesUnread += numBytesRead;
        }
        return true;
    }
 
    public short ReadInt16()
    {
        var val = (short)((int)buffer[bufferOffset] | (int)buffer[bufferOffset+1] << 8);
        bufferOffset += 2;
        return val;
    }
 
    public void Dispose()
    {
        Stream.Close();
    }
}