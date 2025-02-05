
namespace DataAccess
{
    public interface IGenericDAO<T>
    {
        int Size();
        ICollection<T> GetAll();
        T? GetById(int id);
    }
}
