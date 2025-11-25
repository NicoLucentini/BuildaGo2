public abstract class ISaveable<T>
{
    public void SaveData(string path, T obj)
    {
        DataSaver.SaveData(path, obj);
    }
    public T LoadData(string path)
    {
        var temp = DataSaver.LoadData<T>(path);
        return temp == null ? GetDefault() : temp;
    }
    public abstract T GetDefault();
}
