using UnityEngine;

public class SetDecoratorCommand : ICommand
{
    public IAbilityActor actor { get; private set; }
    private PlayerController _player;
    private BulletDecorator _decorator;

    public SetDecoratorCommand(PlayerController player, BulletDecorator decorator)
    {
        _player = player;
        _decorator = decorator;
    }


    public void Execute()
    {
        _player.activeDecorator = _decorator;
    }
}
