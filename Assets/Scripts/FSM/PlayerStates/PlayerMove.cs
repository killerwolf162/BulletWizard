using UnityEngine;

public class PlayerMove : AState<PlayerController>
{
    private float _moveSpeed = 5f;

    public override void Start(PlayerController runner)
    {
        base.Start(runner);
    }

    public override void Update(PlayerController runner)
    {
        Vector2 input = runner.MoveDirection();
        if (input.sqrMagnitude < 0.01f)
        {
            onSwitch(runner.idleState);
            return;
        }

        Vector2 moveDir = input.normalized;
        float distance = _moveSpeed * Time.deltaTime;

        var filter = new ContactFilter2D
        {
            useTriggers = false
        };
        filter.SetLayerMask(PlayerController.WALL_LAYER_MASK);
        RaycastHit2D[] hits = new RaycastHit2D[1];
        int hitCount = runner.col.Cast(moveDir, filter, hits, distance);

        if(hitCount == 0)
        {
            Vector2 newPos = runner.rb.position + moveDir * distance;
            runner.rb.MovePosition(newPos);
        }
    }

    public override void Complete(PlayerController runner)
    {
        base.Complete(runner);
    }
}
