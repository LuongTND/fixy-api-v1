# Kế hoạch Triển khai Chi tiết Phase 2: Rút Tiền KTV (VietQR Động + Tự động Đối Soát 100% qua SePay Webhook)

## 🎯 Tổng quan Giải pháp

Hệ thống **Fixy** xử lý rút tiền cho KTV theo cơ chế **Chuyển tiền VietQR Động + Tự động Đối soát 100% bằng SePay Webhook**:

1. **Khách hàng thanh toán**: Tiền nạp/thanh toán dịch vụ vào thẳng Tài khoản Ngân hàng của Admin (qua VietQR PayOS/VnPay/MoMo).
2. **KTV gửi yêu cầu rút tiền**:
   - Hệ thống kiểm tra số dư ví KTV, trừ tạm số dư (Hold money).
   - Tự động sinh **Mã rút tiền độc nhất** (Ví dụ: `WD1008A2`).
   - Tự động tạo **URL VietQR Động** chứa sẵn: Số tài khoản KTV, Ngân hàng KTV, Số tiền rút và Nội dung chuyển khoản bắt buộc `FIXY RUT WD1008A2`.
3. **Admin duyệt & chuyển tiền**:
   - Web Admin hiển thị **Modal VietQR Động** kèm mã QR nét cao và nút Copy nhanh nội dung.
   - Admin mở App ngân hàng bất kỳ (MBBank, Vietcombank, Techcombank, VPBank,...) quét mã QR trong 1 giây -> Tiền chuyển thẳng từ TK Admin sang STK KTV.
4. **Tự động Đối soát 100% qua SePay Webhook**:
   - Ngay khi tài khoản ngân hàng của Admin bị trừ tiền, dịch vụ **SePay (sepay.vn)** bắt biến động số dư tiền ra (`transferType == "out"`) và bắn HTTP POST Webhook về Fixy Backend trong 1-2 giây.
   - Backend xác thực Secret Token, trích xuất mã `WD1008A2` bằng Regex, đổi trạng thái đơn sang `Approved`, lưu mã đối soát ngân hàng thật (`GatewayTransactionRef` - VD: `FT260810998`).
   - Backend phát tín hiệu qua **SignalR Hub** & **FCM Push Notification** cho cả Web Admin và KTV Mobile App.
   - **Tự động hoàn toàn**: Admin không cần bấm duyệt thủ công lần thứ hai, không cần chụp ảnh hay tải biên lai ngân hàng!

---

## ⚠️ User Review Required

> [!IMPORTANT]
> **Cấu hình SePay Webhook & Secret Token**:
> - Cần thêm cấu hình `SePay:WebhookToken` trong `appsettings.json` (hoặc `appsettings.Development.json`) để bảo mật Webhook endpoint `/api/payment/webhook/sepay`.
> - SePay sẽ gửi header `Authorization: Bearer <SePay_WebhookToken>` khi gọi Webhook.

> [!NOTE]
> **Quy tắc Sinh Mã Rút Tiền (PayoutCode)**:
> - Định dạng: `WD` + 6 ký tự viết hoa alphanumeric độc nhất (VD: `WD889211`, `WD1008A2`).
> - Tiền tố `WD` giúp Regex dễ dàng trích xuất chính xác mã rút tiền từ nội dung tin nhắn chuyển khoản của ngân hàng.

---

## ❓ Open Questions

- Không có open question tồn tại. Cơ chế đã được thống nhất 100% tự động qua SePay Webhook.

---

## 🏗️ Proposed Changes (Chi tiết Triển khai Theo Component)

### 1. Database & Domain Models (Backend)

#### [MODIFY] [PayoutRequest.cs](file:///e:/fixy-api-v1/Domain/Entity/PayoutRequest.cs)
- Bổ sung 3 trường phục vụ đối soát & quét mã VietQR:
  ```csharp
  public string PayoutCode { get; set; } = string.Empty;       // Mã rút tiền duy nhất (VD: WD1008A2)
  public string? GatewayTransactionRef { get; set; }          // Mã giao dịch ngân hàng thực tế từ SePay (VD: FT260810998)
  public string? VietQrUrl { get; set; }                      // URL ảnh VietQR Động
  ```

#### [NEW] Migration EF Core
- Chạy lệnh migration thêm cột vào bảng `PayoutRequests`:
  - `PayoutCode` (nvarchar(32), Indexed)
  - `GatewayTransactionRef` (nvarchar(128), Nullable)
  - `VietQrUrl` (nvarchar(512), Nullable)

---

### 2. DTOs & Application Layer (Backend)

#### [MODIFY] [PayoutRequestDto.cs](file:///e:/fixy-api-v1/Application/DTOs/Payout/PayoutRequestDto.cs)
- Cập nhật DTO trả về cho Web Admin và Mobile App:
  ```csharp
  public class PayoutRequestDto
  {
      public Guid Id { get; set; }
      public string PayoutCode { get; set; } = string.Empty;
      public long Amount { get; set; }
      public string Status { get; set; } = string.Empty;
      public string? RejectReason { get; set; }
      public string? GatewayTransactionRef { get; set; }
      public string? VietQrUrl { get; set; }
      public DateTime CreatedDate { get; set; }
      public DateTime? TransferredAt { get; set; }
      public string AccountNumber { get; set; } = string.Empty;
      public string AccountName { get; set; } = string.Empty;
      public string? BankName { get; set; }
      public string? BankCode { get; set; }
  }
  ```

#### [NEW] [SePayWebhookDto.cs](file:///e:/fixy-api-v1/Application/DTOs/Payment/SePayWebhookDto.cs)
- DTO chuẩn nhận payload biến động số dư từ SePay:
  ```csharp
  namespace Application.DTOs.Payment
  {
      public class SePayWebhookDto
      {
          public long Id { get; set; }                           // ID giao dịch trên SePay
          public string Gateway { get; set; } = string.Empty;    // Tên ngân hàng (MBBank, VCB, TCB...)
          public string AccountNumber { get; set; } = string.Empty; // STK ngân hàng Admin bị trừ tiền
          public string TransferType { get; set; } = string.Empty;  // "out" (tiền ra/chi hộ KTV)
          public long TransferAmount { get; set; }               // Số tiền trừ
          public string Content { get; set; } = string.Empty;       // Nội dung chuyển khoản (chứa FIXY RUT WDxxxxx)
          public string? ReferenceCode { get; set; }             // Mã đối soát FT... của ngân hàng
          public string? TransactionDate { get; set; }           // Thời gian giao dịch ngân hàng
      }
  }
  ```

#### [MODIFY] [IPayoutService.cs](file:///e:/fixy-api-v1/Application/Interfaces/Services/IPayoutService.cs)
- Bổ sung method xử lý Webhook SePay đối soát rút tiền:
  ```csharp
  Task<OperationResult> ProcessSePayWebhookAsync(SePayWebhookDto webhook, string? secretToken, CancellationToken cancellationToken);
  ```

---

### 3. Business Logic & Services (Backend Infrastructure)

#### [MODIFY] [PayoutService.cs](file:///e:/fixy-api-v1/Infrastructure/Services/PayoutService.cs)
1. **Sinh `PayoutCode` & `VietQrUrl` trong `CreateRequestAsync`**:
   - Tạo mã `PayoutCode` ngẫu nhiên dạng `WD` + 6 ký tự Alphanumeric không trùng lặp.
   - Tạo `VietQrUrl` theo VietQR Quick Link Format:
     `https://img.vietqr.io/image/{bankCode}-{accountNumber}-compact2.png?amount={amount}&addInfo=FIXY%20RUT%20{payoutCode}&accountName={accountName}`
   - Lưu `PayoutCode` và `VietQrUrl` vào database.

2. **Cập nhật `GetAllAsync` & `GetMyRequestsAsync`**:
   - Mapping đầy đủ `PayoutCode`, `GatewayTransactionRef`, `VietQrUrl`, `BankCode` vào `PayoutRequestDto`.

3. **Triển khai `ProcessSePayWebhookAsync`**:
   - **Xác thực Secret Token**: Kiểm tra header `Authorization` so với config `SePay:WebhookToken`.
   - **Lọc loại giao dịch**: Bỏ qua nếu `TransferType != "out"` (chỉ xử lý tiền ra/chi hộ).
   - **Regex trích xuất `PayoutCode`**: Tìm mẫu `WD[A-Z0-9]{6}` trong chuỗi `webhook.Content`.
   - **Idempotency (Chống xử lý trùng)**: Tìm `PayoutRequest` theo `PayoutCode`. Nếu `Status == Approved`, trả về Success ngay lập tức mà không xử lý lại.
   - **Cập nhật trạng thái**:
     - `PayoutRequest.Status = PayoutRequestStatus.Approved`
     - `PayoutRequest.TransferredAt = DateTime.UtcNow`
     - `PayoutRequest.GatewayTransactionRef = webhook.ReferenceCode ?? webhook.Id.ToString()`
     - Cập nhật `WalletTransaction.Status = TransactionStatus.Success`
     - Cập nhật `Wallet.LifetimeSpent += request.Amount`
   - **Gửi Thông báo Real-time**:
     - Gọi `_notificationService.SendNotificationAsync(...)` gửi FCM Push + SignalR cho KTV.
     - Phát sự kiện SignalR qua `_hubContext.Clients.All.SendAsync("PayoutApproved", payload)` cho Web Admin dashboard tự động cập nhật UI màu xanh.

---

### 4. API Controllers & Routing (Backend API)

#### [MODIFY] [PaymentController.cs](file:///e:/fixy-api-v1/API/Controllers/PaymentController.cs)
- Thêm route `POST /api/payment/webhook/sepay`:
  ```csharp
  [HttpPost("webhook/sepay")]
  public async Task<IActionResult> HandleSePayWebhook(
      [FromBody] SePayWebhookDto webhook,
      [FromHeader(Name = "Authorization")] string? authorizationHeader,
      CancellationToken cancellationToken
  )
  {
      var result = await _payoutService.ProcessSePayWebhookAsync(webhook, authorizationHeader, cancellationToken);
      return HandleResult(result);
  }
  ```

---

### 5. Web Admin Dashboard (`fixy-fe-v1`)

#### [MODIFY] [payout.api.js](file:///e:/fixy-api-v1/fixy-fe-v1/src/apis/payout.api.js)
- Giữ nguyên các hàm API hiện có, dữ liệu trả về từ `getAll` sẽ có sẵn `payoutCode`, `vietQrUrl`, `gatewayTransactionRef`.

#### [MODIFY] [page.jsx](file:///e:/fixy-api-v1/fixy-fe-v1/src/app/(main)/dashboard/finance/page.jsx)
1. **Bảng Yêu Cầu Rút Tiền**:
   - Hiển thị thêm cột **Mã rút tiền** (`PayoutCode` - VD: `WD1008A2`) với nhãn dạng Tag dán mã nổi bật.
   - Cột Thao tác: Thêm nút **"Quét VietQR chuyển tiền"** màu cam/xanh bên cạnh nút "Từ chối".

2. **Modal Quét VietQR Động Chi Tiết**:
   - Khi Admin bấm "Quét VietQR", hiển thị Modal chứa:
     - Ảnh **VietQR Động** cỡ lớn (`vietQrUrl`).
     - Thông tin chi tiết KTV: Tên KTV, Số tài khoản, Ngân hàng nhận, Số tiền rút.
     - Khung **Nội dung chuyển khoản chuẩn**: `FIXY RUT WD1008A2` kèm nút **Copy 1-Click**.
     - Vùng trạng thái Realtime: *"Đang chờ hệ thống tự động đối soát qua SePay..."* với hiệu ứng Spinner/Pulse.

3. **Tích hợp SignalR Realtime Listener**:
   - Lắng nghe sự kiện `PayoutApproved` từ SignalR.
   - Ngay khi SePay gửi webhook và Backend xử lý xong:
     - Modal VietQR tự động đổi sang trạng thái **Thành công (Tích xanh)** và tự đóng sau 2 giây.
     - Bảng Yêu cầu rút tiền tự động cập nhật dòng dữ liệu sang nhãn xanh **"Đã thanh toán"** kèm mã đối soát ngân hàng `GatewayTransactionRef` mà không cần reload trang.

---

### 6. Mobile App KTV (`fixy-fe-mobile-v1`)

#### [MODIFY] [workers.ts](file:///e:/fixy-api-v1/fixy-fe-mobile-v1/services/api/workers.ts)
- Nâng cấp type `PayoutRequest`:
  ```typescript
  export type PayoutRequest = {
    id: string;
    payoutCode: string;
    amount: number;
    status: number;
    rejectReason?: string;
    gatewayTransactionRef?: string;
    vietQrUrl?: string;
    createdDate: string;
    transferredAt?: string;
    payoutAccount?: PayoutAccount;
  };
  ```

#### [MODIFY] [worker-wallet.tsx](file:///e:/fixy-api-v1/fixy-fe-mobile-v1/app/(worker)/worker-wallet.tsx)
1. **Hiển thị Mã Rút Tiền & Đối Soát Ngân Hàng trong Lịch Sử**:
   - Với mỗi item lịch sử rút tiền:
     - Hiển thị **Mã yêu cầu**: `WD1008A2`.
     - Nếu đã chuyển tiền thành công (`status == Approved`), hiển thị nhãn xanh kèm **Mã giao dịch ngân hàng**: `FT260810998` (Mã gốc từ ngân hàng do SePay trả về).
2. **Push Notification real-time**:
   - Tự động làm mới số dư và lịch sử giao dịch khi KTV mở ứng dụng hoặc khi nhận được thông báo rút tiền thành công từ FCM Push Notification.

---

## 🧪 Verification Plan (Kiểm thử Tự Động & Thủ Công)

### Automated Build & Test
- Kiểm tra biên dịch backend: `dotnet build e:\fixy-api-v1\API\API.csproj`
- Kiểm tra EF Core migration: `dotnet ef database update --project Infrastructure --startup-project API`

### Manual End-to-End Verification
1. **Tạo Yêu Cầu Rút Tiền (Mobile App KTV)**:
   - KTV gửi yêu cầu rút 100.000đ từ App Mobile.
   - Kiểm tra DB: Yêu cầu được tạo với `Status = Pending`, `PayoutCode = WDxxxxxx` và `VietQrUrl` hợp lệ.
2. **Xem & Quét VietQR (Web Admin)**:
   - Admin vào trang **Tài Chính & Giải Ngân** trên Web Admin.
   - Bấm nút **"Quét VietQR"** -> Mở Modal VietQR Động.
   - Kiểm tra ảnh QR hiển thị đầy đủ thông tin: STK, Số tiền 100.000đ, Nội dung `FIXY RUT WDxxxxxx`.
3. **Giả lập / Gọi SePay Webhook**:
   - Bấm chuyển tiền thật từ App ngân hàng (hoặc Postman gửi request `POST /api/payment/webhook/sepay` kèm secret token và nội dung `FIXY RUT WDxxxxxx`).
   - Backend xác thực token, trích xuất `WDxxxxxx`, cập nhật `Status = Approved`, ghi nhận `GatewayTransactionRef`.
4. **Kiểm tra Realtime Sync**:
   - Modal trên Web Admin lập tức hiển thị màu xanh "Đã thanh toán thành công".
   - App Mobile KTV nhận Push Notification *"Rút tiền thành công! 💸 100.000đ đã về tài khoản. Mã ngân hàng: FT..."* và số dư khả dụng được cập nhật chuẩn xác.
