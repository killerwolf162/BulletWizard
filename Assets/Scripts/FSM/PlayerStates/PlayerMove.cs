using UnityEngine;

public class PlayerMove : AState<PlayerController>
    {
        private float _moveSpeed = 8f;

        public override void Start(PlayerController runner)
        {
            base.Start(runner);
        }

        public override void Update(PlayerController runner)
        {
            Vector3 dir = new Vector3(runner.MoveDirection().x, runner.MoveDirection().y ,0);

            runner.rb.MovePosition(runner.gameobject.transform.position + dir * _moveSpeed * Time.deltaTime);

            if (runner.MoveDirection().magnitude < 0.1)
            {
                onSwitch(runner.idleState);
            }
        }

        public override void Complete(PlayerController runner)
        {
            base.Complete(runner);
        }
    }
