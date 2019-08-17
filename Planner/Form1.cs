using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

using System.Drawing.Imaging;
using System.IO;
using System.Data.SqlClient;
using System.Configuration;

namespace Planner
{
    public partial class Form1 : Form
    {
        private DataGridViewCell drag_Image;
        private DataGridViewCell drag_ID;
        private object HoldingID;
        private string CursorID;
        public String myPath = System.IO.Path.GetDirectoryName(Application.ExecutablePath) + @"\Images\";
        public Point startPoint;
        public Point startCellPoint;
        public Point currentCellPoint;
        public int maxDistance = 0;
        public int isHidden = 0;
        public int startCell = 0;
        public int counter = 0;
        public Cursor curFail;

        public Form1()
        {
            InitializeComponent();
            fillData();
            dataGridView1.AllowDrop = true;
            HelpText.Text = File.ReadAllText(myPath + "README.txt");
            customScrollbar1.Maximum = dataGridView1.RowCount;

            // Define a custom cursor incase the temporary cursor fails for whatever reason 
            Bitmap DragCursorFail = new Bitmap(myPath + "-1.png");
            curFail = new Cursor(DragCursorFail.GetHicon());
        }

        private void fillData() //Populate both tables
        {
            //Start first table(bank), we are defining the placeholder image and ID (-2), wanted a simple ID that would never clash with an item
            String ItemID = "-2";
            Image img = Image.FromFile(myPath + "-2.png"); 
            Object[] row = new object[] { img, ItemID, img, ItemID, img, ItemID, img, ItemID, img, ItemID, img, ItemID, img, ItemID, img, ItemID, };
            dataGridView1.Rows.Add(row);

            for (int i = 0; i < 103; i++) //Creating a total of 102 rows
            {
                if (i < 101)
                {
                    row = new object[] { img, ItemID, img, ItemID, img, ItemID, img, ItemID, img, ItemID, img, ItemID, img, ItemID, img, ItemID, };
                    dataGridView1.Rows.Add(row);
                }
                else
                    break;
            }
            //Filling the 'inventory' this reads from the CSV file and associates with PNG file name
            var reader = new StreamReader(File.OpenRead(myPath + "ListNameID.csv"));
            var line1 = reader.ReadLine();
            var line2 = reader.ReadLine();
            while (!reader.EndOfStream)
            {
                var line = reader.ReadLine();               //Split our file path to then automate the process
                string[] ItemName = line.Split(',');
                string str = myPath + ItemName[0] + ".png";     //0 = ID, 1 = Name
                Image ItemImage = Image.FromFile(myPath + "-2.png");   
                if (string.Equals(ItemName[1], "null") || (string.Equals(ItemName[1], "Null")) || ItemName[1].Length < 1)
                {
                    // Skip nulls, this is now redundant that nulls and 'nonames' were physically removed from the list
                    // At the risk of breaking something, we will not remove this
                }
                else
                {
                    try
                    {
                        ItemImage = Image.FromFile(str);                //create the columns and rows
                        row = new object[] { ItemImage, ItemName[0], ItemName[1] };
                        dataGridView2.Rows.Add(row);
                        for (int i = 0; i < dataGridView2.RowCount; i++)
                        {
                            dataGridView2.Rows[i].Visible = false;
                        }
                    }
                    catch (FileNotFoundException ex)
                    {
                        //Item not added to list if no image, this also should now be redundant with the new tool to separate all the images, but will leave to catch any errors
                    }
                }
            }
        }

        private void textBox1_KeyPress(object sender, KeyPressEventArgs e)  //Inventory search function
        {
            if (e.KeyChar == (char)Keys.Enter)              //When pressing Enter, filter the inventory for all items containing the entered text
            {
                ShowAll.Image = Image.FromFile(myPath + "ShowAllN.png");    //Switching toggleable buttons to 'off'
                HideAll.Image = Image.FromFile(myPath + "HideAllN.png");
                dataGridView2.Refresh();
                for (int i = 0; i < dataGridView2.RowCount; i++)
                {
                    if (dataGridView2.Rows[i].Cells[2].Value.ToString().ToUpper().Contains(textBox1.Text.ToUpper()))
                    {
                        dataGridView2.Rows[i].Visible = true;       //Show matches
                    }
                    else
                    {
                        dataGridView2.Rows[i].Visible = false;      //Hide those that do not match
                    }

                }
            }
        }

        public bool NoDuplicates(string IDtoCheck) //Check and prevent adding an item already present in the bank
        {
            int foundFlag = 0;
            for (int i = 0; i < dataGridView1.Rows.Count; i++)
            {
                for (int j = 1; j / 2 < 16 / 2; j += 2) //number of columns is 16 but every other value is the ID
                {
                    if (dataGridView1.Rows[i].Cells[j].Value.ToString() == IDtoCheck)
                    {
                        foundFlag = i = j = dataGridView1.Rows.Count; // stop both "for" loops by forcing a value larger than the number of tiles
                    }
                }
            }
            if (foundFlag > 0)
            {
                //MessageBox.Show("That item is already in the bank.");
                return false;
            }
            return true;
        }

        private void dataGridView2_CellClick(object sender, DataGridViewCellEventArgs e)  //Add the selected cell (inventory) to the first PLACEHOLDER (ID -2) cell in the bank
        {
            int k = dataGridView2.CurrentRow.Index;
            if (NoDuplicates(dataGridView2.Rows[k].Cells[1].Value.ToString())) //Also verify the item hasn't already been added
            {
                int foundFlag = 0;
                for (int i = 0; i < dataGridView1.Rows.Count; i++)
                {
                    for (int j = 1; j / 2 < 16 / 2; j += 2) //number of columns is 16 but every other values is the ID
                    {
                        if (dataGridView1.Rows[i].Cells[j].Value.ToString() == "-2") //Replace any cell with placeholder
                        {
                            dataGridView1.Rows[i].Cells[j - 1].Value = dataGridView2.Rows[k].Cells[0].Value; //add the image to the cell BEFORE (left) of the cell containing "-2"
                            dataGridView1.Rows[i].Cells[j].Value = dataGridView2.Rows[k].Cells[1].Value.ToString(); //add the ID, replacing "-2"
                            foundFlag = i = j = dataGridView1.Rows.Count; // stop both "for" loops by forcing a value larger than the number of tiles
                        }
                    }
                }
                if (foundFlag == 0)
                {
                    MessageBox.Show("Bank is full.");
                }
            }
        }

        private void SwapBtn_Click(object sender, EventArgs e)  //Changing the active button and image
        {
            SwapBtn.Image = Image.FromFile(myPath + "swap2.png");
            InsertBtn.Image = Image.FromFile(myPath + "Insert1.png");

            SwapBtn.Enabled = false;
            InsertBtn.Enabled = true;
        }

        private void InsertBtn_Click(object sender, EventArgs e)  //Changing the active button and image
        {
            SwapBtn.Image = Image.FromFile(myPath + "swap1.png");
            InsertBtn.Image = Image.FromFile(myPath + "insert2.png");

            SwapBtn.Enabled = true;
            InsertBtn.Enabled = false;
        }

        private void FillerBtn_Click(object sender, EventArgs e)  //Adding in "Bank Filler" Items to the bank, look for amount to add 1/5/10/X
        {
            int k = 1; //Setting the default number of fillers to add
            if (Cust5.Enabled == false)
                k = 5;
            if (Cust10.Enabled == false)
                k = 10;
            if (CustX.Enabled == false)
                try
                {
                    k = Convert.ToInt16(CustText.Text);     //Use the numerical value entered into the text box
                }
                catch (Exception ex)
                {

                }
            for (int h = 0; h < k; h++)
            {
                int foundFlag = 0;
                for (int i = 0; i < dataGridView1.Rows.Count; i++)
                {
                    for (int j = 1; j / 2 < 16 / 2; j += 2) //number of columns is 16 but every other values is the ID
                    {
                        if (dataGridView1.Rows[i].Cells[j].Value.ToString() == "-2") //Replace any cell with placeholder
                        {
                            dataGridView1.Rows[i].Cells[j - 1].Value = Image.FromFile(myPath + "-1.png");
                            dataGridView1.Rows[i].Cells[j].Value = "-1";
                            foundFlag = i = j = dataGridView1.Rows.Count; // stop both "for" loops by forcing a value larger than the number of tiles
                        }
                    }
                }
                if (foundFlag == 0)
                {
                    MessageBox.Show("Bank is full");
                    h = k;
                }
            }
        }

        private void Cust1_Click(object sender, EventArgs e)  //Changing the active button and image (For Bank-Filler only) 
        {
            Cust1.Image = Image.FromFile(myPath + "Add1Y.png");     //Add 1
            Cust5.Image = Image.FromFile(myPath + "Add5N.png");
            Cust10.Image = Image.FromFile(myPath + "Add10N.png");
            CustX.Image = Image.FromFile(myPath + "AddXN.png");

            Cust1.Enabled = false;
            Cust5.Enabled = true;
            Cust10.Enabled = true;
            CustX.Enabled = true;
        }

        private void Cust5_Click(object sender, EventArgs e)    //Changing the active button and image (For Bank-Filler only)
        {
            Cust1.Image = Image.FromFile(myPath + "Add1N.png");
            Cust5.Image = Image.FromFile(myPath + "Add5Y.png");     //Add 5
            Cust10.Image = Image.FromFile(myPath + "Add10N.png");
            CustX.Image = Image.FromFile(myPath + "AddXN.png");

            Cust1.Enabled = true;
            Cust5.Enabled = false;
            Cust10.Enabled = true;
            CustX.Enabled = true;
        }

        private void Cust10_Click(object sender, EventArgs e)   //Changing the active button and image (For Bank-Filler only)
        {
            Cust1.Image = Image.FromFile(myPath + "Add1N.png");
            Cust5.Image = Image.FromFile(myPath + "Add5N.png");
            Cust10.Image = Image.FromFile(myPath + "Add10Y.png");       //Add 10
            CustX.Image = Image.FromFile(myPath + "AddXN.png");

            Cust1.Enabled = true;
            Cust5.Enabled = true;
            Cust10.Enabled = false;
            CustX.Enabled = true;
        }

        private void CustX_Click(object sender, EventArgs e)    //Changing the active button and image (For Bank-Filler only)
        {
            Cust1.Image = Image.FromFile(myPath + "Add1N.png");
            Cust5.Image = Image.FromFile(myPath + "Add5N.png");
            Cust10.Image = Image.FromFile(myPath + "Add10N.png");
            CustX.Image = Image.FromFile(myPath + "AddXY.png");         //Add X

            Cust1.Enabled = true;
            Cust5.Enabled = true;
            Cust10.Enabled = true;
            CustX.Enabled = false;
        }

        private void CustText_TextChanged(object sender, EventArgs e)   //Changing the active button and image (For Bank-Filler only) this allows X
        {
            Cust1.Image = Image.FromFile(myPath + "Add1N.png");
            Cust5.Image = Image.FromFile(myPath + "Add5N.png");
            Cust10.Image = Image.FromFile(myPath + "Add10N.png");
            CustX.Image = Image.FromFile(myPath + "AddXY.png");         //Add X, this is selected if the user enters text into the textbox, even without selecting "x"

            Cust1.Enabled = true;
            Cust5.Enabled = true;
            Cust10.Enabled = true;
            CustX.Enabled = false;
        }

        private void Settings_Click(object sender, EventArgs e)     //Open Help/Settings
        {
            //We are hiding and showing many different objects - honestly doing this with images maybe isn't the best way, but it looks nice.
            Settings.Image = Image.FromFile(myPath + "Settings2.png");
            //Buttons
            SaveBank.Visible = true;
            SaveImage.Visible = true;
            LoadBank.Visible = true;
            ShowAll.Visible = true;
            HideAll.Visible = true;
            ClearFillers.Visible = true;
            DeleteBank.Visible = true;
            //Options/Menu
            SettingsClose.Visible = true;
            SettingsHelp.Visible = true;
            OptionsBox.Visible = true;
        }

        private void NewTab_Click(object sender, EventArgs e)   //We will add a new row of cells
        {
            //TODO: ability to insert tab lines, this will require increasing total bank space as tab lines would be identified as items
        }

        private void dataGridView1_DragOver(object sender, DragEventArgs e)  //Define the custom cursor - this is the item you are dragging
        {
            e.Effect = DragDropEffects.Move;
            if (isHidden == 1)
            {
                isHidden = 2;
                try
                {
                    Bitmap DragCursor = new Bitmap(myPath + CursorID + ".png");
                    DragCursor.MakeTransparent(Color.FromArgb(0, 0, 0, 0));     //This does nothing, I think, ideally the image would be slightly transparent
                    Cursor cur;
                    cur = new Cursor(DragCursor.GetHicon());
                    Cursor.Current = cur;
                    DragCursor.Dispose();
                }
                catch (Exception ex)
                {
                    Cursor.Current = curFail;
                }
            }
        }

        private void dataGridView1_DragDrop(object sender, DragEventArgs e)     //Observe were a drag begins and ends to determine what items to switch
        {
            // The mouse locations are relative to the screen, so they must be converted to client coordinates.
            Point clientPoint = dataGridView1.PointToClient(new Point(e.X, e.Y));
            int currentDistance;

            // Get the row index of the item the mouse is below. 
            DataGridView.HitTestInfo hti = dataGridView1.HitTest(clientPoint.X, clientPoint.Y);
            DataGridViewCell targetCell = dataGridView1[hti.ColumnIndex, hti.RowIndex];
            DataGridViewCell targetCellID = dataGridView1[hti.ColumnIndex + 1, hti.RowIndex];

            // If the drag operation was a move then remove and insert the row. First we must define was a move is, we will say at least 25
            if (e.Effect == DragDropEffects.Move)
            {
                currentDistance = ((startPoint.X - clientPoint.X) * (startPoint.X - clientPoint.X) + (startPoint.Y - clientPoint.Y) * (startPoint.Y - clientPoint.Y));
                if (currentDistance > maxDistance) maxDistance = currentDistance;
                textBox2.Text = "maxDistance=" + maxDistance.ToString(); 
                if (maxDistance < 25)  // didn't drag far enough so it is a delete (5 pixels)
                {
                    // the cursor only moved 5 pixels is any direction instead of a move is a delete
                    dataGridView1.Rows[dataGridView1.CurrentCell.RowIndex].Cells[dataGridView1.CurrentCell.ColumnIndex].Value = Image.FromFile(myPath + "-2.png");
                    dataGridView1.Rows[dataGridView1.CurrentCell.RowIndex].Cells[dataGridView1.CurrentCell.ColumnIndex + 1].Value = "-2"; //Set placeholder ID

                }
                else
                {
                    maxDistance = 0;

                    if (SwapBtn.Enabled == false)       //This is named backwards using false... This is for SWAPPING items
                    {
                        HoldingID = targetCellID.Value;
                        targetCellID.Value = drag_ID.Value;
                        drag_ID.Value = HoldingID;
                        targetCell.Value = Image.FromFile(myPath + CursorID + ".png");
                        drag_Image.Value = Image.FromFile(myPath + HoldingID.ToString() + ".png");
                        dataGridView1.Refresh();



                        counter = counter + 1;
                        //label2.Text = Convert.ToString(counter);
                        //label3.Text = drag_ID.Value.ToString();


                    }
                    else
                    {
                        //Insert mode
                        int endCell = (hti.RowIndex * 16) + hti.ColumnIndex;
                        textBox2.Text = startCell.ToString() + " To " + endCell.ToString();
                        if (startCell < endCell)
                        {
                            // If the original item is AFTER the new position, push all up
                            for (int i = startCell; i < endCell; i += 2)
                            {
                                int x1 = i % 16;
                                int y1 = i / 16;
                                int x2 = (i + 2) % 16;
                                int y2 = (i + 2) / 16;
                                dataGridView1.Rows[y1].Cells[x1].Value = dataGridView1.Rows[y2].Cells[x2].Value;
                                dataGridView1.Rows[y1].Cells[x1 + 1].Value = dataGridView1.Rows[y2].Cells[x2 + 1].Value;
                            }
                            targetCellID.Value = CursorID;
                            targetCell.Value = Image.FromFile(myPath + CursorID + ".png");
                        }
                        else if (startCell > endCell)
                        {
                            // If the original item is BEFORE the new position, push all down
                            for (int i = startCell; i > endCell; i -= 2)
                            {
                                int x1 = i % 16;
                                int y1 = i / 16;
                                int x2 = (i - 2) % 16;
                                int y2 = (i - 2) / 16;

                                dataGridView1.Rows[y1].Cells[x1].Value = dataGridView1.Rows[y2].Cells[x2].Value;
                                dataGridView1.Rows[y1].Cells[x1 + 1].Value = dataGridView1.Rows[y2].Cells[x2 + 1].Value;
                            }
                            targetCellID.Value = CursorID;
                            targetCell.Value = Image.FromFile(myPath + CursorID + ".png");
                        }
                        else
                        {
                            // If the image is dragged and then returned to the same spot, we restore it
                            targetCellID.Value = CursorID;
                            targetCell.Value = Image.FromFile(myPath + CursorID + ".png");
                        }

                    }
                }
            }
        }

        public void dataGridView1_MouseDown(object sender, MouseEventArgs e)  //The position (bank item) in which the user selects for dragging
        {
            maxDistance = 0;
            if (e.Button == System.Windows.Forms.MouseButtons.Left)
            {
                DataGridView.HitTestInfo hti = dataGridView1.HitTest(e.X, e.Y);
                dataGridView1.CurrentCell = dataGridView1[hti.ColumnIndex, hti.RowIndex];  //make this cell the currently selected cell
                drag_Image = dataGridView1[hti.ColumnIndex, hti.RowIndex];
                drag_ID = dataGridView1[hti.ColumnIndex + 1, hti.RowIndex];
                CursorID = drag_ID.Value.ToString();
                startCellPoint = MousePosition;
                startPoint.X = e.X;
                startPoint.Y = e.Y;
                startCell = (hti.RowIndex * 16) + hti.ColumnIndex;
                textBox2.Text = startPoint.X.ToString() + "," + startPoint.Y.ToString();
                DragDropEffects dropEffect = dataGridView1.DoDragDrop(drag_Image, DragDropEffects.Move);
            }
        }

        private void dataGridView1_GiveFeedback(object sender, GiveFeedbackEventArgs e)     //Changing the cursor image to the dragged item
        {
            int currentDistance;
            e.UseDefaultCursors = false;
            currentCellPoint = MousePosition;
            textBox2.Text = currentCellPoint.X.ToString() + "," + currentCellPoint.Y.ToString();
            currentDistance = ((startCellPoint.X - currentCellPoint.X) * (startCellPoint.X - currentCellPoint.X) + (startCellPoint.Y - currentCellPoint.Y) * (startCellPoint.Y - currentCellPoint.Y));
            if (currentDistance > maxDistance) maxDistance = currentDistance;
            if ((maxDistance > 25) & (isHidden == 0))
            {   // hide the starting image if it is moved more than 5 pixels
                dataGridView1.Rows[dataGridView1.CurrentCell.RowIndex].Cells[dataGridView1.CurrentCell.ColumnIndex].Value = Image.FromFile(myPath + "-2.png");
                isHidden = 1;
            }
            else
            {
                if (maxDistance < 25) isHidden = 0;
            }
            if (Control.MouseButtons != MouseButtons.Left)
            {
                // The mouse button was lifted without dragging so we assume the user wants to delete the item
                dataGridView1.Rows[dataGridView1.CurrentCell.RowIndex].Cells[dataGridView1.CurrentCell.ColumnIndex].Value = Image.FromFile(myPath + "-2.png");
                dataGridView1.Rows[dataGridView1.CurrentCell.RowIndex].Cells[dataGridView1.CurrentCell.ColumnIndex + 1].Value = "-2"; //Set placeholder ID
            }

        }

        private void SettingsClose_Click(object sender, EventArgs e)    //Close the settings menu, hide/show images
        {
            Settings.Image = Image.FromFile(myPath + "Settings.png");
            OptionsBox.Visible = false;
            SettingsClose.Visible = false;
            SettingsHelp.Visible = false;
            SaveBank.Visible = false;
            SaveImage.Visible = false;
            LoadBank.Visible = false;
            ShowAll.Visible = false;
            HideAll.Visible = false;
            ClearFillers.Visible = false;
            DeleteBank.Visible = false;
        }

        private void SaveBank_Click(object sender, EventArgs e)     //Save a .BIN file of the current bank to reload later
        {
            DataGridView BankGrid = dataGridView1;
            SaveFileDialog saveFileDialog1 = new SaveFileDialog();
            saveFileDialog1.InitialDirectory = System.IO.Path.GetDirectoryName(Application.ExecutablePath) + @"\Bank Saves\";
            saveFileDialog1.CheckPathExists = true;
            saveFileDialog1.Filter = "Binary File|*.bin";
            saveFileDialog1.Title = "Save your bank";
            if (saveFileDialog1.ShowDialog() == DialogResult.OK)
            {
                string file = saveFileDialog1.FileName;
                using (BinaryWriter bw = new BinaryWriter(File.Open(file, FileMode.Create)))
                {
                    bw.Write(BankGrid.Columns.Count);
                    bw.Write(BankGrid.Rows.Count);
                    foreach (DataGridViewRow dgvR in BankGrid.Rows)
                    {
                        for (int j = 1; j / 2 < 16 / 2; j += 2)
                        {
                            object val = dgvR.Cells[j].Value;
                            if (val == null)
                            {
                                bw.Write(false);
                                bw.Write(false);
                            }
                            else
                            {
                                bw.Write(true);
                                bw.Write(val.ToString());
                            }
                        }
                    }
                }
            }
        }

        private void SaveImage_Click(object sender, EventArgs e)        //Save a .PNG image of the entire bank
        {
            dataGridView1.FirstDisplayedScrollingRowIndex = 0; //Force scroll to top otherwise the first row is lost, why? No idea.
            //Resize DataGridView to full height, this code is a bit redundant, resizing the grid twice, but it works
            int height = dataGridView1.Height;
            dataGridView1.Height = dataGridView1.RowCount * dataGridView1.RowTemplate.Height;

            //Create a Bitmap and draw the DataGridView on it.
            Bitmap BankImage = new Bitmap(this.dataGridView1.Width, this.dataGridView1.Height);
            dataGridView1.DrawToBitmap(BankImage, new Rectangle(0, 0, this.dataGridView1.Width, this.dataGridView1.Height));

            //Resize DataGridView back to original height.
            dataGridView1.Height = height;

            Image Background = new Bitmap(myPath + "SaveImageBKG.png");
            Image Foreground = new Bitmap(myPath + "SaveImageBKG.png");
            Graphics g = Graphics.FromImage(Background);// create graphics object from big image,
            g.DrawImage(BankImage, new Point(34, 34));//draw small image over big image starting from position 0,0
            //you have big image with small image on it
            g.DrawImage(Foreground, new Point(0, 0));

            //Save the Bitmap to folder.
            SaveFileDialog saveFileDialog1 = new SaveFileDialog();
            saveFileDialog1.InitialDirectory = System.IO.Path.GetDirectoryName(Application.ExecutablePath) + @"\Bank Saves\";
            saveFileDialog1.CheckPathExists = true;
            saveFileDialog1.Filter = "PNG Image|*.png";
            saveFileDialog1.Title = "Save a bank image";
            saveFileDialog1.ShowDialog();
            if (saveFileDialog1.FileName != "")
            {
                // Saves the Image via a FileStream created by the OpenFile method  
                System.IO.FileStream fs = (System.IO.FileStream)saveFileDialog1.OpenFile();
                switch (saveFileDialog1.FilterIndex)
                {
                    case 1:
                        Background.Save(fs, System.Drawing.Imaging.ImageFormat.Png);
                        break;
                }
                fs.Close();
            }
        }

        private void LoadBank_Click(object sender, EventArgs e)     //Select and open a .BIN file
        {
            DataGridView BankGrid = dataGridView1;

            OpenFileDialog openFileDialog1 = new OpenFileDialog();
            openFileDialog1.InitialDirectory = System.IO.Path.GetDirectoryName(Application.ExecutablePath) + @"\Bank Saves\";
            openFileDialog1.Filter = "Binary File|*.bin";
            openFileDialog1.Title = "Load your bank";

            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                string file = openFileDialog1.FileName;
                BankGrid.Rows.Clear();
                using (BinaryReader bw = new BinaryReader(File.Open(file, FileMode.Open)))
                {
                    int n = bw.ReadInt32();
                    int m = bw.ReadInt32();
                    for (int i = 0; i < m; ++i)
                    {
                        BankGrid.Rows.Add();
                        for (int j = 1; j < n; j += 2)
                        {
                            if (bw.ReadBoolean())
                            {
                                BankGrid.Rows[i].Cells[j].Value = bw.ReadString();
                                BankGrid.Rows[i].Cells[j - 1].Value = Image.FromFile(myPath + BankGrid.Rows[i].Cells[j].Value.ToString() + ".png");
                            }
                            else bw.ReadBoolean();
                        }
                    }
                }
            }
            dataGridView1.Refresh();
        }

        private void SettingsHelp_Click(object sender, EventArgs e)    //Open settings menu
        {
            SaveBank.Visible = false;
            SaveImage.Visible = false;
            LoadBank.Visible = false;
            ShowAll.Visible = false;
            HideAll.Visible = false;
            ClearFillers.Visible = false;
            DeleteBank.Visible = false;

            OptionsBox.Visible = false;
            SettingsHelp.Visible = false;
            SettingsClose.Visible = false;

            HelpClose.Visible = true;
            HelpText.Visible = true;
            SettingsMenu2.Visible = true;
        }

        private void HelpClose_Click(object sender, EventArgs e)        //close help menu
        {
            SettingsMenu2.Visible = false;
            HelpClose.Visible = false;
            HelpText.Visible = false;

            SaveBank.Visible = true;
            SaveImage.Visible = true;
            LoadBank.Visible = true;
            ShowAll.Visible = true;
            HideAll.Visible = true;
            ClearFillers.Visible = true;
            DeleteBank.Visible = true;

            SettingsClose.Visible = true;
            SettingsHelp.Visible = true;
            OptionsBox.Visible = true;
        }

        private void ShowAll_Click(object sender, EventArgs e)      //Show all the available items in the inventory
        {
            ShowAll.Image = Image.FromFile(myPath + "ShowAllY.png");
            HideAll.Image = Image.FromFile(myPath + "HideAllN.png");

            for (int i = 0; i < dataGridView2.RowCount; i++)
            {
                dataGridView2.Rows[i].Visible = true;
            }
        }

        private void HideAll_Click(object sender, EventArgs e)      //Hide all the items from the inventory
        {
            ShowAll.Image = Image.FromFile(myPath + "ShowAllN.png");
            HideAll.Image = Image.FromFile(myPath + "HideAllY.png");

            for (int i = 0; i < dataGridView2.RowCount; i++)
            {
                dataGridView2.Rows[i].Visible = false;
            }
        }

        private void ClearFillers_DoubleClick(object sender, EventArgs e)   //Remove all bank fillers
        {
            for (int i = 0; i < dataGridView1.Rows.Count; i++)
            {
                for (int j = 1; j / 2 < 16 / 2; j += 2) //number of columns is 16 but every other values is the ID
                    if (dataGridView1.Rows[i].Cells[j].Value.ToString() == "-1") //Replace any cell with placeholder
                    {
                        dataGridView1.Rows[i].Cells[j - 1].Value = Image.FromFile(myPath + "-2.png"); //add the image to the cell BEFORE (left) of the cell containing "-2"
                        dataGridView1.Rows[i].Cells[j].Value = "-2"; //add the ID, replacing "-2"
                    }
            }
        }
        private void DeleteBank_DoubleClick(object sender, EventArgs e)     //Clear the entire bank
        {
            for (int i = 0; i < dataGridView1.Rows.Count; i++)
            {
                for (int j = 1; j / 2 < 16 / 2; j += 2) //number of columns is 16 but every other values is the ID
                {
                    dataGridView1.Rows[i].Cells[j - 1].Value = Image.FromFile(myPath + "-2.png"); //add the image to the cell BEFORE (left) of the cell containing "-2"
                    dataGridView1.Rows[i].Cells[j].Value = "-2"; //add the ID, replacing "-2"
                }
            }
        }

        //Custom ScrollBars, this was way to difficult to figure out for something so simple.
        //Thanks to Greg Ellis at https://www.codeproject.com/Articles/14801/How-to-skin-scrollbars-for-Panels-in-C for a helpful resource in creating CustomControls solution

        //For whatever reason mouse wheel scroll does not work when the custom scroll bar is used, this needs to be addressed
        private void dataGridView1_Scroll(object sender, ScrollEventArgs e)     //Reassign the default scrollar to our custom image one
        {
            customScrollbar1.Value = e.NewValue;
        }

        private void customScrollbar1_Scroll(object sender, EventArgs e)        //Also reassigning the scrollbar for the bank
        {
            dataGridView1.FirstDisplayedScrollingRowIndex = customScrollbar1.Value;
        }
        
        //The custom scroll bar for the inventory currently does not work by itself, this is due to rows being hidden, a workaround is needed
        private void customScrollbar2_Scroll(object sender, EventArgs e)        //Inventory custom scroll bar
        {

            //label5.Text = "dataGridView2.Rows[customScrollbar2.Value].Displayed: " + dataGridView2.Rows[customScrollbar2.Value].Displayed;
            dataGridView2.Rows[customScrollbar2.Value].Selected = true;
            //dataGridView2.FirstDisplayedScrollingRowIndex = dataGridView2.SelectedRows[0].Index;
            if (dataGridView2.Rows[customScrollbar2.Value].Visible)
            {
                // dataGridView2.FirstDisplayedScrollingRowIndex = customScrollbar2.Value;
            }
        }

        private void dataGridView2_Scroll(object sender, ScrollEventArgs e)     //Inventory custom scroll bar
        {
            //label2.Text = "customSB.Max:" + customScrollbar2.Maximum.ToString();
            customScrollbar2.Maximum = dataGridView2.Rows.GetRowCount(DataGridViewElementStates.Visible);
            customScrollbar2.Value = e.NewValue;
            //label3.Text = dataGridView2.FirstDisplayedCell.ToString();
            //label4.Text = "customSB.value:" + customScrollbar2.Value.ToString();
            //label5.Text = "dataGridView2.Rows[customScrollbar2.Value].Displayed: " + dataGridView2.Rows[customScrollbar2.Value].Displayed;
        }
    }
}