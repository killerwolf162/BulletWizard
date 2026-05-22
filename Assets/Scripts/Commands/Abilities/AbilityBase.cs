using UnityEngine;

public abstract class AbilityBase : IUpdateable
{
    protected abstract float cooldownTime { get; }
    protected int manaCost;
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
            Cast();
        }
    }

    public abstract void Cast();

    public bool CooldownReady()
    {
        return activeCooldown <= 0;
    }

    public float CooldownProgress => cooldownTime > 0
    ? Mathf.Clamp01(activeCooldown / cooldownTime)
    : 0f;
}
