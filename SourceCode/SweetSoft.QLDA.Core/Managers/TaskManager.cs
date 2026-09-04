using SubSonic;
using SweetSoft.QLDA.Core.Infrastructure.Interfaces;
using SweetSoft.QLDA.Core.Respositories;
using SweetSoft.QLDA.Core.SysManager;
using SweetSoft.QLDA.DataAccess;
using System;
using System.Collections.Generic;
using System.Data;

namespace SweetSoft.QLDA.Core.Managers
{
    public class TaskManager : BaseManager
    {
        private static readonly Lazy<TaskManager> _instance = new Lazy<TaskManager>(() => new TaskManager());
        public static TaskManager Instance => _instance.Value;
        private readonly TaskRepository _repository;
        private readonly AuditManager _auditManager;

        public TaskManager(IAppContext applicationContext = null) : base(applicationContext)
        {
            _auditManager = new AuditManager(GetClientInfo());
            _repository = new TaskRepository(_auditManager);
        }

        #region 1. Lấy dữ liệu & Danh mục
        public DataTable FetchByIdAndOrderASCMaCV(Guid projectId, string searchValue = null) => _repository.FetchByIdAndOrderASCMaCV(projectId, searchValue);
        public TblCongViec FetchById(Guid taskId) => _repository.FetchById(taskId);
        public DataTable GetChildTasks(Guid projectId, Guid taskId) => _repository.GetChildTasks(projectId, taskId);
        public DataTable GetDependentTasks(Guid projectId, Guid taskId) => _repository.GetDependentTasks(projectId, taskId);
        public DataTable GetPrioritiesTable() => _repository.FetchAllPrioritiesTable();
        public DataTable GetProjectMembers(Guid projectId) => _repository.FetchProjectMembers(projectId);

        public Dictionary<Guid, TblDoUuTien> GetDictPriorities()
        {
            var dict = new Dictionary<Guid, TblDoUuTien>();
            foreach (var p in _repository.FetchAllPrioritiesList())
            {
                dict[p.IdDoUuTien] = p;
            }
            return dict;
        }

        public TblCongViec DeleteTask(TblCongViec task)
        {
            _repository.DeleteTask(task);
            ReindexTaskCodesAfterDelete(task.IdDuAn, task.MaCongViec);
            return task;
        }
        private void ReindexTaskCodesAfterDelete(Guid projectId, string deletedCode)
        {
            if (string.IsNullOrEmpty(deletedCode)) return;
            int lastDot = deletedCode.LastIndexOf('.');
            string prefix = lastDot >= 0 ? deletedCode.Substring(0, lastDot + 1) : "";
            string lastPart = lastDot >= 0 ? deletedCode.Substring(lastDot + 1) : deletedCode;

            if (!int.TryParse(lastPart, out int deletedIndex)) return;
            DataTable dt = FetchByIdAndOrderASCMaCV(projectId);
            if (dt == null || dt.Rows.Count == 0) return;

            foreach (DataRow row in dt.Rows)
            {
                if (row[ColIdCongViec] == DBNull.Value || !Guid.TryParse(row[ColIdCongViec].ToString(), out Guid id))
                    continue;

                string code = row[ColMaCv]?.ToString() ?? "";
                if (string.IsNullOrEmpty(code)) continue;

                if (!string.IsNullOrEmpty(prefix) && !code.StartsWith(prefix))
                    continue;

                string subCode = !string.IsNullOrEmpty(prefix) ? code.Substring(prefix.Length) : code;
                string[] parts = subCode.Split('.');

                if (int.TryParse(parts[0], out int currentIndex) && currentIndex > deletedIndex)
                {
                    int newIndex = currentIndex - 1;
                    string rest = subCode.Contains(".") ? subCode.Substring(parts[0].Length) : "";
                    string newCode = prefix + newIndex + rest;
                    TblCongViec t = FetchById(id);
                    if (t != null)
                    {
                        t.MaCongViec = newCode;
                        t.NgayCapNhat = DateTime.Now;
                        t.Save();
                    }
                }
            }
        }
        public string GetNhanVienByCongViec(Guid idCongViec)
        {
            string sql = $@"
                DECLARE @idCongViec VARCHAR(36) = '{idCongViec}';
    
                SELECT u.DisplayName AS TenNhanVien
                FROM TblCongViec_NhanVien cv
                INNER JOIN [dbo].[aspnet_Users] u ON cv.IdNhanVien = u.UserId
                WHERE cv.IdCongViec = @idCongViec
                   AND (u.IsDeleted = 0 OR u.IsDeleted IS NULL); 
            ";

            IDataReader reader = new InlineQuery().ExecuteReader(sql);

            List<string> danhSachTen = new List<string>();

            if (reader != null)
            {
                while (reader.Read())
                {
                    if (reader["TenNhanVien"] != DBNull.Value)
                    {
                        danhSachTen.Add(reader["TenNhanVien"].ToString());
                    }
                }
                reader.Close();
            }
            return string.Join(", ", danhSachTen);
        }
        #endregion

        #region 2. Nghiệp vụ Cây WBS & Mã Công việc
        public bool CheckOverdue(DataRow row)
        {
            int trangThai = Convert.ToInt32(row[ColTrangThai]);
            DateTime ngayKetThuc = Convert.ToDateTime(row[ColNgayKetThuc]);
            return trangThai != 2 && ngayKetThuc.Date < DateTime.Today;
        }

        public bool CheckPhase(TblCongViec task)
        {
            return task == null || task.MaCongViec.Split('.').Length == 1 || !task.IdCongViecCha.HasValue;
        }

        public bool CheckHasChildTasks(Guid projectId, TblCongViec task)
        {
            if (task == null) return false;
            return (GetChildTasks(projectId, task.IdCongViec)?.Rows.Count ?? 0) > 0;
        }

        public string GenerateNewTaskCode(Guid projectId, Guid? parentId)
        {
            if (parentId.HasValue)
            {
                TblCongViec parent = FetchById(parentId.Value);
                DataTable dtChildren = GetChildTasks(projectId, parentId.Value);
                int nextIndex = (dtChildren?.Rows.Count ?? 0) + 1;
                return $"{parent.MaCongViec}.{nextIndex}";
            }
            else
            {
                DataTable dt = FetchByIdAndOrderASCMaCV(projectId);
                int countLevel1 = 0;
                if (dt != null)
                {
                    foreach (DataRow r in dt.Rows)
                    {
                        if (r[ColIdCongViecCha] == DBNull.Value || string.IsNullOrEmpty(r[ColIdCongViecCha]?.ToString()))
                            countLevel1++;
                    }
                }
                return (countLevel1 + 1).ToString();
            }
        }

        public string GetRootPhaseName(Guid projectId, Guid? parentId)
        {
            if (!parentId.HasValue) return "--  --";

            TblCongViec parent = FetchById(parentId.Value);
            if (parent == null) return "-- Không xác định --";

            string rootCode = parent.MaCongViec.Split('.')[0];
            DataTable dt = FetchByIdAndOrderASCMaCV(projectId);
            if (dt != null)
            {
                foreach (DataRow r in dt.Rows)
                {
                    if (r[ColMaCv]?.ToString() == rootCode)
                        return $"[{rootCode}] {r[ColTenCv]}";
                }
            }
            return $"[{parent.MaCongViec}] {parent.TenCongViec}";
        }

        public bool IsAfterOrEqual(string codeA, string codeB)
        {
            if (codeA == codeB) return true;
            var a = codeA.Split('.');
            var b = codeB.Split('.');
            int len = Math.Min(a.Length, b.Length);

            for (int i = 0; i < len; i++)
            {
                int numA = int.TryParse(a[i], out int vA) ? vA : 0;
                int numB = int.TryParse(b[i], out int vB) ? vB : 0;
                if (numA != numB) return numA > numB;
            }
            return a.Length >= b.Length;
        }

        public (DateTime? minDate, string alert) GetMinStartDate(Guid? parentId, Guid? depId)
        {
            DateTime? minStartLimit = null;
            string alert = "";

            if (parentId.HasValue)
            {
                TblCongViec parent = FetchById(parentId.Value);
                if (parent != null && parent.NgayBatDau.HasValue)
                {
                    minStartLimit = parent.NgayBatDau.Value.Date;
                    alert = $"ngày bắt đầu của công việc cha ({parent.NgayBatDau.Value:dd/MM/yyyy})";
                }
            }
            if (depId.HasValue)
            {
                TblCongViec dep = FetchById(depId.Value);
                if (dep != null && dep.NgayKetThuc.HasValue)
                {
                    DateTime depMinStart = dep.NgayKetThuc.Value.Date.AddDays(1);
                    if (!minStartLimit.HasValue || depMinStart > minStartLimit.Value)
                    {
                        minStartLimit = depMinStart;
                        alert = $"sau ngày kết thúc của công việc phụ thuộc [{dep.MaCongViec}] ({dep.NgayKetThuc.Value:dd/MM/yyyy})";
                    }
                }
            }

            return (minStartLimit, alert);
        }
        public (DataTable Dt, Dictionary<Guid, string> DictCodes, int OverdueCount) GetDictTasksAndCountOverdue(Guid projectId, string searchValue=null)
        {
            DataTable dt = FetchByIdAndOrderASCMaCV(projectId, searchValue);
            var dictCodes = new Dictionary<Guid, string>();
            int overdueCount = 0;

            if (dt != null)
            {
                foreach (DataRow row in dt.Rows)
                {
                    if (row[ColIdCongViec] != DBNull.Value && Guid.TryParse(row[ColIdCongViec].ToString(), out Guid id))
                    {
                        dictCodes[id] = row[ColMaCv]?.ToString() ?? "";
                    }

                    if (CheckOverdue(row))
                    {
                        overdueCount++;
                    }
                }
            }

            return (dt, dictCodes, overdueCount);
        }
        #endregion

        #region 3. Tự động Đồng bộ Thời gian & Độ ưu tiên
        public void AutoSetParentPriority(Guid projectId, Guid parentId, Dictionary<Guid, TblDoUuTien> dictPriorities)
        {
            TblCongViec parentTask = FetchById(parentId);
            if (parentTask == null || parentTask.DaXoa == true) return;

            DataTable dtChildTasks = GetChildTasks(projectId, parentId);
            if (dtChildTasks != null && dtChildTasks.Rows.Count > 0)
            {
                int maxScore = -1;
                Guid? highestPriorityId = null;

                foreach (DataRow row in dtChildTasks.Rows)
                {
                    if (row[ColIdDoUuTien] != DBNull.Value && Guid.TryParse(row[ColIdDoUuTien].ToString(), out Guid idPri))
                    {
                        if (dictPriorities.ContainsKey(idPri))
                        {
                            int score = dictPriorities[idPri].DiemUuTien;
                            if (score > maxScore)
                            {
                                maxScore = score;
                                highestPriorityId = idPri;
                            }
                        }
                    }
                }
                parentTask.IdDoUuTien = highestPriorityId;
                parentTask.NgayCapNhat = DateTime.Now;
                parentTask.Save();

                if (parentTask.IdCongViecCha.HasValue)
                {
                    AutoSetParentPriority(projectId, parentTask.IdCongViecCha.Value, dictPriorities);
                }
            }
        }

        public void AutoSetDependentTime(Guid projectId, Guid taskId)
        {
            TblCongViec task = FetchById(taskId);
            if (task == null || !task.NgayKetThuc.HasValue) return;

            DataTable dtDependentTasks = GetDependentTasks(projectId, taskId);
            if (dtDependentTasks != null && dtDependentTasks.Rows.Count > 0)
            {
                foreach (DataRow row in dtDependentTasks.Rows)
                {
                    if (row[ColIdCongViec] != DBNull.Value && Guid.TryParse(row[ColIdCongViec].ToString(), out Guid depTaskId))
                    {
                        TblCongViec depTask = FetchById(depTaskId);
                        if (depTask != null && depTask.DaXoa != true)
                        {
                            DateTime minStart = task.NgayKetThuc.Value.AddDays(1);
                            if (!depTask.NgayBatDau.HasValue || depTask.NgayBatDau.Value.Date < minStart.Date)
                            {
                                int thoiHan = depTask.ThoiHanNgay ?? 1;
                                depTask.NgayBatDau = minStart;
                                depTask.NgayKetThuc = minStart.AddDays(thoiHan - 1);
                                depTask.NgayCapNhat = DateTime.Now;
                                depTask.Save();

                                AutoSetDependentTime(projectId, depTask.IdCongViec);
                                if (depTask.IdCongViecCha.HasValue)
                                {
                                    AutoSetParentTime(projectId, depTask.IdCongViecCha.Value);
                                }
                            }
                        }
                    }
                }
            }
        }

        public void AutoSetFirstChildStartTime(Guid projectId, Guid parentId, DateTime newStartDate)
        {
            TblCongViec firstChild = _repository.GetFirstChildTask(projectId, parentId);
            if (firstChild == null || firstChild.DaXoa == true) return;

            TblCongViec grandChild = _repository.GetFirstChildTask(projectId, firstChild.IdCongViec);
            if (grandChild != null)
            {
                AutoSetFirstChildStartTime(projectId, firstChild.IdCongViec, newStartDate);
            }
            else
            {
                int thoiHan = firstChild.ThoiHanNgay ?? 1;
                firstChild.NgayBatDau = newStartDate;
                firstChild.NgayKetThuc = newStartDate.AddDays(thoiHan - 1);
                firstChild.NgayCapNhat = DateTime.Now;
                firstChild.Save();

                AutoSetDependentTime(projectId, firstChild.IdCongViec);
                if (firstChild.IdCongViecCha.HasValue)
                {
                    AutoSetParentTime(projectId, firstChild.IdCongViecCha.Value);
                }
            }
        }
        public void AutoSetParentStatus(Guid projectId, Guid parentTaskId)
        {
            DataTable dtChildren = _repository.GetChildTasks(projectId,parentTaskId);
            if (dtChildren != null && dtChildren.Rows.Count > 0)
            {
                bool allCompleted = true;
                foreach (DataRow row in dtChildren.Rows)
                {
                    int trangThai = row["TrangThai"] != DBNull.Value ? Convert.ToInt32(row["TrangThai"]) : 0;
                    if (trangThai != 2)
                    {
                        allCompleted = false;
                        break; 
                    }
                }
                TblCongViec parentTask = FetchById(parentTaskId);
                if (parentTask != null)
                {
                    byte newStatus = allCompleted ? (byte)2 : (byte)1;
                    if (parentTask.TrangThai != newStatus)
                    {
                        parentTask.TrangThai = newStatus;
                        parentTask.NgayCapNhat = DateTime.Now;
                        parentTask.Save(); 
                        if (parentTask.IdCongViecCha.HasValue)
                        {
                            AutoSetParentStatus(projectId, parentTask.IdCongViecCha.Value);
                        }
                    }
                }
            }
        }
        public void AutoSetParentTime(Guid projectId, Guid parentId)
        {
            TblCongViec parentTask = FetchById(parentId);
            if (parentTask == null || parentTask.DaXoa == true) return;

            DataTable dtChildTasks = GetChildTasks(projectId, parentId);
            if (dtChildTasks != null && dtChildTasks.Rows.Count > 0)
            {
                DateTime? maxEnd = null;
                foreach (DataRow row in dtChildTasks.Rows)
                {
                    if (row[ColNgayKetThuc] != DBNull.Value && DateTime.TryParse(row[ColNgayKetThuc].ToString(), out DateTime ngayKt))
                    {
                        if (!maxEnd.HasValue || ngayKt > maxEnd.Value)
                            maxEnd = ngayKt;
                    }
                }

                if (maxEnd.HasValue && parentTask.NgayBatDau.HasValue)
                {
                    parentTask.NgayKetThuc = maxEnd.Value;
                    parentTask.ThoiHanNgay = (maxEnd.Value.Date - parentTask.NgayBatDau.Value.Date).Days + 1;
                    parentTask.NgayCapNhat = DateTime.Now;
                    parentTask.Save();

                    AutoSetDependentTime(projectId, parentTask.IdCongViec);
                    if (parentTask.IdCongViecCha.HasValue)
                    {
                        AutoSetParentTime(projectId, parentTask.IdCongViecCha.Value);
                    }
                }
            }
        }
        #endregion

        #region 4. Khai báo Tên cột CSDL
        public static readonly string ColIdCongViec = TblCongViec.Columns.IdCongViec;
        public static readonly string ColIdDuAn = TblCongViec.Columns.IdDuAn;
        public static readonly string ColIdGiaiDoan = TblCongViec.Columns.IdGiaiDoan;
        public static readonly string ColIdCongViecCha = TblCongViec.Columns.IdCongViecCha;
        public static readonly string ColIdCongViecPhuThuoc = TblCongViec.Columns.IdCongViecPhuThuoc;
        public static readonly string ColIdDoUuTien = TblCongViec.Columns.IdDoUuTien;
        public static readonly string ColMaCv = TblCongViec.Columns.MaCongViec;
        public static readonly string ColTenCv = TblCongViec.Columns.TenCongViec;
        public static readonly string ColMoTa = TblCongViec.Columns.MoTa;
        public static readonly string ColNgayBatDau = TblCongViec.Columns.NgayBatDau;
        public static readonly string ColThoiHanNgay = TblCongViec.Columns.ThoiHanNgay;
        public static readonly string ColNgayKetThuc = TblCongViec.Columns.NgayKetThuc;
        public static readonly string ColNgayHoanThanhThucTe = TblCongViec.Columns.NgayHoanThanhThucTe;
        public static readonly string ColPhanTramHoanThanh = TblCongViec.Columns.PhanTramHoanThanh;
        public static readonly string ColTrangThai = TblCongViec.Columns.TrangThai;
        public static readonly string ColDaXoa = TblCongViec.Columns.DaXoa;
        public static readonly string ColNguoiTao = TblCongViec.Columns.NguoiTao;
        public static readonly string ColNgayTao = TblCongViec.Columns.NgayTao;
        public static readonly string ColNguoiCapNhat = TblCongViec.Columns.NguoiCapNhat;
        public static readonly string ColNgayCapNhat = TblCongViec.Columns.NgayCapNhat;
        #endregion
    }
}