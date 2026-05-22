using System.Collections.Generic;
using UnityEngine;

public class EnemyPatrol : AState<EnemyBehaviour>
{
    private Vector3 _walkPoint;
    private bool _walkPointSet;

    private List<Vector3> _currentPath;
    private int _currentWaypointIndex;

    public override void Start(EnemyBehaviour runner)
    {
        base.Start(runner);

        _walkPointSet = false;
        _currentPath = null;
        _currentWaypointIndex = 0;
    }

    public override void Update(EnemyBehaviour runner)
    {
        if (runner == null || runner.gameobject == null)
            return;

        base.Update(runner);

        if(runner.inChaseRange)
        {
            onSwitch(runner.chaseState);
            return;
        }

        if (!_walkPointSet)
        {
            GetNewWalkPoint(runner);
            if (_walkPointSet)
            {
                RecalculatePath(runner);
            }
        }

        if (!_walkPointSet)
            return;

        bool hasPathfinder = GameHandler.instance != null && GameHandler.instance.Pathfinder != null;
        if (!hasPathfinder || _currentPath == null || _currentPath.Count <= 1)
        {
            MoveDirectlyTowards(runner, _walkPoint);
        }
        else
        {
            FollowPath(runner);
        }

        if (Vector3.Distance(runner.gameobject.transform.position, _walkPoint) < 0.1f)
        {
            onSwitch(runner.idleState);
        }
    }

    public override void Complete(EnemyBehaviour runner)
    {
        _walkPointSet = false;
        _currentPath = null;
        _currentWaypointIndex = 0;
        base.Complete(runner);
    }

    private void MoveDirectlyTowards(EnemyBehaviour runner, Vector3 target)
    {
        Vector3 currentPos = runner.gameobject.transform.position;
        float speed = runner.Speed;

        runner.gameobject.transform.position = Vector3.MoveTowards(currentPos, target, speed * Time.deltaTime);
    }

    private void FollowPath(EnemyBehaviour runner)
    {
        if (_currentWaypointIndex < 0 || _currentWaypointIndex >= _currentPath.Count)
        {
            RecalculatePath(runner);
            if (_currentPath == null || _currentPath.Count <= 1)
            {
                MoveDirectlyTowards(runner, _walkPoint);
                return;
            }
        }

        Vector3 currentPos = runner.gameobject.transform.position;
        float speed = runner.Speed;

        Vector3 waypoint = _currentPath[_currentWaypointIndex];

        runner.gameobject.transform.position = Vector3.MoveTowards(
            currentPos,
            waypoint,
            speed * Time.deltaTime);

        if (Vector3.Distance(runner.gameobject.transform.position, waypoint) < 0.05f)
        {
            _currentWaypointIndex++;

            if (_currentWaypointIndex >= _currentPath.Count)
            {
                _currentPath = null;
            }
        }
    }

    private void RecalculatePath(EnemyBehaviour runner)
    {
        if (GameHandler.instance == null || GameHandler.instance.Pathfinder == null)
        {
            _currentPath = null;
            _currentWaypointIndex = 0;
            return;
        }

        _currentPath = GameHandler.instance.Pathfinder.FindPath(runner.gameobject.transform.position, _walkPoint);

        if (_currentPath != null && _currentPath.Count >= 2)
        {
            _currentWaypointIndex = 1;
        }
        else
        {
            _currentPath = null;
            _currentWaypointIndex = 0;
        }
    }

    private void GetNewWalkPoint(EnemyBehaviour runner)
    {
        var patrolPoints = runner.PatrolPoints;

        if (patrolPoints == null || patrolPoints.Count == 0)
        {
            _walkPointSet = false;
            return;
        }

        int idx = Random.Range(0, patrolPoints.Count);
        _walkPoint = patrolPoints[idx];

        if (Vector3.Distance(runner.gameobject.transform.position, _walkPoint) < 0.1f && patrolPoints.Count > 1)
        {
            int safety = 0;
            while (Vector3.Distance(runner.gameobject.transform.position, _walkPoint) < 0.1f && safety < 5)
            {
                idx = Random.Range(0, patrolPoints.Count);
                _walkPoint = patrolPoints[idx];
                safety++;
            }
        }

        _walkPointSet = true;
    }
}
