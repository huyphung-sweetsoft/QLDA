using SubSonic;
using SweetSoft.QLDA.Core.Infrastructure;
using SweetSoft.QLDA.Core.SysManager;
using SweetSoft.QLDA.DataAccess;
using System;
using System.Data;

namespace SweetSoft.QLDA.Core.Managers
{
    public class NotificationJob
    {
        // SubSonic của dự án chưa nhất thiết regenerate TblLichHop sau khi thêm LaHopOnline.
        // Đọc bằng SQL như trang MeetList, chỉ dùng GUID sinh từ phía server.
        private static bool IsMeetingOnline(Guid meetingId)
        {
            try
            {
                string sql = "SELECT TOP 1 LaHopOnline FROM dbo.TblLichHop WHERE IdLichHop='" + meetingId.ToString("D") + "'";
                using (IDataReader reader = new InlineQuery().ExecuteReader(sql))
                {
                    return reader.Read() && reader["LaHopOnline"] != DBNull.Value && Convert.ToBoolean(reader["LaHopOnline"]);
                }
            }
            catch (Exception ex)
            {
                SysLogger.LogError(ex, "Không đọc được LaHopOnline cho cuộc họp " + meetingId);
                return false; // Không tự gắn link nếu không xác định được họp online.
            }
        }

        public static void QuetVaGuiThongBao()
        {
            DateTime currentTime = DateTime.Now;

            // 1. Chỉ gửi nhắc họp trong khoảng: hiện tại < giờ bắt đầu <= hiện tại + 5 phút.
            // Hangfire nên chạy mỗi phút để không bỏ lỡ khoảng này.
            DateTime thoiGian5PhutToi = currentTime.AddMinutes(5);

            var lichHops = new Select().From(TblLichHop.Schema)
                .Where(TblLichHop.Columns.DaXoa).IsEqualTo(false)
                .AndExpression(TblLichHop.Columns.DaGuiNhacNho).IsNull()
                    .Or(TblLichHop.Columns.DaGuiNhacNho).IsEqualTo(false)
                .CloseExpression()
                .And(TblLichHop.Columns.ThoiGianBatDau).IsLessThanOrEqualTo(thoiGian5PhutToi)
                .And(TblLichHop.Columns.ThoiGianBatDau).IsGreaterThan(currentTime)
                .ExecuteAsCollection<TblLichHopCollection>();

            foreach (var hop in lichHops)
            {
                var danhSachThamGia = new Select().From(TblLichHopNhanVien.Schema)
                    .Where(TblLichHopNhanVien.Columns.IdLichHop).IsEqualTo(hop.IdLichHop)
                    .ExecuteAsCollection<TblLichHopNhanVienCollection>();

                bool isOnline = IsMeetingOnline(hop.IdLichHop);
                if (danhSachThamGia != null && danhSachThamGia.Count > 0)
                {
                    string tieuDe = $"Cuộc họp đã mở: {hop.TenCuocHop}";
                    string noiDung = $"Cuộc họp '{hop.TenCuocHop}' đã mở và sẽ bắt đầu lúc {hop.ThoiGianBatDau:HH:mm dd/MM/yyyy}. Bạn có thể tham gia ngay.";

                    foreach (var nguoiThamGia in danhSachThamGia)
                    {
                        if (nguoiThamGia.IdNhanVien == Guid.Empty) continue;
                        try
                        {
                            if (System.Web.Security.Membership.GetUser(nguoiThamGia.IdNhanVien) == null) continue;

                            ThongBaoManager.Instance.CreateMeetingReminderNotification(
                                userId: nguoiThamGia.IdNhanVien,
                                tieuDe: tieuDe,
                                noiDung: noiDung,
                                idDuAn: hop.IdDuAn,
                                meetingName: hop.TenCuocHop,
                                startTime: hop.ThoiGianBatDau,
                                room: hop.DiaDiemHop,
                                isOnline: isOnline
                            );
                        }
                        catch (Exception ex)
                        {
                            SysLogger.LogError(ex, "Lỗi ngầm khi tạo TB lịch họp cho UserId: " + nguoiThamGia.IdNhanVien);
                        }
                    }
                }

                // Giữ cơ chế đánh dấu đã gửi một lần của dự án.
                hop.DaGuiNhacNho = true;
                hop.Save();
            }

            // 2. Nhắc hạn công việc: GIỮ NGUYÊN nghiệp vụ hiện tại.
            DateTime thoiGian2NgayToi = currentTime.AddDays(2);
            var congViecs = new Select().From(TblCongViec.Schema)
                .Where(TblCongViec.Columns.TrangThai).IsNotEqualTo(2)
                .AndExpression(TblCongViec.Columns.DaGuiNhacNho).IsNull()
                    .Or(TblCongViec.Columns.DaGuiNhacNho).IsEqualTo(false)
                .CloseExpression()
                .And(TblCongViec.Columns.NgayKetThuc).IsNotNull()
                .And(TblCongViec.Columns.NgayKetThuc).IsLessThanOrEqualTo(thoiGian2NgayToi)
                .And(TblCongViec.Columns.DaXoa).IsEqualTo(false)
                .ExecuteAsCollection<TblCongViecCollection>();

            foreach (var task in congViecs)
            {
                var danhSachPhanCong = new Select().From(TblCongViecNhanVien.Schema)
                    .Where(TblCongViecNhanVien.Columns.IdCongViec).IsEqualTo(task.IdCongViec)
                    .ExecuteAsCollection<TblCongViecNhanVienCollection>();

                if (danhSachPhanCong != null && danhSachPhanCong.Count > 0)
                {
                    string tieuDe = $"Cảnh báo hạn chót: {task.MaCongViec}";
                    string noiDung = $"Công việc [{task.MaCongViec}] - {task.TenCongViec} sẽ hết hạn vào ngày {task.NgayKetThuc.Value:dd/MM/yyyy}. Vui lòng cập nhật tiến độ để tránh trễ hạn.";

                    foreach (var phanCong in danhSachPhanCong)
                    {
                        if (phanCong.IdNhanVien == Guid.Empty) continue;
                        try
                        {
                            if (System.Web.Security.Membership.GetUser(phanCong.IdNhanVien) == null) continue;
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
                            SysLogger.LogError(ex, "Lỗi ngầm khi tạo TB công việc cho UserId: " + phanCong.IdNhanVien);
                        }
                    }
                }
                task.DaGuiNhacNho = true;
                task.Save();
            }
        }
    }
}
