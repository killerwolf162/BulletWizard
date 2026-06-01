using UnityEngine;

public abstract class AbstractItem
{
    
    public string Name { get; protected set; }
    public string Description { get; protected set; }
    public string StatChange { get; protected set; }

    public abstract void Apply(PlayerController player);

}
