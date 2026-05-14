
using UnityEngine;

public class EnemyAttack : AState<EnemyBehaviour>
{
    private float _attackTimer = 2f;
    private float _currentTime;

    public override void Start(EnemyBehaviour runner)
    {
        Debug.Log("Enter Attack");
        base.Start(runner);
    }

    public override void Update(EnemyBehaviour runner)
    {
        base.Update(runner);

        _currentTime -= Time.deltaTime;
        //Attack Logic    
        if (_currentTime <= 0f)
        {             
            runner.OnEnemeyShootBullet();
            _currentTime = _attackTimer;
        }

        if (!runner.inAttackRange)
        {
            onSwitch(runner.chaseState);
            return;
        }
    }

    public override void Complete(EnemyBehaviour runner)
    {
        base.Complete(runner);
    }
}
