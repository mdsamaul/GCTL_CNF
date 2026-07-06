using GCTL.Core.ViewModels.DeleteHistories;
using GCTL.Data.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GCTL.Service.DeleteHistories
{
    public interface IDeleteHistoryService
    {
        Task<bool> LogDeletedRecordsAsync<T>(List<T> entities, DeleteHistoryViewModel model) where T : class;
        Task<List<DeleteHistory>> GetDeletedRecordsByTableAsync(string tableName);
        Task<DeleteHistory> GetDeletedRecordByDHIDAsync(decimal dhid);

        //reference check
        //Task<List<string>> CheckReferenceBeforeDeleteAsync(string tableName, string keyField, object keyValue);
        //Task<DependencyCheckResult> CheckDependenciesAsync(string tableName, string keyField, List<object> keyValues, List<string> alternateKeyColumns = null);


        void InvalidateCache();
        Task<DependencyCheckResult> CheckDependenciesAsync(
        string masterTableName,
        string keyField,
        List<string> keyValues,
        List<string> alternateKeyColumns = null);
    }

}
