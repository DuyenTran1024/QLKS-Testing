# 🏨 QLKS (Hotel & Homestay Management System) - Testing Portfolio

Dự án kiểm thử phần mềm toàn diện (End-to-End Testing Portfolio) cho hệ thống Quản lý Khách sạn / Homestay (QLKS). Dự án thể hiện quy trình đảm bảo chất lượng phần mềm (SQA) chuyên nghiệp, kết hợp chặt chẽ giữa kiểm thử thủ công (Manual Testing) và kiểm thử tự động (Automation Testing).

---

## 📌 1. Tổng quan hệ thống

- Tên ứng dụng: Website Quản lý Khách sạn `QLKS`
- Môi trường & công nghệ: Windows, IIS, .NET Framework, ASP.NET MVC, Entity Framework, SQL Server
- Mục tiêu: Xây dựng hệ thống quản lý đặt phòng, khách hàng, nhân viên, dịch vụ và hóa đơn, đi kèm với quy trình kiểm thử nghiêm ngặt để đảm bảo tính đúng đắn của dữ liệu và an toàn bảo mật

---

## 📂 2. Cấu trúc Solution

Giải pháp `QLKS_2.sln` được chia thành 2 project độc lập, thể hiện rõ tính phân tách trong kiến trúc phát triển phần mềm:

```text
📦 QLKS-Testing-Portfolio
┣ 📂 01_Documentation/               <-- Tài liệu báo cáo, đề tài và đặc tả yêu cầu
┣ 📂 02_Manual_Testing/              <-- Kiểm thử thủ công (Manual Testing)
┃  ┗ 📊 TestCases_And_Bugs.xlsx      <-- Chứa 259 Test Cases, Bug Report & Thống kê lỗi
┣ 📂 QLKS/                           <-- 🌐 Source code ứng dụng Web chính
┣ 📂 QLKS.Tests/                     <-- 🤖 Source code Automation Test (C#, NUnit, Selenium)
┣ 📄 Testcases_DatPhong.csv          <-- Dữ liệu kiểm thử (Data-Driven Testing)
┗ 📄 README.md                       <-- Giới thiệu tổng quan dự án
```

---

## 🧪 3. Kiểm thử thủ công (Manual Testing)

Thiết kế Test Case: Xây dựng bộ test cases dựa trên các kỹ thuật hộp đen như phân vùng tương đương và phân tích giá trị biên cho các phân hệ cốt lõi:

- Module Đăng ký & Đăng nhập: xác thực dữ liệu, phân quyền tài khoản
- Module Tìm kiếm & Đặt phòng Online: kiểm tra logic ngày đến/ngày đi, chặn trùng lịch
- Module Quản lý Vận hành Admin: Check-in, Check-out, Gọi dịch vụ, Hóa đơn

Quản lý lỗi (Defect Tracking): thực thi test, ghi nhận và phân tích các lỗi nghiêm trọng như:

- Critical / High severity issues
- Thiếu ràng buộc định danh dữ liệu (trùng CCCD, Email, SĐT)
- Rò rỉ phiên làm việc (Session Leakage)

---

## 🤖 4. Kiểm thử tự động (Automation Testing)

Dựa trên các kịch bản kiểm thử thủ công quan trọng, project `QLKS.Tests` được xây dựng để thực hiện kiểm thử hồi quy (Regression Testing) tự động.

### Công nghệ sử dụng

- C#
- NUnit Framework
- Selenium WebDriver
- Microsoft.VisualBasic.FileIO

### Kỹ thuật nổi bật

- Data-Driven Testing: đọc dữ liệu kiểm thử linh hoạt từ file `.csv` (`Testcases_DatPhong.csv`) để chạy hàng loạt bộ dữ liệu khác nhau mà không cần sửa code
- Helper Methods & SQL Integration: xây dựng các hàm hỗ trợ thao tác web (đăng nhập, tìm phòng, xử lý Alert, tương tác bảng table/tr/td) kết hợp kết nối trực tiếp cơ sở dữ liệu SQL Server để thực thi các kịch bản kiểm tra thời gian hết hạn phiên làm việc (Session Timeout)

---

## 🚀 5. Hướng dẫn chạy Automation Test

1. Mở file Solution `QLKS_2.sln` bằng phần mềm Visual Studio.
2. Cấu hình chuỗi kết nối Database (`web.config`) và chuỗi kết nối SQL trong helper test cho phù hợp với máy cá nhân của bạn.
3. Đảm bảo ứng dụng web QLKS đang chạy ở môi trường local.
4. Mở cửa sổ Test Explorer trong Visual Studio: `Test > Test Explorer`.
5. Bấm `Run All` để chạy toàn bộ kịch bản kiểm thử tự động trên trình duyệt Chrome.

---

## ✅ Kết luận

Dự án này minh họa rõ quá trình xây dựng và vận hành một hệ thống kiểm thử phần mềm chuyên nghiệp, từ khâu lên kế hoạch test, ghi nhận lỗi, đến triển khai tự động hóa kiểm thử hồi quy trên ứng dụng thực tế.
