public class BufferedMessage
{
    public readonly int channelType;
    public readonly byte[] rawData;
    public readonly int writtenSize;
    public readonly bool sendToLocal;

    public BufferedMessage(int channelType, byte[] rawData, int writtenSize, bool sendToLocal)
    {
        this.channelType = channelType;
        this.rawData = rawData;
        this.writtenSize = writtenSize;
        this.sendToLocal = sendToLocal;
    }
}