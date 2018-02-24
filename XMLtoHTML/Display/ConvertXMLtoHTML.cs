using System;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using System.Text;
using System.Xml;
using System.IO;
using System.Xml.Xsl;
using System.Threading.Tasks;
using System.Collections.Concurrent;
using System.Net;
using System.Windows.Forms;
using System.Configuration;

namespace Display
{
    class ConvertXMLtoHTML
    {
        static IEnumerable<string> files; static int count = 0;

        //ConcurrentQueue thread-safe first in-first out (FIFO) collection
       static  ConcurrentQueue<Tuple<string>> cq = new ConcurrentQueue<Tuple<string>>();
       
       #region  Get the list of all xml files from the folder i.e. \Data\Computers

       internal static int ReadAllFileswithPattern(string filePath, string searchPattern)
       {
           try
           {
               //get all the file with searchPattern as *.xml
               files = Directory.EnumerateFiles(filePath, searchPattern);
            
               //count the total read files
              count= IterateFiles(files, filePath);
           }
           catch (IOException io)
           {
               MessageBox.Show("Some IO exception during Read:" + io.Message);
           }
           catch (NullReferenceException e)
           {
               MessageBox.Show("Some Null reference exception during Read:" + e.Message);
           }
           return count;
       }

       #endregion

       #region convert xml file to html with xsl stylesheet

       internal static int TransformXMLToHTML(string xslFilePath, string xslFile,string xmlPath)
       {
            Tuple<string> tpl; 
           
           //get full path of xslt file from source locatoin
           string fullPath = Path.Combine(xslFilePath, xslFile);

           //Ignore DTD processing and set no validation 
           XmlReaderSettings xmlReadersettings = new XmlReaderSettings()
           {
               DtdProcessing = DtdProcessing.Ignore,ValidationType=ValidationType.None
           };

           //Declare and create a new XSLT processor object that implements the XSLT
           XslCompiledTransform xslTransform = new XslCompiledTransform();
           
           try
           {
               //Load the XSL
               xslTransform.Load(XmlReader.Create(fullPath, xmlReadersettings));
               
               //Loop till count equals 0 while deach dequeue decreases count by 1
               while (!cq.IsEmpty)
               {
                   // dequeue the value from tuple
                   cq.TryDequeue(out tpl);
 
                   //Transform the xml to an html output and with same name but with html extension instead of xml
                   xslTransform.Transform(tpl.Item1, xmlPath + @"\" + Path.GetFileNameWithoutExtension(tpl.Item1) + ".html");
               }
           }
           catch (IOException io)
           {
               MessageBox.Show("Some IO exception during transform:" + io.Message);            
           }
           catch (XmlException xmlEx)
           {
               MessageBox.Show("Some xml exception during transform:" + xmlEx.Message);
           }
              
           return cq.Count();
       }

       #endregion

       #region Read all files and its contents & store in queue

       internal static int IterateFiles(IEnumerable<string> files, string directory)
       {
           Tuple<string> tpl = null; 
           
           //loop for all files with xml extension and enqueue them
           foreach (var file in files)
           {
               //for verification of dirctory and file  
               Console.WriteLine("{0}", Path.Combine(directory, file));
               
               try
               {
                   //store file on tuple
                   tpl = new Tuple<string>(file);

                   //Enqueue the tuple
                   cq.Enqueue(tpl);
               }
               catch (IOException io)
               {
                   MessageBox.Show("Some IO exception during Iterate:" + io.Message);
               }
               catch (XmlException xmlEx)
               {
                   MessageBox.Show("Some xml exception during Iterate:" + xmlEx.Message);
               }
           }
           return cq.Count;
       }
       #endregion

       #region Copy the input files i.e css file to destination folder

       internal static void CopyInputFile(string sourcefilePath,string destinationFilePath, string searchPattern)
       {
           // search for css file from the source path 
           var filename = Directory.GetFiles(sourcefilePath, searchPattern).SingleOrDefault().ToString();
           
           string sourceFileName = filename;
           
           //set the destinatoin path file name
           string destinationFileName = destinationFilePath + @"\" +Path.GetFileName(filename);
           
           //copy css from source path to the destination path i.e. Output folder
           File.Copy(sourceFileName, destinationFileName, true);
       }
       #endregion
    }
}
