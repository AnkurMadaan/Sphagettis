using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.IO;
using System.Net.Mail;

public class CustomerService
{
    private readonly string _connectionString = "Data Source=(local);Initial Catalog=MyDatabase;Integrated Security=True;";
    private const decimal DISCOUNT_THRESHOLD = 1000.00m;
    private const string VIP_CUSTOMER_STATUS = "VIP";
    private const string VIP_PROMO_EMAIL = "VIPPromo.html";

    // This method does way too much. It handles customer data, business logic, and communication.
    public bool ProcessCustomerOrder(int customerId, List<Product> products, decimal totalAmount, string paymentMethod)
    {
        SqlConnection connection = null;
        try
        {
            // Connect to database to check customer status
            connection = new SqlConnection(_connectionString);
            connection.Open();

            string customerStatus = "Regular";
            SqlCommand statusCommand = new SqlCommand($"SELECT CustomerStatus FROM Customers WHERE CustomerId = {customerId}", connection);
            SqlDataReader reader = statusCommand.ExecuteReader();
            if (reader.Read())
            {
                customerStatus = reader["CustomerStatus"].ToString();
            }
            reader.Close();

            // Apply discounts based on arbitrary logic
            decimal finalAmount = totalAmount;
            if (customerStatus == VIP_CUSTOMER_STATUS || totalAmount > DISCOUNT_THRESHOLD)
            {
                finalAmount *= 0.90m; // 10% discount
            }

            // Process payment based on method
            if (paymentMethod == "CreditCard")
            {
                // Logic for CreditCard processing (hardcoded simulation)
                Console.WriteLine("Processing Credit Card payment...");
                // Some hardcoded, complex logic that might be copied/pasted elsewhere
                if (finalAmount > 5000)
                {
                    Console.WriteLine("Escalating large payment for manual review.");
                }
                // ... Imagine more payment processing code here
            }
            else if (paymentMethod == "PayPal")
            {
                // Logic for PayPal processing
                Console.WriteLine("Processing PayPal payment...");
                // ... Imagine more payment processing code here
            }
            else
            {
                Console.WriteLine($"Unsupported payment method: {paymentMethod}");
                return false;
            }

            // Update customer record in the database
            SqlCommand updateCommand = new SqlCommand($"UPDATE Customers SET LastOrderDate = GETDATE(), LastOrderAmount = {finalAmount} WHERE CustomerId = {customerId}", connection);
            updateCommand.ExecuteNonQuery();

            // Send confirmation email to customer
            string customerEmail = "customer@example.com"; // Hardcoded email for simplicity, but a common issue.
            string emailBody;
            try
            {
                // Read email template from file
                emailBody = File.ReadAllText("OrderConfirmation.html");
                emailBody = emailBody.Replace("{customerName}", "John Doe")
                                     .Replace("{orderAmount}", finalAmount.ToString("C"));
            }
            catch (Exception ex)
            {
                emailBody = $"Your order for {finalAmount:C} has been processed.";
                // Log and ignore file read error
                Console.WriteLine("Error reading email template. Using simple email body.");
            }

            // Send special promo email for VIPs
            if (customerStatus == VIP_CUSTOMER_STATUS)
            {
                // Copied/pasted email sending code
                try
                {
                    string vipEmailBody = File.ReadAllText(VIP_PROMO_EMAIL);
                    SmtpClient smtpClient = new SmtpClient("smtp.example.com");
                    MailMessage mailMessage = new MailMessage("noreply@example.com", customerEmail, "VIP Promotion", vipEmailBody);
                    // smtpClient.Send(mailMessage);
                }
                catch (Exception ex)
                {
                    // Log but ignore error. What if the email is critical?
                    Console.WriteLine("Failed to send VIP promo email.");
                }
            }

            SmtpClient smtpClient = new SmtpClient("smtp.example.com");
            MailMessage mailMessage = new MailMessage("noreply@example.com", customerEmail, "Order Confirmation", emailBody);
            // smtpClient.Send(mailMessage);

            return true;
        }
        catch (SqlException sqlEx)
        {
            // Catch-all SQL exception. Hides specific issues.
            Console.WriteLine($"Database error: {sqlEx.Message}");
            return false;
        }
        catch (Exception ex)
        {
            // Catch-all for everything else. Very bad.
            Console.WriteLine($"An unexpected error occurred: {ex.Message}");
            return false;
        }
        finally
        {
            if (connection != null && connection.State == System.Data.ConnectionState.Open)
            {
                connection.Close();
            }
        }
    }

    public class Product
    {
        public int ProductId { get; set; }
        public string ProductName { get; set; }
    }
}
