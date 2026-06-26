using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

class InteractCommand : ICommand
{
    public IAbilityActor actor { get; private set; }
    private readonly PlayerController _player;

    public InteractCommand(PlayerController player)
    {
        _player = player;
    }

    public void Execute()
    {
        Interact();
    }

    public void Interact()
    {
        if (_player.chestInRange != null)
            _player.chestInRange.OnActivation(_player);

        if (_player.staircaseInRange != null)
            _player.staircaseInRange.OnActivation();
    }
}
