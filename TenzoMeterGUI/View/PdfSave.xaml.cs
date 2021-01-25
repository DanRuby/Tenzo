using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using MigraDoc.DocumentObjectModel;
using MigraDoc.Rendering;
using OxyPlot;
using PdfSharp.Pdf;
using tEngine.DataModel;
using tEngine.Helpers;
using tEngine.MVVM;
using tEngine.MVVM.Converters;
using tEngine.TMeter;
using tEngine.TMeter.DataModel;
using tEngine.UControls;
using MessageBox = System.Windows.Forms.MessageBox;

namespace TenzoMeterGUI.View
{
    /// <summary>
    ///     Interaction logic for PdfSave.xaml
    /// </summary>
    public partial class PdfSave : Window
    {
        private readonly PdfSaveVM mDataContext;

        public PdfSave()
        {
            InitializeComponent();
            WindowManager.UpdateWindowPos(GetType().Name, this);
            mDataContext = new PdfSaveVM { Parent = this };
            DataContext = mDataContext;
        }

        public void SetPrintData(User user, List<Measurement> msms, List<ResultWindowVM.PlotSetResult> settings)
        {
            mDataContext.SetPrintData(user, msms, settings);
        }

        private void Window_OnClosing(object sender, CancelEventArgs e)
        {
            if (mDataContext != null)
            {
                try
                {
                    DialogResult = mDataContext.DialogResult;
                }
                catch (Exception ex)
                {
                    Debug.Assert(false, ex.Message);
                }
            }
            WindowManager.SaveWindowPos(GetType().Name, this);
        }

        private void PdfSave_OnLoaded(object sender, RoutedEventArgs e)
        {
            pve.InitModel();
        }
    }

    public class PdfSaveVM : Observed<PdfSaveVM>
    {
        private string mFileName;
        private List<Measurement> mMsms;
        private List<ResultWindowVM.PlotSetResult> mSettings;
        private User mUser;
        public Command CMDBrowse { get; private set; }
        public Command CMDCancel { get; private set; }
        public Command CMDDrawPlot { get; private set; }
        public Command CMDSave { get; private set; }

        public string FileName
        {
            get { return mFileName; }
            set
            {
                mFileName = value;
                NotifyPropertyChanged(m => m.FileName);
                NotifyPropertyChanged(m => m.CanSave);
            }
        }

        public bool OpenDoc { get; set; }
        public bool PrintConst { get; set; }
        public bool PrintCorr { get; set; }
        public bool PrintSpectrum { get; set; }
        public bool PrintTremor { get; set; }

        public bool CanSave
        {
            get { return !(FileName.IsNullOrEmpty() || mMsms.IsNullOrEmpty()); }
        }

        public PdfSaveVM()
        {
            CMDDrawPlot = new Command(CMDDrawPlot_Func);
            CMDSave = new Command(CMDSave_Func);
            CMDCancel = new Command(CMDCancel_Func);
            CMDBrowse = new Command(CMDBrowse_Func);

            PrintConst = AppSettings.GetValue("PDF_PrintConst", false);
            PrintTremor = AppSettings.GetValue("PDF_PrintTremor", true);
            PrintSpectrum = AppSettings.GetValue("PDF_PrintSpectrum", true);
            PrintCorr = AppSettings.GetValue("PDF_PrintCorr", false);
            OpenDoc = AppSettings.GetValue("PDF_OpenDoc", true);
            FileName = AppSettings.GetValue("PDF_FileName", "");

            foreach (FileInfo file in new DirectoryInfo(Constants.AppImageFolder).GetFiles())
            {
                file.Delete();
            }
        }

        public Document GenerateDataToPrint(User user, List<Measurement> msms, List<ResultWindowVM.PlotSetResult> settings)
        {
            if (msms.IsNullOrEmpty()) return null;

            Document doc = new Document();
            Section section = doc.AddSection();

            section.AddParagraph().AddFormattedText(string.Format("ФИО: {0}", user.UserLong()), TextFormat.Bold);
            section.AddParagraph().AddFormattedText(string.Format("{0}", user.Comment), new Font
            {
                Size = 12,
                Bold = false
            });

            PlotViewEx pve = (Parent as PdfSave).pve;

            for (int i = 0; i < msms.Count; i++)
            {
                Measurement msm = msms[i];

                if (PrintConst)
                {
                    section.AddParagraph().AddFormattedText(msm.Title, new Font
                    {
                        Size = 12,
                        Bold = true
                    });
                    AddImageToSection(section, GetImage(pve, settings[0], msm.Data.GetConst(Hands.Left)),
                        "Произвольное усилие, левая рука");
                    AddImageToSection(section, GetImage(pve, settings[0], msm.Data.GetConst(Hands.Right)),
                        "Произвольное усилие, правая рука");
                }
                if (PrintTremor)
                {
                    section.AddParagraph().AddFormattedText(msm.Title, new Font
                    {
                        Size = 12,
                        Bold = true
                    });
                    AddImageToSection(section, GetImage(pve, settings[1], msm.Data.GetTremor(Hands.Left)),
                        "Тремор, левая рука");
                    AddImageToSection(section, GetImage(pve, settings[1], msm.Data.GetTremor(Hands.Right)),
                        "Тремор, правая рука");
                }
                if (PrintSpectrum)
                {
                    section.AddParagraph().AddFormattedText(msm.Title, new Font
                    {
                        Size = 12,
                        Bold = true
                    });
                    AddImageToSection(section, GetImage(pve, settings[2], msm.Data.GetSpectrum(Hands.Left)),
                        "Спектральная характеристика, левая рука");
                    AddImageToSection(section, GetImage(pve, settings[2], msm.Data.GetSpectrum(Hands.Right)),
                        "Спектральная характеристика, правая рука");
                }
                if (PrintCorr)
                {
                    section.AddParagraph().AddFormattedText(msm.Title, new Font
                    {
                        Size = 12,
                        Bold = true
                    });
                    AddImageToSection(section, GetImage(pve, settings[3], msm.Data.GetCorrelation(Hands.Left)),
                        "Автокорреляционная функция, левая рука");
                    AddImageToSection(section, GetImage(pve, settings[3], msm.Data.GetCorrelation(Hands.Right)),
                        "Автокорреляционная функция, правая рука");
                }
            }
            return doc;
        }

        public void SetPrintData(User user, List<Measurement> msms, List<ResultWindowVM.PlotSetResult> settings)
        {
            mUser = user;
            mMsms = msms;
            mSettings = settings;
        }

        private void AddImageToSection(Section section, BitmapSource bitmap, string title)
        {
            section.AddParagraph().AddFormattedText(title, new Font
            {
                Size = 10,
                Bold = false
            });

            Guid name = Guid.NewGuid();
            bitmap.Save(Constants.AppImageFolder + @"\" + name + ".png");
            section.AddImage(Constants.AppImageFolder + @"\" + name + ".png");
        }

        private void CMDBrowse_Func()
        {
            using (SaveFileDialog sfd = new SaveFileDialog())
            {
                sfd.InitialDirectory = FileName.CutFileName();
                sfd.Filter = @"*.pdf|*.pdf";
                sfd.DefaultExt = "*.pdf";
                if (sfd.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    FileName = sfd.FileName;
                }
            }
        }

        private void CMDCancel_Func()
        {
            EndDialog(false);
        }

        private void CMDDrawPlot_Func()
        {
            PlotViewEx pve = (Parent as PdfSave).pve;
            //pve.InitModel();

            Measurement msm = mMsms[0];
            pve.Clear();
            pve.AddLineSeries(msm.Data.GetConst(Hands.Left), thickness: 2);
            //pve.PlotModel.AcceptSettings( mSettings[0] );
            pve.ShowPlot = true;
            pve.ReDraw();
            pve.ShowPlot = false;
        }

        private void CMDSave_Func()
        {
            // todo файл на доступность для записи
            if (new DirectoryInfo(FileName.CutFileName()).Exists == false)
            {
                MessageBox.Show(@"Неверно задан путь к месту сохранения", @"Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            AppSettings.SetValue("PDF_PrintConst", PrintConst);
            AppSettings.SetValue("PDF_PrintTremor", PrintTremor);
            AppSettings.SetValue("PDF_PrintSpectrum", PrintSpectrum);
            AppSettings.SetValue("PDF_PrintCorr", PrintCorr);
            AppSettings.SetValue("PDF_OpenDoc", OpenDoc);
            AppSettings.SetValue("PDF_FileName", FileName);


            Document doc = GenerateDataToPrint(mUser, mMsms, mSettings);

            const bool unicode = true;
            const PdfFontEmbedding embedding = PdfFontEmbedding.Always;
            PdfDocumentRenderer pdfRenderer = new PdfDocumentRenderer(unicode, embedding)
            {
                Document = doc
            };
            pdfRenderer.RenderDocument();
            pdfRenderer.PdfDocument.Save(FileName);
            if (OpenDoc)
                Process.Start(FileName);
            EndDialog(true);
        }

        private BitmapSource GetImage(PlotViewEx pve, ResultWindowVM.PlotSetResult set, IList<DataPoint> data)
        {
            pve.Clear();
            if (set.Normalize)
            {
                data = data.Normalized();
            }
            if (data.IsNullOrEmpty())
                data = new[] { new DataPoint(0, 0) };
            pve.AddLineSeries(data, thickness: 2);
            pve.PlotModel.AcceptSettings(set);

            pve.PlotModel.Background = OxyColors.White;
            pve.PlotView.Background = new SolidColorBrush(System.Windows.Media.Colors.White);
            pve.Background = new SolidColorBrush(System.Windows.Media.Colors.White);

            pve.ReDraw(false);
            return PlotModelToBitmap.GetBitmapFromPM(pve.PlotModel);
        }
    }
}