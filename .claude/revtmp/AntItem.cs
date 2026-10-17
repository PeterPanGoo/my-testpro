namespace AntdUI;

public class AntItem : NotifyProperty
{
	private object? _value;

	public string key { get; set; }

	public object? value
	{
		get
		{
			return _value;
		}
		set
		{
			if (_value != value)
			{
				_value = value;
				OnPropertyChanged(key);
			}
		}
	}

	public AntItem(string k)
	{
		key = k;
	}

	public AntItem(string k, object? v)
	{
		key = k;
		value = v;
	}

	public bool Try<T>(out T? val)
	{
		if (_value is T val2)
		{
			val = val2;
			return true;
		}
		val = default;
		return false;
	}

	public override string? ToString()
	{
		return _value?.ToString();
	}
}
