using NUnit.Framework;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using System;

namespace QLKS.Tests
{
    public class TrangChuTests
    {
        private IWebDriver driver;

        [SetUp]
        public void Setup()
        {
            // Mở trình duyệt Chrome
            driver = new ChromeDriver();
            driver.Manage().Window.Maximize();

            // Thiết lập thời gian chờ ngầm định là 10 giây 
            // (Phòng trường hợp web load hình ảnh hơi lâu)
            driver.Manage().Timeouts().ImplicitWait = TimeSpan.FromSeconds(10);
        }

        [Test]
        public void TestHienThiTrangChuThanhCong()
        {
            // 1. Điều hướng tới trang chủ đang chạy trên localhost
            driver.Navigate().GoToUrl("http://localhost:49921/");

            // 2. Kiểm tra Title của trình duyệt
            // Chú ý: Do Layout của bạn có thể nối thêm tên web (ví dụ: "Trang Chủ - QLKS") 
            // nên mình dùng hàm Contains() để kiểm tra cho chắc chắn.
            Assert.IsTrue(driver.Title.Contains("Trang Chủ"), "Lỗi: Tiêu đề trang không chứa chữ 'Trang Chủ'!");

            // 3. Tìm thẻ h1 có class 'cursive-font' chứa tên Homestay
            // Sử dụng XPath để tìm chính xác thẻ html này
            IWebElement titleElement = driver.FindElement(By.XPath("//h1[contains(@class, 'cursive-font')]"));

            // 4. Xác nhận nội dung text bên trong thẻ h1 có đúng không
            Assert.AreEqual("LengKeng HOMESTAY!", titleElement.GetAttribute("textContent"), "Lỗi: Không tìm thấy dòng chữ LengKeng HOMESTAY!");
        }

        [TearDown]
        public void TearDown()
        {
            // Đóng trình duyệt và dọn rác
            driver.Quit();
            driver.Dispose();
        }
    }
}