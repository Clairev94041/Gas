﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Runtime.Remoting.Contexts;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml.Linq;
using MySql.Data.MySqlClient;
using Mysqlx.Session;
using MySqlX.XDevAPI;
using Org.BouncyCastle.Crypto.IO;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.StartPanel;
using System.Configuration;
using Mysqlx.Crud;
using static Google.Protobuf.Reflection.FieldOptions.Types;
using System.Collections;
using Google.Protobuf.WellKnownTypes;
using MySqlX.XDevAPI.Relational;

namespace Gas_Company
{
    public partial class Form2 : Form
    {
        //Server: The server name or IP address where the MySQL database is hosted.
        //Database: The name of the database to connect to..
        //Uid: The username used to authenticate the connection.
        //Pwd: The password associated with the provided username.
        private readonly string connectionString = ConfigurationManager.AppSettings["ConnectionString"];

        public Form2()
        {
            InitializeComponent();
        }

        //// Real-time time clock
        private void Form2_Load(object sender, EventArgs e)
        {
            timer1.Start();
            date.Text = DateTime.Now.ToLongDateString();
            time.Text = DateTime.Now.ToLongTimeString();
            week.Text = DateTime.Now.ToString("dddd"); // dddd: Represents the full name of the day of the week (e.g., Monday, Tuesday, ..., Sunday). instaed of abbreviation.
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            time.Text = DateTime.Now.ToLongTimeString();
            timer1.Start();
        }

        //// Simple form display format initiation
        private Form activeForm = null;
        private void openChildForm(Form childForm)
        {
            // Avoid duplicated form opening
            if (activeForm != null)
                activeForm.Close();
            activeForm = childForm;
            childForm.TopLevel = false;
            childForm.FormBorderStyle = FormBorderStyle.None;
            childForm.Dock = DockStyle.Fill;
            panel2.Controls.Add(childForm);
            panel2.Tag = childForm;
            childForm.BringToFront();
            childForm.Show();
        }

        //// Data paint on panel
        private void panel2_Paint(object sender, PaintEventArgs e)
        {
            // Here is to see the orders
            string query = "SELECT o.ORDER_Id, o.CUSTOMER_Id, c.CUSTOMER_PhoneNo, o.DELIVERY_Address, o.DELIVERY_Time, c.CUSTOMER_Name, od.Order_type, od.Order_weight, o.Gas_Quantity, o.COMPANY_Id, ca.Gas_Volume " +
                            "FROM `gas_order` o " +
                            "JOIN `customer` c ON o.CUSTOMER_Id = c.CUSTOMER_Id " +
                            "JOIN `gas_order_detail` od ON o.ORDER_Id = od.Order_ID " +
                            "JOIN `customer_accumulation` ca ON o.CUSTOMER_Id = ca.Customer_Id " +
                            $"WHERE o.COMPANY_Id = {GlobalVariables.CompanyId}";

            using (MySqlConnection connection = new MySqlConnection(connectionString))
            {
                using (MySqlDataAdapter adapter = new MySqlDataAdapter(query, connection))
                {
                    DataTable table = new DataTable();
                    adapter.Fill(table);
                    dataGridView1.DataSource = table;

                    // Columns rename
                    dataGridView1.Columns["ORDER_Id"].HeaderText = "訂單編號";
                    dataGridView1.Columns["CUSTOMER_Id"].HeaderText = "顧客編號";
                    dataGridView1.Columns["CUSTOMER_PhoneNo"].HeaderText = "顧客電話";
                    dataGridView1.Columns["DELIVERY_Address"].HeaderText = "送貨地址";
                    dataGridView1.Columns["DELIVERY_Time"].HeaderText = "送貨時間";
                    dataGridView1.Columns["CUSTOMER_Name"].HeaderText = "訂購人";
                    dataGridView1.Columns["Order_type"].HeaderText = "瓦斯桶種類";
                    dataGridView1.Columns["Order_weight"].HeaderText = "瓦斯規格";
                    dataGridView1.Columns["Gas_Quantity"].HeaderText = "數量";
                    dataGridView1.Columns["COMPANY_Id"].HeaderText = "選擇瓦斯行";
                    dataGridView1.Columns["Gas_Volume"].HeaderText = "顧客累積殘氣量";
                    // You can check each column's name here
                    /*foreach (DataColumn column in table.Columns)
                    {
                        Console.WriteLine(column.ColumnName);
                    }*/

                }
            }
            // Here is to load in the available workers


        }

        // Open 主頁面
        private void GasOrderPage_Click(object sender, EventArgs e)
        {
            if (activeForm != null)
                activeForm.Close();
        }

        // Open 客戶管理視窗
        private void CustomerManagePage_Click(object sender, EventArgs e)
        {
            openChildForm(new Customer());
        }

        // Open 殘氣累積視窗
        private void ResidualGasPage_Click(object sender, EventArgs e)
        {
            openChildForm(new residual_gas());
        }

        // Open 瓦斯桶管理視窗
        private void GasTankManagePage_Click(object sender, EventArgs e)
        {
            openChildForm(new GasTankManage());
        }

        // Open 員工資料視窗
        private void WorkerPage_Click(object sender, EventArgs e)
        {
            openChildForm(new Worker());
        }


        //// Auto-fill when certain row is selected.
        ////* 需求需確認: 要哪些資料? *////
        private void dataGridView1_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0)
            {
                DataGridViewRow selectedRow = dataGridView1.Rows[e.RowIndex];

                // Access the data in the selected row and autofill other fields in the form
                string orderId = selectedRow.Cells["Order_ID"].Value.ToString();
                string customerName = selectedRow.Cells["Customer_Name"].Value.ToString();
                string customerPhone = selectedRow.Cells["Customer_PhoneNo"].Value.ToString();
                string deliveryTime = selectedRow.Cells["Delivery_Time"].Value.ToString();
                string deliveryAddress = selectedRow.Cells["Delivery_Address"].Value.ToString();
                string orderWeight = selectedRow.Cells["Order_weight"].Value.ToString();
                string orderType = selectedRow.Cells["Order_type"].Value.ToString();
                string orderQuantity = selectedRow.Cells["Gas_Quantity"].Value.ToString();
                string gasVolume = selectedRow.Cells["Gas_Volume"].Value.ToString();

                // Autofill the other fields(Textbox) in the form
                OrderID.Text = orderId;
                CustomerName.Text = customerName;
                CustomerPhone.Text = customerPhone;
                DeliveryTime.Text = deliveryTime;
                DeliveryAddress.Text = deliveryAddress;
                GasType.Text = orderType;
                GasWeight.Text = orderWeight;
                GasQuantity.Text = orderQuantity;
                GasVolume.Text = gasVolume;
                TotalPrice.Text = "";
            }
        }

        //// 插入資料
        private void InsertButton_Click(object sender, EventArgs e)
        {

        }

        ////刪除資料視窗
        // dataGridView1: data table side.
        private void delete_Click(object sender, EventArgs e)
        {
            if (dataGridView1.SelectedRows.Count == 1)
            {
                DialogResult result = MessageBox.Show("确定删除此行資料？", "確認", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                if (result == DialogResult.Yes)
                {
                    string orderId = dataGridView1.SelectedRows[0].Cells[0].Value.ToString();

                    using (MySqlConnection connection = new MySqlConnection(connectionString))
                    {
                        connection.Open();

                        // Delete associated child rows first
                        string deleteChildQuery = "DELETE FROM `gas_order_detail` WHERE `Order_ID` = @Order_ID";
                        using (MySqlCommand deleteChildCommand = new MySqlCommand(deleteChildQuery, connection))
                        {
                            deleteChildCommand.Parameters.AddWithValue("@Order_ID", orderId);
                            deleteChildCommand.ExecuteNonQuery();
                        }

                        // Delete the parent row
                        string deleteParentQuery = "DELETE FROM `gas_order` WHERE `Order_ID` = @Order_ID";
                        using (MySqlCommand deleteParentCommand = new MySqlCommand(deleteParentQuery, connection))
                        {
                            deleteParentCommand.Parameters.AddWithValue("@Order_ID", orderId);
                            int rowsAffected = deleteParentCommand.ExecuteNonQuery();
                            if (rowsAffected > 0)
                            {
                                dataGridView1.Rows.RemoveAt(dataGridView1.SelectedRows[0].Index);
                                MessageBox.Show("删除成功", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                            }
                        }

                        connection.Close();
                    }
                }
            }

        }

        private void SearchButton_Click(object sender, EventArgs e)
        {
            string searchTerm = txt.Text.Trim();

            if (!string.IsNullOrEmpty(searchTerm))
            {
                DataTable dataTable = (DataTable)dataGridView1.DataSource;
                if (dataTable != null)
                {
                    string filterExpression = $"CUSTOMER_Name LIKE '%{searchTerm}%' OR CONVERT(ORDER_Id, 'System.String') LIKE '%{searchTerm}%' OR Customer_PhoneNo LIKE '%{searchTerm}%'";
                    dataTable.DefaultView.RowFilter = filterExpression;
                }
            }
            else
            {
                // Clear the filter if the search term is empty
                DataTable dataTable = (DataTable)dataGridView1.DataSource;
                dataTable.DefaultView.RowFilter = string.Empty;
            }
        }
        // 顯示客戶資料
        public class CustomerData
        {
            public string CustomerId { get; set; }
            public string CustomerName { get; set; }
            public string CustomerPhone { get; set; }
            public string CustomerSex { get; set; }
            public string FamilyMemberId { get; set; }
            public string CustomerEmail { get; set; }
        }
        private void CustomerInformation_Click(object sender, EventArgs e)
        {
            //開啟基本用戶資料頁面
            //編輯修改某筆資料
            if (dataGridView1.SelectedRows.Count > 0)
            {
                string id = dataGridView1.SelectedRows[0].Cells["ORDER_Id"].Value.ToString();

                // Pass the selected ID to the customer_form for editing
                customer_form f1 = new customer_form(id);
                if (f1.ShowDialog() == DialogResult.OK)
                {
                }
            }
            else
            {
                MessageBox.Show("請選擇要編輯的資料行", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }


        private void DeliveryMan_Click(object sender, EventArgs e)
        {
            using (MySqlConnection connection = new MySqlConnection(connectionString))
            {
                connection.Open();

                string query = $"SELECT WORKER_Name FROM worker WHERE WORKER_Company_Id = {GlobalVariables.CompanyId}";

                using (MySqlCommand command = new MySqlCommand(query, connection))
                {
                    using (MySqlDataReader reader = command.ExecuteReader())
                    {
                        // Clear existing items in the ComboBox
                        DeliveryMan.Items.Clear();

                        // Loop through the result set and add worker names to the ComboBox
                        while (reader.Read())
                        {
                            string workerName = reader.GetString("WORKER_Name");

                            // Add worker name to the ComboBox
                            DeliveryMan.Items.Add(workerName);
                        }
                    }
                }
            }
        }

        // 歷史訂單
        private void dataGridView1_CellDoubleClick(object sender, DataGridViewCellEventArgs e)
        {
            using (MySqlConnection connection = new MySqlConnection(connectionString))
            {
                connection.Open();

                string query = @"SELECT *
                                FROM gas_order_history
                                WHERE CUSTOMER_Id = (
                                    SELECT CUSTOMER_Id
                                    FROM gas_order_history
                                    WHERE ORDER_Id = @order_id
                                )
                                ORDER BY DELIVERY_Time DESC;
                                ";

                DataGridViewRow selectedRow = dataGridView1.Rows[e.RowIndex];
                string order_id = selectedRow.Cells["Order_Id"].Value.ToString();
                using (MySqlCommand command = new MySqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@order_id", order_id);

                    using (MySqlDataAdapter adapter = new MySqlDataAdapter(command))
                    {
                        DataTable historyOrders = new DataTable();
                        adapter.Fill(historyOrders);

                        // Create an instance of the HistoryOrder form
                        HistoryOrder historyOrderForm = new HistoryOrder();

                        // Pass the historyOrders DataTable to the form
                        historyOrderForm.SetData(historyOrders);

                        // Display the form
                        historyOrderForm.ShowDialog();

                    }
                }
            }
        }

        private void AutoFillButton_Click(object sender, EventArgs e)
        {   // Fill out customer address, customer name,
            // delivery time use now(),
            // order_id ??
            // order_type, order_quantity, order_weight use newest historical data.
            string searchTerm = CustomerPhone.Text;

            using (MySqlConnection connection = new MySqlConnection(connectionString))
            {
                connection.Open();

                // 在 gas_order_history裡面的電話是 string, 要改成 varchar(50)才能用 
                string query = @"SELECT g.CUSTOMER_Id, g.DELIVERY_Time, g.DELIVERY_Address, g.DELIVERY_Phone, g.Gas_Quantity, g.Order_Time, g.Gas_Detail_Id, g.Order_Quantity, g.Order_type, g.Order_weight, g.Exchange, g.Completion_Date, c.CUSTOMER_Name, ca.Gas_Volume
                                FROM gas_order_history g
                                JOIN customer c ON g.CUSTOMER_Id = c.CUSTOMER_Id
                                JOIN customer_accumulation ca ON g.CUSTOMER_Id = ca.CUSTOMER_Id
                                WHERE g.DELIVERY_Phone LIKE CONCAT('%', @searchTerm, '%')
                                ORDER BY g.DELIVERY_Time DESC
                                LIMIT 1
                                ";

                using (MySqlCommand command = new MySqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@searchTerm", searchTerm);

                    using (MySqlDataAdapter adapter = new MySqlDataAdapter(command))
                    {
                        DataTable dataTable = new DataTable();
                        adapter.Fill(dataTable);

                        if (dataTable.Rows.Count > 0)
                        {
                            DataRow row = dataTable.Rows[0];

                            string customerName = row["CUSTOMER_Name"].ToString();
                            //DateTime deliveryTime = (DateTime)row["DELIVERY_Time"];
                            string deliveryAddress = row["DELIVERY_Address"].ToString();
                            string deliveryPhone = row["DELIVERY_Phone"].ToString();
                            string gasType = row["Order_type"].ToString();
                            string gasWeight = row["Order_weight"].ToString();
                            string gasQuantity = row["Order_Quantity"].ToString();
                            string gasVolume = row["Gas_Volume"].ToString();

                            OrderID.Text = "";
                            CustomerName.Text = customerName;
                            DeliveryTime.Text = DateTime.Now.ToLongTimeString();
                            DeliveryAddress.Text = deliveryAddress;
                            CustomerPhone.Text = deliveryPhone;
                            GasType.Text = gasType;
                            GasWeight.Text = gasWeight;
                            GasQuantity.Text = gasQuantity;
                            GasVolume.Text = gasVolume;


                        }
                        else
                        {

                        }
                    }
                }
            }
        }

        private void AddButton_Click(object sender, EventArgs e)
        {
            string searchTerm = CustomerPhone.Text;
            using (MySqlConnection connection = new MySqlConnection(connectionString))
            {
                connection.Open();

                // Step 1: Fetch relevant data from gas_order_history
                string fetchQuery = @"SELECT *
                              FROM gas_order_history
                              WHERE DELIVERY_Phone = @searchTerm
                              ORDER BY DELIVERY_Time DESC
                              LIMIT 1";

                using (MySqlCommand fetchCommand = new MySqlCommand(fetchQuery, connection))
                {
                    fetchCommand.Parameters.AddWithValue("@searchTerm", searchTerm);
                    using (MySqlDataReader reader = fetchCommand.ExecuteReader())
                    {
                        if (reader.Read())
                        {

                            
                            // Step 2: Extract data from the reader
                            int customerId = reader.GetInt32("CUSTOMER_Id");
                            int companyId = reader.GetInt32("COMPANY_Id");
                            string deliveryCondition = reader.GetString("DELIVERY_Condition");
                            string deliveryAddress = reader.GetString("DELIVERY_Address");
                            string deliveryPhone = reader.GetString("DELIVERY_Phone");

                            //下面桶資訊先不要從歷史資訊抓然後插入，顧客可能想改
                            //int gasQuantity = reader.GetInt32("Gas_Quantity");
                            string gasQuantity = GasQuantity.Text;
                            //int orderQuantity = reader.GetInt32("Order_Quantity");
                            string orderQuantity = GasQuantity.Text;
                            //string orderType = reader.GetString("Order_type");
                            string orderType = GasType.Text;                        
                            //int orderWeight = reader.GetInt32("Order_weight");
                            string orderWeight = GasWeight.Text;

                            string exchange = reader.GetString("Exchange");
                            reader.Close();

                            // Step 3: Construct and execute the insert statements
                            string insertOrderQuery = @"INSERT INTO gas_order
                                                (CUSTOMER_Id, COMPANY_Id, DELIVERY_Time, DELIVERY_Condition, Exchange, DELIVERY_Address, DELIVERY_Phone, Gas_Quantity, Order_Time, Expect_Time, Delivery_Method)
                                                VALUES
                                                (@customerId, @companyId, NOW(), @deliveryCondition, @exchange, @deliveryAddress, @deliveryPhone, @gasQuantity, NOW(), NOW(), @deliveryMethod)";

                            using (MySqlCommand insertOrderCommand = new MySqlCommand(insertOrderQuery, connection))
                            {
                                // Bind parameters for the insert statement
                                insertOrderCommand.Parameters.AddWithValue("@customerId", customerId);
                                insertOrderCommand.Parameters.AddWithValue("@companyId", companyId);
                                insertOrderCommand.Parameters.AddWithValue("@deliveryCondition", deliveryCondition);
                                insertOrderCommand.Parameters.AddWithValue("@deliveryAddress", deliveryAddress);
                                insertOrderCommand.Parameters.AddWithValue("@deliveryPhone", deliveryPhone);
                                insertOrderCommand.Parameters.AddWithValue("@gasQuantity", gasQuantity);
                                insertOrderCommand.Parameters.AddWithValue("@exchange", exchange);
                                insertOrderCommand.Parameters.AddWithValue("@deliveryMethod", 1); // Assuming deliveryMethod is always 1

                                // Execute the insert statement for gas_order
                                int rowsAffected = insertOrderCommand.ExecuteNonQuery();

                                if (rowsAffected > 0)
                                {
                                    // The row was successfully inserted
                                    MessageBox.Show("成功加入訂單", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);

                                    // Get the generated ORDER_Id value
                                    long orderId = insertOrderCommand.LastInsertedId;

                                    // Use the orderId variable to insert into gas_order_detail table
                                    string insertOrderDetailQuery = @"INSERT INTO gas_order_detail
                                                              (Order_ID, Order_Quantity, Order_type, Order_weight, exchange)
                                                              VALUES
                                                              (@orderId, @orderQuantity, @orderType, @orderWeight, @exchange)";

                                    using (MySqlCommand insertOrderDetailCommand = new MySqlCommand(insertOrderDetailQuery, connection))
                                    {
                                        // Bind parameters for the insert statement
                                        insertOrderDetailCommand.Parameters.AddWithValue("@orderId", orderId);
                                        insertOrderDetailCommand.Parameters.AddWithValue("@orderQuantity", orderQuantity);
                                        insertOrderDetailCommand.Parameters.AddWithValue("@orderType", orderType);
                                        insertOrderDetailCommand.Parameters.AddWithValue("@orderWeight", orderWeight);
                                        insertOrderDetailCommand.Parameters.AddWithValue("@exchange", exchange);

                                        // Execute the insert statement for gas_order_detail
                                        insertOrderDetailCommand.ExecuteNonQuery();
                                    }
                                    RefreshButton_Click(sender, e);
                                }
                                else
                                {
                                    // Handle case when the row insertion failed
                                    MessageBox.Show("訂單新增失敗", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                                }
                            }
                        }
                        else
                        {
                            // Handle case when no matching data is found
                        }
                    }
                }
            }
        }



        private void RefreshButton_Click(object sender, EventArgs e)
        {
            //刷新dataGridView顯示的資料表
            string query = "SELECT o.ORDER_Id, c.CUSTOMER_PhoneNo, o.DELIVERY_Address, o.DELIVERY_Time, c.CUSTOMER_Name, od.Order_type, od.Order_weight,o.Gas_Quantity, o.COMPANY_Id FROM `gas_order` o JOIN`customer` c ON o.CUSTOMER_Id = c.CUSTOMER_Id JOIN `gas_order_detail` od ON o.ORDER_Id = od.Order_ID;";

            using (MySqlConnection connection = new MySqlConnection(connectionString))
            {
                using (MySqlDataAdapter adapter = new MySqlDataAdapter(query, connection))
                {
                    DataTable table = new DataTable();
                    adapter.Fill(table);

                    dataGridView1.DataSource = table;
                    dataGridView1.Columns["ORDER_Id"].HeaderText = "訂單編號";
                    dataGridView1.Columns["CUSTOMER_PhoneNo"].HeaderText = "顧客電話";
                    dataGridView1.Columns["DELIVERY_Address"].HeaderText = "送貨地址";
                    dataGridView1.Columns["DELIVERY_Time"].HeaderText = "送貨時間";
                    dataGridView1.Columns["CUSTOMER_Name"].HeaderText = "訂購人";
                    dataGridView1.Columns["Order_type"].HeaderText = "瓦斯桶種類";
                    dataGridView1.Columns["Order_weight"].HeaderText = "瓦斯規格";
                    dataGridView1.Columns["Gas_Quantity"].HeaderText = "數量";
                    dataGridView1.Columns["COMPANY_Id"].HeaderText = "選擇瓦斯行";
                }
            }

            OrderID.Text = "";
            CustomerName.Text = "";
            CustomerPhone.Text = "";
            DeliveryTime.Text = "";
            DeliveryAddress.Text = "";
            GasType.Text = "";
            GasWeight.Text = "";
            GasQuantity.Text = "";
            GasVolume.Text = "";
        }

        private void ConfirmButton_Click(object sender, EventArgs e)
        {
            // Check if a row is selected in the dataGridView1
            if (dataGridView1.SelectedRows.Count > 0)
            {
                // Retrieve the customerId and orderId from the selected row
                int customerId = Convert.ToInt32(dataGridView1.SelectedRows[0].Cells["CUSTOMER_Id"].Value);
                int orderId = Convert.ToInt32(dataGridView1.SelectedRows[0].Cells["ORDER_Id"].Value);

                // Get the selected delivery man's name from the DeliveryMan ComboBox
                string deliveryManName = DeliveryMan.SelectedItem?.ToString();

                if (!string.IsNullOrEmpty(deliveryManName))
                {
                    // Query the database to get the workerId based on the selected delivery man's name
                    string query = $"SELECT WORKER_Id FROM worker WHERE WORKER_Name = @WorkerName AND WORKER_Company_Id = {GlobalVariables.CompanyId}";

                    using (MySqlConnection connection = new MySqlConnection(connectionString))
                    {
                        using (MySqlCommand command = new MySqlCommand(query, connection))
                        {
                            command.Parameters.AddWithValue("@WorkerName", deliveryManName);
                            connection.Open();

                            int workerId = Convert.ToInt32(command.ExecuteScalar());

                            // Insert a new record into the assign table
                            string insertQuery = "INSERT INTO assign (CUSTOMER_Id, WORKER_Id, ORDER_Id) VALUES (@CustomerId, @WorkerId, @OrderId)";

                            using (MySqlCommand insertCommand = new MySqlCommand(insertQuery, connection))
                            {
                                insertCommand.Parameters.AddWithValue("@CustomerId", customerId);
                                insertCommand.Parameters.AddWithValue("@WorkerId", workerId);
                                insertCommand.Parameters.AddWithValue("@OrderId", orderId);

                                if (insertCommand.ExecuteNonQuery() == 1)
                                {
                                    MessageBox.Show("Order assigned successfully!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                                    // Additional actions or notifications can be added here if needed
                                }
                                else
                                {
                                    MessageBox.Show("Failed to assign order!", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                                }
                            }

                            connection.Close();
                        }
                    }
                }
                else
                {
                    MessageBox.Show("Please select a delivery man!", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            else
            {
                MessageBox.Show("Please select a row in the dataGridView1!", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}