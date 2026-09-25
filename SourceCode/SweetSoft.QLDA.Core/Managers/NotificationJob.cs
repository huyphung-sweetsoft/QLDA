using SweetSoft.QLDA.Core.MailManager;
using SweetSoft.QLDA.DataAccess;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SweetSoft.QLDA.Core.Managers
{
    public class NotificationJob
    {
        public static void QuetVaGuiThongBao()
        {
            DateTime currentTime = DateTime.Now;

            DateTime thoiGian15PhutToi = currentTime.AddMinutes(15);

            var lichHops = new SubSonic.Select().From(TblLichHop.Schema)
                .Where(TblLichHop.Columns.DaGuiNhacNho).IsNull().Or(TblLichHop.Columns.DaGuiNhacNho).IsEqualTo(false)
                .And(TblLichHop.Columns.ThoiGianBatDau).IsLessThanOrEqualTo(thoiGian15PhutToi)
                .And(TblLichHop.Columns.ThoiGianBatDau).IsGreaterThan(currentTime)
                .And(TblLichHop.Columns.DaXoa).IsEqualTo(false)
                .ExecuteAsCollection<TblLichHopCollection>();

            foreach (var hop in lichHops)
            {
                var danhSachThamGia = new SubSonic.Select().From(TblLichHopNhanVien.Schema)
                    .Where(TblLichHopNhanVien.Columns.IdLichHop).IsEqualTo(hop.IdLichHop)
                    .ExecuteAsCollection<TblLichHopNhanVienCollection>();

                if (danhSachThamGia != null && danhSachThamGia.Count > 0)
                {
                    string tieuDe = $"Sắp đến giờ họp: {hop.TenCuocHop}";
                    string noiDung = $"Cuộc họp '{hop.TenCuocHop}' sẽ bắt đầu lúc {hop.ThoiGianBatDau:HH:mm dd/MM/yyyy}. Vui lòng tham gia đúng giờ.";

                    foreach (var nguoiThamGia in danhSachThamGia)
                    {
                        if (nguoiThamGia.IdNhanVien != Guid.Empty)
                        {
                            ThongBaoManager.Instance.Create(
                                userId: nguoiThamGia.IdNhanVien,
                                tieuDe: tieuDe,
                                noiDung: noiDung,
                                loaiThongBao: ThongBaoTypes.LichHop,
                                idCongViec: null,
                                idDuAn: hop.IdDuAn
                            );
                        }
                    }
                }

                hop.DaGuiNhacNho = true;
                hop.Save();
            }

            DateTime thoiGian2NgayToi = currentTime.AddDays(2);

            var congViecs = new SubSonic.Select().From(TblCongViec.Schema)
                .Where(TblCongViec.Columns.TrangThai).IsNotEqualTo(2)
                .AndExpression(TblCongViec.Columns.DaGuiNhacNho).IsNull().Or(TblCongViec.Columns.DaGuiNhacNho).IsEqualTo(false).CloseExpression()
                .And(TblCongViec.Columns.NgayKetThuc).IsNotNull()
                .And(TblCongViec.Columns.NgayKetThuc).IsLessThanOrEqualTo(thoiGian2NgayToi)
                .And(TblCongViec.Columns.DaXoa).IsEqualTo(false)
                .ExecuteAsCollection<TblCongViecCollection>();

            foreach (var task in congViecs)
            {
                var danhSachPhanCong = new SubSonic.Select().From(TblCongViecNhanVien.Schema)
                    .Where(TblCongViecNhanVien.Columns.IdCongViec).IsEqualTo(task.IdCongViec)
                    .ExecuteAsCollection<TblCongViecNhanVienCollection>();

                if (danhSachPhanCong != null && danhSachPhanCong.Count > 0)
                {
                    string tieuDe = $"Cảnh báo hạn chót: {task.MaCongViec}";
                    string noiDung = $"Công việc [{task.MaCongViec}] - {task.TenCongViec} sẽ hết hạn vào ngày {task.NgayKetThuc.Value:dd/MM/yyyy}. Vui lòng cập nhật tiến độ để tránh trễ hạn.";

                    foreach (var phanCong in danhSachPhanCong)
                    {
                        if (phanCong.IdNhanVien != Guid.Empty)
                        {
                            ThongBaoManager.Instance.Create(
                                userId: phanCong.IdNhanVien,
                                tieuDe: tieuDe,
                                noiDung: noiDung,
                                loaiThongBao: ThongBaoTypes.CongViec,
                                idCongViec: task.IdCongViec,
                                idDuAn: task.IdDuAn
                            );
                        }
                    }
                }

                task.DaGuiNhacNho = true;
                task.Save();
            }
        }
    }
}
