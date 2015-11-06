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
        private string[] documents;
        private string[] documentsTemp;
        private int documentAbsoluteCount = 0;

        private List<string> documentsCompleted = new List<string>();
        private List<string> documentsFailed = new List<string>();

        private IDictionary<string, int> documentsProgress = new Dictionary<string, int>();

        public Form1()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            DialogResult result = folderBrowserDialog1.ShowDialog();
            if (result == DialogResult.OK)
            {
                textBox1.Text = folderBrowserDialog1.SelectedPath;
                documents = Directory.GetFiles(folderBrowserDialog1.SelectedPath, "*.docx");
                documentAbsoluteCount = documents.Length;
            }
            else
            {
                textBox1.Text = "";
            }
        }

        private void initUpload()
        {
            documentsTemp = documents;

            while (documentsTemp.Length > 0)
            {
                int lastIdx = documentsTemp.Length - 1;
                doUpload(documentsTemp[lastIdx]);

                documentsTemp = documentsTemp.Where((val, idx) => idx != lastIdx).ToArray();
            }
        }

        private int computeProgress()
        {
            int count = documentsProgress.Count;
            int progress = 0;
            int total = 0;

            foreach (KeyValuePair<string, int> match in documentsProgress)
            {
                total += match.Value;
            }

            progress = (int) total / count;

            return progress;
        }

        private void doUpload(string filePath)
        {
            string endPoint = "http://eshia.ir/feqh/archive/convert2zip";
            //string filePath = "C:\\Users\\Admin\\Desktop\\eshia-convert\\~\\940618.docx";
            string folder = filePath.Substring(0, filePath.LastIndexOf("\\"));
            string fileName = filePath.Substring(filePath.LastIndexOf("\\") + 1);
            fileName = folder + "\\" + fileName.Substring(0, fileName.LastIndexOf(".")) + ".zip";

            WebClient wc = new WebClient();

            wc.UploadProgressChanged += (o, ea) =>
            {
                if (ea.ProgressPercentage >= 0 && ea.ProgressPercentage <= 100)
                {
                    //progressBar1.Value = ea.ProgressPercentage;
                    documentsProgress[filePath] = ea.ProgressPercentage;

                    progressBar1.Value = computeProgress();
                }
                
            };

            wc.UploadFileCompleted += (o, ea) =>
            {
                if (ea.Error == null)
                {
                    string response = Encoding.UTF8.GetString(ea.Result);

                    try
                    {
                        dynamic result = JsonConvert.DeserializeObject(response);

                        if (result.success == "yes")
                        {
                            using (
                                BinaryWriter bw = new BinaryWriter(
                                     new FileStream(fileName, FileMode.Create, FileAccess.ReadWrite)
                                )
                            )
                            {
                                string content = result.content;
                                byte[] data = Convert.FromBase64String(content);

                                bw.Write(data);
                            }

                            //textBox2.Text = "Upload completed.";
                            documentsCompleted.Add(filePath);
                        }
                        else
                        {
                            //textBox2.Text = "Upload failed.";
                            documentsFailed.Add(filePath);
                        }
                    }
                    catch (Exception ex)
                    {
                        //textBox2.Text = "Upload failed.";
                        documentsFailed.Add(filePath);
                    }

                }
                else
                {
                    //textBox2.Text = "Upload failed.";
                    documentsFailed.Add(filePath);
                }
            };

            wc.UploadFileAsync(new Uri(endPoint), filePath);
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            if (listBox1.Items.Count + listBox2.Items.Count < documentAbsoluteCount)
            {
                listBox1.Items.Clear();
                listBox1.Items.AddRange(documentsCompleted.ToArray());
                
                listBox2.Items.Clear();
                listBox2.Items.AddRange(documentsFailed.ToArray());
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            progressBar1.Value = 0;
            listBox1.Items.Clear();
            listBox2.Items.Clear();
            documentsProgress = new Dictionary<string, int>();
            documentsCompleted = new List<string>();
            documentsFailed = new List<string>();

            initUpload();
        }
    }
}
