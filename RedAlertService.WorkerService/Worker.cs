namespace RedAlertService.WorkerService
{
    public class Worker : BackgroundService
    {
        private readonly IConfiguration _configuration;
        private readonly Email.EmailController _emailController;    
        public Worker()
        {
            _configuration = new ConfigurationBuilder()
            .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
            .Build();
            Common.Logging.LoggingService.ConfigureLogger(_configuration.GetValue<string>("SeriLog:Path"));
            _emailController = new Email.EmailController();
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            try
            {
                Common.Logging.LoggingService.LogInformation("RedAlertService Started");
                await Task.Yield();

                var numberOfProcessors = _configuration.GetValue<int>("EmailConfiguration:NumberOfProcessors");
                var tasks = new List<Task>();

                do
                {
                    tasks.Clear();
                    for (int i = 1; i <= numberOfProcessors; i++)
                    {
                        tasks.Add(_emailController.ProcessEmailRequests(stoppingToken, i));
                    }

                    await Task.WhenAll(tasks);

                    await Task.Delay(1000, stoppingToken);
                }
                while (!stoppingToken.IsCancellationRequested);
            }
            catch (Exception ex)
            {
                Common.Logging.LoggingService.LogError(ex.Message, ex);
            }

        }
    }
}

