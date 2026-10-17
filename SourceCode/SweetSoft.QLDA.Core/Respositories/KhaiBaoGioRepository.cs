using SubSonic;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Configuration;
using SweetSoft.QLDA.Core.Managers;

namespace SweetSoft.QLDA.Core.Respositories
{
    #region DTO nội bộ (chỉ dùng giữa Repository và Manager)

    internal class KbgShiftRow
    {
        public Guid IdKhaiBaoCa { get; set; }
        public byte LoaiCa { get; set; }
        public int TongPhut { get; set; }
        public byte TrangThai { get; set; }
        public byte? KetQua { get; set; }
        public int PhienBan { get; set; }
        public DateTime? VaoCaLuc { get; set; }
        public DateTime? LuuLanCuoiLuc { get; set; }
    }

    internal class KbgEntryRow
    {
        public Guid IdChiTiet { get; set; }
        public Guid IdKhaiBaoCa { get; set; }
        public byte LoaiCa { get; set; }
        public Guid IdCongViec { get; set; }
        public Guid IdDuAn { get; set; }
        public int SoPhut { get; set; }
    }

    internal class KbgLogRow
    {
        public Guid IdNhatKy { get; set; }
        public Guid IdChiTiet { get; set; }
        public Guid IdKhaiBaoCa { get; set; }
        public Guid UserId { get; set; }
        public Guid IdCongViec { get; set; }
        public DateTime BatDauLuc { get; set; }
        public DateTime? KetThucLuc { get; set; }
        public int SoPhut { get; set; }
        public byte? LoaiKetThuc { get; set; }
    }

    internal class KbgFinishedTimerRow
    {
        public DateTime BatDauLuc { get; set; }
        public DateTime KetThucLuc { get; set; }
    }

    internal class KbgRunningTimerRow
    {
        public Guid IdCongViec { get; set; }
        public DateTime BatDauLuc { get; set; }
        public string MaCongViec { get; set; }
        public string TenCongViec { get; set; }
        public string MaDuAn { get; set; }
        public string TenDuAn { get; set; }
        public List<KbgFinishedTimerRow> FinishedSessions { get; set; }
        public KbgRunningTimerRow() { FinishedSessions = new List<KbgFinishedTimerRow>(); }
    }

    internal class KbgTaskRow
    {
        public Guid IdCongViec { get; set; }
        public Guid IdDuAn { get; set; }
        public string MaDuAn { get; set; }
        public string TenDuAn { get; set; }
        public string MaCongViec { get; set; }
        public string TenCongViec { get; set; }
        public DateTime? NgayBatDau { get; set; }
        public DateTime? NgayKetThuc { get; set; }
        public int TrangThai { get; set; }
        public string TenDoUuTien { get; set; }
        public int DiemUuTien { get; set; }
    }

    #endregion

    /// <summary>
    /// Truy vấn dữ liệu cho chức năng bấm giờ làm việc trong ngày.
    /// Sử dụng 3 bảng: TblKhaiBaoGioCa (1 dòng/ngày), TblKhaiBaoGioChiTiet (1 dòng tổng hợp/task/ngày),
    /// và TblKhaiBaoGioNhatKy (nhiều dòng lịch sử bấm Start/Stop cho từng task).
    /// </summary>
    internal class KhaiBaoGioRepository
    {
        // ====================== Helpers đọc dữ liệu ======================
        private static string Str(IDataRecord r, int i) { return r.IsDBNull(i) ? null : Convert.ToString(r.GetValue(i)); }
        private static DateTime? Dt(IDataRecord r, int i) { return r.IsDBNull(i) ? (DateTime?)null : Convert.ToDateTime(r.GetValue(i)); }
        private static int IntV(IDataRecord r, int i) { return r.IsDBNull(i) ? 0 : Convert.ToInt32(r.GetValue(i)); }

        private static QueryCommand NewCmd(string sql)
        {
            return new QueryCommand(sql, DataService.Provider.Name);
        }

        private const string TaskSelect = @"
            SELECT t.IdCongViec, t.IdDuAn, d.MaDuAn, d.TenDuAn, t.MaCongViec, t.TenCongViec,
                   t.NgayBatDau, t.NgayKetThuc, t.TrangThai, ut.TenDoUuTien, ut.DiemUuTien
            FROM dbo.TblCongViec t
            INNER JOIN dbo.TblDuAn d ON d.IdDuAn = t.IdDuAn
            LEFT JOIN dbo.TblDoUuTien ut ON ut.IdDoUuTien = t.IdDoUuTien ";

        private static KbgTaskRow ReadTask(IDataRecord r)
        {
            return new KbgTaskRow
            {
                IdCongViec = r.GetGuid(0),
                IdDuAn = r.GetGuid(1),
                MaDuAn = Str(r, 2),
                TenDuAn = Str(r, 3),
                MaCongViec = Str(r, 4),
                TenCongViec = Str(r, 5),
                NgayBatDau = Dt(r, 6),
                NgayKetThuc = Dt(r, 7),
                TrangThai = IntV(r, 8),
                TenDoUuTien = Str(r, 9),
                DiemUuTien = IntV(r, 10)
            };
        }

        private const string WorkableConditions = @"
              (t.DaXoa = 0 OR t.DaXoa IS NULL)
              AND (d.DaXoa = 0 OR d.DaXoa IS NULL)
              AND d.TrangThai = @ProjectActive
              AND t.TrangThai IN (0, 1)
              AND t.NgayBatDau IS NOT NULL
              AND CAST(t.NgayBatDau AS date) <= CAST(@Day AS date)
              AND NOT EXISTS (SELECT 1 FROM dbo.TblCongViec c
                              WHERE c.IdCongViecCha = t.IdCongViec AND (c.DaXoa = 0 OR c.DaXoa IS NULL)) ";

        /// <summary>
        /// Lấy duy nhất phiên đang chạy của user trong ngày cùng những phiên đã dừng của cùng task.
        /// Một lần đọc SQL, không tải danh sách task, phiếu ngày và lịch sử không liên quan.
        /// </summary>
        public KbgRunningTimerRow GetRunningTimerOfDay(Guid userId, DateTime day)
        {
            const string sql = @"
                ;WITH ActiveTimer AS
                (
                    SELECT TOP (1) nk.IdCongViec, nk.BatDauLuc,
                           t.MaCongViec, t.TenCongViec, d.MaDuAn, d.TenDuAn
                    FROM dbo.TblKhaiBaoGioNhatKy nk
                    INNER JOIN dbo.TblKhaiBaoGioCa ca ON ca.IdKhaiBaoCa = nk.IdKhaiBaoCa
                    LEFT JOIN dbo.TblCongViec t ON t.IdCongViec = nk.IdCongViec
                    LEFT JOIN dbo.TblDuAn d ON d.IdDuAn = t.IdDuAn
                    WHERE nk.UserId = @UserId AND ca.UserId = @UserId
                      AND ca.NgayLam = CAST(@Day AS date)
                      AND ca.DaXoa = 0 AND ca.TrangThai = 0
                      AND nk.KetThucLuc IS NULL
                    ORDER BY nk.BatDauLuc ASC
                )
                SELECT a.IdCongViec, a.BatDauLuc, a.MaCongViec, a.TenCongViec,
                       a.MaDuAn, a.TenDuAn, finished.BatDauLuc, finished.KetThucLuc
                FROM ActiveTimer a
                OUTER APPLY
                (
                    SELECT old.BatDauLuc, old.KetThucLuc
                    FROM dbo.TblKhaiBaoGioNhatKy old
                    INNER JOIN dbo.TblKhaiBaoGioCa ca2 ON ca2.IdKhaiBaoCa = old.IdKhaiBaoCa
                    WHERE old.UserId = @UserId AND ca2.UserId = @UserId
                      AND ca2.NgayLam = CAST(@Day AS date) AND ca2.DaXoa = 0
                      AND old.IdCongViec = a.IdCongViec AND old.KetThucLuc IS NOT NULL
                ) finished;";

            QueryCommand cmd = NewCmd(sql);
            cmd.AddParameter("@UserId", userId, DbType.Guid);
            cmd.AddParameter("@Day", day.Date, DbType.DateTime);
            KbgRunningTimerRow row = null;
            using (IDataReader r = DataService.GetReader(cmd))
            {
                while (r.Read())
                {
                    if (row == null)
                    {
                        row = new KbgRunningTimerRow
                        {
                            IdCongViec = r.GetGuid(0),
                            BatDauLuc = Convert.ToDateTime(r.GetValue(1)),
                            MaCongViec = Str(r, 2),
                            TenCongViec = Str(r, 3),
                            MaDuAn = Str(r, 4),
                            TenDuAn = Str(r, 5)
                        };
                    }
                    if (!r.IsDBNull(6) && !r.IsDBNull(7))
                    {
                        row.FinishedSessions.Add(new KbgFinishedTimerRow
                        {
                            BatDauLuc = Convert.ToDateTime(r.GetValue(6)),
                            KetThucLuc = Convert.ToDateTime(r.GetValue(7))
                        });
                    }
                }
            }
            return row;
        }

        // ====================== Đọc Task & Phiếu ngày ======================

        public List<KbgTaskRow> GetWorkableTasks(Guid userId, DateTime day, byte projectActive)
        {
            string sql = TaskSelect + @"
                INNER JOIN dbo.TblCongViec_NhanVien cn ON cn.IdCongViec = t.IdCongViec
                WHERE cn.IdNhanVien = @UserId AND " + WorkableConditions + @"
                ORDER BY t.NgayKetThuc, t.MaCongViec";

            QueryCommand cmd = NewCmd(sql);
            cmd.AddParameter("@UserId", userId, DbType.Guid);
            cmd.AddParameter("@Day", day.Date, DbType.DateTime);
            cmd.AddParameter("@ProjectActive", projectActive, DbType.Byte);

            List<KbgTaskRow> list = new List<KbgTaskRow>();
            using (IDataReader reader = DataService.GetReader(cmd))
            {
                while (reader.Read()) list.Add(ReadTask(reader));
            }
            return list.GroupBy(x => x.IdCongViec).Select(g => g.First()).ToList();
        }

        public List<KbgTaskRow> GetTaskInfos(IEnumerable<Guid> ids)
        {
            List<KbgTaskRow> list = new List<KbgTaskRow>();
            string inList = BuildGuidList(ids);
            if (inList.Length == 0) return list;

            string sql = TaskSelect + " WHERE t.IdCongViec IN (" + inList + ")";
            QueryCommand cmd = NewCmd(sql);
            using (IDataReader reader = DataService.GetReader(cmd))
            {
                while (reader.Read()) list.Add(ReadTask(reader));
            }
            return list;
        }

        public List<KbgShiftRow> GetShiftsOfDay(Guid userId, DateTime day)
        {
            const string sql = @"
                SELECT IdKhaiBaoCa, LoaiCa, TongPhut, TrangThai, KetQua, PhienBan, VaoCaLuc, LuuLanCuoiLuc
                FROM dbo.TblKhaiBaoGioCa
                WHERE UserId = @UserId AND NgayLam = CAST(@Day AS date) AND DaXoa = 0
                ORDER BY LoaiCa ASC, NgayTao ASC";

            QueryCommand cmd = NewCmd(sql);
            cmd.AddParameter("@UserId", userId, DbType.Guid);
            cmd.AddParameter("@Day", day.Date, DbType.DateTime);

            List<KbgShiftRow> list = new List<KbgShiftRow>();
            using (IDataReader r = DataService.GetReader(cmd))
            {
                while (r.Read())
                {
                    list.Add(new KbgShiftRow
                    {
                        IdKhaiBaoCa = r.GetGuid(0),
                        LoaiCa = Convert.ToByte(r.GetValue(1)),
                        TongPhut = IntV(r, 2),
                        TrangThai = Convert.ToByte(r.GetValue(3)),
                        KetQua = r.IsDBNull(4) ? (byte?)null : Convert.ToByte(r.GetValue(4)),
                        PhienBan = IntV(r, 5),
                        VaoCaLuc = Dt(r, 6),
                        LuuLanCuoiLuc = Dt(r, 7)
                    });
                }
            }
            return list;
        }

        /// <summary>Lấy dòng khai báo đại diện cho ngày (ưu tiên dòng LoaiCa = 1).</summary>
        public KbgShiftRow GetDayShift(Guid userId, DateTime day)
        {
            return GetShiftsOfDay(userId, day).FirstOrDefault();
        }

        public List<KbgEntryRow> GetEntriesOfDay(Guid userId, DateTime day)
        {
            const string sql = @"
                SELECT ct.IdChiTiet, ct.IdKhaiBaoCa, ca.LoaiCa, ct.IdCongViec, ct.IdDuAn, ct.SoPhut
                FROM dbo.TblKhaiBaoGioChiTiet ct
                INNER JOIN dbo.TblKhaiBaoGioCa ca ON ca.IdKhaiBaoCa = ct.IdKhaiBaoCa
                WHERE ca.UserId = @UserId AND ca.NgayLam = CAST(@Day AS date)
                  AND ca.DaXoa = 0 AND ct.DaXoa = 0";

            QueryCommand cmd = NewCmd(sql);
            cmd.AddParameter("@UserId", userId, DbType.Guid);
            cmd.AddParameter("@Day", day.Date, DbType.DateTime);

            List<KbgEntryRow> list = new List<KbgEntryRow>();
            using (IDataReader r = DataService.GetReader(cmd))
            {
                while (r.Read())
                {
                    list.Add(new KbgEntryRow
                    {
                        IdChiTiet = r.GetGuid(0),
                        IdKhaiBaoCa = r.GetGuid(1),
                        LoaiCa = Convert.ToByte(r.GetValue(2)),
                        IdCongViec = r.GetGuid(3),
                        IdDuAn = r.GetGuid(4),
                        SoPhut = IntV(r, 5)
                    });
                }
            }
            return list;
        }

        /// <summary>Lấy toàn bộ lịch sử bấm Start/Stop của nhân viên trong ngày.</summary>
        public List<KbgLogRow> GetLogsOfDay(Guid userId, DateTime day)
        {
            const string sql = @"
                SELECT nk.IdNhatKy, nk.IdChiTiet, nk.IdKhaiBaoCa, nk.UserId, nk.IdCongViec,
                       nk.BatDauLuc, nk.KetThucLuc, nk.SoPhut, nk.LoaiKetThuc
                FROM dbo.TblKhaiBaoGioNhatKy nk
                INNER JOIN dbo.TblKhaiBaoGioCa ca ON ca.IdKhaiBaoCa = nk.IdKhaiBaoCa
                WHERE nk.UserId = @UserId AND ca.NgayLam = CAST(@Day AS date) AND ca.DaXoa = 0
                ORDER BY nk.BatDauLuc ASC";

            QueryCommand cmd = NewCmd(sql);
            cmd.AddParameter("@UserId", userId, DbType.Guid);
            cmd.AddParameter("@Day", day.Date, DbType.DateTime);

            List<KbgLogRow> list = new List<KbgLogRow>();
            using (IDataReader r = DataService.GetReader(cmd))
            {
                while (r.Read())
                {
                    list.Add(ReadLog(r));
                }
            }
            return list;
        }

        /// <summary>Lấy toàn bộ các phiên đang chạy (KetThucLuc IS NULL) của mọi nhân viên trong một ngày để tự chốt cuối ngày.</summary>
        public List<KbgLogRow> GetOpenLogsByDay(DateTime day)
        {
            const string sql = @"
                SELECT nk.IdNhatKy, nk.IdChiTiet, nk.IdKhaiBaoCa, nk.UserId, nk.IdCongViec,
                       nk.BatDauLuc, nk.KetThucLuc, nk.SoPhut, nk.LoaiKetThuc
                FROM dbo.TblKhaiBaoGioNhatKy nk
                INNER JOIN dbo.TblKhaiBaoGioCa ca ON ca.IdKhaiBaoCa = nk.IdKhaiBaoCa
                WHERE ca.NgayLam = CAST(@Day AS date) AND ca.DaXoa = 0 AND nk.KetThucLuc IS NULL";

            QueryCommand cmd = NewCmd(sql);
            cmd.AddParameter("@Day", day.Date, DbType.DateTime);

            List<KbgLogRow> list = new List<KbgLogRow>();
            using (IDataReader r = DataService.GetReader(cmd))
            {
                while (r.Read()) list.Add(ReadLog(r));
            }
            return list;
        }

        private static KbgLogRow ReadLog(IDataRecord r)
        {
            return new KbgLogRow
            {
                IdNhatKy = r.GetGuid(0),
                IdChiTiet = r.GetGuid(1),
                IdKhaiBaoCa = r.GetGuid(2),
                UserId = r.GetGuid(3),
                IdCongViec = r.GetGuid(4),
                BatDauLuc = Convert.ToDateTime(r.GetValue(5)),
                KetThucLuc = Dt(r, 6),
                SoPhut = IntV(r, 7),
                LoaiKetThuc = r.IsDBNull(8) ? (byte?)null : Convert.ToByte(r.GetValue(8))
            };
        }

        /// <summary>Tổng phút đã chốt theo task ở các ngày trước: [0] = đúng hạn, [1] = trễ hạn.</summary>
        public Dictionary<Guid, int[]> GetHistoryMinutes(Guid userId, IEnumerable<Guid> taskIds, DateTime beforeDay)
        {
            Dictionary<Guid, int[]> dict = new Dictionary<Guid, int[]>();
            string inList = BuildGuidList(taskIds);
            if (inList.Length == 0) return dict;

            string sql = @"
                SELECT ct.IdCongViec,
                       SUM(CASE WHEN ct.PhanLoai = 1 THEN CAST(ct.SoPhut AS int) ELSE 0 END),
                       SUM(CASE WHEN ct.PhanLoai = 2 THEN CAST(ct.SoPhut AS int) ELSE 0 END)
                FROM dbo.TblKhaiBaoGioChiTiet ct
                INNER JOIN dbo.TblKhaiBaoGioCa ca ON ca.IdKhaiBaoCa = ct.IdKhaiBaoCa
                WHERE ca.UserId = @UserId AND ca.TrangThai = 1
                  AND ca.NgayLam < CAST(@BeforeDay AS date)
                  AND ca.DaXoa = 0 AND ct.DaXoa = 0
                  AND ct.IdCongViec IN (" + inList + @")
                GROUP BY ct.IdCongViec";

            QueryCommand cmd = NewCmd(sql);
            cmd.AddParameter("@UserId", userId, DbType.Guid);
            cmd.AddParameter("@BeforeDay", beforeDay.Date, DbType.DateTime);
            using (IDataReader r = DataService.GetReader(cmd))
            {
                while (r.Read())
                {
                    dict[r.GetGuid(0)] = new[] { IntV(r, 1), IntV(r, 2) };
                }
            }
            return dict;
        }

        // ====================== Ghi dữ liệu bấm giờ ======================

        /// <summary>Bảo đảm có 1 dòng TblKhaiBaoGioCa cho ngày hôm đó (dùng LoaiCa = 1 làm dòng đại diện cả ngày).</summary>
        public bool InsertDayShiftIfMissing(Guid idKhaiBaoCa, Guid userId, DateTime day,
            string gioBatDau, string gioKetThuc, int soPhutCa, string nguoiTao, DateTime now)
        {
            const string sql = @"
                IF NOT EXISTS (SELECT 1 FROM dbo.TblKhaiBaoGioCa
                               WHERE UserId = @UserId AND NgayLam = CAST(@Day AS date) AND DaXoa = 0)
                BEGIN
                    INSERT INTO dbo.TblKhaiBaoGioCa
                        (IdKhaiBaoCa, UserId, NgayLam, LoaiCa, GioBatDau, GioKetThuc, SoPhutCa,
                         TongPhut, TrangThai, PhienBan, VaoCaLuc, LuuLanCuoiLuc, NguoiTao, NgayTao)
                    VALUES
                        (@Id, @UserId, CAST(@Day AS date), 1, @GioBatDau, @GioKetThuc, @SoPhutCa,
                         0, 0, 1, @Now, @Now, @NguoiTao, @Now)
                    SELECT 1
                END
                ELSE SELECT 0";

            QueryCommand cmd = NewCmd(sql);
            cmd.AddParameter("@Id", idKhaiBaoCa, DbType.Guid);
            cmd.AddParameter("@UserId", userId, DbType.Guid);
            cmd.AddParameter("@Day", day.Date, DbType.DateTime);
            cmd.AddParameter("@GioBatDau", gioBatDau, DbType.String);
            cmd.AddParameter("@GioKetThuc", gioKetThuc, DbType.String);
            cmd.AddParameter("@SoPhutCa", soPhutCa, DbType.Int16);
            cmd.AddParameter("@NguoiTao", nguoiTao, DbType.String);
            cmd.AddParameter("@Now", now, DbType.DateTime);

            try
            {
                object o = DataService.ExecuteScalar(cmd);
                return o != null && Convert.ToInt32(o) == 1;
            }
            catch (SqlException ex)
            {
                if (ex.Number == 2601 || ex.Number == 2627) return false;
                throw;
            }
        }

        /// <summary>Lấy hoặc tạo mới bản ghi tổng hợp của 1 Task trong ngày (TblKhaiBaoGioChiTiet).</summary>
        public Guid EnsureTaskEntry(Guid idKhaiBaoCa, Guid idCongViec, Guid idDuAn, string user, DateTime now)
        {
            const string sql = @"
                DECLARE @ExistingId UNIQUEIDENTIFIER;
                SELECT TOP 1 @ExistingId = IdChiTiet
                FROM dbo.TblKhaiBaoGioChiTiet
                WHERE IdKhaiBaoCa = @IdCa AND IdCongViec = @IdCongViec AND DaXoa = 0;

                IF @ExistingId IS NULL
                BEGIN
                    SET @ExistingId = NEWID();
                    INSERT INTO dbo.TblKhaiBaoGioChiTiet
                        (IdChiTiet, IdKhaiBaoCa, IdCongViec, IdDuAn, SoPhut, DaXoa, NguoiTao, NgayTao)
                    VALUES
                        (@ExistingId, @IdCa, @IdCongViec, @IdDuAn, 0, 0, @User, @Now);
                END
                SELECT @ExistingId;";

            QueryCommand cmd = NewCmd(sql);
            cmd.AddParameter("@IdCa", idKhaiBaoCa, DbType.Guid);
            cmd.AddParameter("@IdCongViec", idCongViec, DbType.Guid);
            cmd.AddParameter("@IdDuAn", idDuAn, DbType.Guid);
            cmd.AddParameter("@User", user, DbType.String);
            cmd.AddParameter("@Now", now, DbType.DateTime);

            object o = DataService.ExecuteScalar(cmd);
            return (Guid)o;
        }

        /// <summary>Tạo 1 bản ghi nhật ký mới khi nhấn Bắt đầu / Tiếp tục.</summary>
        public void InsertStartLog(Guid idNhatKy, Guid idChiTiet, Guid idKhaiBaoCa, Guid userId, Guid idCongViec, DateTime now)
        {
            const string sql = @"
                INSERT INTO dbo.TblKhaiBaoGioNhatKy
                    (IdNhatKy, IdChiTiet, IdKhaiBaoCa, UserId, IdCongViec, BatDauLuc, KetThucLuc, SoPhut, LoaiKetThuc, NgayTao)
                VALUES
                    (@IdNhatKy, @IdChiTiet, @IdCa, @UserId, @IdCongViec, @Now, NULL, 0, NULL, @Now);

                UPDATE dbo.TblKhaiBaoGioCa
                SET VaoCaLuc = ISNULL(VaoCaLuc, @Now),
                    LuuLanCuoiLuc = @Now,
                    PhienBan = PhienBan + 1
                WHERE IdKhaiBaoCa = @IdCa;";

            QueryCommand cmd = NewCmd(sql);
            cmd.AddParameter("@IdNhatKy", idNhatKy, DbType.Guid);
            cmd.AddParameter("@IdChiTiet", idChiTiet, DbType.Guid);
            cmd.AddParameter("@IdCa", idKhaiBaoCa, DbType.Guid);
            cmd.AddParameter("@UserId", userId, DbType.Guid);
            cmd.AddParameter("@IdCongViec", idCongViec, DbType.Guid);
            cmd.AddParameter("@Now", now, DbType.DateTime);
            DataService.ExecuteQuery(cmd);
        }

        /// <summary>Đóng 1 phiên bấm giờ trong TblKhaiBaoGioNhatKy và cập nhật số phút tổng hợp lên TblKhaiBaoGioChiTiet + TblKhaiBaoGioCa.</summary>
        public void CompleteLogAndSyncTotals(Guid idNhatKy, Guid idChiTiet, Guid idKhaiBaoCa,
            DateTime ketThucLuc, int sessionMinutes, byte loaiKetThuc,
            int newTaskTotalMinutes, int maxShiftMinutes, string user, DateTime now)
        {
            const string sql = @"
                UPDATE dbo.TblKhaiBaoGioNhatKy
                SET KetThucLuc = @KetThucLuc,
                    SoPhut = @SessionMinutes,
                    LoaiKetThuc = @LoaiKetThuc
                WHERE IdNhatKy = @IdNhatKy;

                UPDATE dbo.TblKhaiBaoGioChiTiet
                SET SoPhut = @TaskTotalMinutes,
                    NguoiCapNhat = @User,
                    NgayCapNhat = @Now
                WHERE IdChiTiet = @IdChiTiet;

                DECLARE @SumPhut INT;
                SELECT @SumPhut = ISNULL(SUM(CAST(SoPhut AS INT)), 0)
                FROM dbo.TblKhaiBaoGioChiTiet
                WHERE IdKhaiBaoCa = @IdCa AND DaXoa = 0;

                IF @SumPhut > @MaxShiftMinutes SET @SumPhut = @MaxShiftMinutes;

                UPDATE dbo.TblKhaiBaoGioCa
                SET TongPhut = @SumPhut,
                    LuuLanCuoiLuc = @Now,
                    PhienBan = PhienBan + 1,
                    NguoiCapNhat = @User,
                    NgayCapNhat = @Now
                WHERE IdKhaiBaoCa = @IdCa;";

            QueryCommand cmd = NewCmd(sql);
            cmd.AddParameter("@IdNhatKy", idNhatKy, DbType.Guid);
            cmd.AddParameter("@IdChiTiet", idChiTiet, DbType.Guid);
            cmd.AddParameter("@IdCa", idKhaiBaoCa, DbType.Guid);
            cmd.AddParameter("@KetThucLuc", ketThucLuc, DbType.DateTime);
            cmd.AddParameter("@SessionMinutes", sessionMinutes, DbType.Int32);
            cmd.AddParameter("@LoaiKetThuc", loaiKetThuc, DbType.Byte);
            cmd.AddParameter("@TaskTotalMinutes", newTaskTotalMinutes, DbType.Int16);
            cmd.AddParameter("@MaxShiftMinutes", maxShiftMinutes, DbType.Int32);
            cmd.AddParameter("@User", user, DbType.String);
            cmd.AddParameter("@Now", now, DbType.DateTime);
            DataService.ExecuteQuery(cmd);
        }

        /// <summary>
        /// Bắt đầu phiên bấm giờ trong MỘT batch SQL (một transaction trên một kết nối), không dùng TransactionScope/MSDTC.
        /// Khóa dòng phiếu ngày của chính user này để các thao tác của cùng user không chạy chồng nhau.
        /// Trả về: 1 = đã tạo phiên mới; 2 = chính task này đang chạy; 3 = đang có task khác chạy; 4 = phiếu ngày đã khóa.
        /// </summary>
        public int StartLogAtomic(Guid idKhaiBaoCa, Guid userId, Guid idCongViec, Guid idDuAn, string user, DateTime now)
        {
            const string sql = @"
                SET XACT_ABORT ON;
                BEGIN TRAN;

                UPDATE dbo.TblKhaiBaoGioCa
                SET VaoCaLuc = ISNULL(VaoCaLuc, @Now),
                    LuuLanCuoiLuc = @Now,
                    PhienBan = PhienBan + 1
                WHERE IdKhaiBaoCa = @IdCa AND TrangThai = 0 AND DaXoa = 0;

                IF @@ROWCOUNT <> 1
                BEGIN
                    ROLLBACK;
                    SELECT 4;
                    RETURN;
                END

                DECLARE @Running UNIQUEIDENTIFIER;
                SELECT TOP 1 @Running = IdCongViec
                FROM dbo.TblKhaiBaoGioNhatKy
                WHERE IdKhaiBaoCa = @IdCa AND KetThucLuc IS NULL;

                IF @Running IS NOT NULL
                BEGIN
                    ROLLBACK;
                    SELECT CASE WHEN @Running = @IdCongViec THEN 2 ELSE 3 END;
                    RETURN;
                END

                DECLARE @IdChiTiet UNIQUEIDENTIFIER;
                SELECT TOP 1 @IdChiTiet = IdChiTiet
                FROM dbo.TblKhaiBaoGioChiTiet
                WHERE IdKhaiBaoCa = @IdCa AND IdCongViec = @IdCongViec AND DaXoa = 0;

                IF @IdChiTiet IS NULL
                BEGIN
                    SET @IdChiTiet = NEWID();
                    INSERT INTO dbo.TblKhaiBaoGioChiTiet
                        (IdChiTiet, IdKhaiBaoCa, IdCongViec, IdDuAn, SoPhut, DaXoa, NguoiTao, NgayTao)
                    VALUES
                        (@IdChiTiet, @IdCa, @IdCongViec, @IdDuAn, 0, 0, @User, @Now);
                END

                INSERT INTO dbo.TblKhaiBaoGioNhatKy
                    (IdNhatKy, IdChiTiet, IdKhaiBaoCa, UserId, IdCongViec, BatDauLuc, KetThucLuc, SoPhut, LoaiKetThuc, NgayTao)
                VALUES
                    (NEWID(), @IdChiTiet, @IdCa, @UserId, @IdCongViec, @Now, NULL, 0, NULL, @Now);

                COMMIT;
                SELECT 1;";

            QueryCommand cmd = NewCmd(sql);
            cmd.AddParameter("@IdCa", idKhaiBaoCa, DbType.Guid);
            cmd.AddParameter("@UserId", userId, DbType.Guid);
            cmd.AddParameter("@IdCongViec", idCongViec, DbType.Guid);
            cmd.AddParameter("@IdDuAn", idDuAn, DbType.Guid);
            cmd.AddParameter("@User", user, DbType.String);
            cmd.AddParameter("@Now", now, DbType.DateTime);

            object o = DataService.ExecuteScalar(cmd);
            return o == null ? 0 : Convert.ToInt32(o);
        }

        // ====================== Job chốt cuối ngày ======================

        /// <summary>Đóng tất cả các phiếu ngày đang mở của ngày @Day (sau khi đã qua giờ kết thúc ca chiều).</summary>
        public int CloseOpenDayShifts(DateTime day, string system, DateTime now)
        {
            // Xóa các dòng chi tiết có SoPhut = 0 (nếu nhân viên bấm Start rồi bấm Stop ngay lập tức chưa được 1 phút)
            // trước khi chốt sổ để bảng TblKhaiBaoGioChiTiet sạch sẽ.
            const string sql = @"
                DELETE ct
                FROM dbo.TblKhaiBaoGioChiTiet ct
                INNER JOIN dbo.TblKhaiBaoGioCa ca ON ca.IdKhaiBaoCa = ct.IdKhaiBaoCa
                WHERE ca.NgayLam = CAST(@Day AS date) AND ca.TrangThai = 0 AND ca.DaXoa = 0 AND ct.SoPhut <= 0;

                UPDATE dbo.TblKhaiBaoGioCa
                SET TrangThai = 1,
                    KetQua = CASE WHEN TongPhut > 0 THEN 1 ELSE 2 END,
                    ChotLuc = @Now,
                    PhienBan = PhienBan + 1,
                    NguoiCapNhat = @System,
                    NgayCapNhat = @Now
                WHERE NgayLam = CAST(@Day AS date) AND TrangThai = 0 AND DaXoa = 0;
                SELECT @@ROWCOUNT";

            QueryCommand cmd = NewCmd(sql);
            cmd.AddParameter("@Day", day.Date, DbType.DateTime);
            cmd.AddParameter("@System", system, DbType.String);
            cmd.AddParameter("@Now", now, DbType.DateTime);
            object o = DataService.ExecuteScalar(cmd);
            return o == null ? 0 : Convert.ToInt32(o);
        }

        /// <summary>Gắn hạn chót và phân loại đúng hạn (1) / trễ hạn (2) cho các dòng chi tiết của ngày đã đóng.</summary>
        public int ClassifyClosedEntriesByDay(DateTime day, string system, DateTime now)
        {
            const string sql = @"
                UPDATE ct
                SET HanChot = ISNULL(CAST(t.NgayKetThuc AS date), ca.NgayLam),
                    PhanLoai = CASE WHEN t.NgayKetThuc IS NOT NULL
                                     AND ca.NgayLam > CAST(t.NgayKetThuc AS date) THEN 2 ELSE 1 END,
                    NguoiCapNhat = @System,
                    NgayCapNhat = @Now
                FROM dbo.TblKhaiBaoGioChiTiet ct
                INNER JOIN dbo.TblKhaiBaoGioCa ca ON ca.IdKhaiBaoCa = ct.IdKhaiBaoCa
                INNER JOIN dbo.TblCongViec t ON t.IdCongViec = ct.IdCongViec
                WHERE ca.NgayLam = CAST(@Day AS date)
                  AND ca.TrangThai = 1 AND ca.DaXoa = 0
                  AND ct.DaXoa = 0 AND ct.PhanLoai IS NULL;
                SELECT @@ROWCOUNT";

            QueryCommand cmd = NewCmd(sql);
            cmd.AddParameter("@Day", day.Date, DbType.DateTime);
            cmd.AddParameter("@System", system, DbType.String);
            cmd.AddParameter("@Now", now, DbType.DateTime);
            object o = DataService.ExecuteScalar(cmd);
            return o == null ? 0 : Convert.ToInt32(o);
        }

        /// <summary>Tạo 1 dòng "Không khai báo" (LoaiCa = 1, KetQua = 2) cho nhân viên có task trong ngày nhưng không bấm giờ.</summary>
        public int InsertMissedDayShifts(DateTime day, string gioBatDau, string gioKetThuc,
            int soPhutCa, byte projectActive, string system, DateTime now)
        {
            string sql = @"
                INSERT INTO dbo.TblKhaiBaoGioCa
                    (IdKhaiBaoCa, UserId, NgayLam, LoaiCa, GioBatDau, GioKetThuc, SoPhutCa,
                     TongPhut, TrangThai, KetQua, PhienBan, ChotLuc, NguoiTao, NgayTao)
                SELECT NEWID(), u.UserId, CAST(@Day AS date), 1, @GioBatDau, @GioKetThuc, @SoPhutCa,
                       0, 1, 2, 1, @Now, @System, @Now
                FROM dbo.aspnet_Users u
                WHERE ISNULL(u.LaNhanVien, 0) = 1
                  AND ISNULL(u.IsDeleted, 0) = 0
                  AND ISNULL(u.IsActivated, 0) = 1
                  AND u.UserName NOT LIKE N'EMP[_]%'
                  AND (u.NgayGiaNhap IS NULL OR CAST(u.NgayGiaNhap AS date) <= CAST(@Day AS date))
                  AND NOT EXISTS (SELECT 1 FROM dbo.TblKhaiBaoGioCa ca
                                  WHERE ca.UserId = u.UserId AND ca.NgayLam = CAST(@Day AS date)
                                    AND ca.DaXoa = 0)
                  AND EXISTS (
                      SELECT 1 FROM dbo.TblCongViec t
                      INNER JOIN dbo.TblDuAn d ON d.IdDuAn = t.IdDuAn
                      INNER JOIN dbo.TblCongViec_NhanVien cn ON cn.IdCongViec = t.IdCongViec
                      WHERE cn.IdNhanVien = u.UserId AND " + WorkableConditions + @"
                  );
                SELECT @@ROWCOUNT";

            QueryCommand cmd = NewCmd(sql);
            cmd.AddParameter("@Day", day.Date, DbType.DateTime);
            cmd.AddParameter("@GioBatDau", gioBatDau, DbType.String);
            cmd.AddParameter("@GioKetThuc", gioKetThuc, DbType.String);
            cmd.AddParameter("@SoPhutCa", soPhutCa, DbType.Int16);
            cmd.AddParameter("@ProjectActive", projectActive, DbType.Byte);
            cmd.AddParameter("@System", system, DbType.String);
            cmd.AddParameter("@Now", now, DbType.DateTime);
            object o = DataService.ExecuteScalar(cmd);
            return o == null ? 0 : Convert.ToInt32(o);
        }

        // ====================== Tiện ích ======================

        private static string BuildGuidList(IEnumerable<Guid> ids)
        {
            if (ids == null) return string.Empty;
            return string.Join(",", ids.Distinct().Select(g => "'" + g.ToString("D") + "'").ToArray());
        }
    }
}

// ============================================================================
// KHAI BAO GIO 2.0 — BO SUNG VAO CUNG FILE, KHONG XOA API 1.0.
// - 1.0 phuc vu trang WebForms cu, chua ket noi UI 2.0.
// - 2.0 la API moi, doc/ghi dung 3 bang V2; khong dual-write 3 bang cu.
// ============================================================================
namespace SweetSoft.QLDA.Core.Respositories
{
    // Repo moi KHONG ghi 3 bang KBG cu. SqlConnection/SqlTransaction dung chung
    // trong tung thao tac de tranh TransactionScope mo nhieu connection -> MSDTC.
    internal sealed class KhaiBaoGio20Repository
    {
        internal class CongViec
        {
            internal Guid IdCongViec, IdDuAn;
            internal Guid? IdCongViecCha, IdCongViecPhuThuoc;
            internal string MaCongViec, TenCongViec;
            internal DateTime? NgayBatDau, NgayKetThuc;
            internal int TrangThai;
            internal bool ChoDuyet;
        }
        internal readonly string ChuoiKetNoi;
        internal KhaiBaoGio20Repository()
        {
            string ten = ConfigurationManager.AppSettings["KhaiBaoGio20.TenKetNoi"];
            if (string.IsNullOrWhiteSpace(ten))
                throw new ConfigurationErrorsException("Thieu appSettings KhaiBaoGio20.TenKetNoi trong web.config (ten connection string SQL Server dang dung). ");
            ConnectionStringSettings cfg = ConfigurationManager.ConnectionStrings[ten];
            if (cfg == null || string.IsNullOrWhiteSpace(cfg.ConnectionString))
                throw new ConfigurationErrorsException("Khong tim thay connectionStrings[" + ten + "].");
            ChuoiKetNoi = cfg.ConnectionString;
        }
        internal SqlConnection MoKetNoi()
        {
            var conn = new SqlConnection(ChuoiKetNoi);
            conn.Open();
            return conn;
        }
        internal static SqlCommand Lenh(SqlConnection conn, SqlTransaction tx, string sql)
        {
            var cmd = new SqlCommand(sql, conn, tx) { CommandTimeout = 45 };
            return cmd;
        }
        internal static SqlParameter P(SqlCommand cmd, string name, SqlDbType type, object value)
        {
            return cmd.Parameters.Add(new SqlParameter(name, type) { Value = value ?? DBNull.Value });
        }
        internal static DateTime Utc(DateTime v) { return DateTime.SpecifyKind(v, DateTimeKind.Utc); }
        internal static DateTime? UtcOrNull(IDataRecord r, int i) { return r.IsDBNull(i) ? (DateTime?)null : Utc(r.GetDateTime(i)); }
        internal CongViec LayCongViec(SqlConnection con, SqlTransaction tx, Guid id)
        {
            using (var cmd = Lenh(con, tx, @"SELECT IdCongViec,IdDuAn,IdCongViecCha,IdCongViecPhuThuoc,
                 MaCongViec,TenCongViec,NgayBatDau,NgayKetThuc,TrangThai,ISNULL(ChoDuyet,0)
                 FROM dbo.TblCongViec WITH (UPDLOCK, HOLDLOCK)
                 WHERE IdCongViec=@Id AND (DaXoa=0 OR DaXoa IS NULL)"))
            {
                P(cmd, "@Id", SqlDbType.UniqueIdentifier, id);
                using (var rd = cmd.ExecuteReader())
                {
                    if (!rd.Read()) return null;
                    return new CongViec
                    {
                        IdCongViec = rd.GetGuid(0),
                        IdDuAn = rd.GetGuid(1),
                        IdCongViecCha = rd.IsDBNull(2) ? (Guid?)null : rd.GetGuid(2),
                        IdCongViecPhuThuoc = rd.IsDBNull(3) ? (Guid?)null : rd.GetGuid(3),
                        MaCongViec = rd.IsDBNull(4) ? "" : rd.GetString(4),
                        TenCongViec = rd.IsDBNull(5) ? "" : rd.GetString(5),
                        NgayBatDau = rd.IsDBNull(6) ? (DateTime?)null : rd.GetDateTime(6),
                        NgayKetThuc = rd.IsDBNull(7) ? (DateTime?)null : rd.GetDateTime(7),
                        TrangThai = Convert.ToInt32(rd.GetValue(8)),
                        ChoDuyet = rd.GetBoolean(9)
                    };
                }
            }
        }
        internal bool CoPhanCong(SqlConnection con, SqlTransaction tx, Guid idTask, Guid idNhanVien)
        {
            using (var cmd = Lenh(con, tx, @"SELECT COUNT(1) FROM dbo.TblCongViec_NhanVien WITH (UPDLOCK,HOLDLOCK)
                                                 WHERE IdCongViec=@T AND IdNhanVien=@U"))
            {
                P(cmd, "@T", SqlDbType.UniqueIdentifier, idTask);
                P(cmd, "@U", SqlDbType.UniqueIdentifier, idNhanVien);
                return Convert.ToInt32(cmd.ExecuteScalar()) != 0;
            }
        }
        internal List<Guid> LayNguoiPhanCong(SqlConnection con, SqlTransaction tx, Guid idTask)
        {
            var a = new List<Guid>();
            using (var cmd = Lenh(con, tx, @"SELECT cn.IdNhanVien FROM dbo.TblCongViec_NhanVien cn WITH (HOLDLOCK)
              INNER JOIN dbo.aspnet_Users u ON cn.IdNhanVien=u.UserId
              WHERE cn.IdCongViec=@T AND ISNULL(u.IsDeleted,0)=0 ORDER BY cn.IdNhanVien"))
            {
                P(cmd, "@T", SqlDbType.UniqueIdentifier, idTask);
                using (var rd = cmd.ExecuteReader()) while (rd.Read()) a.Add(rd.GetGuid(0));
            }
            return a;
        }
        internal bool DaTungBatDau(SqlConnection con, SqlTransaction tx, Guid idNv, Guid idTask)
        {
            using (var cmd = Lenh(con, tx, @"SELECT TOP 1 1 FROM dbo.TblKhaiBaoGioPhien WITH (UPDLOCK,HOLDLOCK)
                                       WHERE IdNhanVien=@U AND IdCongViec=@T AND LoaiGio=1"))
            {
                P(cmd, "@U", SqlDbType.UniqueIdentifier, idNv);
                P(cmd, "@T", SqlDbType.UniqueIdentifier, idTask);
                return cmd.ExecuteScalar() != null;
            }
        }
        internal Kbg20Phien LayPhienMo(SqlConnection con, SqlTransaction tx, Guid idNv, Guid idTask, Kbg20LoaiGio loai)
        {
            using (var cmd = Lenh(con, tx, @"SELECT TOP 1 IdPhien,IdNhanVien,IdCongViec,IdDuAn,LoaiGio,
              BatDauLuc,KetThucLuc,DaTinhDenLuc,LaTiepTucQuaHan,NguonBatDau
              FROM dbo.TblKhaiBaoGioPhien WITH (UPDLOCK,HOLDLOCK)
              WHERE IdNhanVien=@U AND IdCongViec=@T AND LoaiGio=@L AND KetThucLuc IS NULL"))
            {
                P(cmd, "@U", SqlDbType.UniqueIdentifier, idNv);
                P(cmd, "@T", SqlDbType.UniqueIdentifier, idTask);
                P(cmd, "@L", SqlDbType.TinyInt, (byte)loai);
                using (var rd = cmd.ExecuteReader()) return rd.Read() ? DocPhien(rd) : null;
            }
        }
        internal Kbg20Phien LayPhienSoBo(SqlConnection con, SqlTransaction tx, Guid idPhien, Guid idNv)
        {
            using (var cmd = Lenh(con, tx, @"SELECT IdPhien,IdNhanVien,IdCongViec,IdDuAn,LoaiGio,
              BatDauLuc,KetThucLuc,DaTinhDenLuc,LaTiepTucQuaHan,NguonBatDau
              FROM dbo.TblKhaiBaoGioPhien WHERE IdPhien=@I AND IdNhanVien=@U AND KetThucLuc IS NULL"))
            {
                P(cmd, "@I", SqlDbType.UniqueIdentifier, idPhien);
                P(cmd, "@U", SqlDbType.UniqueIdentifier, idNv);
                using (var rd = cmd.ExecuteReader()) return rd.Read() ? DocPhien(rd) : null;
            }
        }
        internal Kbg20Phien LayPhienMoTheoId(SqlConnection con, SqlTransaction tx, Guid idPhien, Guid idNv)
        {
            using (var cmd = Lenh(con, tx, @"SELECT IdPhien,IdNhanVien,IdCongViec,IdDuAn,LoaiGio,
              BatDauLuc,KetThucLuc,DaTinhDenLuc,LaTiepTucQuaHan,NguonBatDau
              FROM dbo.TblKhaiBaoGioPhien WITH (UPDLOCK,HOLDLOCK)
              WHERE IdPhien=@I AND IdNhanVien=@U AND KetThucLuc IS NULL"))
            {
                P(cmd, "@I", SqlDbType.UniqueIdentifier, idPhien);
                P(cmd, "@U", SqlDbType.UniqueIdentifier, idNv);
                using (var rd = cmd.ExecuteReader()) return rd.Read() ? DocPhien(rd) : null;
            }
        }
        internal static Kbg20Phien DocPhien(IDataRecord rd)
        {
            return new Kbg20Phien
            {
                IdPhien = rd.GetGuid(0),
                IdNhanVien = rd.GetGuid(1),
                IdCongViec = rd.GetGuid(2),
                IdDuAn = rd.GetGuid(3),
                LoaiGio = (Kbg20LoaiGio)Convert.ToByte(rd.GetValue(4)),
                BatDauLucUtc = Utc(rd.GetDateTime(5)),
                KetThucLucUtc = UtcOrNull(rd, 6),
                DaTinhDenLucUtc = UtcOrNull(rd, 7),
                LaTiepTucQuaHan = rd.GetBoolean(8),
                NguonBatDau = Convert.ToByte(rd.GetValue(9))
            };
        }
        internal Guid TaoPhien(SqlConnection con, SqlTransaction tx, Guid nv, CongViec task, Kbg20LoaiGio loai,
            DateTime batDauUtc, Kbg20NguonBatDau nguon, Guid? idKichHoat, Guid? nguoiKichHoat, Guid idThaoTac, bool quaHan)
        {
            Guid phien = Guid.NewGuid();
            using (var cmd = Lenh(con, tx, @"INSERT INTO dbo.TblKhaiBaoGioPhien
              (IdPhien,IdNhanVien,IdCongViec,IdDuAn,LoaiGio,BatDauLuc,DaTinhDenLuc,
               NguonBatDau,IdCongViecKichHoat,IdNguoiKichHoat,IdThaoTac,LaTiepTucQuaHan,NguoiTao)
              VALUES (@I,@U,@T,@D,@L,@B,@B,@N,@K,@G,@O,@Q,N'KhaiBaoGio20')"))
            {
                P(cmd, "@I", SqlDbType.UniqueIdentifier, phien);
                P(cmd, "@U", SqlDbType.UniqueIdentifier, nv);
                P(cmd, "@T", SqlDbType.UniqueIdentifier, task.IdCongViec);
                P(cmd, "@D", SqlDbType.UniqueIdentifier, task.IdDuAn);
                P(cmd, "@L", SqlDbType.TinyInt, (byte)loai);
                P(cmd, "@B", SqlDbType.DateTime2, batDauUtc);
                P(cmd, "@N", SqlDbType.TinyInt, (byte)nguon);
                P(cmd, "@K", SqlDbType.UniqueIdentifier, idKichHoat.HasValue ? (object)idKichHoat.Value : DBNull.Value);
                P(cmd, "@G", SqlDbType.UniqueIdentifier, nguoiKichHoat.HasValue ? (object)nguoiKichHoat.Value : DBNull.Value);
                P(cmd, "@O", SqlDbType.UniqueIdentifier, idThaoTac);
                P(cmd, "@Q", SqlDbType.Bit, quaHan);
                cmd.ExecuteNonQuery();
            }
            return phien;
        }
        internal void ChuyenTrangThaiDangLam(SqlConnection con, SqlTransaction tx, Guid taskId, DateTime nowLocal)
        {
            using (var cmd = Lenh(con, tx, @"UPDATE dbo.TblCongViec SET TrangThai=1,NgayCapNhat=@Now,
                NguoiCapNhat=N'KhaiBaoGio20' WHERE IdCongViec=@T AND TrangThai=0
                AND ISNULL(ChoDuyet,0)=0 AND (DaXoa=0 OR DaXoa IS NULL)"))
            {
                P(cmd, "@Now", SqlDbType.DateTime, nowLocal);
                P(cmd, "@T", SqlDbType.UniqueIdentifier, taskId);
                cmd.ExecuteNonQuery();
            }
        }
        internal void TaoThongBaoWeb(SqlConnection con, SqlTransaction tx, Guid nv, CongViec task, CongViec conTask, DateTime nowLocal)
        {
            // KHONG goi ThongBaoManager.Create (ham do se gui Email).
            using (var cmd = Lenh(con, tx, @"INSERT INTO dbo.TblThongBao
              (IdThongBao,UserId,IdCongViec,IdDuAn,TieuDe,NoiDung,LoaiThongBao,
               DuongDanLienKet,DaDoc,DaXoa,NguoiTao,NgayTao)
              VALUES (NEWID(),@U,@T,@D,@Title,@Body,'CONG_VIEC',NULL,0,0,N'KhaiBaoGio20',@Now)"))
            {
                P(cmd, "@U", SqlDbType.UniqueIdentifier, nv);
                P(cmd, "@T", SqlDbType.UniqueIdentifier, task.IdCongViec);
                P(cmd, "@D", SqlDbType.UniqueIdentifier, task.IdDuAn);
                P(cmd, "@Title", SqlDbType.NVarChar, "Đã tự bắt đầu đồng hồ giám sát: " + task.MaCongViec);
                P(cmd, "@Body", SqlDbType.NVarChar, "Hệ thống đã bắt đầu tính giờ công việc " + task.MaCongViec +
                     " (" + task.TenCongViec + ") vì công việc con " + conTask.MaCongViec + " được bắt đầu.");
                P(cmd, "@Now", SqlDbType.DateTime, nowLocal);
                cmd.ExecuteNonQuery();
            }
        }
        internal void KhoaTheoDuAn(SqlConnection con, SqlTransaction tx, Guid duAn)
        {
            using (var cmd = Lenh(con, tx, @"DECLARE @r INT;
             EXEC @r=sp_getapplock @Resource=@R, @LockMode='Exclusive',@LockOwner='Transaction',@LockTimeout=15000;
             IF @r<0 THROW 51020,N'Khong lay duoc khoa giao dich khai bao gio.',1;"))
            {
                P(cmd, "@R", SqlDbType.NVarChar, "KBG20:DUAN:" + duAn.ToString("N"));
                cmd.ExecuteNonQuery();
            }
        }
        internal List<Kbg20Phien> LayDanhSachPhienMo(int limit)
        {
            var list = new List<Kbg20Phien>();
            using (var con = MoKetNoi()) using (var cmd = Lenh(con, null, @"SELECT TOP (@N) IdPhien,IdNhanVien,IdCongViec,IdDuAn,LoaiGio,
              BatDauLuc,KetThucLuc,DaTinhDenLuc,LaTiepTucQuaHan,NguonBatDau
              FROM dbo.TblKhaiBaoGioPhien WHERE KetThucLuc IS NULL ORDER BY ISNULL(DaTinhDenLuc,BatDauLuc),IdPhien"))
            {
                P(cmd, "@N", SqlDbType.Int, limit);
                using (var rd = cmd.ExecuteReader()) while (rd.Read()) list.Add(DocPhien(rd));
            }
            return list;
        }
        // Idempotency: cung IdThaoTac cua mot lan bam khong duoc tao lai phien
        // ke ca khi phien truoc da dung. Dung chung transaction/app lock.
        internal Kbg20Phien LayPhienTheoThaoTac(SqlConnection con, SqlTransaction tx,
            Guid idThaoTac, Guid nv, Guid task, Kbg20LoaiGio loai)
        {
            using (var cmd = Lenh(con, tx, @"SELECT TOP 1 IdPhien,IdNhanVien,IdCongViec,IdDuAn,LoaiGio,
                BatDauLuc,KetThucLuc,DaTinhDenLuc,LaTiepTucQuaHan,NguonBatDau
                FROM dbo.TblKhaiBaoGioPhien WITH (HOLDLOCK)
                WHERE IdThaoTac=@O AND IdNhanVien=@U AND IdCongViec=@T AND LoaiGio=@L"))
            {
                P(cmd, "@O", SqlDbType.UniqueIdentifier, idThaoTac);
                P(cmd, "@U", SqlDbType.UniqueIdentifier, nv);
                P(cmd, "@T", SqlDbType.UniqueIdentifier, task);
                P(cmd, "@L", SqlDbType.TinyInt, (byte)loai);
                using (var rd = cmd.ExecuteReader()) return rd.Read() ? DocPhien(rd) : null;
            }
        }
        // Phong tru khi trang thai/phan cong thay doi ngoai luong khai bao gio.
        // Nguon chinh van can hook chot tai THOI DIEM thay doi phan cong/submit.
        internal Kbg20LyDoKetThuc? LayLyDoDongBatBuoc(SqlConnection con, SqlTransaction tx,
            Guid idTask, Guid idNhanVien)
        {
            using (var cmd = Lenh(con, tx, @"SELECT
                 t.TrangThai, ISNULL(t.ChoDuyet,0), ISNULL(t.DaXoa,0),
                 ISNULL(d.DaXoa,0), d.TrangThai,
                 CASE WHEN EXISTS (SELECT 1 FROM dbo.TblCongViec_NhanVien cn
                   WHERE cn.IdCongViec=@T AND cn.IdNhanVien=@U) THEN 1 ELSE 0 END
                FROM dbo.TblCongViec t
                INNER JOIN dbo.TblDuAn d ON d.IdDuAn=t.IdDuAn
                WHERE t.IdCongViec=@T"))
            {
                P(cmd, "@T", SqlDbType.UniqueIdentifier, idTask);
                P(cmd, "@U", SqlDbType.UniqueIdentifier, idNhanVien);
                using (var rd = cmd.ExecuteReader())
                {
                    if (!rd.Read()) return Kbg20LyDoKetThuc.CongViecBiKhoa;
                    if (Convert.ToInt32(rd.GetValue(5)) == 0) return Kbg20LyDoKetThuc.GoPhanCong;
                    // 2 = hoan thanh dung han; 3 = hoan thanh tre (khong phai huy).
                    if (Convert.ToInt32(rd.GetValue(0)) == 2 || Convert.ToInt32(rd.GetValue(0)) == 3 ||
                        rd.GetBoolean(1) || rd.GetBoolean(2) || rd.GetBoolean(3) ||
                        Convert.ToInt32(rd.GetValue(4)) !=
                            (int)SweetSoft.QLDA.Core.EnumHelper.Defines.DuAnStatus.DangThucHien)
                        return Kbg20LyDoKetThuc.CongViecBiKhoa;
                }
            }
            return null;
        }
        internal List<Kbg20Phien> LayPhienTheoNhanVien(Guid nv)
        {
            var list = new List<Kbg20Phien>();
            using (var con = MoKetNoi()) using (var cmd = Lenh(con, null, @"SELECT p.IdPhien,p.IdNhanVien,p.IdCongViec,p.IdDuAn,p.LoaiGio,
              p.BatDauLuc,p.KetThucLuc,p.DaTinhDenLuc,p.LaTiepTucQuaHan,p.NguonBatDau,
              t.MaCongViec,t.TenCongViec,d.MaDuAn,d.TenDuAn,
              (SELECT ISNULL(SUM(k.SoGiay),0) FROM dbo.TblKhaiBaoGioKhoangTinh k WHERE k.IdPhien=p.IdPhien)
              FROM dbo.TblKhaiBaoGioPhien p INNER JOIN dbo.TblCongViec t ON t.IdCongViec=p.IdCongViec
              INNER JOIN dbo.TblDuAn d ON d.IdDuAn=p.IdDuAn
              WHERE p.IdNhanVien=@U AND p.KetThucLuc IS NULL ORDER BY p.BatDauLuc"))
            {
                P(cmd, "@U", SqlDbType.UniqueIdentifier, nv);
                using (var rd = cmd.ExecuteReader()) while (rd.Read())
                    {
                        var p = DocPhien(rd);
                        p.MaCongViec = rd.IsDBNull(10) ? "" : rd.GetString(10);
                        p.TenCongViec = rd.IsDBNull(11) ? "" : rd.GetString(11);
                        p.MaDuAn = rd.IsDBNull(12) ? "" : rd.GetString(12);
                        p.TenDuAn = rd.IsDBNull(13) ? "" : rd.GetString(13);
                        p.SoGiayDaKetToan = Convert.ToInt64(rd.GetValue(14));
                        list.Add(p);
                    }
            }
            return list;
        }
        internal List<Kbg20Phien> LayPhienMoCuaTask(SqlConnection con, SqlTransaction tx, Guid task)
        {
            var list = new List<Kbg20Phien>();
            using (var cmd = Lenh(con, tx, @"SELECT IdPhien,IdNhanVien,IdCongViec,IdDuAn,LoaiGio,
               BatDauLuc,KetThucLuc,DaTinhDenLuc,LaTiepTucQuaHan,NguonBatDau
               FROM dbo.TblKhaiBaoGioPhien WITH(UPDLOCK,HOLDLOCK)
               WHERE IdCongViec=@T AND KetThucLuc IS NULL"))
            {
                P(cmd, "@T", SqlDbType.UniqueIdentifier, task);
                using (var rd = cmd.ExecuteReader()) while (rd.Read()) list.Add(DocPhien(rd));
            }
            return list;
        }
        internal void GhiKhoang(SqlConnection con, SqlTransaction tx, Guid phien, DateTime ngayLocal, DateTime dauUtc, DateTime cuoiUtc, string moTa)
        {
            long giay = (long)(cuoiUtc - dauUtc).TotalSeconds;
            if (giay <= 0) return;
            using (var cmd = Lenh(con, tx, @"INSERT INTO dbo.TblKhaiBaoGioKhoangTinh
              (IdKhoangTinh,IdPhien,NgayLam,BatDauLuc,KetThucLuc,SoGiay,MoTaNguonLich)
              VALUES (NEWID(),@I,@Day,@B,@E,@S,@M)"))
            {
                P(cmd, "@I", SqlDbType.UniqueIdentifier, phien);
                P(cmd, "@Day", SqlDbType.Date, ngayLocal.Date);
                P(cmd, "@B", SqlDbType.DateTime2, dauUtc);
                P(cmd, "@E", SqlDbType.DateTime2, cuoiUtc);
                P(cmd, "@S", SqlDbType.BigInt, giay);
                P(cmd, "@M", SqlDbType.NVarChar, moTa);
                cmd.ExecuteNonQuery();
            }
        }
        internal void CapNhatDaTinh(SqlConnection con, SqlTransaction tx, Guid phien, DateTime nowUtc)
        {
            using (var cmd = Lenh(con, tx, @"UPDATE dbo.TblKhaiBaoGioPhien SET DaTinhDenLuc=@Now
                             WHERE IdPhien=@I AND (DaTinhDenLuc IS NULL OR DaTinhDenLuc<=@Now)"))
            {
                P(cmd, "@I", SqlDbType.UniqueIdentifier, phien);
                P(cmd, "@Now", SqlDbType.DateTime2, nowUtc);
                cmd.ExecuteNonQuery();
            }
        }
        internal void DongPhien(SqlConnection con, SqlTransaction tx, Guid phien, DateTime ketThucUtc, Kbg20LyDoKetThuc lyDo)
        {
            using (var cmd = Lenh(con, tx, @"UPDATE dbo.TblKhaiBaoGioPhien SET KetThucLuc=@End,
                       LyDoKetThuc=@Reason,NgayCapNhat=SYSUTCDATETIME(),NguoiCapNhat=N'KhaiBaoGio20'
                       WHERE IdPhien=@I AND KetThucLuc IS NULL"))
            {
                P(cmd, "@I", SqlDbType.UniqueIdentifier, phien);
                P(cmd, "@End", SqlDbType.DateTime2, ketThucUtc);
                P(cmd, "@Reason", SqlDbType.TinyInt, (byte)lyDo);
                if (cmd.ExecuteNonQuery() != 1) throw new InvalidOperationException("Phien dong ho da duoc dong boi thao tac khac.");
            }
        }
        internal bool ConHauDueChuaHoanThanh(SqlConnection con, SqlTransaction tx, Guid task)
        {
            using (var cmd = Lenh(con, tx, @";WITH H AS
                  (SELECT IdCongViec,IdCongViecCha,TrangThai,DaXoa FROM dbo.TblCongViec WHERE IdCongViecCha=@T
                   UNION ALL SELECT c.IdCongViec,c.IdCongViecCha,c.TrangThai,c.DaXoa
                   FROM dbo.TblCongViec c INNER JOIN H ON c.IdCongViecCha=H.IdCongViec)
                  SELECT TOP 1 1 FROM H WHERE (DaXoa=0 OR DaXoa IS NULL) AND TrangThai NOT IN (2,3)
                  OPTION (MAXRECURSION 100)"))
            {
                P(cmd, "@T", SqlDbType.UniqueIdentifier, task);
                return cmd.ExecuteScalar() != null;
            }
        }
    }
}
