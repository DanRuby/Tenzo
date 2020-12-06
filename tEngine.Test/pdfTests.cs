using System.Diagnostics;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MigraDoc.DocumentObjectModel;
using MigraDoc.Rendering;
using PdfSharp.Pdf;

namespace tEngine.Test {
    [TestClass]
    public class pdfTests {
        [TestMethod]
        public void TestMethod1() {
            var doc = CreateDocument();
            // ===== Unicode encoding and font program embedding in MigraDoc is demonstrated here =====

            // A flag indicating whether to create a Unicode PDF or a WinAnsi PDF file.
            // This setting applies to all fonts used in the PDF document.
            // This setting has no effect on the RTF renderer.
            const bool unicode = true;

            // An enum indicating whether to embed fonts or not.
            // This setting applies to all font programs used in the document.
            // This setting has no effect on the RTF renderer.
            // (The term 'font program' is used by Adobe for a file containing a font. Technically a 'font file'
            // is a collection of small programs and each program renders the glyph of a character when executed.
            // Using a font in PDFsharp may lead to the embedding of one or more font programms, because each outline
            // (regular, bold, italic, bold+italic, ...) has its own fontprogram)
            const PdfFontEmbedding embedding = PdfFontEmbedding.Always;
            // Create a renderer for the MigraDoc document.
            var pdfRenderer = new PdfDocumentRenderer( unicode, embedding );

            // Associate the MigraDoc document with a renderer
            pdfRenderer.Document = doc;

            // Layout and render document to PDF
            pdfRenderer.RenderDocument();

            // Save the document...
            const string filename = @"d:\Tenzo\SoftPC\HelloWorld.pdf";
            pdfRenderer.PdfDocument.Save( filename );
            // ...and start a viewer.
            Process.Start( filename );
        }

        /// <summary>
        ///     Creates an absolutely minimalistic document.
        /// </summary>
        private static Document CreateDocument() {
            // Create a new MigraDoc document
            var document = new Document();

            // Add a section to the document
            var section = document.AddSection();

            // Add a paragraph to the section
            var paragraph = section.AddParagraph();

            // Add some text to the paragraph
            paragraph.AddFormattedText( "Павел Николаевич", TextFormat.Italic );
            //section.AddImage(  )

            return document;
        }
    }
}