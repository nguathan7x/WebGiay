using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using QuestPDF.Previewer;

namespace WebGiayOnline.Models;

    public class InvoiceDocument : IDocument
{
    public string OrderId { get; set; }
    public string Customer { get; set; }
    public decimal Amount { get; set; }
    public string OrderInfo { get; set; }
    public DateTime PaymentTime { get; set; }

    public DocumentMetadata GetMetadata() => DocumentMetadata.Default;

    public void Compose(IDocumentContainer container)
    {
        container.Page(page =>
        {
            page.Size(PageSizes.A5);
            page.Margin(30);
            page.DefaultTextStyle(x => x.FontSize(14));

            page.Content()
                .Column(col =>
                {
                    col.Item().PaddingBottom(20).Text("🧾 HÓA ĐƠN THANH TOÁN").Bold().FontSize(20).AlignCenter();

                    //col.Item().Text("🧾 HÓA ĐƠN THANH TOÁN").Bold().FontSize(20).AlignCenter().SpacingBottom(20);

                    col.Item().Text($"Khách hàng: {Customer}");
                    col.Item().Text($"Mã đơn hàng: {OrderId}");
                    col.Item().Text($"Số tiền: {Amount:C0}");
                    col.Item().Text($"Nội dung: {OrderInfo}");
                    col.Item().Text($"Thời gian thanh toán: {PaymentTime:dd/MM/yyyy HH:mm:ss}");
                });
        });
    }
}
