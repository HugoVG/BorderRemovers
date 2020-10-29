using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Border_Remover
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        FileSystemWatcher fs;
        DateTime fsLastRaised;
        string watchingFolder;
        string folderPathInput = "";
        public MainWindow()
        {
            InitializeComponent();
        }
        
        #region Crop image
        public static Bitmap Crop(Bitmap bmp)
        {
            int w = bmp.Width;
            int h = bmp.Height;

            Func<int, bool> allWhiteRow = row =>
            {
                for (int i = 0; i < w; ++i)
                    if (bmp.GetPixel(i, row).R != 255)
                        return false;
                return true;
            };

            Func<int, bool> allWhiteColumn = col =>
            {
                for (int i = 0; i < h; ++i)
                    if (bmp.GetPixel(col, i).R != 255)
                        return false;
                return true;
            };

            int topmost = 0;
            for (int row = 0; row < h; ++row)
            {
                if (allWhiteRow(row))
                    topmost = row;
                else break;
            }

            int bottommost = 0;
            for (int row = h - 1; row >= 0; --row)
            {
                if (allWhiteRow(row))
                    bottommost = row;
                else break;
            }

            int leftmost = 0, rightmost = 0;
            for (int col = 0; col < w; ++col)
            {
                if (allWhiteColumn(col))
                    leftmost = col;
                else
                    break;
            }

            for (int col = w - 1; col >= 0; --col)
            {
                if (allWhiteColumn(col))
                    rightmost = col;
                else
                    break;
            }

            if (rightmost == 0) rightmost = w; // As reached left
            if (bottommost == 0) bottommost = h; // As reached top.

            int croppedWidth = rightmost - leftmost;
            int croppedHeight = bottommost - topmost;

            if (croppedWidth == 0) // No border on left or right
            {
                leftmost = 0;
                croppedWidth = w;
            }

            if (croppedHeight == 0) // No border on top or bottom
            {
                topmost = 0;
                croppedHeight = h;
            }

            try
            {
                var target = new Bitmap(croppedWidth, croppedHeight);
                using (Graphics g = Graphics.FromImage(target))
                {
                    g.DrawImage(bmp,
                      new RectangleF(0, 0, croppedWidth, croppedHeight),
                      new RectangleF(leftmost, topmost, croppedWidth, croppedHeight),
                      GraphicsUnit.Pixel);
                }
                return target;
            }
            catch (Exception ex)
            {
                throw new Exception(
                  string.Format("Values are topmost={0} btm={1} left={2} right={3} croppedWidth={4} croppedHeight={5}", topmost, bottommost, leftmost, rightmost, croppedWidth, croppedHeight),
                  ex);
            }
        }
        #endregion
        private void btnSatrt_Click(object sender, RoutedEventArgs e)
        {
            if (Inputbox.IsChecked == true)
            {
                //the folder to be watched
                Debug.Print(folderPathInput);
                watchingFolder = folderPathInput;
                //initialize the filesystem watcher
                fs = new FileSystemWatcher(watchingFolder, "*.*");

                fs.EnableRaisingEvents = true;
                fs.IncludeSubdirectories = true;
                //This event will check for  new files added to the watching folder
                fs.Created += new FileSystemEventHandler(newfile);
                //This event will check for any changes in the existing files in the watching folder
                //fs.Changed += new FileSystemEventHandler(fs_Changed);
                //this event will check for any rename of file in the watching folder
                //fs.Renamed += new RenamedEventHandler(fs_Renamed);
                //this event will check for any deletion of file in the watching folder
                //fs.Deleted += new FileSystemEventHandler(fs_Deleted);
                listBox1.Items.Add("Gestart op: "  + DateTime.Now.ToShortTimeString());
                errorlabel.Content = "";
            }
            else
            {
                errorlabel.Content = "Selecteer een map eerst!";
            }
        }
      
        #region file Added to the folder
        protected void newfile(object fscreated, FileSystemEventArgs Eventocc)
        {
            try
            {   
                //to avoid same process to be repeated ,if the time between two events is more   than 1000 milli seconds only the second process will be considered
                if (DateTime.Now.Subtract(fsLastRaised).TotalMilliseconds > 10)
                {
                    //to get the newly created file name and extension and also the name of the event occured in the watching folder
                    string CreatedFileName = Eventocc.Name;
                    FileInfo createdFile = new FileInfo(CreatedFileName);
                    string extension = createdFile.Extension;
                    string eventoccured = Eventocc.ChangeType.ToString();
                    Debug.Print(watchingFolder);
                    Debug.Print(CreatedFileName);

                    //to note the time of event occured
                    fsLastRaised = DateTime.Now;
                    //Delay is given to the thread for avoiding same process to be repeated
                    System.Threading.Thread.Sleep(10);
                    //dispatcher invoke
                    this.Dispatcher.Invoke((Action)(() =>
                    {
                        //give a notification in the application about the change in folder
                        //listBox1.Items.Add("Newly Created File Name :" + CreatedFileName + ";  Event Occured  :" + eventoccured + ";  Created Time  :" + DateTime.Now.ToShortTimeString());

                        //if image file is created, then crop image and put watermark 

                        try
                        {
                            if (extension.ToLower() == ".jpg" || extension.ToLower() == ".png" || extension.ToLower() == ".jpeg")
                            {
                                var named = watchingFolder + "\\" + CreatedFileName;

                                var outputPath = named.Replace("Input", "Output");
                                var bitmap = new Bitmap(Image.FromFile(watchingFolder + "\\" + CreatedFileName));
                                var cropped = Crop(bitmap);
                                if (CB_wm.IsChecked == true)
                                {
                                    cropped = watermark(cropped);
                                }
                                cropped.Save(outputPath, ImageFormat.Png);
                                listBox1.Items.Add("Newly Cropped File Success: " + CreatedFileName + ";  Event Occured  :" + eventoccured + ";  Created Time  :" + DateTime.Now.ToShortTimeString());
                                if (CB_Del.IsChecked == true)
                                {
                                    if (System.IO.File.Exists(named))
                                    {
                                        try
                                        {
                                            File.Delete(named);

                                        }
                                        catch (System.IO.IOException)
                                        {
                                            return;
                                        }

                                    }

                                }
                            }

                            else
                            {
                                listBox1.Items.Add("Newly Added file is not an image! " + CreatedFileName + " Event Occured: " + eventoccured + " Created Time : " + DateTime.Now.ToShortTimeString());
                            }
                        }
                        catch
                        {
                            System.Windows.MessageBox.Show("Error with Manupilation of the image", "Error", MessageBoxButton.YesNo);
                        }
                    }));
                }
            }
            catch (Exception)
            {
                listBox1.Items.Add("Can not Edit file, is image downloaded?");
            }
            
        }
        #endregion

        #region Add watermark to IMage
        public static Bitmap watermark(Bitmap bmp)
        {
            try
                {
                Graphics grimg = Graphics.FromImage(bmp);

                int w = bmp.Width;
                int h = bmp.Height;

                var WM_img = Properties.Resources.WaterMerk;
                Bitmap watermerk = new Bitmap(WM_img);

                Bitmap resized = new Bitmap(watermerk);
                int wm_W = resized.Width;
                int wm_H = resized.Height;
                if (wm_H > w)
                {
                    wm_H = h;
                }
                if (wm_W > w)
                {

                    resized = new Bitmap(watermerk, new System.Drawing.Size(w - 10, WM_img.Height / 2));
                    wm_W = resized.Width - 10;
                }


                int place_WM_W = (w - wm_W) / 2 - 10;
                int place_WM_H = (h - wm_H) - 15;

                grimg.DrawImage(resized, place_WM_W, place_WM_H);
                grimg.Dispose();
                Bitmap result = new Bitmap(bmp);
                return result;
            }
            catch
            {
                System.Windows.MessageBox.Show("Error with watter mark", "Error", MessageBoxButton.YesNo);
                return null;
            }
        }
        #endregion

        public void InputBut_Click(object sender, RoutedEventArgs e)
        {
            
            FolderBrowserDialog folderBrowserDialog1 = new FolderBrowserDialog();
            folderBrowserDialog1.ShowDialog();
            Debug.Print(folderBrowserDialog1.SelectedPath);
            folderPathInput = folderBrowserDialog1.SelectedPath;
            if (folderPathInput != "")
            {
                Inputbox.IsChecked = true;
            }
            
        }
    }
}
