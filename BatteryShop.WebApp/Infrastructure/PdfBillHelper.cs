using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using iTextSharp.text;
using iTextSharp.text.pdf;
using BatteryShop.DataAccess.Models;
using BatteryShop.DataAccess.ViewModels;

namespace BatteryShop.WebApp.Infrastructure
{
    public static class PdfBillHelper
    {
        private static readonly Font TitleFont = FontFactory.GetFont("Arial", 20, Font.BOLD, BaseColor.BLACK);
        private static readonly Font SubtitleFont = FontFactory.GetFont("Arial", 13, Font.NORMAL, BaseColor.DARK_GRAY);
        private static readonly Font HeaderFont = FontFactory.GetFont("Arial", 10, Font.BOLD, BaseColor.WHITE);
        private static readonly Font BodyFont = FontFactory.GetFont("Arial", 9, Font.NORMAL, BaseColor.BLACK);
        private static readonly Font BodyBoldFont = FontFactory.GetFont("Arial", 9, Font.BOLD, BaseColor.BLACK);
        private static readonly Font SmallFont = FontFactory.GetFont("Arial", 8, Font.NORMAL, BaseColor.DARK_GRAY);
        private static readonly BaseColor PrimaryColor = new BaseColor(13, 110, 253);
        private static readonly BaseColor LightBg = new BaseColor(248, 249, 250);

        public static byte[] GenerateBillPdf(BillModel bill, List<Dictionary<string, object>> items,
            string customerCity, List<VehicleModelViewModel> itemTypes, List<BrandListViewModel> brands)
        {
            var brandNames = brands.ToDictionary(b => b.BrandId, b => b.BrandName);
            var typeLookup = itemTypes.ToDictionary(t => t.BrandId + "-" + t.TypeId, t => t.TypeName);

            using (MemoryStream ms = new MemoryStream())
            {
                Document doc = new Document(PageSize.A4, 36, 36, 36, 36);
                PdfWriter writer = PdfWriter.GetInstance(doc, ms);
                doc.Open();

                AddHeader(doc);
                AddBillInfo(doc, bill, customerCity);
                AddItemsTable(doc, items, brandNames, typeLookup);
                AddSummary(doc, bill, items);
                AddFooter(doc);

                doc.Close();
                return ms.ToArray();
            }
        }

        private static void AddHeader(Document doc)
        {
            Paragraph title = new Paragraph("BATTERY STORE", TitleFont);
            title.Alignment = Element.ALIGN_CENTER;
            doc.Add(title);

            Paragraph subtitle = new Paragraph("Sales Invoice", SubtitleFont);
            subtitle.Alignment = Element.ALIGN_CENTER;
            subtitle.SpacingAfter = 6;
            doc.Add(subtitle);

            Chunk line = new Chunk(new iTextSharp.text.pdf.draw.LineSeparator(0.5f, 100, PrimaryColor, Element.ALIGN_CENTER, -2));
            doc.Add(line);
            doc.Add(Chunk.NEWLINE);
        }

        private static void AddBillInfo(Document doc, BillModel bill, string customerCity)
        {
            PdfPTable infoTable = new PdfPTable(2);
            infoTable.WidthPercentage = 100;
            infoTable.SetWidths(new float[] { 50, 50 });
            infoTable.SpacingAfter = 10;

            infoTable.AddCell(CreateInfoCell("Bill #", bill.BillId.ToString()));
            infoTable.AddCell(CreateInfoCell("Date", bill.DateOfSale.ToString("dd-MMM-yyyy")));
            infoTable.AddCell(CreateInfoCell("Customer", bill.UserFullName));
            infoTable.AddCell(CreateInfoCell("Phone", bill.UserPhone));
            infoTable.AddCell(CreateInfoCell("City", customerCity));
            infoTable.AddCell(CreateInfoCell("", ""));

            doc.Add(infoTable);
        }

        private static PdfPCell CreateInfoCell(string label, string value)
        {
            PdfPCell cell = new PdfPCell();
            cell.Border = Rectangle.NO_BORDER;
            cell.PaddingBottom = 3;

            if (!string.IsNullOrEmpty(label))
            {
                cell.AddElement(new Phrase(label + ": ", BodyBoldFont));
            }
            cell.AddElement(new Phrase(value, BodyFont));
            return cell;
        }

        private static void AddItemsTable(Document doc, List<Dictionary<string, object>> items,
            Dictionary<int, string> brandNames, Dictionary<string, string> typeLookup)
        {
            PdfPTable table = new PdfPTable(7);
            table.WidthPercentage = 100;
            table.SetWidths(new float[] { 5, 22, 18, 12, 12, 12, 19 });
            table.SpacingAfter = 10;
            table.HeaderRows = 1;

            string[] headers = { "#", "Serial No", "Brand / Type", "Price (₹)", "Disc %", "Disc Amt", "Final (₹)" };
            foreach (string h in headers)
            {
                PdfPCell cell = new PdfPCell(new Phrase(h, HeaderFont));
                cell.BackgroundColor = PrimaryColor;
                cell.HorizontalAlignment = Element.ALIGN_CENTER;
                cell.Padding = 5;
                table.AddCell(cell);
            }

            int index = 1;
            decimal subtotal = 0, totalTradein = 0, totalDiscount = 0, totalFinal = 0;

            foreach (var item in items)
            {
                string serial = GetString(item, "itemSerialNumber");
                int brandId = GetInt(item, "BrandId");
                int typeId = GetInt(item, "itemTypeId");
                decimal price = GetDecimal(item, "itemPrice");
                decimal oldPrice = GetDecimal(item, "oldItemPrice");
                decimal discPct = GetDecimal(item, "discountPercentage");
                bool hasOldItem = item.ContainsKey("oldItemId") && item["oldItemId"] != DBNull.Value;

                string brandName = brandNames.ContainsKey(brandId) ? brandNames[brandId] : "Unknown";
                string typeKey = brandId + "-" + typeId;
                string typeName = typeLookup.ContainsKey(typeKey) ? typeLookup[typeKey] : "";
                string brandType = string.IsNullOrEmpty(typeName) ? brandName : brandName + " - " + typeName;

                decimal discAmt = hasOldItem ? 0 : Math.Round(price * discPct / 100, 2);
                decimal tradein = hasOldItem ? oldPrice : 0;
                decimal finalPrice = hasOldItem ? price - oldPrice : price - discAmt;

                subtotal += price;
                totalTradein += tradein;
                totalDiscount += discAmt;
                totalFinal += finalPrice;

                AddCell(table, index.ToString(), Element.ALIGN_CENTER);
                AddCell(table, serial, Element.ALIGN_CENTER);
                AddCell(table, brandType, Element.ALIGN_LEFT);
                AddCell(table, price.ToString("N2"), Element.ALIGN_RIGHT);
                AddCell(table, hasOldItem ? "-" : discPct.ToString("N0"), Element.ALIGN_CENTER);
                AddCell(table, discAmt > 0 ? discAmt.ToString("N2") : "-", Element.ALIGN_RIGHT);
                AddCell(table, finalPrice.ToString("N2"), Element.ALIGN_RIGHT);

                index++;
            }

            doc.Add(table);
        }

        private static void AddSummary(Document doc, BillModel bill, List<Dictionary<string, object>> items)
        {
            PdfPTable table = new PdfPTable(2);
            table.WidthPercentage = 50;
            table.HorizontalAlignment = Element.ALIGN_RIGHT;
            table.SetWidths(new float[] { 60, 40 });
            table.SpacingAfter = 10;

            AddSummaryRow(table, "Subtotal", items.Sum(i => GetDecimal(i, "itemPrice")).ToString("N2"), false);
            decimal tradein = items.Sum(i => i.ContainsKey("oldItemId") && i["oldItemId"] != DBNull.Value ? GetDecimal(i, "oldItemPrice") : 0);
            if (tradein > 0)
            {
                AddSummaryRow(table, "Less Trade-in", "(" + tradein.ToString("N2") + ")", false);
            }
            decimal discount = items.Sum(i =>
            {
                bool hasOld = i.ContainsKey("oldItemId") && i["oldItemId"] != DBNull.Value;
                return hasOld ? 0 : Math.Round(GetDecimal(i, "itemPrice") * GetDecimal(i, "discountPercentage") / 100, 2);
            });
            if (discount > 0)
            {
                AddSummaryRow(table, "Less Warranty Discount", "(" + discount.ToString("N2") + ")", false);
            }

            AddSummaryRow(table, "Total Amount", bill.TotalAmount.ToString("N2"), true);
            AddSummaryRow(table, "Paid Amount", bill.PaidAmount.ToString("N2"), false);
            AddSummaryRow(table, "Due Amount", bill.DueAmount.ToString("N2"), true);

            doc.Add(table);
        }

        private static void AddSummaryRow(PdfPTable table, string label, string value, bool bold)
        {
            Font f = bold ? BodyBoldFont : BodyFont;
            PdfPCell labelCell = new PdfPCell(new Phrase(label, f));
            labelCell.Border = Rectangle.NO_BORDER;
            labelCell.PaddingBottom = 3;
            labelCell.PaddingRight = 10;
            labelCell.HorizontalAlignment = Element.ALIGN_RIGHT;
            table.AddCell(labelCell);

            PdfPCell valueCell = new PdfPCell(new Phrase(value, f));
            valueCell.Border = Rectangle.NO_BORDER;
            valueCell.PaddingBottom = 3;
            valueCell.HorizontalAlignment = Element.ALIGN_RIGHT;
            table.AddCell(valueCell);
        }

        private static void AddFooter(Document doc)
        {
            Chunk line = new Chunk(new iTextSharp.text.pdf.draw.LineSeparator(0.5f, 100, PrimaryColor, Element.ALIGN_CENTER, -2));
            doc.Add(line);
            doc.Add(Chunk.NEWLINE);

            Paragraph generated = new Paragraph("Generated on: " + DateTime.Now.ToString("dd-MMM-yyyy hh:mm tt"), SmallFont);
            generated.Alignment = Element.ALIGN_CENTER;
            doc.Add(generated);

            Paragraph powered = new Paragraph("Battery Store - Billing System", SmallFont);
            powered.Alignment = Element.ALIGN_CENTER;
            doc.Add(powered);
        }

        private static void AddCell(PdfPTable table, string text, int alignment)
        {
            PdfPCell cell = new PdfPCell(new Phrase(text, BodyFont));
            cell.HorizontalAlignment = alignment;
            cell.Padding = 4;
            cell.VerticalAlignment = Element.ALIGN_MIDDLE;
            if (table.Rows.Count % 2 == 0)
                cell.BackgroundColor = LightBg;
            table.AddCell(cell);
        }

        private static string GetString(Dictionary<string, object> dict, string key)
        {
            return dict.ContainsKey(key) && dict[key] != DBNull.Value ? dict[key].ToString() : "";
        }

        private static int GetInt(Dictionary<string, object> dict, string key)
        {
            if (dict.ContainsKey(key) && dict[key] != DBNull.Value)
                return Convert.ToInt32(dict[key]);
            return 0;
        }

        private static decimal GetDecimal(Dictionary<string, object> dict, string key)
        {
            if (dict.ContainsKey(key) && dict[key] != DBNull.Value)
                return Convert.ToDecimal(dict[key]);
            return 0;
        }
    }
}
