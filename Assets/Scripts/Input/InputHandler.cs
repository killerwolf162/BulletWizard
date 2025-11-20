using System.Collections.Generic;
using UnityEngine;

public enum KeypressType
{
    Hold,
    Down,
    Up
}

public class InputHandler
{
    private List<KeyCommand> commands = new List<KeyCommand>();

    public void HandleInput()
    {
        foreach (var binding in commands)
        {
            switch (binding.inputType)
            {
                case KeypressType.Hold:
                    if (Input.GetKey(binding.key))
                    {
                        binding.command.Execute();
                    }
                    break;

                case KeypressType.Down:
                    if (Input.GetKeyDown(binding.key))
                    {
                        binding.command.Execute();
                    }
                    break;

                case KeypressType.Up:
                    if (Input.GetKeyUp(binding.key))
                    {
                        binding.command.Execute();
                    }
                    break;

                default:
                    break;
            }
        }
    }
    public void BindKeyToCommand(KeyCode key, KeypressType inputType, ICommand command)
    {
        commands.Add(new KeyCommand
        {
            key = key,
            command = command,
            inputType = inputType
        });
    }

    public void UnbindKey(KeyCode key)
    {
        var items = commands.FindAll(x => x.key == key);
        items.ForEach(x => commands.Remove(x));
    }
}

public class KeyCommand
{
    public KeyCode key;
    public ICommand command;
    public KeypressType inputType;
}
