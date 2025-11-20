public interface IPoolable
{
    bool active { get; set; }
    void OnEnableObject();
    void OnDisableObject();
}
