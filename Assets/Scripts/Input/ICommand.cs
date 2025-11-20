public interface ICommand
{
    public IAbilityActor actor { get; }
    public void Execute();
}
