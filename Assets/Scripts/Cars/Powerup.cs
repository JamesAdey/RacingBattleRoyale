public abstract class Powerup
{
    private byte id;

    internal void SetID(byte newId)
    {
        id = newId;
    }

    public byte GetByteID()
    {
        return id;
    }

    public abstract void Use(BaseCar car);

}