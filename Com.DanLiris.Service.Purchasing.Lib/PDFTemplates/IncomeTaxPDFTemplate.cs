using Com.DanLiris.Service.Purchasing.Lib.Helpers;
using Com.DanLiris.Service.Purchasing.Lib.ViewModels.GarmentInvoiceViewModels;
using iTextSharp.text;
using iTextSharp.text.pdf;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;

namespace Com.DanLiris.Service.Purchasing.Lib.PDFTemplates
{
	public class IncomeTaxPDFTemplate
	{
		 
		
	public MemoryStream GeneratePdfTemplate(GarmentInvoiceViewModel viewModel, int clientTimeZoneOffset)
		{
			Font header_font = FontFactory.GetFont(BaseFont.HELVETICA, BaseFont.CP1250, BaseFont.NOT_EMBEDDED, 18);
			Font normal_font = FontFactory.GetFont(BaseFont.HELVETICA, BaseFont.CP1250, BaseFont.NOT_EMBEDDED, 9);
			Font bold_font = FontFactory.GetFont(BaseFont.HELVETICA_BOLD, BaseFont.CP1250, BaseFont.NOT_EMBEDDED, 8);
			//Font header_font = FontFactory.GetFont(BaseFont.HELVETICA, BaseFont.CP1250, BaseFont.NOT_EMBEDDED, 8);

			Document document = new Document(PageSize.A4, 40, 40, 40, 40);
			document.AddHeader("Header", viewModel.incomeTaxNo);
			MemoryStream stream = new MemoryStream();
			PdfWriter writer = PdfWriter.GetInstance(document, stream);
			writer.PageEvent = new PDFPages();
			document.Open();

			Chunk chkHeader = new Chunk(" ");
			Phrase pheader = new Phrase(chkHeader);
			HeaderFooter header = new HeaderFooter(pheader, false);
			header.Border = Rectangle.NO_BORDER;
			header.Alignment = Element.ALIGN_RIGHT;
			document.Header = header;


			#region Header

			PdfPTable tableHeader = new PdfPTable(1);
			tableHeader.SetWidths(new float[] { 4f });
			PdfPCell cellHeaderContentLeft = new PdfPCell() { Border =Rectangle.NO_BORDER, HorizontalAlignment = Element.ALIGN_LEFT };
			//PdfPCell cellHeaderContentRight = new PdfPCell() { Border = Rectangle.NO_BORDER, HorizontalAlignment = Element.ALIGN_RIGHT };

			cellHeaderContentLeft.Phrase = new Phrase("PT DAN LIRIS" + "\n" + "Head Office: Kelurahan Banaran" + "\n" + "Kecamatan Grogol" + "\n" + "Sukoharjo 57193 - INDONESIA" + "\n" + "PO.BOX 166 Solo 57100" + "\n" + "Telp. (0271) 740888, 714400" + "\n" + "Fax. (0271) 735222, 740777", bold_font);
			tableHeader.AddCell(cellHeaderContentLeft);
			
			//string noPO = viewModel.Supplier.Import ? "FM-PB-00-06-009/R1" + "\n" + "PO: " + EPONo  : "PO: " + EPONo;

			 
			PdfPCell cellHeader = new PdfPCell(tableHeader); // dont remove
			tableHeader.ExtendLastRow = false;
			tableHeader.SpacingAfter = 10f;
			document.Add(tableHeader);

			string titleString = "NOTA PAJAK PPH\n\n";
			Paragraph title = new Paragraph(titleString, bold_font) { Alignment = Element.ALIGN_CENTER };
			document.Add(title);
			bold_font.SetStyle(Font.NORMAL);

			PdfPTable tableIncomeTax = new PdfPTable(3);
			tableIncomeTax.SetWidths(new float[] { 1.2f, 4f, 4f });
			PdfPCell cellTaxLeft = new PdfPCell() { Border = Rectangle.NO_BORDER, HorizontalAlignment = Element.ALIGN_LEFT };
			cellTaxLeft.Phrase = new Phrase("No. Nota Pajak :", normal_font);
			tableIncomeTax.AddCell(cellTaxLeft);

			cellTaxLeft.Phrase = new Phrase(viewModel.incomeTaxNo, normal_font);
			tableIncomeTax.AddCell(cellTaxLeft);

				 

			PdfPCell cellSupplier = new PdfPCell(tableIncomeTax); // dont remove
			tableIncomeTax.ExtendLastRow = false;
			tableIncomeTax.SpacingAfter = 10f;
			document.Add(tableIncomeTax);
			#endregion
			#region data
			PdfPCell cellCenter = new PdfPCell() { Border = Rectangle.TOP_BORDER | Rectangle.LEFT_BORDER | Rectangle.BOTTOM_BORDER | Rectangle.RIGHT_BORDER, HorizontalAlignment = Element.ALIGN_CENTER, VerticalAlignment = Element.ALIGN_MIDDLE, Padding = 5 };
			PdfPCell cellRight = new PdfPCell() { Border = Rectangle.TOP_BORDER | Rectangle.LEFT_BORDER | Rectangle.BOTTOM_BORDER | Rectangle.RIGHT_BORDER, HorizontalAlignment = Element.ALIGN_RIGHT, VerticalAlignment = Element.ALIGN_MIDDLE, Padding = 5 };
			PdfPCell cellLeft = new PdfPCell() { Border = Rectangle.TOP_BORDER | Rectangle.LEFT_BORDER | Rectangle.BOTTOM_BORDER | Rectangle.RIGHT_BORDER, HorizontalAlignment = Element.ALIGN_LEFT, VerticalAlignment = Element.ALIGN_MIDDLE, Padding = 5 };

			PdfPTable tableContent = new PdfPTable(6);
			tableContent.SetWidths(new float[] { 4.5f, 4f, 3.5f, 4f, 3.2f, 5f });
			cellCenter.Phrase = new Phrase("No Surat Jalan", bold_font);
			tableContent.AddCell(cellCenter);
			cellCenter.Phrase = new Phrase("Tanggal Surat Jalan", bold_font);
			tableContent.AddCell(cellCenter);
			cellCenter.Phrase = new Phrase("No Invoice", bold_font);
			tableContent.AddCell(cellCenter);
			cellCenter.Phrase = new Phrase("Nama Barang", bold_font);
			tableContent.AddCell(cellCenter);
			cellCenter.Phrase = new Phrase("Rate PPh", bold_font);
			tableContent.AddCell(cellCenter);
			cellCenter.Phrase = new Phrase("Sub Total PPh", bold_font);
			tableContent.AddCell(cellCenter);

			double total = 0;
			foreach (GarmentInvoiceItemViewModel item in viewModel.items)
			{
				 
				total += item.deliveryOrder.totalAmount;

				cellLeft.Phrase = new Phrase(item.deliveryOrder.doNo, normal_font);
				tableContent.AddCell(cellLeft);

				string doDate = item.deliveryOrder.doDate.ToOffset(new TimeSpan(clientTimeZoneOffset, 0, 0)).ToString("dd MMMM yyyy", new CultureInfo("id-ID"));

				cellLeft.Phrase = new Phrase(doDate, normal_font);
				tableContent.AddCell(cellLeft);

				cellLeft.Phrase = new Phrase(viewModel.invoiceNo, normal_font);
				tableContent.AddCell(cellLeft);

				
				foreach (GarmentInvoiceDetailViewModel detail in item.details)
				{
					 
					cellLeft.Phrase = new Phrase(detail.product.Name, normal_font);
					tableContent.AddCell(cellLeft);
				}

				cellLeft.Phrase = new Phrase(viewModel.incomeTaxRate.ToString(), normal_font);
				tableContent.AddCell(cellLeft);
				cellLeft.Phrase = new Phrase((viewModel.incomeTaxRate * item.deliveryOrder.totalAmount/100).ToString(), normal_font);
				tableContent.AddCell(cellLeft);

			}

			PdfPCell cellContent = new PdfPCell(tableContent); // dont remove
			tableContent.ExtendLastRow = false;
			tableContent.SpacingAfter = 20f;
			document.Add(tableContent);
			#endregion
			#region TableSignature

			PdfPTable tableSignature = new PdfPTable(2);

			PdfPCell cellSignatureContent = new PdfPCell() { Border  = Rectangle.TOP_BORDER | Rectangle.LEFT_BORDER | Rectangle.BOTTOM_BORDER | Rectangle.RIGHT_BORDER, HorizontalAlignment = Element.ALIGN_CENTER };
			 
			cellSignatureContent.Phrase = new Phrase("Administrasi\n\n\n\n\n\n\n(  " + "(Nama & Tanggal)" + "  )", bold_font);
			tableSignature.AddCell(cellSignatureContent);
			cellSignatureContent.Phrase = new Phrase("Staff Pembelian\n\n\n\n\n\n\n(  " + "(Nama & Tanggal)" + "  )", bold_font);
			tableSignature.AddCell(cellSignatureContent);
			cellSignatureContent.Phrase = new Phrase("Verifikasi\n\n\n\n\n\n\n(  " + "(Nama & Tanggal)" + "  )", bold_font);
			tableSignature.AddCell(cellSignatureContent);


			PdfPCell cellSignature = new PdfPCell(tableSignature); // dont remove
			tableSignature.ExtendLastRow = false;
			tableSignature.SpacingBefore = 20f;
			tableSignature.SpacingAfter = 20f;
			document.Add(tableSignature);

			#endregion
			document.Close();
			byte[] byteInfo = stream.ToArray();
			stream.Write(byteInfo, 0, byteInfo.Length);
			stream.Position = 0;

			return stream;
		}
	}
}
