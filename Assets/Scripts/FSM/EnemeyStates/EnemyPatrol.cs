using UnityEngine;

 public class EnemyPatrol : AState<EnemyBehaviour>
    {
        private Vector3 _walkPoint;
        private float _walkPointRange = 10f;
        private bool _walkPointSet;
        private float _speed = 2;

        public override void Start(EnemyBehaviour runner)
        {
            Debug.Log("Enter Patrol");
            base.Start(runner);
        }

        public override void Update(EnemyBehaviour runner)
        {
            base.Update(runner);

            if (!_walkPointSet) GetNewWalkPoint(runner);

            if (_walkPointSet) 
            {
                runner.gameobject.transform.position = Vector3.MoveTowards(runner.gameobject.transform.position, _walkPoint, _speed * Time.deltaTime);
            }

            Vector3 disToWalkPoint = runner.gameobject.transform.position - _walkPoint;

            if (disToWalkPoint.magnitude < 0.1)
            {
                onSwitch(runner.idleState);
            }

        }

        public override void Complete(EnemyBehaviour runner)
        {
            _walkPointSet = false;
            base.Complete(runner);
        }

        private void GetNewWalkPoint(EnemyBehaviour runner)
        {
            float randomY = Random.Range(-_walkPointRange, _walkPointRange);
            float randomX = Random.Range(-_walkPointRange, _walkPointRange);

            Vector3 pos = runner.gameobject.transform.position;
            _walkPoint = new Vector3(pos.x + randomX, pos.y + randomY, 0);
            _walkPointSet = true;
        }

    }
