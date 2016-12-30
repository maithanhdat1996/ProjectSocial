using Microsoft.AspNet.SignalR;
using SocialFashion.Model.Models;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Web;
using Microsoft.AspNet.Identity;


namespace SocialFashion.Web
{
    public class NotificationComponent 
    {
        
        public  void RegisterNotification()
        {

            var currentUserId = string.Empty;
            if (System.Web.HttpContext.Current != null)
            {


                currentUserId = System.Web.HttpContext.Current.User.Identity.GetUserId();
            }
            using (var connection = new SqlConnection(ConfigurationManager.ConnectionStrings["DefaultConnection"].ConnectionString))
            {
                connection.Open();
                using (SqlCommand command = new SqlCommand(@"SELECT [NotificationId], [OwnerId], [RecieverId], [Type] FROM Notifications WHERE [RecieverId] = @currentUserId AND IsRead = 0", connection))
                {
                    // Make sure the command object does not already have
                    // a notification object associated with it.
                    command.Notification = null;
                    SqlParameter Param = command.Parameters.AddWithValue("@currentUserId", currentUserId);


                    if (currentUserId == null)
                    {
                        Param.Value = DBNull.Value;
                    }
                    SqlDependency dependency = new SqlDependency(command);
                    dependency.OnChange += new OnChangeEventHandler(SqlDep_OnChange);

                    if (connection.State == ConnectionState.Closed)
                        connection.Open();

                    using (var reader = command.ExecuteReader())
                    {

                    }
                }

               
            }
        }

        

        private void SqlDep_OnChange(object sender, SqlNotificationEventArgs e)
        {
            
                SqlDependency sqlDep = sender as SqlDependency;
                sqlDep.OnChange -= SqlDep_OnChange;
                NotificationHub.Show();
                RegisterNotification();

            
        }

        
    }
}