using System.Collections.Generic;

public class CheckableVariable<T>
{
    public CheckableVariable(T value)
    {
        _value = value;
    }
    public delegate void OnVariableChangeDelegate(T newVal);
    public event OnVariableChangeDelegate OnVariableChange;

    private T _value;
    public T Value
    {
        get => _value;
        set
        {
            if (EqualityComparer<T>.Default.Equals(_value, value)) return;
            _value = value;
            OnVariableChange?.Invoke(value);
        }
    }
}