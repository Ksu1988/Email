using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using SCCBA.DB;
using SCCBA.Extensions;
using System;
using System.Linq;
using System.Net.Mail;
using System.Threading;
using System.Threading.Tasks;
using WorkerHrEmail.Model;
using WorkerHrEmail.Services;

namespace WorkerHrEmail
{

    public class Worker : IHostedService, IDisposable
    {
        private ILogger<Worker> _logger; // = Log.Logger.ForContext<Worker>();
        private readonly IConfiguration _config;
        private readonly EmailService _emailService;
        private static readonly object _lock = new();
        private int _counter = 0;

        private Timer _timer;

        private readonly string currentDirectory = AppDomain.CurrentDomain.BaseDirectory;

        public Worker(ILogger<Worker> logger, IConfiguration configuration, EmailService emailService)
        {
            _logger = logger;
            _config = configuration;
            _emailService = emailService;
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
                    _logger.LogInformation("Running DoWork iteration {Counter}", _counter);

                    Work_NewEmployees();
                    Work_OneYearEmployees();
                    Work_ComplienceEmployees();
                    Work_Report();

                    _logger.LogInformation("DoWork {CorrentCount} finished, will start iteration {NextCount}", _counter, (_counter + 1));
                }

                catch (Exception e)
                {
                    _logger.LogCritical("{Message}\r\n{StackTrace}", e.Message, e.StackTrace);
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
            try
            {
                _logger.LogInformation("{Method} wellcome emails start", nameof(Work_NewEmployees));
                string cs = _config.GetSection("ConnectionStrings:CbaConnectionString").Value; // я вот это раскоментировал так как ниже была ошибка из-за cs
                string emailFrom = _config.GetSection("Email:From").Value;
                using (var hr = new MSSqlConnection(cs))
                using (var conn = new MySqlConnection(MySqlServer.Main))
                {
                    var users = conn.GetUsers(ReasonsForSelect.Wellcome);//Получаем пользователей, которые подходят под получение wellcome письма

                    //users = conn.GetUsers(ReasonsForSelect.test);

                    foreach (var user in users)
                    {
                        if (!hr.WasWellcomeEmail(user)) //этому работнику еще не отсылали
                        {
                            _logger.LogInformation("try sending email for {EmployeeId} ({Mail})", user.EmployeeId, user.Mail);
                            //формируем письмо
                            using (var message = new EmailMessage(
                                to: user.Mail,
                                subject: "Добро пожаловать в STADA!",
                                filename: $"{currentDirectory}\\data\\wellcome.teml",
                                from: emailFrom,
                                Tuple.Create("Name", user.FirstNameRU) //добавляем имя
                            ))
                            {
                                //отсылаем письмо
                                _emailService.SendMessage(message);
                                hr.UserReceivedWellcomeEmail(user); //записываем в базу данных, что пользователю письмо отправленно
                            }
                            _logger.LogInformation("email for {EmployeeId} ({Mail}) was sent", user.EmployeeId, user.Mail);
                        }
                    }
                }
                _logger.LogInformation("wellcome emails comleted");
            }
            catch (Exception ex)
            {
                _logger.LogError("welcome emails not sent", ex.Message, ex);
            }
        }

        private void Work_OneYearEmployees()
        {
            try
            {
                _logger.LogInformation("one year email start");
                string cs = _config.GetSection("ConnectionStrings:CbaConnectionString").Value;
                string emailFrom = _config.GetSection("Email:From").Value;
                using (var hr = new MSSqlConnection(cs))
                using (var conn = new MySqlConnection(MySqlServer.Main))
                {
                    var users = conn.GetUsers(ReasonsForSelect.OneYear);//Получаем пользователей, которые подходят под получение wellcome письма

                    //users = conn.GetUsers(ReasonsForSelect.test);

                    foreach (var user in users)
                    {
                        if (!hr.WasOneYearEmail(user) //этому работнику еще не отсылали
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
                                subject: "Поздравляем с годом работы в STADA!",
                                filename: $"{currentDirectory}\\data\\oneyear.teml",
                                from: emailFrom
                            ))
                            {
                                //отсылаем письмо
                                _emailService.SendMessage(message);
                                hr.UserReceivedOneYearEmail(user); //записываем в базу данных, что пользователю письмо отправленно
                            }
                            _logger.LogInformation("email for {EmployeeId} ({Mail}) was sent", user.EmployeeId, user.Mail);
                        }
                    }
                }
                _logger.LogInformation("one year email completed");
            }            
            catch (Exception ex)
            {
                _logger.LogError("one year emails not sent", ex.Message, ex);
            }
}
        

        private void Work_ComplienceEmployees()
        {
            if (DateTime.Now.DayOfWeek != DayOfWeek.Friday)
            {
                return;
            }
            try
            {
                _logger.LogInformation("compliance and ethics email start");
                var cs = _config.GetSection("ConnectionStrings:CbaConnectionString").Value;
                string emailFrom = _config.GetSection("Email:From").Value;
                

                using (var hr = new MSSqlConnection(cs))
                using (var conn = new MySqlConnection(MySqlServer.Main))
                {
                    var users = conn.GetUsers(ReasonsForSelect.OneWeek).Where(e => !string.IsNullOrEmpty(e.Mail));//Получаем пользователей, которые подходят под получение письма Команда по комплаенс и этике раз в неделю
                    var message = new EmailMessage(
                                                    to: _config.GetSection("Email:ForReport").Value,
                                                    subject: "Для новых сотрудников: полезные материалы Группы по комплаенс и этике ",
                                                    filename: $"{currentDirectory}\\data\\oneWeek.html",
                                                    from: "compliance@stada.ru"
                                                );
                    foreach (var user in users)
                    {
                        if (!hr.WasOneWeekEmail(user))
                        {
                            _logger.LogInformation("try sending email for {EmployeeId} ({Mail})", user.EmployeeId, user.Mail);
                            message.To.Add(user.Mail);
                        }
                    }
                    //отсылаем письмо
                    if (users.Any())
                    {
                        message.CC.Add(new MailAddress("ekaterina.sarandaeva@stada.ru"));
                        message.CC.Add(new MailAddress("julia.zhuga@stada.ru"));
                        _emailService.SendMessage(message);
                    }
                    foreach (var user in users)
                    {
                        hr.UserReceivedOneWeekEmail(user);
                        _logger.LogInformation($"email for {user.EmployeeId} ({user.Mail}) was sent");//записываем в базу данных, что пользователю письмо отправленно
                    }
                }

                _logger.LogInformation("compliance and ethics email comleted");
            }
            catch (Exception e)
            {
                _logger.LogError("compliance and ethics email not send", e.Message);
            }
        }


        /// <summary>
        /// раз в месяц отсылаем письмо с отчетом
        /// </summary>
        private void Work_Report()
        {
            _logger.LogInformation("report start");

            string cs = _config.GetSection("ConnectionStrings:CbaConnectionString").Value;
            string emailFrom = _config.GetSection("Email:From").Value;

            using (var conn = new MSSqlConnection(cs))
            {
                var history = conn.GetHistory();//Получаем историю отправки писем, которые еще не оформляли в отчет
                if (history.Any())
                {
                    if (history.Max(x => x.Diff1) >= 30 || history.Max(x => x.Diff2) >= 30) //Накопилось больше-равно 30 дней. Оформляем отчет
                    {
                        _logger.LogInformation("send report");
                        using var message = new EmailReport(_config.GetSection("Email:ForReport").Value, history.ToArray());
                        //отсылаем письмо
                        _emailService.SendMessage(message);
                        //отмечаем у всех пользователей, что мы отчитались по отправке писем
                        foreach (var user in history)
                        {
                            if (user.WellcomeEmail != null && user.ReportWellcome == null)
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
