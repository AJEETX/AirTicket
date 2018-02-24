using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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

         int countNumberReadFiles = 0;
        string searchCSSpattern = ConfigurationManager.AppSettings["cssFilePattern"].ToString();
        //assign the path of xml files to read from configuration file
        string xmlFilePath = Environment.CurrentDirectory + ConfigurationManager.AppSettings["xmlFilePath"].ToString();

        //assign the Computer.xslt file path from configuration file
        string xslFilePath = Environment.CurrentDirectory + ConfigurationManager.AppSettings["xslFilePath"].ToString();

        // set the output path of to be transformed html files with configuration file
        string htmlOutputFilePath = Environment.CurrentDirectory + ConfigurationManager.AppSettings["htmlFilePath"].ToString();
        
        // set the xslt file name through configuration file
        string xslFile = ConfigurationManager.AppSettings["xslFileName"].ToString();

        #endregion

        public Form1()
        {
            InitializeComponent();

        }

        // Read the xml files 
        private void ReadFiles()
        {
            //Read all the files with search pattern in the folder i this case its for searching xml files
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
        //Delete the Output folder if exists and put style.css file within
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

        //Make picturebox visible during transform process and invisible on completion of transform regardless of the thread context you are in
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
        //create a delegate and invoke the delegate using control.Invoke
        delegate void EnablePictureBoxVisibilityHandler(PictureBox btn, bool enable);
        private void button1_Click(object sender, EventArgs e)
        {
            //show picturebox
            EnablePictureBoxVisibility(pictureBox1, true);

            // Call the method to Delete the output folder and files within 
            DeleteOutputFolderFiles();
                
            //Disable the button
            button1.Enabled = false;
            
            //create the thread to read the xml files
            readThread = new Thread(new ThreadStart(ReadFiles));
            
            //start the thread for reading the xml files
            readThread.Start();

            //create a new thread to transform the read files 
            transformThread = new Thread(new ThreadStart(MakeHTML));

            //start the thread to transform XML to HTML files
            transformThread.Start();
        }
    }
}
