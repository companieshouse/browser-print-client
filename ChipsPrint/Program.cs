using System;
using System.Drawing.Printing;
using System.Net;
using System.Windows.Forms;
using System.IO;
using PdfiumViewer;

namespace ChipsPrint
{
    class Program
    {
        [STAThreadAttribute]
        static void Main(string[] args)
        {
            Options options = ExtractOptions(args[0]);

            if(options.Url != null)
            {
                using (var client = new WebClient())
                {
                    debug("About to download PDF with url: " + options.Url, options);

                    byte[] downloadedPDF = client.DownloadData(options.Url);
                    Stream pdfStream = new MemoryStream(downloadedPDF);
                    PrintPDF(options, pdfStream);
                }
            } 
        }

        static Options ExtractOptions(string arg)
        {
            Options options = new Options();

            string decodedArg = WebUtility.UrlDecode(arg).Replace("chipsprint:", "");

            if (decodedArg.IndexOf("|") > -1)
            {
                string[] argArray = decodedArg.Split('|');
                options = setValidUrl(argArray[0], options); 
                
                string[] paramArray = argArray[1].ToLower().Split(',');
                foreach (string param in paramArray)
                {
                    if (param.Equals("cert=true"))
                    {
                        options.Cert = true;
                    }

                    if (param.Equals("debug=true"))
                    {
                        options.Debug = true;
                    }
                }
            }
            else
            {
                options = setValidUrl(decodedArg, options);        
            }
            return options;
        }

        static Options setValidUrl(string url, Options options)
        {
            Uri uriResult;
            bool isValid = Uri.TryCreate(url, UriKind.Absolute, out uriResult)
                && (uriResult.Scheme == Uri.UriSchemeHttp || uriResult.Scheme == Uri.UriSchemeHttps);

            if(isValid)
            {
                options.Url = url;
            }
            else
            {
                error("Invalid url:" + url);
                System.Environment.Exit(1);
            }
            return options;
        }

        static void PrintPDF(Options options, Stream pdf)
        {
            try
            {
                // Create the printer settings for our printer
                var printerSettings = new PrinterSettings();

                // TODO: Set the printerName in printerSettings if there is a printer called CHIPS_*
                // And set the tray to the suffix of the printerName if the print type is cert
                // to allow custom tray for certs
                // For now, using the default printer and tray 0 (auto) for normal prints and tray 6 for certs

                int tray = 0;
                if (options.Cert)
                {
                    tray = 6;
                }

                info("Printing:" + options.Url + ", on tray:" + tray + ", on printer:" + printerSettings.PrinterName);

                // Create our page settings for the paper size selected
                var pageSettings = new PageSettings(printerSettings)
                {
                    Margins = new Margins(0, 0, 0, 0),
                    PaperSource = printerSettings.PaperSources[tray]
                };

                foreach (PaperSize paperSize in printerSettings.PaperSizes)
                {
                    if (paperSize.PaperName == "A4")
                    {
                        pageSettings.PaperSize = paperSize;
                        break;
                    }
                }

                // Now print the PDF document
                using (var document = PdfDocument.Load(pdf))
                {
                    using (var printDocument = document.CreatePrintDocument())
                    {
                        printDocument.PrinterSettings = printerSettings;
                        printDocument.DefaultPageSettings = pageSettings;
                        printDocument.PrintController = new StandardPrintController();
                        printDocument.Print();
                    }
                }
                
            }
            catch(Exception e)
            {
                error(e.Message);
                error(e.StackTrace);
            }
        }

        static void debug(string message, Options options)
        {
            if (options.Debug)
            {
                showMessage(message, MessageBoxIcon.None);
            }
        }

        static void error(string message)
        {
            showMessage(message, MessageBoxIcon.Error);
        }

        static void info(string message)
        {
            showMessage(message, MessageBoxIcon.Information);
        }

        static void showMessage(string message, MessageBoxIcon icon)
        {
            MessageBox.Show(message, "ChipsPrint", MessageBoxButtons.OK, icon);
        }

    }
}