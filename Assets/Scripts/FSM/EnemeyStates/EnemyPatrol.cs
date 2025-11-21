using UnityEngine;

public class EnemyPatrol : AState<EnemyBehaviour>
{
    private Vector3 _walkPoint;
    private bool _walkPointSet;

    public override void Start(EnemyBehaviour runner)
    {
        Debug.Log("Enter Patrol");
        _walkPointSet = false;
        base.Start(runner);
    }

    public override void Update(EnemyBehaviour runner)
    {
        base.Update(runner);

        if (!_walkPointSet) 
            GetNewWalkPoint(runner);

        if (_walkPointSet)
        {
            var currentPos = runner.gameobject.transform.position;
            float speed = runner.Speed;

            runner.gameobject.transform.position = Vector3.MoveTowards(currentPos, _walkPoint, speed * Time.deltaTime);

            Vector3 disToWalkPoint = runner.gameobject.transform.position - _walkPoint;

            if (disToWalkPoint.magnitude < 0.1)
            {
                onSwitch(runner.idleState);
            }
        }    
    }

    public override void Complete(EnemyBehaviour runner)
    {
        _walkPointSet = false;
        base.Complete(runner);
    }

    private void GetNewWalkPoint(EnemyBehaviour runner)
    {
        var patrolPoints = runner.PatrolPoints;

        if(patrolPoints != null && patrolPoints.Count > 0)
        {
            int idx = Random.Range(0, patrolPoints.Count);
            _walkPoint = patrolPoints[idx];
            _walkPointSet = true;
        }
    }

}
