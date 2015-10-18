using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.IO;

namespace SecureClient
{
    public partial class Register : Form
    {
        private string path;

        public Register(string path)
        {
            this.path = path;
            InitializeComponent();
        }

        //regsiter button
        private void btnRegister_Click(object sender, EventArgs e)
        {
            //check passwords...
            if (txtPassword1.Text == txtPassword2.Text)
            {
                //append to login.txt
                File.AppendAllText(path + @"\login.txt", txtUsername.Text + "|::::|" + PasswordHash.CreateHash(txtPassword1.Text) + Environment.NewLine);

                //generate new key pair
                string[] rsaKeyPair = ClassRSA.GenerateRSAKey();

                //encrypt private key
                string privateKey = ClassAES.EncryptStringAes(rsaKeyPair[0], txtPassword1.Text);
                string publicKey = rsaKeyPair[1];

                //save all data to profiles.txt
                string profileString = txtUsername.Text + "|::::|" + txtHostIn.Text + "|::::|" + txtPortIn.Text + "|::::|" + txtHostOut.Text +
                    "|::::|" + txtPortOut.Text + "|::::|" + checkBox1.Checked + "|::::|" + checkBox2.Checked + "|::::|" + privateKey + "|::::|" + publicKey;

                File.AppendAllText(path + @"\profiles.txt", profileString + Environment.NewLine);

                MessageBox.Show("Profile added");
                this.Close();
            }
            else
            {
                MessageBox.Show("Wrong password");
                txtPassword1.Clear();
                txtPassword2.Clear();
            }
            
        }
    }
}
