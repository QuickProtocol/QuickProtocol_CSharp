namespace Quick.Protocol.Model;

public interface IQpModel
{
    string Serialize(object obj);
    object Deserialize(string value);
}

public interface IQpModel<T> : IQpModel
{
    string Serialize(T obj);
    new T Deserialize(string value);
}
