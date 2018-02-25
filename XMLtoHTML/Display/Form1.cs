using System;
using System.Windows.Forms;
using System.Threading;
using System.Diagnostics;
using System.IO;
using System.Configuration;

namespace Display
{
    public partial class Form1 : Form
    {
        #region Intialize variables
        Thread readThread, transformThread;

        string searchXMLpattern = ConfigurationManager.AppSettings["xmlFilePattern"].ToString();
        string searchHTMLpattern = ConfigurationManager.AppSettings["htmlFilePattern"].ToString();
        string searchCSSpattern = ConfigurationManager.AppSettings["cssFilePattern"].ToString();
        string xmlFilePath = Environment.CurrentDirectory + ConfigurationManager.AppSettings["xmlFilePath"].ToString();
        string xslFilePath = Environment.CurrentDirectory + ConfigurationManager.AppSettings["xslFilePath"].ToString();
        string htmlOutputFilePath = Environment.CurrentDirectory + ConfigurationManager.AppSettings["htmlFilePath"].ToString();
        string xslFile = ConfigurationManager.AppSettings["xslFileName"].ToString();

        #endregion

        public Form1()
        {
            InitializeComponent();
        }

        private void ReadFiles()
        {
            ConvertXMLtoHTML.ReadAllFileswithPattern(xmlFilePath, searchXMLpattern);
        }

        private void MakeHTML()
        {
            int countFilesRemainingtoTransform = ConvertXMLtoHTML.TransformXMLToHTML(xslFilePath, xslFile, htmlOutputFilePath);
            
            //If all the files on the queue have been converted i.e. no files remaining then show the sucess message
            if (countFilesRemainingtoTransform == 0)
            {
                //do not show picturebox as the transform is complete
                EnablePictureBoxVisibility(pictureBox1, false);

                //Open a window stating that the transform is complete
                MessageBox.Show("XML files converted to html at Date Time:"+ DateTime.Now.ToLocalTime(), "XML to HTML conversion", MessageBoxButtons.OK);

                //open the folder where html files are created
                Process.Start(htmlOutputFilePath);
                
                // close the application
                Application.Exit();
            }
        }
        private void DeleteOutputFolderFiles()
        {
            //check if output directory exits
            if (!Directory.Exists(htmlOutputFilePath))
            {
                //create a new Output folder
                Directory.CreateDirectory(htmlOutputFilePath);
            }
                ConvertXMLtoHTML.CopyInputFile(xslFilePath, htmlOutputFilePath, searchCSSpattern);
        }
        private void EnablePictureBoxVisibility(PictureBox pb, bool enable)
        {
            //check if the caller method should call invoke method when making call to the control as caller is on different thread
            if (!pb.InvokeRequired)
            {
                //set the visibility of picturebox
                pb.Visible = enable;
            }
            else
            {

                pb.Invoke(new EnablePictureBoxVisibilityHandler(EnablePictureBoxVisibility), pb, enable);
            }
        }
        delegate void EnablePictureBoxVisibilityHandler(PictureBox btn, bool enable);
        private void button1_Click(object sender, EventArgs e)
        {
            EnablePictureBoxVisibility(pictureBox1, true);
            DeleteOutputFolderFiles();
            button1.Enabled = false;
            readThread = new Thread(new ThreadStart(ReadFiles));
            readThread.Start();
            transformThread = new Thread(new ThreadStart(MakeHTML));
            transformThread.Start();
        }
    }
}
