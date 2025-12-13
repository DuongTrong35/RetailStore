using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using RetailStore.Models;
using System.Linq;

public class InvoiceDocument : IDocument
{
    private readonly Order _order;

    public InvoiceDocument(Order order)
    {
        _order = order;
    }

    public DocumentMetadata GetMetadata() => DocumentMetadata.Default;

    public void Compose(IDocumentContainer container)
    {
        // ===== TÍNH TOÁN SỐ =====
        decimal discount = _order.DiscountAmount ?? 0;  // nếu null thì dùng 0
        decimal totalAmount = _order.OrderItems.Sum(i => i.Quantity * i.Price);

        string totalText = (totalAmount * 1000).ToString("N0") + " đ";
        string discountText = (discount).ToString("N0") + " đ";
        string paymentText = ((totalAmount*1000 - discount)).ToString("N0") + " đ";

        container.Page(page =>
        {
            page.Size(PageSizes.A4);
            page.Margin(25);
            page.DefaultTextStyle(x => x.FontSize(11));

            page.Content().Column(col =>
            {
                col.Spacing(10);

                // ===== HEADER =====
                col.Item().Text("HÓA ĐƠN BÁN HÀNG")
                    .FontSize(18)
                    .Bold()
                    .AlignCenter();

                col.Item().Text($"Mã đơn: {_order.OrderId}");
                col.Item().Text($"Ngày: {_order.OrderDate:dd/MM/yyyy HH:mm}");
                col.Item().Text($"Trạng thái: {_order.Status}");

                // ===== TABLE =====
                col.Item().Table(table =>
                {
                    table.ColumnsDefinition(cols =>
                    {
                        cols.RelativeColumn(4); // Sản phẩm
                        cols.RelativeColumn(1); // SL
                        cols.RelativeColumn(2); // Đơn giá
                        cols.RelativeColumn(2); // Thành tiền
                    });

                    // Header
                    table.Header(h =>
                    {
                        h.Cell().Text("Sản phẩm").Bold();
                        h.Cell().Text("SL").Bold();
                        h.Cell().Text("Đơn giá").Bold();
                        h.Cell().Text("Thành tiền").Bold();
                    });

                    // Nội dung
                    foreach (var i in _order.OrderItems)
                    {
                        table.Cell().Text(i.ProductId.ToString());
                        table.Cell().Text(i.Quantity.ToString());
                        table.Cell().Text((i.Price).ToString("N0") + ".000 đồng");
                        table.Cell().Text((i.Quantity * i.Price).ToString("N0") + ".000 đồng");
                    }
                });

                // ===== TOTAL =====
                col.Item().AlignRight().Text($"Tổng tiền: {totalText}").Bold();
                col.Item().AlignRight().Text($"Giảm giá: {discountText}");
                col.Item().AlignRight().Text($"Thanh toán: {paymentText}")
                    .FontSize(13)
                    .Bold();
            });
        });
    }
}
