using UnityEngine;
public class EnemyIdle : AState<EnemyBehaviour>
{
    private float _timer = 3f;
    private float _currentTime;

    public override void Start(EnemyBehaviour runner)
    {
        base.Start(runner);
        _currentTime = _timer;
    }

    public override void Update(EnemyBehaviour runner)
    {
        base.Update(runner);

        //# Timer to Switch to patrol*
        _currentTime -= Time.deltaTime;

        if (runner.inChaseRange)
        {
            onSwitch(runner.chaseState);
            return;
        }

        if (_currentTime <= 0f)
        {
            onSwitch(runner.patrolState);
            return;
        }
    }

    public override void Complete(EnemyBehaviour runner)
    {
        base.Complete(runner);
    }
}
