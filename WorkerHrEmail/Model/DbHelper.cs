using SCCBA.DB;
using SCCBA.Extensions;
using System;
using System.Collections.Generic;
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
        /// <summary>
        /// Выбирает работников у которых одна запись в таблице Movement, у которых нет даты увольнения, 
        /// которые устроились не раньше недели назад
        /// </summary>
        //private static string sqlForWellcomeEmail = @"
        //      SELECT [main].[EmployeeID]
        //          ,[DateStart]
        //       ,counts.cnt
        //       ,[Core].[dbo].[User].[Mail]
        //       ,[Core].[dbo].[User].[FirstNameRU]
        //      FROM [Core].[dbo].[MovementHistory] main   
        //       inner join [Core].[dbo].[User] on ([Core].[dbo].[User].[EmployeeID] = [main].[EmployeeID]),
        //       (select EmployeeID, count(*) cnt from [Core].[dbo].[MovementHistory] group by EmployeeID ) as counts
        //      where DateFinish is null
        //      and counts.EmployeeID = main.EmployeeID
        //      and counts.cnt = 1
        //      and DateStart >= GetDate() - 7
        //      order by 2 desc";

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

        /// <summary>
        /// Select employees for wellcome letter
        /// </summary>
        /// <param name="db"></param>
        /// <returns></returns>
        public static IEnumerable<User> GetUsers(this MSSqlConnection db, ReasonsForSelect reason)
        {
            var rawItems = db.GetItems(reason == ReasonsForSelect.wellcome?  sqlForWellcomeEmail: sqlForOneYearEmail);

            var res = new List<User>();
            foreach(var row in rawItems)
            {
                var u = new User()
                {
                    EmployeeId = row["EmployeeID"].ToInt32(),
                    FirstNameRU = row["FirstNameRU"].ToString(),
                    Mail = row["Mail"].ToString(),
                    FirstDate = SqlDateTime.Parse(row["FirstDate"].ToString()).Value
                };

                //if (DateTime.TryParseExact(row["FirstDate"].ToString(), "yyyy-MM-dd HH:mm:ss", null, DateTimeStyles.None, out dt))
                //{
                //    u.FirstDate = dt.Value;
                //}
                res.Add(u);
            }

            return res;
        }

        /// <summary>
        /// will return true, if employee was received welcome email
        /// </summary>
        /// <param name="db"></param>
        /// <param name="user"></param>
        /// <returns></returns>
        public static bool WasWellcomeEmail(this MSSqlConnection db, User user)
        {
            var userReceived = db.GetItem($"SELECT * FROM [HR].[dbo].[UserReceivedEmail] WHERE EmployeeId = {user.EmployeeId} and WelcomeEmail is not null");
            return userReceived != null;
        }

        public static bool WasOneYearEmail(this MSSqlConnection db, User user)
        {
            var userReceived = db.GetItem($"SELECT * FROM [HR].[dbo].[UserReceivedEmail] WHERE EmployeeId = {user.EmployeeId} and OneYearEmail is not null");
            return userReceived != null;
        }

        public static string ToMSSQLDate(this DateTime dt)
        {
            //2021-11-11 16:02:45.680
            return dt.ToString("yyyy-MM-dd HH:mm:ss");
        }

        /// <summary>
        /// Ставим отметку в БД, что пользователь уже получил wellcome письмо
        /// </summary>
        /// <param name="db"></param>
        /// <param name="user"></param>
        public static void UserReceivedWellcomeEmail(this MSSqlConnection db, User user)
        {
            var data = db.GetItem($"SELECT * FROM [HR].[dbo].[UserReceivedEmail] WHERE EmployeeId = {user.EmployeeId}");
            if (data == null)
            {
                db.Query(@"INSERT INTO [HR].[dbo].[UserReceivedEmail] (EmployeeId, WellcomeEmail, OneYearEmail) " +
                    $@"VALUES ({user.EmployeeId}, '{DateTime.Now.ToMSSQLDate()}', null)");
            }
            else
            {
                db.Query($@"UPDATE [HR].[dbo].[UserReceivedEmail] SET WellcomeEmail = '{DateTime.Now.ToMSSQLDate()}' WHERE EmployeeID={user.EmployeeId}");
            }
        }

        public static void UserReceivedOneYearEmail(this MSSqlConnection db, User user)
        {
            var data = db.GetItem($"SELECT * FROM [HR].[dbo].[UserReceivedEmail] WHERE EmployeeId = {user.EmployeeId}");
            if (data == null)
            {
                db.Query(@"INSERT INTO [HR].[dbo].[UserReceivedEmail] (EmployeeId, WellcomeEmail, OneYearEmail) " +
                    $@"VALUES ({user.EmployeeId}, '{DateTime.Now.ToMSSQLDate()}', '{DateTime.Now.ToMSSQLDate()}')"); //ставим заодно и wellcome ибо проработал год, всяко должен был получить
            }
            else
            {
                db.Query($@"UPDATE [HR].[dbo].[UserReceivedEmail] SET OneYearEmail = '{DateTime.Now.ToMSSQLDate()}' WHERE EmployeeID={user.EmployeeId}");
            }
        }
    }
}
