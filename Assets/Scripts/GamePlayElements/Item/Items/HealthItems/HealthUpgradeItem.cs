using UnityEngine;

public class HealthUpgradeItem : AbstractItem
{
    private readonly int _amount;

    public HealthUpgradeItem(string name = "Health Crystal", int amount = 3)
    {
        _amount = amount;
        Name = name;
        Description = $"Increases max HP by {amount}";
    }

    public override void Apply(PlayerController player) => player.IncreaseMaxHP(_amount);
}
