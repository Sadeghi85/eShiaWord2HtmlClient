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

        private void button1_Click(object sender, EventArgs e)
        {
            

            string URI = textBox1.Text;
            //string myParameters = "?q=" +textBox2.Text;

            //URI += myParameters;

            using (WebClient wc = new WebClient())
            {
                try
                {
                    wc.Encoding = Encoding.UTF8;
                    //wc.Headers[HttpRequestHeader.ContentType] = "application/x-www-form-urlencoded";
                    NameValueCollection queryStrings = new NameValueCollection();
                    queryStrings.Add("q", textBox2.Text);

                    wc.QueryString = queryStrings;

                    string HtmlResult = wc.DownloadString(URI);
                    //MessageBox.Show(HtmlResult);

                    //byte[] encodedText = Encoding.UTF8.GetBytes(HtmlResult);
                    Encoding utf8WithoutBom = new UTF8Encoding(false);

                    using (
                        StreamWriter sw = new StreamWriter(
                             new FileStream("C:\\Users\\Admin\\Desktop\\eshia-convert\\~\\test", FileMode.Create, FileAccess.ReadWrite),
                        utf8WithoutBom
                        )
                    )
                    {
                        sw.Write(HtmlResult.ToString());
                    }

                    //FileStream fs = new FileStream("C:\\Users\\Admin\\Desktop\\eshia-convert\\~\\test", FileMode.Create);
                    //BinaryWriter bw = new BinaryWriter(fs,Encoding.UTF8);

                    //HtmlResult = Convert.ToString(HtmlResult);

                    //bw.Write(HtmlResult);
                    //fs.Write(encodedText, 0, encodedText.Length);

                    //fs.Close();
                    //bw.Close();

                }
                catch (Exception ex)
                {
                    // handle error
                    MessageBox.Show(ex.Message);
                }
            }

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
                            Encoding utf8WithoutBom = new UTF8Encoding(false);

                            using (
                                StreamWriter sw = new StreamWriter(
                                     new FileStream("C:\\Users\\Admin\\Desktop\\eshia-convert\\~\\test", FileMode.Create, FileAccess.ReadWrite)
                                )
                            )
                            {
                                string content = result.content;
                                byte[] data = Convert.FromBase64String(content);
                                
                                sw.Write(Encoding.UTF8.GetString(data));
                                //sw.Write(result.content);
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

                    //if (response == "Success")
                    //    label1.Text = "Upload Complete.";
                    //else
                    //{
                    //    label1.Text = "Upload Failed.";
                    //    MessageBox.Show(response);
                    //}
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
