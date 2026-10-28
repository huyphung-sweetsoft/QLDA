using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;

namespace SweetSoft.QLDA.Core.Respositories
{
    // Chi thao tac voi phien IdVanDe, KHONG doc/sua phien Task.
    internal sealed class GioXuLyVanDeRepository
    {
        internal const byte LoaiGioXuLy = 3;
        // LaTangCaVanDe: NULL=Task, 0=gio xu ly thuong Issue, 1=tang ca Issue.
        private readonly string _connectionString;

        internal GioXuLyVanDeRepository()
        {
            // Dung CUNG database voi KhaiBaoGio hien tai; khong them config moi.
            string ten = ConfigurationManager.AppSettings["KhaiBaoGio20.TenKetNoi"];
            ConnectionStringSettings cfg = string.IsNullOrWhiteSpace(ten) ? null : ConfigurationManager.ConnectionStrings[ten];
            if (cfg == null || string.IsNullOrWhiteSpace(cfg.ConnectionString))
                throw new ConfigurationErrorsException("Chua cau hinh ket noi database KhaiBaoGio.");
            _connectionString = cfg.ConnectionString;
        }

        internal SqlConnection MoKetNoi()
        {
            var con = new SqlConnection(_connectionString);
            con.Open();
            return con;
        }

        internal static SqlCommand Lenh(SqlConnection con, SqlTransaction tx, string sql)
        {
            return new SqlCommand(sql, con, tx) { CommandTimeout = 45 };
        }

        internal static void P(SqlCommand cmd, string ten, SqlDbType kieu, object giaTri)
        {
            cmd.Parameters.Add(ten, kieu).Value = giaTri ?? DBNull.Value;
        }

        // Dung CUNG tai nguyen khoa du an cua Task/KhaiBaoGio (khong sinh lock domain moi).
        internal void KhoaDuAn(SqlConnection con, SqlTransaction tx, Guid duAn)
        {
            using (var cmd = Lenh(con, tx, @"DECLARE @Result int;
                EXEC @Result = sys.sp_getapplock @Resource=@R, @LockMode='Exclusive',
                     @LockOwner='Transaction', @LockTimeout=15000;
                IF @Result < 0 THROW 51020, N'Khong lay duoc khoa giao dich khai bao gio.', 1;"))
            {
                P(cmd, "@R", SqlDbType.NVarChar, "KBG20:DUAN:" + duAn.ToString("N"));
                cmd.ExecuteNonQuery();
            }
        }

        internal bool DuocPhepXuLy(SqlConnection con, SqlTransaction tx, Guid duAn, Guid vanDe, Guid nhanVien)
        {
            using (var cmd = Lenh(con, tx, @"SELECT CASE WHEN EXISTS (
                SELECT 1 FROM dbo.TblVanDe v WITH (UPDLOCK,HOLDLOCK)
                INNER JOIN dbo.TblDuAn d ON d.IdDuAn=v.IdDuAn
                WHERE v.IdVanDe=@V AND v.IdDuAn=@D AND ISNULL(v.DaXoa,0)=0
                  AND ISNULL(d.DaXoa,0)=0 AND v.TrangThai=0
                  AND ISNULL(v.TrangThaiDuyetGio,0) IN (0,3)
                  AND NOT EXISTS (SELECT 1 FROM dbo.TblKhaiBaoGioDoiSoat b
                      WHERE b.IdDuAn=@D AND b.IdVanDe=@V AND b.IdNhanVien=@U
                        AND b.LaBanHienTai=1 AND b.LoaiSuKien IN(1,3,4,6)
                        AND b.LanDoiSoat=(CASE WHEN v.TrangThaiDuyetGio=3 THEN v.LanGuiDuyetGio+1
                             WHEN v.LanGuiDuyetGio<1 THEN 1 ELSE v.LanGuiDuyetGio END))
                  AND EXISTS (SELECT 1 FROM dbo.TblVanDe_NhanVien a WITH (HOLDLOCK)
                              WHERE a.IdVanDe=v.IdVanDe AND a.IdNhanVien=@U)
            ) THEN 1 ELSE 0 END"))
            {
                P(cmd, "@V", SqlDbType.UniqueIdentifier, vanDe);
                P(cmd, "@D", SqlDbType.UniqueIdentifier, duAn);
                P(cmd, "@U", SqlDbType.UniqueIdentifier, nhanVien);
                return Convert.ToInt32(cmd.ExecuteScalar()) == 1;
            }
        }

        internal Guid? LayPhienTheoThaoTac(SqlConnection con, SqlTransaction tx, Guid duAn, Guid vanDe, Guid nhanVien, Guid thaoTac)
        {
            using (var cmd = Lenh(con, tx, @"SELECT TOP 1 IdPhien FROM dbo.TblKhaiBaoGioPhien WITH (UPDLOCK,HOLDLOCK)
                    WHERE IdDuAn=@D AND IdVanDe=@V AND IdNhanVien=@U AND IdThaoTac=@O
                      AND IdCongViec IS NULL AND LoaiGio=3"))
            {
                P(cmd, "@D", SqlDbType.UniqueIdentifier, duAn);
                P(cmd, "@V", SqlDbType.UniqueIdentifier, vanDe);
                P(cmd, "@U", SqlDbType.UniqueIdentifier, nhanVien);
                P(cmd, "@O", SqlDbType.UniqueIdentifier, thaoTac);
                object result = cmd.ExecuteScalar();
                return result == null || result == DBNull.Value ? (Guid?)null : (Guid)result;
            }
        }

        internal PhienGioVanDe LayPhienMo(SqlConnection con, SqlTransaction tx, Guid duAn, Guid vanDe, Guid? nhanVien)
        {
            using (var cmd = Lenh(con, tx, @"SELECT TOP 1 IdPhien,IdNhanVien,IdVanDe,IdDuAn,
                      BatDauLuc,DaTinhDenLuc,KetThucLuc,ISNULL(LaTangCaVanDe,0)
                 FROM dbo.TblKhaiBaoGioPhien WITH (UPDLOCK,HOLDLOCK)
                 WHERE IdDuAn=@D AND IdVanDe=@V AND IdCongViec IS NULL AND LoaiGio=3
                       AND KetThucLuc IS NULL AND (@U IS NULL OR IdNhanVien=@U)
                 ORDER BY BatDauLuc,IdPhien"))
            {
                P(cmd, "@D", SqlDbType.UniqueIdentifier, duAn);
                P(cmd, "@V", SqlDbType.UniqueIdentifier, vanDe);
                P(cmd, "@U", SqlDbType.UniqueIdentifier, nhanVien.HasValue ? (object)nhanVien.Value : DBNull.Value);
                using (var r = cmd.ExecuteReader())
                {
                    return r.Read() ? DocPhien(r) : null;
                }
            }
        }

        internal List<PhienGioVanDe> LayTatCaPhienMo(SqlConnection con, SqlTransaction tx, Guid duAn, Guid vanDe)
        {
            var result = new List<PhienGioVanDe>();
            using (var cmd = Lenh(con, tx, @"SELECT IdPhien,IdNhanVien,IdVanDe,IdDuAn,
                    BatDauLuc,DaTinhDenLuc,KetThucLuc,ISNULL(LaTangCaVanDe,0)
                 FROM dbo.TblKhaiBaoGioPhien WITH (UPDLOCK,HOLDLOCK)
                 WHERE IdDuAn=@D AND IdVanDe=@V AND IdCongViec IS NULL AND LoaiGio=3
                       AND KetThucLuc IS NULL ORDER BY IdPhien"))
            {
                P(cmd, "@D", SqlDbType.UniqueIdentifier, duAn);
                P(cmd, "@V", SqlDbType.UniqueIdentifier, vanDe);
                using (var r = cmd.ExecuteReader()) while (r.Read()) result.Add(DocPhien(r));
            }
            return result;
        }

        private static PhienGioVanDe DocPhien(IDataRecord r)
        {
            return new PhienGioVanDe {
                IdPhien=r.GetGuid(0), IdNhanVien=r.GetGuid(1), IdVanDe=r.GetGuid(2),
                IdDuAn=r.GetGuid(3), BatDauUtc=r.GetDateTime(4),
                DaTinhDenUtc=r.IsDBNull(5) ? (DateTime?)null : r.GetDateTime(5),
                KetThucUtc=r.IsDBNull(6) ? (DateTime?)null : r.GetDateTime(6),
                LaTangCa=Convert.ToBoolean(r.GetValue(7))
            };
        }

        internal Guid TaoPhien(SqlConnection con, SqlTransaction tx, Guid duAn, Guid vanDe, Guid nhanVien,
                               Guid thaoTac, DateTime batDauUtc, bool tangCa)
        {
            Guid id = Guid.NewGuid();
            using (var cmd = Lenh(con, tx, @"INSERT INTO dbo.TblKhaiBaoGioPhien
              (IdPhien,IdNhanVien,IdCongViec,IdVanDe,IdDuAn,LoaiGio,BatDauLuc,DaTinhDenLuc,
               NguonBatDau,IdThaoTac,LaTiepTucQuaHan,NguoiTao,LaTangCaVanDe)
              VALUES (@I,@U,NULL,@V,@D,3,@B,@B,1,@O,0,N'GioXuLyVanDe',@TangCa)"))
            {
                P(cmd, "@I", SqlDbType.UniqueIdentifier, id);
                P(cmd, "@U", SqlDbType.UniqueIdentifier, nhanVien);
                P(cmd, "@V", SqlDbType.UniqueIdentifier, vanDe);
                P(cmd, "@D", SqlDbType.UniqueIdentifier, duAn);
                P(cmd, "@B", SqlDbType.DateTime2, batDauUtc);
                P(cmd, "@O", SqlDbType.UniqueIdentifier, thaoTac);
                P(cmd,"@TangCa",SqlDbType.Bit,tangCa);
                if (cmd.ExecuteNonQuery()!=1) throw new InvalidOperationException("Không tạo được phiên xử lý vấn đề.");
            }
            return id;
        }

        internal void GhiKhoang(SqlConnection con, SqlTransaction tx, Guid idPhien,
                                DateTime ngayLam, DateTime tuUtc, DateTime denUtc)
        {
            long soGiay = (long)(denUtc-tuUtc).TotalSeconds;
            if (soGiay<=0) return;
            using (var cmd = Lenh(con, tx, @"INSERT INTO dbo.TblKhaiBaoGioKhoangTinh
                  (IdKhoangTinh,IdPhien,NgayLam,BatDauLuc,KetThucLuc,SoGiay,MoTaNguonLich)
                  VALUES (NEWID(),@I,@Ngay,@Tu,@Den,@Giay,N'Giờ xử lý vấn đề thực tế')"))
            {
                P(cmd,"@I",SqlDbType.UniqueIdentifier,idPhien);
                P(cmd,"@Ngay",SqlDbType.Date,ngayLam.Date);
                P(cmd,"@Tu",SqlDbType.DateTime2,tuUtc);
                P(cmd,"@Den",SqlDbType.DateTime2,denUtc);
                P(cmd,"@Giay",SqlDbType.BigInt,soGiay);
                if (cmd.ExecuteNonQuery()!=1) throw new InvalidOperationException("Không lưu được khoảng giờ xử lý.");
            }
        }

        internal void DongPhien(SqlConnection con, SqlTransaction tx, Guid phien, DateTime ketThucUtc, byte lyDo)
        {
            using (var cmd = Lenh(con, tx, @"UPDATE dbo.TblKhaiBaoGioPhien
                 SET DaTinhDenLuc=@End,KetThucLuc=@End,LyDoKetThuc=@Reason,
                     NgayCapNhat=SYSUTCDATETIME(),NguoiCapNhat=N'GioXuLyVanDe'
                 WHERE IdPhien=@I AND IdCongViec IS NULL AND IdVanDe IS NOT NULL
                   AND LoaiGio=3 AND KetThucLuc IS NULL AND DaTinhDenLuc<=@End"))
            {
                P(cmd,"@I",SqlDbType.UniqueIdentifier,phien);
                P(cmd,"@End",SqlDbType.DateTime2,ketThucUtc);
                P(cmd,"@Reason",SqlDbType.TinyInt,lyDo);
                if(cmd.ExecuteNonQuery()!=1) throw new InvalidOperationException("Phiên giờ đã thay đổi, vui lòng làm mới.");
            }
        }

        // Dot 3B: chi DOC nhung phien Issue con mo cua dung nhan vien, ke ca Issue da doi trang thai.
        // Khong join bang phan cong de tranh lam bien mat phien con mo sau khi doi phan cong.
        // SoGiayDaKetToan tinh theo IdPhien, KHONG lay tong cua ca Issue.
        internal List<PhienNoiVanDe> LayPhienMoChoDongHoNoi(Guid nhanVien)
        {
            var result = new List<PhienNoiVanDe>();
            if (nhanVien == Guid.Empty) return result;
            using (var con = MoKetNoi())
            using (var cmd = Lenh(con, null, @"SELECT p.IdPhien,p.IdVanDe,p.IdDuAn,
                     p.BatDauLuc,p.DaTinhDenLuc,ISNULL(p.LaTangCaVanDe,0),
                     ISNULL(v.MaVanDe,N''),ISNULL(v.TenVanDe,N''),
                     ISNULL(d.MaDuAn,N''),ISNULL(d.TenDuAn,N''),
                     ISNULL(k.SoGiay,0)
                FROM dbo.TblKhaiBaoGioPhien p
                LEFT JOIN dbo.TblVanDe v ON v.IdVanDe=p.IdVanDe AND v.IdDuAn=p.IdDuAn
                LEFT JOIN dbo.TblDuAn d ON d.IdDuAn=p.IdDuAn
                OUTER APPLY(SELECT SUM(x.SoGiay) SoGiay
                  FROM dbo.TblKhaiBaoGioKhoangTinh x WHERE x.IdPhien=p.IdPhien) k
                WHERE p.IdNhanVien=@U AND p.IdVanDe IS NOT NULL AND p.IdCongViec IS NULL
                      AND p.LoaiGio=3 AND p.KetThucLuc IS NULL
                ORDER BY p.IdDuAn,p.BatDauLuc,p.IdPhien"))
            {
                P(cmd, "@U", SqlDbType.UniqueIdentifier, nhanVien);
                using (var r = cmd.ExecuteReader())
                    while (r.Read()) result.Add(new PhienNoiVanDe {
                        IdPhien=r.GetGuid(0),IdVanDe=r.GetGuid(1),IdDuAn=r.GetGuid(2),
                        BatDauUtc=r.GetDateTime(3),
                        DaTinhDenUtc=r.IsDBNull(4)?(DateTime?)null:r.GetDateTime(4),
                        LaTangCa=Convert.ToBoolean(r.GetValue(5)),
                        MaVanDe=r.GetString(6),TenVanDe=r.GetString(7),
                        MaDuAn=r.GetString(8),TenDuAn=r.GetString(9),
                        SoGiayDaKetToan=Convert.ToInt64(r.GetValue(10))
                    });
            }
            return result;
        }

        internal long LayTongGiay(SqlConnection con, SqlTransaction tx, Guid duAn, Guid vanDe, Guid nhanVien)
        {
            using(var cmd=Lenh(con,tx,@"SELECT ISNULL(SUM(k.SoGiay),0)
                 FROM dbo.TblKhaiBaoGioKhoangTinh k
                 INNER JOIN dbo.TblKhaiBaoGioPhien p ON p.IdPhien=k.IdPhien
                 WHERE p.IdDuAn=@D AND p.IdVanDe=@V AND p.IdCongViec IS NULL
                       AND p.LoaiGio=3 AND p.IdNhanVien=@U"))
            {
                P(cmd,"@D",SqlDbType.UniqueIdentifier,duAn);
                P(cmd,"@V",SqlDbType.UniqueIdentifier,vanDe);
                P(cmd,"@U",SqlDbType.UniqueIdentifier,nhanVien);
                return Convert.ToInt64(cmd.ExecuteScalar());
            }
        }
    }

    // Du lieu noi bo giua Repository va Manager, khong expose SQL cho handler WebForms.
    internal sealed class PhienNoiVanDe
    {
        internal Guid IdPhien, IdVanDe, IdDuAn;
        internal DateTime BatDauUtc;
        internal DateTime? DaTinhDenUtc;
        internal bool LaTangCa;
        internal string MaVanDe, TenVanDe, MaDuAn, TenDuAn;
        internal long SoGiayDaKetToan;
    }

    internal sealed class PhienGioVanDe
    {
        internal Guid IdPhien;
        internal Guid IdNhanVien;
        internal Guid IdVanDe;
        internal Guid IdDuAn;
        internal DateTime BatDauUtc;
        internal DateTime? DaTinhDenUtc;
        internal DateTime? KetThucUtc;
        internal bool LaTangCa;
    }
}
