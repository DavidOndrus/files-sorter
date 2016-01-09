using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Forms;
using System.IO;
using System.Diagnostics;

namespace VideoMover
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        string errorlogFilename = "errorlog.txt";
        private static Action EmptyDelegate = delegate() { };

        public MainWindow()
        {
            InitializeComponent();
        }

        private void srcBrowseBtn_Click(object sender, RoutedEventArgs e)
        {
            using (FolderBrowserDialog dlg = new FolderBrowserDialog())
            {
                DialogResult result = dlg.ShowDialog();
                if (result == System.Windows.Forms.DialogResult.OK)
                {
                    srcTxtb.Text = dlg.SelectedPath;
                }
            }
        }

        private void destBrowseBtn_Click(object sender, RoutedEventArgs e)
        {
            using (FolderBrowserDialog dlg = new FolderBrowserDialog())
            {
                DialogResult result = dlg.ShowDialog();
                if (result == System.Windows.Forms.DialogResult.OK)
                {
                    destTxtb.Text = dlg.SelectedPath;
                }
            }
        }

        private void copyBtn_Click(object sender, RoutedEventArgs e)
        {
            errorlogTxtb.Text = String.Empty;
            int pocetNeuspesne = 0, pocetUspesne = 0;
            string[] files;
            try
            {
                files = Directory.GetFiles(srcTxtb.Text, filterTxtb.Text, SearchOption.AllDirectories);
                progressBar.Minimum = 0;
                progressBar.Maximum = files.Length;
                progressBar.Value = 0;

                string destDir = destTxtb.Text;
                Task copyTask = Task.Run(() =>
                {
                    foreach (string file in files)
                    {
                        try
                        {
                            DateTime creationTime = File.GetLastWriteTime(file);
                            string destDirYear = Path.Combine(destDir, creationTime.Year.ToString());
                            if (!Directory.Exists(destDirYear))
                            {
                                Directory.CreateDirectory(destDirYear);
                            }
                            string destDirYearMonth = Path.Combine(destDirYear, creationTime.Month.ToString("00"));
                            if (!Directory.Exists(destDirYearMonth))
                            {
                                Directory.CreateDirectory(destDirYearMonth);
                            }
                            File.Copy(file, Path.Combine(destDirYearMonth, Path.GetFileName(file)));
                            this.Dispatcher.Invoke(() => { pocetUspesne++; });
                        }
                        catch (Exception ex1)
                        {
                            this.Dispatcher.Invoke(() =>
                            {
                                errorlogTxtb.Text += file + Environment.NewLine;
                                pocetNeuspesne++;
                            });
                            using (StreamWriter sw = new StreamWriter(errorlogFilename, true))
                            {
                                sw.WriteLine(ex1.Message);
                            }
                        }
                        this.Dispatcher.Invoke(() =>
                        {
                            progressBar.Value++;
                        });

                        }
                        this.Dispatcher.Invoke(() =>
                        {
                            uspesneTxtb.Text = pocetUspesne.ToString();
                            neuspesneTxtb.Text = pocetNeuspesne.ToString();
                        });
                });
            }
            catch (Exception ex2)
            {
                errorlogTxtb.Text = "Chyba pri načítavaní súborov zo zdrojového priečinka podľa zvoleného filtra.";
                using (StreamWriter sw = new StreamWriter(errorlogFilename, true))
                {
                    sw.WriteLine(ex2.Message);
                }
            }
        }

        private void errorlogBtn_Click(object sender, RoutedEventArgs e)
        {
            if (File.Exists(errorlogFilename))
            {
                Process.Start(errorlogFilename);
            }
            else
            {
                System.Windows.MessageBox.Show("Neexistuje žiadny errorlog.", "Errorlog", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }
    }
}
