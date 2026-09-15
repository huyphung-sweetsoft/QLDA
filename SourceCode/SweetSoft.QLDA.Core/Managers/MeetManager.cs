using OfficeOpenXml.FormulaParsing.Excel.Functions.Text;
using SweetSoft.QLDA.Core.EnumHelper.Defines;
using SweetSoft.QLDA.Core.ExceptionHelpers;
using SweetSoft.QLDA.Core.Infrastructure;
using SweetSoft.QLDA.Core.Infrastructure.Interfaces;
using SweetSoft.QLDA.Core.ResourceTexts;
using SweetSoft.QLDA.Core.Respositories;
using SweetSoft.QLDA.Core.SysManager;
using SweetSoft.QLDA.DataAccess;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SweetSoft.QLDA.Core.Managers
{
    public class MeetManager : BaseManager
    {
        private static readonly Lazy<MeetManager> _instance = new Lazy<MeetManager>(() => new MeetManager());
        public static MeetManager Instance => _instance.Value;
        private readonly MeetRepository _repository;
        private readonly AuditManager _auditManager;

        public MeetManager(IAppContext applicationContext = null) : base(applicationContext)
        {
            _auditManager = new AuditManager(GetClientInfo());
            _repository = new MeetRepository(_auditManager);
        }
        public DataTable SearchMeeting(Guid projectId, string searchTerm, Dictionary<string, object> parameters, string orderBy, int startRow, int endRow, out int totalRecord)
        {
            return _repository.SearchMeeting(projectId, searchTerm, parameters, orderBy, startRow, endRow, out totalRecord);
        }
        public string GenerateMaCuocHop(Guid projectId)
        {
            return _repository.GenerateMaCuocHop(projectId);
        }

        public byte GetMeetStatus(DateTime? dtStart, DateTime? dtEnd)
        {
            if (dtStart.HasValue && dtEnd.HasValue)
            {
                DateTime now = DateTime.Now;

                if (now > dtEnd.Value)
                {
                    return (byte)TrangThaiCuocHopEnum.Ended;
                }
                else if (now >= dtStart.Value && now <= dtEnd.Value)
                {
                    return (byte)TrangThaiCuocHopEnum.Ongoing;
                }
                else if ((dtStart.Value - now).TotalMinutes > 0 && (dtStart.Value - now).TotalMinutes <= 15)
                {
                    return (byte)TrangThaiCuocHopEnum.Upcoming;
                }
                else
                {
                    return (byte)TrangThaiCuocHopEnum.Scheduled;
                }
            }

            return (byte)TrangThaiCuocHopEnum.Scheduled;
        }
        public string GetValuForTrangThaiCuoHop(object status)
        {
            switch (status)
            {
                case TrangThaiCuocHopEnum.Scheduled:
                    return "SCHEDULED";
                case TrangThaiCuocHopEnum.Upcoming:
                    return "UPCOMING";
                case TrangThaiCuocHopEnum.Ongoing:
                    return "ONGOING";
                case TrangThaiCuocHopEnum.Ended:
                    return "ENDED";
                default:
                    return status.ToString();
            }
        }
        public void UpdateTblLichHopNhanVien(Guid idLichHop, string lstNhanVienIds)
        {
            _repository.UpdateTblLichHopNhanVien(idLichHop, lstNhanVienIds);
        }
        public DataTable GetNhanVienThamGia(Guid idLichHop)
        {
            return _repository.GetNhanVienThamGia(idLichHop);
        }
        public TblLichHop CreateOrUpdate(TblLichHop dto, string nhanVienIds)
        {
            BusinessValidator.ThrowIfNull(dto, BackEndResourceKeys.INVALID_DATA);
            BusinessValidator.ThrowIfNullOrEmpty(dto.TenCuocHop, BackEndResourceKeys.PLEASE_ENTER_THE_VALUE, nameof(dto.TenCuocHop));
            BusinessValidator.ThrowIf(dto.IdDuAn == Guid.Empty || dto.IdDuAn == null, BackEndResourceKeys.INVALID_DATA, nameof(dto.IdDuAn));

            Guid currentUserId = SweetContext.Current != null ? SweetContext.Current.UserId : Guid.Empty;
            bool isInsert = (dto.IdLichHop == Guid.Empty);
            TblLichHop result = null;

            int trangThai = GetMeetStatus(dto.ThoiGianBatDau, dto.ThoiGianKetThuc);

            if (isInsert)
            {
                dto.IdLichHop = Guid.NewGuid();
                dto.DaXoa = false;
                dto.TrangThai = (byte)trangThai;

                dto.MaCuocHop = GenerateMaCuocHop(dto.IdDuAn);

                dto.NgayTao = DateTime.Now;
                dto.IdNguoiTao = currentUserId;

                dto.Save();
                result = dto;
            }
            else
            {
                TblLichHop existingMeet = TblLichHop.FetchByID(dto.IdLichHop);
                BusinessValidator.ThrowIfNull(existingMeet, BackEndResourceKeys.NOT_FOUND, nameof(dto.IdLichHop), ErrorCodes.NotFound);

                existingMeet.TenCuocHop = dto.TenCuocHop;
                existingMeet.NoiDungCuocHop = dto.NoiDungCuocHop;
                existingMeet.DiaDiemHop = dto.DiaDiemHop;
                existingMeet.ThoiGianBatDau = dto.ThoiGianBatDau;
                existingMeet.ThoiGianKetThuc = dto.ThoiGianKetThuc;
                existingMeet.TrangThai = (byte)trangThai;

                existingMeet.NgayCapNhat = DateTime.Now;
                existingMeet.IdNguoiCapNhat = currentUserId;

                existingMeet.Save();
                result = existingMeet;
            }

            if (result != null)
            {
                UpdateTblLichHopNhanVien(result.IdLichHop, nhanVienIds);
            }

            return result;
        }
        public void DeleteMeet(TblLichHop meet)
        {
            _repository.DeleteMeet(meet);
        }
    }
}
