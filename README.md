# AssetmentTest
Dưới đây là danh sách toàn bộ các tính năng, cơ chế đã được thực hiện:

---
1. Thay đổi skin từ các vật phẩm sang cá

---

2. Thay đổi Gameplay
- Cơ chế nhặt vật phẩm: Người chơi sẽ nhấn vào vật phẩm để đưa chúng xuống một thanh chứa (Slot Bar) nằm dưới cùng màn hình.
-Thanh chứa (Slot Bar):
  + Sức chứa tối đa là 5.
  + Bất cứ khi nào có 3 vật phẩm giống nhau xuất hiện trong thanh này, chúng sẽ tự động bị loại bỏ (match).
- Điều kiện Thắng/Thua:
  + Thắng: Khi người chơi nhặt sạch toàn bộ vật phẩm trên bảng.
  + Thua: Khi thanh chứa lấp đầy 5 ô mà không có bộ 3 nào được tạo ra.
- Đảm bảo quy luật sinh vật phẩm: Thuật toán sinh: `Board.Fill()` được tinh chỉnh để đảm bảo tổng số lượng mỗi loại vật phẩm trên bảng luôn chia hết cho 3, đồng thời luôn có đủ 7 loại cá trên bảng.

---

3. Hệ thống Giao diện và Màn hình Kết quả

- Tạo các nút Auto Win, Auto Lose, Time Attack trên màn hình Home Menu.
- Hiển thị màn hình LEVEL WIN khi người chơi thắng.
- Hiển thị màn hình GAME OVER khi người chơi thua.

---

4. Thêm Hiệu ứng
- Hiệu ứng khi nhấn: Khi nhấn vào một con cá trên bảng, nó không trượt thẳng xuống mà sẽ nhảy vòng cung (`DOJump`) rớt xuống thanh Slot Bar bên dưới.
- Hiệu ứng Gom & Nổ: Khi có 3 con cá giống nhau ở thanh Slot Bar, thay vì biến mất ngay lập tức, 2 con cá ở hai bên sẽ trượt nhanh về vị trí con cá ở giữa, phóng to nhẹ lên và cuối cùng thu nhỏ lại biến mất hoàn toàn. Các con cá còn lại sẽ tự động trượt lấp vào chỗ trống sau khi vụ nổ kết thúc.


---

5. Thêm Chế độ Tự động Chơi
- Auto Win: mỗi 0.5s, tìm và nhặt các con cá cùng loại với các con cá đang có sẵn trong Slot Bar để ghép thành bộ 3 nhanh nhất.
- Auto Lose: mỗi 0.5s, tìm và nhặt các con cá khác loại nhau làm thanh Slot Bar đầy và dẫn đến màn hình Game Over.

---

6. Thêm Chế độ Time Attack
- Đồng hồ đếm ngược: Người chơi chỉ có đúng 60 giây để dọn sạch bảng.
- Luật chơi : Nếu thanh Slot Bar bị lấp đầy 5 ô, trò chơi không lập tức Game Over. Thay vào đó, người chơi sẽ bị khóa không thể nhặt thêm cá trên bảng. Trò chơi chỉ Game Over khi hết 60 giây.
- Cơ chế Return to Board: Để gỡ rối khi Slot Bar bị đầy, người chơi có thể Click vào một con cá bất kỳ trên Slot Bar. Con cá đó sẽ tự động nhảy ngược lại đúng ô vuông gốc trên bảng ban đầu của nó, để lại một chỗ trống trên Slot Bar giúp người chơi tiếp tục tìm kiếm bộ 3 mới. 
