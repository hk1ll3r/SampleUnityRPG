public class UIProp<T> where T : class {
    private T _value = null;

    public T Value {
        get { return _value; }
        set { _value = value; }
    }

    public void Set (T value) { _value = value; }

    public void Clear() { _value = null; }

    public bool HasValue {
        get { return _value != null; }
    }

    public UIProp(T value = null) {
        Value = value;
    }
}