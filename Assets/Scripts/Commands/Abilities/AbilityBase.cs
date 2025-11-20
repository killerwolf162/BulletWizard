using UnityEngine;

public abstract class AbilityBase : IUpdateable
{
    protected abstract float cooldownTime { get; }
    protected float activeCooldown = 0;

    public void Start()
    {
        GameHandler.instance.Subscribe(this);
    }

    public virtual void Update()
    {
        activeCooldown -= Time.deltaTime;
    }

    public virtual void UseAbility()
    {
        if (CooldownReady())
        {
            activeCooldown = cooldownTime;
            Cast();
        }
    }

    public abstract void Cast();

    public bool CooldownReady()
    {
        return activeCooldown <= 0;
    }
}
