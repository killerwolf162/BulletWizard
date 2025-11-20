 public abstract class AState<T> : IState<T> where T : IStateRunner
    {
        public virtual void Start(T runner) { }
        public virtual void Update(T runner) { }
        public virtual void FixedUpdate(T runner) { }
        public virtual void Complete(T runner) { }

        public StateEvent<T> onSwitch { get; set; }
    }
