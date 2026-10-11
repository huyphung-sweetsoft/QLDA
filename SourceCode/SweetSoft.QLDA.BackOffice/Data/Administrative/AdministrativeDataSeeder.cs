using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Web;

namespace SweetSoft.QLDA.BackOffice.Data.Administrative
{
    public static class AdministrativeDataSeeder
    {
        public static void Seed()
        {
            // 1. Xác định đường dẫn 2 file JSON
            string provincePath = HttpContext.Current.Server.MapPath(
                "~/Data/Administrative/all-province.json");

            string wardPath = HttpContext.Current.Server.MapPath(
                "~/Data/Administrative/all-ward.json");

            if (!File.Exists(provincePath))
            {
                throw new FileNotFoundException(
                    "Không tìm thấy file all-province.json.",
                    provincePath);
            }

            if (!File.Exists(wardPath))
            {
                throw new FileNotFoundException(
                    "Không tìm thấy file all-ward.json.",
                    wardPath);
            }

            // 2. Đọc JSON
            string provinceJson = File.ReadAllText(provincePath);
            string wardJson = File.ReadAllText(wardPath);

            List<ProvinceSeedModel> provinces =
                JsonConvert.DeserializeObject<List<ProvinceSeedModel>>(
                    provinceJson);

            List<WardSeedModel> wards =
                JsonConvert.DeserializeObject<List<WardSeedModel>>(
                    wardJson);

            if (provinces == null || provinces.Count == 0)
            {
                throw new InvalidOperationException(
                    "File all-province.json không có dữ liệu.");
            }

            if (wards == null || wards.Count == 0)
            {
                throw new InvalidOperationException(
                    "File all-ward.json không có dữ liệu.");
            }

            // 3. Lấy connection string từ Web.config
            ConnectionStringSettings connectionSettings =
                ConfigurationManager.ConnectionStrings[
                    "SweetSoft.QLDA.BackOffice"];

            if (connectionSettings == null ||
                string.IsNullOrWhiteSpace(connectionSettings.ConnectionString))
            {
                throw new InvalidOperationException(
                    "Không tìm thấy connection string 'SweetSoft.QLDA.BackOffice'.");
            }

            string connectionString =
                connectionSettings.ConnectionString;

            // 4. Kết nối SQL Server
            using (SqlConnection connection =
                new SqlConnection(connectionString))
            {
                connection.Open();

                // Toàn bộ seed chạy trong 1 transaction
                using (SqlTransaction transaction =
                    connection.BeginTransaction())
                {
                    try
                    {
                        SeedProvinces(
                            connection,
                            transaction,
                            provinces);

                        SeedWards(
                            connection,
                            transaction,
                            wards);

                        transaction.Commit();
                    }
                    catch
                    {
                        transaction.Rollback();
                        throw;
                    }
                }
            }
        }

        private static void SeedProvinces(
            SqlConnection connection,
            SqlTransaction transaction,
            List<ProvinceSeedModel> provinces)
        {
            const string sql = @"
IF NOT EXISTS
(
    SELECT 1
    FROM TblTinhThanh
    WHERE MaTinh = @MaTinh
)
BEGIN
    INSERT INTO TblTinhThanh
    (
        MaTinh,
        TenTinhThanh,
        Slug,
        KichHoat
    )
    VALUES
    (
        @MaTinh,
        @TenTinhThanh,
        @Slug,
        1
    )
END";

            foreach (ProvinceSeedModel province in provinces)
            {
                if (province == null ||
                    province.Name == null)
                {
                    continue;
                }

                string maTinh =
                    !string.IsNullOrWhiteSpace(province.Code?.Id)
                        ? province.Code.Id
                        : province.Id;

                if (string.IsNullOrWhiteSpace(maTinh))
                    continue;

                using (SqlCommand command =
                    new SqlCommand(sql, connection, transaction))
                {
                    command.Parameters.Add(
                        "@MaTinh",
                        SqlDbType.VarChar,
                        2).Value = maTinh;

                    command.Parameters.Add(
                        "@TenTinhThanh",
                        SqlDbType.NVarChar,
                        255).Value =
                            province.Name.Local ?? string.Empty;

                    command.Parameters.Add(
                        "@Slug",
                        SqlDbType.VarChar,
                        255).Value =
                            string.IsNullOrWhiteSpace(province.Name.Slug)
                                ? (object)DBNull.Value
                                : province.Name.Slug;

                    command.ExecuteNonQuery();
                }
            }
        }

        private static void SeedWards(
            SqlConnection connection,
            SqlTransaction transaction,
            List<WardSeedModel> wards)
        {
            const string sql = @"
IF NOT EXISTS
(
    SELECT 1
    FROM TblXaPhuong
    WHERE MaXa = @MaXa
)
BEGIN
    INSERT INTO TblXaPhuong
    (
        MaXa,
        TenXaPhuong,
        Slug,
        MaTinh,
        KichHoat
    )
    VALUES
    (
        @MaXa,
        @TenXaPhuong,
        @Slug,
        @MaTinh,
        1
    )
END";

            foreach (WardSeedModel ward in wards)
            {
                if (ward == null ||
                    ward.Name == null ||
                    ward.Parent == null)
                {
                    continue;
                }

                string maXa =
                    !string.IsNullOrWhiteSpace(ward.Code?.Id)
                        ? ward.Code.Id
                        : ward.Id;

                string maTinh = ward.Parent.Id;

                if (string.IsNullOrWhiteSpace(maXa) ||
                    string.IsNullOrWhiteSpace(maTinh))
                {
                    continue;
                }

                using (SqlCommand command =
                    new SqlCommand(sql, connection, transaction))
                {
                    command.Parameters.Add(
                        "@MaXa",
                        SqlDbType.VarChar,
                        10).Value = maXa;

                    command.Parameters.Add(
                        "@TenXaPhuong",
                        SqlDbType.NVarChar,
                        255).Value =
                            ward.Name.Local ?? string.Empty;

                    command.Parameters.Add(
                        "@Slug",
                        SqlDbType.VarChar,
                        255).Value =
                            string.IsNullOrWhiteSpace(ward.Name.Slug)
                                ? (object)DBNull.Value
                                : ward.Name.Slug;

                    command.Parameters.Add(
                        "@MaTinh",
                        SqlDbType.VarChar,
                        2).Value = maTinh;

                    command.ExecuteNonQuery();
                }
            }
        }
    }
}