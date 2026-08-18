using SubSonic;
using SweetSoft.QLDA.Core.FileManager;
using SweetSoft.QLDA.Core.SysManager;
using SweetSoft.QLDA.Core.Utils;
using SweetSoft.QLDA.DataAccess;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SweetSoft.QLDA.Core.Respositories
{
    public class FileRepository : BaseRepository<TblUploadFile>
    {
        public FileRepository(AuditManager auditManager) : base(auditManager) { }

        public override DataTable SearchPaging(string searchTerm, string orderBy, int pageNumber, int pageSize, out int totalRecord)
        {
            totalRecord = 0;
            string sql = $@"
                DECLARE @startRow INT = {pageNumber};
                DECLARE @endRow INT = {pageSize};
                DECLARE @singleKeyWord NVARCHAR(150) = N'%{InlineQueryHelpers.SQLEncode(searchTerm)}%';
                select * from (
					select ROW_NUMBER() OVER (ORDER BY {orderBy}) AS RowNum, T.* from (
						select f.*
                        , COUNT(1) OVER() AS total_records
                        from TblUploadFile f
                        where IsDeleted = 0
                        AND (@singleKeyWord = N'%%'
                        or f.Name LIKE @singleKeyWord
                        or f.FileUrl LIKE @singleKeyWord)
					) as T
                ) T1 WHERE RowNum >= @startRow AND RowNum <= @endRow
            ";
            IDataReader iDataReader = new InlineQuery().ExecuteReader(sql);
            if (iDataReader == null)
                return null;

            DataTable dt = new DataTable();
            dt.Load(iDataReader);
            InlineQueryHelpers.GetTotal(ref dt, out totalRecord);
            return dt;
        }

        public DataTable SearchPaging(Guid ownerId, string searchTerm, string orderBy, int pageNumber, int pageSize, out int totalRecord)
        {
            totalRecord = 0;
            string sql = $@"
                DECLARE @startRow INT = {pageNumber};
                DECLARE @endRow INT = {pageSize};
                DECLARE @ownerId VARCHAR(36) = '{ownerId}';
                DECLARE @singleKeyWord NVARCHAR(150) = N'%{InlineQueryHelpers.SQLEncode(searchTerm)}%';
                select * from (
					select ROW_NUMBER() OVER (ORDER BY {orderBy}) AS RowNum, T.* from (
						select f.*
                        , CASE WHEN f.FileType = 'External' THEN 
                            (
                                SELECT FullName from TblParticipants where Id = f.OwnerId
                            )
                          ELSE
                            (
                                SELECT DisplayName from aspnet_Users where UserId = f.OwnerId
                            )
                          END AS OwnerName
                        , COUNT(1) OVER() AS total_records
                        from TblUploadFile f
                        where IsDeleted = 0 
                        AND (@ownerId = '00000000-0000-0000-0000-000000000000' or f.OwnerId = @ownerId)
                        AND (@singleKeyWord = N'%%'
                        or f.Name LIKE @singleKeyWord
                        or f.FileUrl LIKE @singleKeyWord
                        or f.OriginalFileName LIKE @singleKeyWord)
					) as T
                ) T1 WHERE RowNum >= @startRow AND RowNum <= @endRow
            ";
            IDataReader iDataReader = new InlineQuery().ExecuteReader(sql);
            if (iDataReader == null)
                return null;

            DataTable dt = new DataTable();
            dt.Load(iDataReader);
            InlineQueryHelpers.GetTotal(ref dt, out totalRecord);
            return dt;
        }

        public override DataTable SearchPaging(Dictionary<string, object> parameters, string orderBy, int pageNumber, int pageSize, out int totalRecord)
        {
            totalRecord = 0;
            string sql = $@"
                DECLARE @startRow INT = {pageNumber};
                DECLARE @endRow INT = {pageSize};
                DECLARE @name NVARCHAR(150) = N'%{InlineQueryHelpers.SQLEncode(parameters[TblUploadFile.Columns.Name].ToString())}%';
                DECLARE @fileUrl NVARCHAR(250) = N'%{InlineQueryHelpers.SQLEncode(parameters[TblUploadFile.Columns.FileUrl].ToString())}%';
                DECLARE @refType VARCHAR(150) = '{InlineQueryHelpers.SQLEncode(parameters[TblUploadFile.Columns.RefType].ToString())}';
                DECLARE @fileType VARCHAR(50) = '{InlineQueryHelpers.SQLEncode(parameters[TblUploadFile.Columns.FileType].ToString())}';
                select * from (
					select ROW_NUMBER() OVER (ORDER BY {orderBy}) AS RowNum, T.* from (
						select f.*
                        , COUNT(1) OVER() AS total_records
                        from TblUploadFile f
                        where IsDeleted = 0 
                        AND (@name = N'%%' or Name LIKE @name)
                        AND (@fileUrl = N'%%' or FileUrl LIKE @fileUrl)
                        AND (@refType = '' or RefType = @refType)
                        AND (@fileType = '' or FileType = @fileType)
                          
					) as T
                ) T1 WHERE RowNum >= @startRow AND RowNum <= @endRow";
            IDataReader iDataReader = new InlineQuery().ExecuteReader(sql);
            if (iDataReader == null)
                return null;

            DataTable dt = new DataTable();
            dt.Load(iDataReader);
            InlineQueryHelpers.GetTotal(ref dt, out totalRecord);
            return dt;
        }
        public override TblUploadFile GetById(Guid id)
        {
            return new Select()
                .From(TblUploadFile.Schema)
                .Where(TblUploadFile.IdColumn).IsEqualTo(id)
                .And(TblUploadFile.IsDeletedColumn).IsEqualTo(0)
                .ExecuteSingle<TblUploadFile>();
        }

        public string GetUploadFileNameById(Guid Id)
        {
            return new Select(TblUploadFile.NameColumn)
                .From(TblUploadFile.Schema)
                .Where(TblUploadFile.IdColumn).IsEqualTo(Id)
                .And(TblUploadFile.IsDeletedColumn).IsEqualTo(0)
                .ExecuteScalar<string>();
        }
        public List<TblUploadFile> GetActivatedList()
        {
            return new Select()
                .From(TblUploadFile.Schema)
                .Where(TblUploadFile.IsDeletedColumn).IsEqualTo(0)
                .OrderAsc(TblUploadFile.Columns.Name)
                .ExecuteTypedList<TblUploadFile>();
        }
        public string GetNameById(Guid id)
        {
            return new Select(TblUploadFile.Columns.Name)
                .From(TblUploadFile.Schema)
                .Where(TblUploadFile.IdColumn).IsEqualTo(id)
                .And(TblUploadFile.IsDeletedColumn).IsEqualTo(0)
                .ExecuteScalar<string>() ?? string.Empty;
        }
        public List<TblUploadFile> GetListFileByRefId(Guid refId, FileUploadTypes refType)
        {
            Select select = new Select();
            select.From(TblUploadFile.Schema);
            select.Where(TblUploadFile.RefIdColumn).IsEqualTo(refId);
            select.And(TblUploadFile.RefTypeColumn).IsEqualTo(refType);
            select.And(TblUploadFile.IsDeletedColumn).IsEqualTo(0);
            return select.ExecuteTypedList<TblUploadFile>();
        }
        public TblUploadFile GetFileByParams(Guid refId, FileUploadTypes refType, string fileName)
        {
            Select select = new Select();
            select.From(TblUploadFile.Schema);
            select.Where(TblUploadFile.RefIdColumn).IsEqualTo(refId);
            select.And(TblUploadFile.RefTypeColumn).IsEqualTo(refType);
            select.And(TblUploadFile.NameColumn).IsEqualTo(fileName);
            select.And(TblUploadFile.IsDeletedColumn).IsEqualTo(0);
            return select.ExecuteSingle<TblUploadFile>();
        }

        public TblUploadFile GetListFileByRefIdAndRefType(Guid refId, FileUploadTypes refType)
        {
            Select select = new Select();
            select.Top("1");
            select.From(TblUploadFile.Schema);
            select.Where(TblUploadFile.RefIdColumn).IsEqualTo(refId);
            select.And(TblUploadFile.RefTypeColumn).IsEqualTo(refType);
            select.And(TblUploadFile.IsDeletedColumn).IsEqualTo(0);
            return select.ExecuteSingle<TblUploadFile>();
        }
        public string RemoveFiles(List<Guid> fileIDs)
        {
            string sqlDeletePhoto = $@"DELETE TblUploadFile where Id in ({string.Join(",", fileIDs.Select(t => $"'{t}'").ToArray())});";
            return new InlineQuery().ExecuteScalar<string>(sqlDeletePhoto);
        }
        public string GetFilePaths(List<Guid> fileIDs, FileUploadTypes uploadTypes)
        {
            string sql = $@"select FileUrl +'|' AS [text()] 
                from Tbluploadfile 
                where Id in ({string.Join(",", fileIDs.Select(t => $"'{t}'").ToArray())}) and RefType = '{uploadTypes.ToString()}' FOR XML PATH('')";
            return new InlineQuery().ExecuteScalar<string>(sql);
        }
    }
}
