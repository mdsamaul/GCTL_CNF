using iText.Kernel.Colors;
using iText.Kernel.Events;
using iText.Kernel.Font;
using iText.Kernel.Pdf.Canvas;
using iText.Layout;
using iText.Layout.Element;
using iText.Layout.Properties;

namespace GCTL_App.Controllers.PDfFooterHandler
{
    public class PdfPrintDateHandler : IEventHandler
    {
        #region Service

        private readonly PdfFont _font;

        public PdfPrintDateHandler(PdfFont font)
        {
            _font = font;
        }

        #endregion


        #region Print Date & Time
        public void HandleEvent(Event @event)
        {
            var pdfEvent = (PdfDocumentEvent)@event;
            var pdf = pdfEvent.GetDocument();
            var page = pdfEvent.GetPage();
            var pageSize = page.GetPageSize();

            // Canvas
            var canvas = new Canvas(new PdfCanvas(page.NewContentStreamBefore(), page.GetResources(), pdf), pageSize);

            float bottomY = 12;    // footer height
            float leftX = 30;      // left margin

            string dateTime = DateTime.Now.ToString("dd/MM/yyyy hh:mm:ss tt");
            string text = $"Print Date & Time: {dateTime}";

            canvas.ShowTextAligned(
                new Paragraph(text)
                    .SetFont(_font)
                    .SetFontSize(8)
                    .SetFontColor(ColorConstants.GRAY),
                leftX,
                bottomY,
                TextAlignment.LEFT
            );

            canvas.Close();
        }

        #endregion

    }

}
