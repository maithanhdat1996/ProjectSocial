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
    public class FanComponent 
    {
        public void RegisterFan()
        {
            var currentUserId = string.Empty;
            if (System.Web.HttpContext.Current != null)
            {


                 currentUserId = System.Web.HttpContext.Current.User.Identity.GetUserId();
            }
            using (var connection = new SqlConnection(ConfigurationManager.ConnectionStrings["DefaultConnection"].ConnectionString))
                {


                    using (SqlCommand command = new SqlCommand(@"SELECT b.[FanId],b.[SenderId],b.[RequestId],b.[FriendShipDate],b.[Message],b.[Status]
                            FROM [Fan] AS b
                            WHERE b.[RequestId] = @currentUserId AND b.[Status] = 0", connection))
                    {
                        // Make sure the command object does not already have
                        // a notification object associated with it.


                        SqlParameter Param = command.Parameters.AddWithValue("@currentUserId", currentUserId);


                        if (currentUserId == null)
                        {
                            Param.Value = DBNull.Value;
                        }
                        SqlDependency dependency = new SqlDependency(command);
                        dependency.OnChange += new OnChangeEventHandler(SqlDep_OnChangeFan);

                        if (connection.State == ConnectionState.Closed)
                            connection.Open();

                        using (var reader = command.ExecuteReader())
                        {

                        }
                    }
                }
            
        }

        private void SqlDep_OnChangeFan(object sender, SqlNotificationEventArgs e)
        {

            SqlDependency sqlDep = sender as SqlDependency;
            sqlDep.OnChange -= SqlDep_OnChangeFan;
            FanHub.ShowFan();
            RegisterFan();
        }
    }
}