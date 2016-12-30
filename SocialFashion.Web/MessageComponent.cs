using Microsoft.AspNet.Identity;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Web;


namespace SocialFashion.Web
{
    public class MessageComponent 
    {
        public void RegisterNotificationMsg()
        {
            var currentUserMail = string.Empty;
            if (System.Web.HttpContext.Current != null)
            {

                 currentUserMail = System.Web.HttpContext.Current.User.Identity.GetUserName();
            }
            using (var connection = new SqlConnection(ConfigurationManager.ConnectionStrings["DefaultConnection"].ConnectionString))
                {


                    using (SqlCommand command = new SqlCommand(@"SELECT b.[MessageId],b.[SenderEmail],b.[RecieverEmail],b.[Date],b.[Content],b.[IsRead]
                            FROM [Messages] AS b
                            WHERE b.[RecieverEmail] = @currentUserMail   AND b.[IsRead] = 0", connection))
                    {
                        // Make sure the command object does not already have
                        // a notification object associated with it.
                        command.Notification = null;
                        SqlParameter Param = command.Parameters.AddWithValue("@currentUserMail", currentUserMail);
                        if (currentUserMail == null)
                        {
                            Param.Value = DBNull.Value;
                        }
                        SqlDependency dependency = new SqlDependency(command);
                        dependency.OnChange += new OnChangeEventHandler(SqlDep_OnChangeMsg);

                        if (connection.State == ConnectionState.Closed)
                            connection.Open();

                        using (var reader = command.ExecuteReader())
                        {

                        }
                    }
                }

            
        }

        private void SqlDep_OnChangeMsg(object sender, SqlNotificationEventArgs e)
        {
            SqlDependency sqlDep = sender as SqlDependency;
            sqlDep.OnChange -= SqlDep_OnChangeMsg;
            MessageHub.ShowMsg();
            RegisterNotificationMsg();
        }
    }
}