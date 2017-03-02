using GenderPayGap.Core.Classes;

namespace GenderPayGap.Core.Interfaces
{
    public interface IPagedRepository<T>
    {
        void Insert(T entity);
        void Delete(T entity);

        PagedResult<T> Search(string searchText, int page, int pageSize);
    }
}
