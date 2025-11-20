using UnityEngine;

public class EnemyChase : AState<EnemyBehaviour>
{
    private float _chaseSpeed = 4;

    public override void Start(EnemyBehaviour runner)
    {
        Debug.Log("Enter Chase");
        base.Start(runner);
    }

    public override void Update(EnemyBehaviour runner)
    {
        base.Update(runner);

        if (runner.gameobject != null)
            runner.gameobject.transform.position = Vector3.MoveTowards(runner.gameobject.transform.position, runner._player.position, _chaseSpeed * Time.deltaTime);

        if (!runner.inChaseRange) onSwitch(runner.idleState);
    }

    public override void Complete(EnemyBehaviour runner)
    {
        base.Complete(runner);
    }

}
