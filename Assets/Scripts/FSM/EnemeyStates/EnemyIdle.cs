using UnityEngine;
public class EnemyIdle : AState<EnemyBehaviour>
{
    private float _timer = 3f;
    private float _currentTime;

    public override void Start(EnemyBehaviour runner)
    {
        Debug.Log("Enter IdleState");
        base.Start(runner);
        _currentTime = _timer;
    }

    public override void Update(EnemyBehaviour runner)
    {
        base.Update(runner);

        //# Timer to Switch to patrol*
        _currentTime -= Time.deltaTime;

        if (_currentTime <= 0f)
        {
            onSwitch(runner.patrolState);
        }
    }

    public override void Complete(EnemyBehaviour runner)
    {
        base.Complete(runner);
    }
}
