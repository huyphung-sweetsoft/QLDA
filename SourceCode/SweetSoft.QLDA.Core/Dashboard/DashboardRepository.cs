using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using SubSonic;
using SweetSoft.QLDA.DataAccess;

namespace SweetSoft.QLDA.Core.Dashboard
{
    /// <summary>
    /// Centralizes all dashboard reads so soft-delete, project and date filters
    /// are applied consistently at the database layer.
    /// </summary>
    public class DashboardRepository
    {
        public DashboardFinancialSummary GetFinancialSummary(DashboardFilter filter)
        {
            StringBuilder sql = new StringBuilder(
                "WITH FilteredProjects AS (" +
                " SELECT p.IdDuAn, p.IdHopDongThucHien" +
                " FROM TblDuAn p WHERE p.DaXoa = 0");
            Dictionary<string, object> parameters = new Dictionary<string, object>();

            AppendProjectFilter(sql, parameters, "p.IdDuAn", filter);
            AppendProjectDateFilter(sql, parameters, "p", filter);

            sql.Append(
                "), FilteredContracts AS (" +
                " SELECT DISTINCT h.IdHopDongThucHien, h.GiaTriHopDong" +
                " FROM TblHopDongThucHien h" +
                " INNER JOIN FilteredProjects p" +
                " ON p.IdHopDongThucHien = h.IdHopDongThucHien" +
                " WHERE h.DaXoa = 0" +
                ")" +
                " SELECT" +
                " TotalContractValue = COALESCE((" +
                " SELECT SUM(COALESCE(h.GiaTriHopDong, 0))" +
                " FROM FilteredContracts h), 0)," +
                " ActualCost = COALESCE((" +
                " SELECT SUM(c.SoTien)" +
                " FROM TblChiPhi c" +
                " INNER JOIN FilteredProjects p ON p.IdDuAn = c.IdDuAn" +
                " WHERE c.DaXoa = 0");

            if (HasDateRange(filter))
            {
                AddDateRangeParameters(parameters, filter);
                sql.Append(
                    " AND c.NgayPhatSinh >= @FromDate" +
                    " AND c.NgayPhatSinh < @ToDateExclusive");
            }

            sql.Append(
                "), 0)," +
                " ReceivedPayment = COALESCE((" +
                " SELECT SUM(pmt.SoTien)" +
                " FROM TblThanhToan pmt" +
                " INNER JOIN FilteredProjects p ON p.IdDuAn = pmt.IdDuAn" +
                " WHERE pmt.DaXoa = 0");

            if (HasDateRange(filter))
            {
                sql.Append(
                    " AND pmt.NgayThanhToanThucTe IS NOT NULL" +
                    " AND pmt.NgayThanhToanThucTe >= @FromDate" +
                    " AND pmt.NgayThanhToanThucTe < @ToDateExclusive");
            }

            sql.Append("), 0)");

            using (IDataReader reader = ExecuteReader(sql.ToString(), parameters))
            {
                if (reader != null && reader.Read())
                {
                    return new DashboardFinancialSummary
                    {
                        TotalContractValue = Convert.ToDecimal(
                            reader["TotalContractValue"]),
                        ActualCost = Convert.ToDecimal(reader["ActualCost"]),
                        ReceivedPayment = Convert.ToDecimal(
                            reader["ReceivedPayment"])
                    };
                }
            }

            return new DashboardFinancialSummary();
        }

        public List<TblDuAn> GetProjects(DashboardFilter filter, bool applyDateRange = true)
        {
            StringBuilder sql = new StringBuilder(
                "SELECT p.* FROM TblDuAn p WHERE p.DaXoa = 0");
            Dictionary<string, object> parameters = new Dictionary<string, object>();

            AppendProjectFilter(sql, parameters, "p.IdDuAn", filter);
            if (applyDateRange)
            {
                AppendProjectDateFilter(sql, parameters, "p", filter);
            }

            sql.Append(" ORDER BY p.MaDuAn");
            return ExecuteList<TblDuAn>(sql, parameters);
        }

        public List<TblCongViec> GetTasks(DashboardFilter filter, bool applyDateRange)
        {
            StringBuilder sql = new StringBuilder(
                "SELECT t.* FROM TblCongViec t WHERE t.DaXoa = 0");
            Dictionary<string, object> parameters = new Dictionary<string, object>();

            AppendProjectFilter(sql, parameters, "t.IdDuAn", filter);
            if (applyDateRange && HasDateRange(filter))
            {
                AddDateRangeParameters(parameters, filter);
                sql.Append(
                    " AND t.NgayBatDau IS NOT NULL" +
                    " AND t.NgayBatDau < @ToDateExclusive" +
                    " AND (t.NgayHoanThanhThucTe IS NULL OR t.NgayHoanThanhThucTe >= @FromDate)");
            }

            return ExecuteList<TblCongViec>(sql, parameters);
        }

        public List<TblRuiRoDuAn> GetRisks(DashboardFilter filter)
        {
            StringBuilder sql = new StringBuilder(
                "SELECT" +
                " r.IdRuiRo_DuAn AS IdRuiRoDuAn," +
                " r.IdDuAn, r.IdNhanVienXuLy, r.TenRuiRo," +
                " r.XacSuatXayRa, r.MucDoAnhHuong, r.DiemRuiRo," +
                " r.KeHoachPhongNgua, r.KeHoachUngPho, r.DaXoa," +
                " r.NguoiTao, r.NgayTao, r.NguoiCapNhat, r.NgayCapNhat" +
                " FROM TblRuiRo_DuAn r WHERE r.DaXoa = 0");
            Dictionary<string, object> parameters = new Dictionary<string, object>();

            AppendProjectFilter(sql, parameters, "r.IdDuAn", filter);
            if (HasDateRange(filter))
            {
                AddDateRangeParameters(parameters, filter);
                sql.Append(" AND r.NgayTao < @ToDateExclusive");
            }

            return ExecuteList<TblRuiRoDuAn>(sql, parameters);
        }

        public List<TblVanDe> GetIssues(DashboardFilter filter)
        {
            StringBuilder sql = new StringBuilder(
                "SELECT i.* FROM TblVanDe i WHERE i.DaXoa = 0");
            Dictionary<string, object> parameters = new Dictionary<string, object>();

            AppendProjectFilter(sql, parameters, "i.IdDuAn", filter);
            if (HasDateRange(filter))
            {
                AddDateRangeParameters(parameters, filter);
                sql.Append(" AND i.NgayTao < @ToDateExclusive");
            }

            return ExecuteList<TblVanDe>(sql, parameters);
        }

        public List<TblChiPhi> GetCosts(DashboardFilter filter)
        {
            StringBuilder sql = new StringBuilder(
                "SELECT c.* FROM TblChiPhi c WHERE c.DaXoa = 0");
            Dictionary<string, object> parameters = new Dictionary<string, object>();

            AppendProjectFilter(sql, parameters, "c.IdDuAn", filter);
            if (HasDateRange(filter))
            {
                AddDateRangeParameters(parameters, filter);
                sql.Append(
                    " AND c.NgayPhatSinh >= @FromDate" +
                    " AND c.NgayPhatSinh < @ToDateExclusive");
            }

            return ExecuteList<TblChiPhi>(sql, parameters);
        }

        public List<TblLichHop> GetMeetings(DashboardFilter filter)
        {
            StringBuilder sql = new StringBuilder(
                "SELECT m.* FROM TblLichHop m WHERE m.DaXoa = 0");
            Dictionary<string, object> parameters = new Dictionary<string, object>();

            AppendProjectFilter(sql, parameters, "m.IdDuAn", filter);
            if (HasDateRange(filter))
            {
                AddDateRangeParameters(parameters, filter);
                sql.Append(
                    " AND m.ThoiGianBatDau >= @FromDate" +
                    " AND m.ThoiGianBatDau < @ToDateExclusive");
            }

            return ExecuteList<TblLichHop>(sql, parameters);
        }

        public List<TblThanhToan> GetPayments(DashboardFilter filter)
        {
            StringBuilder sql = new StringBuilder(
                "SELECT p.* FROM TblThanhToan p WHERE p.DaXoa = 0");
            Dictionary<string, object> parameters = new Dictionary<string, object>();

            AppendProjectFilter(sql, parameters, "p.IdDuAn", filter);
            if (HasDateRange(filter))
            {
                AddDateRangeParameters(parameters, filter);
                sql.Append(
                    " AND p.NgayThanhToanThucTe IS NOT NULL" +
                    " AND p.NgayThanhToanThucTe >= @FromDate" +
                    " AND p.NgayThanhToanThucTe < @ToDateExclusive");
            }

            return ExecuteList<TblThanhToan>(sql, parameters);
        }

        public List<TblHopDongThucHien> GetContracts(DashboardFilter filter)
        {
            StringBuilder sql = new StringBuilder(
                "SELECT DISTINCT h.*" +
                " FROM TblHopDongThucHien h" +
                " INNER JOIN TblDuAn p ON p.IdHopDongThucHien = h.IdHopDongThucHien" +
                " WHERE h.DaXoa = 0 AND p.DaXoa = 0");
            Dictionary<string, object> parameters = new Dictionary<string, object>();

            AppendProjectFilter(sql, parameters, "p.IdDuAn", filter);
            AppendProjectDateFilter(sql, parameters, "p", filter);

            return ExecuteList<TblHopDongThucHien>(sql, parameters);
        }

        public List<AspnetUser> GetEmployees()
        {
            const string sql =
                "SELECT u.* FROM aspnet_Users u" +
                " WHERE u.LaNhanVien = 1 AND u.IsDeleted = 0";
            return ExecuteList<AspnetUser>(
                sql,
                new Dictionary<string, object>());
        }

        public List<TblCongViecNhanVien> GetTaskAssignments(
            DashboardFilter filter)
        {
            StringBuilder sql = new StringBuilder(
                "SELECT DISTINCT a.*" +
                " FROM TblCongViec_NhanVien a" +
                " INNER JOIN TblCongViec t ON t.IdCongViec = a.IdCongViec" +
                " INNER JOIN TblDuAn p ON p.IdDuAn = t.IdDuAn" +
                " WHERE t.DaXoa = 0 AND p.DaXoa = 0");
            Dictionary<string, object> parameters =
                new Dictionary<string, object>();

            AppendProjectFilter(sql, parameters, "p.IdDuAn", filter);
            return ExecuteList<TblCongViecNhanVien>(sql, parameters);
        }

        public List<TblPhongBan> GetDepartments()
        {
            const string sql =
                "SELECT d.* FROM TblPhongBan d" +
                " WHERE d.DaXoa = 0 AND d.KichHoat = 1" +
                " ORDER BY d.ThuTuHienThi, d.TenPhongBan";

            return ExecuteList<TblPhongBan>(
                sql,
                new Dictionary<string, object>());
        }

        public List<TblChucDanh> GetJobTitles()
        {
            const string sql =
                "SELECT j.* FROM TblChucDanh j" +
                " WHERE j.DaXoa = 0 AND j.KichHoat = 1" +
                " ORDER BY j.ThuTuHienThi, j.TenChucDanh";

            return ExecuteList<TblChucDanh>(
                sql,
                new Dictionary<string, object>());
        }

        public List<TblDoUuTien> GetPriorities()
        {
            const string sql =
                "SELECT p.* FROM TblDoUuTien p ORDER BY p.DiemUuTien DESC";

            return ExecuteList<TblDoUuTien>(
                sql,
                new Dictionary<string, object>());
        }

        public List<TblCauHinhTuanLamViec> GetWorkWeekConfigurations()
        {
            const string sql =
                "WITH RankedConfigurations AS (" +
                " SELECT c.*, ROW_NUMBER() OVER (" +
                " PARTITION BY c.NgayTrongTuan" +
                " ORDER BY COALESCE(c.NgayCapNhat, c.NgayTao) DESC," +
                " c.IdCauHinh DESC) AS RowNumber" +
                " FROM TblCauHinhTuanLamViec c" +
                ")" +
                " SELECT IdCauHinh, NgayTrongTuan, LaNgayLamViec," +
                " GioBatDauSang, GioKetThucSang," +
                " GioBatDauChieu, GioKetThucChieu," +
                " NguoiTao, NgayTao, NguoiCapNhat, NgayCapNhat" +
                " FROM RankedConfigurations" +
                " WHERE RowNumber = 1" +
                " ORDER BY NgayTrongTuan";

            return ExecuteList<TblCauHinhTuanLamViec>(
                sql,
                new Dictionary<string, object>());
        }

        public List<TblLichNgoaiLe> GetCalendarExceptions(
            DateTime fromDate,
            DateTime toDate)
        {
            const string sql =
                "WITH RankedExceptions AS (" +
                " SELECT e.*, ROW_NUMBER() OVER (" +
                " PARTITION BY e.TenNgoaiLe, e.NgayBatDau, e.NgayKetThuc" +
                " ORDER BY COALESCE(e.NgayCapNhat, e.NgayTao) DESC," +
                " e.IdNgoaiLe DESC) AS RowNumber" +
                " FROM TblLichNgoaiLe e" +
                " WHERE e.DaXoa = 0" +
                " AND e.NgayBatDau <= @ToDate" +
                " AND e.NgayKetThuc >= @FromDate" +
                ")" +
                " SELECT IdNgoaiLe, TenNgoaiLe, NgayBatDau, NgayKetThuc," +
                " LaNgayLamViec, GioBatDauSang, GioKetThucSang," +
                " GioBatDauChieu, GioKetThucChieu, MoTa, DaXoa," +
                " NguoiTao, NgayTao, NguoiCapNhat, NgayCapNhat" +
                " FROM RankedExceptions" +
                " WHERE RowNumber = 1" +
                " ORDER BY NgayBatDau, NgayKetThuc";

            Dictionary<string, object> parameters =
                new Dictionary<string, object>
                {
                    { "@FromDate", fromDate.Date },
                    { "@ToDate", toDate.Date }
                };

            return ExecuteList<TblLichNgoaiLe>(sql, parameters);
        }

        public List<TblDuAn> GetCompletedProjects(DashboardCostFilter filter)
        {
            StringBuilder sql = new StringBuilder(
                "SELECT p.* FROM TblDuAn p" +
                " WHERE p.DaXoa = 0" +
                " AND p.NgayHoanThanhThucTe IS NOT NULL");
            Dictionary<string, object> parameters =
                new Dictionary<string, object>();

            if (filter != null && filter.ProjectId.HasValue)
            {
                sql.Append(" AND p.IdDuAn = @ProjectId");
                parameters.Add("@ProjectId", filter.ProjectId.Value);
            }

            if (filter != null && filter.CompletedFrom.HasValue)
            {
                sql.Append(" AND p.NgayHoanThanhThucTe >= @CompletedFrom");
                parameters.Add(
                    "@CompletedFrom",
                    filter.CompletedFrom.Value.Date);
            }

            if (filter != null && filter.CompletedTo.HasValue)
            {
                sql.Append(" AND p.NgayHoanThanhThucTe < @CompletedToExclusive");
                parameters.Add(
                    "@CompletedToExclusive",
                    filter.CompletedTo.Value.Date.AddDays(1));
            }

            sql.Append(" ORDER BY p.NgayHoanThanhThucTe DESC, p.MaDuAn");
            return ExecuteList<TblDuAn>(sql, parameters);
        }

        public List<TblChiPhi> GetCostsForProjects(
            IEnumerable<Guid> projectIds)
        {
            Dictionary<string, object> parameters;
            string idList = BuildGuidParameterList(
                projectIds,
                "@CostProjectId",
                out parameters);

            if (string.IsNullOrEmpty(idList))
            {
                return new List<TblChiPhi>();
            }

            string sql =
                "SELECT c.* FROM TblChiPhi c" +
                " WHERE c.DaXoa = 0" +
                " AND c.IdDuAn IN (" + idList + ")" +
                " ORDER BY c.NgayPhatSinh, c.MaKhoanChi";

            return ExecuteList<TblChiPhi>(sql, parameters);
        }

        public List<TblThanhToan> GetPaymentsForProjects(
            IEnumerable<Guid> projectIds)
        {
            Dictionary<string, object> parameters;
            string idList = BuildGuidParameterList(
                projectIds,
                "@PaymentProjectId",
                out parameters);

            if (string.IsNullOrEmpty(idList))
            {
                return new List<TblThanhToan>();
            }

            string sql =
                "SELECT p.* FROM TblThanhToan p" +
                " WHERE p.DaXoa = 0" +
                " AND p.IdDuAn IN (" + idList + ")" +
                " ORDER BY p.HanThanhToan, p.MaDotThanhToan";

            return ExecuteList<TblThanhToan>(sql, parameters);
        }

        public List<TblHopDongThucHien> GetContractsForProjects(
            IEnumerable<Guid> projectIds)
        {
            Dictionary<string, object> parameters;
            string idList = BuildGuidParameterList(
                projectIds,
                "@ContractProjectId",
                out parameters);

            if (string.IsNullOrEmpty(idList))
            {
                return new List<TblHopDongThucHien>();
            }

            string sql =
                "SELECT DISTINCT h.*" +
                " FROM TblHopDongThucHien h" +
                " INNER JOIN TblDuAn p" +
                " ON p.IdHopDongThucHien = h.IdHopDongThucHien" +
                " WHERE h.DaXoa = 0" +
                " AND p.IdDuAn IN (" + idList + ")" +
                " ORDER BY h.SoHopDong";

            return ExecuteList<TblHopDongThucHien>(sql, parameters);
        }

        public List<TblThanhVienDuAn> GetProjectMembers(DashboardFilter filter)
        {
            StringBuilder sql = new StringBuilder(
                "SELECT DISTINCT m.*" +
                " FROM TblThanhVienDuAn m" +
                " INNER JOIN TblDuAn p ON p.IdDuAn = m.IdDuAn" +
                " WHERE m.DaXoa = 0 AND p.DaXoa = 0");
            Dictionary<string, object> parameters = new Dictionary<string, object>();

            AppendProjectFilter(sql, parameters, "p.IdDuAn", filter);
            AppendProjectDateFilter(sql, parameters, "p", filter);

            return ExecuteList<TblThanhVienDuAn>(sql, parameters);
        }

        public List<TblDuAn> GetProjectsForFilter()
        {
            return GetProjects(null, false);
        }

        public AspnetUser GetEmployeeByUserId(Guid userId)
        {
            const string sql =
                "SELECT TOP 1 u.*" +
                " FROM aspnet_Users u" +
                " WHERE u.UserId = @UserId" +
                " AND u.LaNhanVien = 1 AND u.IsDeleted = 0";

            Dictionary<string, object> parameters =
                new Dictionary<string, object> { { "@UserId", userId } };

            return ExecuteList<AspnetUser>(sql, parameters)
                .FirstOrDefault();
        }

        public List<TblDuAn> GetEmployeeProjects(
            Guid employeeId,
            DashboardFilter filter,
            bool applyDateRange)
        {
            StringBuilder sql = new StringBuilder(
                "SELECT DISTINCT p.*" +
                " FROM TblDuAn p" +
                " INNER JOIN TblThanhVienDuAn m ON m.IdDuAn = p.IdDuAn" +
                " WHERE p.DaXoa = 0 AND m.DaXoa = 0" +
                " AND m.IdNhanVien = @EmployeeId");
            Dictionary<string, object> parameters =
                new Dictionary<string, object> { { "@EmployeeId", employeeId } };

            AppendProjectFilter(sql, parameters, "p.IdDuAn", filter);
            if (applyDateRange)
            {
                AppendProjectDateFilter(sql, parameters, "p", filter);
            }

            sql.Append(" ORDER BY p.MaDuAn");
            return ExecuteList<TblDuAn>(sql, parameters);
        }

        public List<TblCongViec> GetEmployeeTasks(
            Guid employeeId,
            DashboardFilter filter,
            bool applyDateRange)
        {
            StringBuilder sql = new StringBuilder(
                "SELECT DISTINCT t.*" +
                " FROM TblCongViec t" +
                " INNER JOIN TblCongViec_NhanVien a ON a.IdCongViec = t.IdCongViec" +
                " WHERE t.DaXoa = 0 AND a.IdNhanVien = @EmployeeId");
            Dictionary<string, object> parameters =
                new Dictionary<string, object> { { "@EmployeeId", employeeId } };

            AppendProjectFilter(sql, parameters, "t.IdDuAn", filter);
            if (applyDateRange && HasDateRange(filter))
            {
                AddDateRangeParameters(parameters, filter);
                sql.Append(
                    " AND t.NgayBatDau IS NOT NULL" +
                    " AND t.NgayBatDau < @ToDateExclusive" +
                    " AND (t.NgayHoanThanhThucTe IS NULL OR t.NgayHoanThanhThucTe >= @FromDate)");
            }

            return ExecuteList<TblCongViec>(sql, parameters);
        }

        public List<TblCongViec> GetTasksForProjects(IEnumerable<Guid> projectIds)
        {
            List<Guid> ids = projectIds == null
                ? new List<Guid>()
                : projectIds.Where(x => x != Guid.Empty).Distinct().ToList();

            if (ids.Count == 0)
            {
                return new List<TblCongViec>();
            }

            Dictionary<string, object> parameters = new Dictionary<string, object>();
            List<string> names = new List<string>();

            for (int index = 0; index < ids.Count; index++)
            {
                string name = "@ProjectId" + index;
                names.Add(name);
                parameters.Add(name, ids[index]);
            }

            string sql =
                "SELECT t.* FROM TblCongViec t" +
                " WHERE t.DaXoa = 0" +
                " AND t.IdDuAn IN (" + string.Join(",", names) + ")";

            return ExecuteList<TblCongViec>(sql, parameters);
        }

        public List<TblLichHop> GetUpcomingMeetings(
            Guid employeeId,
            DashboardFilter filter,
            DateTime fromDate,
            int take)
        {
            StringBuilder sql = new StringBuilder(
                "SELECT DISTINCT TOP (@Take) m.*" +
                " FROM TblLichHop m" +
                " INNER JOIN TblDuAn p ON p.IdDuAn = m.IdDuAn" +
                " INNER JOIN TblThanhVienDuAn tv ON tv.IdDuAn = p.IdDuAn" +
                " WHERE m.DaXoa = 0 AND p.DaXoa = 0 AND tv.DaXoa = 0" +
                " AND tv.IdNhanVien = @EmployeeId" +
                " AND m.ThoiGianBatDau >= @FromDate");
            Dictionary<string, object> parameters = new Dictionary<string, object>
            {
                { "@Take", take },
                { "@EmployeeId", employeeId },
                { "@FromDate", fromDate }
            };

            AppendProjectFilter(sql, parameters, "p.IdDuAn", filter);
            sql.Append(" ORDER BY m.ThoiGianBatDau");

            return ExecuteList<TblLichHop>(sql, parameters);
        }

        private static List<T> ExecuteList<T>(
            StringBuilder sql,
            Dictionary<string, object> parameters)
            where T : IActiveRecord, new()
        {
            return ExecuteList<T>(sql.ToString(), parameters);
        }

        private static List<T> ExecuteList<T>(
            string sql,
            IDictionary<string, object> parameters)
            where T : IActiveRecord, new()
        {
            List<T> result = new List<T>();

            using (IDataReader reader = DataService.GetReader(
                CreateCommand(sql, parameters)))
            {
                while (reader.Read())
                {
                    T entity = new T();
                    entity.Load(reader);
                    result.Add(entity);
                }
            }

            return result;
        }

        private static IDataReader ExecuteReader(
            string sql,
            IDictionary<string, object> parameters)
        {
            return DataService.GetReader(CreateCommand(sql, parameters));
        }

        /// <summary>
        /// Creates a SubSonic command with explicitly typed parameters.
        /// InlineQuery's positional overload treats all values as ANSI strings,
        /// which makes culture-formatted DateTime values fail in SQL Server.
        /// </summary>
        private static QueryCommand CreateCommand(
            string sql,
            IDictionary<string, object> parameters)
        {
            System.Text.RegularExpressions.MatchCollection matches =
                System.Text.RegularExpressions.Regex.Matches(
                    sql + " ",
                    @"@\w*|:\w*");
            QueryCommand command = new InlineQuery().GetCommand(sql);
            HashSet<string> addedParameters =
                new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            for (int index = 0; index < matches.Count; index++)
            {
                string parameterName = matches[index].Value;
                if (!addedParameters.Add(parameterName))
                {
                    continue;
                }

                object value;

                if (parameters == null
                    || !parameters.TryGetValue(parameterName, out value))
                {
                    throw new ArgumentException(
                        "Missing SQL parameter value: " + parameterName,
                        "parameters");
                }

                command.Parameters.Add(
                    parameterName,
                    value ?? DBNull.Value,
                    GetDbType(value));
            }

            return command;
        }

        private static DbType GetDbType(object value)
        {
            if (value == null || value == DBNull.Value)
            {
                return DbType.String;
            }

            if (value is Guid)
            {
                return DbType.Guid;
            }

            if (value is byte[])
            {
                return DbType.Binary;
            }

            if (value is TimeSpan)
            {
                return DbType.Time;
            }

            switch (Type.GetTypeCode(value.GetType()))
            {
                case TypeCode.Boolean:
                    return DbType.Boolean;
                case TypeCode.Byte:
                    return DbType.Byte;
                case TypeCode.Int16:
                    return DbType.Int16;
                case TypeCode.Int32:
                    return DbType.Int32;
                case TypeCode.Int64:
                    return DbType.Int64;
                case TypeCode.Single:
                    return DbType.Single;
                case TypeCode.Double:
                    return DbType.Double;
                case TypeCode.Decimal:
                    return DbType.Decimal;
                case TypeCode.DateTime:
                    return DbType.DateTime;
                default:
                    return DbType.String;
            }
        }

        private static string BuildGuidParameterList(
            IEnumerable<Guid> values,
            string parameterPrefix,
            out Dictionary<string, object> parameters)
        {
            parameters = new Dictionary<string, object>();
            List<Guid> ids = values == null
                ? new List<Guid>()
                : values
                    .Where(x => x != Guid.Empty)
                    .Distinct()
                    .ToList();
            List<string> names = new List<string>();

            for (int index = 0; index < ids.Count; index++)
            {
                string name = parameterPrefix + index;
                names.Add(name);
                parameters.Add(name, ids[index]);
            }

            return string.Join(",", names);
        }

        private static void AppendProjectFilter(
            StringBuilder sql,
            IDictionary<string, object> parameters,
            string projectColumn,
            DashboardFilter filter)
        {
            if (filter == null || !filter.ProjectId.HasValue)
            {
                return;
            }

            sql.Append(" AND " + projectColumn + " = @ProjectId");
            if (!parameters.ContainsKey("@ProjectId"))
            {
                parameters.Add("@ProjectId", filter.ProjectId.Value);
            }
        }

        private static void AppendProjectDateFilter(
            StringBuilder sql,
            IDictionary<string, object> parameters,
            string projectAlias,
            DashboardFilter filter)
        {
            if (!HasDateRange(filter))
            {
                return;
            }

            AddDateRangeParameters(parameters, filter);
            sql.Append(
                " AND " + projectAlias + ".NgayBatDau < @ToDateExclusive" +
                " AND (" + projectAlias + ".NgayHoanThanhThucTe IS NULL" +
                " OR " + projectAlias + ".NgayHoanThanhThucTe >= @FromDate)");
        }

        private static void AddDateRangeParameters(
            IDictionary<string, object> parameters,
            DashboardFilter filter)
        {
            if (!parameters.ContainsKey("@FromDate"))
            {
                parameters.Add("@FromDate", filter.FromDate.Date);
            }

            if (!parameters.ContainsKey("@ToDateExclusive"))
            {
                parameters.Add("@ToDateExclusive", filter.ToDate.Date.AddDays(1));
            }
        }

        private static bool HasDateRange(DashboardFilter filter)
        {
            return filter != null
                && filter.FromDate > DateTime.MinValue
                && filter.ToDate > DateTime.MinValue
                && filter.ToDate < DateTime.MaxValue.Date
                && filter.FromDate.Date <= filter.ToDate.Date;
        }
    }

    public class DashboardFinancialSummary
    {
        public decimal TotalContractValue { get; set; }

        public decimal ActualCost { get; set; }

        public decimal ReceivedPayment { get; set; }
    }
}
