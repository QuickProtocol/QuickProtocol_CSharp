namespace Quick.Protocol;

public interface IQpSerializer
{
    string Serialize(object obj);
    object Deserialize(string value);
}

public interface IQpModel<T> : IQpSerializer
{
    string Serialize(T obj);
    new T Deserialize(string value);
}
