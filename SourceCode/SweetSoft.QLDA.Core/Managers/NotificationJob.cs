using SweetSoft.QLDA.Core.Infrastructure;
using SweetSoft.QLDA.Core.MailManager;
using SweetSoft.QLDA.Core.SysManager;
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

            // ========================================================
            // 1. QUÉT VÀ GỬI THÔNG BÁO LỊCH HỌP SẮP DIỄN RA
            // ========================================================
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
                            try
                            {
                                if (System.Web.Security.Membership.GetUser(nguoiThamGia.IdNhanVien) == null) continue;

                                // [ĐÃ SỬA] Gọi thẳng vào luồng riêng biệt, truyền đầy đủ Data để build Email
                                ThongBaoManager.Instance.CreateMeetingReminderNotification(
                                    userId: nguoiThamGia.IdNhanVien,
                                    tieuDe: tieuDe,
                                    noiDung: noiDung,
                                    idDuAn: hop.IdDuAn,
                                    meetingName: hop.TenCuocHop,
                                    startTime: hop.ThoiGianBatDau,
                                    room: hop.DiaDiemHop
                                );
                            }
                            catch (Exception ex)
                            {
                                SysLogger.LogError(ex, $"Lỗi ngầm khi tạo TB lịch họp cho UserId: {nguoiThamGia.IdNhanVien}");
                            }
                        }
                    }
                }

                hop.DaGuiNhacNho = true;
                hop.Save();
            }

            // ========================================================
            // 2. QUÉT VÀ GỬI THÔNG BÁO CÔNG VIỆC SẮP ĐẾN HẠN
            // ========================================================
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
                            try
                            {
                                if (System.Web.Security.Membership.GetUser(phanCong.IdNhanVien) == null) continue;

                                // [ĐÃ SỬA] Gọi thẳng vào luồng riêng biệt, truyền đầy đủ Data để build Email
                                ThongBaoManager.Instance.CreateTaskReminderNotification(
                                    userId: phanCong.IdNhanVien,
                                    tieuDe: tieuDe,
                                    noiDung: noiDung,
                                    idCongViec: task.IdCongViec,
                                    idDuAn: task.IdDuAn,
                                    taskCode: task.MaCongViec,
                                    taskName: task.TenCongViec,
                                    deadline: task.NgayKetThuc.Value
                                );
                            }
                            catch (Exception ex)
                            {
                                SysLogger.LogError(ex, $"Lỗi ngầm khi tạo TB công việc cho UserId: {phanCong.IdNhanVien}");
                            }
                        }
                    }
                }

                task.DaGuiNhacNho = true;
                task.Save();
            }
        }
    }
}