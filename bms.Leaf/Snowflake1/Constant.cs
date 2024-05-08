namespace Bms.Leaf.Snowflake
{
    public class Constant
    {
        public static readonly string RootName = "snowflake-id-generate";
        public static readonly string PersistentName = "persistent";
        public static readonly string EphemeralName = "ephemeral";

        public static readonly string Colon = ":";
        public static readonly string Slash = "/";
        public static readonly string Strike = "-";
        public static readonly int MaxSequential = 1024;

        public static readonly long Bit = 31L;

        // Redis脚本用于添加持久节点
        public static readonly string RedisAddPersistentScript = @"
            local table_maxn = function(t)
              local mn = -1;
              for k, v in pairs(t) do
                local n = v + 0
                if mn < n then
                  mn = n;
                end
              end
              return mn;
            end
            redis.call('HSET', KEYS[2], KEYS[3], ARGV[1]);
            local sq = redis.call('HGET', KEYS[1], KEYS[3]);
            if (sq) then
              return sq + 0;
            end
            local vals = redis.call('HVALS', KEYS[1]);
            local max = table_maxn(vals);
            local v = max + 1;
            redis.call('HSET', KEYS[1], KEYS[3], v);
            return v;";

        // Redis脚本用于更新临时节点
        public static readonly string RedisUpdateEphemeralScript = @"
            redis.call('HSET', KEYS[2], KEYS[3], ARGV[1]);
            redis.call('SET', KEYS[1], ARGV[1], 'EX', ARGV[2]);";
    }
}
