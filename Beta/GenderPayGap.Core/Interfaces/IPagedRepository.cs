using GenderPayGap.Core.Classes;

namespace GenderPayGap.Core.Interfaces
{
    public interface IPagedRepository<T>
    {
        PagedResult<T> Search(string searchText, int page, int pageSize);
    }
}
