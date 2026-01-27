namespace Saves
{
    public interface ISavable
    {
        public void Save(SaveData saveData);

        public void Load(SaveData saveData);
    }
}