using UnityEngine;

public class ManaUpgradeItem : AbstractItem
{
    private readonly int _amount;

    public ManaUpgradeItem(int amount = 3)
    {
        _amount = amount;
        Name = "Mana Crystal";
        Description = $"Increases max mana by {amount}";
    }

    public override void Apply(PlayerController player) => player.IncreaseMaxMana(_amount);
}
