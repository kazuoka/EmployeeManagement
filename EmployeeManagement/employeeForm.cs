using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Data.SqlClient;

namespace EmployeeManagement
{
    public partial class employeeForm : Form
    {
        public employeeForm()
        {
            InitializeComponent();
        }

        private string dbCommand = ""; // INSERT INTO OR UPDATE

        private void employeeForm_Load(object sender, EventArgs e)
        { 

            db.openConnection();

            updateDataBinding();

        }

        private void createAutoComplete()
        {

            // Open the connection.
            db.openConnection();

            SqlDataReader rd;

            try
            {

                db.sql = "SELECT AutoID, FirstName + ' ' + LastName AS FullName FROM Employees ORDER BY AutoID ASC";
                db.cmd.CommandText = db.sql;

                AutoCompleteStringCollection autoComp = new AutoCompleteStringCollection();

                rd = db.cmd.ExecuteReader();

                if (rd.HasRows)
                {
                    while (rd.Read())
                    {
                        if (findByIDRadioButton.Checked)
                        {
                            autoComp.Add(Convert.ToString(rd.GetInt32(0))); // AutoID || Employee ID
                        }
                        else
                        {
                            autoComp.Add(rd.GetString(1)); // Full Name
                        }
                    }
                }

                keywordTextBox.AutoCompleteMode = AutoCompleteMode.SuggestAppend;
                keywordTextBox.AutoCompleteSource = AutoCompleteSource.CustomSource;
                keywordTextBox.AutoCompleteCustomSource = autoComp;

                // Close the reader.
                rd.Close();

            }
            catch (Exception ex)
            {
                MessageBox.Show("Error: " + ex.Message, "Auto Complete : GeneralKazuoka.", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                // Close the connection.
                db.closeConnection();
            }

            var txt = keywordTextBox;
            if (txt.CanSelect)
            {
                txt.Select();
                txt.SelectAll();
            }

        }

        private void updateDataBinding(SqlCommand command = null, Button btn = null)
        {
            try
            {

                TextBox txt;
                ComboBox cmb;
                RadioButton rdb;

                foreach (Control c in groupBox1.Controls)
                {
                    if(c.GetType() == typeof(TextBox))
                    {
                        txt = (TextBox)c;
                        txt.DataBindings.Clear();
                        txt.Text = "";
                    }
                    else if (c.GetType() == typeof(ComboBox))
                    {
                        cmb = (ComboBox)c;
                        cmb.DataBindings.Clear();
                        cmb.DropDownStyle = ComboBoxStyle.DropDownList;
                    }
                }

                foreach (Control c in groupBox3.Controls)
                {
                    if (c.GetType() == typeof(ComboBox))
                    {
                        cmb = (ComboBox)c;
                        cmb.DataBindings.Clear();
                        cmb.DropDownStyle = ComboBoxStyle.DropDownList;

                        if (btn == null)
                        {
                            cmb.Enabled = false;
                        }

                    }
                    else if (c.GetType() == typeof(RadioButton))
                    {
                        rdb = (RadioButton)c;
                        if(btn == null)
                        {
                            rdb.Checked = false;
                        }
                    }
                }

                if(command == null)
                {
                    // db.sql = "";
                    db.cmd.CommandText = "SELECT Employees.*, Departments.DepartmentName FROM Employees INNER JOIN Departments ON Employees.DepartmentID = Departments.DepartmentID ORDER BY Employees.AutoID ASC";
                }
                else
                {
                    db.cmd = command;
                }

                db.da = new SqlDataAdapter(db.cmd);
                db.ds = new DataSet();
                db.da.Fill(db.ds, "EmployeeList");

                db.bs = new BindingSource(db.ds, "EmployeeList");

                bindingNavigator1.BindingSource = db.bs;

                // Simple Data Bindings
                autoIDTextBox.DataBindings.Add("Text", db.bs, "AutoID");
                firstNameTextBox.DataBindings.Add("Text", db.bs, "FirstName");
                lastNameTextBox.DataBindings.Add("Text", db.bs, "LastName");
                emailTextBox.DataBindings.Add("Text", db.bs, "Email");
                jobTitleTextBox.DataBindings.Add("Text", db.bs, "JobTitle");
                phoneTextBox.DataBindings.Add("Text", db.bs, "Phone");

                dataGridView1.Enabled = true;
                dataGridView1.DataSource = db.bs;

                dataGridView1.AutoResizeColumns(DataGridViewAutoSizeColumnsMode.AllCells);
                dataGridView1.SelectionMode = DataGridViewSelectionMode.FullRowSelect;

                // Complex Data Binding

                string mySQL = "SELECT DepartmentID, DepartmentName FROM Departments ORDER BY DepartmentID ASC";

                db.cmd.CommandText = mySQL;
                db.da.SelectCommand = db.cmd;
                db.da.Fill(db.ds, "Division");

                DataRow row1 = db.ds.Tables["Division"].NewRow();
                row1["DepartmentID"] = 0;
                row1["DepartmentName"] = "-- Please Select --";
                db.ds.Tables["Division"].Rows.InsertAt(row1, 0);

                divisionComboBox.DataSource = db.ds.Tables["Division"];
                divisionComboBox.DisplayMember = "DepartmentName";
                divisionComboBox.ValueMember = "DepartmentID";
                divisionComboBox.DataBindings.Add("SelectedValue", db.bs, "DepartmentID");

                if(btn == null)
                {

                    db.cmd.CommandText = mySQL;
                    db.da.SelectCommand = db.cmd;

                    DataSet DataSt = new DataSet();

                    db.da.Fill(DataSt, "SearchDivision");

                    DataRow row2 = DataSt.Tables["SearchDivision"].NewRow();
                    row2["DepartmentID"] = 0;
                    row2["DepartmentName"] = "-- Please Select --";
                    DataSt.Tables["SearchDivision"].Rows.InsertAt(row2, 0);

                    findByDivisionComboBox.DataSource = DataSt.Tables["SearchDivision"];
                    findByDivisionComboBox.DisplayMember = "DepartmentName";
                    findByDivisionComboBox.ValueMember = "DepartmentID";

                }

                createAutoComplete();

            }
            catch (Exception ex)
            {
                MessageBox.Show("Update Data Binding Error: " + ex.Message);
            }
            finally
            {
                if (keywordTextBox.CanSelect)
                {
                    keywordTextBox.Select();
                }
            }
        }

        private void employeeForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            db.closeConnection();
        }

        private void bindingNavigatorAddNewItem_Click(object sender, EventArgs e)
        {

            try
            {

                if (bindingNavigatorAddNewItem.Text == "Add new")
                {

                    bindingNavigatorAddNewItem.Text = "Cancel";
                    bindingNavigatorAddNewItem.ToolTipText = "Cancel";

                    bindingNavigatorMoveFirstItem.Enabled = false;
                    bindingNavigatorMovePreviousItem.Enabled = false;
                    bindingNavigatorPositionItem.Enabled = false;
                    bindingNavigatorMoveNextItem.Enabled = false;
                    bindingNavigatorMoveLastItem.Enabled = false;

                    dataGridView1.ClearSelection();
                    dataGridView1.Enabled = false;

                }
                else // Press Cancel button.
                {

                    bindingNavigatorAddNewItem.Text = "Add new";
                    bindingNavigatorAddNewItem.ToolTipText = "Add new";

                    bindingNavigatorMoveFirstItem.Enabled = true;
                    bindingNavigatorMovePreviousItem.Enabled = true;
                    bindingNavigatorPositionItem.Enabled = true;
                    bindingNavigatorMoveNextItem.Enabled = true;
                    bindingNavigatorMoveLastItem.Enabled = true;

                    updateDataBinding();

                    return;

                }

                TextBox txt;
                ComboBox cmb;

                foreach(Control c in groupBox1.Controls)
                {
                    if (c.GetType() == typeof(TextBox))
                    {
                        txt = (TextBox)c;
                        txt.Text = "";
                        if (txt.Name.Equals("firstNameTextBox"))
                        {
                            if (txt.CanSelect)
                            {
                                txt.Select();
                            }
                        }
                    } else if (c.GetType() == typeof(ComboBox))
                    {
                        cmb = (ComboBox)c;
                        cmb.SelectedIndex = 0;
                    }
                }

            }
            catch (Exception)
            {
                // Error:
            }
            finally
            {

            }

        }

        private void addCommandParameters()
        {
            db.cmd.Parameters.Clear();
            db.cmd.CommandText = db.sql;

            db.cmd.Parameters.AddWithValue("FirstName", firstNameTextBox.Text.Trim());
            db.cmd.Parameters.AddWithValue("LastName", lastNameTextBox.Text.Trim());
            db.cmd.Parameters.AddWithValue("Email", emailTextBox.Text.Trim());
            db.cmd.Parameters.AddWithValue("JobTitle", jobTitleTextBox.Text.Trim());
            db.cmd.Parameters.AddWithValue("Phone", phoneTextBox.Text.Trim());

            db.cmd.Parameters.AddWithValue("Division", divisionComboBox.SelectedValue);

            if (dbCommand.ToUpper() == "UPDATE")
            {
                db.cmd.Parameters.AddWithValue("ID", autoIDTextBox.Text.Trim());
            }

        }

        private void bindingNavigatorUpdateItem_Click(object sender, EventArgs e)
        {

            TextBox txt;
            foreach(Control c in groupBox1.Controls)
            {
                if(c.GetType() == typeof(TextBox))
                {
                    txt = (TextBox)c;
                    if (!txt.Name.Equals("autoIDTextBox"))
                    {
                        if (string.IsNullOrEmpty(txt.Text.Trim()))
                        {
                            MessageBox.Show("Please fill in the required fields.");
                            return;
                        }
                    }
                }
            }

            if(divisionComboBox.SelectedIndex == 0)
            {
                MessageBox.Show("Please select department in the combobox.");
                return;
            }

            db.openConnection();

            try
            {

                if (bindingNavigatorAddNewItem.Text.Equals("Add new"))
                {
                    // UPDATE SET WHERE
                    if(autoIDTextBox.Text.Trim() == "" || string.IsNullOrEmpty(autoIDTextBox.Text.Trim()))
                    {
                        MessageBox.Show("Please select an item from datagridview.");
                        return;
                    }

                    if(MessageBox.Show("ID: " + autoIDTextBox.Text.Trim() + " Do you want to update the selected record?", "C# and SQL SERVER : UPDATE", MessageBoxButtons.YesNo, MessageBoxIcon.Question, MessageBoxDefaultButton.Button2) == DialogResult.No)
                    {
                        return;
                    }

                    dbCommand = "UPDATE";
                    // WHERE clause
                    db.sql = "UPDATE Employees SET FirstName = @FirstName, LastName = @LastName, Email = @Email, JobTitle = @JobTitle, Phone = @Phone, DepartmentID = @Division WHERE AutoID = @ID";

                    addCommandParameters();

                }
                else if (bindingNavigatorAddNewItem.Text == "Cancel")
                {
                    
                    DialogResult result;
                    result = MessageBox.Show("Do you want to add a new employee record? (Y/N)",
                        "C# and SQL Server (INSERT INTO) : GeneralKazuoka.", MessageBoxButtons.YesNo,
                           MessageBoxIcon.Question);

                    if (result == DialogResult.Yes)
                    {
                        // INSERT INTO
                        dbCommand = "INSERT INTO";
                        db.sql = "INSERT INTO Employees(FirstName, LastName, Email, JobTitle, Phone, DepartmentID) VALUES(@FirstName, @LastName, @Email, @JobTitle, @Phone, @Division)";
                        // Command Parameters
                        addCommandParameters();
                    }
                    else
                    {
                        return;
                    }

                }

                int execute = db.cmd.ExecuteNonQuery();

                if(execute != -1)
                {
                    MessageBox.Show("The data have been saved. " + dbCommand);
                    updateDataBinding();
                    bindingNavigatorAddNewItem.Text = "Add new";
                }

            }
            catch (Exception ex)
            {
                MessageBox.Show("Save Data Failed: " + ex.Message);
            }
            finally
            {
                dbCommand = "";
                db.closeConnection();
            }
            

        }

        private bool IsAddingNewRecord()
        {
            if(bindingNavigatorAddNewItem.Text == "Cancel")
            {
                MessageBox.Show("Please cancel adding new record first.");
                return true;
            }
            else
            {
                return false;
            }
        }

        private void bindingNavigatorDeleteItem_Click(object sender, EventArgs e)
        {
            if(IsAddingNewRecord() == true)
            {
                return;
            }

            db.openConnection();

            if (autoIDTextBox.Text.Trim() == "" || string.IsNullOrEmpty(autoIDTextBox.Text.Trim()))
            {
                MessageBox.Show("Please select an item from datagridview.");
                return;
            }

            try
            {

                if (MessageBox.Show("ID: " + autoIDTextBox.Text.Trim() + " Do you want to delete the selected record?", "C# and SQL SERVER : DELETE", MessageBoxButtons.YesNo, MessageBoxIcon.Question, MessageBoxDefaultButton.Button2) == DialogResult.No)
                {
                    return;
                }

                // Press Yes button.
                dbCommand = "DELETE";

                db.sql = "DELETE FROM Employees WHERE AutoID = @ID";

                db.cmd.Parameters.Clear();
                db.cmd.CommandText = db.sql;

                db.cmd.Parameters.AddWithValue("ID", autoIDTextBox.Text.Trim());

                int execute = db.cmd.ExecuteNonQuery();

                if (execute != -1)
                {
                    MessageBox.Show("The data have been deleted. " + dbCommand);
                    updateDataBinding();
                }

            }
            catch (Exception ex)
            {
                MessageBox.Show("Delete data error: " + ex.Message);
            }
            finally
            {
                dbCommand = "";
                db.closeConnection();
            }

        }

        private void bindingNavigatorRefreshData_Click(object sender, EventArgs e)
        {

            if(IsAddingNewRecord() == true)
            {
                return;
            }

            updateDataBinding();

            keywordTextBox.Clear();

        }

        private void findByIDRadioButton_CheckedChanged(object sender, EventArgs e)
        {
            if(findByIDRadioButton.Checked == true)
            {
                findByIDRadioButton.ForeColor = Color.Red;

                createAutoComplete();
            }
            else
            {
                findByIDRadioButton.ForeColor = Color.Black;
            }
        }

        private void findByDivisionRadioButton_CheckedChanged(object sender, EventArgs e)
        {
            if (findByDivisionRadioButton.Checked == true)
            {
                findByDivisionRadioButton.ForeColor = Color.Red;
                findByDivisionComboBox.Enabled = true;

                createAutoComplete();
            }
            else
            {
                findByDivisionRadioButton.ForeColor = Color.Black;
                findByDivisionComboBox.Enabled = false;
            }
        }

        private void searchButton_Click(object sender, EventArgs e)
        {

            if(IsAddingNewRecord() == true)
            {
                return;
            }

            db.openConnection();

            try
            {

                if(findByDivisionRadioButton.Checked == false && 
                    findByDivisionRadioButton.Checked == false)
                {
                    if (string.IsNullOrEmpty(keywordTextBox.Text.Trim()))
                    {
                        updateDataBinding(null, searchButton);
                        return;
                    }
                }

                db.sql = "SELECT Employees.*, Departments.DepartmentName FROM Employees INNER JOIN Departments ON Employees.DepartmentID = Departments.DepartmentID ";

                if(findByIDRadioButton.Checked == true)
                {
                    if (string.IsNullOrEmpty(keywordTextBox.Text.Trim()))
                    {
                        MessageBox.Show("Please input an employee id.");
                        return;
                    }

                    // Convert to string
                    db.sql += "WHERE CONVERT(NVARCHAR, Employees.AutoID) = @Keyword1 ";
                    db.sql += "ORDER BY Employees.AutoID ASC";

                }
                else if (findByDivisionRadioButton.Checked == true)
                {
                    if(findByDivisionComboBox.SelectedIndex == 0)
                    {
                        MessageBox.Show("Please select the department.");
                        return;
                    }

                    // -OLD-
                    //db.sql += "WHERE (Employees.FirstName LIKE @Keyword2 ";
                    //db.sql += "OR Employees.LastName LIKE @Keyword2 ";
                    //db.sql += "OR Employees.JobTitle LIKE @Keyword2) ";
                    //db.sql += "AND (Employees.DepartmentID = @Keyword1x) ";
                    //db.sql += "ORDER BY Employees.AutoID ASC";

                    db.sql += "WHERE (Employees.FirstName + ' ' + Employees.LastName LIKE @Keyword2 ";
                    db.sql += "OR Employees.JobTitle LIKE @Keyword2) ";
                    db.sql += "AND (Employees.DepartmentID = @Keyword1x) ";
                    db.sql += "ORDER BY Employees.AutoID ASC";

                }
                else
                {

                    // -OLD-
                    //db.sql += "WHERE Employees.FirstName LIKE @Keyword2 ";
                    //db.sql += "OR Employees.LastName LIKE @Keyword2 ";
                    //db.sql += "OR Employees.Email = @Keyword1 ";
                    //db.sql += "OR Employees.JobTitle LIKE @Keyword2 "; // บางส่วน Prog - Dev
                    //db.sql += "OR Employees.Phone = @Keyword1 "; // ตรงกับรายละเอียด 
                    //db.sql += "ORDER BY Employees.AutoID ASC";

                    db.sql += "WHERE Employees.FirstName + ' ' + Employees.LastName LIKE @Keyword2 ";
                    db.sql += "OR Employees.Email = @Keyword1 ";
                    db.sql += "OR Employees.JobTitle LIKE @Keyword2 "; // บางส่วน Prog - Dev
                    db.sql += "OR Employees.Phone = @Keyword1 "; // ตรงกับรายละเอียด 
                    db.sql += "ORDER BY Employees.AutoID ASC";

                }

                db.cmd.CommandType = CommandType.Text;
                db.cmd.CommandText = db.sql;

                db.cmd.Parameters.Clear();

                db.cmd.Parameters.AddWithValue("Keyword1", keywordTextBox.Text.Trim());

                db.cmd.Parameters.AddWithValue("Keyword1x", findByDivisionComboBox.SelectedValue.ToString());

                // Wildcard. %
                string keywordString = string.Format("%{0}%", keywordTextBox.Text);
                db.cmd.Parameters.AddWithValue("Keyword2", keywordString);

                updateDataBinding(db.cmd, searchButton);

            }
            catch (Exception ex)
            {
                MessageBox.Show("Search Error: " + ex.Message);
            }
            finally
            {
                db.closeConnection();
                if (keywordTextBox.CanFocus)
                {
                    keywordTextBox.Focus();
                }

            }

        }
    }
}
