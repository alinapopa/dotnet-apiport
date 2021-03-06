## 71: SqlBulkCopy uses destination column encoding for strings

### Scope
Edge

### Version Introduced
4.5

### Source Analyzer Status
Planned

### Change Description
When inserting data into a column, SqlBulkCopy uses the encoding of the destination column rather than the default encoding for `VARCHAR` and `CHAR` types. This change eliminates the possibility of data corruption caused by using the default encoding when the destination column does not use the default encoding. In rare cases, an existing application may throw a SqlException exception if the change in encoding produces data that is too big to fit into the destination column.

- [ ] Quirked
- [ ] Build-time break

### Recommended Action
Expect that SqlBulkCopy will no longer corrupt data due to encoding differences. If strings near the destination column's size limit are being copied, it may be necessary to either pre-encode data (to be copied to check that the data will fit in the destination column) or catch SqlExceptions.

### Affected APIs
* `T:System.Data.SqlClient.SqlBulkCopy`
* `M:System.Data.SqlClient.SqlBulkCopy.#ctor(System.Data.SqlClient.SqlConnection)`

### Category
Data

[More information](https://msdn.microsoft.com/en-us/library/hh367887(v=vs.110).aspx#xml)
