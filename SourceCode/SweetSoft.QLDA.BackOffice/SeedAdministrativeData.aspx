<%@ Page Language="C#" AutoEventWireup="true"
    CodeBehind="SeedAdministrativeData.aspx.cs"
    Inherits="SweetSoft.QLDA.BackOffice.SeedAdministrativeData" %>

<!DOCTYPE html>

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title>Seed dữ liệu hành chính</title>

    <style>
        body {
            font-family: Arial, sans-serif;
            padding: 40px;
        }

        .container {
            max-width: 700px;
            margin: auto;
        }

        .btn {
            padding: 10px 20px;
            cursor: pointer;
        }

        .result {
            margin-top: 20px;
            padding: 15px;
            border: 1px solid #ddd;
        }
    </style>
</head>

<body>
    <form id="form1" runat="server">

        <div class="container">

            <h2>Seed dữ liệu hành chính</h2>

            <p>
                Trang này dùng để import dữ liệu
                Tỉnh/Thành phố và Xã/Phường/Đặc khu
                từ file JSON vào SQL Server.
            </p>

            <asp:Button
                runat="server"
                ID="btnSeed"
                Text="Seed dữ liệu"
                CssClass="btn"
                OnClick="btnSeed_Click" />

            <div class="result">
                <asp:Label
                    runat="server"
                    ID="lblResult" />
            </div>

        </div>

    </form>
</body>
</html>