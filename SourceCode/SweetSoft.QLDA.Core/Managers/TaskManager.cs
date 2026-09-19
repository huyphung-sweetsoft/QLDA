using SubSonic;
using SweetSoft.QLDA.Core.Infrastructure;
using SweetSoft.QLDA.Core.Infrastructure.Interfaces;
using SweetSoft.QLDA.Core.Respositories;
using SweetSoft.QLDA.Core.SysManager;
using SweetSoft.QLDA.DataAccess;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Transactions;
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
        public void ValidateCanCompleteProject(Guid idDuAn)
        {
            DataTable dt = FetchByIdAndOrderASCMaCV(idDuAn, null);
            foreach (DataRow row in dt.Rows)
            {
                bool daXoa = row[ColDaXoa] != DBNull.Value && Convert.ToBoolean(row[ColDaXoa]);
                if (daXoa) continue;

                int trangThai = row[ColTrangThai] != DBNull.Value ? Convert.ToInt32(row[ColTrangThai]) : 0;
                if (trangThai != 2 && trangThai != 3) // 2: Hoàn thành, 3: Đã hủy
                {
                    throw new SweetSoft.QLDA.Core.ExceptionHelpers.BusinessException("Không thể hoàn thành dự án do vẫn còn Công việc chưa hoàn thành hoặc chưa bị hủy.", null, SweetSoft.QLDA.Core.ExceptionHelpers.ErrorCodes.Conflict);
                }
            }
        }
        public DataTable GetPrioritiesTable() => _repository.FetchAllPrioritiesTable();
        public DataTable GetProjectMembers(Guid projectId) => _repository.FetchProjectMembers(projectId);
        public DataTable GetActiveTasksByNhanVienInRange(Guid idNhanVien, DateTime start, DateTime end)
        => _repository.GetActiveTasksByNhanVienInRange(idNhanVien, start, end);

        public DataTable GetActiveTasksByNhanViensInRange(List<Guid> idNhanViens, DateTime start, DateTime end)
        => _repository.GetActiveTasksByNhanViensInRange(idNhanViens, start, end);
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
            if (task == null) return null;
            DuAnManager.Instance.EnsureCanModifyStructure(task.IdDuAn);
            _repository.DeleteTask(task);
            if (!string.IsNullOrEmpty(task.MaCongViec))
            {
                ReindexTaskCodesAfterDelete(task.IdDuAn, task.MaCongViec);
            }

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
                if (string.IsNullOrEmpty(code) || code == deletedCode) continue;

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
                    if (t != null && (t.DaXoa == false || t.DaXoa == null))
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
        public TblCongViec GetRootTaskByStageId(Guid idGiaiDoanDuAn)
        {
            return _repository.GetRootTaskByStageId(idGiaiDoanDuAn);
        }

        private List<TblCongViec> GetDescendantTasks(Guid projectId, Guid parentTaskId)
        {
            List<TblCongViec> result = new List<TblCongViec>();

            HashSet<Guid> visited = new HashSet<Guid>();

            CollectDescendantTasks(projectId, parentTaskId, result, visited);

            return result;
        }

        public string GetStageDisplayName(TblGiaiDoanDuAn stage)
        {
            if (!string.IsNullOrWhiteSpace(stage.TenGiaiDoanTuyChinh))
            {
                return stage.TenGiaiDoanTuyChinh.Trim();
            }

            if (stage.IdGiaiDoan.HasValue && stage.IdGiaiDoan.Value != Guid.Empty)
            {
                TblGiaiDoan commonStage = GiaiDoanManager.Instance.GetById(stage.IdGiaiDoan.Value);

                if (commonStage != null)
                    return commonStage.TenGiaiDoan;
            }

            return "Giai đoạn";
        }
        public List<Guid> GetAssignedNhanVienIds(Guid idCongViec)
        {
            return _repository.GetAssignedNhanVienIds(idCongViec);
        }

        public void UpdateAssignments(Guid idDuAn, Guid idCongViec, List<Guid> newAssigneeIds)
        {
            TblCongViec task = FetchById(idCongViec);
            DuAnManager.Instance.EnsureCanModifyStructure(idDuAn);
            if (task == null)
                throw new InvalidOperationException("Không tìm thấy công việc.");

            if (task.TrangThai == 2)
                throw new InvalidOperationException(
                    "Không thể thay đổi nhân sự của công việc đã hoàn thành.");
            
            // Lọc trùng lặp do mảng từ Client đẩy lên (phòng hờ)
            newAssigneeIds = (newAssigneeIds ?? new List<Guid>()).Distinct().ToList();

            // Lấy thông tin công việc để dùng trong thông báo
            TblCongViec congViec = FetchById(idCongViec);

            using (var scope = new TransactionScope())
            {
                // 1. So sánh Diff
                List<Guid> danhSachCu = _repository.GetAssignedNhanVienIds(idCongViec);
                List<Guid> canGo = danhSachCu.Except(newAssigneeIds).ToList();
                List<Guid> canThem = newAssigneeIds.Except(danhSachCu).ToList();

                // 2. Gỡ những người bị bỏ tick
                string tenCongViec = congViec != null ? congViec.TenCongViec : "Công việc";
                foreach (Guid id in canGo)
                {
                    _repository.RemoveAssignment(idCongViec, id);

                    //ThongBaoManager.Instance.Create(
                    //    userId: id,
                    //    tieuDe: $"Bạn đã bị gỡ khỏi công việc: {tenCongViec}",
                    //    noiDung: $"Công việc: {tenCongViec}",
                    //    loaiThongBao: ThongBaoTypes.HeThong,
                    //    idCongViec: idCongViec,
                    //    idDuAn: idDuAn
                    //);
                }

                // 3. Chuẩn bị dữ liệu Auto-Join
                List<Guid> thanhVienHienTai = ThanhVienDuAnManager.Instance.GetAllActiveMemberIds(idDuAn);
                TblVaiTroDuAn vaiTroThanhVien = VaiTroDuAnManager.Instance.GetActiveByIdVaiTro("NGUOI_THAM_GIA");

                // 4. Thêm những người mới được tick
                foreach (Guid id in canThem)
                {
                    _repository.AddAssignment(idCongViec, id);

                    // Hiệu ứng phụ: Auto-Join Dự án
                    if (!thanhVienHienTai.Contains(id))
                    {
                        ThanhVienDuAnManager.Instance.AddOrUpdate(new TblThanhVienDuAn
                        {
                            IdDuAn = idDuAn,
                            IdNhanVien = id,
                            IdVaiTroDuAn = vaiTroThanhVien.IdVaiTroDuAn
                        });

                        // Cập nhật lại mảng hiện tại để đề phòng gán liên tiếp người ngoài dự án
                        thanhVienHienTai.Add(id);

                        TblDuAn d = DuAnManager.Instance.GetDuAnById(idDuAn);
                        string tenDuAn = d != null ? d.TenDuAn : "Dự án";

                        //ThongBaoManager.Instance.Create(
                        //    userId: id,
                        //    tieuDe: $"Bạn đã được thêm vào dự án: {tenDuAn}",
                        //    noiDung: $"Dự án: {tenDuAn}",
                        //    loaiThongBao: ThongBaoTypes.DuAn,
                        //    idDuAn: idDuAn
                        //);
                    }

                    // Thông báo: gửi cho nhân viên vừa được giao công việc
                    // IdNhanVien trong TblCongViec_NhanVien chính là aspnet_Users.UserId
                    Guid assigneeUserId = id;
                    if (congViec != null && assigneeUserId != Guid.Empty)
                    {
                        System.Threading.Tasks.Task.Run(() =>
                        {
                            try
                            {
                                string tieuDe = $"Bạn được giao công việc: {congViec.TenCongViec}";

                                //ThongBaoManager.Instance.Create(
                                //    userId          : assigneeUserId,
                                //    tieuDe          : tieuDe,
                                //    loaiThongBao    : ThongBaoTypes.CongViec,
                                //    idCongViec      : idCongViec,
                                //    idDuAn          : idDuAn
                                //);
                            }
                            catch (Exception ex)
                            {
                                SysLogger.LogError(ex, "Failed to create ThongBao for task assignment");
                            }
                        });
                    }
                }

                scope.Complete();
            }
        }

        #endregion

        #region 2. Nghiệp vụ Cây WBS & Mã Công việc
        public void SyncPendingTasksAfterScheduleChange(DateTime affectedFromDate)
        {
            List<TblCongViec> pendingTasks = _repository.GetPendingLeafTasksFromDate(affectedFromDate);

            foreach (var task in pendingTasks)
            {
                // Chỉ quét các task có đủ dữ liệu, tránh gãy ngang vòng lặp
                if (!task.NgayBatDau.HasValue || !task.ThoiHanNgay.HasValue || !task.NgayKetThuc.HasValue)
                    continue;
                try
                {
                    bool isChanged = false;
                    // [FIX NGHIỆP VỤ MỚI]: LOẠI BỎ logic tự động đẩy Ngày bắt đầu (GetNextWorkingDay).
                    // Giữ nguyên Ngày bắt đầu của Task mặc kệ ngày đó có biến thành Ngày nghỉ lễ hay không.
                    // CHỈ tính toán lại Ngày kết thúc (giãn thời gian ra để bù cho ngày nghỉ)
                    DateTime newEndDate = LichBieuChungManager.Instance.CalculateTaskEndDate(task.NgayBatDau.Value, task.ThoiHanNgay.Value);

                    if (newEndDate.Date != task.NgayKetThuc.Value.Date)
                    {
                        task.NgayKetThuc = newEndDate;
                        isChanged = true;
                    }

                    if (isChanged)
                    {
                        task.NgayCapNhat = DateTime.Now;
                        task.NguoiCapNhat = SweetContext.Current != null ? SweetContext.Current.UserName : "System_AutoSync";
                        task.Save();

                        AutoSetDependentTime(task.IdDuAn, task.IdCongViec);

                        if (task.IdCongViecCha.HasValue)
                        {
                            AutoSetParentTime(task.IdDuAn, task.IdCongViecCha.Value);
                        }
                    }
                }
                catch (Exception ex)
                {
                    // Quăng thẳng lỗi ra kèm InnerException để Transaction Scope Rollback. KHÔNG NUỐT LỖI.
                    throw new Exception($"Lỗi khi tự động đồng bộ công việc [{task.MaCongViec}].", ex);
                }
            }
        }
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
        public List<TblCongViec> GetLeafTasksForSchedule(Guid taskId)
        {
            TblCongViec currentTask = FetchById(taskId);
            List<TblCongViec> leafTasks = new List<TblCongViec>();
            if (currentTask == null) return leafTasks;

            bool isParent = CheckHasChildTasks(currentTask.IdDuAn, currentTask);
            if (isParent)
            {
                var allDescendants = GetDescendantTasks(currentTask.IdDuAn, currentTask.IdCongViec);
                foreach (var t in allDescendants)
                {
                    // Lấy tất cả Task lá (không có con), không lọc theo Trạng thái (Todo, Doing, Done lấy hết)
                    if (!CheckHasChildTasks(currentTask.IdDuAn, t) && t.NgayBatDau.HasValue && t.NgayKetThuc.HasValue)
                    {
                        leafTasks.Add(t);
                    }
                }
            }
            else
            {
                leafTasks.Add(currentTask);
            }
            return leafTasks;
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
        public (DataTable Dt, Dictionary<Guid, string> DictCodes, int OverdueCount) GetDictTasksAndCountOverdue(Guid projectId, string searchValue = null)
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

        private void CollectDescendantTasks(Guid projectId, Guid parentTaskId, List<TblCongViec> result, HashSet<Guid> visited)
        {
            if (!visited.Add(parentTaskId))
                return;

            DataTable children = GetChildTasks(projectId, parentTaskId);

            if (children == null || children.Rows.Count == 0)
            {
                return;
            }

            foreach (DataRow row in children.Rows)
            {
                if (row[ColIdCongViec] == DBNull.Value)
                    continue;

                Guid childId;

                if (!Guid.TryParse(row[ColIdCongViec].ToString(), out childId))
                {
                    continue;
                }

                TblCongViec childTask = FetchById(childId);

                if (childTask == null || childTask.DaXoa == true)
                {
                    continue;
                }

                result.Add(childTask);

                CollectDescendantTasks(projectId, childTask.IdCongViec, result, visited);
            }
        }

        private int GetTaskDuration(TblCongViec task)
        {
            if (task.ThoiHanNgay.HasValue && task.ThoiHanNgay.Value > 0)
            {
                return task.ThoiHanNgay.Value;
            }

            if (task.NgayBatDau.HasValue && task.NgayKetThuc.HasValue)
            {
                int duration = (task.NgayKetThuc.Value.Date - task.NgayBatDau.Value.Date).Days + 1;

                return Math.Max(1, duration);
            }

            return 1;
        }

        private void MoveTasksBeforeStageStart(Guid projectId, Guid rootTaskId, DateTime minimumStartDate)
        {
            DateTime minStart = minimumStartDate.Date;

            List<TblCongViec> tasks = GetDescendantTasks(projectId, rootTaskId);

            tasks = tasks.OrderBy(task => string.IsNullOrWhiteSpace(task.MaCongViec)
                                ? 0
                                : task.MaCongViec.Split('.').Length)
                .ThenBy(task => task.MaCongViec)
                .ToList();

            foreach (TblCongViec task in tasks)
            {
                bool hasDependency = task.IdCongViecPhuThuoc.HasValue && task.IdCongViecPhuThuoc.Value != Guid.Empty;

                if (hasDependency)
                    continue;

                if (!task.NgayBatDau.HasValue)
                    continue;

                if (task.NgayBatDau.Value.Date >= minStart)
                    continue;

                int duration = GetTaskDuration(task);

                task.NgayBatDau = minStart;

                task.NgayKetThuc = LichBieuChungManager.Instance.CalculateTaskEndDate(minStart, duration);

                task.ThoiHanNgay = duration;

                task.NgayCapNhat = DateTime.Now;

                task.Save();

                AutoSetDependentTime(projectId, task.IdCongViec);

                if (task.IdCongViecCha.HasValue)
                {
                    AutoSetParentTime(projectId, task.IdCongViecCha.Value);
                }
            }

            AutoSetParentTime(projectId, rootTaskId);
        }

        public TblCongViec UpdateStageTask(
            TblGiaiDoanDuAn stage,
            string stageName,
            DateTime? oldStartDate)
        {
            DuAnManager.Instance.EnsureCanModifyStructure(stage.IdDuAn);
            if (stage == null)
            {
                throw new ArgumentNullException(
                    nameof(stage));
            }

            if (stage.IdGiaiDoanDuAn == Guid.Empty)
            {
                throw new InvalidOperationException(
                    "Giai đoạn dự án không hợp lệ.");
            }

            if (string.IsNullOrWhiteSpace(stageName))
            {
                throw new InvalidOperationException(
                    "Tên giai đoạn không được để trống.");
            }

            if (!stage.NgayBatDau.HasValue)
            {
                throw new InvalidOperationException(
                    "Ngày bắt đầu không được để trống.");
            }

            TblCongViec rootTask =
                GetRootTaskByStageId(
                    stage.IdGiaiDoanDuAn);

            if (rootTask == null)
            {
                throw new InvalidOperationException(
                    "Không tìm thấy công việc gốc của giai đoạn.");
            }

            if (rootTask.IdDuAn != stage.IdDuAn)
            {
                throw new InvalidOperationException(
                    "Công việc gốc không thuộc dự án của giai đoạn.");
            }

            bool hasChildTasks =
                CheckHasChildTasks(
                    stage.IdDuAn,
                    rootTask);

            DateTime newStartDate =
                stage.NgayBatDau.Value.Date;

            rootTask.IdGiaiDoanDuAn =
                stage.IdGiaiDoanDuAn;

            rootTask.TenCongViec =
                stageName.Trim();

            rootTask.MoTa =
                string.IsNullOrWhiteSpace(stage.MoTa)
                    ? null
                    : stage.MoTa.Trim();

            rootTask.NgayBatDau =
                newStartDate;

            rootTask.NgayHoanThanhThucTe =
                stage.NgayHoanThanhThucTe.HasValue
                    ? stage.NgayHoanThanhThucTe.Value.Date
                    : (DateTime?)null;

            rootTask.NgayCapNhat =
                DateTime.Now;

            if (!hasChildTasks)
            {
                if (!stage.NgayDuKienHoanThanh.HasValue)
                {
                    throw new InvalidOperationException(
                        "Ngày dự kiến hoàn thành không được để trống.");
                }

                DateTime expectedEndDate =
                    stage.NgayDuKienHoanThanh.Value.Date;

                if (expectedEndDate < newStartDate)
                {
                    throw new InvalidOperationException(
                        "Ngày dự kiến hoàn thành không được nhỏ hơn ngày bắt đầu.");
                }

                rootTask.NgayKetThuc =
                    expectedEndDate;

                rootTask.ThoiHanNgay =
                    (expectedEndDate -
                     newStartDate).Days + 1;

                rootTask.Save();

                return rootTask;
            }

            /*
             * Nếu đã có công việc con thì chưa lấy ngày kết thúc
             * từ stage. Ngày này sẽ được tính lại từ công việc con.
             */
            rootTask.Save();

            bool movedToLaterDate =
                !oldStartDate.HasValue ||
                newStartDate >
                    oldStartDate.Value.Date;

            if (movedToLaterDate)
            {
                MoveTasksBeforeStageStart(
                    stage.IdDuAn,
                    rootTask.IdCongViec,
                    newStartDate);
            }
            else
            {
                /*
                 * Dời giai đoạn sớm hơn:
                 * giữ nguyên công việc con,
                 * chỉ tính lại ngày kết thúc công việc gốc.
                 */
                AutoSetParentTime(
                    stage.IdDuAn,
                    rootTask.IdCongViec);
            }

            return FetchById(
                rootTask.IdCongViec);
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

                        // [CHỐT CHẶN]: Chỉ tự động dời (cả tiến lẫn lùi) nếu Task phụ thuộc CHƯA BẮT ĐẦU
                        if (depTask != null && depTask.DaXoa != true && depTask.TrangThai == 0)
                        {
                            // [FIX NGHIỆP VỤ]: Ngày bắt đầu = Ngày kết thúc của task trước + 1 ngày.
                            // Không dùng GetNextWorkingDay() ở đây nữa để tránh việc đẩy qua T7/CN/Lễ.
                            DateTime minStart = task.NgayKetThuc.Value.AddDays(1);

                            // KIỂM TRA KHÁC NHAU LÀ DỜI (Không quan tâm tiến hay lùi)
                            if (!depTask.NgayBatDau.HasValue || depTask.NgayBatDau.Value.Date != minStart.Date)
                            {
                                int thoiHan = depTask.ThoiHanNgay ?? 1;

                                depTask.NgayBatDau = minStart;

                                // Lưu ý: Ngày kết thúc vẫn đưa vào Engine để rải ngày công bỏ qua lễ/tết cho chuẩn
                                depTask.NgayKetThuc = LichBieuChungManager.Instance.CalculateTaskEndDate(minStart, thoiHan);

                                depTask.NgayCapNhat = DateTime.Now;
                                depTask.NguoiCapNhat = SweetContext.Current != null ? SweetContext.Current.UserName : "System_AutoSync";
                                depTask.Save();

                                // Lan truyền domino tiếp cho các task phụ thuộc của thằng này
                                AutoSetDependentTime(projectId, depTask.IdCongViec);

                                // Báo cáo lên Giai đoạn/Task cha để kéo lùi ngày kết thúc của Cha
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
            firstChild.NgayBatDau = newStartDate;
            TblCongViec grandChild = _repository.GetFirstChildTask(projectId, firstChild.IdCongViec);
            if (grandChild != null)
            {
                firstChild.NgayCapNhat = DateTime.Now;
                firstChild.Save();
                AutoSetFirstChildStartTime(projectId, firstChild.IdCongViec, newStartDate);
            }
            else
            {
                int thoiHan = firstChild.ThoiHanNgay ?? 1;
                firstChild.NgayBatDau = newStartDate;
                firstChild.NgayKetThuc = LichBieuChungManager.Instance.CalculateTaskEndDate(newStartDate, thoiHan);
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
            DataTable dtChildren = _repository.GetChildTasks(projectId, parentTaskId);
            if (dtChildren != null && dtChildren.Rows.Count > 0)
            {
                int countNotStarted = 0;
                int countCompleted = 0;
                int totalChildren = dtChildren.Rows.Count;

                foreach (DataRow row in dtChildren.Rows)
                {
                    int trangThai = row["TrangThai"] != DBNull.Value ? Convert.ToInt32(row["TrangThai"]) : 0;

                    if (trangThai == 0) countNotStarted++;
                    else if (trangThai == 2) countCompleted++;
                }

                TblCongViec parentTask = FetchById(parentTaskId);
                if (parentTask != null)
                {
                    byte newStatus = 1;

                    if (countCompleted == totalChildren)
                    {
                        newStatus = 2;
                    }
                    else if (countNotStarted == totalChildren)
                    {
                        newStatus = 0; 
                    }

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
                    int newThoiHan = LichBieuChungManager.Instance.CountWorkingDaysInRange(parentTask.NgayBatDau.Value, maxEnd.Value);

                    // [CHỐT CHẶN CHỐNG TREO]: Chỉ Lưu và chạy dây chuyền nếu THỰC SỰ có thay đổi dữ liệu
                    if (!parentTask.NgayKetThuc.HasValue || parentTask.NgayKetThuc.Value.Date != maxEnd.Value.Date || parentTask.ThoiHanNgay != newThoiHan)
                    {
                        parentTask.NgayKetThuc = maxEnd.Value;
                        parentTask.ThoiHanNgay = newThoiHan;
                        parentTask.NgayCapNhat = DateTime.Now;
                        parentTask.Save();

                        AutoSetDependentTime(projectId, parentTask.IdCongViec);

                        if (parentTask.IdCongViecCha.HasValue)
                        {
                            AutoSetParentTime(projectId, parentTask.IdCongViecCha.Value);
                        }
                        else
                        {
                            // Nếu IdCongViecCha là null -> Đây là Root Task. 
                            if (parentTask.IdGiaiDoanDuAn.HasValue && parentTask.IdGiaiDoanDuAn.Value != Guid.Empty)
                            {
                                var phase = new SubSonic.Select().From(TblGiaiDoanDuAn.Schema)
                                                .Where(TblGiaiDoanDuAn.Columns.IdGiaiDoanDuAn).IsEqualTo(parentTask.IdGiaiDoanDuAn.Value)
                                                .ExecuteSingle<TblGiaiDoanDuAn>();

                                if (phase != null && phase.NgayBatDau != parentTask.NgayBatDau)
                                {
                                    phase.NgayBatDau = parentTask.NgayBatDau;
                                    phase.NgayCapNhat = DateTime.Now;
                                    phase.NguoiCapNhat = "System_AutoSync";
                                    phase.Save();
                                }
                            }
                        }
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