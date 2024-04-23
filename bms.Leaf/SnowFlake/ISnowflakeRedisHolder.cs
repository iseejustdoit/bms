namespace bms.Leaf.SnowFlake
{
    public interface ISnowflakeRedisHolder
    {
        bool Init();

        int GetWorkerId();
    }
}
