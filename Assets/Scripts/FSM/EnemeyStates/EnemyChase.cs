using System;
using System.Collections.Generic;
using UnityEngine;

public class EnemyChase : AState<EnemyBehaviour>
{
    private List<Vector3> _currentPath;
    private int _currentWaypointIndex;

    private Vector3Int _lastTargetCell;
    private float _repathTimer;
    private const float RepathInterval = 0.1f;

    public override void Start(EnemyBehaviour runner)
    {
        base.Start(runner);

        _currentPath = null;
        _currentWaypointIndex = 0;
        _repathTimer = 0f;

        RecalculatePath(runner);
    }

    public override void Update(EnemyBehaviour runner)
    {
        if (runner.gameobject == null || GameHandler.instance == null)
            return;

        base.Update(runner);

        if (!runner.inChaseRange)
        {
            onSwitch(runner.idleState);
            return;
        }

        if (runner.hasReachedThreshold)
        {
            onSwitch(runner.attackState);
            return;
        }

        var floorMap = GameHandler.instance.FloorMap;
        _repathTimer += Time.deltaTime;


        if (floorMap != null && runner._player != null)
        {
            Vector3Int playerCell = floorMap.WorldToCell(runner._player.position);
            if (playerCell != _lastTargetCell || _repathTimer >= RepathInterval)
            {
                RecalculatePath(runner);
            }
        }

        Vector3 targetPos = _currentPath[_currentWaypointIndex];
        Vector3 currentPos = runner.gameobject.transform.position;

        runner.gameobject.transform.position = Vector3.MoveTowards(currentPos, targetPos, runner.Speed * Time.deltaTime);

        if (Vector3.Distance(runner.gameobject.transform.position, targetPos) < 0.1f)
        {
            _currentWaypointIndex++;
        }

        if (_currentPath == null || _currentPath.Count == 0)
        {
            runner.gameobject.transform.position = Vector3.MoveTowards(runner.gameobject.transform.position, runner._player.position, runner.Speed * Time.deltaTime);
            return;
        }
    }

    public override void Complete(EnemyBehaviour runner)
    {
        base.Complete(runner);
        _currentPath = null;
        _currentWaypointIndex = 0;
    }

    private void RecalculatePath(EnemyBehaviour runner)
    {
        _repathTimer = 0f;
        if (GameHandler.instance == null || GameHandler.instance.Pathfinder == null)
        {
            _currentPath = null;
            _currentWaypointIndex = 0;
            return;
        }
        _currentPath = GameHandler.instance.Pathfinder.FindPath(
            runner.gameobject.transform.position, runner._player.position);

        _currentWaypointIndex = 0;

        if (_currentPath != null)
        {
            if (_currentPath.Count < 2)
            {
                _currentPath = null;
            }
            else
            {
                _currentWaypointIndex = 1;
            }
        }

        if (GameHandler.instance.FloorMap != null)
        {
            _lastTargetCell = GameHandler.instance.FloorMap.WorldToCell(runner._player.position);
        }
    }
}
