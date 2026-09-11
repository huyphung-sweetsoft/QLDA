using SweetSoft.QLDA.Core.Infrastructure.Interfaces;
using SweetSoft.QLDA.Core;
using SweetSoft.QLDA.Core.Respositories;
using SweetSoft.QLDA.Core.ScheduleManager;
using SweetSoft.QLDA.Core.SysManager;
using SweetSoft.QLDA.DataAccess;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using SweetSoft.QLDA.Core.Managers;

namespace SweetSoft.QLDA.Core.ScheduleManager
{
    public class LichTrinhManager : BaseManager
    {
        private static readonly Lazy<LichTrinhManager> _instance = new Lazy<LichTrinhManager>(() => new LichTrinhManager());
        public static LichTrinhManager Instance => _instance.Value;

        private readonly AuditManager _auditManager;
        private readonly CauHinhTuanLamViecRepository _tuanRepository;
        private readonly LichNgoaiLeRepository _ngoaiLeRepository;

        public LichTrinhManager(IAppContext applicationContext = null) : base(applicationContext)
        {
            _auditManager = new AuditManager(GetClientInfo());
            _tuanRepository = new CauHinhTuanLamViecRepository(_auditManager);
            _ngoaiLeRepository = new LichNgoaiLeRepository(_auditManager);
        }

        public List<ThongTinLichNgay> LayLichTrinhNhanVien(Guid idNhanVien, DateTime startDate, DateTime endDate)
        {
            List<ThongTinLichNgay> ketQua = new List<ThongTinLichNgay>();

            // 1. Lấy dữ liệu cấu hình Lịch chung (Ngoại lệ & Mặc định)
            var lichNgoaiLes = _ngoaiLeRepository.GetExceptionsInRange(startDate, endDate);
            var lichTuanMacDinh = _tuanRepository.GetAll().ToDictionary(x => x.NgayTrongTuan, x => x.LaNgayLamViec);

            // 2. Lấy danh sách Task của nhân viên trong khoảng thời gian này
            DataTable dtTasks = TaskManager.Instance.GetActiveTasksByNhanVienInRange(idNhanVien, startDate, endDate);

            // 3. Quét từng ngày một để dán nhãn (Labeling)
            for (DateTime date = startDate.Date; date <= endDate.Date; date = date.AddDays(1))
            {
                ThongTinLichNgay info = new ThongTinLichNgay { Ngay = date };

                // Kiểm tra xem ngày này có trúng Ngoại lệ (Lễ tết/Làm bù) không?
                var ngoaiLe = lichNgoaiLes?.FirstOrDefault(x => date >= x.NgayBatDau.Date && date <= x.NgayKetThuc.Date);

                bool isWorkingDay = false;

                if (ngoaiLe != null)
                {
                    isWorkingDay = ngoaiLe.LaNgayLamViec;
                    if (!isWorkingDay) // Nếu là ngày nghỉ lễ
                    {
                        info.TrangThaiLich = "holiday";
                        info.TenNgoaiLe = ngoaiLe.TenNgoaiLe;
                        info.ChoPhepClick = true; // Bấm vào để xem đây là ngày lễ gì
                    }
                }
                else
                {
                    // Trượt Ngoại lệ -> Xét Lịch tuần mặc định (Thứ 7, CN...)
                    byte dayOfWeek = (byte)date.DayOfWeek;
                    if (lichTuanMacDinh.ContainsKey(dayOfWeek))
                    {
                        isWorkingDay = lichTuanMacDinh[dayOfWeek];
                    }

                    if (!isWorkingDay)
                    {
                        info.TrangThaiLich = "weekend";
                        info.ChoPhepClick = false; // Cuối tuần rảnh thì khỏi click
                    }
                }

                // Nếu là ngày đi làm (hoặc làm bù), kiểm tra xem có Task nào không
                if (isWorkingDay)
                {
                    // Lọc các Task đè lên ngày hiện tại
                    var tasksInDay = dtTasks?.AsEnumerable().Where(r =>
                        Convert.ToDateTime(r["NgayBatDau"]).Date <= date &&
                        Convert.ToDateTime(r["NgayKetThuc"]).Date >= date).ToList();

                    if (tasksInDay != null && tasksInDay.Count > 0)
                    {
                        info.TrangThaiLich = "busy";
                        info.ChoPhepClick = true; // Bấm vào để xem danh sách Task

                        // Chuyển DataRow thành Object TomTatCongViec
                        foreach (var row in tasksInDay)
                        {
                            info.DanhSachCongViec.Add(new TomTatCongViec
                            {
                                IdCongViec = Guid.Parse(row["IdCongViec"].ToString()),
                                MaCongViec = row["MaCongViec"]?.ToString(),
                                TenCongViec = row["TenCongViec"]?.ToString(),
                                IdDuAn = Guid.Parse(row["IdDuAn"].ToString()),
                                MaDuAn = row["MaDuAn"]?.ToString(),
                                TenDuAn = row["TenDuAn"]?.ToString(),
                                NgayBatDau = Convert.ToDateTime(row["NgayBatDau"]),
                                NgayKetThuc = Convert.ToDateTime(row["NgayKetThuc"]),
                                TrangThai = Convert.ToInt32(row["TrangThai"])
                            });
                        }
                    }
                    else
                    {
                        info.TrangThaiLich = "free";
                        info.ChoPhepClick = false; // Rảnh rang thì không có gì để click
                    }
                }

                ketQua.Add(info);
            }

            return ketQua;
        }
    }
}