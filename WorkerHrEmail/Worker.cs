using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using SCCBA.DB;
using SCCBA.Extensions;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using WorkerHrEmail.Model;
using WorkerHrEmail.Services;

namespace WorkerHrEmail
{    

    public class Worker : IHostedService, IDisposable
    {
        private readonly ILogger<Worker> _logger;
        private readonly IConfiguration _config;
        private static object _lock = new object();
        private int _counter = 0;

        private Timer _timer;

        public Worker(ILogger<Worker> logger, IConfiguration configuration)
        {
            _logger = logger;
            _config = configuration;
        }

        public void Dispose()
        {
            _timer?.Dispose();
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            _timer = new Timer(DoWork, null, TimeSpan.Zero, TimeSpan.FromSeconds(3));
            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            _timer?.Change(Timeout.Infinite, 0);
            return Task.CompletedTask;
        }

        private void DoWork(object state)
        {
            var period = _config.GetSection("Period").Value.ToInt32();
            _timer.Change(period * 60 * 1000, period * 60 * 1000);

            if (Monitor.TryEnter(_lock))
            {
                try
                {
                    _logger.LogDebug($"Running DoWork iteration {_counter}");

                    Work_NewEmployees();
                    Work_OneYearEmployees();
                    Work_Report();

                    _logger.LogDebug($"DoWork {_counter} finished, will start iteration {_counter + 1}");
                }
                catch (Exception e)
                {
                    _logger.LogCritical(e.Message);
                }
                finally
                {
                    _counter++;
                    Monitor.Exit(_lock);
                }
            }
        }

        private void Work_NewEmployees()
        {
            _logger.LogInformation("wellcome emails start");
            string cs = _config.GetSection("ConnectionStrings:CbaConnectionString").Value;
            using (var conn = new MSSqlConnection(cs))
            {
                var users = conn.GetUsers(ReasonsForSelect.wellcome);//Получаем пользователей, которые подходят под получение wellcome письма

                foreach(var user in users)
                {
                    if( !conn.WasWellcomeEmail(user) ) //этому работнику еще не отсылали
                    {
                        _logger.LogInformation($"sending email for {user.EmployeeId} ({user.Mail})");
                        //формируем письмо
                        using (var message = new EmailMessage(
                            to: user.Mail,
                            subject: "Добро пожаловать в STADA!",
                            filename: "data\\wellcome.teml",
                            Tuple.Create("Name", user.FirstNameRU) //добавляем имя
                        ))
                        {
                            //отсылаем письмо
                            EmailService.SendMessage(message);
                            conn.UserReceivedWellcomeEmail(user); //записываем в базу данных, что пользователю письмо отправленно
                        }
                        _logger.LogInformation($"email for {user.EmployeeId} ({user.Mail}) was sent");
                    }
                }
            }
            _logger.LogInformation("wellcome emails comleted");
        }

        private void Work_OneYearEmployees()
        {
            _logger.LogInformation("one year email start");
            string cs = _config.GetSection("ConnectionStrings:CbaConnectionString").Value;
            using (var conn = new MSSqlConnection(cs))
            {
                var users = conn.GetUsers(ReasonsForSelect.oneYear);//Получаем пользователей, которые подходят под получение wellcome письма

                foreach (var user in users)
                {
                    if (!conn.WasOneYearEmail(user) //этому работнику еще не отсылали
                                                    //дополнительные проверки, что именно год назад
                        && user.FirstDate.Value.Year == DateTime.Now.Year - 1
                        && user.FirstDate.Value.Month == DateTime.Now.Month
                        && user.FirstDate.Value.Day == DateTime.Now.Day
                        ) 
                    {
                        _logger.LogInformation($"sending email for {user.EmployeeId} ({user.Mail})");
                        //формируем письмо
                        using (var message = new EmailMessage(
                            to: user.Mail,
                            subject: "Ура, ты уже год в STADA!",
                            filename: "data\\oneyear.teml"
                        ))
                        {
                            //отсылаем письмо
                            EmailService.SendMessage(message);
                            conn.UserReceivedOneYearEmail(user); //записываем в базу данных, что пользователю письмо отправленно
                        }
                        _logger.LogInformation($"email for {user.EmployeeId} ({user.Mail}) was sent");
                    }
                }
            }
            _logger.LogInformation("one year email comleted");
        }


        /// <summary>
        /// раз в месяц отсылаем письмо с отчетом
        /// </summary>
        private void Work_Report()
        {
            _logger.LogInformation("report start");

            string cs = _config.GetSection("ConnectionStrings:CbaConnectionString").Value;
            using (var conn = new MSSqlConnection(cs))
            {
                var history = conn.GetHistory();//Получаем историю отправки писем, которые еще не оформляли в отчет

                if (history.Max(x => x.Diff1) >= 30 || history.Max(x => x.Diff2) >= 30) //Накопилось больше-равно 30 дней. Оформляем отчет
                {
                    _logger.LogInformation("send report");
                    using (var message = new EmailReport(_config.GetSection("Email:ForReport").Value, history.ToArray()))
                    {
                        //отсылаем письмо
                        EmailService.SendMessage(message);
                        //отмечаем у всех пользователей, что мы отчитались по отправке писем
                        foreach (var user in history)
                        {
                            if( user.WellcomeEmail != null && user.ReportWellcome == null )
                                conn.ReportedWellcomeEmail(user.EmployeeId); //записываем в базу данных, что по пользователю отчитались

                            if (user.OneYearEmail != null && user.ReportOneYear == null)
                                conn.ReportedOneYearEmail(user.EmployeeId); //записываем в базу данных, что по пользователю отчитались
                        }
                    }
                }
            }

            _logger.LogInformation("report comleted");
        }
    }
}
