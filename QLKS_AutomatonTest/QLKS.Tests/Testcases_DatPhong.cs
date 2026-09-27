using NUnit.Framework;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.VisualBasic.FileIO;
using System.Threading;
using System.Linq;

namespace QLKS.Tests
{
    public class DatPhongTests
    {
        private IWebDriver driver;

        [SetUp]
        public void Setup()
        {
            // TẠO CẤU HÌNH ĐỂ "BỊT MIỆNG" CHROME
            ChromeOptions options = new ChromeOptions();

            // 1. Tắt hoàn toàn trình quản lý mật khẩu và cảnh báo lộ mật khẩu
            options.AddUserProfilePreference("credentials_enable_service", false);
            options.AddUserProfilePreference("profile.password_manager_enabled", false);
            options.AddUserProfilePreference("profile.password_manager_leak_detection", false);

            // 2. Tắt các thông báo (Notifications) và Popup rác khác của trình duyệt
            options.AddArgument("--disable-notifications");
            options.AddArgument("--disable-popup-blocking");

            // (Tùy chọn) Chạy ẩn danh để đảm bảo môi trường test luôn sạch sẽ, không lưu cache/cookie
            options.AddArgument("--incognito");

            // Truyền cấu hình này vào lúc mở Chrome
            driver = new ChromeDriver(options);

            driver.Manage().Window.Maximize();
            driver.Manage().Timeouts().ImplicitWait = TimeSpan.FromSeconds(5);
        }

        // HÀM LÕI: Đọc và lọc CSV theo danh sách ID truyền vào
        public static IEnumerable<TestCaseData> ReadCsvAndFilter(params string[] targetTestCases)
        {
            string path = Path.Combine(TestContext.CurrentContext.TestDirectory, "Testcases_DatPhong.csv");

            using (TextFieldParser parser = new TextFieldParser(path))
            {
                parser.TextFieldType = FieldType.Delimited;
                parser.SetDelimiters(",");
                parser.HasFieldsEnclosedInQuotes = true;
                parser.ReadFields(); // Bỏ qua dòng Tiêu đề (Header)

                while (!parser.EndOfData)
                {
                    string[] fields = null;
                    try { fields = parser.ReadFields(); }
                    catch (MalformedLineException) { continue; }

                    // Nếu dòng có dữ liệu và cột ID (fields[0]) nằm trong danh sách cần lấy
                    if (fields != null && fields.Length >= 7 && Array.Exists(targetTestCases, id => id == fields[0]))
                    {
                        string testCaseId = fields[0];
                        string testData = fields[5];
                        string expectedResult = fields[6];

                        // Gói dữ liệu lại và gửi đi
                        yield return new TestCaseData(testCaseId, testData, expectedResult).SetName(testCaseId);
                    }
                }
            }
        }

        // DỮ LIỆU NHÓM 1: Kiểm tra lỗi ngày tháng
        public static IEnumerable<TestCaseData> GetNhom1Data()
        {
            return ReadCsvAndFilter("TC_Booking_005", "TC_Booking_006", "TC_Booking_007", "TC_Booking_008");
        }

        // DỮ LIỆU NHÓM 2: Happy Path (Đặt thành công)
        public static IEnumerable<TestCaseData> GetNhom2Data()
        {
            return ReadCsvAndFilter("TC_Booking_001", "TC_Booking_002", "TC_Booking_003");
        }

        // DỮ LIỆU NHÓM 3: Validate trang Chọn phòng
        public static IEnumerable<TestCaseData> GetNhom3Data()
        {
            return ReadCsvAndFilter("TC_Booking_004", "TC_Booking_009");
        }

        // DỮ LIỆU NHÓM 4: Kiểm tra Phiên lưu nháp (Session/Cookie)
        public static IEnumerable<TestCaseData> GetNhom4Data()
        {
            return ReadCsvAndFilter("TC_Booking_010", "TC_Booking_011", "TC_Booking_012", "TC_Booking_013", "TC_Booking_014");
        }

        // DỮ LIỆU NHÓM 5: Khách hàng đặt phòng -> Admin kiểm tra (Tích hợp chiều đi)
        public static IEnumerable<TestCaseData> GetNhom5Data()
        {
            return ReadCsvAndFilter("TC_Booking_015", "TC_Booking_017", "TC_Booking_018", "TC_Booking_019");
        }

        // DỮ LIỆU NHÓM 6: Khách hàng nhập sai -> Admin kiểm tra (Tích hợp chặn lỗi)
        public static IEnumerable<TestCaseData> GetNhom6Data()
        {
            return ReadCsvAndFilter("TC_Booking_024", "TC_Booking_025", "TC_Booking_026", "TC_Booking_027", "TC_Booking_028");
        }

        // DỮ LIỆU NHÓM 7: Khách hàng Hủy phòng -> Admin kiểm tra
        public static IEnumerable<TestCaseData> GetNhom7Data()
        {
            return ReadCsvAndFilter("TC_Booking_016", "TC_Booking_023");
        }

        // DỮ LIỆU NHÓM 8: Admin duyệt phòng -> Khách hàng kiểm tra (Tích hợp chiều về)
        public static IEnumerable<TestCaseData> GetNhom8Data()
        {
            return ReadCsvAndFilter("TC_Booking_020", "TC_Booking_021", "TC_Booking_022");
        }

        // DỮ LIỆU Nhóm 9: Kiểm tra đa trình duyệt (Cross-browser)
        public static IEnumerable<TestCaseData> GetNhom9Data()
        {
            return ReadCsvAndFilter("TC_Booking_030", "TC_Booking_031", "TC_Booking_032", "TC_Booking_033");
        }

        // hàm phụ trợ để tách giá trị từ chuỗi dữ liệu thô (ví dụ: "Ngày vào: 2024-07-01" -> "2024-07-01")
        private string ExtractValue(string rawData, string key)
        {
            if (string.IsNullOrEmpty(rawData)) return "";
            var lines = rawData.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
            foreach (var line in lines)
            {
                if (line.ToLower().Contains(key.ToLower()))
                    return line.Split(':')[1].Trim();
            }
            return "";
        }

        // NHÓM 2: KIỂM THỬ TÌM NGÀY THẤT BẠI
        [Test, TestCaseSource(nameof(GetNhom1Data))]
        public void Test_ValidateNgayTimPhong(string testCaseId, string testData, string expectedResult)
        {
            // 1. CHUẨN BỊ DỮ LIỆU
            string ngayVao = ExtractValue(testData, "Ngày vào");
            string ngayRa = ExtractValue(testData, "Ngày ra");

            TestContext.Progress.WriteLine($"[Chạy {testCaseId}] -> Ngày vào: {ngayVao} | Ngày ra: {ngayRa}");

            // 2. GỌI CÁC HÀM DÙNG CHUNG (Cực kỳ ngắn gọn và dễ hiểu)
            LoginUser("DangQuocHung", "123456"); // Đổi ID test nếu cần
            GoToTimPhongPage();
            SearchRoom(ngayVao, ngayRa);

            // 3. KIỂM CHỨNG KẾT QUẢ (ASSERT LOGIC ĐẶC THÙ CỦA NHÓM 1)
            try
            {
                IAlert alert = driver.SwitchTo().Alert();
                string actualAlertText = alert.Text;
                TestContext.Progress.WriteLine($"[Web Báo Lỗi]: {actualAlertText}");

                alert.Accept(); // Đóng Alert

                if (expectedResult.Contains(actualAlertText))
                {
                    TestContext.Progress.WriteLine($"=> PASS: Web đã chặn đúng lỗi '{actualAlertText}'!");
                    Assert.Pass("Bắt được Alert thông báo lỗi chính xác.");
                }
                else
                {
                    Assert.Fail($"Lỗi logic: Mong đợi: {expectedResult} | Thực tế: {actualAlertText}");
                }
            }
            catch (NoAlertPresentException)
            {
                Assert.Fail("Bug: Dữ liệu ngày tháng sai nhưng hệ thống không có Alert cảnh báo!");
            }
        }

        // NHÓM 2: KIỂM THỬ ĐẶT PHÒNG THÀNH CÔNG
        [Test, TestCaseSource(nameof(GetNhom2Data))]
        public void Test_DatPhongThanhCong(string testCaseId, string testData, string expectedResult)
        {
            // 1. CHUẨN BỊ DỮ LIỆU
            string ngayVao = ExtractValue(testData, "Ngày vào");
            string ngayRa = ExtractValue(testData, "Ngày ra");
            string roomId = ExtractValue(testData, "Phòng");

            TestContext.Progress.WriteLine($"[Chạy {testCaseId}] -> Đặt phòng: {roomId}");

            // 2. GỌI CÁC HÀM DÙNG CHUNG
            LoginUser("DangQuocHung", "123456");
            GoToTimPhongPage();
            SearchRoom(ngayVao, ngayRa);

            // Bắt Try-Catch lỡ web văng Alert
            try
            {
                IAlert alert = driver.SwitchTo().Alert();
                string alertText = alert.Text;
                alert.Accept();
                Assert.Fail($"Bug dữ liệu: Kịch bản đúng nhưng web lại báo lỗi: {alertText}");
            }
            catch (NoAlertPresentException)
            {
                // Tiếp tục dùng các hàm Helper mới tạo
                SelectRoomAndComplete(roomId);
                ConfirmBooking();

                // 3. KIỂM CHỨNG KẾT QUẢ CUỐI CÙNG (ASSERT)
                if (driver.PageSource.Contains("Đã đặt phòng thành công"))
                {
                    TestContext.Progress.WriteLine("=> PASS: Luồng đặt phòng thành công");
                    Assert.Pass("Hệ thống hiển thị Đặt phòng thành công.");
                }
                else
                {
                    Assert.Fail("Lỗi: Quá trình đặt phòng thất bại hoặc không nhảy sang được trang Result");
                }
            }
        }

        // NHÓM 3: KIỂM THỬ VALIDATE BƯỚC CHỌN PHÒNG
        [Test, TestCaseSource(nameof(GetNhom3Data))]
        public void Test_ValidateChonPhong(string testCaseId, string testData, string expectedResult)
        {
            // 1. CHUẨN BỊ DỮ LIỆU TỪ CSV
            string ngayVao = ExtractValue(testData, "Ngày vào");
            string ngayRa = ExtractValue(testData, "Ngày ra");
            string roomId = ExtractValue(testData, "Phòng"); // Với TC_009 biến này sẽ tự rỗng

            TestContext.Progress.WriteLine($"[Chạy {testCaseId}] -> Xử lý ngoại lệ trang FindRoom");

            // 2. GỌI CÁC HÀM DÙNG CHUNG
            LoginUser("DangQuocHung", "123456");
            GoToTimPhongPage();
            SearchRoom(ngayVao, ngayRa);

            // 3. RẼ NHÁNH XỬ LÝ THEO TỪNG KỊCH BẢN ĐẶC THÙ

            // TRƯỜNG HỢP 1: TC_004 (Kiểm tra phòng bị trùng có biến mất không)
            if (expectedResult.Contains("ẩn đi") || expectedResult.Contains("không hiển thị"))
            {
                var tableRows = driver.FindElements(By.TagName("tr"));
                bool isRoomFound = false;

                foreach (var row in tableRows)
                {
                    if (row.Text.Contains(roomId))
                    {
                        isRoomFound = true;
                        break;
                    }
                }

                if (isRoomFound)
                {
                    Assert.Fail($"Bug hệ thống: Phòng {roomId} đã bị đặt trùng nhưng vẫn hiện cho khách chọn");
                }
                else
                {
                    TestContext.Progress.WriteLine($"=> PASS: Phòng {roomId} đã bị ẩn đi");
                    Assert.Pass("Hệ thống đã chặn và ẩn thành công phòng bị trùng.");
                }
            }

            // TRƯỜNG HỢP 2: TC_009 (Bỏ qua nút Chọn phòng, bấm thẳng Hoàn Tất)
            else if (expectedResult.Contains("chưa chọn phòng"))
            {
                // Thò tay bấm luôn chữ Hoàn Tất bằng LinkText
                IWebElement btnHoanTat = driver.FindElement(By.PartialLinkText("Hoàn Tất"));
                IJavaScriptExecutor js = (IJavaScriptExecutor)driver;
                js.ExecuteScript("arguments[0].click();", btnHoanTat);
                Thread.Sleep(1500);

                // Bắt lỗi kép (Quét Alert trước, không có thì quét mã nguồn PageSource)
                try
                {
                    IAlert alert = driver.SwitchTo().Alert();
                    string alertText = alert.Text;
                    alert.Accept();

                    if (alertText.Contains("chưa chọn phòng"))
                    {
                        TestContext.Progress.WriteLine("=> PASS: Bắt được Alert khách chưa chọn phòng");
                        Assert.Pass("Hệ thống chặn thành công bằng Alert.");
                    }
                    else
                    {
                        Assert.Fail($"Lỗi Alert sai nội dung: {alertText}");
                    }
                }
                catch (NoAlertPresentException)
                {
                    if (driver.PageSource.Contains("chưa chọn phòng"))
                    {
                        TestContext.Progress.WriteLine("=> PASS: Hiện chữ cảnh báo chưa chọn phòng trên trang web");
                        Assert.Pass("Hệ thống chặn thành công bằng Text hiển thị.");
                    }
                    else
                    {
                        Assert.Fail("Bug nghiêm trọng: Không chọn phòng nào, bấm Hoàn Tất mà hệ thống vẫn cho qua");
                    }
                }
            }
        }

        // NHÓM 4: KIỂM THỬ PHIÊN LƯU NHÁP (SESSION/COOKIE)
        [Test, TestCaseSource(nameof(GetNhom4Data))]
        public void Test_KiemTraPhieuDatPhongNhaps(string testCaseId, string testData, string expectedResult)
        {
            // 1. CHUẨN BỊ DỮ LIỆU
            string ngayVao = ExtractValue(testData, "Ngày vào");
            string ngayRa = ExtractValue(testData, "Ngày ra");
            string roomId = ExtractValue(testData, "Phòng");

            string user1 = ExtractValue(testData, "Tên đăng nhập tài khoản ban đầu");
            if (string.IsNullOrEmpty(user1)) user1 = "DangQuocHung";

            string user2 = ExtractValue(testData, "Tên đăng nhập");
            if (string.IsNullOrEmpty(user2)) user2 = "KH001";

            TestContext.Progress.WriteLine($"[Chạy {testCaseId}] -> Test Session với Phòng: {roomId}");


            // HÀM CỤC BỘ: GỘP 4 HELPER METHOD THÀNH 1 LỆNH DUY NHẤT
            void TaoPhieuNhap(string tk, string mk)
            {
                LoginUser(tk, mk);
                GoToTimPhongPage();
                SearchRoom(ngayVao, ngayRa);
                SelectRoomAndComplete(roomId);
            }


            // RẼ NHÁNH KỊCH BẢN 

            // Bước đầu tiên cho mọi TC: Tài khoản 1 đăng nhập và tạo phiếu nháp
            TaoPhieuNhap(user1, "123456");
            bool isRoomInDraft = driver.PageSource.Contains(roomId);

            switch (testCaseId)
            {
                case "TC_Booking_010":
                    // Test xem phiếu nháp có hiển thị đủ thông tin không
                    bool hasInfo = driver.PageSource.Contains("Họ tên") && driver.PageSource.Contains("Ngày vào");
                    bool hasBtn = driver.FindElements(By.CssSelector("input[value='Đặt phòng']")).Count > 0;
                    if (hasInfo && hasBtn && isRoomInDraft) Assert.Pass("TC_010 PASS: Hiển thị đúng phiếu đặt phòng.");
                    else Assert.Fail("TC_010 FAIL: Thiếu thông tin trên phiếu xem trước.");
                    break;

                case "TC_Booking_011":
                    // Test Đăng xuất rồi vào lại ngay
                    LogoutUser();
                    LoginUser(user1, "123456");
                    driver.Navigate().GoToUrl("http://localhost:49921/Home/BookRoom");
                    if (driver.PageSource.Contains(roomId)) Assert.Pass("TC_011 PASS: Phiếu nháp vẫn được lưu an toàn.");
                    else Assert.Fail("TC_011 FAIL: Vừa đăng xuất vào lại đã mất phiếu nháp!");
                    break;

                case "TC_Booking_012":
                    LogoutUser();

                    TestContext.Progress.WriteLine("Đang kích hoạt cỗ máy thời gian SQL để tua nhanh 5 phút...");
                    HackThoiGianPhieuNhap(user1); // chỉnh lùi DB về 6 phút trước để demo

                    Thread.Sleep(2000); // Chỉ cần chờ nhẹ 2 giây cho DB đồng bộ là xong!

                    LoginUser(user1, "123456");
                    driver.Navigate().GoToUrl("http://localhost:49921/Home/BookRoom");

                    if (!driver.PageSource.Contains(roomId))
                        Assert.Pass("TC_012 PASS: Phiếu nháp đã bị hệ thống tự động dọn dẹp (do quá 5 phút).");
                    else
                        Assert.Fail("TC_012 FAIL: Quá 5 phút (đã hack DB) mà phiếu nháp vẫn còn sống!");
                    break;

                case "TC_Booking_013":
                    // Test Dính Session giữa 2 tài khoản khác nhau
                    LogoutUser();
                    LoginUser(user2, "123456"); // Vào bằng tài khoản thứ 2
                    driver.Navigate().GoToUrl("http://localhost:49921/Home/BookRoom");
                    if (!driver.PageSource.Contains(roomId)) Assert.Pass("TC_013 PASS: Đổi tài khoản thành công, không bị dính session.");
                    else Assert.Fail("TC_013 FAIL BUG: Tài khoản sau lại nhìn thấy phiếu nháp của tài khoản trước!");
                    break;

                case "TC_Booking_014":
                    // Test Tự động hủy nháp khi bị người khác hớt tay trên
                    LogoutUser();

                    // Tài khoản 2 nhảy vào Đặt phòng chốt đơn
                    TaoPhieuNhap(user2, "123456");
                    ConfirmBooking();

                    // Tài khoản 1 quay lại kiểm tra
                    LogoutUser();
                    LoginUser(user1, "123456");
                    driver.Navigate().GoToUrl("http://localhost:49921/Home/BookRoom");

                    if (!driver.PageSource.Contains(roomId)) Assert.Pass("TC_014 PASS: Phòng bị đặt nên nháp của mình tự hủy.");
                    else Assert.Fail("TC_014 FAIL BUG: Phòng bị người khác đặt rồi mà phiếu nháp vẫn còn cho thanh toán!");
                    break;
            }
        }

        // NHÓM 5: TEST TÍCH HỢP (KHÁCH ĐẶT -> ADMIN KIỂM TRA)
        [Test, TestCaseSource(nameof(GetNhom5Data))]
        public void Test_TichHop_DatPhongHopLe_AdminKiemTra(string testCaseId, string testData, string expectedResult)
        {
            // 1. CHUẨN BỊ DỮ LIỆU TỪ CSV
            string ngayVao = ExtractValue(testData, "Ngày vào");
            string ngayRa = ExtractValue(testData, "Ngày ra");
            string roomId = ExtractValue(testData, "Phòng");

            // Đọc thông tin Admin từ dòng Test Data
            string adminUser = ExtractValue(testData, "Tên tài khoản admin");
            if (string.IsNullOrEmpty(adminUser)) adminUser = "Admin";

            string adminPass = ExtractValue(testData, "Mật khẩu");
            if (adminPass.Contains("admin123")) adminPass = "admin123";

            TestContext.Progress.WriteLine($"[Chạy {testCaseId}] -> Luồng E2E: Đặt phòng {roomId} và Admin kiểm tra");

            TestContext.Progress.WriteLine("=> [Pha 1] Khách hàng đang thực hiện đặt phòng...");
            LoginUser("DangQuocHung", "123456");
            GoToTimPhongPage();
            SearchRoom(ngayVao, ngayRa);

            // Rào lại bằng Try-Catch phòng hờ web văng Alert báo lỗi ngày tháng/trùng phòng
            try
            {
                IAlert alert = driver.SwitchTo().Alert();
                string alertText = alert.Text;
                alert.Accept();
                Assert.Fail($"Bug dữ liệu Khách: Mong đợi đặt thành công nhưng web báo lỗi '{alertText}'");
            }
            catch (NoAlertPresentException)
            {
                SelectRoomAndComplete(roomId);
                ConfirmBooking();
            }

            TestContext.Progress.WriteLine("=> Khách hàng đăng xuất, Admin vào việc...");
            LogoutUser();
            LoginAdmin(adminUser, adminPass);
            GoToAdminPhieuDatPhong();

            // Admin quét toàn bộ mã nguồn của trang danh sách để tìm số phòng và trạng thái
            string adminPageSource = driver.PageSource;

            if (expectedResult.Contains("Chờ xác nhận"))
            {
                // Phải thỏa mãn 2 điều kiện: Thấy số phòng VÀ thấy chữ "Chờ xác nhận" (hoặc Chưa duyệt)
                bool hasRoom = adminPageSource.Contains(roomId);
                bool hasStatus = adminPageSource.Contains("Chờ xác nhận") || adminPageSource.Contains("Chưa duyệt");

                if (hasRoom && hasStatus)
                {
                    TestContext.Progress.WriteLine($"=> PASS: Tích hợp hoàn hảo! Admin đã thấy phiếu đặt phòng {roomId} đang chờ xác nhận.");
                    Assert.Pass("Luồng Khách -> Admin thành công rực rỡ.");
                }
                else
                {
                    Assert.Fail($"LỖI TÍCH HỢP: Khách đã đặt thành công nhưng Admin mở lên không thấy phòng {roomId} hoặc sai trạng thái!");
                }
            }
        }

        // NHÓM 6: TEST TÍCH HỢP CHẶN LỖI (KHÁCH NHẬP SAI -> ADMIN CHECK DB)
        [Test, TestCaseSource(nameof(GetNhom6Data))]
        public void Test_TichHop_DatPhongLoi_AdminKiemTra(string testCaseId, string testData, string expectedResult)
        {
            // 1. CHUẨN BỊ DỮ LIỆU
            string ngayVao = ExtractValue(testData, "Ngày vào");
            string ngayRa = ExtractValue(testData, "Ngày ra");
            string roomId = ExtractValue(testData, "Phòng"); // TC 25->28 có thể rỗng vì bị chặn ngay từ ngày tháng

            string userTk = "KH001"; // Account test mặc định
            string adminUser = ExtractValue(testData, "Tên tài khoản admin");
            if (string.IsNullOrEmpty(adminUser)) adminUser = "Admin";
            string adminPass = "admin123";

            TestContext.Progress.WriteLine($"[Chạy {testCaseId}] -> Khách cố tình làm sai, Admin đi tuần tra");

            // PHA 1: KHÁCH HÀNG THAO TÁC SAI
            LoginUser(userTk, "123456");
            GoToTimPhongPage();
            SearchRoom(ngayVao, ngayRa);

            // Bắt lỗi theo 2 kịch bản của Nhóm 6
            try
            {
                // Kịch bản 1: TC_25 đến TC_28 (Sai ngày tháng -> Bị chặn bằng Alert)
                IAlert alert = driver.SwitchTo().Alert();
                string alertText = alert.Text;
                alert.Accept();
                TestContext.Progress.WriteLine($"=> [Bảo vệ lớp 1]: Khách bị chặn vì lỗi '{alertText}'");
            }
            catch (NoAlertPresentException)
            {
                // Kịch bản 2: TC_24 (Ngày tháng đúng, nhưng phòng đã bị người khác đặt -> Bị ẩn)
                if (expectedResult.Contains("ẩn đi") && !string.IsNullOrEmpty(roomId))
                {
                    if (driver.PageSource.Contains(roomId))
                    {
                        Assert.Fail($"Bug hiển thị: Phòng {roomId} đã bị đặt trùng nhưng vẫn hiện lên cho khách chọn");
                    }
                    else
                    {
                        TestContext.Progress.WriteLine($"=> [Bảo vệ lớp 1]: Khách bị chặn vì phòng {roomId} đã tự động ẩn.");
                    }
                }
            }


            LogoutUser();
            LoginAdmin(adminUser, adminPass);
            GoToAdminPhieuDatPhong();

            // Admin soi toàn bộ các dòng trong bảng danh sách phiếu
            var tableRows = driver.FindElements(By.TagName("tr"));
            bool hasGarbageBooking = false;

            foreach (var row in tableRows)
            {
                // Kiểm tra xem có dòng nào chứa Tên khách hàng VÀ có chữ "Chờ xác nhận" (Phiếu mới) không?
                if (row.Text.Contains(userTk) && (row.Text.Contains("Chờ xác nhận") || row.Text.Contains("Chưa duyệt")))
                {
                    // Nếu là TC_24 (có truyền roomId), ta soi kỹ thêm xem đúng cái phòng đó bị lọt vào không
                    if (string.IsNullOrEmpty(roomId) || row.Text.Contains(roomId))
                    {
                        hasGarbageBooking = true;
                        break; 
                    }
                }
            }

            if (hasGarbageBooking)
            {
                Assert.Fail($"LỖI BẢO MẬT NGHIÊM TRỌNG: Giao diện web có chặn lỗi, nhưng Database vẫn lưu phiếu rác của '{userTk}' hiển thị lên cho Admin");
            }
            else
            {
                TestContext.Progress.WriteLine("=> PASS: Hệ thống chặn lỗi. Trang Admin sạch sẽ, không có rác lọt vào.");
                Assert.Pass("Test Tích Hợp Phủ định thành công.");
            }
        }

        // NHÓM 7: TEST TÍCH HỢP (KHÁCH HỦY PHÒNG -> ADMIN KIỂM TRA)
        [Test, TestCaseSource(nameof(GetNhom7Data))]
        public void Test_TichHop_HuyPhong_AdminKiemTra(string testCaseId, string testData, string expectedResult)
        {
            // 1. CHUẨN BỊ DỮ LIỆU TỪ CSV
            // TC_016 trong file của bạn không có data mẫu, nên ta setup 1 giá trị mặc định để nó chạy
            string ngayVao = ExtractValue(testData, "Ngày vào");
            if (string.IsNullOrEmpty(ngayVao)) ngayVao = "27/03/2026"; // Mặc định nếu thiếu

            string ngayRa = ExtractValue(testData, "Ngày ra");
            if (string.IsNullOrEmpty(ngayRa)) ngayRa = "28/03/2026";

            string roomId = ExtractValue(testData, "Phòng");
            if (string.IsNullOrEmpty(roomId)) roomId = "102"; // Lấy phòng 102

            string userTk = "DangQuocHung";
            string adminUser = "Admin";
            string adminPass = "admin123";

            TestContext.Progress.WriteLine($"[Chạy {testCaseId}] -> Khách Hủy phòng {roomId} và Admin kiểm tra");


            // 1: TẠO DỮ LIỆU (KHÁCH PHẢI ĐẶT PHÒNG THÌ MỚI CÓ CÁI ĐỂ HỦY)
            LoginUser(userTk, "123456");
            GoToTimPhongPage();
            SearchRoom(ngayVao, ngayRa);
            SelectRoomAndComplete(roomId);
            ConfirmBooking();

            // 2: KHÁCH HÀNG THỰC HIỆN HỦY PHÒNG
            CancelBookingUser(roomId);

            // Kiểm tra ngay trên giao diện khách hàng xem phiếu đã biến mất chưa
            if (driver.PageSource.Contains(roomId))
            {
                Assert.Fail($"Bug Giao diện Khách: Đã bấm Hủy phiếu nhưng phòng {roomId} vẫn còn lù lù trên màn hình Khách hàng!");
            }

            // Nếu là TC_016 (Chỉ test luồng khách) thì tới đây là xong
            if (testCaseId == "TC_Booking_016")
            {
                TestContext.Progress.WriteLine("=> PASS: Khách hàng hủy phòng thành công, phiếu đã biến mất khỏi màn hình.");
                Assert.Pass("Hủy phòng phía User thành công.");
                return;
            }


            // 3: CHUYỂN GIAO QUYỀN LỰC & ADMIN KIỂM TRA (Dành cho TC_023)
            LogoutUser();
            LoginAdmin(adminUser, adminPass);
            GoToAdminPhieuDatPhong();

            // Admin quét toàn bộ mã nguồn để tìm trạng thái thực sự của phòng vừa hủy
            var tableRows = driver.FindElements(By.TagName("tr"));
            bool isCancelledStatusFound = false;
            bool isBugStatusFound = false;

            foreach (var row in tableRows)
            {
                // Tìm đúng dòng của User này và Phòng này
                if (row.Text.Contains(userTk) && row.Text.Contains(roomId))
                {
                    if (row.Text.Contains("Đã hủy"))
                    {
                        isCancelledStatusFound = true;
                    }
                    else if (row.Text.Contains("Đã nhận phòng"))
                    {
                        isBugStatusFound = true; // Chộp được cái Bug hệ thống đang bị lỗi
                    }
                    break;
                }
            }

            // 4. ASSERT BÁO CÁO KẾT QUẢ
            if (isCancelledStatusFound)
            {
                TestContext.Progress.WriteLine("=> PASS: Tích hợp hoàn hảo. Admin thấy trạng thái 'Đã hủy'.");
                Assert.Pass("Đúng như mong đợi: Trạng thái đã cập nhật thành Đã Hủy.");
            }
            else if (isBugStatusFound)
            {
                // Cố tình đánh Fail màu đỏ để làm nổi bật Bug cho Dev sửa (Đúng ý đồ file CSV của bạn)
                Assert.Fail($"BUG LỚN: Khách đã hủy phòng nhưng bên Admin lại hiển thị trạng thái là 'Đã nhận phòng' (Giống hệt file CSV mô tả)!");
            }
            else
            {
                Assert.Fail($"LỖI TÍCH HỢP: Quét bảng Admin không tìm thấy phòng {roomId} hoặc trạng thái không xác định.");
            }
        }

        // NHÓM 8: TEST TÍCH HỢP (ADMIN DUYỆT PHÒNG -> KHÁCH KIỂM TRA)
        [Test, TestCaseSource(nameof(GetNhom8Data))]
        public void Test_TichHop_AdminDuyet_KhachKiemTra(string testCaseId, string testData, string expectedResult)
        {
            // 1. CHUẨN BỊ DỮ LIỆU TỪ CSV
            string roomId = ExtractValue(testData, "Phòng");
            if (string.IsNullOrEmpty(roomId)) roomId = "101"; // Setup mặc định nếu CSV trống

            string userTk = "DangQuocHung";
            string adminUser = "Admin";
            string adminPass = "admin123";

            TestContext.Progress.WriteLine($"[Chạy {testCaseId}] -> Luồng E2E Đảo ngược: Admin duyệt phòng {roomId}");

            // 1: TIỀN ĐIỀU KIỆN (KHÁCH PHẢI ĐẶT PHÒNG TRƯỚC ĐÃ)
            LoginUser(userTk, "123456");
            GoToTimPhongPage();
            SearchRoom("27/03/2026", "28/03/2026"); // Điền bừa 1 ngày hợp lệ
            SelectRoomAndComplete(roomId);
            ConfirmBooking();
            LogoutUser();

            // PHA 2: ADMIN VÀO DUYỆT PHÒNG
            LoginAdmin(adminUser, adminPass);
            AdminDuyetNhanPhong(roomId);

            // Kịch bản TC_020: Chỉ check xem Admin có thấy thông báo thành công không
            if (testCaseId == "TC_Booking_020")
            {
                if (driver.PageSource.Contains("Đã đặt phòng thành công") || driver.PageSource.Contains("thành công"))
                {
                    TestContext.Progress.WriteLine("=> PASS: Admin duyệt phòng thành công, có thông báo.");
                    Assert.Pass("Admin duyệt thành công.");
                    return;
                }
                else
                {
                    Assert.Fail("Lỗi: Admin bấm Nhận phòng xong nhưng web không báo thành công!");
                }
            }

            // PHA 3: KHÁCH HÀNG VÀO CHECK LẠI (Dành cho TC_021 và TC_022)
            LogoutUser(); // Đăng xuất Admin
            LoginUser(userTk, "123456"); // Khách hàng trở lại

            if (testCaseId == "TC_Booking_021")
            {
                // TC_021: Vào trang Phiếu đặt phòng, phòng này phải biến mất (vì đã chuyển thành Hóa đơn)
                driver.Navigate().GoToUrl("http://localhost:49921/Home/BookRoom");
                Thread.Sleep(1000);

                if (driver.PageSource.Contains(roomId))
                {
                    Assert.Fail($"BUG LOGIC: Admin đã duyệt phòng {roomId} rồi mà trang Khách hàng vẫn còn cho phép Hủy phòng!");
                }
                else
                {
                    TestContext.Progress.WriteLine("=> PASS: Phòng đã được duyệt nên tàng hình khỏi danh sách Đang chờ.");
                    Assert.Pass("Phòng đã duyệt không còn ở trạng thái chờ.");
                }
            }
            else if (testCaseId == "TC_Booking_022")
            {
                LoginUser(userTk, "123456");
                // TC_022: Vào trang Hóa đơn, kiểm tra xem có hóa đơn Chưa thanh toán không
                GoToHoaDonPage();

                // Sử dụng table id="dataTable" từ mã HTML bạn gửi
                IWebElement hoaDonTable = driver.FindElement(By.Id("dataTable"));
                var rows = hoaDonTable.FindElements(By.TagName("tr"));

                bool isInvoiceFound = false;

                // Bỏ qua dòng thead (th), duyệt các dòng tbody (td)
                for (int i = 1; i < rows.Count; i++)
                {
                    var cells = rows[i].FindElements(By.TagName("td"));
                    if (cells.Count >= 9) // Bảng của bạn có 10 cột
                    {
                        string cellSoPhong = cells[1].Text; // Cột số 2 là Số phòng
                        string cellTinhTrang = cells[8].Text; // Cột số 9 là Tình trạng

                        if (cellSoPhong.Contains(roomId) && cellTinhTrang.Contains("Chưa thanh toán"))
                        {
                            isInvoiceFound = true;
                            break;
                        }
                    }
                }

                if (isInvoiceFound)
                {
                    TestContext.Progress.WriteLine($"=> PASS: Tích hợp vòng tròn hoàn hảo! Hóa đơn phòng {roomId} đã hiển thị 'Chưa thanh toán'.");
                    Assert.Pass("Đã sinh ra hóa đơn Chưa thanh toán sau khi Admin duyệt.");
                }
                else
                {
                    Assert.Fail($"LỖI E2E: Admin đã duyệt nhưng Khách hàng không thấy Hóa đơn phòng {roomId} với trạng thái 'Chưa thanh toán'!");
                }
            }
        }


        [TearDown]
        public void TearDown()
        {
            // Kiểm tra xem driver có đang tồn tại không trước khi tắt
            if (driver != null)
            {
                try
                {
                    driver.Quit();
                }
                catch (Exception)
                {
                    // Tránh trường hợp crash lúc tắt làm hỏng kết quả test
                }
                finally
                {
                    driver.Dispose(); // Giải phóng hoàn toàn tài nguyên driver khỏi bộ nhớ máy tính
                    driver = null;    // Gán về null để đảm bảo không bị dùng lại nhầm
                }
            }
        }

        // =========================================================================
        // DANH SÁCH CÁC HÀM DÙNG CHUNG (HELPER METHODS)
        // =========================================================================

        // 1. Hàm Đăng nhập
        private void LoginUser(string username, string password)
        {
            driver.Navigate().GoToUrl("http://localhost:49921/Account/Login");

            IWebElement txtUser = driver.FindElement(By.Id("ma_kh"));
            txtUser.Clear();
            txtUser.SendKeys(username);

            IWebElement txtPass = driver.FindElement(By.Id("mat_khau"));
            txtPass.Clear();
            txtPass.SendKeys(password);

            driver.FindElement(By.CssSelector("input[type='submit'][value='Đăng Nhập']")).Click();
            Thread.Sleep(1500);
        }

        // 2. Hàm Chuyển đến trang Tìm phòng
        private void GoToTimPhongPage()
        {
            try
            {
                driver.FindElement(By.LinkText("Tìm phòng")).Click();
            }
            catch
            {
                driver.FindElement(By.CssSelector("a[href*='TimPhong']")).Click();
            }
            Thread.Sleep(1500);
        }

        // 3. Hàm Nhập ngày và ấn Tìm kiếm
        private void SearchRoom(string ngayVao, string ngayRa)
        {
            IJavaScriptExecutor js = (IJavaScriptExecutor)driver;

            if (!string.IsNullOrEmpty(ngayVao))
            {
                js.ExecuteScript($"document.getElementById('datestart').value='{ngayVao}';");
            }

            if (!string.IsNullOrEmpty(ngayRa))
            {
                js.ExecuteScript($"document.getElementById('dateend').value='{ngayRa}';");
            }

            driver.FindElement(By.CssSelector("input[type='submit'][value='Tìm Kiếm']")).Click();
            Thread.Sleep(1000);
        }

        // 4. Hàm Chọn một phòng cụ thể và ấn Hoàn Tất
        private void SelectRoomAndComplete(string roomId)
        {
            var tableRows = driver.FindElements(By.TagName("tr"));
            IWebElement btnChon = null;

            foreach (var row in tableRows)
            {
                if (row.Text.Contains(roomId))
                {
                    btnChon = row.FindElement(By.CssSelector("button[id^='btn_']"));
                    break;
                }
            }

            if (btnChon == null)
            {
                Assert.Fail($"Lỗi: Không tìm thấy phòng {roomId} trên trang FindRoom, có thể phòng đã bị đặt mất!");
            }

            IJavaScriptExecutor js = (IJavaScriptExecutor)driver;
            js.ExecuteScript("arguments[0].click();", btnChon);
            Thread.Sleep(1000);

            IWebElement btnHoanTat = driver.FindElement(By.PartialLinkText("Hoàn Tất"));
            js.ExecuteScript("arguments[0].click();", btnHoanTat);
            Thread.Sleep(2000); // Đợi load sang trang BookRoom
        }

        // 5. Hàm Xác nhận Đặt phòng tại trang BookRoom
        private void ConfirmBooking()
        {
            if (driver.Url.Contains("BookRoom"))
            {
                IWebElement btnDatPhong = driver.FindElement(By.CssSelector("input[type='submit'][value='Đặt phòng']"));
                IJavaScriptExecutor js = (IJavaScriptExecutor)driver;
                js.ExecuteScript("arguments[0].click();", btnDatPhong);
                Thread.Sleep(3000); // Đợi DB lưu và chuyển sang trang Result
            }
            else
            {
                Assert.Fail("Lỗi điều hướng: Web không chịu chuyển sang trang BookRoom!");
            }
        }

        // 6. Hàm Đăng xuất
        private void LogoutUser()
        {
            IWebElement btnLogout = driver.FindElement(By.CssSelector("a[href*='Logout']"));
            IJavaScriptExecutor js = (IJavaScriptExecutor)driver;
            js.ExecuteScript("arguments[0].click();", btnLogout);
            Thread.Sleep(1000);
            // Bắt hộp thoại xác nhận đăng xuất (nếu có)
            try { driver.SwitchTo().Alert().Accept(); Thread.Sleep(1000); } catch { }
        }

        // 7. Hàm Hack thời gian (Lùi ngay_dat về quá khứ 6 phút)
        private void HackThoiGianPhieuNhap(string username)
        {
            // TODO: Sửa lại chuỗi kết nối này cho khớp với Database QLKS thực tế trên máy bạn
            string connectionString = "Data Source=DESKTOP-3NKIPOC;Initial Catalog=dataQLKS;Integrated Security=True;TrustServerCertificate=True";

            try
            {
                using (Microsoft.Data.SqlClient.SqlConnection conn = new Microsoft.Data.SqlClient.SqlConnection(connectionString))
                {
                    conn.Open();
                    // Lệnh SQL: Tìm phiếu nháp (ma_tinh_trang = 1) của User này, trừ đi 6 phút ở cột ngay_dat
                    string query = @"UPDATE PhieuDatPhong 
                             SET ngay_dat = DATEADD(minute, -6, GETDATE()) 
                             WHERE ma_kh = @ma_kh AND ma_tinh_trang = 1";

                    using (Microsoft.Data.SqlClient.SqlCommand cmd = new Microsoft.Data.SqlClient.SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@ma_kh", username);
                        int rowsAffected = cmd.ExecuteNonQuery();

                        TestContext.Progress.WriteLine($"[Cỗ Máy Thời Gian] Đã hack thành công! Lùi thời gian {rowsAffected} phiếu nháp của '{username}' về 6 phút trước.");
                    }
                }
            }
            catch (Exception ex)
            {
                TestContext.Progress.WriteLine($"[Lỗi SQL] Không thể hack thời gian: {ex.Message}");
            }
        }

        // 8. Hàm Đăng nhập Admin
        private void LoginAdmin(string username, string password)
        {
            driver.Navigate().GoToUrl("http://localhost:49921/Admin/Index/Login");

            // Dùng ID cực kỳ ổn định thay vì XPath
            IWebElement txtUser = driver.FindElement(By.Id("tai_khoan"));
            txtUser.Clear();
            txtUser.SendKeys(username);

            IWebElement txtPass = driver.FindElement(By.Id("mat_khau"));
            txtPass.Clear();
            txtPass.SendKeys(password);

            // Dùng CssSelector bấm nút Đăng nhập Admin
            driver.FindElement(By.CssSelector("input[type='submit'][value='Đăng Nhập'], input[value='Đăng Nhập']")).Click();
            Thread.Sleep(1500);
        }

        // 9. Hàm Mở danh sách Phiếu đặt phòng (Admin)
        private void GoToAdminPhieuDatPhong()
        {
            try
            {
                // Vượt qua Menu ẩn bằng cách dùng PartialLinkText kết hợp JavaScript Click

                IWebElement menuPhieu = driver.FindElement(By.PartialLinkText("Phiếu đặt phòng"));
                ((IJavaScriptExecutor)driver).ExecuteScript("arguments[0].click();", menuPhieu);
                Thread.Sleep(1500);
            }
            catch
            {
                // Nếu menu web thiết kế kiểu Hover chuột khó click, 
                // ta ép nó nhảy thẳng URL luôn 
                TestContext.Progress.WriteLine("Điều hướng thẳng đến trang Quản lý Phiếu đặt phòng...");
                driver.Navigate().GoToUrl("http://localhost:49921/Admin/PhieuDatPhong");
                Thread.Sleep(1500);
            }
        }

        // 10. Hàm Hủy phòng ở trang Khách hàng
        private void CancelBookingUser(string roomId)
        {
            driver.Navigate().GoToUrl("http://localhost:49921/Home/BookRoom");
            Thread.Sleep(1500);

            // 1. Tìm nút "Hủy" của đúng cái phòng cần hủy
            var tableRows = driver.FindElements(By.TagName("tr"));
            IWebElement btnHuy = null;

            foreach (var row in tableRows)
            {
                if (row.Text.Contains(roomId))
                {
                    // Tìm thẻ <a> có chứa chữ Hủy hoặc class/href liên quan đến hủy
                    try { btnHuy = row.FindElement(By.PartialLinkText("Hủy")); }
                    catch { btnHuy = row.FindElement(By.CssSelector("a[href*='Delete'], a[href*='Cancel'], button")); }
                    break;
                }
            }

            if (btnHuy != null)
            {
                IJavaScriptExecutor js = (IJavaScriptExecutor)driver;
                js.ExecuteScript("arguments[0].click();", btnHuy);
                Thread.Sleep(1000);

                // 2. Xác nhận "Hủy phiếu"
                // Thường xác nhận hủy sẽ là 1 cái Alert hoặc 1 cái Modal (Hộp thoại nổi). Ta quét cả 2:
                try
                {
                    driver.SwitchTo().Alert().Accept(); // Nếu là Alert
                    Thread.Sleep(1500);
                }
                catch
                {
                    // Nếu là Hộp thoại nổi (Modal), ta tìm tất cả các nút (button/input) và click nút có chữ "Hủy phiếu"
                    var buttons = driver.FindElements(By.CssSelector("button, input[type='submit'], input[type='button']"));
                    foreach (var btn in buttons)
                    {
                        if (btn.Text.Contains("Hủy phiếu") || btn.GetAttribute("value").Contains("Hủy phiếu"))
                        {
                            js.ExecuteScript("arguments[0].click();", btn);
                            Thread.Sleep(1500);
                            break;
                        }
                    }
                }
            }
            else
            {
                Assert.Fail($"Lỗi logic: Không tìm thấy nút Hủy cho phòng {roomId}. Có thể phòng chưa được đặt!");
            }
        }

        // 11. Hàm Admin Duyệt phòng (Cập nhật chuẩn theo giao diện)
        private void AdminDuyetNhanPhong(string roomId)
        {
            // 1. Vào trang "Chọn cách đặt phòng"
            driver.Navigate().GoToUrl("http://localhost:49921/Admin/Index/ChonCachDatPhong");
            Thread.Sleep(1500);

            // 2. Click vào khối "Nhận phòng" (có hình xe đẩy/shopping cart)
            // Lợi dụng PartialLinkText hoặc quét thẻ <a> chứa chữ "Nhận phòng"
            var menuCards = driver.FindElements(By.TagName("a"));
            foreach (var card in menuCards)
            {
                if (card.Text.Contains("Nhận phòng"))
                {
                    ((IJavaScriptExecutor)driver).ExecuteScript("arguments[0].click();", card);
                    break;
                }
            }
            Thread.Sleep(1500);

            // Kiểm tra xem đã nhảy sang đúng trang PhieuDatPhong/List chưa
            if (!driver.Url.Contains("PhieuDatPhong/List"))
            {
                // Thêm phương án backup bằng cách gõ thẳng URL cho an toàn tuyệt đối
                driver.Navigate().GoToUrl("http://localhost:49921/Admin/PhieuDatPhong/List");
                Thread.Sleep(1000);
            }

            // 3. Tìm phòng trong bảng danh sách chờ
            IWebElement table = driver.FindElement(By.Id("dataTable"));
            var tableRows = table.FindElements(By.TagName("tr"));
            IWebElement btnNhanPhong_TrongBang = null;

            // Bắt đầu duyệt từ dòng thứ 2 (bỏ qua dòng Tiêu đề)
            for (int i = 1; i < tableRows.Count; i++)
            {
                var cells = tableRows[i].FindElements(By.TagName("td"));
                if (cells.Count >= 7) // Bảng của bạn có 7 cột
                {
                    string soPhong = cells[5].Text; // Cột số phòng (index 5)
                    if (soPhong.Contains(roomId))
                    {
                        // Cột cuối cùng (index 6) chứa link "Nhận Phòng"
                        btnNhanPhong_TrongBang = cells[6].FindElement(By.PartialLinkText("Nhận Phòng"));
                        break;
                    }
                }
            }

            // 4. Bấm duyệt phòng
            if (btnNhanPhong_TrongBang != null)
            {
                ((IJavaScriptExecutor)driver).ExecuteScript("arguments[0].click();", btnNhanPhong_TrongBang);
                Thread.Sleep(2000);

                // NẾU CÓ FORM "Khách đi cùng" NẰM Ở TRANG SAU (HoaDon/Add)
                // Đoạn này tự động rà quét và điền dữ liệu nếu thấy form
                var inputFields = driver.FindElements(By.CssSelector("input[type='text']"));
                if (inputFields.Count > 0)
                {
                    foreach (var input in inputFields)
                    {
                        try { input.SendKeys("Khách test tự động"); } catch { }
                    }
                    // Bấm submit (Lưu / Nhận phòng / Thêm hóa đơn)
                    try
                    {
                        IWebElement btnSubmit = driver.FindElement(By.CssSelector("input[type='submit'], button[type='submit']"));
                        ((IJavaScriptExecutor)driver).ExecuteScript("arguments[0].click();", btnSubmit);
                        Thread.Sleep(2000);
                    }
                    catch { }
                }
            }
            else
            {
                Assert.Fail($"Lỗi Admin: Không tìm thấy phòng {roomId} trong bảng 'Phiếu đặt phòng ngày hôm nay'!");
            }
        }

        // 12. Hàm Khách hàng vào trang Hóa Đơn
        private void GoToHoaDonPage()
        {
            try
            {
                // Vượt qua menu hover bằng cách điều hướng thẳng URL
                // Cấu trúc URL thông thường trong ASP.NET MVC
                driver.Navigate().GoToUrl("http://localhost:49921/Account/HoaDon");
                Thread.Sleep(1500);
            }
            catch { }
        }
    }
}