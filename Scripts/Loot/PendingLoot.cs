public class PendingLoot
{
    public int num { get; set; } = 3;
    public LootType? lootType { get; set; }
    public int? wave { get; set; }
    public PendingLoot(int num, LootType? lootType = null, int? wave = null)
    {
        this.num = num;
        this.lootType = lootType;
        this.wave = wave;
    }
}