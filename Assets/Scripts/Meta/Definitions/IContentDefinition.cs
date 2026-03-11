namespace WaveGame.Meta.Definitions
{
    public interface IContentDefinition
    {
        string ContentId { get; }
        ContentRarity Rarity { get; }
    }
}
