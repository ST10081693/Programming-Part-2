using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.UI.WebControls;

/*
Name: Rui Manuel
Student number: ST10081693
Assignment: PROG Part 2
 */

/*The code used in this project was based on a combination of two projects from the 2nd year. 
The PROG Part 2 project where we were asked to do a application that would keep track of modules(Using SQL databases.). And also, The PROG POE where we were asked to replacate
what with had done in part 2 but that time, we had to create a web application.
Some of the styles and code ideias where also provided by ChatGPT.*/

//Note: For this project, I tried to implementing entity framework, but that brought some errors, and so, I decided to use only a SQL database.

namespace Farm_Central_prototype.Controllers
{

    public class HomeController : Controller
    {

        private string cs = ConfigurationManager.ConnectionStrings["Mycs"].ConnectionString;


        //--------------------------This is the Action for the homepage, it returns the view homepage and sets the session to null.--------------------------//


        public ActionResult Index()
        {
            Session["Username1"] = null;
            return View();

        }


        //--------------------------------------------------------------------------------------------------------------------------------------------------//



        //---------------------------------------------Action used to redirect the employee to the login page.----------------------------------------------//


        public ActionResult Login()
        {
            ViewBag.Title = "Login";

            return View();
        }


        //--------------------------------------------------------------------------------------------------------------------------------------------------//



        //---------------------------------------------Action used to redirect the farmer to the login page.-----------------------------------------------//


        public ActionResult Login1()
        {

            return View();
        }


        //--------------------------------------------------------------------------------------------------------------------------------------------------//



        //----------------------------------------------Method used to check if the Employee is in the database---------------------------------------------//

        public bool validatecred()
        {

            using (var connection = new SqlConnection(@cs))
            {
                string username = Request.Form["usern"];
                string password = Request.Form["pass"];
                connection.Open();
                string query = "SELECT  count (*) from ADM where Username = '" + username + "' And Password1 = '" + password + "' ";
                using (var command = new SqlCommand(query, connection))
                {
                    int count = (int)command.ExecuteScalar();

                    return count > 0;
                }
            }
        }


        //--------------------------------------------------------------------------------------------------------------------------------------------------//



        //----------------------------------------------Method used to check if the farmer is in the database----------------------------------------------//
        public bool Evalidatecred()
        {
            using (var connection = new SqlConnection(@cs))
            {
                string username = Request.Form["Eusern"];
                string password = Request.Form["Epass"];
                connection.Open();
                string query = "SELECT  count (*) from Farmers where Username = '" + username + "' And Passcode = '" + password + "' ";
                using (var command = new SqlCommand(query, connection))
                {
                    int count = (int)command.ExecuteScalar();

                    Session["Username"] = username;
                    return count > 0;
                }
            }
        }

        //--------------------------------------------------------------------------------------------------------------------------------------------------//




        //-----------------------------Action used to take the employee to the login page if the employee is in the database--------------------------------//

        //If the employee is in the database, he will be taken to the the employee homepage, If not, he will be taken to the employee Login Page.
        public ActionResult ADMHome()
        {
            bool valid = validatecred();

            if (valid)
            {
                Session["Username1"] = "Employee";
                return View();
            }

            ViewBag.ErrorMessage = "Invalid username or password";
            return View("Login");
        }


        //--------------------------------------------------------------------------------------------------------------------------------------------------//



        //-----------------------------Action used to take the farmer to the login in page if the farmer is in the database--------------------------------//

        //If the farmer is in the database, he will be taken to the the farmer homepage, If not, he will be taken to the farmer Login Page.
        public ActionResult EHome()
        {
            bool valid = Evalidatecred();

            if (valid)
            {
                Session["Username1"] = "Farmer";
                return View();
            }

            ViewBag.ErrorMessage = "Invalid username or password";
            return View("Login1");
        }


        //--------------------------------------------------------------------------------------------------------------------------------------------------//



        //---------------------------------------------------Action Used to display the add farmer window---------------------------------------------------//
        public ActionResult AddFarmer()
        {
            ViewBag.ERole = "Employee";
            return View();
        }

        //--------------------------------------------------------------------------------------------------------------------------------------------------//



        //-------------------------------------------------------Action to add a farmer to the database----------------------------------------------------//

        public ActionResult CreateF()
        {
            try {
                using (var connection = new SqlConnection(@cs))
                {
                    string Name = Request.Form["fname"];
                    string Surname = Request.Form["fsurname"];
                    string Username = Request.Form["fuser"];
                    string Password = Request.Form["fpass"];
                    connection.Open();
                    string query = "Insert  into Farmers values ('" + Name + "', '" + Surname + "', '" + Username + "', '" + Password + "')";
                    using (var command = new SqlCommand(query, connection))
                    {
                        command.ExecuteNonQuery();
                    }

                    return View("AddFarmer");
                }
                }
            catch (Exception ex)
            {
                ViewBag.ErrorMessage1 = "User already exists";
                return View("AddFarmer");
            }
        }

        //--------------------------------------------------------------------------------------------------------------------------------------------------//



        //------------------------------------------------------Action used to display the Add product View-------------------------------------------------//
        public ActionResult AddProduct()
        {
            ViewBag.ERole = "Farmer";
            return View();
        }

        //------------------------------------------------------Class used to create a Object of Product----------------------------------------------------//

        public class Product
        {
            public string Name { get; set; }
            public string User { get; set; }
            public string Amount { get; set; }
            public string Date { get; set; }
        }

        //--------------------------------------------------------------------------------------------------------------------------------------------------//



        //----------------------------------------------Action used to Load the Dropdown list and to filter the data----------------------------------------//
        public ActionResult ViewProducts(string selectedName, DateTime? startDate, DateTime? endDate)
        {
            ViewBag.ERole = "Employee";
            List<string> names = new List<string>();
            List<Product> products = new List<Product>();

            //Code used to load the dropdown list withthe names of the farmer.
            using (var connection = new SqlConnection(@cs))
            {
                connection.Open();

                string sqlQuery = "SELECT DISTINCT Username FROM Products";
                using (SqlCommand command = new SqlCommand(sqlQuery, connection))
                {
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            string productName = reader.GetString(0);
                            names.Add(productName);
                        }
                    }
                }
                //-------------------------------------------------------------

                //Code used to Filter the list according to the name, the start and end date the user selects.
                string sqlQuery2 = "SELECT ProductName, Username, Amount, PDate FROM Products WHERE Username = @Name AND PDate BETWEEN @StartDate AND @EndDate";
                using (SqlCommand command = new SqlCommand(sqlQuery2, connection))
                {
                    command.Parameters.AddWithValue("@Name", string.IsNullOrEmpty(selectedName) ? (object)DBNull.Value : selectedName);
                    command.Parameters.AddWithValue("@StartDate", startDate.HasValue ? startDate.Value : (object)DBNull.Value);
                    command.Parameters.AddWithValue("@EndDate", endDate.HasValue ? endDate.Value : (object)DBNull.Value);


                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            string productName = reader.GetString(0);
                            string user = reader.GetString(1);
                            string amount = reader.GetInt32(2).ToString();
                            DateTime date = reader.GetDateTime(3);
                            string formattedDate = date.ToShortDateString();

                            Product product = new Product
                            {
                                Name = productName,
                                User = user,
                                Amount = amount,
                                Date = formattedDate
                            };

                            products.Add(product);
                        }
                    }
                }
                //---------------------------------------------------------------
            }
            
            ViewBag.Names = names;
            return View(products);
        }

        //--------------------------------------------------------------------------------------------------------------------------------------------------//



        //----------------------------------------------------------Method used to add a product to the database--------------------------------------------//

        public ActionResult CreateP()
        {
            try
            {
                using (var connection = new SqlConnection(@cs))
                {
                    string Name = Request.Form["Pname"];
                    string Amount = Request.Form["Pamount"];
                    string Username = (string)Session["Username"];
                    string Date = DateTime.Today.ToString();
                    connection.Open();
                    string query = "Insert  into Products (ProductName, Amount, Username, PDate) values ('" + Name + "', '" + Amount + "', '" + Username + "', '" + Date + "')";
                    using (var command = new SqlCommand(query, connection))
                    {
                        command.ExecuteNonQuery();
                    }
                    connection.Close();

                    return View("AddProduct");
                }
            }
            catch (Exception ex1)
            {
                ViewBag.ErrorMessage2 = "The amount must be a number";
                return View("AddProduct");
            }
        }

        //--------------------------------------------------------------------------------------------------------------------------------------------------//
    }
}