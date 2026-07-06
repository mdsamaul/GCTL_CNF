using Dapper;
using GCTL.Core.Repository;
using GCTL.Core.ViewModels.DeleteHistories;
using GCTL.Data.Models;

using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using SixLabors.ImageSharp;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace GCTL.Service.DeleteHistories
{
    public class DependencyCheckResult
    {
        public bool CanDelete { get; set; }
        public List<string> DependentTables { get; set; }
        public string Message { get; set; }

        public DependencyCheckResult()
        {
            DependentTables = new List<string>();
        }
    }

    public class DeleteHistoryService : IDeleteHistoryService
    {
        private readonly IGenericRepository<DeleteHistory> deleteHistoryRepository;
        private readonly AppDbContext context;
        //private readonly ICommonService commonService;

        private readonly IMemoryCache _cache;
        private readonly MemoryCacheEntryOptions _cacheOptions = new MemoryCacheEntryOptions { AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(30) };
        private const string CacheKeyPrefix = "ReferentialIntegrityCandidateColumns_";


        public DeleteHistoryService(IGenericRepository<DeleteHistory> deleteHistoryRepository, AppDbContext context, IMemoryCache memoryCache /*, ICommonService commonService*/)
        {
            this.deleteHistoryRepository = deleteHistoryRepository;
            this.context = context;

            _cache = memoryCache;
            //this.commonService = commonService;
        }

        public async Task<bool> LogDeletedRecordsAsync<T>(List<T> entities,  DeleteHistoryViewModel model) where T : class
        {
            if (entities == null || !entities.Any())
                return false;

            try
            {
                var deleteHistoryRecords = new List<DeleteHistory>();
                decimal currentDhid = await GenerateUniqueDHIDAsync();

                foreach (var entity in entities)
                {
                    var deleteHistory = new DeleteHistory
                    {
                        DHID = currentDhid++,
                        TableName = model.tableName,
                        LIP = model.LIP,
                        LMAC = model.LMAC,
                        LDate = DateTime.Now,
                        LUser = model.CreatedBy.ToString(),
                        CompanyCode = model.CompanyCode,
                        UserInfoEmployeeID = model.CreatedBy.ToString()
                    };

                    var properties = typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance);

                    int fieldIndex;
                    for (fieldIndex = 0; fieldIndex < Math.Min(properties.Length, 99); fieldIndex++)
                    {
                        var property = properties[fieldIndex];
                        var value = property.GetValue(entity);
                        var fieldName = $"Field{fieldIndex + 1}";
                        var fieldValue = value?.ToString();

                        var fieldProperty = typeof(DeleteHistory).GetProperty(fieldName);
                        fieldProperty?.SetValue(deleteHistory, fieldValue);
                    }

                    var jsonFieldName = $"Field{fieldIndex + 1}";
                    var jsonFieldProperty = typeof(DeleteHistory).GetProperty(jsonFieldName);
                    var jsonData = new Dictionary<string, object> { { model.tableName, entity } };
                    jsonFieldProperty?.SetValue(deleteHistory, JsonSerializer.Serialize(jsonData));

                    deleteHistoryRecords.Add(deleteHistory);
                }

                await deleteHistoryRepository.AddRangeAsync(deleteHistoryRecords);
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public async Task<List<DeleteHistory>> GetDeletedRecordsByTableAsync(string tableName)
        {
            return await deleteHistoryRepository.All()
                .Where(dh => dh.TableName == tableName)
                .OrderByDescending(dh => dh.AutoId)
                .ToListAsync();
        }

        public async Task<DeleteHistory> GetDeletedRecordByDHIDAsync(decimal dhid)
        {
            return await deleteHistoryRepository.All()
                .FirstOrDefaultAsync(dh => dh.DHID == dhid);
        }

        private async Task<decimal> GenerateUniqueDHIDAsync()
        {
            var maxDhid = await deleteHistoryRepository.All()
                .MaxAsync(dh => (decimal?)dh.DHID);

            return (maxDhid ?? 0) + 1;
        }

        public void InvalidateCache()
        {
            _cache.Remove(CacheKeyPrefix + "Version");
        }

        private int GetCacheVersion()
        {
            if (!_cache.TryGetValue<int>(CacheKeyPrefix + "Version", out var v))
            {
                v = 1;
                _cache.Set(CacheKeyPrefix + "Version", v, _cacheOptions);
            }

            return v;
        }

        private string BuildCacheKey(IEnumerable<string> columnNames)
        {
            var version = GetCacheVersion();
            var joined = string.Join("__", columnNames.Select(s => s.ToLowerInvariant()));
            return CacheKeyPrefix + version + "__" + joined;
        }

        public async Task<DependencyCheckResult> CheckDependenciesAsync(string masterTableName, string keyField, List<string> keyValues, List<string> alternateKeyColumns = null)
        {
            if (string.IsNullOrWhiteSpace(masterTableName)) throw new ArgumentNullException(nameof(masterTableName));
            if (string.IsNullOrWhiteSpace(keyField)) throw new ArgumentNullException(nameof(keyField));
            if (keyValues == null || keyValues.Count == 0) throw new ArgumentNullException(nameof(keyValues));

            var columnNames = new HashSet<string>(StringComparer.OrdinalIgnoreCase) { keyField };
            if (alternateKeyColumns != null)
            {
                foreach (var a in alternateKeyColumns.Where(x => !string.IsNullOrWhiteSpace(x)))
                    columnNames.Add(a);
            }

            var cacheKey = BuildCacheKey(columnNames);
            var candidates = await _cache.GetOrCreateAsync(cacheKey, async entry =>
            {
                entry.SetOptions(_cacheOptions);
                return await DiscoverCandidateColumnsAsync(columnNames, masterTableName);
            });

            var result = new DependencyCheckResult { CanDelete = true, Message = string.Empty };
            var dependentLines = new List<string>();

            if (candidates == null || candidates.Count == 0)
            {
                result.CanDelete = true;
                result.Message = "No candidate reference columns found.";
                return result;
            }

            var stringValues = keyValues.Select(v => v?.ToString() ?? string.Empty).ToList();

            // ✅ FIX 1: Don't use 'using' - let EF Core manage its connection
            var conn = context.Database.GetDbConnection();
            if (conn.State != System.Data.ConnectionState.Open)
                await conn.OpenAsync();
            
            foreach (var cand in candidates)
            {
                if (string.Equals(cand.TableName, masterTableName, StringComparison.OrdinalIgnoreCase))
                    continue;

                // ✅ FIX 2: Use 'using' for cmd to prevent resource leak
                using var cmd = conn.CreateCommand();
                cmd.CommandType = System.Data.CommandType.Text;


                var paramNames = new List<string>();
                for (int i = 0; i < stringValues.Count; i++)
                {
                    var p = cmd.CreateParameter();
                    p.ParameterName = $"@p{i}";
                    p.Value = stringValues[i] ?? (object)DBNull.Value;
                    p.DbType = System.Data.DbType.String;
                    cmd.Parameters.Add(p);
                    paramNames.Add(p.ParameterName);
                }

                var inClause = paramNames.Count > 0 ? string.Join(", ", paramNames) : "NULL";

                string schemaQ = QuoteIdentifier(cand.TableSchema);
                string tableQ = QuoteIdentifier(cand.TableName);
                string columnQ = QuoteIdentifier(cand.ColumnName);
                // ✅ Using your custom STRING_SPLIT function with correct parameter order
                // Note: @Delimiter comes FIRST in your custom function
                cmd.CommandText = $@"
                    SELECT COUNT(1) 
                    FROM {schemaQ}.{tableQ}
                    CROSS APPLY STRING_SPLIT({columnQ},',') AS split
                    WHERE LTRIM(RTRIM(split.Value)) IN ({inClause})
                ";

                //cmd.CommandText = $"SELECT COUNT(1) FROM {schemaQ}.{tableQ} WHERE {columnQ} IN ({inClause})";

                int count = 0;
                try
                {
                    var scalar = await cmd.ExecuteScalarAsync();
                    if (scalar != null && scalar != DBNull.Value)
                        count = Convert.ToInt32(scalar);
                }
                catch (Exception ex)
                {
                    dependentLines.Add($"{cand.TableSchema}.{cand.TableName}({cand.ColumnName}) => ERROR: {ex.Message}");
                    result.CanDelete = false;
                    continue;
                }

                if (count > 0)
                {
                    result.CanDelete = false;
                    var friendlyName = await GetFriendlyNameForTableAsync(conn, cand.TableSchema, cand.TableName);
                    dependentLines.Add($"{friendlyName}");
                }
            }

            result.DependentTables = dependentLines.Distinct().ToList();
            if (!result.CanDelete)
            {
                string joined = string.Join(", ", result.DependentTables);

                if (result.DependentTables.Count > 1)
                {
                    int lastComma = joined.LastIndexOf(", ");
                    if (lastComma >= 0)
                    {
                        joined = joined.Substring(0, lastComma) + " &" + joined.Substring(lastComma + 1);
                    }

                    result.Message = $"Can't be deleted. Please delete first from {joined}  respectively.";
                }
                else
                {
                    result.Message = $"Can't be deletede. Please delete first from {joined}.";
                }
            }
            else
            {
                result.Message = "No references found. Safe to delete.";
            }

            return result;
        }

        private static string QuoteIdentifier(string ident)
        {
            if (string.IsNullOrEmpty(ident)) return ident;
            return "[" + ident.Replace("]", "]]") + "]";
        }

        private async Task<List<CandidateColumn>> DiscoverCandidateColumnsAsync(HashSet<string> columnNames, string masterTableName)
        {
            var list = new List<CandidateColumn>();

            if (columnNames == null || columnNames.Count == 0) return list;

            // ✅ FIX: Don't use 'using' - let EF Core manage its connection
            var conn = context.Database.GetDbConnection();
            if (conn.State != System.Data.ConnectionState.Open)
                await conn.OpenAsync();

            using var cmd = conn.CreateCommand();
            cmd.CommandType = System.Data.CommandType.Text;

            var paramNames = new List<string>();
            int idx = 0;
            foreach (var c in columnNames)
            {
                var p = cmd.CreateParameter();
                p.ParameterName = $"@cn{idx}";
                p.Value = c;
                p.DbType = System.Data.DbType.String;
                cmd.Parameters.Add(p);
                paramNames.Add(p.ParameterName);
                idx++;
            }

            cmd.CommandText = $@"
        SELECT TABLE_SCHEMA, TABLE_NAME, COLUMN_NAME
        FROM INFORMATION_SCHEMA.COLUMNS
        WHERE COLUMN_NAME IN ({string.Join(", ", paramNames)})
          AND (DATA_TYPE = 'nvarchar' OR DATA_TYPE = 'varchar' OR DATA_TYPE = 'char' OR DATA_TYPE = 'nchar')
          AND TABLE_NAME <> @masterTableName
          AND TABLE_SCHEMA NOT IN ('INFORMATION_SCHEMA', 'sys')
        ";

            var masterParam = cmd.CreateParameter();
            masterParam.ParameterName = "@masterTableName";
            masterParam.Value = masterTableName;
            masterParam.DbType = System.Data.DbType.String;
            cmd.Parameters.Add(masterParam);

            using var rdr = await cmd.ExecuteReaderAsync();
            while (await rdr.ReadAsync())
            {
                var schema = rdr.GetString(0);
                var table = rdr.GetString(1);
                var col = rdr.GetString(2);
                list.Add(new CandidateColumn { TableSchema = schema, TableName = table, ColumnName = col });
            }

            return list;
        }

        private async Task<string> GetFriendlyNameForTableAsync(DbConnection conn, string schema, string tableName)
        {

            var prefixes = new[]
            {
                "RMG_Prod_Def", "RMG_Prod_Temp", "RMG_Pro_BTB", "RMG_Prod", "RMG_Def", "RMG_Inv", "RMG",
                "HRM_Payroll", "HRM_ATD", "HRM_Att", "HRM_Def", "HRM_PAY", "HRM",
                "Def_Inv", "Inv_Def", "INV", "Prod_Def", "Sales_Def", "TB_Def", "CA_Def",
                "Core", "Acc", "POS", "TBM", "tbl", "dbo."
            };

            var pretty = tableName;
            foreach (var prefix in prefixes)
            {
                if (!string.IsNullOrEmpty(prefix) && pretty.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
                {
                    pretty = pretty.Substring(prefix.Length);
                    break;
                }
            }

            pretty = pretty.Replace("_", " ").Trim();

            try
            {
                using var cmd = conn.CreateCommand();
                cmd.CommandType = System.Data.CommandType.Text;

                var p = cmd.CreateParameter();
                p.ParameterName = "@tbl";
                p.Value = tableName;
                p.DbType = System.Data.DbType.String;
                cmd.Parameters.Add(p);

                cmd.CommandText = @"
                    SELECT TOP (1) NULLIF(m.Title,'') AS FriendlyName
                    FROM dbo.MenuTab ac
                    WHERE ac.TableName = @tbl
                ";

                var scalar = await cmd.ExecuteScalarAsync();
                if (scalar != null && scalar != DBNull.Value)
                {
                    var found = scalar.ToString();
                    if (!string.IsNullOrWhiteSpace(found))
                        return found;
                }
            }
            catch
            {
                // swallow
            }

            try
            {
                using var cmd2 = conn.CreateCommand();
                cmd2.CommandType = System.Data.CommandType.Text;

                var pExact = cmd2.CreateParameter();
                pExact.ParameterName = "@tblExact";
                pExact.Value = tableName;
                pExact.DbType = System.Data.DbType.String;
                cmd2.Parameters.Add(pExact);

                var pLike = cmd2.CreateParameter();
                pLike.ParameterName = "@tblLike";
                pLike.Value = "%" + tableName + "%";
                pLike.DbType = System.Data.DbType.String;
                cmd2.Parameters.Add(pLike);

                var prettyLike = cmd2.CreateParameter();
                prettyLike.ParameterName = "@prettyLike";
                prettyLike.Value = pretty;
                prettyLike.DbType = System.Data.DbType.String;
                cmd2.Parameters.Add(prettyLike);

                cmd2.CommandText = @"
                    SELECT TOP (1) NULLIF(m.Title,'') AS FriendlyName
                    FROM dbo.MenuTab m
                    WHERE 
                        (LOWER(m.Title) = LOWER(@tblExact))
                    UNION
                    SELECT TOP (1) NULLIF(m.Title,'') AS FriendlyName
                    FROM dbo.MenuTab m
                    WHERE
                        (m.ControllerName IS NOT NULL AND LOWER(m.ControllerName) LIKE LOWER(@tblLike))
                        OR (m.ControllerName IS NOT NULL AND LOWER(m.ControllerName) LIKE LOWER(@prettyLike))
                        OR (m.ViewName IS NOT NULL AND LOWER(m.ViewName) LIKE LOWER(@tblLike))
                        OR (m.Title IS NOT NULL AND LOWER(m.Title) LIKE LOWER(@tblLike))
                    ";

                var scalar2 = await cmd2.ExecuteScalarAsync();
                if (scalar2 != null && scalar2 != DBNull.Value)
                {
                    var found2 = scalar2.ToString();
                    if (!string.IsNullOrWhiteSpace(found2))
                        return found2;
                }
            }
            catch
            {
                // swallow
            }
            pretty = Regex.Replace(pretty, @"(?<!^)([A-Z])", " $1");

            return string.IsNullOrWhiteSpace(pretty) ? tableName : pretty;
        }

        private class CandidateColumn
        {
            public string TableSchema { get; set; }
            public string TableName { get; set; }
            public string ColumnName { get; set; }
        }
    }
}