using UnityEngine;

public class PlayerIdle : AState<PlayerController>
    {
        public override void Start(PlayerController runner)
        {
            base.Start(runner);
        }

        public override void Update(PlayerController runner)
        {
            base.Update(runner);
            if(runner.MoveDirection().magnitude > 0.1)
            {
                onSwitch(runner.moveState);
            }
        }

        public override void Complete(PlayerController runner)
        {
            base.Complete(runner);
        }
    }
