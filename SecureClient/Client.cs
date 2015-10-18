using System;
using System.Collections.Generic;
using System.IO;
using System.Windows.Forms;

namespace SecureClient
{
    public partial class Client : Form
    {
        private static string localPath = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) + @"\SecureClient";

        //profile data
        private string username;
        private string password;

        private string hostIn;
        private int portIn;

        private string hostOut;
        private int portOut;

        private bool secureReceiving;
        private bool secureSending;

        private string privateKey;
        private string publicKey;

        //downloaded mail bodys
        private string[] bodys;

        //mail address and its public key in base64
        private Dictionary<string, string> addresser;

        public Client(string u, string p)
        {
            this.username = u;
            this.password = p;

            //get data from profiles.txt
            string line;

            string profileFile = localPath + @"\profiles.txt";
            System.IO.StreamReader file = new System.IO.StreamReader(profileFile);
            while ((line = file.ReadLine()) != null)
            {
                string[] elements = line.Split(new string[] { "|::::|" }, StringSplitOptions.None);
                if (elements[0] == username)
                {
                    this.hostIn = elements[1];
                    this.portIn = Int32.Parse(elements[2]);
                    this.hostOut = elements[3];
                    this.portOut = Int32.Parse(elements[4]);
                    this.secureReceiving = Boolean.Parse(elements[5]);
                    this.secureSending = Boolean.Parse(elements[6]);

                    this.privateKey = ClassAES.DecryptStringAes(elements[7], password);
                    this.publicKey = elements[8];

                    break;
                }
            }
            file.Close();

            //get public keys from addresser
            addresser = new Dictionary<string, string>();

            string line2;

            string addressFile = localPath + @"\addresser.txt";
            System.IO.StreamReader file2 = new System.IO.StreamReader(addressFile);
            while ((line2 = file2.ReadLine()) != null)
            {
                string[] elements = line2.Split(new string[] { "|::::|" }, StringSplitOptions.None);
                if (!addresser.ContainsKey(elements[0]))
                {
                    addresser.Add(elements[0], elements[1]);
                }
            }
            file.Close();

            InitializeComponent();
        }

        private void Form2_Load(object sender, EventArgs e)
        {
            //start timer to refresh mails on every 5 minutes
            timer1.Interval = 2000;
            timer1.Start();

        }

        //send mail button
        private void button1_Click(object sender, EventArgs e)
        {
            //receiver
            string to = textBox1.Text;

            //construct mail body...
            string message = "<SecureClient email>" + Environment.NewLine;

            //plain text body
            string body = textBox3.Text.Trim();

            //calculate signature
            string signature = ClassRSA.getSignature(privateKey, body);

            //search for private key to encrypt message  
            if (addresser.ContainsKey(to))
            {
                string yourPubKey = addresser[to];
                body = ClassRSA.encrypt(yourPubKey, body);
                message += "<Encrypted>" + Environment.NewLine;
            }
            message += body + Environment.NewLine;
            message += "<Signature>" + Environment.NewLine + signature + Environment.NewLine;
            message += "<Public key>" + Environment.NewLine + publicKey;
            
            //send mail using SMTP
            string poslano = ClassMail.Send(username, password, to,textBox2.Text, message, hostOut, portOut, secureSending);
            MessageBox.Show(poslano);
        }

        //timer to check mails
        private void timer1_Tick(object sender, EventArgs e)
        {
            timer1.Interval = 5 * 60 * 1000;
            fillTable();
            
        }

        //show selected mail body in richtextbox
        private void listView1_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (this.listView1.SelectedItems.Count == 0)
                return;

            richTextBox1.Text = bodys[Int32.Parse(listView1.SelectedItems[0].SubItems[5].Text)];
        }

        //get mails
        private void fillTable()
        {
            //download mails
            List<string[]> sporocila = ClassMail.getMails(username, password, hostIn, portIn, secureReceiving);

            //array for storing mail bodys
            bodys = new string[sporocila.Count];

            int stevec = 0;
            for (int i = sporocila.Count - 1; i >= 0; i--)
            {
                //Read mail
                string[] s = sporocila[i];

                //get data
                string fromMail = s[0];
                string subject = s[1];
                string date = s[2];
                string body = s[3];

                //sign and encryption flag
                string sign = "0";
                string enc = "0";

                //if mail is from SecureClient
                if (body.Length > "<SecureClient email>".Length && body.Substring(0, "<SecureClient email>".Length) == "<SecureClient email>")
                {
                    try
                    {
                        //explode body to parts
                        string[] parts = body.Split(new string[] { "<SecureClient email>", "<Signature>", "<Public key>" }, StringSplitOptions.RemoveEmptyEntries);

                        //check if body is encrpyted
                        body = parts[0].Trim();
                        if (body.Length >"<Encrypted>".Length && body.Substring(0, "<Encrypted>".Length) == "<Encrypted>")
                        {
                            
                            body = body.Replace("<Encrypted>", "");

                            //try to decrpyt it
                            try
                            {
                                body = ClassRSA.decrypt(privateKey, body);
                                enc = "OK";
                            }
                            catch (Exception ex)
                            {
                                MessageBox.Show(ex.ToString());
                                enc = "FAIL";
                            }
                        }

                        //check signature
                        string signature = parts[1].Trim();
                        string yourPublicKey = parts[2].Trim();

                        if (addresser.ContainsKey(fromMail))
                        {
                            //if we know the sender
                            if (yourPublicKey != addresser[fromMail])
                            {
                                sign = "[ALERT]Key changed!"; //posiljatelj je spremenil identiteto!
                            }
                        }
                        else
                        {
                            //add new public key to addresser
                            addresser.Add(fromMail, yourPublicKey);
                            File.AppendAllText(localPath + @"\addresser.txt", fromMail + "|::::|" + yourPublicKey  + Environment.NewLine);
                        }

                        //now try to verify the signature
                        try
                        {
                            sign = ClassRSA.checkSignature(body, signature, yourPublicKey).ToString();
                            sign = "OK";
                        }
                        catch (Exception ex)
                        {
                            sign = "FAIL";
                            MessageBox.Show(ex.ToString());
                        }
                        
                        
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(ex.ToString());
                    }
                }

                //basic mail, just show it
                listView1.Items.Add(new ListViewItem(new string[] { fromMail, subject, date, sign, enc, stevec.ToString()}));
                bodys[stevec] = body;
                
                stevec++;
            }
        }
    }
}
