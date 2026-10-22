using SubSonic;
using SweetSoft.QLDA.Core.EnumHelper;
using SweetSoft.QLDA.Core.EnumHelper.Defines;
using SweetSoft.QLDA.Core.Infrastructure;
using SweetSoft.QLDA.Core.Infrastructure.Interfaces;
using SweetSoft.QLDA.Core.Respositories;
using SweetSoft.QLDA.Core.SysManager;
using SweetSoft.QLDA.DataAccess;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
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
        public Dictionary<Guid, List<string>> GetConflictTasks(DateTime start, DateTime end, Guid currentTaskId)
        {
            return _repository.GetConflictTasks(start, end, currentTaskId);
        }

        #region 1. Lấy dữ liệu & Danh mục
        public DataTable FetchByIdAndOrderASCMaCV(Guid projectId, string searchValue = null) => _repository.FetchByIdAndOrderASCMaCV(projectId, searchValue);
        public DataTable FetchByEmployeeIdAndOrderASCMaCV(Guid projectId, Guid idNhanVien, string searchValue = null) => _repository.FetchByEmployeeIdAndOrderASCMaCV(projectId, idNhanVien, searchValue);
        public TblCongViec FetchById(Guid taskId) => _repository.FetchById(taskId);
        public DataTable GetChildTasks(Guid projectId, Guid taskId) => _repository.GetChildTasks(projectId, taskId);
        public DataTable GetDependentTasks(Guid projectId, Guid taskId) => _repository.GetDependentTasks(projectId, taskId);
        public Dictionary<Guid, int> GetTaskCountsByProject(Guid idDuAn)
        {
            return _repository.GetTaskCountsByProject(idDuAn);
        }
        // Luu thong bao sau commit (khong gui email truoc khi SQL transaction commit).
        // Neu co TransactionScope bao ngoai, su kien Completed cua ambient transaction thuc su
        // moi phat thong bao. Ve lau dai co the chuyen sang outbox neu can delivery dam bao.
        private sealed class ThongBaoThayDoiPhanCong
        {
            public Guid NguoiNhan, DuAn, CongViec;
            public string TieuDe, NoiDung, Loai;
        }

        private static void PhatThongBaoDaCommit(List<ThongBaoThayDoiPhanCong> ds,
            System.Transactions.Transaction giaoDichBenNgoai)
        {
            if (ds == null || ds.Count == 0) return;
            Action gui = () =>
            {
                foreach (var thongBao in ds)
                {
                    try
                    {
                        var result = ThongBaoManager.Instance.Create(
                            userId: thongBao.NguoiNhan,
                            tieuDe: thongBao.TieuDe,
                            noiDung: thongBao.NoiDung,
                            loaiThongBao: thongBao.Loai,
                            idCongViec: thongBao.CongViec == Guid.Empty ? (Guid?)null : thongBao.CongViec,
                            idDuAn: thongBao.DuAn,
                            guiEmail: false); // 2A: chi thong bao tren web, khong tu gui mail
                        if (result == null)
                            SysLogger.LogError("Task: khong tao duoc thong bao sau commit, Task=" + thongBao.CongViec);
                    }
                    catch (Exception exc)
                    {
                        SysLogger.LogError(exc, "Task: loi tao thong bao sau commit, Task=" + thongBao.CongViec);
                    }
                }
            };
            if (giaoDichBenNgoai == null) { gui(); return; }
            giaoDichBenNgoai.TransactionCompleted += (sender, args) =>
            {
                if (args.Transaction.TransactionInformation.Status == TransactionStatus.Committed) gui();
            };
        }

        private static ThongBaoThayDoiPhanCong TaoThongBaoGoPhanCong(
            Guid idDuAn, TblCongViec task, Guid idNhanVien,
            KhaiBaoGio20Manager.KetQuaGoPhanCong daChot, bool doRoiDuAn)
        {
            int trongCa = daChot == null ? 0 : daChot.SoPhienTrongCa;
            int tangCa = daChot == null ? 0 : daChot.SoPhienTangCa;
            string ten = "[" + (task.MaCongViec ?? "--") + "] " + (task.TenCongViec ?? "Công việc");
            bool coChot = trongCa + tangCa > 0;
            string lyDo = doRoiDuAn
                ? "Bạn không còn tham gia dự án này nên đã được gỡ khỏi công việc."
                : "Bạn đã được gỡ khỏi danh sách nhân sự thực hiện công việc.";
            string chiTiet = coChot
                ? " Hệ thống đã tự động dừng " + trongCa + " phiên giờ trong ca và " + tangCa +
                  " phiên tăng ca tại thời điểm gỡ phân công. Toàn bộ giờ đã ghi nhận vẫn được bảo lưu."
                : (daChot == null
                    ? " Không có thông tin chốt phiên KhaiBaoGio 2.0 trong lần cập nhật này."
                    : " Không có phiên tính giờ 2.0 nào đang mở cần dừng. Giờ đã ghi nhận trước đây vẫn được bảo lưu.");
            return new ThongBaoThayDoiPhanCong
            {
                NguoiNhan = idNhanVien, DuAn = idDuAn, CongViec = task.IdCongViec,
                Loai = ThongBaoTypes.CongViec, // icon Cong viec trong chuong, KHONG phai HE_THONG
                TieuDe = (coChot ? "Đã dừng tính giờ và gỡ khỏi công việc: " : "Bạn đã bị gỡ khỏi công việc: ") + ten,
                NoiDung = "Công việc: " + ten + ". " + lyDo + chiTiet
            };
        }

        public void RemoveUserFromActiveTasks(Guid idDuAn, Guid idNhanVien)
        {
            if (idDuAn == Guid.Empty || idNhanVien == Guid.Empty)
                throw new ArgumentException("Dự án hoặc nhân viên không hợp lệ.");
            // Doc so bo NGOAI transaction de khong giu lock row assignment trong khi cho AppLock.
            var duKienGo = _repository.GetActiveTaskIdsOfUserInProject(idDuAn, idNhanVien)
                .Distinct().ToList();
            Transaction ambientBenNgoai = Transaction.Current;
            var dsThongBao = new List<ThongBaoThayDoiPhanCong>();
            using (var scope = new TransactionScope())
            {
                KhaiBaoGio20Manager.PhienDongBoPhanCong khoa = null;
                try
                {
                    if (duKienGo.Count > 0 && KhaiBaoGioManager.KhaiBaoGio20DaKichHoat())
                        khoa = new KhaiBaoGio20Manager().MoDongBoPhanCongTrongTransactionScope(idDuAn);
                    // Doc lai SAU khi giu khoa; neu membership vua doi thi fail-closed.
                    var tasksToRemove = _repository.GetActiveTaskIdsOfUserInProject(idDuAn, idNhanVien)
                        .Distinct().ToList();
                    if (!new HashSet<Guid>(duKienGo).SetEquals(tasksToRemove))
                        throw new InvalidOperationException("Danh sách phân công đã thay đổi, vui lòng thử lại.");
                    foreach (Guid taskId in tasksToRemove)
                    {
                        TblCongViec task = FetchById(taskId);
                        if (task == null || task.IdDuAn != idDuAn) continue;
                        var daChot = khoa == null ? null : khoa.ChotPhien(taskId, idNhanVien, DateTime.UtcNow);
                        _repository.RemoveAssignment(taskId, idNhanVien);
                        dsThongBao.Add(TaoThongBaoGoPhanCong(idDuAn, task, idNhanVien, daChot, true));
                    }
                }
                finally { if (khoa != null) khoa.Dispose(); }
                scope.Complete();
            }
            PhatThongBaoDaCommit(dsThongBao, ambientBenNgoai);
        }
        public void ValidateCanCompleteProject(Guid idDuAn)
        {
            DataTable dt = FetchByIdAndOrderASCMaCV(idDuAn, null);
            foreach (DataRow row in dt.Rows)
            {
                bool daXoa = row[ColDaXoa] != DBNull.Value && Convert.ToBoolean(row[ColDaXoa]);
                if (daXoa) continue;

                int trangThai = row[ColTrangThai] != DBNull.Value ? Convert.ToInt32(row[ColTrangThai]) : 0;
                if (trangThai != 2 && trangThai != 3) // 2: Hoàn thành, 3: Hoàn thành trễ
                {
                    throw new SweetSoft.QLDA.Core.ExceptionHelpers.BusinessException("Không thể hoàn thành dự án do vẫn còn Công việc chưa hoàn thành hoặc chưa bị hủy.", null, SweetSoft.QLDA.Core.ExceptionHelpers.ErrorCodes.Conflict);
                }
            }
        }
        public DataTable FetchPhasesByProjectId(Guid projectId)
        {
            return _repository.FetchPhasesByProjectId(projectId);
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
        /// <summary>
        /// 4C.2: Xoa mem toan bo nhanh; chot DUNG phien dang mo cua tung Task bi xoa,
        /// KHONG dong phien Task cha hay Task khac ngoai nhanh. Bao toan toan bo lich su gio.
        /// SQL Task, KBG20 va reindex chung TransactionScope. Thong bao sau COMMIT.
        /// </summary>
        public TblCongViec DeleteTask(TblCongViec task)
        {
            if (task == null) return null;
            Guid projectId = task.IdDuAn;
            DuAnManager.Instance.EnsureCanModifyStructure(projectId);
            if (task.IdCongViec == Guid.Empty || projectId == Guid.Empty)
                throw new InvalidOperationException("Không xác định được công việc hoặc dự án cần xóa.");

            Transaction ambientBenNgoai = Transaction.Current;
            var dsThongBao = new List<ThongBaoThayDoiPhanCong>();
            using (var scope = new TransactionScope())
            {
                KhaiBaoGio20Manager.PhienDongBoPhanCong khoa = null;
                try
                {
                    // Khoa du an TRUOC truy van nhanh/phan cong, dung thu tu AppLock cua timer.
                    if (KhaiBaoGioManager.KhaiBaoGio20DaKichHoat())
                        khoa = new KhaiBaoGio20Manager().MoDongBoPhanCongTrongTransactionScope(projectId);

                    TblCongViec taskMoi = FetchById(task.IdCongViec);
                    if (taskMoi == null || taskMoi.IdDuAn != projectId || taskMoi.DaXoa == true)
                        throw new InvalidOperationException("Công việc đã bị xóa hoặc dữ liệu vừa thay đổi.");

                    // Khoanh nhanh theo CUNG bo loc voi SQL Delete (ca DaXoa=NULL),
                    // tranh bo sot dong ho cua Task con tren du lieu legacy.
                    List<TblCongViec> nhanh = _repository.GetActiveDeleteBranch(projectId, taskMoi.IdCongViec);

                    // Chi phat mot thong bao cho moi nhan vien, ke ca duoc giao nhieu Task trong nhanh.
                    var thongBaoTheoNguoi = new Dictionary<Guid, List<string>>();
                    foreach (TblCongViec node in nhanh)
                    {
                        foreach (Guid nhanVien in _repository.GetAssignedNhanVienIds(node.IdCongViec).Distinct())
                        {
                            if (!thongBaoTheoNguoi.ContainsKey(nhanVien))
                                thongBaoTheoNguoi[nhanVien] = new List<string>();
                            thongBaoTheoNguoi[nhanVien].Add("[" + (node.MaCongViec ?? "--") + "] " +
                                (node.TenCongViec ?? "Công việc"));
                        }
                    }

                    // Lay ca phien cua Task (KHONG chi nguoi dang duoc phan cong), do co the
                    // ton tai phien mo duoc tao truoc khi phan cong thay doi.
                    DateTime utc = DateTime.UtcNow;
                    if (khoa != null)
                        foreach (TblCongViec node in nhanh)
                            khoa.ChotTatCaPhienCuaTaskBiXoa(node.IdCongViec, utc);

                    _repository.DeleteTask(taskMoi); // xoa mem ca nhanh (khong xoa bang gio)
                    if (!string.IsNullOrEmpty(taskMoi.MaCongViec))
                        ReindexTaskCodesAfterDelete(projectId, taskMoi.MaCongViec);

                    foreach (var item in thongBaoTheoNguoi)
                    {
                        string cacTask = string.Join("; ", item.Value.Take(4));
                        string conLai = item.Value.Count > 4 ? "; và " + (item.Value.Count - 4) + " công việc khác" : "";
                        dsThongBao.Add(new ThongBaoThayDoiPhanCong
                        {
                            NguoiNhan = item.Key,
                            DuAn = projectId,
                            // Cong viec da xoa khong con URL chi tiet hop le.
                            CongViec = Guid.Empty,
                            Loai = ThongBaoTypes.CongViec,
                            TieuDe = "Công việc được phân công đã bị xóa",
                            NoiDung = "Các công việc đã xóa: " + cacTask + conLai +
                                ". Đồng hồ đang chạy trên các công việc này đã được chốt; " +
                                "toàn bộ giờ công và lịch sử đã ghi được bảo lưu."
                        });
                    }
                }
                finally { if (khoa != null) khoa.Dispose(); }
                scope.Complete();
            }
            PhatThongBaoDaCommit(dsThongBao, ambientBenNgoai);
            return task;
        }
        public TblDoUuTien GetPriorityById(Guid idDoUuTien) => _repository.GetPriorityById(idDoUuTien);
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
        /// <summary>
        /// Sắp xếp lại thứ tự các Phase (root task) bằng cách kéo thả.
        /// Đổi mã WBS của Phase và toàn bộ descendants nhưng không đổi Id.
        /// Quan hệ phụ thuộc nội bộ của task con được giữ nguyên; chỉ nối lại dependency giữa các Phase trong vùng bị ảnh hưởng.
        /// </summary>
        public void ReorderPhase(Guid projectId, Guid draggedPhaseId, Guid targetPhaseId, string dropPosition)
        {
            ReorderPhase(projectId, draggedPhaseId, targetPhaseId, dropPosition, false, true);
        }
        public void ReorderPhase(Guid projectId, Guid draggedPhaseId, Guid targetPhaseId, string dropPosition, bool autoUpdateDates, bool keepStatus)
        {
            if (projectId == Guid.Empty)
                throw new InvalidOperationException("Không xác định được dự án.");
            if (draggedPhaseId == Guid.Empty || targetPhaseId == Guid.Empty)
                throw new InvalidOperationException("Dữ liệu giai đoạn không hợp lệ.");
            if (draggedPhaseId == targetPhaseId)
                return;
            DuAnManager.Instance.EnsureCanModifyStructure(projectId);
            TblCongViec draggedPhase = FetchById(draggedPhaseId);
            TblCongViec targetPhase = FetchById(targetPhaseId);
            if (draggedPhase == null || targetPhase == null)
                throw new InvalidOperationException("Không tìm thấy giai đoạn cần sắp xếp.");
            if (draggedPhase.IdDuAn != projectId || targetPhase.IdDuAn != projectId)
                throw new InvalidOperationException("Giai đoạn không thuộc dự án hiện tại.");
            if (draggedPhase.IdCongViecCha.HasValue || targetPhase.IdCongViecCha.HasValue ||
                string.IsNullOrWhiteSpace(draggedPhase.MaCongViec) || string.IsNullOrWhiteSpace(targetPhase.MaCongViec))
                throw new InvalidOperationException("Chỉ có thể kéo thả các giai đoạn cấp cao nhất.");
            if (!string.Equals(dropPosition, "before", StringComparison.OrdinalIgnoreCase) &&
                !string.Equals(dropPosition, "after", StringComparison.OrdinalIgnoreCase))
                dropPosition = "before";
            DataTable phaseTable = FetchPhasesByProjectId(projectId);
            if (phaseTable == null || phaseTable.Rows.Count == 0)
                throw new InvalidOperationException("Dự án chưa có giai đoạn để sắp xếp.");
            var phases = new List<TblCongViec>();
            foreach (DataRow row in phaseTable.Rows)
            {
                if (row[ColIdCongViec] == DBNull.Value ||
                    !Guid.TryParse(row[ColIdCongViec].ToString(), out Guid phaseId))
                    continue;
                TblCongViec phase = FetchById(phaseId);
                if (phase == null || phase.IdDuAn != projectId || phase.IdCongViecCha.HasValue)
                    continue;
                phases.Add(phase);
            }
            phases = phases
                .OrderBy(p => ParseRootPhaseNumber(p.MaCongViec))
                .ThenBy(p => p.MaCongViec)
                .ToList();
            if (phases.Count < 2)
                return;
            int draggedIndex = phases.FindIndex(p => p.IdCongViec == draggedPhaseId);
            int targetIndex = phases.FindIndex(p => p.IdCongViec == targetPhaseId);
            if (draggedIndex < 0 || targetIndex < 0)
                throw new InvalidOperationException("Không xác định được vị trí giai đoạn.");
            var originalPhaseOrder = phases.Select(p => p.IdCongViec).ToList();
            int affectedStartIndex = Math.Min(draggedIndex, targetIndex);
            int affectedEndIndex = phases.Count - 1;
            DateTime? affectedAnchorStart = phases[affectedStartIndex].NgayBatDau;
            TblCongViec movedPhase = phases[draggedIndex];
            phases.RemoveAt(draggedIndex);
            if (draggedIndex < targetIndex)
                targetIndex--;
            if (string.Equals(dropPosition, "after", StringComparison.OrdinalIgnoreCase))
                targetIndex++;
            if (targetIndex < 0) targetIndex = 0;
            if (targetIndex > phases.Count) targetIndex = phases.Count;
            phases.Insert(targetIndex, movedPhase);

            if (phases.Select(p => p.IdCongViec).SequenceEqual(originalPhaseOrder))
                return;

            DataTable dtAllTasks = FetchByIdAndOrderASCMaCV(projectId, null);
            if (dtAllTasks == null || dtAllTasks.Rows.Count == 0)
                return;
            var taskMap = new Dictionary<Guid, TblCongViec>();
            foreach (DataRow row in dtAllTasks.Rows)
            {
                if (row[ColIdCongViec] == DBNull.Value ||
                    !Guid.TryParse(row[ColIdCongViec].ToString(), out Guid id))
                    continue;
                TblCongViec task = FetchById(id);
                if (task != null)
                    taskMap[id] = task;
            }
            var originalPhases = phases
                .OrderBy(p => ParseRootPhaseNumber(p.MaCongViec))
                .ThenBy(p => p.MaCongViec)
                .ToList();
            var phaseOldCodes = new Dictionary<Guid, string>();
            var phaseGroups = new Dictionary<Guid, List<TblCongViec>>();
            foreach (TblCongViec phase in originalPhases)
            {
                string oldCode = phase.MaCongViec;
                phaseOldCodes[phase.IdCongViec] = oldCode;
                phaseGroups[phase.IdCongViec] = taskMap.Values
                    .Where(t => !string.IsNullOrEmpty(t.MaCongViec) &&
                                (t.MaCongViec == oldCode || t.MaCongViec.StartsWith(oldCode + ".", StringComparison.Ordinal)))
                    .ToList();
            }
            using (var scope = new TransactionScope())
            {
                int tempIndex = 1;
                var tempRoots = new Dictionary<Guid, string>();
                foreach (TblCongViec phase in originalPhases)
                {
                    string tempRoot = "TMPP" + tempIndex++;
                    tempRoots[phase.IdCongViec] = tempRoot;
                    string oldCode = phaseOldCodes[phase.IdCongViec];
                    foreach (TblCongViec task in phaseGroups[phase.IdCongViec])
                    {
                        string suffix = task.MaCongViec.Length > oldCode.Length
                            ? task.MaCongViec.Substring(oldCode.Length)
                            : "";
                        task.MaCongViec = tempRoot + suffix;
                        task.NgayCapNhat = DateTime.Now;
                        task.Save();
                    }
                }
                for (int i = 0; i < phases.Count; i++)
                {
                    TblCongViec phase = phases[i];
                    string newRoot = (i + 1).ToString();
                    string tempRoot = tempRoots[phase.IdCongViec];
                    foreach (TblCongViec task in phaseGroups[phase.IdCongViec])
                    {
                        string suffix = task.MaCongViec.StartsWith(tempRoot, StringComparison.Ordinal)
                            ? task.MaCongViec.Substring(tempRoot.Length)
                            : "";
                        task.MaCongViec = newRoot + suffix;
                        task.NgayCapNhat = DateTime.Now;
                        task.Save();
                    }
                }
                // Nối lại dependency cho toàn bộ chuỗi từ Phase đầu vùng ảnh hưởng đến hết dự án.
                // Phase đầu tiên của toàn dự án không phụ thuộc ai; các Phase sau phụ thuộc Phase ngay trước.
                // Task con bên trong từng Phase giữ nguyên quan hệ nội bộ.
                for (int i = affectedStartIndex; i <= affectedEndIndex && i < phases.Count; i++)
                {
                    TblCongViec currentPhase = phases[i];
                    Guid? expectedDependency = i > 0 ? phases[i - 1].IdCongViec : (Guid?)null;
                    if (currentPhase.IdCongViecPhuThuoc != expectedDependency)
                    {
                        currentPhase.IdCongViecPhuThuoc = expectedDependency;
                        currentPhase.NgayCapNhat = DateTime.Now;
                        currentPhase.Save();
                    }
                }
                if (!keepStatus)
                {
                    var affectedTaskIds = new HashSet<Guid>();
                    for (int i = affectedStartIndex; i <= affectedEndIndex && i < phases.Count; i++)
                    {
                        foreach (TblCongViec task in phaseGroups[phases[i].IdCongViec])
                            affectedTaskIds.Add(task.IdCongViec);
                    }
                    foreach (Guid taskId in affectedTaskIds)
                    {
                        TblCongViec task = taskMap[taskId];
                        if (task.TrangThai != 0)
                        {
                            task.TrangThai = 0;
                            task.NgayHoanThanhThucTe = null;
                            task.NgayCapNhat = DateTime.Now;
                            task.Save();
                        }
                    }
                }
                if (autoUpdateDates)
                    RecalculateReorderedPhaseDates(projectId, phases, affectedStartIndex, affectedEndIndex, affectedAnchorStart);
                scope.Complete();
            }
        }
        /// <summary>
        /// Chạy thử chính xác luồng ReorderPhase trong một TransactionScope bên ngoài
        /// nhưng cố ý không Complete để toàn bộ thay đổi được rollback.
        /// Mục đích là lấy dữ liệu review sau reorder mà không ghi thay đổi thật xuống DB.
        /// </summary>
        public List<TblCongViec> PreviewReorderPhase(
            Guid projectId,
            Guid draggedPhaseId,
            Guid targetPhaseId,
            string dropPosition,
            bool autoUpdateDates,
            bool keepStatus,
            IEnumerable<Guid> affectedPhaseIds,
            DateTime? anchorStart = null)
        {
            var affectedSet = new HashSet<Guid>(
                affectedPhaseIds ?? Enumerable.Empty<Guid>());

            if (affectedSet.Count == 0)
                return new List<TblCongViec>();

            using (var previewScope = new TransactionScope())
            {
                // Preview chỉ mô phỏng việc đổi mã WBS/dependency/status trước.
                // Khi bật cập nhật lịch, chạy đúng một lần thuật toán tính lịch
                // với anchor lấy từ trạng thái trước khi đổi.
                ReorderPhase(
                    projectId,
                    draggedPhaseId,
                    targetPhaseId,
                    dropPosition,
                    false,
                    keepStatus);

                if (autoUpdateDates && anchorStart.HasValue)
                {
                    DataTable previewPhaseTable = FetchPhasesByProjectId(projectId);
                    var previewPhases = new List<TblCongViec>();
                    if (previewPhaseTable != null)
                    {
                        foreach (DataRow row in previewPhaseTable.Rows)
                        {
                            if (row[ColIdCongViec] == DBNull.Value ||
                                !Guid.TryParse(row[ColIdCongViec].ToString(), out Guid phaseId))
                                continue;

                            TblCongViec phase = FetchById(phaseId);
                            if (phase != null && phase.IdDuAn == projectId && !phase.IdCongViecCha.HasValue)
                                previewPhases.Add(phase);
                        }
                    }

                    previewPhases = previewPhases
                        .OrderBy(p => ParseRootPhaseNumber(p.MaCongViec))
                        .ThenBy(p => p.MaCongViec)
                        .ToList();

                    int previewStartIndex = previewPhases.FindIndex(p => affectedSet.Contains(p.IdCongViec));
                    int previewEndIndex = -1;
                    for (int i = previewPhases.Count - 1; i >= 0; i--)
                    {
                        if (affectedSet.Contains(previewPhases[i].IdCongViec))
                        {
                            previewEndIndex = i;
                            break;
                        }
                    }

                    if (previewStartIndex >= 0 && previewEndIndex >= previewStartIndex)
                    {
                        // Đây là điểm neo duy nhất của vùng bị ảnh hưởng:
                        // ngày cũ + IdCongViec của phase đầu vùng -> AutoSetFirstChildStartTime.
                        RecalculateReorderedPhaseDates(
                            projectId,
                            previewPhases,
                            previewStartIndex,
                            previewEndIndex,
                            anchorStart.Value.Date);
                    }
                }

                DataTable dt = FetchByIdAndOrderASCMaCV(projectId, null);
                var allTasks = new Dictionary<Guid, TblCongViec>();

                if (dt != null)
                {
                    foreach (DataRow row in dt.Rows)
                    {
                        if (!Guid.TryParse(Convert.ToString(row[ColIdCongViec]), out Guid id))
                            continue;

                        TblCongViec task = FetchById(id);
                        if (task != null && task.IdDuAn == projectId)
                            allTasks[id] = task;
                    }
                }

                var previewTasks = new List<TblCongViec>();
                foreach (TblCongViec task in allTasks.Values)
                {
                    TblCongViec root = GetRootTaskFromMap(task, allTasks);
                    if (root == null || !affectedSet.Contains(root.IdCongViec))
                        continue;

                    TblCongViec snapshot = CloneReorderPreviewTask(task);
                    previewTasks.Add(snapshot);
                }

                previewTasks = previewTasks
                    .OrderBy(t => string.IsNullOrWhiteSpace(t.MaCongViec)
                        ? 0
                        : t.MaCongViec.Split('.').Length)
                    .ThenBy(t => t.MaCongViec, Comparer<string>.Create(CompareWbsCodes))
                    .ToList();

                // Không Complete => rollback toàn bộ reorder sau khi lấy snapshot.
                return previewTasks;
            }
        }

        private int CompareWbsCodes(string codeA, string codeB)
        {
            if (string.Equals(codeA, codeB, StringComparison.OrdinalIgnoreCase))
                return 0;

            string[] a = (codeA ?? "").Split('.');
            string[] b = (codeB ?? "").Split('.');
            int len = Math.Min(a.Length, b.Length);

            for (int i = 0; i < len; i++)
            {
                int va = int.TryParse(a[i], out int parsedA) ? parsedA : int.MaxValue;
                int vb = int.TryParse(b[i], out int parsedB) ? parsedB : int.MaxValue;
                int cmp = va.CompareTo(vb);

                if (cmp != 0)
                    return cmp;
            }

            int lengthCompare = a.Length.CompareTo(b.Length);
            return lengthCompare != 0
                ? lengthCompare
                : StringComparer.OrdinalIgnoreCase.Compare(codeA, codeB);
        }

        private TblCongViec GetRootTaskFromMap(
            TblCongViec task,
            Dictionary<Guid, TblCongViec> allTasks)
        {
            if (task == null)
                return null;

            TblCongViec current = task;
            var visited = new HashSet<Guid>();

            while (current.IdCongViecCha.HasValue && current.IdCongViecCha.Value != Guid.Empty)
            {
                if (!visited.Add(current.IdCongViec))
                    return null;

                if (!allTasks.TryGetValue(current.IdCongViecCha.Value, out TblCongViec parent))
                    return null;

                current = parent;
            }

            return current;
        }

        private TblCongViec CloneReorderPreviewTask(TblCongViec task)
        {
            if (task == null)
                return null;

            var snapshot = new TblCongViec();
            snapshot.IdCongViec = task.IdCongViec;
            snapshot.IdDuAn = task.IdDuAn;
            snapshot.IdCongViecCha = task.IdCongViecCha;
            snapshot.IdCongViecPhuThuoc = task.IdCongViecPhuThuoc;
            snapshot.MaCongViec = task.MaCongViec;
            snapshot.TenCongViec = task.TenCongViec;
            snapshot.ThoiHanNgay = task.ThoiHanNgay;
            snapshot.NgayBatDau = task.NgayBatDau;
            snapshot.NgayKetThuc = task.NgayKetThuc;
            snapshot.TrangThai = task.TrangThai;
            snapshot.NgayHoanThanhThucTe = task.NgayHoanThanhThucTe;
            return snapshot;
        }

        private void RecalculateReorderedPhaseDates(Guid projectId, List<TblCongViec> phases, int affectedStartIndex, int affectedEndIndex, DateTime? affectedAnchorStart)
        {
            if (phases == null || phases.Count == 0 || !affectedAnchorStart.HasValue)
                return;

            int startIndex = Math.Max(0, affectedStartIndex);
            int endIndex = Math.Min(phases.Count - 1, affectedEndIndex);

            DataTable dtAllTasks = FetchByIdAndOrderASCMaCV(projectId, null);
            if (dtAllTasks == null || dtAllTasks.Rows.Count == 0)
                return;

            var taskMap = new Dictionary<Guid, TblCongViec>();

            foreach (DataRow row in dtAllTasks.Rows)
            {
                if (row[ColIdCongViec] == DBNull.Value ||
                    !Guid.TryParse(row[ColIdCongViec].ToString(), out Guid id))
                    continue;

                TblCongViec task = FetchById(id);

                if (task != null &&
                    task.IdDuAn == projectId &&
                    task.DaXoa != true)
                {
                    taskMap[id] = task;
                }
            }

            DateTime currentStart = affectedAnchorStart.Value.Date;

            // Xử lý lần lượt từ Phase đầu tiên bị ảnh hưởng đến hết vùng reorder.
            // Không gọi AutoSetDependentTime / AutoSetParentTime trong đoạn này
            // để tránh chuỗi đệ quy qua lại.
            for (int i = startIndex; i <= endIndex; i++)
            {
                TblCongViec phase;

                if (!taskMap.TryGetValue(phases[i].IdCongViec, out phase))
                {
                    phase = FetchById(phases[i].IdCongViec);
                }

                if (phase == null || phase.DaXoa == true)
                    continue;

                DateTime newStart = currentStart.Date;
                int phaseDuration = GetTaskDuration(phase);

                // Đảm bảo Phase luôn hợp lệ trước khi Save.
                phase.NgayBatDau = newStart;

                if (!phase.NgayKetThuc.HasValue ||
                    phase.NgayKetThuc.Value.Date < newStart.Date)
                {
                    phase.NgayKetThuc = LichBieuChungManager.Instance.CalculateTaskEndDate(
                        newStart,
                        phaseDuration);
                }

                phase.ThoiHanNgay = phaseDuration;
                phase.NgayCapNhat = DateTime.Now;
                phase.Save();

                taskMap[phase.IdCongViec] = phase;

                // Chỉ đẩy first-child chain giống nghiệp vụ hiện tại,
                // nhưng thực hiện bằng vòng lặp, không gọi AutoSetFirstChildStartTime.
                ReorderSetFirstChildChain(
                    projectId,
                    phase.IdCongViec,
                    newStart,
                    taskMap);

                // Giải quyết dependency bên trong vùng task của Phase bằng vòng lặp.
                ReorderResolveTaskDependencies(
                    projectId,
                    phase.IdCongViec,
                    taskMap);

                // Roll-up từ child -> parent -> Phase, cũng không dùng
                // AutoSetParentTime để tránh recursion.
                ReorderRollupTaskTree(
                    projectId,
                    phase.IdCongViec,
                    taskMap);

                phase = FetchById(phase.IdCongViec);

                if (phase == null)
                    continue;

                // Phòng trường hợp dữ liệu sau roll-up vẫn chưa có End hợp lệ.
                if (!phase.NgayBatDau.HasValue)
                {
                    phase.NgayBatDau = newStart;
                }

                if (!phase.NgayKetThuc.HasValue ||
                    phase.NgayKetThuc.Value.Date < phase.NgayBatDau.Value.Date)
                {
                    int safeDuration = GetTaskDuration(phase);

                    phase.NgayKetThuc = LichBieuChungManager.Instance.CalculateTaskEndDate(
                        phase.NgayBatDau.Value.Date,
                        safeDuration);

                    phase.ThoiHanNgay = safeDuration;
                    phase.NgayCapNhat = DateTime.Now;
                    phase.Save();
                }

                taskMap[phase.IdCongViec] = phase;

                SyncReorderedPhaseToProjectStage(phase);

                if (!phase.NgayKetThuc.HasValue)
                    continue;

                // End của Phase hiện tại là Start của Phase kế tiếp.
                currentStart = phase.NgayKetThuc.Value.Date.AddDays(1);
            }

            // Roll-up cuối cùng toàn bộ vùng bị ảnh hưởng.
            for (int i = startIndex; i <= endIndex; i++)
            {
                if (!taskMap.TryGetValue(phases[i].IdCongViec, out TblCongViec phase))
                    continue;

                ReorderRollupTaskTree(
                    projectId,
                    phase.IdCongViec,
                    taskMap);

                phase = FetchById(phase.IdCongViec);

                if (phase == null)
                    continue;

                SyncReorderedPhaseToProjectStage(phase);
            }
        }
        private void ReorderSetFirstChildChain(Guid projectId, Guid parentId, DateTime newStartDate, Dictionary<Guid, TblCongViec> taskMap)
        {
            Guid currentParentId = parentId;
            DateTime currentStart = newStartDate.Date;

            const int maxDepth = 1000;
            int depth = 0;

            while (depth++ < maxDepth)
            {
                TblCongViec firstChild = _repository.GetFirstChildTask(
                    projectId,
                    currentParentId);

                if (firstChild == null || firstChild.DaXoa == true)
                    break;

                if (taskMap.ContainsKey(firstChild.IdCongViec))
                {
                    firstChild = taskMap[firstChild.IdCongViec];
                }

                firstChild.NgayBatDau = currentStart;

                int duration = GetTaskDuration(firstChild);

                // Luôn tính End từ Start mới để không có trạng thái Start > End.
                firstChild.NgayKetThuc =
                    LichBieuChungManager.Instance.CalculateTaskEndDate(
                        currentStart,
                        duration);

                firstChild.ThoiHanNgay = duration;
                firstChild.NgayCapNhat = DateTime.Now;
                firstChild.Save();

                taskMap[firstChild.IdCongViec] = firstChild;

                currentParentId = firstChild.IdCongViec;
            }
        }
        private void ReorderRollupTaskTree(Guid projectId, Guid rootTaskId, Dictionary<Guid, TblCongViec> taskMap)
        {
            if (taskMap == null || taskMap.Count == 0)
                return;

            var descendants = taskMap.Values
                .Where(x =>
                    x.IdCongViec != rootTaskId &&
                    x.DaXoa != true &&
                    GetRootTaskFromMap(x, taskMap)?.IdCongViec == rootTaskId)
                .OrderByDescending(x =>
                    string.IsNullOrWhiteSpace(x.MaCongViec)
                        ? 0
                        : x.MaCongViec.Split('.').Length)
                .ThenByDescending(x => x.MaCongViec, Comparer<string>.Create(CompareWbsCodes))
                .ToList();

            var processList = new List<TblCongViec>(descendants);

            if (taskMap.TryGetValue(rootTaskId, out TblCongViec rootTask))
            {
                processList.Add(rootTask);
            }

            foreach (TblCongViec task in processList)
            {
                List<TblCongViec> children = taskMap.Values
                    .Where(x =>
                        x.IdCongViecCha.HasValue &&
                        x.IdCongViecCha.Value == task.IdCongViec &&
                        x.DaXoa != true)
                    .ToList();

                if (children.Count == 0)
                    continue;

                DateTime? maxEnd = children
                    .Where(x => x.NgayKetThuc.HasValue)
                    .Select(x => (DateTime?)x.NgayKetThuc.Value.Date)
                    .OrderByDescending(x => x.Value)
                    .FirstOrDefault();

                if (!maxEnd.HasValue)
                    continue;

                if (!task.NgayBatDau.HasValue)
                    continue;

                DateTime newStart = task.NgayBatDau.Value.Date;
                DateTime newEnd = maxEnd.Value.Date;

                // Không bao giờ để Parent có End < Start.
                if (newEnd < newStart)
                {
                    int duration = GetTaskDuration(task);

                    newEnd =
                        LichBieuChungManager.Instance.CalculateTaskEndDate(
                            newStart,
                            duration);
                }

                int newDuration =
                    LichBieuChungManager.Instance.CountWorkingDaysInRange(
                        newStart,
                        newEnd);

                bool changed =
                    !task.NgayKetThuc.HasValue ||
                    task.NgayKetThuc.Value.Date != newEnd.Date ||
                    task.ThoiHanNgay != newDuration;

                if (!changed)
                    continue;

                task.NgayKetThuc = newEnd;
                task.ThoiHanNgay = newDuration;
                task.NgayCapNhat = DateTime.Now;
                task.Save();

                taskMap[task.IdCongViec] = task;
            }
        }
        private void SyncReorderedPhaseToProjectStage(TblCongViec phaseTask)
        {
            if (phaseTask == null ||
                phaseTask.IdCongViecCha.HasValue ||
                !phaseTask.IdGiaiDoanDuAn.HasValue ||
                phaseTask.IdGiaiDoanDuAn.Value == Guid.Empty)
            {
                return;
            }

            var phase = new SubSonic.Select()
                .From(TblGiaiDoanDuAn.Schema)
                .Where(TblGiaiDoanDuAn.Columns.IdGiaiDoanDuAn)
                .IsEqualTo(phaseTask.IdGiaiDoanDuAn.Value)
                .ExecuteSingle<TblGiaiDoanDuAn>();

            if (phase == null)
                return;

            bool changed = false;

            if (phase.NgayBatDau != phaseTask.NgayBatDau)
            {
                phase.NgayBatDau = phaseTask.NgayBatDau;
                changed = true;
            }

            if (phase.NgayBatDau.HasValue &&
                phase.NgayDuKienHoanThanh.HasValue &&
                phase.NgayDuKienHoanThanh.Value.Date < phase.NgayBatDau.Value.Date)
            {
                DateTime safeEnd = phase.NgayBatDau.Value.Date;

                if (phaseTask.NgayKetThuc.HasValue &&
                    phaseTask.NgayKetThuc.Value.Date >= safeEnd)
                {
                    safeEnd = phaseTask.NgayKetThuc.Value.Date;
                }

                phase.NgayDuKienHoanThanh = safeEnd;
                changed = true;
            }

            if (changed)
            {
                phase.NgayCapNhat = DateTime.Now;
                phase.NguoiCapNhat = "System_AutoSync";
                phase.Save();
            }
        }
        private void ReorderResolveTaskDependencies(Guid projectId, Guid rootPhaseId, Dictionary<Guid, TblCongViec> taskMap)
        {
            if (taskMap == null || taskMap.Count == 0)
                return;

            var phaseTasks = taskMap.Values
                .Where(x =>
                {
                    TblCongViec root = GetRootTaskFromMap(x, taskMap);
                    return root != null &&
                           root.IdCongViec == rootPhaseId &&
                           x.DaXoa != true;
                })
                .OrderBy(x => string.IsNullOrWhiteSpace(x.MaCongViec)
                    ? 0
                    : x.MaCongViec.Split('.').Length)
                .ThenBy(x => x.MaCongViec, Comparer<string>.Create(CompareWbsCodes))
                .ToList();

            const int maxPasses = 100;

            for (int pass = 0; pass < maxPasses; pass++)
            {
                bool changed = false;

                foreach (TblCongViec task in phaseTasks)
                {
                    if (task.IdCongViec == rootPhaseId)
                        continue;

                    if (!task.IdCongViecPhuThuoc.HasValue ||
                        task.IdCongViecPhuThuoc.Value == Guid.Empty)
                        continue;

                    if (!taskMap.TryGetValue(
                        task.IdCongViecPhuThuoc.Value,
                        out TblCongViec dependency))
                    {
                        continue;
                    }

                    if (!dependency.NgayKetThuc.HasValue)
                        continue;

                    DateTime expectedStart =
                        dependency.NgayKetThuc.Value.Date.AddDays(1);

                    bool startChanged =
                        !task.NgayBatDau.HasValue ||
                        task.NgayBatDau.Value.Date != expectedStart.Date;

                    if (!startChanged)
                        continue;

                    task.NgayBatDau = expectedStart;

                    int duration = GetTaskDuration(task);

                    // Có con -> chỉ đặt Start cho task cha.
                    // Roll-up ở ReorderRollupTaskTree sẽ tính End từ con.
                    List<TblCongViec> children = taskMap.Values
                        .Where(x =>
                            x.IdCongViecCha.HasValue &&
                            x.IdCongViecCha.Value == task.IdCongViec &&
                            x.DaXoa != true)
                        .ToList();

                    if (children.Count == 0)
                    {
                        task.NgayKetThuc =
                            LichBieuChungManager.Instance.CalculateTaskEndDate(
                                expectedStart,
                                duration);

                        task.ThoiHanNgay = duration;
                    }
                    else
                    {
                        if (!task.NgayKetThuc.HasValue ||
                            task.NgayKetThuc.Value.Date < expectedStart.Date)
                        {
                            task.NgayKetThuc =
                                LichBieuChungManager.Instance.CalculateTaskEndDate(
                                    expectedStart,
                                    duration);
                        }

                        task.ThoiHanNgay = duration;
                    }

                    task.NgayCapNhat = DateTime.Now;
                    task.Save();

                    taskMap[task.IdCongViec] = task;

                    // Nếu dependency trỏ vào Parent có children,
                    // đẩy first-child chain của Parent theo Start mới.
                    if (children.Count > 0)
                    {
                        ReorderSetFirstChildChain(
                            projectId,
                            task.IdCongViec,
                            expectedStart,
                            taskMap);
                    }

                    changed = true;
                }

                // Sau mỗi pass phải roll-up lại để Dependency pass kế tiếp
                // nhìn thấy End mới nhất.
                ReorderRollupTaskTree(
                    projectId,
                    rootPhaseId,
                    taskMap);

                if (!changed)
                    break;
            }
        }
        private void RecalculateAffectedTaskDependencies(Guid projectId, HashSet<Guid> affectedPhaseIds)
        {
            if (affectedPhaseIds == null || affectedPhaseIds.Count == 0)
                return;

            DataTable dtAllTasks = FetchByIdAndOrderASCMaCV(projectId, null);
            if (dtAllTasks == null || dtAllTasks.Rows.Count == 0)
                return;

            var taskMap = new Dictionary<Guid, TblCongViec>();
            foreach (DataRow row in dtAllTasks.Rows)
            {
                if (row[ColIdCongViec] == DBNull.Value ||
                    !Guid.TryParse(row[ColIdCongViec].ToString(), out Guid id))
                    continue;

                TblCongViec task = FetchById(id);
                if (task != null && task.IdDuAn == projectId && task.DaXoa != true)
                    taskMap[id] = task;
            }

            var affectedTasks = taskMap.Values
                .Where(task => affectedPhaseIds.Contains(GetRootTaskFromMap(task, taskMap)?.IdCongViec ?? Guid.Empty))
                .Where(task => !affectedPhaseIds.Contains(task.IdCongViec))
                .ToList();

            const int maxPasses = 100;
            for (int pass = 0; pass < maxPasses; pass++)
            {
                bool changed = false;

                foreach (TblCongViec task in affectedTasks)
                {
                    if (!task.IdCongViecPhuThuoc.HasValue || task.IdCongViecPhuThuoc.Value == Guid.Empty)
                        continue;

                    TblCongViec dependency;
                    if (!taskMap.TryGetValue(task.IdCongViecPhuThuoc.Value, out dependency) ||
                        !dependency.NgayKetThuc.HasValue)
                        continue;

                    DateTime expectedStart = dependency.NgayKetThuc.Value.Date.AddDays(1);
                    if (!task.NgayBatDau.HasValue || task.NgayBatDau.Value.Date != expectedStart.Date)
                    {
                        int duration = GetTaskDuration(task);
                        task.NgayBatDau = expectedStart;
                        task.NgayKetThuc = LichBieuChungManager.Instance.CalculateTaskEndDate(expectedStart, duration);
                        task.NgayCapNhat = DateTime.Now;
                        task.Save();
                        changed = true;
                    }
                }

                foreach (TblCongViec task in affectedTasks)
                {
                    if (task.IdCongViecCha.HasValue)
                        AutoSetParentTime(projectId, task.IdCongViecCha.Value);
                }

                if (!changed)
                    break;

                foreach (TblCongViec task in affectedTasks)
                {
                    TblCongViec refreshed = FetchById(task.IdCongViec);
                    if (refreshed != null)
                        task.NgayKetThuc = refreshed.NgayKetThuc;
                }
            }
        }
        private int ParseRootPhaseNumber(string code)
        {
            if (string.IsNullOrWhiteSpace(code))
                return int.MaxValue;
            string root = code.Split('.')[0];
            return int.TryParse(root, out int result) ? result : int.MaxValue;
        }

        public void NotifyPMsOnScheduleChange(DateTime fromDate, DateTime? toDate, string detailReason, string loaiThayDoi = null, Guid? idCauHinh = null, Guid? idNgoaiLe = null)
        {
            try
            {
                string sql = @"
                    SELECT 
                        c.IdCongViec,
                        c.IdDuAn,
                        d.TenDuAn,
                        d.IdNhanVienQuanLy,
                        c.MaCongViec,
                        c.TenCongViec,
                        c.TrangThai
                    FROM TblCongViec c
                    INNER JOIN TblDuAn d ON c.IdDuAn = d.IdDuAn
                    WHERE (c.DaXoa = 0 OR c.DaXoa IS NULL)
                      AND c.TrangThai IN (0, 1)
                      AND c.NgayBatDau IS NOT NULL 
                      AND c.NgayKetThuc IS NOT NULL
                      AND c.NgayKetThuc >= @fromDate
                      AND (@toDate IS NULL OR c.NgayBatDau <= @toDate)
                      AND (d.DaXoa = 0 OR d.DaXoa IS NULL)";

                QueryCommand cmd = new QueryCommand(sql, DataService.Provider.Name);
                cmd.AddParameter("@fromDate", fromDate.Date, DbType.DateTime);
                cmd.AddParameter("@toDate", toDate.HasValue ? (object)toDate.Value.Date : DBNull.Value, DbType.DateTime);
                DataSet ds = DataService.GetDataSet(cmd);

                if (ds == null || ds.Tables.Count == 0 || ds.Tables[0].Rows.Count == 0)
                    return;

                var groupedProjects = ds.Tables[0].AsEnumerable()
                    .GroupBy(row => new
                    {
                        IdDuAn = row.Field<Guid>("IdDuAn"),
                        TenDuAn = row.Field<string>("TenDuAn"),
                        NguoiQuanLy = row.Field<Guid?>("IdNhanVienQuanLy")
                    });

                foreach (var projectGroup in groupedProjects)
                {
                    Guid? pmId = projectGroup.Key.NguoiQuanLy;
                    if (pmId == null || pmId == Guid.Empty) continue;

                    StringBuilder taskListHtml = new StringBuilder("<ul style='padding-left: 20px; margin-top: 10px;'>");
                    int count = 0;

                    foreach (var task in projectGroup)
                    {
                        if (count < 10)
                        {
                            string maCV = task.Field<string>("MaCongViec");
                            string tenCV = task.Field<string>("TenCongViec");

                            // Lấy trạng thái (do câu SQL chỉ lọc 0 và 1)
                            int trangThai = task["TrangThai"] != DBNull.Value ? Convert.ToInt32(task["TrangThai"]) : 0;
                            string strTrangThai = trangThai == 1 ? "Đang làm" : "Chưa bắt đầu";

                            // Phục hồi lại format in đậm [Mã] và in nghiêng (Trạng thái)
                            taskListHtml.AppendFormat("<li><b>[{0}]</b> {1} <i>({2})</i></li>", maCV, tenCV, strTrangThai);
                        }
                        count++;
                    }

                    if (count > 10)
                    {
                        taskListHtml.AppendFormat("<li><i>... và {0} công việc khác.</i></li>", count - 10);
                    }
                    taskListHtml.Append("</ul>");

                    string bellTitle = $"Cảnh báo lịch biểu chung thay đổi: {count} công việc thuộc [{projectGroup.Key.TenDuAn}] bị ảnh hưởng";
                    string bellContent = $"Có {count} công việc bị ảnh hưởng do {detailReason.ToLower()} Kiểm tra Email để xem chi tiết.";

                    TblThongBao notification = ThongBaoManager.Instance.CreateScheduleChangeNotification(
                        pmId.Value,
                        bellTitle,
                        bellContent,
                        projectGroup.Key.IdDuAn,
                        projectGroup.Key.TenDuAn,
                        detailReason,
                        taskListHtml.ToString()
                    );
                    if (notification != null)
                    {
                        List<TblNhacViecLichCongViec> reminderItems =
                            new List<TblNhacViecLichCongViec>();

                        foreach (var row in projectGroup)
                        {
                            Guid idCongViec = row.Field<Guid>("IdCongViec");

                            reminderItems.Add(new TblNhacViecLichCongViec
                            {
                                IdNhacViec = Guid.NewGuid(),
                                IdThongBao = notification.IdThongBao,
                                IdDuAn = projectGroup.Key.IdDuAn,
                                IdCongViec = idCongViec,
                                UserId = pmId.Value,
                                LoaiThayDoi = loaiThayDoi,
                                IdCauHinh = idCauHinh,
                                IdNgoaiLe = idNgoaiLe,
                                NoiDung = detailReason,
                                NgayHieuLucTu = fromDate,
                                NgayHieuLucDen = toDate
                            });
                        }

                        if (reminderItems.Count > 0)
                        {
                            NhacViecLichCongViecManager.Instance.CreateRange(reminderItems);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                SysLogger.LogError(ex, "Lỗi ngầm khi quét dự án bị ảnh hưởng do đổi lịch biểu");
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

        /// <summary>
        /// DOT 4B: Khong duoc bao hoan thanh / PM duyet mot Task cha
        /// neu bat ky Task con/hau due con hieu luc nao chua dat 2 hoac 3.
        /// Task la khong co con: giu nguyen quy trinh hoan thanh hien tai.
        /// Goi lai phuong thuc nay ngay TRUOC khi Save, ke ca khi popup da mo tu truoc.
        /// </summary>
        /// <summary>
        /// 4C.1: Chi duoc them con vao nhanh ma tat ca to tien con o trang thai 0/1.
        /// Kiem tra tren server; khong duoc dua vao Visible cua nut trong GridView.
        /// </summary>
        public void DamBaoDuocThemCongViecCon(Guid projectId, Guid parentTaskId)
        {
            if (projectId == Guid.Empty || parentTaskId == Guid.Empty)
                throw new InvalidOperationException("Không xác định được dự án hoặc công việc cha.");

            var visited = new HashSet<Guid>();
            Guid? currentId = parentTaskId;
            while (currentId.HasValue && currentId.Value != Guid.Empty)
            {
                if (!visited.Add(currentId.Value))
                    throw new InvalidOperationException("Cấu trúc công việc có vòng lặp cha - con; không thể thêm công việc.");
                TblCongViec current = FetchById(currentId.Value);
                if (current == null || current.DaXoa == true || current.IdDuAn != projectId)
                    throw new InvalidOperationException("Công việc cha hoặc cấp trên không còn hợp lệ trong dự án.");
                if (current.TrangThai == 4 || current.TrangThai == 2 || current.TrangThai == 3)
                    throw new InvalidOperationException("Không thể thêm công việc con vì [" +
                        (current.MaCongViec ?? "--") + "] " + (current.TenCongViec ?? "Công việc") +
                        " đang chờ duyệt hoặc đã hoàn thành.");
                if (current.TrangThai != 0 && current.TrangThai != 1)
                    throw new InvalidOperationException("Trạng thái công việc cha/cấp trên không hợp lệ để thêm công việc con.");
                currentId = current.IdCongViecCha;
            }
        }

        public void DamBaoHauDueDaHoanThanh(Guid projectId, Guid taskId)
        {
            if (projectId == Guid.Empty || taskId == Guid.Empty)
                throw new InvalidOperationException("Khong xac dinh duoc du an hoac cong viec can hoan thanh.");

            TblCongViec task = FetchById(taskId);
            if (task == null || task.IdDuAn != projectId || task.DaXoa == true)
                throw new InvalidOperationException("Cong viec khong con thuoc du an hoac da bi xoa.");

            // Su dung logic duyet cay da co san: chi lay con con hieu luc,
            // co HashSet visited de tranh lap vo han khi du lieu bi loi.
            List<TblCongViec> hauDue = GetDescendantTasks(projectId, taskId);
            if (hauDue.Any(t => t.IdCongViec == taskId))
                throw new InvalidOperationException("Cau truc cong viec co vong lap cha - con, can kiem tra lai truoc khi hoan thanh.");

            List<TblCongViec> chuaHoanThanh = hauDue
                .Where(t => t.TrangThai != 2 && t.TrangThai != 3)
                .OrderBy(t => t.MaCongViec)
                .ToList();
            if (chuaHoanThanh.Count == 0) return;

            string chiTiet = string.Join("; ", chuaHoanThanh.Take(5).Select(t =>
                "[" + (t.MaCongViec ?? "--") + "] " + (t.TenCongViec ?? "Công việc") +
                " (" + (t.TrangThai == 0 ? "Chưa bắt đầu" :
                          t.TrangThai == 1 ? "Đang thực hiện" :
                          t.TrangThai == 4 ? "Chờ duyệt" : "Chưa hoàn thành") + ")"));
            string conLai = chuaHoanThanh.Count > 5
                ? " và " + (chuaHoanThanh.Count - 5) + " công việc khác" : string.Empty;
            throw new InvalidOperationException(
                "Chưa thể báo hoặc duyệt hoàn thành công việc này: còn " +
                chuaHoanThanh.Count + " công việc con/cháu chưa hoàn thành. " +
                "Cần hoàn thành 100% công việc con trước. " + chiTiet + conLai + ".");
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
            if (idDuAn == Guid.Empty || idCongViec == Guid.Empty)
                throw new ArgumentException("Dự án hoặc công việc không hợp lệ.");
            DuAnManager.Instance.EnsureCanModifyStructure(idDuAn);
            TblCongViec task = FetchById(idCongViec);
            if (task == null || task.IdDuAn != idDuAn)
                throw new InvalidOperationException("Không tìm thấy công việc thuộc dự án hiện tại.");
            if (task.TrangThai == (byte)TrangThaiCongViec.HoanThanh ||
                task.TrangThai == (byte)TrangThaiCongViec.HoanThanhTre ||
                task.TrangThai == (byte)TrangThaiCongViec.ChoDuyet)
                throw new InvalidOperationException("Không thể thay đổi phân công của công việc đã hoàn thành hoặc đang chờ duyệt.");
            newAssigneeIds = (newAssigneeIds ?? new List<Guid>())
                .Where(x => x != Guid.Empty).Distinct().ToList();

            // Query so bo truoc TransactionScope, tranh dao nguoc lock assignment -> AppLock.
            List<Guid> danhSachTam = _repository.GetAssignedNhanVienIds(idCongViec).Distinct().ToList();
            bool canDongKBG = KhaiBaoGioManager.KhaiBaoGio20DaKichHoat();
            Transaction ambientBenNgoai = Transaction.Current;
            var dsThongBao = new List<ThongBaoThayDoiPhanCong>();
            using (var scope = new TransactionScope())
            {
                KhaiBaoGio20Manager.PhienDongBoPhanCong khoa = null;
                try
                {
                    if (canDongKBG && danhSachTam.Except(newAssigneeIds).Any())
                        khoa = new KhaiBaoGio20Manager().MoDongBoPhanCongTrongTransactionScope(idDuAn);
                    task = FetchById(idCongViec);
                    if (task == null || task.IdDuAn != idDuAn ||
                        task.TrangThai == (byte)TrangThaiCongViec.HoanThanh ||
                        task.TrangThai == (byte)TrangThaiCongViec.HoanThanhTre ||
                        task.TrangThai == (byte)TrangThaiCongViec.ChoDuyet)
                        throw new InvalidOperationException("Trạng thái công việc đã thay đổi, vui lòng tải lại.");

                    List<Guid> danhSachCu = _repository.GetAssignedNhanVienIds(idCongViec).Distinct().ToList();
                    // Trong luc popup mo neu user khac doi danh sach, yeu cau reload de tranh go nham.
                    if (!new HashSet<Guid>(danhSachTam).SetEquals(danhSachCu))
                        throw new InvalidOperationException("Phân công vừa thay đổi, vui lòng mở lại danh sách.");
                    List<Guid> canGo = danhSachCu.Except(newAssigneeIds).ToList();
                    if (canDongKBG && canGo.Count > 0 && khoa == null)
                        throw new InvalidOperationException("Phân công có thay đổi đồng thời, vui lòng thử lại.");
                    List<Guid> canThem = newAssigneeIds.Except(danhSachCu).ToList();
                    foreach (Guid id in canGo)
                    {
                        // Ket toan + dong CHI phien cua nguoi nay tren task nay, truoc khi xoa assignment.
                        var daChot = khoa == null ? null : khoa.ChotPhien(idCongViec, id, DateTime.UtcNow);
                        _repository.RemoveAssignment(idCongViec, id);
                        dsThongBao.Add(TaoThongBaoGoPhanCong(idDuAn, task, id, daChot, false));
                    }

                    List<Guid> thanhVienHienTai = canThem.Count == 0
                        ? new List<Guid>() : ThanhVienDuAnManager.Instance.GetAllActiveMemberIds(idDuAn);
                    TblVaiTroDuAn vaiTroThanhVien = canThem.Count == 0
                        ? null : VaiTroDuAnManager.Instance.GetActiveByIdVaiTro("NGUOI_THAM_GIA");
                    if (canThem.Count > 0 && vaiTroThanhVien == null)
                        throw new InvalidOperationException("Không có vai trò thành viên dự án NGUOI_THAM_GIA.");
                    foreach (Guid id in canThem)
                    {
                        _repository.AddAssignment(idCongViec, id);
                        if (!thanhVienHienTai.Contains(id))
                        {
                            ThanhVienDuAnManager.Instance.AddOrUpdate(new TblThanhVienDuAn
                            {
                                IdDuAn = idDuAn,
                                IdNhanVien = id,
                                IdVaiTroDuAn = vaiTroThanhVien.IdVaiTroDuAn
                            });
                            thanhVienHienTai.Add(id);
                            TblDuAn d = DuAnManager.Instance.GetDuAnById(idDuAn);
                            string tenDuAn = d != null ? d.TenDuAn : "Dự án";
                            dsThongBao.Add(new ThongBaoThayDoiPhanCong
                            {
                                NguoiNhan = id, DuAn = idDuAn, CongViec = Guid.Empty,
                                Loai = ThongBaoTypes.DuAn,
                                TieuDe = "Bạn đã được thêm vào dự án: " + tenDuAn,
                                NoiDung = "Bạn đã được thêm vào dự án " + tenDuAn + " để tham gia thực hiện công việc."
                            });
                        }
                        // Khong dung Task.Run trong transaction: co the gui truoc commit, khong co noiDung.
                        // Them vao task DANG THUC HIEN khong tu dong Start dong ho cho nguoi moi.
                        string ten = "[" + (task.MaCongViec ?? "--") + "] " + (task.TenCongViec ?? "Công việc");
                        string khoangNgay = task.NgayBatDau.HasValue && task.NgayKetThuc.HasValue
                            ? " Kế hoạch: " + task.NgayBatDau.Value.ToString("dd/MM/yyyy") + "–" +
                              task.NgayKetThuc.Value.ToString("dd/MM/yyyy") + "." : "";
                        dsThongBao.Add(new ThongBaoThayDoiPhanCong
                        {
                            NguoiNhan = id, DuAn = idDuAn, CongViec = idCongViec,
                            Loai = ThongBaoTypes.CongViec,
                            TieuDe = "Bạn được giao công việc: " + ten,
                            NoiDung = "Bạn vừa được phân công công việc " + ten + "." + khoangNgay +
                                " Đồng hồ tính giờ của bạn chưa tự bắt đầu; hãy mở Tính giờ công việc khi đến lúc thực hiện."
                        });
                    }
                }
                finally { if (khoa != null) khoa.Dispose(); }
                scope.Complete();
            }
            PhatThongBaoDaCommit(dsThongBao, ambientBenNgoai);
        }
        public string GetValueForTrangThaiCongViec(TrangThaiCongViec impact)
        {
            switch (impact)
            {
                case TrangThaiCongViec.ChuaBatDau:
                    return "NOT_YET_STARTED";
                case TrangThaiCongViec.DangThucHien:
                    return "IN_PROGRESS";
                case TrangThaiCongViec.HoanThanh:
                case TrangThaiCongViec.HoanThanhTre:
                    return "COMPLETED";
                default:
                    return "—";
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
        public (DataTable Dt, Dictionary<Guid, string> DictCodes, int OverdueCount) GetDictTasksAndCountOverdue(Guid projectId, string searchValue = null, bool IsPM = false)
        {
            DataTable dt = new DataTable();
            if (IsPM)
            {
                dt = FetchByIdAndOrderASCMaCV(projectId, searchValue);
            }
            else
            {
                dt = FetchByEmployeeIdAndOrderASCMaCV(projectId, SweetContext.Current.UserId, searchValue);
            }
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

        public void AutoSetDependentTime(Guid projectId, Guid taskId, bool forceUpdateStartedTask = false)
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

                        // Mặc định chỉ tự động dời task chưa bắt đầu.
                        // Riêng flow Edit có thể truyền true để cascade cả task đã bắt đầu.
                        if (depTask == null || depTask.DaXoa == true || (!forceUpdateStartedTask && depTask.TrangThai != 0))
                            continue;

                        // Dependency hiện tại của hệ thống là nối tiếp tuyệt đối:
                        // Start của task phụ thuộc = End của task trước + 1 ngày.
                        DateTime newStartDate = task.NgayKetThuc.Value.Date.AddDays(1);
                        int thoiHan = depTask.ThoiHanNgay ?? 1;
                        bool hasChildren = CheckHasChildTasks(projectId, depTask);

                        depTask.NgayBatDau = newStartDate;
                        depTask.NgayKetThuc = LichBieuChungManager.Instance.CalculateTaskEndDate(newStartDate, thoiHan);
                        depTask.NgayCapNhat = DateTime.Now;
                        depTask.NguoiCapNhat = SweetContext.Current != null ? SweetContext.Current.UserName : "System_AutoSync";
                        depTask.Save();

                        if (hasChildren)
                        {
                            // Nếu dependency trỏ vào Parent/Phase:
                            // 1. Đẩy ngày xuống FULL cây con.
                            // 2. Roll-up lại thời gian của Parent/Phase.
                            // 3. Từ Parent/Phase đó tiếp tục đi dependency kế tiếp.
                            AutoSetFirstChildStartTime(projectId, depTask.IdCongViec, newStartDate, forceUpdateStartedTask);
                        }
                        else
                        {
                            // Task lá: tiếp tục domino sang dependency của chính nó.
                            AutoSetDependentTime(projectId, depTask.IdCongViec, forceUpdateStartedTask);

                            if (depTask.IdCongViecCha.HasValue)
                            {
                                AutoSetParentTime(projectId, depTask.IdCongViecCha.Value, forceUpdateStartedTask);
                            }
                        }
                    }
                }
            }
        }

        public void AutoSetFirstChildStartTime(Guid projectId, Guid parentId, DateTime newStartDate, bool forceUpdateStartedTask = false)
        {
            AutoSetFirstChildStartTimeInternal(projectId, parentId, newStartDate, forceUpdateStartedTask);

            // Bước cuối: tính lại thời gian của Parent/Phase hiện tại
            // sau khi toàn bộ task con và các quan hệ phụ thuộc đã được xử lý.
            AutoSetParentTime(projectId, parentId, forceUpdateStartedTask);

            // Kiểm tra các task phụ thuộc trực tiếp vào Parent/Phase hiện tại,
            // kể cả khi ngày kết thúc của Parent/Phase không thay đổi.
            AutoSetDependentTime(projectId, parentId, forceUpdateStartedTask);
        }

        private void AutoSetFirstChildStartTimeInternal(Guid projectId, Guid parentId, DateTime newStartDate, bool forceUpdateStartedTask)
        {
            TblCongViec firstChild = _repository.GetFirstChildTask(projectId, parentId);
            if (firstChild == null || firstChild.DaXoa == true) return;

            firstChild.NgayBatDau = newStartDate;
            int firstChildDuration = Math.Max(1, firstChild.ThoiHanNgay ?? 1);
            TblCongViec grandChild = _repository.GetFirstChildTask(projectId, firstChild.IdCongViec);

            if (grandChild != null)
            {
                // Đảm bảo ngày kết thúc hợp lệ trước khi lưu ngày bắt đầu mới.
                if (!firstChild.NgayKetThuc.HasValue ||
                    firstChild.NgayKetThuc.Value.Date < newStartDate.Date)
                {
                    firstChild.NgayKetThuc = LichBieuChungManager.Instance.CalculateTaskEndDate(
                        newStartDate,
                        firstChildDuration);
                }

                firstChild.NgayCapNhat = DateTime.Now;
                firstChild.Save();

                // 1. Đi sâu xuống toàn bộ cây task con trước.
                AutoSetFirstChildStartTimeInternal(
                    projectId,
                    firstChild.IdCongViec,
                    newStartDate,
                    forceUpdateStartedTask);

                // 2. Sau khi task con đã được cập nhật, tính lại thời gian của
                // task cha trung gian theo ngày kết thúc lớn nhất của các task con.
                AutoSetParentTime(
                    projectId,
                    firstChild.IdCongViec,
                    forceUpdateStartedTask);

                // 3. Kiểm tra phụ thuộc trực tiếp của task cha trung gian,
                // bất kể AutoSetParentTime có phát hiện thay đổi dữ liệu hay không.
                // Ví dụ: 1.1 kết thúc 31/08 thì 1.2 phải bắt đầu 01/09.
                AutoSetDependentTime(
                    projectId,
                    firstChild.IdCongViec,
                    forceUpdateStartedTask);
            }
            else
            {
                // Task lá: cập nhật ngày bắt đầu và kết thúc dự kiến.
                firstChild.NgayBatDau = newStartDate;
                firstChild.NgayKetThuc = LichBieuChungManager.Instance.CalculateTaskEndDate(
                    newStartDate,
                    firstChildDuration);
                firstChild.NgayCapNhat = DateTime.Now;
                firstChild.Save();

                // Xử lý các task phụ thuộc trực tiếp vào task lá.
                AutoSetDependentTime(
                    projectId,
                    firstChild.IdCongViec,
                    forceUpdateStartedTask);
            }
        }
        public void AutoSetParentStatus(Guid projectId, Guid parentTaskId)
        {
            // DOT 4: Dong bo 1 chieu CHI tu "Chua bat dau" (0) sang "Dang thuc hien" (1).
            // KHONG tu dong ket thuc/tra ve trang thai, KHONG gan/xoa ngay hoan thanh thuc te.
            // Hoan thanh cua moi task (ke ca task cha) thuoc flow bao hoan thanh va PM duyet.
            if (projectId == Guid.Empty || parentTaskId == Guid.Empty) return;

            // Duyet tung cap cha thay vi chi de quy khi cap hien tai thay doi:
            // neu cha da la 1 nhung ong la 0, ong van can duoc kich hoat.
            // visited ngan vong lap neu du lieu IdCongViecCha bi loi.
            var visited = new HashSet<Guid>();
            Guid? currentParentId = parentTaskId;

            while (currentParentId.HasValue &&
                   currentParentId.Value != Guid.Empty &&
                   visited.Add(currentParentId.Value))
            {
                TblCongViec parentTask = FetchById(currentParentId.Value);
                if (parentTask == null || parentTask.DaXoa == true || parentTask.IdDuAn != projectId)
                    break;

                // Chi task con con hieu luc trong DU AN moi duoc tinh.
                DataTable dtChildren = _repository.GetChildTasks(projectId, parentTask.IdCongViec);
                if (parentTask.TrangThai == 0 && dtChildren != null && dtChildren.Rows.Count > 0)
                {
                    bool hasStartedChild = false;
                    foreach (DataRow row in dtChildren.Rows)
                    {
                        // 1: Dang thuc hien, 4: Cho duyet, 2/3: Da hoan thanh.
                        // Tat ca deu chung to cong viec con da duoc bat dau truoc do.
                        if (row[ColTrangThai] == DBNull.Value) continue;
                        int childStatus = Convert.ToInt32(row[ColTrangThai]);
                        if (childStatus == 1 || childStatus == 2 || childStatus == 3 || childStatus == 4)
                        {
                            hasStartedChild = true;
                            break;
                        }
                    }

                    if (hasStartedChild)
                    {
                        parentTask.TrangThai = 1;
                        parentTask.NgayCapNhat = DateTime.Now;
                        parentTask.Save();
                    }
                }

                // KHONG cham vao 1/4/2/3, khong lam sach NgayHoanThanhThucTe.
                currentParentId = parentTask.IdCongViecCha;
            }
        }
        public void AutoSetParentTime(Guid projectId, Guid parentId, bool forceUpdateStartedTask = false)
        {
            // DOT 4: Chi dong bo KE HOACH. NgayHoanThanhThucTe do flow bao/duyet task quan ly.
            TblCongViec parentTask = FetchById(parentId);
            if (parentTask == null || parentTask.DaXoa == true) return;

            DataTable dtChildTasks = GetChildTasks(projectId, parentId);
            if (dtChildTasks != null && dtChildTasks.Rows.Count > 0)
            {
                DateTime? maxEnd = null;

                foreach (DataRow row in dtChildTasks.Rows)
                {
                    // Tìm ngày kết thúc dự kiến lớn nhất trong các task con.
                    if (row[ColNgayKetThuc] != DBNull.Value &&
                        DateTime.TryParse(row[ColNgayKetThuc].ToString(), out DateTime ngayKt))
                    {
                        if (!maxEnd.HasValue || ngayKt > maxEnd.Value)
                            maxEnd = ngayKt;
                    }

                }

                if (maxEnd.HasValue && parentTask.NgayBatDau.HasValue)
                {
                    int newThoiHan = LichBieuChungManager.Instance.CountWorkingDaysInRange(
                        parentTask.NgayBatDau.Value,
                        maxEnd.Value);


                    // Chỉ lưu task cha khi dữ liệu thực sự thay đổi.
                    bool parentChanged =
                        !parentTask.NgayKetThuc.HasValue ||
                        parentTask.NgayKetThuc.Value.Date != maxEnd.Value.Date ||
                        parentTask.ThoiHanNgay != newThoiHan;

                    if (parentChanged)
                    {
                        parentTask.NgayKetThuc = maxEnd.Value;
                        parentTask.ThoiHanNgay = newThoiHan;
                        parentTask.NgayCapNhat = DateTime.Now;
                        parentTask.Save();
                    }

                    // QUAN TRỌNG:
                    // Luôn kiểm tra các task phụ thuộc, kể cả khi ngày kết thúc của cha
                    // không thay đổi. Ví dụ: 1.1 vẫn kết thúc ngày 31/08 nhưng 1.2
                    // đang có ngày bắt đầu 31/08 thì cần được điều chỉnh thành 01/09.
                    AutoSetDependentTime(
                        projectId,
                        parentTask.IdCongViec,
                        forceUpdateStartedTask);

                    // Chỉ cập nhật các cấp cha phía trên khi dữ liệu của cha hiện tại thay đổi.
                    if (parentChanged)
                    {
                        if (parentTask.IdCongViecCha.HasValue)
                        {
                            AutoSetParentTime(
                                projectId,
                                parentTask.IdCongViecCha.Value,
                                forceUpdateStartedTask);
                        }
                        else
                        {
                            // Task gốc của một giai đoạn: đồng bộ thời gian với bản ghi giai đoạn.
                            if (parentTask.IdGiaiDoanDuAn.HasValue &&
                                parentTask.IdGiaiDoanDuAn.Value != Guid.Empty)
                            {
                                TblGiaiDoanDuAn phase = new SubSonic.Select()
                                    .From(TblGiaiDoanDuAn.Schema)
                                    .Where(TblGiaiDoanDuAn.Columns.IdGiaiDoanDuAn)
                                    .IsEqualTo(parentTask.IdGiaiDoanDuAn.Value)
                                    .ExecuteSingle<TblGiaiDoanDuAn>();

                                if (phase != null)
                                {
                                    bool phaseChanged = false;

                                    if (phase.NgayBatDau != parentTask.NgayBatDau)
                                    {
                                        phase.NgayBatDau = parentTask.NgayBatDau;
                                        phaseChanged = true;
                                    }

                                    // Ngày kết thúc dự kiến của giai đoạn không được nhỏ hơn ngày bắt đầu.
                                    if (phase.NgayBatDau.HasValue &&
                                        phase.NgayDuKienHoanThanh.HasValue &&
                                        phase.NgayDuKienHoanThanh.Value.Date <
                                            phase.NgayBatDau.Value.Date)
                                    {
                                        DateTime safeExpectedEnd = phase.NgayBatDau.Value.Date;

                                        if (parentTask.NgayKetThuc.HasValue &&
                                            parentTask.NgayKetThuc.Value.Date >= safeExpectedEnd)
                                        {
                                            safeExpectedEnd = parentTask.NgayKetThuc.Value.Date;
                                        }

                                        phase.NgayDuKienHoanThanh = safeExpectedEnd;
                                        phaseChanged = true;
                                    }

                                    if (phaseChanged)
                                    {
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
        }
        #endregion

        #region 4. Khai báo Tên cột CSDL
        public static readonly string ColIdCongViec = TblCongViec.Columns.IdCongViec;
        public static readonly string ColIdDuAn = TblCongViec.Columns.IdDuAn;
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