    //polymorphism, the ability for objects of different classes to be treated
    //as objects of a common base class, allowing methods to be invoked on them dynamically at runtime.
    public delegate void StateEvent<T>(IState<T> state) where T : IStateRunner;

    public interface IState<T> where T : IStateRunner
    {
        void Start(T runner);
        void Update(T runner);
        void FixedUpdate(T runner);
        void Complete(T runner);

        StateEvent<T> onSwitch { get; set; }
    }

    public interface IStateRunner
    {
        ScratchPad sharedData { get; }
    }

    public interface IPhysicsControllable
    {
        void Push(float x, float y, float z);
        void Drag(float x, float y, float z);
    }
