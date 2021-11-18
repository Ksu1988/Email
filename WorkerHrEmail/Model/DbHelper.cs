﻿using SCCBA.DB;
using SCCBA.Extensions;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Data.SqlClient;
using System.Data.SqlTypes;
using System.Diagnostics;
using System.Globalization;
using System.Runtime.CompilerServices;
using System.Text;

namespace WorkerHrEmail.Model
{
    public enum ReasonsForSelect { wellcome, oneYear }
    public static class DbHelper
    {
        private static string sqlForWellcomeEmail = @"
            select * from
            (
	            SELECT [main].[EmployeeID]
		              ,MIN([DateStart]) firstdate
		              ,[Core].[dbo].[User].[Mail]
		              ,[Core].[dbo].[User].[FirstNameRU]
	            FROM [Core].[dbo].[MovementHistory] main   
	               inner join [Core].[dbo].[User] on ([Core].[dbo].[User].[EmployeeID] = [main].[EmployeeID])
	            where 
		            Mail is not null 
		            and DateFinish is null	
                    and StatusID = 1
	            group by [main].[EmployeeID], [Core].[dbo].[User].[Mail],[Core].[dbo].[User].[FirstNameRU]
            ) as main
            where firstdate >= GETDATE() - 2
                and Mail is not null
            order by 2 desc";

        private static string sqlForOneYearEmail = @"
            select * from
            (
	            SELECT [main].[EmployeeID]
		              ,MIN([DateStart]) firstdate
		              ,[Core].[dbo].[User].[Mail]
		              ,[Core].[dbo].[User].[FirstNameRU]
	            FROM [Core].[dbo].[MovementHistory] main   
	               inner join [Core].[dbo].[User] on ([Core].[dbo].[User].[EmployeeID] = [main].[EmployeeID])
	            where 
		            Mail is not null 
		            and DateFinish is null	
                    and StatusID = 1
	            group by [main].[EmployeeID], [Core].[dbo].[User].[Mail],[Core].[dbo].[User].[FirstNameRU]
            ) as main
            where firstdate >= GETDATE() - 370
                and firstdate <= GETDATE() - 360
            order by 2 desc";

        private static string sqlForReport = @"
            SELECT hist.*, 
	            DATEDIFF(day, WellcomeEmail, GETDATE()) diff1,
	            DATEDIFF(day, OneYearEmail, GETDATE()) diff2,
	            [Core].[dbo].[User].LastNameRU,
	            [Core].[dbo].[User].FirstNameRU,
	            [Core].[dbo].[User].MiddleNameRU,
	            [Core].[dbo].[User].Mail
              FROM [HR].[dbo].[UserReceivedEmail] hist
              inner join [Core].[dbo].[User] on ([Core].[dbo].[User].[EmployeeID] = [hist].[EmployeeID])
              where 
                 (hist.ReportWellcome is null and hist.WellcomeEmail is not null)
			  or (hist.ReportOneYear is null and hist.OneYearEmail is not null)
              order by WellcomeEmail, OneYearEmail";

        /// <summary>
        /// Select employees for wellcome letter
        /// </summary>
        /// <param name="db"></param>
        /// <returns></returns>
        public static IEnumerable<User> GetUsers(this MSSqlConnection db, ReasonsForSelect reason)
        {
            var rawItems = db.GetItems(reason == ReasonsForSelect.wellcome?  sqlForWellcomeEmail: sqlForOneYearEmail, 
                new DbParameter[] { });

            var res = new List<User>();
            foreach(var row in rawItems)
            {
                var u = new User()
                {
                    EmployeeId = row["EmployeeID"].ToInt32(),
                    FirstNameRU = row["FirstNameRU"].ToString(),
                    Mail = row["Mail"].ToString(),
                    FirstDate = MSSQL2DT(row["FirstDate"])
                };

                //var patt = "dd/MM/yyyy hh:mm:ss tt";
                //DateTime dt;
                //if( DateTime.TryParseExact(row["FirstDate"].ToString(), patt, null, DateTimeStyles.None, out dt) )
                //    u.FirstDate = dt;
                res.Add(u);
            }

            return res;
        }

        public static List<History> GetHistory(this MSSqlConnection db)
        {
            var rawItems = db.GetItems(sqlForReport, new DbParameter[] { });

            var res = new List<History>();
            foreach (var row in rawItems)
            {
                var u = new History()
                {
                    EmployeeId = row["EmployeeID"].ToInt32(),

                    Diff1 = row["Diff1"].ToInt32(),
                    Diff2 = row["Diff2"].ToInt32(),

                    LastNameRu = row["LastNameRU"].ToString(),
                    FirstNameRu = row["FirstNameRU"].ToString(),
                    MiddleNameRu = row["MiddleNameRU"].ToString(),
                    Mail = row["Mail"].ToString(),

                    WellcomeEmail = MSSQL2DT(row["WellcomeEmail"]),
                    OneYearEmail = MSSQL2DT(row["OneYearEmail"]),
                    ReportWellcome = MSSQL2DT(row["ReportWellcome"]),
                    ReportOneYear = MSSQL2DT(row["ReportOneYear"]),
                };

                res.Add(u);
            }

            return res;
        }

        private static DateTime? MSSQL2DT(object obj)
        {
            if (obj == null) 
                return null;
            if (obj == DBNull.Value)
                return null;
            return Convert.ToDateTime(obj);
        }

        /// <summary>
        /// will return true, if employee was received welcome email
        /// </summary>
        /// <param name="db"></param>
        /// <param name="user"></param>
        /// <returns></returns>
        public static bool WasWellcomeEmail(this MSSqlConnection db, User user)
        {
            var userReceived = db.GetItem($"SELECT * FROM [HR].[dbo].[UserReceivedEmail] WHERE EmployeeId = @EmployeeId", 
                new DbParameter[] {
                    new SqlParameter("@EmployeeId", user.EmployeeId)
                });
            if (userReceived == null ) return false;

            return MSSQL2DT(userReceived["WellcomeEmail"]) != null;
        }

        public static bool WasOneYearEmail(this MSSqlConnection db, User user)
        {
            var userReceived = db.GetItem($"SELECT * FROM [HR].[dbo].[UserReceivedEmail] WHERE EmployeeId = @EmployeeId", 
                new DbParameter[] {
                    new SqlParameter("@EmployeeId", user.EmployeeId)
                });
            if (userReceived == null) return false;
            return MSSQL2DT(userReceived["OneYearEmail"]) != null;
        }

        //public static string ToMSSQLDate(this DateTime dt)
        //{
        //    return dt.ToString("yyyy-MM-dd HH:mm:ss");
        //}


        /// <summary>
        /// Ставим отметку в БД, что пользователь уже получил wellcome письмо
        /// </summary>
        /// <param name="db"></param>
        /// <param name="user"></param>
        public static void UserReceivedWellcomeEmail(this MSSqlConnection db, User user)
        {
            var data = db.GetItem($"SELECT * FROM [HR].[dbo].[UserReceivedEmail] WHERE EmployeeId = @EmployeeId", 
                new DbParameter[] {
                   new SqlParameter("@EmployeeId", user.EmployeeId)
                }
            );
            if (data == null)
            {
                //db.Query(@"INSERT INTO [HR].[dbo].[UserReceivedEmail] (EmployeeId, WellcomeEmail, OneYearEmail) " +
                //                    $@"VALUES ({user.EmployeeId}, '{DateTime.Now.ToMSSQLDate()}', null)");
                db.Query(@"INSERT INTO [HR].[dbo].[UserReceivedEmail] (EmployeeId, WellcomeEmail, OneYearEmail) " +
                    $@"VALUES (@EmployeeId, @dt, null)",
                    new DbParameter[] {
                        new SqlParameter("@EmployeeId", user.EmployeeId),
                        new SqlParameter("@dt", DateTime.Now)
                    });
            }
            else
            {
                db.Query($@"UPDATE [HR].[dbo].[UserReceivedEmail] SET WellcomeEmail = @dt WHERE EmployeeID=@EmployeeId", 
                    new DbParameter[] {
                        new SqlParameter("@EmployeeId", user.EmployeeId),
                        new SqlParameter("@dt", DateTime.Now)
                    });
            }
        }

        public static void ReportedWellcomeEmail(this MSSqlConnection db, int employeeId)
        {
            db.Query($@"UPDATE [HR].[dbo].[UserReceivedEmail] SET ReportWellcome = @dt WHERE EmployeeID=@EmployeeId", 
                new DbParameter[] {
                    new SqlParameter("@EmployeeId", employeeId),
                    new SqlParameter("@dt", DateTime.Now)
            });
        }

        public static void UserReceivedOneYearEmail(this MSSqlConnection db, User user)
        {
            var data = db.GetItem($"SELECT * FROM [HR].[dbo].[UserReceivedEmail] WHERE EmployeeId = @EmployeeId", 
                new DbParameter[] {
                    new SqlParameter("@EmployeeId", user.EmployeeId)
                });
            if (data == null)
            {
                db.Query(@"INSERT INTO [HR].[dbo].[UserReceivedEmail] (EmployeeId, WellcomeEmail, OneYearEmail) " +
                    $@"VALUES (@EmployeeId, null, @dt)", 
                    new DbParameter[] {
                        new SqlParameter("@EmployeeId", user.EmployeeId),
                        new SqlParameter("@dt", DateTime.Now)
                    }); //ставим заодно и wellcome ибо проработал год, всяко должен был получить
            }
            else
            {
                db.Query($@"UPDATE [HR].[dbo].[UserReceivedEmail] SET OneYearEmail = @dt WHERE EmployeeID=@EmployeeId", 
                    new DbParameter[] {
                        new SqlParameter("@EmployeeId", user.EmployeeId),
                        new SqlParameter("@dt", DateTime.Now)
                    });
            }
        }

        public static void ReportedOneYearEmail(this MSSqlConnection db, int employeeId)
        {
            db.Query($@"UPDATE [HR].[dbo].[UserReceivedEmail] SET ReportOneYear = @dt WHERE EmployeeID=@EmployeeId", 
                new DbParameter[] {
                    new SqlParameter("@EmployeeId", employeeId),
                    new SqlParameter("@dt", DateTime.Now)
                });
        }
    }
}
