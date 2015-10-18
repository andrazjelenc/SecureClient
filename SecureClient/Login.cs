using System;
using System.Collections.Generic;
using System.IO;
using System.Windows.Forms;

namespace SecureClient
{
    public partial class Login : Form
    {
        //path to C:\Users\<user>\AppData\Local\SecureClient
        private static string localPath = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) + @"\SecureClient";
        
        public Login()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            //create if not exsist
            Directory.CreateDirectory(localPath);
            
            //check if files exsist
            if (!File.Exists(localPath + @"\login.txt"))
            {
                File.Create(localPath + @"\login.txt").Dispose();
            }
            if (!File.Exists(localPath + @"\profiles.txt"))
            {
                File.Create(localPath + @"\profiles.txt").Dispose();
            }
            if (!File.Exists(localPath + @"\addresser.txt"))
            {
                File.Create(localPath + @"\addresser.txt").Dispose();
            }

        }

        //login button
        private void button1_Click(object sender, EventArgs e)
        {
            string username = txtUsername.Text;
            string password = txtPassword.Text;

            string line;

            //Read login file
            System.IO.StreamReader file = new System.IO.StreamReader(localPath + @"\login.txt");
            while ((line = file.ReadLine()) != null)
            {
                string[] elements = line.Split(new string[] { "|::::|" }, StringSplitOptions.None);

                //find our login line
                if (elements[0] == username)
                {
                    //get save hash
                    string hash = elements[1];

                    if (PasswordHash.ValidatePassword(password, hash)) //compare entered password with hash
                    {
                        Client form2 = new Client(username, password);
                        this.Hide();
                        form2.ShowDialog();
                        form2.Dispose();
                        this.Close();
                    }
                    else
                    {
                        MessageBox.Show("Wrong data!");
                        txtPassword.Clear();
                    }
                    break;
                }
            }
            file.Close();
        }

        //register button
        private void button2_Click(object sender, EventArgs e)
        {
            Register form3 = new Register(localPath);
            this.Hide();
            form3.ShowDialog();
            form3.Dispose();
            this.Show();
        }

       
    }
}
