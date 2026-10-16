using SubSonic;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;

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