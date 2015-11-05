using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Net;
using System.IO;
using System.Collections.Specialized;
using Newtonsoft.Json;

namespace eShiaWord2HtmlClient
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        
        private void button2_Click(object sender, EventArgs e)
        {
            string endPoint = "http://eshia.ir/feqh/archive/convert2zip";
            string filePath = "C:\\Users\\Admin\\Desktop\\eshia-convert\\~\\940618.docx";

            WebClient wc = new WebClient();

            // fired when upload progress is changed
            // this updates a progressbar named pbProgress
            wc.UploadProgressChanged += (o, ea) =>
            {
                if (ea.ProgressPercentage >= 0 && ea.ProgressPercentage <= 100)
                    progressBar1.Value = ea.ProgressPercentage;
            };

            // fired when the file upload is complete
            // fired if an error occurs or if it's successful
            wc.UploadFileCompleted += (o, ea) =>
            {
                // determine if upload failed or not
                if (ea.Error == null)
                {
                    // response will let us know if there
                    // was an error on the PHP side
                    string response = Encoding.UTF8.GetString(ea.Result);

                    try
                    {
                        dynamic result = JsonConvert.DeserializeObject(response);

                        if (result.success == "yes")
                        {
                            using (
                                BinaryWriter bw = new BinaryWriter(
                                     new FileStream("C:\\Users\\Admin\\Desktop\\eshia-convert\\~\\test", FileMode.Create, FileAccess.ReadWrite)
                                )
                            )
                            {
                                string content = result.content;
                                byte[] data = Convert.FromBase64String(content);
                                
                                bw.Write(data);
                            }

                            label1.Text = "Upload completed.";
                        }
                        else
                        {
                            label1.Text = "Upload failed3.";
                        }
                    }
                    catch (Exception ex)
                    {
                        label1.Text = "Upload failed2.";
                    }

                    
                }
                else
                {
                    label1.Text = "Upload failed1.";
                    MessageBox.Show(ea.Error.Message);
                }
            };

            // set the status to Uploading
            label1.Text = "Uploading...";

            // tell the webclient to upload the file
            wc.UploadFileAsync(new Uri(endPoint), filePath);
        }
    }
}
